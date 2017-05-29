using System;
using UnityEngine;

namespace SpaceBattles
{
    public class GameplayInputAdapterAndroid : GameplayInputAdapterModule
    {
        public GameplayInputAdapterAndroid ()
        {
            // set default values
            VirtualJoystickEnabled  = true;
            AccelerateButtonEnabled = false;
            FireButtonEnabled       = true;
            InvertPitchControls     = true;
            InvertRollControls      = false;
            FPSCounterEnabled       = false;
            NetworkTesterEnabled    = false;
        }

        override
        public bool ShipSelectMenuOpenInput()
        {
            // TODO: Implement
            return false;
        }

        // Primary in-game menu
        override
        public bool InGameMenuOpenInput()
        {
            return Input.GetKeyDown(KeyCode.Menu);
        }

        override
        public bool ExitNetGameInput()
        {
            // Escape corresponds to the back button on a phone
            return Input.GetKeyDown(KeyCode.Escape);
        }

        override
        public bool InGameScoreboardOpenInput()
        {
            // TODO: Implement properly.
            //       I think this needs a special button
            //       and/or a mobile-specific, drag-down top-bar
            //       for these kinds of input.
            return false;
            //throw new NotImplementedException();
        }

        override
        public bool InGameScoreboardCloseInput()
        {
            // TODO: Implement properly.
            //       I think this needs a special button
            //       and/or a mobile-specific, drag-down top-bar
            //       for these kinds of input.
            return false;
            //throw new NotImplementedException();
        }
    }
}