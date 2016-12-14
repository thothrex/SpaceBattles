using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SpaceBattles
{
    [DisallowMultipleComponent]
    public class PlayerIdentifier : NetworkBehaviour
    {
        /// <summary>
        /// The NetworkedPlayerController which 
        /// this identifier represents/protects
        /// </summary>
        public NetworkedPlayerController BackingController;

        [SyncVar]
        public NetworkInstanceId PlayerID;

        private static readonly string NPCNotPresentMessage
            = "NetworkedPlayerController component on "
            + "the object identified by controllerNetworkId";
        private static readonly string GameObjectNotPresentMessage
            = "GameObject identified by controllerNetworkId";
        private static readonly string NoGameObjectWarning
            = "PlayerIdentifier: This component is not "
            + "attached to a GameObject, "
            + "attempting to circumvent this problem";

        public static PlayerIdentifier
        CreateNew
            (NetworkedPlayerController controller)
        {
            MyContract.RequireArgumentNotNull(
                controller,
                "NetworkedPlayerController"
            );

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
                NewId.SetBackingController(controller);
                Debug.Log(
                    "No existing PlayerIdentifier for "
                    + NewId.ToString()
                    + "\nCreating new PlayerIdentifier with net id "
                    + NewId.netId
                );
            }
            return NewId;
        }

        public static PlayerIdentifier
        CreateNew
            (NetworkInstanceId controllerNetworkId)
        {
            MyContract.RequireArgumentNotNull(
                controllerNetworkId,
                "controllerNetworkId"
            );

            GameObject PlayerControllerHost
                = NetworkServer.FindLocalObject(controllerNetworkId);
            MyContract.RequireArgumentNotNull(
                PlayerControllerHost,
                GameObjectNotPresentMessage
            );

            NetworkedPlayerController PlayerController
                = PlayerControllerHost
                .GetComponent<NetworkedPlayerController>();
            MyContract.RequireArgumentNotNull(
                PlayerController,
                NPCNotPresentMessage
            );

            return CreateNew(PlayerController);
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
            if (PlayerID != null)
            {
                if (BackingController != null)
                {
                    return StandardIdentifier();
                }
                else // can't use netid yet
                {
                    // try to recover
                    GameObject NPCHost;

                    // The following section is disgusting,
                    // but I can't tell why this is happening.
                    // I only pull the value after
                    // the local player controller is spawned
                    // (in NetworkedPlayerController.OnStartServer)
                    // so there's no reason why the gameobject
                    // should be null/unready.
                    // 
                    // It looks (to me) like Unity's networking tries to
                    // instantiate this object with new rather than
                    // with AddComponent, so it becomes "headless".
                    // This is all undocumented behaviour,
                    // so I'm just going to try and avoid dealing with it.
                    try
                    {
                        NPCHost = gameObject;
                    }
                    catch (NullReferenceException nre)
                    {
                        Debug.LogWarning(NoGameObjectWarning);
                        NPCHost = NetworkServer.FindLocalObject(PlayerID);
                        if (NPCHost == null)
                        {
                            return BackupIndentifier();
                        }
                    }
                    NetworkedPlayerController NPC
                            = NPCHost
                            .GetComponent<NetworkedPlayerController>();
                    MyContract.RequireFieldNotNull(
                            NPC,
                            "NetworkedPlayerController attached to the "
                                + "local object corresponding to NetworkInstanceId "
                                + PlayerID.ToString()
                        );
                    BackingController = NPC;
                    return StandardIdentifier();
                }
            }
            else
            {
                return "Null" + PrintInitStatus();
            }
        }

        private string PrintInitStatus ()
        {
            return "("
                + (PlayerID == null
                  ? "No PlayerID"
                  : "PlayerID valid")
                + ", "
                + (BackingController == null
                  ? "No BackingController"
                  : "BackingController valid")
                + ")";
        }

        private string StandardIdentifier ()
        {
            return "Player " + BackingController.netId;
        }

        private string BackupIndentifier ()
        {
            return "Player " + PlayerID.ToString();
        }
    }
}
