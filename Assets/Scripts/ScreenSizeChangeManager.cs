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

        // I use CamelCase for public variables
        // because the Unity editor makes it look nicer

        /// <summary>
        /// By internal, I mean it works within a prefab/UI component.
        /// For example, within the settings menu.
        /// The "external", or "global" manager only uses the trigger
        /// as a signal to retrieve the actual pixel values from its UI camera.
        /// Conversely, the internal managers use the values directly provided
        /// to them by their assigned trigger.
        /// This, combined with the list of internal breakpoint clients,
        /// allow per-component breakpoints to be used in a more local way
        /// i.e. the action and reaction can be kept together
        /// to make things clearer.
        /// </summary>
        public bool IsInternal;
        /// <summary>
        /// Set by code, not the editor
        /// </summary>
        public Camera FixedUICamera;
        /// <summary>
        /// Rect is the new size
        /// </summary>
        public UnityEventRect ScreenResized;
        public List<ScreenBreakpointClient> BreakpointClients;

        private ScreenSizeChangeLogic logic = null;
        private bool init_done = false;

        // <max_trigger_value, per_object_triggers<trigger_value, handler>>
        /// <summary>
        /// "protected"
        /// </summary>
        override
        protected void Awake()
        {
            ensureLogicIsInstantiated();
        }

        /// <summary>
        /// "protected"
        /// </summary>
        override
        protected void Start()
        {
            if (!init_done)
            {
                foreach (ScreenBreakpointClient client in BreakpointClients)
                {
                    client.RegisterBreakpoints(this);
                }
                init_done = true;
            }
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
            &&  !IsInternal
            &&  FixedUICamera.isActiveAndEnabled)
            {
                Rect rectangle = FixedUICamera.pixelRect;
                Debug.Log("Triggering screen size change with camera rect: "
                        + rectangle.ToString());
                ScreenResized.Invoke(rectangle);
            }
        }

        /// <summary>
        /// Overload to be used for internal breakpoints.
        /// </summary>
        /// <param name="new_size"></param>
        public void OnScreenSizeChange(Rect new_size)
        {
            if (this.isActiveAndEnabled
            &&  IsInternal)
            {
                Debug.Log("Triggering screen size change with changed rect: "
                        + new_size.ToString());
                ScreenResized.Invoke(new_size);
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
            ensureLogicIsInstantiated();
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
            ensureLogicIsInstantiated();
            logic.registerHeightBreakpointHandlers(object_breakpoints, registrant);
        }

        private void ensureLogicIsInstantiated ()
        {
            if (logic == null)
            {
                logic = new ScreenSizeChangeLogic();
                ScreenResized.AddListener(logic.screenSizeChangeHandler);
            }
        }

        [Serializable]
        public class UnityEventRect : UnityEvent<Rect> { };
    }
}
