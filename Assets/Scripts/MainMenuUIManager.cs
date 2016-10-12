using System;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBattles
{
    public class MainMenuUIManager : MonoBehaviour
    {
        // -- fields --
        public GameObject StartGameButtonObject;
        public GameObject ButtonBlock;
        public GameObject ButtonGroup;
        public GameObject HeaderBlock;
        public Vector2 MobileButtonBlockAnchorMax;
        public Vector2 DesktopButtonBlockAnchorMax;

        private ButtonMainMenuPlayGame start_game_button_manager;
        private MENU_LAYOUT current_layout = MENU_LAYOUT.DESKTOP;
        private MENU_LAYOUT target_layout = MENU_LAYOUT.DESKTOP;

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

        // -- enums --
        public enum MENU_LAYOUT { DESKTOP, MOBILE }

        
        // -- methods --
        public void Awake()
        {
            start_game_button_manager
                = StartGameButtonObject.GetComponent<ButtonMainMenuPlayGame>();
        }

        public void Update ()
        {
            // Need to do this "late" change
            // in order to avoid Unity crashes.
            // I don't understand why it crashes.
            if (target_layout != current_layout)
            {
                switch (target_layout)
                {
                    case MENU_LAYOUT.DESKTOP:
                        doLayoutChangeToDesktop();
                        break;
                    case MENU_LAYOUT.MOBILE:
                        doLayoutChangeToMobile();
                        break;
                    default:
                        throw new UnexpectedEnumValueException<MENU_LAYOUT>(target_layout);
                }
            }
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

        public void setLayoutToMobile ()
        {
            MyContract.RequireFieldNotNull(ButtonBlock, "ButtonBlock");
            MyContract.RequireFieldNotNull(HeaderBlock, "HeaderBlock");

            Debug.Log("MainMenuManager setting layout to mobile");

            target_layout = MENU_LAYOUT.MOBILE;
        }

        public void setLayoutToDesktop ()
        {
            MyContract.RequireFieldNotNull(ButtonBlock, "ButtonBlock");
            MyContract.RequireFieldNotNull(HeaderBlock, "HeaderBlock");

            Debug.Log("MainMenuManager setting layout to desktop");

            target_layout = MENU_LAYOUT.DESKTOP;
        }

        private void doLayoutChangeToMobile ()
        {
            HeaderBlock.SetActive(false);

            RectTransform bbt //button block transform
                = ButtonBlock.GetComponent<RectTransform>();
            MyContract.RequireFieldNotNull(
                bbt, "ButtonBlock RectTranform component"
            );
            bbt.anchorMax = MobileButtonBlockAnchorMax;

            VerticalLayoutGroup bgvlg //button group vertical layout group
                = ButtonGroup.GetComponent<VerticalLayoutGroup>();
            MyContract.RequireFieldNotNull(
                bgvlg, "ButtonGroup VerticalLayoutGroup component"
            );
            bgvlg.childAlignment = TextAnchor.UpperCenter;

            HorizontalLayoutGroup hlg
                = ButtonBlock.GetComponent<HorizontalLayoutGroup>();
            MyContract.RequireFieldNotNull(
                hlg, "ButtonBlock HorizontalLayoutGroup component"
            );
            hlg.childAlignment = TextAnchor.UpperCenter;

            current_layout = MENU_LAYOUT.MOBILE;
        }

        private void doLayoutChangeToDesktop()
        {
            HeaderBlock.SetActive(true);

            RectTransform bbt //button block transform
                = ButtonBlock.GetComponent<RectTransform>();
            MyContract.RequireFieldNotNull(
                bbt, "ButtonBlock RectTranform component"
            );
            bbt.anchorMax = DesktopButtonBlockAnchorMax;

            VerticalLayoutGroup bgvlg //button group vertical layout group
                = ButtonGroup.GetComponent<VerticalLayoutGroup>();
            MyContract.RequireFieldNotNull(
                bgvlg, "ButtonGroup VerticalLayoutGroup component"
            );
            bgvlg.childAlignment = TextAnchor.LowerLeft;

            HorizontalLayoutGroup hlg
                = ButtonBlock.GetComponent<HorizontalLayoutGroup>();
            MyContract.RequireFieldNotNull(
                hlg, "ButtonBlock HorizontalLayoutGroup component"
            );
            hlg.childAlignment = TextAnchor.LowerLeft;

            current_layout = MENU_LAYOUT.DESKTOP;
        }
    }
}
