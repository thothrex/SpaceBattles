using System;
using UnityEngine;

namespace SpaceBattles
{
    public interface InputAdapterModule
    {
        bool virtual_joystick_enabled { get; set; }
        bool invert_pitch_controls { set; }
        bool invert_roll_controls { set; }
        GameObject virtual_joystick_element { set; } // get is private
        bool accelerateInput();
        bool brakeInput();
        bool fireInput();
        bool shipSelectMenuOpenInput();
        bool inGameMenuOpenInput();
        bool exitNetGameInput();
        float getRollInputValue();
        float getPitchInputValue();
    }
}

