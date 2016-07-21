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
        /*
        public delegate void
        ClientConnectedEventHandler(NetworkConnection conn, GameObject player_obj);
        [SyncEvent]
        public event ClientConnectedEventHandler ClientConnected;

        override
        public void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            // default implementation
            // need to pull out the player object
            // because for some bullshit reason
            // it's not accessible any other way
            // derspite being critical to basic game function
            GameObject player = (GameObject)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

            // Fire event
            ClientConnected(conn, player);
        }
        */


    }
}
