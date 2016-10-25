using System;
using UnityEngine;

namespace SpaceBattles
{
    public interface InputAdapterModule
    {
        bool game_UI_enabled { set; }
        bool virtual_joystick_enabled { get; set; }
        bool invert_pitch_controls { set; }
        bool invert_roll_controls { set; }
        GameObject VirtualJoystickElement { set; }
        GameObject AccelerateButtonElement { set; }
        GameObject fire_button_element { set; }
        string AccelerateButtonName { set; }
        string fire_button_name { set; }
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

