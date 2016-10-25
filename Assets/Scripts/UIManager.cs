using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBattles
{
    public class UIManager : MonoBehaviour
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
        private const string SSC_MANAGER_NOT_INITIALISED_EXC
            = "The ScreenSizeChange manager has not been instantiated "
            + "yet, but it needs to have been instantiated already";
        private const string ACCELERATE_BUTTON_NAME
            = "Acceleration";
        private const string FIRE_BUTTON_NAME
            = "Fire";
        private const float INSIGNIFICANT_ROLL_INPUT_CHANGE_THRESHOLD
            = 0.001f;
        private const float INSIGNIFICANT_PITCH_INPUT_CHANGE_THRESHOLD
            = 0.001f;

        // -- (variable) fields --
        public bool dont_destroy_on_load;

        public IncorporealPlayerController player_controller;

        public Canvas PlayerScreenCanvasPrefab;
        public Camera fixed_UI_camera_prefab;
        public Camera ship_select_camera_prefab;
        
        public List<GameObject> UiComponentObjectPrefabs;
        public GameObject menu_background_object_prefab;
        public bool PrintScreenSizeDebugText;

        //public Vector3 player_centred_UI_offset;
        
        private float input_roll = 0.0f;
        private float input_pitch = 0.0f;
        private bool UI_objects_instantiated = false;
        private bool in_game_menu_visible = false;
        private bool ship_select_menu_visible = false;
        private UiInputState ui_state = UiInputState.MainMenu;
        private UIElements UIElement_ALL;
        private GameObject menu_background_object = null;
        private GameObject debug_textbox = null;

        private Camera player_UI_camera = null; // for the which UI follows player avatar
        private Camera fixed_UI_camera = null; // UI doesn't move compared to camera
        private Camera ship_select_camera = null; // Camera inhabits a separate "scene"

        //private Canvas player_centred_canvas = null;
        private Canvas PlayerScreenCanvas = null;

        private ScreenSizeChangeManager SSCManager = null;
        private MainMenuUIManager MainMenuUIManager = null;
        private GameplayUIManager GameplayUiManager = null;
        private InGameMenuManager in_game_menu_manager = null;
        private SettingsMenuUIManager settings_menu_manager = null;

        private GameplayInputAdapterModule InputAdapter = null;
        private UIElements ActiveUIElements = UIElements.None;
        private Stack<UIElements> UITransitionHistory
            = new Stack<UIElements>();
        private UiComponentRegistryModule ComponentRegistry
            = new UiComponentRegistryModule();

        // -- delegates --
        public delegate void enterOrreryEventHandler();
        public delegate void exitNetworkGameInputEventHandler ();
        public delegate void exitProgramInputEventHandler ();
        public delegate void rollInputEventHandler (float roll_input);
        public delegate void pitchInputEventHandler (float pitch_input);

        // -- events --
        public event enterOrreryEventHandler EnterOrreryInputEvent;
        public event exitNetworkGameInputEventHandler ExitNetGameInputEvent;
        public event exitProgramInputEventHandler ExitProgramInputEvent;
        public event rollInputEventHandler RollInputEvent;
        public event pitchInputEventHandler PitchInputEvent;
        // Propagates events from child UI elements upwards to this object,
        // hopefully making hookup simpler (sorry if this is horrible! I'm new to this & experimenting)
        public event PlayGameButtonPressEventHandler PlayGameButtonPress
        {
            add { MainMenuUIManager.PlayGameButtonPress += value; }
            remove { MainMenuUIManager.PlayGameButtonPress -= value; }
        }

        // -- enums --
        public enum PlayerConnectState { IDLE, SEARCHING_FOR_SERVER, JOINING_SERVER, CREATING_SERVER };
        private enum UiInputState { InGame, MainMenu };

        // -- properties --

        // -- methods --
        public void Awake ()
        {
            if (!UI_objects_instantiated)
            {
                // UI objects
                Debug.Log("UI Manager instantiating UI objects");

                initialiseUIElementAll();
                SSCManager = GetComponent<ScreenSizeChangeManager>();
                InstantiateUIObjects();
                menu_background_object
                    = Instantiate(menu_background_object_prefab);

                MainMenuUIManager
                    = ComponentRegistry
                    .RetrieveGameObject(UIElements.MainMenu)
                    .GetComponent<MainMenuUIManager>();

                settings_menu_manager
                    = ComponentRegistry
                    .RetrieveGameObject(UIElements.SettingsMenu)
                    .GetComponent<SettingsMenuUIManager>();

                in_game_menu_manager
                    = ComponentRegistry
                    .RetrieveGameObject(UIElements.InGameMenu)
                    .GetComponent<InGameMenuManager>();
                
                GameplayUiManager
                    = ComponentRegistry
                    .RetrieveGameObject(UIElements.GameplayUI)
                    .GetComponent<GameplayUIManager>();

                GameplayUiManager.InitialiseSubComponents(SSCManager);
                InitialiseInputAdapter();
                GameplayUiManager.ActivateVirtualJoystick(
                    InputAdapter.VirtualJoystickEnabled
                );

                //player_centred_canvas_object = GameObject.Instantiate(player_centred_UI_prefab);
                //player_centred_canvas        = player_centred_canvas_object.GetComponent<Canvas>();

                // Initialise UI events structure
                in_game_menu_manager.ExitNetGameButtonPress += exitNetGameButtonPress;
                in_game_menu_manager.ExitInGameMenuEvent += toggleInGameMenu;
                in_game_menu_manager.EnterSettingsMenuEvent += enterSettingsMenu;
                MainMenuUIManager.EnterSettingsMenuEvent += enterSettingsMenu;
                MainMenuUIManager.EnterOrreryMenuEvent += enterOrrery;
                MainMenuUIManager.ExitProgramEvent += exitProgram;
                settings_menu_manager.ExitSettingsMenuEvent += exitSettingsMenu;
                settings_menu_manager.VirtualJoystickSetEvent += OnVirtualJoystickEnabled;

                // Initialise cameras
                initialiseGameUICameras();
                //hideShipSelectionUI();

                if (dont_destroy_on_load)
                {
                    //Test
                    UnityEngine.Object.DontDestroyOnLoad(PlayerScreenCanvas);
                    UnityEngine.Object.DontDestroyOnLoad(menu_background_object);
                    //Sets this to not be destroyed when reloading scene
                    UnityEngine.Object.DontDestroyOnLoad(gameObject);
                    UnityEngine.Object.DontDestroyOnLoad(fixed_UI_camera);
                    UnityEngine.Object.DontDestroyOnLoad(ship_select_camera);
                    Debug.Log("objects prevented from being destroyed on load");
                }

                if (PrintScreenSizeDebugText)
                {
                    debug_textbox
                        = ComponentRegistry
                        .RetrieveGameObject(UIElements.DebugOutput);
                    VariableTextboxPrinter printer
                        = debug_textbox.GetComponent<VariableTextboxPrinter>();
                    MyContract.RequireField(printer != null,
                                            "debug textbox object has a variabletextprinter component attached",
                                            "DebugTextboxPrefab");
                    SSCManager.ScreenResized.AddListener(printer.printVariable);
                }

                UI_objects_instantiated = true;
            }
        }

        public void Start ()
        {
            EnterMainMenuRoot();
        }

        public void Update ()
        {
            if (ui_state == UiInputState.InGame)
            {
                if (InputAdapter.ExitNetGameInput())
                {
                    ExitNetGameInputEvent();
                }

                if (InputAdapter.InGameMenuOpenInput())
                {
                    toggleInGameMenu();
                }

                if (InputAdapter.ShipSelectMenuOpenInput())
                {
                    Debug.Log("ship select button pressed");
                    //toggleShipSelectUI();
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
                if (player_controller != null)
                {
                    if (InputAdapter.AccelerateInput())
                    {
                        player_controller.accelerateShip(new Vector3(0, 0, 1));
                    }
                    else if (InputAdapter.BrakeInput())
                    {
                        player_controller.brakeShip();
                    }

                    if (InputAdapter.FireInput())
                    {
                        player_controller.firePrimaryWeapon();
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

        public void EnterMainMenuRoot ()
        {
            ui_state = UiInputState.MainMenu;
            fixed_UI_camera.gameObject.SetActive(true);
            MainMenuUIManager.SetPlayerConnectState(PlayerConnectState.IDLE);
            // disable all elements which aren't the main menu
            showUIElementFromFlags(false, 
                                   (UIElement_ALL ^ UIElements.MainMenu));
            showUIElement(true, UIElements.MainMenu);
            menu_background_object.SetActive(true);
        }

        public void enterSettingsMenu ()
        {
            if (ui_state == UiInputState.MainMenu)
            {
                showUIElement(false, UIElements.MainMenu);
            }
            else if (ui_state == UiInputState.InGame)
            {
                showUIElement(false, UIElements.InGameMenu);
            }
            settings_menu_manager.displayVirtualJoystickButtonState(
                InputAdapter.VirtualJoystickEnabled
            );
            showUIElement(true, UIElements.SettingsMenu);
        }

        public void exitSettingsMenu ()
        {
            showUIElement(false, UIElements.SettingsMenu);
            if (ui_state == UiInputState.MainMenu)
            {
                showUIElement(true, UIElements.MainMenu);
            }
            else if (ui_state == UiInputState.InGame)
            {
                showUIElement(true, UIElements.InGameMenu);
            }
        }

        public void enteringMultiplayerGame ()
        {
            // TODO: change to start in ship selection
            ui_state = UiInputState.InGame;
            menu_background_object.SetActive(false);
            showUIElementFromFlags(false, UIElements.MainMenu);
            showUIElement(true, UIElements.GameplayUI);
        }

        public void enterOrrery ()
        {
            EnterOrreryInputEvent();
        }

        public void exitProgram()
        {
            ExitProgramInputEvent();
        }

        public void playerShipCreated ()
        {
            //TODO: implement
            // hide ship selection UI
            // show HUD UI
            Debug.Log("UI Manager received playerShipCreated message");
        }

        public void toggleInGameMenu ()
        {
            bool toggle_on = in_game_menu_visible;
            showUIElement(!toggle_on, UIElements.InGameMenu);
            showUIElement(toggle_on, UIElements.GameplayUI);
            in_game_menu_visible = !toggle_on;
        }

        public void setPlayerController(IncorporealPlayerController player_controller)
        {
            if (player_UI_camera == null)
            {
                throw new InvalidOperationException(CAMERA_NOT_SET_EXCEPTION_MESSAGE);
            }
            this.player_controller = player_controller;
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
        public void initialiseGameUICameras()
        {
            fixed_UI_camera
                = Instantiate(fixed_UI_camera_prefab);
            ship_select_camera
                = Instantiate(ship_select_camera_prefab);
            if (SSCManager == null)
            {
                throw new InvalidOperationException(SSC_MANAGER_NOT_INITIALISED_EXC);
            }
            SSCManager.FixedUICamera = fixed_UI_camera;

            setInGameUICamerasActive(false);
        }

        /// <summary>
        /// Utility method to enable/disable in-game UI cameras
        /// </summary>
        /// <param name="active">cameras active/true or inactive/false</param>
        public void setInGameUICamerasActive(bool active)
        {
            fixed_UI_camera.gameObject.SetActive(active);
            ship_select_camera.gameObject.SetActive(active);
        }

        /// <summary>
        /// Tells the UI manager which camera follows the player's avatar
        /// This camera will also be used by the general graphics of the game
        /// to display things other than the UI (e.g. FX, game state, etc.)
        /// so I currently feel it belongs in the more general
        /// Program Instance Manager (PIM) class
        /// 
        /// Note that the camera is assumed to be set up and controlled
        /// by the PIM, so the UI Manager should only ever read values
        /// from the camera, and shouldn't write anything to it
        /// e.g. the UIManager shouldn't change the location, orientation, etc.
        /// of the camera.
        /// This should probably be enforced by passing through a read only view
        /// of the camera, but I'm not sure what that will break atm,
        /// as I'm still changing this code a lot.
        /// </summary>
        /// <param name="player_camera">
        /// The Camera which follows the player's in-game avatar
        /// </param>
        public void setPlayerCamera(Camera player_camera)
        {
            this.player_UI_camera = player_camera;
            if (player_camera == null)
            {
                Debug.Log("Warning - setting player camera to null");
            }
        }

        /// <summary>
        /// Allows us to use the UIElement "all" as necessary
        /// (we already have none included but all seems to cause problems
        /// [if it doesn't cause problems just add it back in])
        /// </summary>
        private void initialiseUIElementAll ()
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
            if (SSCManager == null)
            {
                throw new InvalidOperationException(SSC_MANAGER_NOT_INITIALISED_EXC);
            }
            ScreenSizeChangeTrigger SSCTrigger
                = PlayerScreenCanvas.GetComponent<ScreenSizeChangeTrigger>();
            if (SSCTrigger == null)
            {
                throw new InvalidOperationException(NO_SSCT_EXC);
            }
            SSCTrigger
                .ScreenResized
                .AddListener(SSCManager.OnScreenSizeChange);

            ComponentRegistry.InitialiseAndRegisterPrefabs(
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
            InputAdapter = new GameplayInputAdapterAndroid();
            //InputAdapter = new GameplayInputAdapterPc();
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
            foreach (UIElements e in elements)
            {
                //Debug.Log("Attempting to show " + e.ToString());
                GameObject obj;
                if (ComponentRegistry.TryGetValue(e, out obj))
                {
                    obj.SetActive(show);
                }
                else
                {
                    Debug.LogWarning("Attempting to "
                                   + (show ? "show" : "hide")
                                   + " an uninitialised UI element: "
                                   + e.ToString());
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

        // event handler for in_game_menu_UI button press
        private void exitNetGameButtonPress ()
        {
            ExitNetGameInputEvent();
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
    }
}
