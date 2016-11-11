using System;
using UnityEngine;
using UnityEngine.Events;

namespace SpaceBattles
{
    public class InGameMenuManager : MonoBehaviour
    {
        // -- delegates --

        // -- events --

        public UnityEvent ExitInGameMenuEvent;
        public UnityEvent EnterSettingsMenuEvent;
        public UnityEvent ExitNetGameButtonPress;

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
            EnterSettingsMenuEvent.Invoke();
        }

        public void exitInGameMenu()
        {
            ExitInGameMenuEvent.Invoke();
        }

        public void ExitNetGameButtonPressPropagator ()
        {
            ExitNetGameButtonPress.Invoke();
        }
    }
}
