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
        // -- Fields --
        public const string NO_NETBEHAVIOUR_ERRMSG
            = "player object does not have a network behaviour";
        /// <summary>
        /// Can't use Unity's built in player prefab because
        /// IT FUCKING OVERWRITES CUSTOM SPAWN HANDLERS
        /// </summary>
        public GameObject player_prefab;
        private OptionalEventModule oem;

        // -- Delegates --
        public delegate void LocalPlayerStartHandler (NetworkedPlayerController IPC);
        public delegate void PlayerDisconnectedHandler (PlayerIdentifier pID);

        // -- Events --
        public event LocalPlayerStartHandler    LocalPlayerStarted;
        /// <summary>
        /// Called when this client is disconnected from the server
        /// </summary>
        public event Action                     ClientDisconnected;
        /// <summary>
        /// Called when a player disconnects from this server
        /// </summary>
        public event PlayerDisconnectedHandler  PlayerDisconnected;

        // -- Properties --
        public bool FinishedLoading { get; private set; }

        // -- Methods --
        public void Awake ()
        {
            FinishedLoading = false;
        }

        public void Start ()
        {
            Debug.Log("player controller spawn handler registered");
            // Register player object spawn handler
            ClientScene.RegisterPrefab(player_prefab,
                                       playerControllerSpawnHandler,
                                       playerControllerDespawnHandler);
            oem = new OptionalEventModule();
            oem.AllowNoEventListeners = false;
            FinishedLoading = true;
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
            NetworkedPlayerController spawned_player_controller
                = player_obj.GetComponent<NetworkedPlayerController>();

            spawned_player_controller.LocalPlayerStarted += passthroughLocalPlayerStarted;
            spawned_player_controller.SetupComplete();

            return player_obj;
        }

        public void playerControllerDespawnHandler (GameObject player_controller_obj)
        {
            Destroy(player_controller_obj);
        }

        override
        public void OnClientDisconnect (NetworkConnection conn)
        {
            Debug.Log("OnClientDisconnect message received");
            if (Network.isServer)
            {
                Debug.Log("Local server connection disconnected");
            }
            else
            {
                Debug.Log("Successfully disconnected from the server");
            }

            if (oem.shouldTriggerEvent(ClientDisconnected))
            {
                ClientDisconnected();
            }
        }

        override
        public void OnClientError (NetworkConnection conn, int errorCode)
        {
            Debug.Log("Network error message received. Code: " + errorCode);
            if (oem.shouldTriggerEvent(ClientDisconnected))
            {
                ClientDisconnected();
            }
        }

        override
        public void OnServerDisconnect (NetworkConnection conn)
        {
            if (oem.shouldTriggerEvent(PlayerDisconnected))
            {
                PlayerDisconnected(PlayerIdentifier.CreateNew(conn));
            }
        }

        private void passthroughLocalPlayerStarted (NetworkedPlayerController IPC)
        {
            LocalPlayerStartHandler handler = LocalPlayerStarted;
            if (oem.shouldTriggerEvent(handler))
            {
                handler(IPC);
            }
        }
    }
}
