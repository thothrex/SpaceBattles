using System;
using System.Collections;
using UnityEngine;

namespace SpaceBattles
{
    public class UIManager : MonoBehaviour
    {
        private enum UIState { IN_GAME, MAIN_MENU };
        private enum UIElements { GAME_UI, IN_GAME_MENU, MAIN_MENU}
        // searching for server deliberately unused as we can just go there immediately
        // once the play button is pressed
        public  enum PlayerConnectState { IDLE, SEARCHING_FOR_SERVER, JOINING_SERVER, CREATING_SERVER };

        public bool dont_destroy_on_load;

        public GameObject player_object;
        public PlayerShipController ship_controller;

        public GameObject game_UI_prefab;
        public GameObject in_game_menu_UI_prefab;
        public GameObject main_menu_UI_prefab;
        public GameObject player_centred_UI_prefab;
        public GameObject player_screen_game_UI_prefab;
        public GameObject ship_select_UI_prefab;
        public Camera fixed_UI_camera_prefab;
        public Camera ship_select_camera_prefab;

        public Vector3 player_centred_UI_offset;

        public GameObject game_UI;
        public GameObject in_game_menu_UI;
        public GameObject main_menu_UI;
        public GameObject player_centred_canvas_object;
        public GameObject player_screen_UI_object;
        public GameObject ship_select_UI_object;
        public PlayerScreenInGameUIManager player_screen_UI_manager;

        private bool UI_objects_instantiated = false;
        private Camera player_UI_camera = null; // for the which UI follows player avatar
        private Camera fixed_UI_camera = null; // UI doesn't move compared to camera
        private Camera ship_select_camera = null; // Camera inhabits a separate "scene"
        private bool in_game_menu_visible = false;
        private bool ship_select_menu_visible = false;
        private UIState ui_state = UIState.IN_GAME;
        private Canvas player_centred_canvas = null;
        private Canvas player_screen_canvas  = null;
        private MainMenuUIManager main_menu_UI_manager = null;
        private InGameMenuManager in_game_menu_manager = null;

        // Events are lower down

        void Awake ()
        {
            if (!UI_objects_instantiated)
            {
                Debug.Log("UI Manager instantiating UI objects");
                //game_UI                   = GameObject.Instantiate(game_UI_prefab);
                in_game_menu_UI             = GameObject.Instantiate(in_game_menu_UI_prefab);
                in_game_menu_manager        = in_game_menu_UI.GetComponent<InGameMenuManager>();
                main_menu_UI                = GameObject.Instantiate(main_menu_UI_prefab);
                main_menu_UI_manager        = main_menu_UI.GetComponent<MainMenuUIManager>();
                //player_centred_canvas_object = GameObject.Instantiate(player_centred_UI_prefab);
                //player_centred_canvas        = player_centred_canvas_object.GetComponent<Canvas>();
                player_screen_UI_object     = GameObject.Instantiate(player_screen_game_UI_prefab);
                player_screen_UI_manager    = player_screen_UI_object.GetComponent<PlayerScreenInGameUIManager>();
                ship_select_UI_object       = GameObject.Instantiate(ship_select_UI_prefab);

                // Initialise UI events structure
                in_game_menu_manager.ExitNetGameButtonPress += exitNetGameButtonPress;

                // Initialise cameras
                initialiseGameUICameras();
                hideShipSelectionUI();

                if (dont_destroy_on_load)
                {
                    //Sets this to not be destroyed when reloading scene
                    UnityEngine.Object.DontDestroyOnLoad(gameObject);
                    UnityEngine.Object.DontDestroyOnLoad(player_centred_canvas_object);
                    UnityEngine.Object.DontDestroyOnLoad(player_screen_UI_object);
                    UnityEngine.Object.DontDestroyOnLoad(in_game_menu_UI);
                    UnityEngine.Object.DontDestroyOnLoad(main_menu_UI);
                    UnityEngine.Object.DontDestroyOnLoad(ship_select_UI_object);
                    UnityEngine.Object.DontDestroyOnLoad(fixed_UI_camera);
                    UnityEngine.Object.DontDestroyOnLoad(ship_select_camera);
                    Debug.Log("objects prevented from being destroyed on load");
                }
                UI_objects_instantiated = true;
            }
        }

        void Start ()
        {
            enteringMainMenu();
        }

        void Update ()
        {
            if (ui_state == UIState.IN_GAME)
            {
#if UNITY_ANDROID
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ExitNetGameInputEvent();
                }

                if (Input.GetKeyDown(KeyCode.Menu))
                {
                    toggleInGameMenu();
                }
#endif
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                if (Input.GetButtonDown("menu2"))
                {
                    toggleInGameMenu();
                }
#endif

                // this can happen during transition
                // from game to menu (for a frame or two)
                if (ship_controller != null)
                {
                    if (Input.GetAxis("Acceleration") > 0)
                    {
                        ship_controller.accelerate(new Vector3(0, 0, 1));
                    }
                    else if (Input.GetAxis("Acceleration") == 0)
                    {
                        ship_controller.brake();
                    }

                    foreach (Touch touch in Input.touches)
                    {
                        if (touch.phase == TouchPhase.Began)
                        {
                            ship_controller.accelerate(new Vector3(0, 0, 1));
                            break;
                        }
                        else if (touch.phase == TouchPhase.Ended)
                        {
                            ship_controller.brake();
                            break;
                        }
                    }

                    if (Input.GetButtonDown("Fire"))
                    {
                        ship_controller.CmdFirePhaser();
                    }

                    if (Input.GetButtonDown("menu1"))
                    {
                        Debug.Log("ship select button pressed");
                        toggleShipSelectUI();
                    }
                }
            }
        }

