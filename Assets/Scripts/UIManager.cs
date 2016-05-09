using System;
using UnityEngine;

namespace SpaceBattles
{
    public class UIManager : MonoBehaviour
    {
        private enum UIState { IN_GAME, MAIN_MENU };
        private enum UIElements { GAME_UI, IN_GAME_MENU, MAIN_MENU}

        public PlayerShipController ship;
        public Canvas UI_2D_Canvas;

        public GameObject game_UI_prefab;
        public GameObject in_game_menu_UI_prefab;
        public GameObject main_menu_UI_prefab;

        public GameObject game_UI;
        public GameObject in_game_menu_UI;
        public GameObject main_menu_UI;

        private bool in_game_menu_visible = false;
        private UIState ui_state = UIState.IN_GAME;


        void Start ()
        {
        }

        void Update ()
        {
            if (ui_state == UIState.IN_GAME)
            {
                if (Input.GetKeyDown("escape"))
                {
                    toggleInGameMenu();
                }

                if (Input.GetKeyDown("space"))
                {
                    Debug.Log("spacebar pressed");
                    ship.accelerate(new Vector3(0, 0, 1));
                }
                else if (Input.GetKeyUp("space"))
                {
                    Debug.Log("spacebar released");
                    ship.brake();
                }
                
                foreach (Touch touch in Input.touches)
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        ship.accelerate(new Vector3(0, 0, 1));
                        break;
                    }
                    else if (touch.phase == TouchPhase.Ended)
                    {
                        ship.brake();
                        break;
                    }
                }

            }
        }

        public void enteringMultiplayerGame ()
        {
            ui_state = UIState.IN_GAME;
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
            game_UI.SetActive(true);
        }
        public void hideGameUI()
        {
            game_UI.SetActive(false);
        }

        public void showMainMenu ()
        {

        }

        public void hideMainMenu ()
        {

        }
    }
}
