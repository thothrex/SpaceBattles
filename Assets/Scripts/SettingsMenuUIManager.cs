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
        public ToggleSettingManager FPSCounterVisibilitySetting;
        public ToggleSettingManager NetworkTesterVisibilitySetting;
        public ToggleSettingManager PingDisplayVisibilitySetting;
        public SliderAndTextBoxInput FrameRateCapSetting;

        private OptionalEventModule oem = new OptionalEventModule();
        private HashSet<UIElements> ActiveUIElements
            = new HashSet<UIElements>();
        
        // -- delegates --
        public delegate void UIElementSettingHandler(bool enabled);
        public delegate void IntegerSettingHandler(int value);

        // -- events --
        public UnityEvent ExitSettingsMenuEvent;
        public event UIElementSettingHandler VirtualJoystickSetEvent;
        public event UIElementSettingHandler AccelerateButtonSet;
        public event UIElementSettingHandler FireButtonSet;
        public event UIElementSettingHandler FPSCounterVisibilitySet;
        public event UIElementSettingHandler NetworkTesterVisibilitySet;
        public event UIElementSettingHandler PingDisplayVisibilitySet;
        public event IntegerSettingHandler FrameRateCapSet;

        // -- enums --
        public enum Setting {
            AccelerateButton,
            FireButton,
            VirtualJoystick,
            FrameRateCap,
            FPSCounter,
            NetworkTester
        }

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
            MyContract.RequireFieldNotNull(
                FrameRateCapSetting,
                "Frame-rate Cap Setting"
            );
            MyContract.RequireFieldNotNull(
                FPSCounterVisibilitySetting,
                "FPS Counter Visibility Setting"
            );
            MyContract.RequireFieldNotNull(
                NetworkTesterVisibilitySetting,
                "Network Tester Visibility Setting"
            );
            MyContract.RequireFieldNotNull(
                PingDisplayVisibilitySetting,
                "Ping Display Visibility Setting"
            );

            AccelerateButtonSetting.ToggleSet
                += PropagateAccelerateButtonSet;
            FireButtonSetting.ToggleSet
                += PropagateFireButtonSet;
            VirtualJoystickSetting.ToggleSet
                += PropagateJoystickSet;
            FPSCounterVisibilitySetting.ToggleSet
                += PropagateFPSCounterVisibilitySet;
            FrameRateCapSetting.FrameRateCapSet
                += PropagateFrameRateCapSet;
            NetworkTesterVisibilitySetting.ToggleSet
                += PropagateNetworkTesterVisibilitySet;
            PingDisplayVisibilitySetting.ToggleSet
                += PropagatePingDisplayVisibilitySet;
        }

        public void ExitSettingsMenu ()
        {
            ExitSettingsMenuEvent.Invoke();
        }

        public void ToggleAccelerateButton()
            { ToggleButton(AccelerateButtonSetting, "AccelerateButtonSetting"); }
        public void ToggleFireButton()
            { ToggleButton(FireButtonSetting, "FireButtonSetting"); }
        public void toggleVirtualJoystick ()
            { ToggleButton(VirtualJoystickSetting, "VirtualJoystickSetting"); }
        public void ToggleFPSCounterVisibilitySetting ()
            { ToggleButton(FPSCounterVisibilitySetting, "FPS Counter Visibility Setting"); }
        public void ToggleNetworkTesterVisibilitySetting ()
            { ToggleButton(NetworkTesterVisibilitySetting, "Network Tester Visibility Setting"); }
        public void TogglePingDisplayVisibilitySetting()
            { ToggleButton(PingDisplayVisibilitySetting, "Ping Display Visibility Setting"); }

        public void DisplayVirtualJoystickButtonState (bool on)
            { DisplayToggleSettingState(VirtualJoystickSetting, "VirtualJoystickSetting", on); }
        public void DisplayFireButtonState (bool on)
            { DisplayToggleSettingState(FireButtonSetting, "FireButtonSetting", on); }
        public void DisplayAccelerateButtonState (bool on)
            { DisplayToggleSettingState(AccelerateButtonSetting, "AccelerateButtonSetting", on); }
        public void DisplayFPSCounterButtonState (bool on)
            { DisplayToggleSettingState(FPSCounterVisibilitySetting, "FPS Counter Visiblity Setting", on); }
        public void DisplayNetworkTesterVisibilityState(bool on)
            { DisplayToggleSettingState(NetworkTesterVisibilitySetting, "Network Tester Visibility Setting", on); }
        public void DisplayPingDisplayVisibilityState(bool on)
        { DisplayToggleSettingState(PingDisplayVisibilitySetting, "Ping Display Visibility Setting", on); }

        public void DisplayFrameRateCapState (double initialValue)
        {
            MyContract.RequireFieldNotNull(
                FrameRateCapSetting,
                "Frame-rate Cap Setting"
            );
            FrameRateCapSetting.DisplayValue(initialValue);
        }        

        private void PropagateJoystickSet (bool on)
            { PropagateVisibilitySet(VirtualJoystickSetEvent, on); }
        private void PropagateAccelerateButtonSet(bool on)
            { PropagateVisibilitySet(AccelerateButtonSet, on); }
        private void PropagateFireButtonSet(bool on)
            { PropagateVisibilitySet(FireButtonSet, on); }
        private void PropagateFPSCounterVisibilitySet (bool visible)
            { PropagateVisibilitySet(FPSCounterVisibilitySet, visible); }
        private void PropagateNetworkTesterVisibilitySet (bool visible)
            { PropagateVisibilitySet(NetworkTesterVisibilitySet, visible); }
        private void PropagatePingDisplayVisibilitySet (bool visible)
            { PropagateVisibilitySet(PingDisplayVisibilitySet, visible); }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="OverflowException"></exception>
        /// <param name="newFrameRateCap"></param>
        private void PropagateFrameRateCapSet (double newFrameRateCap)
        {
            int cap = Convert.ToInt32(newFrameRateCap);
            var e = FrameRateCapSet;
            if (oem.shouldTriggerEvent(e))
            {
                e.Invoke(cap);
            }
        }


        private void ToggleButton(ToggleSettingManager tsm, string settingName)
        {
            MyContract.RequireFieldNotNull(tsm, settingName);
            tsm.Toggle();
        }

        private void
        DisplayToggleSettingState
            (ToggleSettingManager tsm, string settingName, bool on)
        {
            MyContract.RequireFieldNotNull(tsm, settingName);
            tsm.SetInitialToggleGraphicState(on);
        }

        private void
        PropagateVisibilitySet
            (UIElementSettingHandler propagatingEvent, bool visible)
        {
            var e = propagatingEvent;
            if (oem.shouldTriggerEvent(e))
            {
                e.Invoke(visible);
            }
        }
    }  
}
