using System;
using UnityEngine;

namespace SpaceBattles
{
    public class MainMenuUIManager : MonoBehaviour
    {
        // -- fields --
        public GameObject start_game_button_object;
        private ButtonMainMenuPlayGame start_game_button_manager;

        // -- delegates --
        public delegate void enterSettingsMenuEventHandler();

        // -- events --
        public event enterSettingsMenuEventHandler EnterSettingsMenuEvent;
        // Propagates events from child UI elements upwards to this object,
        // hopefully making hookup simpler (sorry if this is horrible! I'm new to this & experimenting)
        public event PlayGameButtonPressEventHandler PlayGameButtonPress
        {
            add { start_game_button_manager.PlayGameButtonPress += value; }
            remove { start_game_button_manager.PlayGameButtonPress -= value; }
        }

        void Awake()
        {
            start_game_button_manager
                = start_game_button_object.GetComponent<ButtonMainMenuPlayGame>();
        }

        public void setPlayerConnectState (UIManager.PlayerConnectState new_state)
        {
            if (new_state == UIManager.PlayerConnectState.IDLE)
            {
                start_game_button_manager.setButtonState(0);
            }
            else if (new_state == UIManager.PlayerConnectState.JOINING_SERVER)
            {
                start_game_button_manager.setButtonState(2);
            }
            else if (new_state == UIManager.PlayerConnectState.CREATING_SERVER)
            {
                start_game_button_manager.setButtonState(3);
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    "new_state", new_state, "Attempting to display unexpected player connection state in UI"
                );
            }
        }

        public void enterSettingsMenu ()
        {
            EnterSettingsMenuEvent();
        }
    }
}
