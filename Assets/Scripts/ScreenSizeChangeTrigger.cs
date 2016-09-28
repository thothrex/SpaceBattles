using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

namespace SpaceBattles
{
    /// <summary>
    /// Needs to be attached directly to a viewport
    /// (I'm not sure if I'm using that word correctly;
    /// it needs to be attached to the rect which fills
    /// the whole screen).
    /// 
    /// Also because UnityEvents are brittle pieces of trash
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
    public class ScreenSizeChangeTrigger : UIBehaviour, IScreenSizeTrigger
    {
        private const string NO_RECTTRANSFORM_EXC
            = "ScreenSizeChangeTrigger behaviour has been placed onto "
            + "an object without a RectTransform. This script cannot "
            + " function correctly as a result.";
        private const string LOGIC_NOT_READY_EXC
            = "This ScreenSizeChangeTrigger's ScreenSizeChangeLogic module "
            + "has not been initialised yet.";
        private const string INIT_RECTTRANSFORM_EXC
            = "The recttransform has not been correctly initialised yet.";

        /// <summary>
        /// Rect is the new size
        /// </summary>
        public UnityEventRect ScreenResized;

        private RectTransform host_rect;
        private ScreenSizeChangeLogic logic;
        // <max_trigger_value, per_object_triggers<trigger_value, handler>>
        /// <summary>
        /// "protected"
        /// </summary>
        override
        protected void Awake()
        {
            host_rect = GetComponent<RectTransform>();
            if (host_rect == null)
            {
                throw new InvalidOperationException(NO_RECTTRANSFORM_EXC);
            }
            logic = new ScreenSizeChangeLogic();
            ScreenResized.AddListener(logic.screenSizeChangeHandler);
        }

        /// <summary>
        /// "protected"
        /// </summary>
        override
        protected void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            if (isActiveAndEnabled)
            {
                if (host_rect == null)
                {
                    throw new InvalidOperationException(INIT_RECTTRANSFORM_EXC);
                }
                Rect rectangle = host_rect.rect;
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
