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
    public class ScreenSizeChangeTrigger : UIBehaviour
    {
        private const string NO_RECTTRANSFORM_EXC
            = "ScreenSizeChangeTrigger behaviour has been placed onto "
            + "an object without a RectTransform. This script cannot "
            + " function correctly as a result.";
        private const string INIT_RECTTRANSFORM_EXC
            = "The recttransform has not been correctly initialised yet.";

        /// <summary>
        /// Rect is the new size
        /// </summary>
        public UnityEventRect ScreenResizedInternal;
        public UnityEvent ScreenResized;

        private RectTransform host_rect;
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
        }

        /// <summary>
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
                ScreenResized.Invoke();
                ScreenResizedInternal.Invoke(host_rect.rect);
            }
            //Debug.Log("Trigger received a size change event");
        }

        [Serializable]
        public class UnityEventRect : UnityEvent<Rect> { };
    }
}
