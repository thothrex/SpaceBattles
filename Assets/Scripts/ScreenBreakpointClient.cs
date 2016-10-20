using System;
using System.Collections.Generic;
using UnityEngine;
using BreakpointHandler = SpaceBattles.ScreenSizeChangeLogic.ScreenBreakpointHandler;

/// Consider doing deregistration of the client object
/// when either it or this object is deconstructed
/// (I don't know exactly how to do the former correctly, yet)

namespace SpaceBattles
{
    [Serializable]
    public class ScreenBreakpointClient : MonoBehaviour
    {
        private const string BREAKPOINT_LIST_UNINITIALISED_EXC
            = "This ScreenBreakpointClient's list of breakpoints "
            + "has not been initialised yet (which is done in its Start())";

        // I use this camel case on the public variables
        // so that the unity editor formats them more nicely
        public MonoBehaviour ListeningObject;
        [SerializeField]
        public List<BreakpointEntry> BreakpointEntries;

        private FloatInverseOrderAllowDuplicatesComparer inverse_comparer
            = new FloatInverseOrderAllowDuplicatesComparer();
        private bool breakpoint_handlers_rebuilt = false;

        public void Start()
        {
            if (BreakpointEntries == null)
            {
                BreakpointEntries = new List<BreakpointEntry>();
            }
            else
            {
                ensureBreakpointHandlersHaveBeenRebuilt();
            }
        }

        public void RegisterBreakpoints (IScreenSizeRegister register)
        {
            MyContract.RequireField(BreakpointEntries != null,
                                    "is not null", "BreakpointEntries");
            ensureBreakpointHandlersHaveBeenRebuilt();

            SortedList<float, BreakpointHandler> height_breakpoints
                = new SortedList<float, BreakpointHandler>(inverse_comparer);
            SortedList<float, BreakpointHandler> width_breakpoints
                = new SortedList<float, BreakpointHandler>(inverse_comparer);
            foreach (BreakpointEntry breakpoint in BreakpointEntries)
            {
                switch (breakpoint.dimension)
                {
                    case Dimension.HEIGHT:
                        height_breakpoints.Add(breakpoint.breakpoint,
                                               breakpoint.handler);
                        break;
                    case Dimension.WIDTH:
                        width_breakpoints.Add(breakpoint.breakpoint,
                                              breakpoint.handler);
                        break;
                    default:
                        throw new UnexpectedEnumValueException<Dimension>(
                            breakpoint.dimension
                         );
                }
            }
            if (height_breakpoints.Count > 0)
            {
                register.registerHeightBreakpointHandlers(height_breakpoints,
                                                      this);
            }
            if (width_breakpoints.Count > 0)
            {
                register.registerWidthBreakpointHandlers(width_breakpoints,
                                                     this);
            }
        }

        /// <summary>
        /// Need to do this when deserializing
        /// i.e. loading from prefab/saved values
        /// </summary>
        public void ensureBreakpointHandlersHaveBeenRebuilt ()
        {
            if (!breakpoint_handlers_rebuilt)
            {
                foreach (BreakpointEntry breakpoint in BreakpointEntries)
                {
                    breakpoint.rebuildHandlerFromFunctionName();
                }
                breakpoint_handlers_rebuilt = true;
            }
        }
    }
}