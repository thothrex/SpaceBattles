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
        private const string PREFAB_NO_RECTTRANSFORM_ERRMSG
            = "Prefab does not have a RectTransform attached";
        private const float INSIGNIFICANT_ROLL_INPUT_CHANGE_THRESHOLD
            = 0.001f;
        private const float INSIGNIFICANT_PITCH_INPUT_CHANGE_THRESHOLD
            = 0.001f;

        // -- (variable) fields --
        public bool dont_destroy_on_load;

        public IncorporealPlayerController player_controller;

        public Canvas player_screen_canvas_prefab;
        public Camera fixed_UI_camera_prefab;
        public Camera ship_select_camera_prefab;
        
        public List<GameObject> UI_component_object_prefabs;

        //public Vector3 player_centred_UI_offset;
        
        private Dictionary<UIElement,GameObject> UI_component_objects = null;
        
        private float input_roll = 0.0f;
        private float input_pitch = 0.0f;
        private bool UI_objects_instantiated = false;
        private bool in_game_menu_visible = false;
        private bool ship_select_menu_visible = false;
        private UIState ui_state = UIState.MAIN_MENU;

        private Camera player_UI_camera = null; // for the which UI follows player avatar
        private Camera fixed_UI_camera = null; // UI doesn't move compared to camera
        private Camera ship_select_camera = null; // Camera inhabits a separate "scene"

        //private Canvas player_centred_canvas = null;
        private Canvas player_screen_canvas = null;

        private MainMenuUIManager main_menu_UI_manager = null;
        private GameplayUIManager gameplay_UI_manager = null;
        private InGameMenuManager in_game_menu_manager = null;

        private InputAdapterModule input_adapter = null;

        // -- delegates --
        public delegate void exitNetworkGameInputEventHandler ();
        public delegate void rollInputEventHandler (float roll_input);
        public delegate void pitchInputEventHandler (float pitch_input);

        // -- events --
        public event exitNetworkGameInputEventHandler ExitNetGameInputEvent;
        public event rollInputEventHandler RollInputEvent;
        public event pitchInputEventHandler PitchInputEvent;
        // Propagates events from child UI elements upwards to this object,
        // hopefully making hookup simpler (sorry if this is horrible! I'm new to this & experimenting)
        public event PlayGameButtonPressEventHandler PlayGameButtonPress
        {
            add { main_menu_UI_manager.PlayGameButtonPress += value; }
            remove { main_menu_UI_manager.PlayGameButtonPress -= value; }
        }

        // -- enums --
        public enum PlayerConnectState { IDLE, SEARCHING_FOR_SERVER, JOINING_SERVER, CREATING_SERVER };
        private enum UIState { IN_GAME, MAIN_MENU };

        // -- properties --

        // -- methods --
        public void Awake ()
        {
            if (!UI_objects_instantiated)
            {
                // instantiate pure code objects
#if UNITY_ANDROID
                input_adapter = new AndroidInputManager();
#endif
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                input_adapter = new PCInputManager();
#endif
                // UI objects
                Debug.Log("UI Manager instantiating UI objects");

                instantiateUIObjects();
                main_menu_UI_manager
                    = UI_component_objects[UIElement.MAIN_MENU]
                    .GetComponent<MainMenuUIManager>();
                
                in_game_menu_manager
                    = UI_component_objects[UIElement.IN_GAME_MENU]
                    .GetComponent<InGameMenuManager>();
                
                gameplay_UI_manager
                    = UI_component_objects[UIElement.GAMEPLAY_UI]
                    .GetComponent<GameplayUIManager>();

                // We always instantiate because this should be possible to
                // enable /disable dynamically
                Debug.Log("Creating joystick");
                input_adapter.virtual_joystick_element
                    = UI_component_objects[UIElement.VIRTUAL_JOYSTICK];
                
                //player_centred_canvas_object = GameObject.Instantiate(player_centred_UI_prefab);
                //player_centred_canvas        = player_centred_canvas_object.GetComponent<Canvas>();

                // Initialise UI events structure
                in_game_menu_manager.ExitNetGameButtonPress += exitNetGameButtonPress;

                // Initialise cameras
                initialiseGameUICameras();
                hideShipSelectionUI();

                if (dont_destroy_on_load)
                {
                    //Test
                    UnityEngine.Object.DontDestroyOnLoad(player_screen_canvas);
                    //Sets this to not be destroyed when reloading scene
                    UnityEngine.Object.DontDestroyOnLoad(gameObject);
                    UnityEngine.Object.DontDestroyOnLoad(fixed_UI_camera);
                    UnityEngine.Object.DontDestroyOnLoad(ship_select_camera);
                    Debug.Log("objects prevented from being destroyed on load");
                }
                UI_objects_instantiated = true;
            }
        }

        public void Start ()
        {
            enteringMainMenuRoot();
        }

        public void Update ()
        {
            if (ui_state == UIState.IN_GAME)
            {
                if (input_adapter.exitNetGameInput())
                {
                    ExitNetGameInputEvent();
                }

                if (input_adapter.inGameMenuOpenInput())
                {
                    toggleInGameMenu();
                }

                if (input_adapter.shipSelectMenuOpenInput())
                {
                    Debug.Log("ship select button pressed");
                    toggleShipSelectUI();
                }
            }
        }

        public void FixedUpdate()
        {
            if (ui_state == UIState.IN_GAME)
            {
                float new_roll = input_adapter.getRollInputValue();
                float new_pitch = input_adapter.getPitchInputValue();
                
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
                    if (input_adapter.accelerateInput())
                    {
                        player_controller.accelerateShip(new Vector3(0, 0, 1));
                    }
                    else if (input_adapter.brakeInput())
                    {
                        player_controller.brakeShip();
                    }

                    if (input_adapter.fireInput())
                    {
                        player_controller.firePrimaryWeapon();
                    }
                }
            }
        }

        public void setCurrentPlayerHealth(double new_value)
        {
            Debug.Log("UI Manager updating local player health");
            gameplay_UI_manager.localPlayerSetCurrentHealth(new_value);
        }

        public void setCurrentPlayerMaxHealth(double new_value)
        {
            gameplay_UI_manager.localPlayerSetMaxHealth(new_value);
        }

        public void setPlayerConnectState(PlayerConnectState new_state)
        {
            main_menu_UI_manager.setPlayerConnectState(new_state);
        }

        public void enteringMainMenuRoot ()
        {
            ui_state = UIState.MAIN_MENU;
            main_menu_UI_manager.setPlayerConnectState(PlayerConnectState.IDLE);
            hideInGameUI();
            showMainMenu(true);
        }

        public void enterSettingsMenu ()
        {
            if (ui_state == UIState.MAIN_MENU)
            {
                showMainMenu(false);
            }
            else if (ui_state == UIState.IN_GAME)
            {
                hideInGameMenu();
            }
            showSettingsMenu(true);
        }

        public void exitSettingsMenu ()
        {
            showSettingsMenu(false);
            if (ui_state == UIState.MAIN_MENU)
            {
                showMainMenu(true);
            }
            else if (ui_state == UIState.IN_GAME)
            {
                showInGameMenu();
            }
        }

        public void enteringMultiplayerGame ()
        {
            // TODO: change to start in ship selection
            ui_state = UIState.IN_GAME;
            hideInGameMenu();
            showMainMenu(false);
            showGameplayUI(true);
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
            if (in_game_menu_visible)
            {
                hideInGameMenu();
                showGameplayUI(true);
            }
            else
            {
                showInGameMenu();
                showGameplayUI(false);
            }
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
        
        public void toggleShipSelectUI()
        {
            if (ship_select_menu_visible)
            {
                hideInGameMenu();
                showGameplayUI(false);
                showShipSelectionUI();
            }
            else
            {
                showGameplayUI(true);
                hideShipSelectionUI();
            }
        }

        private void instantiateUIObjects()
        {
            player_screen_canvas
                    = GameObject.Instantiate(player_screen_canvas_prefab);
            int expected_num_UI_elements
                = Enum.GetValues(typeof(UIElement)).Length;
            Debug.Log("Expected number of UI elements: "
                     + expected_num_UI_elements);
            UI_component_objects
                = new Dictionary<UIElement, GameObject>(expected_num_UI_elements);
            foreach (GameObject prefab in UI_component_object_prefabs)
            {
                GameObject instance
                    = setupUIComponentFromPrefab(prefab, player_screen_canvas.transform);
                UIElement element = instance.GetComponent<UIComponent>()
                                  .ElementIdentifier;
                if (UI_component_objects.ContainsKey(element)
                &&  UI_component_objects[element] != null)
                {
                    throw new InvalidOperationException(
                        "Trying to instantiate a second " + element.ToString()
                    );
                }
                else
                {
                    UI_component_objects.Add(element, instance);
                }
            }
        }

        /// <summary>
        /// Instantiates, but also sets parent canvas
        /// and recentres UI component relative to that parent canvas
        /// </summary>
        /// <param name="prefab">
        /// Prefab to instantiate the UI component from
        /// </param>
        /// <param name="parent_transform">
        /// Transform which will be the parent of the instantiated GameObject
        /// </param>
        /// <returns></returns>
        private GameObject setupUIComponentFromPrefab(GameObject prefab, Transform parent_transform)
        {
            GameObject new_obj = GameObject.Instantiate(prefab);
            RectTransform new_UI_transform = new_obj.GetComponent<RectTransform>();
            if (new_UI_transform == null)
            {
                throw new ArgumentException(PREFAB_NO_RECTTRANSFORM_ERRMSG,
                                            "prefab");
            }
            new_UI_transform.SetParent(parent_transform);
            new_UI_transform.localScale = new Vector3(1, 1, 1);
            // translate the editor-set position into the local reference frame
            new_UI_transform.anchoredPosition = new_obj.transform.position;
            return new_obj;
        }

        private void showInGameMenu ()
        {
            UI_component_objects[UIElement.IN_GAME_MENU].SetActive(true);
            in_game_menu_visible = true;
        }

        private void hideInGameMenu ()
        {
            UI_component_objects[UIElement.IN_GAME_MENU].SetActive(false);
            in_game_menu_visible = false;
        }

        /// <summary>
        /// Hides all game-related UI.
        /// Used for swapping to menus, etc.
        /// </summary>
        private void hideInGameUI ()
        {
            showGameplayUI(false);
            hideInGameMenu();
        }

        /// <summary>
        /// Specifically gameplay UI such as health bar,
        /// targeting reticule, etcetera.
        /// </summary>
        private void showGameplayUI (bool show)
        {
            //player_centred_canvas_object.gameObject.SetActive(true);
            UI_component_objects[UIElement.GAMEPLAY_UI]
                .SetActive(show);
            UI_component_objects[UIElement.VIRTUAL_JOYSTICK]
                .SetActive(show);
            setInGameUICamerasActive(show);
        }

        private void showMainMenu (bool show)
        {
            UI_component_objects[UIElement.MAIN_MENU].SetActive(show);
        }

        private void showShipSelectionUI ()
        {
            UI_component_objects[UIElement.SHIP_SELECT].SetActive(true);
            ship_select_menu_visible = true;
            ship_select_camera.gameObject.SetActive(true);
        }

        private void hideShipSelectionUI()
        {
            UI_component_objects[UIElement.SHIP_SELECT].SetActive(false);
            ship_select_menu_visible = false;
            ship_select_camera.gameObject.SetActive(false);
        }

        private void showSettingsMenu (bool show)
        {
            UI_component_objects[UIElement.SETTINGS_MENU]
                .SetActive(show);
        }

        // event handler for in_game_menu_UI button press
        private void exitNetGameButtonPress ()
        {
            ExitNetGameInputEvent();
        }
    }
}
