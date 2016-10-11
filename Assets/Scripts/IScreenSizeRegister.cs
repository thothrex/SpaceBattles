using System;
using System.Collections.Generic;

//typedef
using BreakpointHandler = SpaceBattles.ScreenSizeChangeLogic.ScreenBreakpointHandler;

namespace SpaceBattles
{
    public interface IScreenSizeRegister
    {
        void registerWidthBreakpointHandlers
            (SortedList<float, BreakpointHandler> object_breakpoints, 
             object registrant);
        void registerHeightBreakpointHandlers
            (SortedList<float, BreakpointHandler> object_breakpoints,
             object registrant);
    }
}