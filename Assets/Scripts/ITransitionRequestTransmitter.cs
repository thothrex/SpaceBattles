using System;
using UnityEngine;

namespace SpaceBattles
{
    public interface ITransitionRequestTransmitter
    {
        void
        RequestTransition
            (UiElementTransitionType transitionType,
             UIElements targets);
        void RequestBacktrack();
    }
}
