using System;
using UnityEngine;

namespace SpaceBattles
{
    public class GameplayInputAdapterPc : GameplayInputAdapterModule
    {
        public bool VirtualJoystickEnabled      { set; get; }
        public bool InvertPitchControls         { set; private get; }
        public bool InvertRollControls          { set; private get; }
        public string AccelerateButtonName      { set; private get; }
        public string FireButtonName            { set; private get; }
        public string JoystickDepressedAxisName { set; private get; }

        public GameplayInputAdapterPc ()
        {
            // set default values
            // testing
            VirtualJoystickEnabled = true;
            InvertPitchControls    = true;
            InvertRollControls     = false;
            JoystickDepressedAxisName = "JoystickDepressed";
        }

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
                      .GetAxis("Acceleration") > 0;
            }
        }

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
            return CnControls.CnInputManager.GetButtonDown("menu1");
        }

        // Primary in-game menu
        public bool InGameMenuOpenInput()
        {
            return CnControls.CnInputManager.GetButtonDown("menu2");
        }

        /// <summary>
        /// No such button exists for the PC version.
        /// </summary>
        /// <returns>Always false</returns>
        public bool ExitNetGameInput ()
        {
            return false;
        }

        public float ReadRollInputValue ()
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

        public float ReadPitchInputValue ()
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

