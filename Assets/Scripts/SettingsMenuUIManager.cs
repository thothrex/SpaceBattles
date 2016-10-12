using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBattles
{
    public class SettingsMenuUIManager : MonoBehaviour
    {
        public bool editor_virtual_joystick_initial_state;
        public List<EasyTween> joystick_button_tweens;
        public List<EasyTween> joystick_button_antitweens;
        public MyWaypointMover joystick_button_mover;

        private bool virtual_joystick_state;
        private bool done_init = false;
        private OptionalEventModule oem = new OptionalEventModule();
        
        // -- delegates --
        public delegate void exitSettingsMenuEventHandler();
        public delegate void virtualJoystickSettingHandler(bool enabled);

        // -- events --
        public event exitSettingsMenuEventHandler ExitSettingsMenuEvent;
        public event virtualJoystickSettingHandler VirtualJoystickSetEvent;

        public void Start ()
        {
            ensureInit();
        }

        public void exitSettingsMenu ()
        {
            ExitSettingsMenuEvent();
        }

        public void toggleVirtualJoystick ()
        {
            virtual_joystick_state = !virtual_joystick_state;
            var handler = VirtualJoystickSetEvent;
            if (oem.shouldTriggerEvent(handler))
            {
                handler(virtual_joystick_state);
            }
        }

        public void displayVirtualJoystickButtonState (bool on)
        {
            MyContract.RequireFieldNotNull(joystick_button_mover,
                                           "joystick_button_mover");

            Debug.Log("Current joystick state: " + virtual_joystick_state.ToString());
            Debug.Log("Setting joystick state "
                    + (on ? "on" : "off"));
            ensureInit();
            if (on != virtual_joystick_state)
            {
                foreach (EasyTween et in joystick_button_tweens)
                {
                    et.OpenCloseObjectAnimation();
                }
                foreach (EasyTween at in joystick_button_antitweens)
                {
                    at.OpenCloseObjectAnimation();
                }
            }
            joystick_button_mover.setMoveState(on);
            virtual_joystick_state = on;
        }

        private void ensureInit()
        {
            if (!done_init)
            {
                Debug.Log("initialising settings menu from editor values");
                virtual_joystick_state = editor_virtual_joystick_initial_state;
                done_init = true;
            }
        }
    }  
}
