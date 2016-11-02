using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBattles
{
    [Serializable]
    [Flags]
    public enum UIElements
    {
        None = 0,
        MainMenu = 1,
        SettingsMenu = 2,
        ShipSelect = 4,
        GameplayUI = 8,
        VirtualJoystick = 16,
        InGameMenu = 32,
        AccelerateButton = 64,
        FireButton = 128,
        DebugOutput = 256,
        // FreeSlot = 512,
        OrreryUI = 1024
    }
}