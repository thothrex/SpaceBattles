﻿using System;
using UnityEngine;

namespace SpaceBattles
{
    public class PCInputManager : InputAdapterModule
    {
        public bool accelerateInput()
        {
            return Input.GetAxis("Acceleration") > 0;
        }

        public bool brakeInput()
        {
            return Input.GetAxis("Acceleration") == 0;
        }

        public bool fireInput()
        {
            return Input.GetButtonDown("Fire");
        }
        
        public bool shipSelectMenuOpenInput()
        {
            return Input.GetButtonDown("menu1");
        }

        // Primary in-game menu
        public bool inGameMenuOpenInput()
        {
            return Input.GetButtonDown("menu2");
        }

        /// <summary>
        /// No such button exists for the PC version.
        /// </summary>
        /// <returns>Always false</returns>
        public bool exitNetGameInput()
        {
            return false;
        }
    }
}

