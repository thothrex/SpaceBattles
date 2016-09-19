using System;
using UnityEngine;

namespace SpaceBattles
{
    public interface InputAdapterModule
    {
        bool accelerateInput();
        bool brakeInput();
        bool fireInput();
        bool shipSelectMenuOpenInput();
        bool inGameMenuOpenInput();
        bool exitNetGameInput();
    }
}

