using UnityEngine;
using UnityEngine.UI;
using System;

namespace SpaceBattles
{
    public class ButtonExitGame : MonoBehaviour
    {
        void Start()
        {
            Button b = gameObject.GetComponent<Button>();
            b.onClick.AddListener(delegate () { Application.Quit(); });
        }
    }
}