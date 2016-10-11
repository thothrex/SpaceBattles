using System;
using UnityEngine;
using BreakpointHandler = SpaceBattles.ScreenSizeChangeLogic.ScreenBreakpointHandler;

namespace SpaceBattles
{
    /// <summary>
    /// One breakpoint as entered into an object's list of breakpoints
    /// </summary>
    [Serializable]
    public class BreakpointEntry
    {
        public Dimension dimension;
        public float breakpoint;
        public BreakpointHandler handler;

        public BreakpointEntry()
        {
            dimension = Dimension.WIDTH;
            breakpoint = 0.0f;
            handler = null;
        }

        override
        public string ToString()
        {
            return
                  "[Dimension: " + dimension.ToString()
                + ", Breakpoint: " + breakpoint.ToString()
                + ", Handler is " + (handler == null ? "not set" : "set")
                + "]";
        }
    }
}
