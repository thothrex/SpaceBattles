using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpaceBattles
{
    public class SettingsMenuUIManager : MonoBehaviour
    {
        public ToggleSettingManager VirtualJoystickSetting;
        public ToggleSettingManager AccelerateButtonSetting;
        public ToggleSettingManager FireButtonSetting;

        private OptionalEventModule oem = new OptionalEventModule();
        private HashSet<UIElements> ActiveUIElements
            = new HashSet<UIElements>();
        
        // -- delegates --
        public delegate void UIElementSettingHandler(bool enabled);

        // -- events --
        public UnityEvent ExitSettingsMenuEvent;
        public event UIElementSettingHandler VirtualJoystickSetEvent;
        public event UIElementSettingHandler AccelerateButtonSet;
        public event UIElementSettingHandler FireButtonSet;

        // -- enums --
        public enum Setting { AccelerateButton, FireButton, VirtualJoystick}

        // -- Methods --

        public void Start ()
        {
            MyContract.RequireFieldNotNull(
                AccelerateButtonSetting,
                "AccelerateButtonSetting"
            );
            MyContract.RequireFieldNotNull(
                FireButtonSetting,
                "FireButtonSetting"
            );
            MyContract.RequireFieldNotNull(
                VirtualJoystickSetting,
                "VirtualJoystickSetting"
            );

            AccelerateButtonSetting.ToggleSet
                += PropagateAccelerateButtonSet;
            FireButtonSetting.ToggleSet
                += PropagateFireButtonSet;
            VirtualJoystickSetting.ToggleSet
                += PropagateJoystickSet;
        }

        public void ExitSettingsMenu ()
        {
            ExitSettingsMenuEvent.Invoke();
        }

        public void ToggleAccelerateButton()
        {
            MyContract.RequireFieldNotNull(
                AccelerateButtonSetting,
                "AccelerateButtonSetting"
            );
            AccelerateButtonSetting.Toggle();
        }

        public void ToggleFireButton()
        {
            MyContract.RequireFieldNotNull(
                FireButtonSetting,
                "FireButtonSetting"
            );
            FireButtonSetting.Toggle();
        }

        public void toggleVirtualJoystick ()
        {
            MyContract.RequireFieldNotNull(
                VirtualJoystickSetting,
                "VirtualJoystickSetting"
            );
            VirtualJoystickSetting.Toggle();
        }
        
        public void DisplayVirtualJoystickButtonState (bool on)
        {
            MyContract.RequireFieldNotNull(
                VirtualJoystickSetting,
                "VirtualJoystickSetting"
            );
            VirtualJoystickSetting.SetInitialToggleGraphicState(on);
        }

        public void DisplayFireButtonState (bool on)
        {
            MyContract.RequireFieldNotNull(
                FireButtonSetting,
                "FireButtonSetting"
            );
            FireButtonSetting.SetInitialToggleGraphicState(on);
        }

        public void DisplayAccelerateButtonState (bool on)
        {
            MyContract.RequireFieldNotNull(
                AccelerateButtonSetting,
                "AccelerateButtonSetting"
            );
            AccelerateButtonSetting.SetInitialToggleGraphicState(on);
        }

        private void PropagateJoystickSet (bool on)
        {
            VirtualJoystickSetEvent.Invoke(on);
        }

        private void PropagateAccelerateButtonSet(bool on)
        {
            AccelerateButtonSet.Invoke(on);
        }

        private void PropagateFireButtonSet(bool on)
        {
            FireButtonSet.Invoke(on);
        }
    }  
}
