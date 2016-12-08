using System;
using UnityEngine;
using UnityEngine.Networking;

namespace SpaceBattles
{
    public class PlayerIdentifier : NetworkBehaviour
    {
        /// <summary>
        /// The NetworkedPlayerController which 
        /// this identifier represents/protects
        /// </summary>
        public NetworkedPlayerController BackingController;

        public NetworkInstanceId PlayerID;

        public static PlayerIdentifier
        CreateNew
            (NetworkedPlayerController controller)
        {
            PlayerIdentifier ExtantId
                = controller
                .gameObject
                .GetComponent<PlayerIdentifier>();
            PlayerIdentifier NewId = ExtantId;
            if (ExtantId == null)
            {
                NewId = controller
                      .gameObject
                      .AddComponent<PlayerIdentifier>();
            }
            NewId.SetBackingController(controller);
            return NewId;
        }

        public void
        SetBackingController
            (NetworkedPlayerController controller)
        {
            MyContract.RequireArgumentNotNull(
                controller,
                "Networked Player Controller"
            );
            BackingController = controller;
            PlayerID = controller.netId;
        }

        /// <summary>
        /// This represents their "tag"
        /// which will be used in-game,
        /// rather than just a developer view
        /// </summary>
        /// <returns></returns>
        new
        public string ToString()
        {
            if (BackingController == null)
            {
                return "Null";
            }
            else
            {
                return "Player " + PlayerID.ToString();
            }
        }
    }
}
