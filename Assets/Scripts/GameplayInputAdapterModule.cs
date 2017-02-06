using System;
using UnityEngine;

namespace SpaceBattles
{
    public abstract class GameplayInputAdapterModule
    {
        // -- Fields --
        public bool VirtualJoystickEnabled      { set; get; }
        public bool InvertPitchControls         { set; private get; }
        public bool InvertRollControls          { set; private get; }
        public string AccelerateButtonName      { set; private get; }
        public string FireButtonName            { set; private get; }
        public string JoystickDepressedAxisName { set; private get; }

        protected static readonly string ScoreboardAxisName
            = "InGameScoreboard";
        protected static readonly string InGameMenuAxisName = "menu2";
        protected static readonly string ShipSelectAxisName = "menu1";
        protected static readonly string AccelerationAxisName
            = "Acceleration";
        protected static readonly string PitchAxisName = "Pitch";
        protected static readonly string RollAxisName = "Roll";

        private readonly float FullRotationAngle = 360.0f;

        // -- Methods --
        public abstract bool ShipSelectMenuOpenInput();
        public abstract bool InGameMenuOpenInput();
        public abstract bool ExitNetGameInput();
        public abstract bool InGameScoreboardOpenInput();
        public abstract bool InGameScoreboardCloseInput();

        public GameplayInputAdapterModule ()
        {
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
                      .GetAxis(AccelerationAxisName) > 0;
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

        public bool FireInput ()
        {
            return CnControls
                  .CnInputManager
                  .GetButton(FireButtonName);
        }

        public float ReadRollInputValue ()
        {
            // accel for accelerometer,
            // GetAxis for keyboard and virtual joystick
            float NewRoll
                    = -Input.acceleration.x * 0.5f
                    + (-CnControls.CnInputManager.GetAxis(RollAxisName))
                    ;
            if (InvertRollControls)
            {
                NewRoll *= -1;
            }

            return NewRoll;
        }

        public float ReadPitchInputValue ()
        {
            // accel for accelerometer,
            // GetAxis for keyboard and virtual joystick
            float NewPitch
                    = Input.acceleration.z * 0.5f
                    + (-CnControls.CnInputManager.GetAxis(PitchAxisName))
                    ;
            if (InvertPitchControls)
            {
                NewPitch *= -1;
            }
            return NewPitch;
        }
    }
}

