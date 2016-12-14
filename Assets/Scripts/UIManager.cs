using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SpaceBattles
{
    public class UIManager : MonoBehaviour, IScoreListener
    {
        // -- constant fields --
        private const string CAMERA_NOT_SET_EXCEPTION_MESSAGE
            = "attempting to setPlayerShip without having set the camera first";
        private const string NO_SSCT_EXC
            = "The fixed UI canvas does not have a Screen Size Change Trigger "
            + "attached to it.";
        private const string NO_SSCM_EXC
            = "The UIManager does not have a ScreenBreakpointManager "
            + "module.MonoBehaviour attached to the same Unity GameObject "
            + " it is attached to.";
        private const string ACCELERATE_BUTTON_NAME
            = "Acceleration";
        private const string FIRE_BUTTON_NAME
            = "Fire";
        private const float INSIGNIFICANT_ROLL_INPUT_CHANGE_THRESHOLD
            = 0.001f;
        private const float INSIGNIFICANT_PITCH_INPUT_CHANGE_THRESHOLD
            = 0.001f;

        // -- (variable) fields --
        public bool DontDestroyOnLoad;

        public NetworkedPlayerController PlayerController;

        public Canvas PlayerScreenCanvasPrefab;
        public Camera FixedUiCameraPrefab;
        public Camera ship_select_camera_prefab;
        
        public List<GameObject> UiComponentObjectPrefabs;
        public List<Camera> CameraPrefabs;
        public bool PrintScreenSizeDebugText;
        public GameObject ScreenFadeImageHost;

        //public Vector3 player_centred_UI_offset;
        
        private float input_roll = 0.0f;
        private float input_pitch = 0.0f;
        private bool UiObjectsInstantiated = false;
        private bool InGameMenuVisible = false;
        private bool ship_select_menu_visible = false;
        private UiInputState ui_state = UiInputState.MainMenu;
        private UIElements UIElement_ALL;
        private GameObject DebugTextbox = null;

        //private Canvas player_centred_canvas = null;
        private Canvas PlayerScreenCanvas = null;

        private ScreenSizeChangeManager SSCManager = null;
        private MainMenuUIManager MainMenuUIManager = null;
        private GameplayUIManager GameplayUiManager = null;
        private InGameMenuManager InGameMenuManager = null;
        private SettingsMenuUIManager SettingsMenuManager = null;
        private OrreryUIManager OrreryUiManager = null;

        private GameplayInputAdapterModule InputAdapter = null;
        private UIElements ActiveUIElements = UIElements.None;
        private CameraRoles ActiveCameras = CameraRoles.None;
        private Stack<UIElements> UITransitionHistory
            = new Stack<UIElements>();
        private UIRegistry ComponentRegistry
            = new UIRegistry();
        private CameraRegistry CameraRegistry
            = new CameraRegistry();

        // -- delegates --
        public delegate void enterOrreryEventHandler();
        public delegate void exitNetworkGameInputEventHandler ();
        public delegate void exitProgramInputEventHandler ();
        public delegate void rollInputEventHandler (float roll_input);
        public delegate void pitchInputEventHandler (float pitch_input);

        // -- events --
        [HideInInspector]
        public UnityEvent EnterOrreryInputEvent;
        [HideInInspector]
        public UnityEvent ExitNetGameInputEvent;
        [HideInInspector]
        public UnityEvent ExitProgramInputEvent;
        [HideInInspector]
        public UnityEvent PlayGameButtonPress;
        public event rollInputEventHandler RollInputEvent;
        public event pitchInputEventHandler PitchInputEvent;

        // -- enums --
        public enum PlayerConnectState { IDLE, SEARCHING_FOR_SERVER, JOINING_SERVER, CREATING_SERVER };

        private enum UiInputState { InGame, MainMenu };

        // -- properties --
        public OrreryManager OrreryManager
        {
            set;
            private get;
        }

        // -- methods --

        // TODO: Remove once debugging complete
        // need to do it this way because only occurs in standalone
        public void DebugLogRegistryStatus()
        {
            Debug.Log(CameraRegistry.PrintDebugDestroyedRegisteredObjectCheck()
                + "\n" + ComponentRegistry.PrintDebugDestroyedRegisteredObjectCheck());
        }

        public void Awake ()
        {
            if (!UiObjectsInstantiated)
            {
                InitialiseUIElementAll();
                SSCManager = GetComponent<ScreenSizeChangeManager>();
                InstantiateUIObjects();
                ComponentRegistry.RegisterTransitions(this);
                InitialiseManagerFields();
                InitialiseGameplayUi();
                InitialiseSettingsMenu();
                InitialiseOrreryUi();

                EventSwitchboard Switchboard
                    = GetComponent<EventSwitchboard>();
                Switchboard.ConnectCords(ComponentRegistry);

                InitialiseUICameras();
                InitialiseScreenFader();

                if (DontDestroyOnLoad)
                {
                    UnityEngine.Object.DontDestroyOnLoad(PlayerScreenCanvas);
                    UnityEngine.Object.DontDestroyOnLoad(gameObject);
                }

                if (PrintScreenSizeDebugText)
                    { InitialiseDebugTextUiObject(); }

                UiObjectsInstantiated = true;
            }
        }

        public void Start ()
        {
            InertialCameraController MainMenuBackgroundCamera
                = CameraRegistry[(int)CameraRoles.MainMenuAndOrrery]
                .GetComponent<InertialCameraController>();
            MyContract.RequireFieldNotNull(MainMenuBackgroundCamera,
                                           "MainMenuBackgroundCamera");
            MainMenuBackgroundCamera.FollowTransform
                = MainMenuUIManager
                .BackgroundOrbitalBody
                .GetComponent<Transform>();

            showUIElementFromFlags(false, UIElement_ALL);
            EnterMainMenuRoot();
        }

        public void Update ()
        {
            if (ui_state == UiInputState.InGame)
            {
                if (InputAdapter.ExitNetGameInput())
                {
                    ExitNetGameInputEvent.Invoke();
                }
                else if (InputAdapter.InGameMenuOpenInput())
                {
                    ToggleInGameMenu();
                }
                else if (InputAdapter.ShipSelectMenuOpenInput())
                {
                    Debug.Log("ship select button pressed");
                    //toggleShipSelectUI();
                }
                else if (InputAdapter.InGameScoreboardOpenInput())
                {
                    Debug.Log("Scoreboard opened!");
                    TransitionToUIElements(
                        UiElementTransitionType.Additive,
                        UIElements.Scoreboard
                    );
                }
                else if (InputAdapter.InGameScoreboardCloseInput())
                {
                    Debug.Log("Scoreboard closed!");
                    TransitionToUIElements(
                        UiElementTransitionType.Subtractive,
                        UIElements.Scoreboard
                    );
                }
            }
        }

        public void FixedUpdate()
        {
            if (ui_state == UiInputState.InGame)
            {
                float new_roll = InputAdapter.ReadRollInputValue();
                float new_pitch = InputAdapter.ReadPitchInputValue();
                
                if (Math.Abs(new_roll - input_roll)
                >   INSIGNIFICANT_ROLL_INPUT_CHANGE_THRESHOLD)
                {
                    input_roll = new_roll;
                    RollInputEvent(new_roll);
                }

                if (Math.Abs(new_pitch - input_pitch)
                > INSIGNIFICANT_PITCH_INPUT_CHANGE_THRESHOLD)
                {
                    input_pitch = new_pitch;
                    PitchInputEvent(new_pitch);
                }

                // this can happen during transition
                // from game to menu (for a frame or two)
                // TODO: Update this code so that it triggers events
                //       such that the PIM can decide what to do
                //       with the input,
                //       rather than having program logic decisions
                //       here in the input manager.
                if (PlayerController != null)
                {
                    if (InputAdapter.AccelerateInput())
                    {
                        PlayerController.accelerateShip(new Vector3(0, 0, 1));
                    }
                    else if (InputAdapter.BrakeInput())
                    {
                        PlayerController.brakeShip();
                    }

                    if (InputAdapter.FireInput())
                    {
                        PlayerController.firePrimaryWeapon();
                    }
                }
            }
        }

        public void setCurrentPlayerHealth(double new_value)
        {
            Debug.Log("UI Manager updating local player health");
            GameplayUiManager.LocalPlayerSetCurrentHealth(new_value);
        }

        public void setCurrentPlayerMaxHealth(double new_value)
        {
            GameplayUiManager.LocalPlayerSetMaxHealth(new_value);
        }

        public void SetPlayerConnectState(PlayerConnectState newState)
        {
            MainMenuUIManager.SetPlayerConnectState(newState);
        }

        public void CameraTransition (CameraRoles newActiveCameras)
        {
            // Deactivate cameras
            CameraRegistry.ActivateGameObjectsFromIntFlag(
                false,
                (int)ActiveCameras
            );
            // Activate cameras
            CameraRegistry.ActivateGameObjectsFromIntFlag(
                true,
                (int)newActiveCameras
            );
            ActiveCameras = newActiveCameras;
        }

        public void
        TransitionToUIElements
            (UiElementTransition transition)
        {
            TransitionToUIElements(
                transition.Type,
                transition.Targets
            );
        }

        public void
        TransitionToUIElements
            (UiElementTransitionType transitionType,
            UIElements newUIElements)
        {
            if (transitionType == UiElementTransitionType.Tracked)
            {
                // Add current active elements as a new history element
                UITransitionHistory.Push(ActiveUIElements);
            }

            if (transitionType == UiElementTransitionType.Fresh
            || transitionType == UiElementTransitionType.Tracked)
            {
                // Deactivate current UIElements
                //Debug.Log("TransitionToUIElements hiding elements " + ActiveUIElements);
                showUIElementFromFlags(false, ActiveUIElements);
                ActiveUIElements = UIElements.None;
            }

            if (transitionType == UiElementTransitionType.Subtractive)
            {
                // Deactivate newUIElements
                showUIElementFromFlags(false, newUIElements);
                // Remove deactivated elements from ActiveUIElements
                UIElements DeactivatedComponents
                    = newUIElements & ActiveUIElements;
                ActiveUIElements ^= DeactivatedComponents;
            }
            else
            {
                // Activate new UIElements
                //Debug.Log("TransitionToUIElements showing elements " + newUIElements);
                showUIElementFromFlags(true, newUIElements);
                ActiveUIElements |= newUIElements;
            }

            if (transitionType == UiElementTransitionType.Fresh)
            {
                // Clear history
                UITransitionHistory.Clear();
            }
        }

        public void TransitionUIElementsBacktrack ()
        {
            MyContract.RequireField(UITransitionHistory.Count > 0,
                                    "has at least one entry",
                                    "UITransitionHistory");
            UIElements PreviousState = UITransitionHistory.Pop();
            showUIElementFromFlags(false, ActiveUIElements);
            showUIElementFromFlags(true, PreviousState);
            ActiveUIElements = PreviousState;
        }

        public void EnterMainMenuRoot ()
        {
            ui_state = UiInputState.MainMenu;
            // TODO: pull this debug text into the main menu manager
            if (PrintScreenSizeDebugText)
            {
                DebugTextbox.SetActive(true);
            }
            TransitionToUIElements(
                UiElementTransitionType.Fresh,
                UIElements.MainMenu
            );
            CameraTransition(CameraRoles.MainMenuAndOrrery
                           | CameraRoles.FixedUi);
        }

        public void EnterSettingsMenu ()
        {
            // Deprecated - cameras now persist independently
            //bool ShouldUseMainMenuBackgroundCamera
            //    = (ActiveUIElements & UIElements.MainMenu) > 0;
            TransitionToUIElements(
                UiElementTransitionType.Tracked,
                UIElements.SettingsMenu
            );

            // TODO: Investigate putting this into
            //       settings manager's start function
            SettingsMenuManager.DisplayVirtualJoystickButtonState(
                InputAdapter.VirtualJoystickEnabled
            );
        }

        public void ExitSettingsMenu ()
        {
            TransitionUIElementsBacktrack();
        }

        public void EnteringMultiplayerGame ()
        {
            // TODO: change to start in ship selection
            ui_state = UiInputState.InGame;
            //TransitionToUIElements(
            //    UiElementTransitionType.Fresh,
            //    UIElements.GameplayUI
            //);
            //CameraTransition(CameraRoles.FixedUi);
        }

        public void playerShipCreated ()
        {
            //TODO: implement
            // hide ship selection UI
            // show HUD UI
            Debug.Log("UI Manager received playerShipCreated message");
        }

        public void ToggleInGameMenu ()
        {
            // TODO: Pull this toggle bool into the InputAdapter
            //       and pass back an enter/exit in-game menu event
            InGameMenuVisible = !InGameMenuVisible;
            if (InGameMenuVisible)
            {
                TransitionToUIElements(
                    UiElementTransitionType.Additive,
                    UIElements.InGameMenu
                );
                TransitionToUIElements(
                    UiElementTransitionType.Subtractive,
                    UIElements.GameplayUI
                );
            }
            else
            {
                TransitionToUIElements(
                    UiElementTransitionType.Subtractive,
                    UIElements.InGameMenu
                );
                TransitionToUIElements(
                    UiElementTransitionType.Additive,
                    UIElements.GameplayUI
                );
            }
        }

        public void SetPlayerController(NetworkedPlayerController playerController)
        {
            if (!CameraRegistry.Contains((int)CameraRoles.FixedUi))
            {
                throw new InvalidOperationException(CAMERA_NOT_SET_EXCEPTION_MESSAGE);
            }

            ScoreboardUiManager ScoreUIManager
                = ComponentRegistry
                .RetrieveManager
                    <ScoreboardUiManager>
                    (UIElements.Scoreboard);
            if (ScoreUIManager != null)
            {
                ScoreUIManager.LocalPlayerId = playerController.netId;
            }

            this.PlayerController = playerController;
            //player_centred_canvas.worldCamera = player_UI_camera;
            //player_centred_canvas_object.transform.SetParent(player_object.transform);
            //player_centred_canvas_object.transform.localPosition = player_centred_UI_offset;
        }

        public void initShipCentredUI()
        {
            throw new NotImplementedException("initShipCentred UI called - this function is not implemented yet");
            //player_object.AddComponent<Canvas>();
            // TODO: Unfinished
        }

        /// <summary>
        /// This is expected to be called at game start,
        /// as the cameras could be used from the main menu
        /// </summary>
        public void InitialiseUICameras()
        {
            MyContract.RequireFieldNotNull(SSCManager, "Screen Size Change Manager");
            CameraRegistry.PersistThroughScenes = true;
            CameraRegistry.KeyEnum = typeof(CameraRoles);
            CameraRegistry.InitialiseAndRegisterGenericPrefabs(CameraPrefabs);
            SSCManager.FixedUICamera
                = CameraRegistry[(int)CameraRoles.FixedUi];
            Debug.Log("Fixed UI Camera for the SSCManager has been set");
        }
        public void ProvideCamera (Camera camera)
        {
            List<Camera> NewCameraList
                = new List<Camera>();
            NewCameraList.Add(camera);
            CameraRegistry.RegisterObjects(NewCameraList);
            //Debug.Log("Camera provided to UIManager with key "
            //        + cameraHostObject
            //         .GetComponent<IGameObjectRegistryKeyComponent>()
            //         .Key);
        }

        public void SetOrreryDateTimeTrigger (DateTime newTime)
        {
            OrreryManager.SetExplicitDateTime(newTime);
        }
        
        public void FadeCamera (bool fadeOut, Action fadeCallback)
        {
            if (fadeOut)
            {
                CameraRegistry.FadeAllToBlack(fadeCallback);
            } 
            else
            {
                CameraRegistry.FadeAllToClear(fadeCallback);
            }
        }

        public void
        RegisterTransitionHandlers
        (ITransitionRequestBroadcaster broadcaster)
        {
            broadcaster.UiBacktrackRequest += TransitionUIElementsBacktrack;
            broadcaster.UiTransitionRequest += TransitionToUIElements;
        }

        public void
        OnScoreUpdate
            (PlayerIdentifier playerId,
            int newScore)
        {
            MyContract.RequireField(
                ComponentRegistry.Contains(UIElements.Scoreboard),
                "contains a scoreboard",
                "ComponentRegistry"
            );
            
            ComponentRegistry
                .RetrieveManager<ScoreboardUiManager>(UIElements.Scoreboard)
                .ChangePlayerScore(playerId, newScore);
            // Debug
            Debug.Log("Score was updated for player "
                     + playerId.PlayerID
                     + " to new score "
                     + newScore);
        }

        /// <summary>
        /// Allows us to use the UIElement "all" as necessary
        /// (we already have none included but all seems to cause problems
        /// [if it doesn't cause problems just add it back in])
        /// </summary>
        private void InitialiseUIElementAll ()
        {
            Array allUIElementValues = Enum.GetValues(typeof(UIElements));
            UIElement_ALL = UIElements.None;
            foreach (UIElements e in allUIElementValues)
            {
                UIElement_ALL |= e;
            }
        }

        /// <summary>
        /// Prerequisites: ssc_manager is instantiated
        /// </summary>
        private void InstantiateUIObjects ()
        {
            PlayerScreenCanvas
                    = GameObject.Instantiate(PlayerScreenCanvasPrefab);
            MyContract.RequireFieldNotNull(SSCManager, "SSCManager");

            ScreenSizeChangeTrigger SSCTrigger
                = PlayerScreenCanvas.GetComponent<ScreenSizeChangeTrigger>();
            if (SSCTrigger == null)
            {
                throw new InvalidOperationException(NO_SSCT_EXC);
            }
            SSCTrigger
                .ScreenResized
                .AddListener(SSCManager.OnScreenSizeChange);

            ComponentRegistry.KeyEnum = typeof(UIElements);
            ComponentRegistry.InitialiseAndRegisterUiPrefabs(
                UiComponentObjectPrefabs, SSCManager, PlayerScreenCanvas
            );
        }
        

        private void InitialiseInputAdapter ()
        {
#if UNITY_ANDROID
            InputAdapter = new GameplayInputAdapterAndroid();
#endif
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            // testing
            //InputAdapter = new GameplayInputAdapterAndroid();
            InputAdapter = new GameplayInputAdapterPc();
#endif

            InputAdapter.AccelerateButtonName = ACCELERATE_BUTTON_NAME;
            if (!CnControls
                .CnInputManager
                .ButtonExists(ACCELERATE_BUTTON_NAME))
            {
                throw new ArgumentException(
                    button_name_errmsg(UIElements.AccelerateButton,
                                       ACCELERATE_BUTTON_NAME)
                );
            }

            InputAdapter.FireButtonName = FIRE_BUTTON_NAME;
            if (!CnControls
                .CnInputManager
                .ButtonExists(FIRE_BUTTON_NAME))
            {
                throw new ArgumentException(
                    button_name_errmsg(UIElements.FireButton,
                                       FIRE_BUTTON_NAME)
                );
            }
        }

        /// <summary>
        /// Sets each element n elements to active if show is true
        /// and vice versa.
        /// </summary>
        /// <param name="show">
        /// Whether or not the listed elements should be visible
        /// (in this case visible also means active in the Unity sense,
        /// so invisible UI elements will also not get Update calls, etcetera).
        /// </param>
        /// <param name="elements">
        /// Variable-length number of UIElements to show or hide.
        /// </param>
        private void showUIElement (bool show, params UIElements[] elements)
        {
            foreach (UIElements Element in elements)
            {
                //Debug.Log("Attempting to show " + e.ToString());
                GameObject obj;
                if (ComponentRegistry.TryGetValue((int)Element, out obj))
                {
                    obj.SetActive(show);
                    //Debug.Log(
                    //      (show ? "Showing" : "Hiding")
                    //    + " element "
                    //    + Element);
                }
                else
                {
                    Debug.LogWarning("Attempting to "
                                   + (show ? "show" : "hide")
                                   + " an uninitialised UI element: "
                                   + Element.ToString());
                }
            }
        }

        private void showUIElementFromFlags(bool show, UIElements elements)
        {
            Array allUIElementValues = Enum.GetValues(typeof(UIElements));
            List<UIElements> args = new List<UIElements>();
            foreach (UIElements e in allUIElementValues)
            {
                if ((e & elements) > 0)
                {
                    args.Add(e);
                }
            }
            showUIElement(show, args.ToArray());
        }

        private string button_name_errmsg (UIElements button_element, 
                                           String intended_name)
        {
            return "The "
                 + button_element.ToString()
                 + " has not been set up correctly in the editor.\n"
                 + "Please set its output name to its proper value: \""
                 + intended_name
                 + "\"";
        }

        private void OnVirtualJoystickEnabled(bool enabled)
        {
            InputAdapter.VirtualJoystickEnabled = enabled;
            GameplayUiManager.ActivateVirtualJoystick(enabled);
        }

        private void InitialiseDebugTextUiObject()
        {
            DebugTextbox = ComponentRegistry[(int)UIElements.DebugOutput];
            DebugTextbox.SetActive(true);
            VariableTextboxPrinter Printer
                = DebugTextbox.GetComponent<VariableTextboxPrinter>();
            MyContract.RequireField(Printer != null,
                                    "debug textbox object has a variabletextprinter component attached",
                                    "DebugTextboxPrefab");
            SSCManager.ScreenResized.AddListener(Printer.PrintVariable);
        }

        private void InitialiseScreenFader()
        {
            CameraRegistry.CoroutineHost = this;
            ScreenFadeImageHost
                .transform
                .SetParent(PlayerScreenCanvas.transform, false);
            CameraFader Fader =
                CameraRegistry[(int)CameraRoles.FixedUi]
                .GetComponent<CameraFader>();
            MyContract.RequireFieldNotNull(
                Fader, "Fixed UI CameraFader component"
            );
            Fader.FadeImg = ScreenFadeImageHost.GetComponent<Image>();
            MyContract.RequireFieldNotNull(
                Fader.FadeImg, "ScreenFadeImageHost Image component"
            );
            PlayerScreenCanvas
                .GetComponent<ScreenSizeChangeTrigger>()
                .ScreenResizedInternal
                .AddListener(Fader.OnScreenSizeChange);
            Fader.OnScreenSizeChange(PlayerScreenCanvas.GetComponent<RectTransform>().rect);
        }

        private void InitialiseOrreryUi()
        {
            OrreryUiManager.DateTimeSet += SetOrreryDateTimeTrigger;
        }

        private void InitialiseSettingsMenu()
        {
            SettingsMenuManager.VirtualJoystickSetEvent += OnVirtualJoystickEnabled;
        }

        private void InitialiseGameplayUi()
        {
            GameplayUiManager.InitialiseSubComponents(SSCManager);
            InitialiseInputAdapter();
            GameplayUiManager.ActivateVirtualJoystick(
                InputAdapter.VirtualJoystickEnabled
            );
        }

        private void InitialiseManagerFields()
        {
            MainMenuUIManager
                = ComponentRegistry
                .RetrieveManager<MainMenuUIManager>
                    (UIElements.MainMenu);

            SettingsMenuManager
                = ComponentRegistry
                .RetrieveManager<SettingsMenuUIManager>
                    (UIElements.SettingsMenu);

            InGameMenuManager
                = ComponentRegistry
                .RetrieveManager<InGameMenuManager>
                    (UIElements.InGameMenu);

            GameplayUiManager
                = ComponentRegistry
                .RetrieveManager<GameplayUIManager>
                    (UIElements.GameplayUI);

            OrreryUiManager
                = ComponentRegistry
                .RetrieveManager<OrreryUIManager>
                    (UIElements.OrreryUI);
        }
    }
}
