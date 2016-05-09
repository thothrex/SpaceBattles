using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace SpaceBattles
{
    public class ButtonAccelerate : MonoBehaviour
    {
        public PlayerShipController target;
        public Vector3 button_direction;

        void Start()
        {
            Button b = gameObject.GetComponent<Button>();
            b.onClick.AddListener(delegate () { target.accelerate(button_direction); });
        }
    }
}
