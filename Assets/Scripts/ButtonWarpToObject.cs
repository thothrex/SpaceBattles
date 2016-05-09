using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace SpaceBattles
{
    public class ButtonWarpToObject : MonoBehaviour
    {
        public PlayerShipController warp_client;
        public OrbitingBodyBackgroundGameObject warp_target;

        void Start()
        {
            Button b = gameObject.GetComponent<Button>();
            b.onClick.AddListener(delegate () { warp_to_object(); });
        }

        public void warp_to_object()
        {
            Debug.Log("warp button pressed (DOES NOTHING)");
        }
    }
}