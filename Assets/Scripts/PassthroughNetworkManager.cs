using System;
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
    }
}
