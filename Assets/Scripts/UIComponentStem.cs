using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBattles
{
    [DisallowMultipleComponent]
    public class
    UIComponentStem
        : MonoBehaviour,
          IGameObjectRegistryKeyComponent,
          ITransitionRequestTransmitter,
          ITransitionRequestBroadcaster
    {
        public UIElements ElementIdentifier;
        public List<ScreenBreakpointClient> BreakpointClients;
        public UiTransitionRequestEventModule
            TransitionRequester = new UiTransitionRequestEventModule();

        public event UiTransitionRequestHandler UiTransitionRequest;
        public event UiTransitionBacktrackHandler UiBacktrackRequest;

        public int Key
        {
            get
            {
                return (int)ElementIdentifier;
            }
        }

        public void Awake ()
        {
            TransitionRequester.UiTransitionRequest
                += UiTransitionRequestPropagator;
            TransitionRequester.UiBacktrackRequest
                += UiTransitionBacktrackPropagator;
        }

        public void RegisterBreakpoints(IScreenSizeBreakpointRegister register)
        {
            MyContract.RequireArgumentNotNull(register, "register");
            foreach (ScreenBreakpointClient Client in BreakpointClients)
            {
                // there will be a null entry to cover for
                // UnityEvents not accepting listeners added through code
                // unless they have at least one pre-code listener
                if (Client != null)
                {
                    Client.RegisterBreakpoints(register);
                }
            }
        }

        /// <summary>
        /// Mainly for editor registration
        /// </summary>
        /// <param name="transitionType"></param>
        /// <param name="targets"></param>
        public void
        RequestTransition
            (UiElementTransitionType transitionType,
             UIElements targets)
        {
            TransitionRequester.RequestTransition(
                transitionType,
                targets
            );
        }

        public void RequestBacktrack()
        {
            TransitionRequester.RequestBacktrack();
        }

        public void
        UiTransitionRequestPropagator
        (UiElementTransition  requestedTransition)
        {
            UiTransitionRequest(requestedTransition);
        }

        public void UiTransitionBacktrackPropagator ()
        {
            UiBacktrackRequest();
        }
    }
}
