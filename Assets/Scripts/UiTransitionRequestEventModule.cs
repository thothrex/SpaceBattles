using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBattles
{
    public class UiTransitionRequestEventModule
    {
        // -- Delegates --
        public delegate void
            UiTransitionRequestHandler
                (UiElementTransition requestedTransition);
        public delegate void UiTransitionBacktrackHandler();

        // -- Events --
        public event UiTransitionRequestHandler UiTransitionRequest;
        public event UiTransitionBacktrackHandler UiBacktrackRequest;

        public void
        RequestTransition
            (UiElementTransitionType transitionType,
             UIElements targets)
        {
            UiElementTransition RequestedTransition
                = new UiElementTransition(
                    transitionType,
                    targets
                  );
            UiTransitionRequest(RequestedTransition);
        }

        public void RequestBacktrack ()
        {
            UiBacktrackRequest();
        }
    }
}

