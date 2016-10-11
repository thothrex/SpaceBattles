using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

namespace SpaceBattles
{
    /// <summary>
    /// Because UnityEvents are brittle pieces of trash
    /// the UnityEvent in the Editor needs to have at least
    /// one listener attached to it
    /// or else one cannot add any listeners in code.
    /// 
    /// If we want this event to be constantly linked up,
    /// then this behaviour should also be placed on an object
    /// which is preserved between scenes,
    /// otherwise one would need to link the listening behaviour
    /// on a per-scene basis.
    /// </summary>
    public class ScreenSizeChangeManager : UIBehaviour, IScreenSizeRegister
    {
        private const string LOGIC_NOT_READY_EXC
            = "This ScreenSizeChangeTrigger's ScreenSizeChangeLogic module "
            + "has not been initialised yet.";

        /// <summary>
        /// Rect is the new size
        /// </summary>
        public UnityEventRect ScreenResized;
        // I use this stupid notation because the Unity editor makes it look nicer
        /// <summary>
        /// Set by code, not the editor
        /// </summary>
        public Camera FixedUICamera;
        
        private ScreenSizeChangeLogic logic;

        // <max_trigger_value, per_object_triggers<trigger_value, handler>>
        /// <summary>
        /// "protected"
        /// </summary>
        override
        protected void Awake()
        {
            logic = new ScreenSizeChangeLogic();
        }

        /// <summary>
        /// Gets the rect from the provided fixed UI camera
        /// to propagate the viewport size to listeners.
        /// 
        /// This will be set up to listen to the appropriate
        /// ScreenSizeChangeTrigger
        /// </summary>
        public void OnScreenSizeChange()
        {
            if (this.isActiveAndEnabled
            &&  FixedUICamera.isActiveAndEnabled)
            {
                Rect rectangle = FixedUICamera.pixelRect;
                ScreenResized.Invoke(rectangle);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="object_breakpoints">
        /// This needs to be sorted using the FloatInverseOrderComparer
        /// </param>
        /// <param name="registrant">
        /// The object registering these breakpoints.
        /// Needed to ensure deduplication of triggers.
        /// </param>
        public void registerWidthBreakpointHandlers
            (SortedList<float, ScreenSizeChangeLogic.ScreenBreakpointHandler> object_breakpoints,
             object registrant)
        {
            if (logic == null)
            {
                throw new InvalidOperationException(LOGIC_NOT_READY_EXC);
            }
            logic.registerWidthBreakpointHandlers(object_breakpoints, registrant);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="object_breakpoints">
        /// This needs to be sorted using the FloatInverseOrderComparer
        /// </param>
        /// <param name="registrant">
        /// The object registering these breakpoints.
        /// Needed to ensure deduplication of triggers.
        /// </param>
        public void registerHeightBreakpointHandlers
            (SortedList<float, ScreenSizeChangeLogic.ScreenBreakpointHandler> object_breakpoints,
             object registrant)
        {
            if (logic == null)
            {
                throw new InvalidOperationException(LOGIC_NOT_READY_EXC);
            }
            logic.registerHeightBreakpointHandlers(object_breakpoints, registrant);
        }

        [Serializable]
        public class UnityEventRect : UnityEvent<Rect> { };
    }
}
