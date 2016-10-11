﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBattles
{
    [Serializable]
    [Flags]
    public enum UIElement
    {
        NONE = 0,
        MAIN_MENU = 1,
        SETTINGS_MENU = 2,
        SHIP_SELECT = 4,
        GAMEPLAY_UI = 8,
        VIRTUAL_JOYSTICK = 16,
        IN_GAME_MENU = 32,
        ACCELERATE_BUTTON = 64,
        FIRE_BUTTON = 128,
        DEBUG_OUTPUT = 256
    }
}