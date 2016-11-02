using System;
using UnityEngine;

namespace SpaceBattles
{
    public interface ITransitionRequestBroadcaster
    {
        // -- Events --
        event UiTransitionRequestHandler UiTransitionRequest;
        event UiTransitionBacktrackHandler UiBacktrackRequest;

        // -- Methods -- 
        // We have these to prompt the developer to use a transition request
        // module rather than implementing the events directly
        void UiTransitionRequestPropagator
            (UiElementTransition requestedTransition);
        void UiTransitionBacktrackPropagator();
    }
}
