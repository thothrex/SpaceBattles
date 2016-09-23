using System;
using UnityEngine;

namespace SpaceBattles
{
    public class AndroidInputManager : InputAdapterModule
    {
        private GameObject _virtual_joystick_backing_object;
        private CnControls.SimpleJoystick virtual_joystick;
        private bool _player_wants_joystick_enabled;

        public bool virtual_joystick_enabled
        {
            get
            {
                return _player_wants_joystick_enabled
                    && virtual_joystick_element != null;
            }
            set
            {
                Debug.Log((value ? "Enabling" : "Disabling")
                         + " the virtual joystick.");
                _player_wants_joystick_enabled = value;
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

        public GameObject accelerate_button_element
        {
            set; private get;
        }

        public bool game_UI_enabled
        {
            set
            {
                fire_button_element.SetActive(value);
                if (virtual_joystick_enabled)
                {
                    virtual_joystick_element.SetActive(value);
                }
                else
                {
                    accelerate_button_element.SetActive(value);
                }
            }
        }

        public GameObject fire_button_element
        {
            set; private get;
        }

        public string accelerate_button_name
        {
            set; private get;
        }

        public string fire_button_name
        {
            set; private get;
        }

        public AndroidInputManager ()
        {
            _virtual_joystick_backing_object = null;
            _player_wants_joystick_enabled   = true;
            // set default values
            virtual_joystick_enabled = false;

            // testing
            invert_pitch_controls = true;
            invert_roll_controls = false;
        }

        /// <summary>
        /// Touching the joystick accelerates the ship
        /// </summary>
        /// <returns></returns>
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
                      .GetButton(accelerate_button_name);
            }
        }

        /// <summary>
        /// When not accelerating, the ship automatically brakes.
        /// </summary>
        /// <returns></returns>
        public bool brakeInput()
        {
            return !accelerateInput();
        }

        public bool fireInput()
        {
            return CnControls
                  .CnInputManager
                  .GetButton(fire_button_name);
        }

        public bool shipSelectMenuOpenInput()
        {
            // TODO: Implement
            return false;
        }

        // Primary in-game menu
        public bool inGameMenuOpenInput()
        {
            return Input.GetKeyDown(KeyCode.Menu);
        }

        public bool exitNetGameInput()
        {
            // Escape corresponds to the back button on a phone
            return Input.GetKeyDown(KeyCode.Escape);
        }

        public float getRollInputValue()
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

        public float getPitchInputValue()
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