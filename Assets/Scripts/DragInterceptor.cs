using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpaceBattles
{
    public class DragInterceptor : MonoBehaviour, IPointerDownHandler
    {
        // -- Fields --
        private static readonly string OrreryOrbitStartRotationButtonName
            = "StartOrreryRotation";
        private static readonly string OrreryOrbitRotationXAxisName
            = "OrreryXRotation";
        private static readonly string OrreryOrbitRotationYAxisName
            = "OrreryYRotation";

        private Vector2 LastMouseReading = new Vector2(0, 0);
        private Vector2 CurrentOrreryEulerRotationOffest = new Vector2(0, 0);
        private bool InMouseDrag = false;

        // -- Events --
        public MyVector2Event MouseDrag;

        // -- Methods --
        public void Update()
        {
            if (InMouseDrag)
            {
                MouseDrag.Invoke(ReadOrreryCameraOffsetEulerRotationValue());
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // TODO: Check what button was pressed
            //       / use the eventData for more things
            //Debug.Log("Starting Mouse Drag");
            InMouseDrag = true;
            LastMouseReading.x
                = CnControls
                .CnInputManager
                .GetAxis(OrreryOrbitRotationXAxisName);
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
            //if (!InMouseDrag
            //&&  CnControls
            //    .CnInputManager
            //    .GetButton(OrreryOrbitStartRotationButtonName))
            //{
            //    InMouseDrag = true;
            //    LastMouseReading.x
            //        = CnControls
            //        .CnInputManager
            //        .GetAxis(OrreryOrbitRotationXAxisName);
            //}
            // Check if we're exiting a mouse drag
            if (InMouseDrag
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
                MouseDelta.y
                    = CnControls
                    .CnInputManager
                    .GetAxis(OrreryOrbitRotationYAxisName);

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
            }
            // Return whatever the last value was
            return CurrentOrreryEulerRotationOffest;
        }
    }

}
