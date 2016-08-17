using System;
using UnityEngine;

namespace SpaceBattles
{
    public class InGameMenuManager : MonoBehaviour
    {
        // Propagates events from child UI elements upwards to this object,
        // hopefully making hookup simpler (sorry if this is horrible! I'm new to this & experimenting)
        public event ButtonExitNetworkGame.exitNetworkGameButtonPressEventHandler ExitNetGameButtonPress
        {
            add { exit_game_button_manager.ExitNetGameButtonPress += value; }
            remove { exit_game_button_manager.ExitNetGameButtonPress -= value; }
        }

        public GameObject exit_game_button_object;
        private ButtonExitNetworkGame exit_game_button_manager;

        void Awake()
        {
            exit_game_button_manager
                = exit_game_button_object.GetComponent<ButtonExitNetworkGame>();
        }
    }
}
