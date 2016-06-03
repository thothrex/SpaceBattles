using UnityEngine;
using UnityEngine.Networking;
using System.Collections;


namespace SpaceBattles
{
    public class PassthroughNetworkDiscovery : NetworkDiscovery
    {
        public delegate void ServerDetectedEventHandler(string fromAddress, string data);
        public event ServerDetectedEventHandler ServerDetected;

        public override void OnReceivedBroadcast(string fromAddress, string data)
        {
            ServerDetected(fromAddress, data);
        }
    }
}

