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
        protected static readonly string OrreryOrbitStartRotationButtonName
            = "StartOrreryRotation";
        protected static readonly string OrreryOrbitRotationXAxisName
            = "OrreryXRotation";
        protected static readonly string OrreryOrbitRotationYAxisName
            = "OrreryYRotation";

        private readonly float FullRotationAngle = 360.0f;
        private bool InMouseDrag = false;
        private Vector2 LastMouseReading = new Vector2(0,0);
        private Vector2 CurrentOrreryEulerRotationOffest = new Vector2(0, 0);

        

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

        /// <summary>
        /// Should be called every frame,
        /// but I'll try to cope for when it isn't
        /// </summary>
        /// <returns>
        /// The current euler angle rotation of the orbiting camera
        /// about the target.
        /// This is the sum of all the relevant drags
        /// </returns>
        public Vector2 ReadOrreryCameraOffsetEulerRotationValue()
        {
            // Check if we're entering a mouse drag
            if (!InMouseDrag
            && CnControls
              .CnInputManager
              .GetButton(OrreryOrbitStartRotationButtonName))
            {
                InMouseDrag = true;
                LastMouseReading.x
                    = CnControls
                    .CnInputManager
                    .GetAxis(OrreryOrbitRotationXAxisName);
                //Debug.Log("Starting mouse drag");
            }
            // Check if we're exiting a mouse drag
            else if (InMouseDrag
                 && !CnControls
                    .CnInputManager
                    .GetButton(OrreryOrbitStartRotationButtonName))
            {
                InMouseDrag = false;
                //Debug.Log("Ending mouse drag");
            }

            // Read drag values
            if (InMouseDrag)
            {
                Vector2 MouseDelta = new Vector2();

                // This reading is the DELTA of the mouse position,
                // not the direct mouse position
                MouseDelta.x
                    = CnControls
                    .CnInputManager
                    .GetAxis(OrreryOrbitRotationXAxisName);
                // TODO: Make y axis
                MouseDelta.y
                    = CnControls
                    .CnInputManager
                    .GetAxis(OrreryOrbitRotationYAxisName);

                //Debug.Log(
                //    "Reading x axis rotation as "
                //    + MouseDelta.x
                //);

                //Debug.Log(
                //    "Doing "
                //    + CurrentReadings.ToString()
                //    + " - "
                //    + LastMouseReading
                //);
                //Debug.Log(
                //    " + "
                //    + CurrentOrreryEulerRotationOffest.ToString()
                //);

                // We only want to measure the change in mouse x axis
                // within a drag motion
                CurrentOrreryEulerRotationOffest
                    += MouseDelta;

                // Prevent overflow by repeated dragging (hopefully)
                //CurrentOrreryEulerRotationOffest.x
                //    %= FullRotationAngle;
                //CurrentOrreryEulerRotationOffest.y
                //    %= FullRotationAngle;

                LastMouseReading = MouseDelta;
                //Debug.Log(
                //    "Returning mouse drag reading: "
                //    + CurrentOrreryEulerRotationOffest.ToString()
                //);
            }
            // Return whatever the last value was
            return CurrentOrreryEulerRotationOffest;
        }
    }
}

