using System;
using UnityEngine;

namespace SpaceBattles
{
    public interface InputAdapterModule
    {
        bool invert_pitch_controls { get; set; }
        bool invert_roll_controls { get; set; }
        bool accelerateInput();
        bool brakeInput();
        bool fireInput();
        bool shipSelectMenuOpenInput();
        bool inGameMenuOpenInput();
        bool exitNetGameInput();
    }
}

