using System;
using UnityEngine;

namespace SpaceBattles
{
    public class PCInputManager : InputAdapterModule
    {
        private GameObject _virtual_joystick_backing_object;
        private CnControls.SimpleJoystick virtual_joystick;
        private bool _player_wants_joystick_enabled;

        public bool virtual_joystick_enabled
        {
            get
            {
                return _player_wants_joystick_enabled
                    && virtual_joystick_element != null // short-circuit
                    && virtual_joystick_element.activeInHierarchy;
            }
            set
            {
                _player_wants_joystick_enabled = value;
                if (virtual_joystick_element != null)
                {
                    virtual_joystick_element.SetActive(value);
                }
            }
        }
        public bool invert_pitch_controls { private get; set; }
        public bool invert_roll_controls { private get; set; }
        public GameObject virtual_joystick_element
        {
            set
            {
                _virtual_joystick_backing_object = value;
                if (value != null)
                {
                    _virtual_joystick_backing_object.SetActive(
                        _player_wants_joystick_enabled
                    );
                    virtual_joystick
                        = _virtual_joystick_backing_object
                        .GetComponent<CnControls.SimpleJoystick>();
                }
                else
                {
                    virtual_joystick = null;
                }
            }

            private get
            {
                return _virtual_joystick_backing_object;
            }
        }

        public PCInputManager ()
        {
            // set default values
            // testing
            virtual_joystick_enabled = true;
            invert_pitch_controls    = true;
            invert_roll_controls     = false;
            //virtual_joystick_enabled = false;
        }

        public bool accelerateInput()
        {
            return CnControls.CnInputManager.GetAxis("Acceleration") > 0;
        }

        public bool brakeInput()
        {
            return CnControls.CnInputManager.GetAxis("Acceleration") == 0;
        }

        public bool fireInput()
        {
            return CnControls.CnInputManager.GetButtonDown("Fire");
        }
        
        public bool shipSelectMenuOpenInput()
        {
            return CnControls.CnInputManager.GetButtonDown("menu1");
        }

        // Primary in-game menu
        public bool inGameMenuOpenInput()
        {
            return CnControls.CnInputManager.GetButtonDown("menu2");
        }

        /// <summary>
        /// No such button exists for the PC version.
        /// </summary>
        /// <returns>Always false</returns>
        public bool exitNetGameInput ()
        {
            return false;
        }

        public float getRollInputValue ()
        {
            // accel for accelerometer,
            // GetAxis for keyboard and virtual joystick
            float new_roll
                    = -Input.acceleration.x * 0.5f
                    + (-CnControls.CnInputManager.GetAxis("Roll"))
                    ;
            if (invert_roll_controls)
            {
                new_roll *= -1;
            }

            return new_roll;
        }

        public float getPitchInputValue ()
        {
            // accel for accelerometer,
            // GetAxis for keyboard and virtual joystick
            float new_pitch
                    = Input.acceleration.z * 0.5f
                    + (-CnControls.CnInputManager.GetAxis("Pitch"))
                    ;
            if (invert_pitch_controls)
            {
                new_pitch *= -1;
            }
            return new_pitch;
        }
    }
}

