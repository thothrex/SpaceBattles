using System;
using UnityEngine;

namespace SpaceBattles
{
    public class GameplayInputAdapterPc : GameplayInputAdapterModule
    {
        public GameplayInputAdapterPc ()
        {
            // set default values
            // testing
            VirtualJoystickEnabled = true;
            InvertPitchControls    = true;
            InvertRollControls     = false;
        }
        
        override
        public bool ShipSelectMenuOpenInput()
        {
            return CnControls
                  .CnInputManager
                  .GetButtonDown(ShipSelectAxisName);
        }

        // Primary in-game menu
        override
        public bool InGameMenuOpenInput()
        {
            return CnControls
                  .CnInputManager
                  .GetButtonDown(InGameMenuAxisName);
        }

        /// <summary>
        /// No such button exists for the PC version.
        /// </summary>
        /// <returns>Always false</returns>
        override
        public bool ExitNetGameInput ()
        {
            return false;
        }

        override
        public bool InGameScoreboardOpenInput()
        {
            return CnControls.CnInputManager.GetButtonDown(ScoreboardAxisName);
        }

        override
        public bool InGameScoreboardCloseInput()
        {
            return CnControls.CnInputManager.GetButtonUp(ScoreboardAxisName);
        }
    }
}

