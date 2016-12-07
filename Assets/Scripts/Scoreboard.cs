using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace SpaceBattles
{
    public class Scoreboard : NetworkBehaviour
    {
        // -- Fields --
        private readonly int InitialScore = 0;
        // Should use the player's playercontrollerid as the key
        private Dictionary<NetworkInstanceId, int> PlayerScore
            = new Dictionary<NetworkInstanceId, int>();

        // -- Delegates
        public delegate void ScoreUpdateDelegate
            (PlayerIdentifier playerId, int newScore);

        // -- Events
        [SyncEvent]
        public event ScoreUpdateDelegate EventScoreUpdate;

        // -- Methods --
        [Server]
        public void RegisterNewPlayer(PlayerIdentifier newPlayerId)
        {
            MyContract.RequireArgumentNotNull(newPlayerId, "newPlayerId");
            MyContract.RequireField(
                !PlayerScore.ContainsKey(newPlayerId.PlayerID),
                "PlayerScore does not contain the new player ID "
                    + newPlayerId.ToString(),
                "newPlayerId " + newPlayerId.ToString() 
            );
            PlayerScore.Add(newPlayerId.PlayerID, InitialScore);
            Debug.Log("Scoreboard: Added player id "
                    + newPlayerId.ToString()
                    + " to the scoreboard.");
        }

        [Server]
        public void
        OnKillEvent
            (PlayerIdentifier hunter,
             PlayerIdentifier hunted)
        {
            MyContract.RequireField(PlayerScore.ContainsKey(hunter.PlayerID),
                                    "contains an entry for netid " + hunter.ToString(),
                                    "PlayerScore");
            PlayerScore[hunter.PlayerID] += 1;
            EventScoreUpdate.Invoke(hunter, PlayerScore[hunter.PlayerID]);
        }
    }
}
