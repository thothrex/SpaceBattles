using System;
using UnityEngine.Networking;

namespace SpaceBattles
{
    public interface IScoreListener
    {
        void OnScoreUpdate (PlayerIdentifier playerId, int newScore);
    }
}
