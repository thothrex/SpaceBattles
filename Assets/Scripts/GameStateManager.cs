using System;
using UnityEngine;
using UnityEngine.Networking;

namespace SpaceBattles
{
    /// <summary>
    /// Preconditions:
    /// 1)
    ///     This should only exist on the multiplayer scene,
    ///     and there should only be one in any multiplayer scene.
    /// 2)
    ///     This must exist both client-side and server-side
    ///     (client-side is necessary so that the PIM can
    ///      register UI event listeners,
    ///      e.g. scoreupdate events)
    /// 
    /// Dependants:
    ///     NetworkedPlayerController.OnStartServer()
    ///     ProgramInstanceManager.EnterOnlineSceneConnectionComplete()
    /// </summary>
    public class GameStateManager : NetworkBehaviour
    {
        public Scoreboard Scoreboard;
        public SpaceShipClassManager SSClassManager;

        public static GameStateManager FindCurrentGameManager ()
        {
            GameObject SceneControllerHostObject
                = GameObject.Find("Game State Manager");
            MyContract.RequireFieldNotNull(
                SceneControllerHostObject,
                "Scene Controller Host Object"
            );
            GameStateManager ServerController
                = SceneControllerHostObject.GetComponent<GameStateManager>();
            MyContract.RequireFieldNotNull(ServerController, "Game State Manager");

            return ServerController;
        }

        /// <summary>
        /// Expected to be a server action - not sure if that's necessary or not
        /// </summary>
        /// <param name="playerController"></param>
        [Server]
        public void OnPlayerJoin (NetworkedPlayerController playerController)
        {
            MyContract.RequireArgumentNotNull(playerController, "playerController");
            PlayerIdentifier NewPlayerId = PlayerIdentifier.CreateNew(playerController);
            playerController.ShipDestroyed
                += delegate (PlayerIdentifier hunter)
                   {
                       Scoreboard.OnKillEvent(hunter, NewPlayerId);
                   };
            Scoreboard.ScoreUpdate += playerController.OnScoreUpdate;
            Scoreboard.RegisterNewPlayer(NewPlayerId);
            //Debug.Log("Game State Manager: Checking if we have a spaceship class manager");
            MyContract.RequireFieldNotNull(SSClassManager, "Spaceship Class Manager");
            //Debug.Log("Game State Manager: Initialising given NPC with our SSCManager");
            playerController.initialiseShipClassManager(SSClassManager);
        }

        [Server]
        public void InitialiseScoreListener (IScoreListener listener)
        {
            var Scores = Scoreboard.GetScoreList();
            foreach (var Score in Scores)
            {
                listener.OnScoreUpdate(Score.Key, Score.Value);
            }
        }
    }
}
