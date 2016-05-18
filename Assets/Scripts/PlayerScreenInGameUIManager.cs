using System;
using UnityEngine;

namespace SpaceBattles
{
    public class PlayerScreenInGameUIManager : MonoBehaviour
    {
        public UIBarManager local_player_health_bar;

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