        public void enteringMainMenu ()
        {
            ui_state = UIState.MAIN_MENU;
            main_menu_UI_manager.setPlayerConnectState(PlayerConnectState.IDLE);
            hideGameUI();
            hideInGameMenu();
            showMainMenu();
        }

        public void enteringMultiplayerGame ()
        {
            // TODO: change to start in ship selection
            ui_state = UIState.IN_GAME;
            showGameUI();
            hideInGameMenu();
            hideMainMenu();
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
                showGameUI();
            }
            else
            {
                showInGameMenu();
                hideGameUI();
            }
        }

        public void showInGameMenu ()
        {
            in_game_menu_UI.SetActive(true);
            in_game_menu_visible = true;
        }

        public void hideInGameMenu ()
        {
            in_game_menu_UI.SetActive(false);
            in_game_menu_visible = false;
        }

        public void showGameUI ()
        {
            //game_UI.SetActive(true);
            //player_centred_canvas_object.gameObject.SetActive(true);
            player_screen_UI_object.SetActive(true);
            setInGameUICamerasActive(true);
        }
        public void hideGameUI()
        {
            //game_UI.SetActive(false);
            //player_centred_canvas_object.gameObject.SetActive(false);
            player_screen_UI_object.SetActive(false);
            setInGameUICamerasActive(false);
        }

        public void showMainMenu ()
        {
            main_menu_UI.SetActive(true);
        }

        public void hideMainMenu ()
        {
            main_menu_UI.SetActive(false);
        }

        public void toggleShipSelectUI()
        {
            if (ship_select_menu_visible)
            {
                hideInGameMenu();
                hideGameUI();
                showShipSelectionUI();
            }
            else
            {
                showGameUI();
                hideShipSelectionUI();
            }
        }

        public void showShipSelectionUI ()
        {
            ship_select_UI_object.SetActive(true);
            ship_select_menu_visible = true;
            ship_select_camera.gameObject.SetActive(true);
        }

        public void hideShipSelectionUI()
        {
            ship_select_UI_object.SetActive(false);
            ship_select_menu_visible = false;
            ship_select_camera.gameObject.SetActive(false);
        }

        /// <summary>
        /// This is expected to be called at game start,
        /// as the cameras could be used from the main menu
        /// </summary>
        public void initialiseGameUICameras ()
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
        public void setInGameUICamerasActive (bool active)
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
        public void setPlayerCamera (Camera player_camera)
        {
            this.player_UI_camera = player_camera;
            if (player_camera == null)
            {
                Debug.Log("Warning - setting player camera to null");
            }
        }

        private const string CAMERA_NOT_SET_EXCEPTION_MESSAGE
            = "attempting to setPlayerShip without having set the camera first";

        public void setPlayerShip (GameObject player)
        {
            if (player_UI_camera == null)
            {
                throw new InvalidOperationException(CAMERA_NOT_SET_EXCEPTION_MESSAGE);
            }
            this.player_object = player;
            this.ship_controller = player.GetComponent<PlayerShipController>();
            //player_centred_canvas.worldCamera = player_UI_camera;
            //player_centred_canvas_object.transform.SetParent(player_object.transform);
            //player_centred_canvas_object.transform.localPosition = player_centred_UI_offset;
        }

        public void initShipCentredUI ()
        {
            throw new NotImplementedException("initShipCentred UI called - this function is not implemented yet");
            //player_object.AddComponent<Canvas>();
            // TODO: Unfinished
        }

        public void setCurrentPlayerHealth (double new_value)
        {
            player_screen_UI_manager.localPlayerSetCurrentHealth(new_value);
        }

        public void setCurrentPlayerMaxHealth (double new_value)
        {
            player_screen_UI_manager.localPlayerSetMaxHealth(new_value);
        }

        public void setPlayerConnectState (PlayerConnectState new_state)
        {
            main_menu_UI_manager.setPlayerConnectState(new_state);
        }

        // Propagates events from child UI elements upwards to this object,
        // hopefully making hookup simpler (sorry if this is horrible! I'm new to this & experimenting)
        public event ButtonMainMenuPlayGame.PlayGameButtonPressEventHandler PlayGameButtonPress
        {
            add { main_menu_UI_manager.PlayGameButtonPress += value; }
            remove { main_menu_UI_manager.PlayGameButtonPress -= value; }
        }

        public delegate void exitNetworkGameInputEventHandler();
        public event exitNetworkGameInputEventHandler ExitNetGameInputEvent;

        // event handler for in_game_menu_UI button press
        private void exitNetGameButtonPress ()
        {
            ExitNetGameInputEvent();
        }
    }
}
