using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace SpaceBattles
{
    /// <summary>
    /// This class acts as an adapter.
    /// Unity's networking hooks (OnClientStart, OnServerStart, etc.) should be 
    /// redirected by this class to more appropriate classes,
    /// e.g. ClientManager and ServerManager.
    /// This is both for clarity and decoupling from
    /// a specific networking implementation.
    /// 
    /// These more specific classes should probably be sub-classes of NetworkClient
    /// and NetworkServer (both Unity.Networking classes)
    /// </summary>
    public class PassthroughNetworkManager : NetworkManager
    {
        public const string NO_NETBEHAVIOUR_ERRMSG
            = "player object does not have a network behaviour";
        /// <summary>
        /// Can't use Unity's built in player prefab because
        /// IT FUCKING OVERWRITES CUSTOM SPAWN HANDLERS
        /// </summary>
        public GameObject player_prefab;

        // Delegates
        public delegate void LocalPlayerStartHandler(IncorporealPlayerController IPC);

        // Events
        public event LocalPlayerStartHandler    LocalPlayerStarted;

        public void Start ()
        {
            Debug.Log("player controller spawn handler registered");
            // Register player object spawn handler
            ClientScene.RegisterPrefab(player_prefab,
                                       playerControllerSpawnHandler,
                                       playerControllerDespawnHandler);
        }

        override
        public void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            NetworkHash128 player_prefab_id =
                player_prefab.GetComponent<NetworkIdentity>().assetId;
            // This function is only called on the server.
            // playerControllerSpawnHandler is already called on all true clients
            // (via the prefab handler being registered in this class's Start())
            // so it just needs to also be called on the Host as well
            GameObject IPO = playerControllerSpawnHandler(Vector3.zero, player_prefab_id);
            NetworkServer.AddPlayerForConnection(conn, IPO, playerControllerId);
        }

        public GameObject playerControllerSpawnHandler(Vector3 position, NetworkHash128 assetId)
        {
            Debug.Log("player controller spawn handler called");
            GameObject player_obj
                = (GameObject)Instantiate(player_prefab,
                                          position,
                                          Quaternion.identity);
            IncorporealPlayerController spawned_player_controller
                = player_obj.GetComponent<IncorporealPlayerController>();

            spawned_player_controller.LocalPlayerStarted += passthroughLocalPlayerStarted;
            spawned_player_controller.SetupComplete();

            return player_obj;
        }

        public void playerControllerDespawnHandler(GameObject player_controller_obj)
        {
            Destroy(player_controller_obj);
        }

        private void passthroughLocalPlayerStarted (IncorporealPlayerController IPC)
        {
            LocalPlayerStarted(IPC);
        }
    }
}
