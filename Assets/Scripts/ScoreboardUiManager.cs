using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;

namespace SpaceBattles
{
    public class ScoreboardUiManager : MonoBehaviour
    {
        // -- Fields --
        public Transform UiElementListParent;
        public int NumberOfHeaderElementsInList;
        public List<ScoreUiElementManager> ScoreUiElements;
        public GameObject ScoreUiElementPrefab;
        public GameObject LocalPlayerScoreUiElementPrefab;

        [HideInInspector]
        public Dictionary<NetworkInstanceId, ScoreUiElementManager> ScoreElement
            = new Dictionary<NetworkInstanceId, ScoreUiElementManager>();

        // -- Properties --
        public NetworkInstanceId LocalPlayerId
        {
            set; private get;
        }

        // -- Methods --
        public void Start ()
        {
            MyContract.RequireFieldNotNull(ScoreUiElementPrefab,
                                           "ScoreUiElementPrefab");
        }

        public void AddNewPlayer (PlayerIdentifier player, int score)
        {
            MyContract.RequireArgumentNotNull(player, "player");
            MyContract.RequireField(
                !ScoreElement.ContainsKey(player.PlayerID),
                "does not already contain an entry for player "
                    + player.ToString(),
                "ScoreElement"
            );

            Debug.Log(
                "ScoreboardUIManager: Adding new element ["
                + player.ToString()
                + " -> "
                + score+ "]"
            );

            GameObject NewScoreUiElement;
            string DisplayName = "[Uninitialised]";
            if (player.PlayerID == LocalPlayerId)
            {
                NewScoreUiElement
                    = Instantiate(LocalPlayerScoreUiElementPrefab);
                DisplayName = player.ToString() + " (Me)";
            }
            else
            {
                NewScoreUiElement
                    = Instantiate(ScoreUiElementPrefab);
                DisplayName = player.ToString();
            }
            
            NewScoreUiElement.transform.SetParent(UiElementListParent, false);

            ScoreUiElementManager ElementManager
                = NewScoreUiElement.GetComponent<ScoreUiElementManager>();
            // This is programmer/compile-time error if it occurs
            // as in, this shouldn't be needed in production code
            MyContract.RequireFieldNotNull(ElementManager, "ScoreUiElementManager");
            ElementManager.Initialise(DisplayName, score);
            NewScoreUiElement.SetActive(true);

            ScoreUiElements.Add(ElementManager);
            SortScoreElements();
            ScoreElement.Add(player.PlayerID, ElementManager);
        }

        public void RemovePlayer (PlayerIdentifier player)
        {
            MyContract.RequireArgumentNotNull(player, "player");
            MyContract.RequireField(
                ScoreElement.ContainsKey(player.PlayerID),
                "Contains an entry for player "
                    + player.ToString(),
                "ScoreElement"
            );

            ScoreUiElementManager ElementManager = ScoreElement[player.PlayerID];

            ScoreElement.Remove(player.PlayerID);
            ScoreUiElements.Remove(ElementManager);
            // If needed this can be optimised by pooling the
            // score ui elements,
            // so when a player is removed they are deactivated,
            // then when a new player is added they try to reactivate
            // an old slot before instantiating a new one.
            Destroy(ElementManager.gameObject);
            //SortScoreElements(); should remain sorted
        }

        public void ChangePlayerScore (PlayerIdentifier player, int score)
        {
            MyContract.RequireArgumentNotNull(player, "player");
            if (ScoreElement.ContainsKey(player.PlayerID))
            {
                ScoreElement[player.PlayerID].UpdateScore(score);
                SortScoreElements();
            }
            else
            {
                Debug.Log(
                    "ScoreboardUIManager: Received a ChangePlayerScore "
                    + "request for an unregistered player ("
                    + player.ToString()
                    + ")"
                );
                AddNewPlayer(player, score);
            }
        }

        private void SortScoreElements ()
        {
            // This sort could probably be done quicker
            // given that the list is already fully sorted before
            // the new element is added
            // (i.e. one round of insertion sort, guaranteed to be O(n))
            // but I'm assuming the library implementation is fast enough
            // for it to not matter,
            // and it may catch edge cases that re-implementing a sort here
            // might miss.

            // Sort our internal list
            ScoreUiElements.Sort();

            // Sort the graphical elements (Unity UI elements)
            var Enumerator = ScoreUiElements.GetEnumerator();
            for (int Index = 0;
                 Index < ScoreUiElements.Count && Enumerator.MoveNext();
                 Index++)
            {
                Transform ElementTransform = Enumerator.Current.transform;
                ElementTransform
                    .SetSiblingIndex(Index + NumberOfHeaderElementsInList);
            }
        }

        private void ClearUiElements ()
        {
            foreach(ScoreUiElementManager ElementManager in ScoreUiElements)
            {
                Destroy(ElementManager.gameObject);
            }
            ScoreUiElements.Clear();
            ScoreElement.Clear();
        }
    }
}