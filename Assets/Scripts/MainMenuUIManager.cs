using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SpaceBattles
{
    public class MainMenuUIManager : MonoBehaviour
    {
        // -- Constant Fields --

        // -- Fields --
        public GameObject StartGameButtonObject;
        public GameObject BackgroundOrbitalBody;
        public ExplicitLayoutGroup MobileLayout;
        public ExplicitLayoutGroup DesktopLayout;
        [Tooltip("Relative to km, e.g. 0.001 means metres")]
        public double BackgroundOrbitalBodyScale = 0.01;

        private ButtonMainMenuPlayGame start_game_button_manager;
        private MenuLayout CurrentLayout = MenuLayout.Desktop;
        private MenuLayout target_layout = MenuLayout.Desktop;

        // -- Delegates --

        // -- Events --
        public UnityEvent EnterSettingsMenuEvent;
        public UnityEvent EnterOrreryMenuEvent;
        public UnityEvent ExitProgramEvent;
        public UnityEvent PlayGameButtonPress;

        // -- Enums --
        private enum MenuLayout { Desktop, Mobile }

        
        // -- Methods --
        public void Awake ()
        {
            start_game_button_manager
                = StartGameButtonObject.GetComponent<ButtonMainMenuPlayGame>();
            start_game_button_manager.PlayGameButtonPress += PlayGameButtonPressPropagator;
        }

        public void Start ()
        {
            Debug.Log("Setting explicit background earth scale");
            BackgroundOrbitalBody
                .GetComponent<OrbitingBodyBackgroundGameObject>()
                .SetRelativeScaleExplicitly(BackgroundOrbitalBodyScale);
        }

        public void Update ()
        {
            // Need to do this "late" change
            // in order to avoid Unity crashes.
            // I don't understand why it crashes.
            //
            // Actually I think the cause of the crash was an infinite loop
            // I had elsewhere. This "late" update method
            // can probably be safely removed.
            if (target_layout != CurrentLayout)
            {
                switch (target_layout)
                {
                    case MenuLayout.Desktop:
                        doLayoutChangeToDesktop();
                        break;
                    case MenuLayout.Mobile:
                        doLayoutChangeToMobile();
                        break;
                    default:
                        throw new UnexpectedEnumValueException<MenuLayout>(target_layout);
                }
            }
        }

        public void SetPlayerConnectState (UIManager.PlayerConnectState new_state)
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

        /// <summary>
        /// Passthrough to the relevant event
        /// so that the OnClick can be hooked up in the editor
        /// </summary>
        public void enterSettingsMenu ()
        {
            EnterSettingsMenuEvent.Invoke();
        }

        /// <summary>
        /// Passthrough to the relevant event
        /// so that the OnClick can be hooked up in the editor
        /// </summary>
        public void enterOrrery ()
        {
            EnterOrreryMenuEvent.Invoke();
        }

        /// <summary>
        /// Passthrough to the relevant event
        /// so that the OnClick can be hooked up in the editor
        /// </summary>
        public void ExitProgram ()
        {
            ExitProgramEvent.Invoke();
        }

        public void setLayoutToMobile ()
        {
            MyContract.RequireFieldNotNull(MobileLayout, "Mobile Layout");

            Debug.Log("MainMenuManager setting layout to mobile");

            target_layout = MenuLayout.Mobile;
        }

        public void setLayoutToDesktop ()
        {
            MyContract.RequireFieldNotNull(DesktopLayout, "Desktop Layout");

            Debug.Log("MainMenuManager setting layout to desktop");

            target_layout = MenuLayout.Desktop;
        }

        public void PlayGameButtonPressPropagator ()
        {
            PlayGameButtonPress.Invoke();
        }

        private void doLayoutChangeToMobile ()
        {
            MobileLayout.applyLayout();
            CurrentLayout = MenuLayout.Mobile;
        }

        private void doLayoutChangeToDesktop()
        {
            DesktopLayout.applyLayout();
            CurrentLayout = MenuLayout.Desktop;
        }
    }
}
