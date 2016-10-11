using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBattles
{
    public class UIComponentStem : MonoBehaviour
    {
        public UIElement ElementIdentifier;
        public List<ScreenBreakpointClient> BreakpointClients;

        public void doBreakpointRegistrations(IScreenSizeRegister register)
        {
            MyContract.RequireArgumentNotNull(register, "register");

            foreach (ScreenBreakpointClient client in BreakpointClients)
            {
                // there will be a null entry to cover for
                // UnityEvents not accepting listeners added through code
                // unless they have at least one pre-code listener
                if (client != null)
                {
                    client.doBreakpointRegistration(register);
                }
            }
        }
    }
}
