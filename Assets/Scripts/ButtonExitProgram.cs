using UnityEngine;
using UnityEngine.UI;
using System;

namespace SpaceBattles
{
    public class ButtonExitProgram : MonoBehaviour
    {
        void Start()
        {
            Button b = gameObject.GetComponent<Button>();
            b.onClick.AddListener(exitApplication);
        }

        public void exitApplication ()
        {
            Application.Quit();
        }
    }
}