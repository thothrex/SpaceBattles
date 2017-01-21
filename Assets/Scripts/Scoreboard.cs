using System;
using System.Collections.Generic;
using System.Linq;
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
        private OptionalEventModule oem = new OptionalEventModule();

        // -- Delegates
        public delegate void ScoreUpdateDelegate
            (PlayerIdentifier playerId, int newScore);
        public delegate void PlayerRemovedDelegate
            (PlayerIdentifier playerId);

        // -- Events
        public event ScoreUpdateDelegate ScoreUpdate;
        public event PlayerRemovedDelegate PlayerRemoved;

        // -- Methods --
        override
        public void OnStartServer ()
        {
            oem.AllowNoEventListeners = true;
            oem.SuppressErrorMessages = false;
        }


        [Server]
        public void RegisterNewPlayer (PlayerIdentifier newPlayerId)
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
            Debug.Log(PrintPlayerScore());
            if (oem.shouldTriggerEvent(ScoreUpdate))
                { ScoreUpdate(newPlayerId, InitialScore); }
        }

        [Server]
        public void RemovePlayer (PlayerIdentifier playerId)
        {
            MyContract.RequireArgumentNotNull(playerId, "newPlayerId");
            MyContract.RequireField(
                PlayerScore.ContainsKey(playerId.PlayerID),
                "PlayerScore contains the player ID "
                    + playerId.ToString(),
                "playerId " + playerId.ToString()
            );
            PlayerScore.Remove(playerId.PlayerID);
            Debug.Log("Scoreboard: Removed player id "
                    + playerId.ToString()
                    + " from the scoreboard.");
            Debug.Log(PrintPlayerScore());
            if (oem.shouldTriggerEvent(PlayerRemoved))
                { PlayerRemoved(playerId); }
        }

        [Server]
        public void
        OnKillEvent
            (PlayerIdentifier hunter,
             PlayerIdentifier hunted)
        {
            Debug.Log(PrintPlayerScore());
            MyContract.RequireField(PlayerScore.ContainsKey(hunter.PlayerID),
                                    "contains an entry for netid " + hunter.ToString(),
                                    "PlayerScore");
            PlayerScore[hunter.PlayerID] += 1;
            ScoreUpdate(hunter, PlayerScore[hunter.PlayerID]);
        }

        [Server]
        public List<KeyValuePair<PlayerIdentifier, int>>
        GetScoreList ()
        {
            return PlayerScore
                .Select(
                    kvp => 
                    new KeyValuePair<PlayerIdentifier, int>
                        (PlayerIdentifier.CreateNew(kvp.Key),
                         kvp.Value)
                )
                .ToList();
        }

        private string PrintPlayerScore ()
        {
            string returnstring = "(";
            foreach (var ScoreEntry in PlayerScore)
            {
                returnstring
                    += "["
                    + ScoreEntry.Key.ToString()
                    + " -> "
                    + ScoreEntry.Value
                    + "],";
            }
            returnstring += ")";
            return returnstring;
        }
    }
}
