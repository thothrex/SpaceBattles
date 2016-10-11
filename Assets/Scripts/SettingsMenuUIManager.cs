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
        private OptionalEventModule oem = new OptionalEventModule();
        
        // -- delegates --
        public delegate void exitSettingsMenuEventHandler();
        public delegate void virtualJoystickSettingHandler(bool enabled);

        // -- events --
        public event exitSettingsMenuEventHandler ExitSettingsMenuEvent;
        public event virtualJoystickSettingHandler VirtualJoystickSetEvent;

        public void Start ()
        {
            virtual_joystick_state = editor_virtual_joystick_initial_state;
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

        public void initialiseVirtualJoystickButtonState (bool on)
        {
            MyContract.RequireFieldNotNull(joystick_button_mover,
                                           "joystick_button_mover");

            foreach (EasyTween et in joystick_button_tweens)
            {
                et.ChangeSetState(on);
            }
            foreach (EasyTween at in joystick_button_antitweens)
            {
                at.ChangeSetState(!on);
            }
            joystick_button_mover.setMoveState(on);
        }
    }  
}
