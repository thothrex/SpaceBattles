using System;
using UnityEngine;

namespace SpaceBattles
{
    public class GameplayInputAdapterAndroid : GameplayInputAdapterModule
    {
        public bool VirtualJoystickEnabled      { set; get; }
        public bool InvertPitchControls         { set; private get; }
        public bool InvertRollControls          { set; private get; }
        public string AccelerateButtonName      { set; private get; }
        public string FireButtonName            { set; private get; }
        public string JoystickDepressedAxisName { set; private get; }

        public GameplayInputAdapterAndroid ()
        {
            // set default values
            VirtualJoystickEnabled = false;
            JoystickDepressedAxisName = "JoystickDepressed";

            // testing
            InvertPitchControls = true;
            InvertRollControls = false;
        }

        /// <summary>
        /// Touching the joystick accelerates the ship
        /// </summary>
        /// <returns></returns>
        public bool AccelerateInput()
        {
            if (VirtualJoystickEnabled)
            {
                float JoystickDepressedValue
                    = CnControls
                     .CnInputManager
                     .GetAxis(JoystickDepressedAxisName);
                return JoystickDepressedValue > 0;
            }
            else
            {
                return CnControls
                      .CnInputManager
                      .GetButton(AccelerateButtonName);
            }
        }

        /// <summary>
        /// When not accelerating, the ship automatically brakes.
        /// </summary>
        /// <returns></returns>
        public bool BrakeInput()
        {
            return !AccelerateInput();
        }

        public bool FireInput()
        {
            return CnControls
                  .CnInputManager
                  .GetButton(FireButtonName);
        }

        public bool ShipSelectMenuOpenInput()
        {
            // TODO: Implement
            return false;
        }

        // Primary in-game menu
        public bool InGameMenuOpenInput()
        {
            return Input.GetKeyDown(KeyCode.Menu);
        }

        public bool ExitNetGameInput()
        {
            // Escape corresponds to the back button on a phone
            return Input.GetKeyDown(KeyCode.Escape);
        }

        public float ReadRollInputValue()
        {
            // accel for accelerometer,
            // GetAxis for keyboard and virtual joystick
            float new_roll
                    = -Input.acceleration.x * 0.5f
                    + (-CnControls.CnInputManager.GetAxis("Roll"))
                    ;
            if (InvertRollControls)
            {
                new_roll *= -1;
            }

            return new_roll;
        }

        public float ReadPitchInputValue()
        {
            // accel for accelerometer,
            // GetAxis for keyboard and virtual joystick
            float new_pitch
                    = Input.acceleration.z * 0.5f
                    + (-CnControls.CnInputManager.GetAxis("Pitch"))
                    ;
            if (InvertPitchControls)
            {
                new_pitch *= -1;
            }
            return new_pitch;
        }
    }
}