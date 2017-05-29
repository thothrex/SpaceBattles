using System;
using UnityEngine;

namespace SpaceBattles
{
    public class ScreenRotater : MonoBehaviour
    {
        public void RotateScreen ()
        {
            Debug.Log(
                "Current screen orientation: "
                + Screen.orientation.ToString()
            );
            if (Screen.orientation == ScreenOrientation.Portrait)
            {
                Screen.orientation = ScreenOrientation.LandscapeLeft;
            }
            else
            {
                Screen.orientation = ScreenOrientation.Portrait;
            }
            Debug.Log(
                "Setting screen orientation to "
                + Screen.orientation.ToString()
            );
        }
    }
}

