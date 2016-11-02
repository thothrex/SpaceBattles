using System;
using UnityEngine;

namespace SpaceBattles
{
    public class TransitionPayloadHolder : MonoBehaviour
    {
        public ITransitionRequestTransmitter Transmitter;
        public UiElementTransitionType TransitionType;
        public UIElements Targets;

        public void TransmitStoredPayload ()
        {
            Transmitter.RequestTransition(TransitionType, Targets);
        }
    }
}

