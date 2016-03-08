using System;
using UnityEngine;

namespace SpaceBattles
{
    public class UIManager : MonoBehaviour
    {
        public enum UIState
        {
            MAIN_MENU,
            GAME_UI
        }
        public GameObject game_UI;
        public GameObject main_menu_UI;
        private UIState current_UI_state;

        public UIManager()
        {
        }

        void Start()
        {
            swapToGame();
        }

        public void toggleInGameMenu ()
        {
            if (current_UI_state == UIState.GAME_UI)
            {
                swapToMenu();
            }
            else if (current_UI_state == UIState.MAIN_MENU)
            {
                swapToGame();
            }
        }

        public void swapToMenu ()
        {
            game_UI.SetActive(false);
            main_menu_UI.SetActive(true);
            current_UI_state = UIState.MAIN_MENU;
        }

        public void swapToGame ()
        {
            main_menu_UI.SetActive(false);
            game_UI.SetActive(true);
            current_UI_state = UIState.GAME_UI;
        }
    }
}
