using System;
using UnityEngine;

namespace SpaceBattles
{
    public class GameplayUIManager : MonoBehaviour
    {
        public GameObject local_player_health_bar_object;
        private UIBarManager local_player_health_bar;

        void Awake ()
        {
            local_player_health_bar
                = local_player_health_bar_object.GetComponent<UIBarManager>();
        }

        public void localPlayerSetMaxHealth (double value)
        {
            local_player_health_bar.setMaxValue(value);
        }

        public void localPlayerSetCurrentHealth (double value)
        {
            local_player_health_bar.setCurrentValue(value);
        }
    }
}

