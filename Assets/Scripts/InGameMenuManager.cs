using System;
using UnityEngine;

namespace SpaceBattles
{
    public class InGameMenuManager : MonoBehaviour
    {
        // -- delegates --
        public delegate void enterSettingsMenuEventHandler();
        public delegate void exitInGameMenuEventHandler();

        // -- events --

        public event exitInGameMenuEventHandler ExitInGameMenuEvent;
        public event enterSettingsMenuEventHandler EnterSettingsMenuEvent;
        // Propagates events from child UI elements upwards to this object,
        // hopefully making hookup simpler (sorry if this is horrible! I'm new to this & experimenting)
        public event ButtonExitNetworkGame.exitNetworkGameButtonPressEventHandler ExitNetGameButtonPress;

        public GameObject exit_game_button_object;
        private ButtonExitNetworkGame ExitGameButtonManager;

        public void Awake()
        {
            ExitGameButtonManager
                = exit_game_button_object.GetComponent<ButtonExitNetworkGame>();
            ExitGameButtonManager.ExitNetGameButtonPress += ExitNetGameButtonPressPropagator;
        }

        public void enterSettingsMenu()
        {
            EnterSettingsMenuEvent();
        }

        public void exitInGameMenu()
        {
            ExitInGameMenuEvent();
        }

        public void ExitNetGameButtonPressPropagator ()
        {
            ExitNetGameButtonPress();
        }
    }
}
