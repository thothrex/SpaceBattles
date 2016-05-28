using System;
using UnityEngine;

namespace SpaceBattles
{
    public class UIManager : MonoBehaviour
    {
        private enum UIState { IN_GAME, MAIN_MENU };
        private enum UIElements { GAME_UI, IN_GAME_MENU, MAIN_MENU}

        public bool dont_destroy_on_load;

        public GameObject player_object;
        public PlayerShipController player_controller;

        public GameObject game_UI_prefab;
        public GameObject in_game_menu_UI_prefab;
        public GameObject main_menu_UI_prefab;
        public GameObject player_centred_UI_prefab;
        public GameObject player_screen_game_UI_prefab;

        public GameObject game_UI;
        public GameObject in_game_menu_UI;
        public GameObject main_menu_UI;
        public GameObject player_centred_canvas_object;
        public GameObject player_screen_UI_object;
        public PlayerScreenInGameUIManager player_screen_UI_manager;

        private Camera player_UI_camera = null;
        private bool in_game_menu_visible = false;
        private UIState ui_state = UIState.IN_GAME;
        private Canvas player_centred_canvas = null;
        private Canvas player_screen_canvas  = null;

        void Awake ()
        {
            //game_UI               = GameObject.Instantiate(game_UI_prefab);
            in_game_menu_UI       = GameObject.Instantiate(in_game_menu_UI_prefab);
            //main_menu_UI          = GameObject.Instantiate(main_menu_UI_prefab);
            player_centred_canvas_object = GameObject.Instantiate(player_centred_UI_prefab);
            player_centred_canvas        = player_centred_canvas_object.GetComponent<Canvas>();
            player_screen_UI_object      = GameObject.Instantiate(player_screen_game_UI_prefab);
            player_screen_UI_manager     = player_screen_UI_object.GetComponent<PlayerScreenInGameUIManager>();

            if (dont_destroy_on_load)
            {
                //Sets this to not be destroyed when reloading scene
                UnityEngine.Object.DontDestroyOnLoad(gameObject);
                UnityEngine.Object.DontDestroyOnLoad(player_centred_canvas_object);
                UnityEngine.Object.DontDestroyOnLoad(player_screen_UI_object);
                UnityEngine.Object.DontDestroyOnLoad(in_game_menu_UI);
                Debug.Log("UI manager prevented from being destroyed on load");
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
                if (Input.GetKeyDown("escape"))
                {
                    toggleInGameMenu();
                }

                // this can happen during transition
                // from game to menu (for a frame or two)
                if (player_controller != null)
                {
                    if (Input.GetAxis("Acceleration") > 0)
                    {
                        player_controller.accelerate(new Vector3(0, 0, 1));
                    }
                    else if (Input.GetAxis("Acceleration") == 0)
                    {
                        player_controller.brake();
                    }

                    foreach (Touch touch in Input.touches)
                    {
                        if (touch.phase == TouchPhase.Began)
                        {
                            player_controller.accelerate(new Vector3(0, 0, 1));
                            break;
                        }
                        else if (touch.phase == TouchPhase.Ended)
                        {
                            player_controller.brake();
                            break;
                        }
                    }

                    if (Input.GetAxis("Fire") > 0)
                    {
                        player_controller.CmdFirePhaser();
                    }
                }
            }
        }

        public void enteringMainMenu ()
        {
            ui_state = UIState.MAIN_MENU;
            hideGameUI();
            hideInGameMenu();
            showMainMenu();
        }

        public void enteringMultiplayerGame ()
        {
            ui_state = UIState.IN_GAME;
            showGameUI();
            hideInGameMenu();
            hideMainMenu();
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
            // regardless
            in_game_menu_visible = !in_game_menu_visible;
        }

        public void showInGameMenu ()
        {
            in_game_menu_UI.SetActive(true);
        }

        public void hideInGameMenu ()
        {
            in_game_menu_UI.SetActive(false);
        }

        public void showGameUI ()
        {
            //game_UI.SetActive(true);
            //player_centred_canvas_object.gameObject.SetActive(true);
            player_screen_UI_object.SetActive(true);
        }
        public void hideGameUI()
        {
            //game_UI.SetActive(false);
            //player_centred_canvas_object.gameObject.SetActive(false);
            player_screen_UI_object.SetActive(false);
        }

        public void showMainMenu ()
        {

        }

        public void hideMainMenu ()
        {

        }

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
            this.player_controller = player.GetComponent<PlayerShipController>();
            player_centred_canvas.worldCamera = player_UI_camera;
            player_centred_canvas_object.transform.SetParent(player_object.transform);
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
    }
}
