using System;
using UnityEngine;

namespace SpaceBattles
{
    public class MainMenuUIManager : MonoBehaviour
    {
        // Propagates events from child UI elements upwards to this object,
        // hopefully making hookup simpler (sorry if this is horrible! I'm new to this & experimenting)
        public event ButtonMainMenuPlayGame.PlayGameButtonPressEventHandler PlayGameButtonPress
        {
            add { start_game_button_manager.PlayGameButtonPress += value; }
            remove { start_game_button_manager.PlayGameButtonPress -= value; }
        }

        public GameObject start_game_button_object;
        private ButtonMainMenuPlayGame start_game_button_manager;

        void Awake()
        {
            start_game_button_manager
                = start_game_button_object.GetComponent<ButtonMainMenuPlayGame>();
        }
    }
}
