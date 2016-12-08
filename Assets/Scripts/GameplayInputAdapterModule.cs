using System;
using UnityEngine;

namespace SpaceBattles
{
    public interface GameplayInputAdapterModule
    {
        bool VirtualJoystickEnabled         { get; set; }
        bool InvertPitchControls            { set; }
        bool InvertRollControls             { set; }
        string AccelerateButtonName         { set; }
        string FireButtonName               { set; }
        string JoystickDepressedAxisName    { set; }

        bool AccelerateInput();
        bool BrakeInput();
        bool FireInput();
        bool ShipSelectMenuOpenInput();
        bool InGameMenuOpenInput();
        bool ExitNetGameInput();
        bool InGameScoreboardOpenInput();
        bool InGameScoreboardCloseInput();
        float ReadRollInputValue();
        float ReadPitchInputValue();
    }
}

