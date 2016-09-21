using System;
using UnityEngine;

namespace SpaceBattles
{
    public class AndroidInputManager : InputAdapterModule
    {
        public bool invert_pitch_controls { get; set; }
        public bool invert_roll_controls { get; set; }

        public AndroidInputManager ()
        {
            // set default values

            // testing
            invert_pitch_controls = true;
            invert_roll_controls = false;
        }

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