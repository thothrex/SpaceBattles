using System;
using UnityEngine;

namespace SpaceBattles
{
    public class AndroidInputManager : InputAdapterModule
    {
        /// <summary>
        /// Touching the screen (anywhere) accelerates the ship
        /// </summary>
        /// <returns></returns>
        public bool accelerateInput()
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Releasing a touch from the screen (anywhere) brakes the ship
        /// </summary>
        /// <returns></returns>
        public bool brakeInput()
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Ended)
                {
                    return true;
                }
            }
            return false;
        }

        public bool fireInput()
        {
            throw new NotImplementedException();
        }

        public bool shipSelectMenuOpenInput()
        {
            throw new NotImplementedException();
        }

        // Primary in-game menu
        public bool inGameMenuOpenInput()
        {
            return Input.GetKeyDown(KeyCode.Menu);
        }

        public bool exitNetGameInput()
        {
            // Escape corresponds to the back button on a phone
            return Input.GetKeyDown(KeyCode.Escape);
        }
    }
}