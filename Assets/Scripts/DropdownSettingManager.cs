using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBattles
{
    public class DropdownSettingManager : MonoBehaviour
    {
        // -- Fields --
        public bool InitialToggleState;
        public List<EasyTween> ToggleTweens;
        public List<EasyTween> ToggleAntiTweens;
        public MyWaypointMover ToggleButtonMover;

        private bool CurrentToggleState;
        private bool DoneInit = false;
        private OptionalEventModule oem = new OptionalEventModule();

        // -- delegates --
        public delegate void ToggleSettingHandler(bool enabled);

        // -- Events --
        public event ToggleSettingHandler ToggleSet;

        // -- Methods --
        public void Toggle()
        {
            if (oem.shouldTriggerEvent(ToggleSet))
            {
                ToggleSet(!CurrentToggleState);
            }
            CurrentToggleState = !CurrentToggleState;
        }

        /// <summary>
        /// Sets both the state of all the graphics,
        /// and the simple bool state.
        /// </summary>
        /// <param name="on"></param>
        public void SetInitialToggleGraphicState(bool on)
        {
            MyContract.RequireFieldNotNull(ToggleButtonMover,
                                           "Toggle Button Mover");

            //Debug.Log(
            //      "Current toggle state: "
            //    + CurrentToggleState.ToString()
            //);
            //Debug.Log("Setting toggle state "
            //        + (on ? "on" : "off"));
            ensureInit();
            if (on != CurrentToggleState)
            {
                foreach (EasyTween et in ToggleTweens)
                {
                    et.OpenCloseObjectAnimation();
                }
                foreach (EasyTween at in ToggleAntiTweens)
                {
                    at.OpenCloseObjectAnimation();
                }
            }
            ToggleButtonMover.setMoveState(on);
            CurrentToggleState = on;
        }

        private void ensureInit()
        {
            if (!DoneInit)
            {
                //Debug.Log("initialising toggle state from editor values");
                CurrentToggleState = InitialToggleState;
                DoneInit = true;
            }
        }
    }
}

