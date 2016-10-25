using System;
using UnityEngine;

namespace SpaceBattles
{
    public class PCInputAdapter : InputAdapterModule
    {
        private GameObject _virtual_joystick_backing_object;
        private CnControls.SimpleJoystick virtual_joystick;
        private bool _player_wants_joystick_enabled;

        public bool virtual_joystick_enabled
        {
            get
            {
                return _player_wants_joystick_enabled
                    && VirtualJoystickElement != null;
            }
            set
            {
                _player_wants_joystick_enabled = value;
            }
        }
        public bool invert_pitch_controls { private get; set; }
        public bool invert_roll_controls { private get; set; }
        public GameObject VirtualJoystickElement
        {
            set
            {
                _virtual_joystick_backing_object = value;
                if (value != null)
                {
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

        /// <summary>
        /// Unused in this implementation
        /// </summary>
        public GameObject AccelerateButtonElement
        {
            set; private get;
        }

        public bool game_UI_enabled
        {
            set
            {
                if (virtual_joystick_enabled)
                {
                    VirtualJoystickElement.SetActive(value);
                }
            }
        }

        /// <summary>
        /// Unused in this implementation
        /// </summary>
        public GameObject fire_button_element
        {
            set; private get;
        }

        public string AccelerateButtonName
        {
            set; private get;
        }

        public string fire_button_name
        {
            set; private get;
        }

        public PCInputAdapter ()
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
            if (virtual_joystick_enabled)
            {
                return virtual_joystick.receiving_input;
            }
            else
            {
                return CnControls
                      .CnInputManager
                      .GetAxis("Acceleration") > 0;
            }
        }

        public bool brakeInput()
        {
            return !accelerateInput();
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

