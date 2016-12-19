using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace SpaceBattles
{
    /// <summary>
    /// This class is the representation of the player on the server.
    /// Crucially, this means it should be the only source of [Command] and [ClientRPC] methods
    /// as it is the point where the network and the client interact.
    /// </summary>
    public class NetworkedPlayerController : NetworkBehaviour, IScoreListener
    {
        // -- Constants --
        private const string SHIP_CONTROLLER_NOT_SET_ERRMSG
            = "The ship controller has not been set yet.";
        private const string LOCAL_SHIP_SPAWN_NO_LISTENERS_ERRMSG
            = "There are no listeners for the local ship spawning event.";
        private const string LOCAL_PLAYER_SPAWN_NO_LISTENERS_ERRMSG
            = "There are no listeners for the local player controller spawning event.";
        private const string NONLOCAL_PLAYER_SPAWN_NO_LISTENERS_ERRMSG
            = "There are no listeners for the non-local player controller spawning event.";
        private static readonly string ShipAlreadySpawnedWarning
            = "Attempting to spawn a spaceship when one already exists";
        // -- Fields --

        // The following are set in the editor,
        // so should be left unassigned here
        public GameObject explosion_prefab;

        private readonly float RespawnDelay = 3.0f;
        private readonly float SpaceshipDestroyDelay = 0.5f;
        private readonly int RespawnRequestMaxAttempts = 3;
        private bool warping = false;
        private bool setup_complete = false;
        private bool CanRespawn = false;
        // Cannot be synced - use with caution
        private PlayerShipController ShipController = null;
        private OrbitingBodyBackgroundGameObject current_nearest_orbiting_body;
        // Only valid for the server
        private NetworkStartPosition[] spawnPoints;
        // Write-only variable (written by the pim from UI manager etc.)
        private SpaceShipClass CurrentShipClassChoice = SpaceShipClass.NONE;
        private SpaceShipClassManager SpaceshipClassManager = null;
        private OptionalEventModule oem = null;
        private GameStateManager ServerController = null;

        private System.Object SpaceshipSpawnLock = new System.Object();
        private System.Object RespawnRequestLock = new System.Object();

        // NB SyncVars ALWAYS sync from server->client,
        //    even for client-authoritative objects (such as this)
        [SyncVar]
        private GameObject CurrentSpaceship = null;
        [SyncVar]
        private bool ShipSpawned = false;

        // -- Delegates --
        public delegate void LocalPlayerStartHandler
            (NetworkedPlayerController IPC);
        public delegate void ShipSpawnedHandler
            (PlayerShipController player_ship_controller);
        public delegate void ShipHealthChangeHandler
            (double new_health);
        public delegate void ShipDestructionHandler
            (PlayerIdentifier killer);
        public delegate void ScoreboardResetHandler
            (List<KeyValuePair<PlayerIdentifier, int>> newScores);
        public delegate void ScoreUpdateHandler
            (PlayerIdentifier playerId, int newScore);

        // -- Events --
        public event LocalPlayerStartHandler    LocalPlayerStarted;
        public event ShipSpawnedHandler         LocalPlayerShipSpawned;
        public event ShipHealthChangeHandler    LocalPlayerShipHealthChanged;
        public event ShipDestructionHandler     ShipDestroyed;
        [SyncEvent] public event ScoreUpdateHandler         EventScoreUpdated;

        // -- Methods --

        /// <summary>
        /// Command which allows clients to instigate ship spawning
        /// (disable this?)
        /// 
        /// Needs to be called by the authoritative client.
        /// </summary>
        /// <param name="ss_type"></param>
        [Command]
        public void CmdSpawnStartingSpaceShip(SpaceShipClass ss_type)
        {
            if (!ShipSpawned)
            {
                SpawnSpaceShip(ss_type);
            }
        }

        /// <summary>
        /// Needs to be called by the authoritative client.
        /// </summary>
        /// <param name="newShipClass"></param>
        [Command]
        public void CmdRequestRespawn(SpaceShipClass newShipClass)
        {
            lock (RespawnRequestLock)
            {
                if (CanRespawn)
                {
                    SpawnSpaceShip(newShipClass);
                }
            }
        }

        [Command]
        public void
        CmdSendScoreboardStateToUI()
        {
            MyContract.RequireFieldNotNull(
                ServerController, "ServerControler"
            );

            ServerController.InitialiseScoreListener(this);
        }

        /// <summary>
        /// Authority isn't set yet so cannot check it
        /// </summary>
        override
        public void OnStartClient ()
        {
            initialiseShipSpawnStatus();
            setSpawnLocations(); // TODO: Why is this done here?
            oem = new OptionalEventModule();
            oem.AllowNoEventListeners = false;
        }

        /// <summary>
        /// I need some setup to be done before I want
        /// to trigger the event
        /// </summary>
        override
        public void OnStartAuthority ()
        {
            //Debug.Log("IPC authority started");
            if (setup_complete)
            {
                LocalPlayerStartHandler handler = LocalPlayerStarted;
                if (oem.shouldTriggerEvent(handler))
                {
                    handler(this);
                }
            }
        }

        /// <summary>
        /// Precondition: requires there to only be one GameStateManager
        /// </summary>
        override
        public void OnStartServer ()
        {
            ServerController
                = GameStateManager.FindCurrentGameManager();

            ServerController.OnPlayerJoin(this);
        }

        /// <summary>
        /// I only want to propagate this event
        /// if we have local authority
        /// </summary>
        public void SetupComplete ()
        {
            Debug.Log("IPC setup complete");
            setup_complete = true;
            if (hasAuthority)
            {
                LocalPlayerStartHandler handler = LocalPlayerStarted;
                if (oem.shouldTriggerEvent(handler))
                {
                    handler(this);
                }
            }
        }

        public void initialiseShipClassManager (SpaceShipClassManager ss_manager)
        {
            SpaceshipClassManager = ss_manager;
        }

        /// <summary>
        /// This happens on the client,
        /// so theoretically could be given directly
        /// to the PlayerShipController,
        /// but it is easier to passthrough so that
        /// this class deals with the relinking
        /// that has to occur after the player ship dies or respawns
        /// </summary>
        /// <param name="direction"></param>
        public void accelerateShip(Vector3 direction)
        {
            // prevent it from happening before intialisation
            if (ShipController != null)
            {
                ShipController.accelerate(direction);
            }
        }

        /// <summary>
        /// This happens on the client,
        /// so theoretically could be given directly
        /// to the PlayerShipController,
        /// but it is easier to passthrough so that
        /// this class deals with the relinking
        /// that has to occur after the player ship dies or respawns
        /// </summary>
        public void brakeShip()
        {
            // prevent it from happening before intialisation
            if (ShipController != null)
            {
                ShipController.brake();
            }
        }

        /// <summary>
        /// This happens on the client,
        /// so theoretically could be given directly
        /// to the PlayerShipController,
        /// but it is easier to passthrough so that
        /// this class deals with the relinking
        /// that has to occur after the player ship dies or respawns
        /// </summary>
        public void firePrimaryWeapon()
        {
            // prevent it from happening before intialisation
            if (ShipController != null)
            {
                ShipController.CmdFirePhaser();
            }
        }

        /// <summary>
        /// Should be called when connecting to the server.
        /// This checks whether or not players have already spawned their ships,
        /// and if so it triggers the ship spawn event on this client
        /// to ensure that the program status is correct.
        /// 
        /// N.B. Might break if the current spaceship is dead
        /// (but had been spawned).
        /// I think it should be fine though, I'm just not sure.
        /// TODO: test this
        /// </summary>
        public void initialiseShipSpawnStatus ()
        {
            if (ShipSpawned)
            {
                MyContract.RequireFieldNotNull(
                    CurrentSpaceship,
                    "Current Spaceship"
                );
                playerShipSpawnedHandler(CurrentSpaceship);
            }
        }

        public void setCurrentShipChoice(SpaceShipClass new_choice)
        {
            CurrentShipClassChoice = new_choice;
        }

        public void setRoll(float new_roll)
        {
            if (ShipController != null)
            {
                ShipController.setRoll(new_roll);
            }
        }

        public void setPitch(float new_pitch)
        {
            if (ShipController != null)
            {
                ShipController.setPitch(new_pitch);
            }
        }

        public void OnScoreUpdate(PlayerIdentifier playerId, int newScore)
        {
            EventScoreUpdated(playerId, newScore);
        }

        /// <summary>
        /// Mainly filters the trigger so that the event only occurs on
        /// the authoritative client.
        /// 
        /// Minorly, propagates the ship controller change
        /// </summary>
        [ClientRpc]
        private void RpcPlayerShipSpawned (GameObject spawnedSpaceship)
        {
            MyContract.RequireArgumentNotNull(
                spawnedSpaceship,
                "Spawned Spaceship"
            );
            Debug.Log("NPC: received RPC to receive spaceship");
            playerShipSpawnedHandler(spawnedSpaceship);
            // This should be here, because if this ship controller
            // represents the local player, then we're guaranteed
            // to have witnessed all ship spawns directly through
            // this RPC; 
            // therefore there's no need to check in the generic
            // case of playerShipSpawnedHandler which is also triggered
            // by the game-joining routines.
            //
            // N.B. This assumes that a player always initialises
            // their own ship spawning after they join the game.
            if (hasAuthority)
            {
                if (ShipController == null)
                {
                    throw new InvalidOperationException(
                        SHIP_CONTROLLER_NOT_SET_ERRMSG
                    );
                }
                ShipSpawnedHandler handler = LocalPlayerShipSpawned;
                if (oem.shouldTriggerEvent(handler))
                {
                    handler(ShipController);
                }
            }
        }

        [Client]
        private void playerShipSpawnedHandler(GameObject spawnedSpaceship)
        {
            MyContract.RequireArgumentNotNull(
                spawnedSpaceship,
                "Spawned Spaceship"
            );
            Debug.Log("Player Ship spawn registered on this client");
            ShipController
                = spawnedSpaceship.GetComponent<PlayerShipController>();
            ShipController.EventDeath += PlayerBodyKilled;
            ShipController.EventHealthChanged += shipHealthChanged;
        }

        /// <summary>
        /// Helper function which spawns a ship.
        /// 
        /// Needs to be invoked via a [Command] from the authoritative client.
        /// </summary>
        /// <param name="spaceShipType"></param>
        [Server]
        private void SpawnSpaceShip(SpaceShipClass spaceShipType)
        {
            lock (SpaceshipSpawnLock)
            {
                if (ShipSpawned)
                {
                    Debug.LogWarning(ShipAlreadySpawnedWarning);
                    return;
                }
                MyContract.RequireArgument(
                    spaceShipType != SpaceShipClass.NONE,
                    "is not NONE",
                    "spaceShipType"
                );
                MyContract.RequireFieldNotNull(
                    SpaceshipClassManager,
                    "Spaceship Class Manager"
                );
                GameObject SpaceshipPrefab
                    = SpaceshipClassManager.getSpaceShipPrefab(spaceShipType);
                MyContract.RequireFieldNotNull(SpaceshipPrefab, "Spaceship Prefab");

                // Should not remain null unless Unity.Instantiate can return null
                GameObject ServerSpaceship = null;
                if (CurrentSpaceship != null
                && ShipController.getSpaceshipClass() == spaceShipType)
                {
                    // current_spaceship was just despawned, not destroyed,
                    // so it simply needs to be respawned
                    ServerSpaceship = CurrentSpaceship;
                    ServerSpaceship.SetActive(true);
                }
                else
                {
                    // Create the ship locally (local to the server)
                    // NB: the ship will be moved to an appropriate NetworkStartPosition
                    //     by the server so the location specified here is irrelevant
                    ServerSpaceship = (GameObject)Instantiate(
                        SpaceshipPrefab,
                        transform.TransformPoint(chooseSpawnLocation()),
                        transform.rotation);

                    ShipController = ServerSpaceship.GetComponent<PlayerShipController>();
                    ShipController.SetSpaceshipClass(spaceShipType);
                    ShipController.owner = PlayerIdentifier.CreateNew(this);
                    ShipController.EventDeath += ShipDestroyedServerAction;
                }
                MyContract.RequireFieldNotNull(
                    ServerSpaceship,
                    "Server Spaceship"
                );
                CanRespawn = false;
                ShipSpawned = true;

                // Spawn the ship on the clients
                NetworkServer.SpawnWithClientAuthority(
                    ServerSpaceship,
                    connectionToClient
                );
                CurrentSpaceship = ServerSpaceship; // Update [SyncVar]
                RpcPlayerShipSpawned(CurrentSpaceship);
            }
        }
        /// <summary>
        /// </summary>
        /// <param name="death_location">Ignored for this function</param>
        [Server]
        private void
        ShipDestroyedServerAction
            (PlayerIdentifier killer, Vector3 deathLocation)
        {
            Debug.Log("Ship destroyed - taking server action");
            ShipSpawned = false;
            ShipDestroyed.Invoke(killer);
            Vector3 respawn_location = chooseSpawnLocation();
            StartCoroutine(destroyShipWithDelayCoroutine());
            StartCoroutine(EnableRespawnAfterDelayCoroutine());
        }

        [Server]
        private IEnumerator destroyShipWithDelayCoroutine()
        {
            yield return new WaitForSeconds(SpaceshipDestroyDelay);
            Debug.Log("Unspawning spaceship");
            CurrentSpaceship.SetActive(false);
            NetworkServer.UnSpawn(CurrentSpaceship);
        }

        [Server]
        private IEnumerator EnableRespawnAfterDelayCoroutine()
        {
            yield return new WaitForSeconds(RespawnDelay);
            Debug.Log("Enabling spacehsip respawn");
            CanRespawn = true;
        }

        /// <summary>
        /// NOT ACTUALLY USED FOR ANYTHING MEANINGFUL YET
        /// CAN BE DELETED
        /// 
        /// Sample code mostly copied from:
        /// https://unity3d.com/learn/tutorials/topics/multiplayer-networking/spawning-and-respawning
        /// 
        /// Only used on the server side
        /// (as spawnPoints is only set server-side)
        /// </summary>
        /// <returns></returns>
        [Server]
        private Vector3 chooseSpawnLocation()
        {
            // Set the spawn point to origin as a default value
            Vector3 spawnPoint = Vector3.zero;

            // If there is a spawn point array and the array is not empty, pick one at random
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                int random_spawn = UnityEngine.Random.Range(0, spawnPoints.Length);
                spawnPoint = spawnPoints[random_spawn].transform.position;
            }

            return spawnPoint;
        }

        private void setSpawnLocations()
        {
            spawnPoints = FindObjectsOfType<NetworkStartPosition>();
        }

        /// <summary>
        /// Event handler from an event propagated upward from whatever the current
        /// "body" of the player is (e.g. space ship).
        /// 
        /// Checks for authority, so it is okay to trigger this on non-authoritative clients.
        /// 
        /// Probably should have a different one for other objects owned by this player.
        /// </summary>
        /// <param name="deathLocation">
        /// Used to set camera transforms during respawn period
        /// </param>
        [Client]
        private void PlayerBodyKilled(PlayerIdentifier killer, Vector3 deathLocation)
        {
            AnyShipDestroyedAction();

            if (hasAuthority)
            {
                LocalShipDestroyedAction(deathLocation);
            }
        }

        [Client]
        private void AnyShipDestroyedAction()
        {
            Debug.Log("A player is dead!");

            // Create the explosion locally
            GameObject explosion = (GameObject)Instantiate(
                 explosion_prefab,
                 CurrentSpaceship.transform.position,
                 CurrentSpaceship.transform.rotation);

            // The spaceship gameObject is destroyed by the server
            // as server is still technically the source
            // of client-authoritative objects (slightly confusingly)

            // Set self-destruct timer
            Destroy(explosion, 2.0f);
        }

        [Client]
        private void LocalShipDestroyedAction(Vector3 deathLocation)
        {
            Debug.Log("Our player is dead!");
            this.transform.position = deathLocation;
            //LocalShipDestroyed(); // TODO: Listen to this event
            StartCoroutine(RequestRespawn());
        }

        private IEnumerator RequestRespawn ()
        {
            yield return new WaitForSeconds(RespawnDelay);
            for (int i = 0; i < RespawnRequestMaxAttempts; i++)
            {
                Debug.Log("Requesting Respawn " + i);
                if (ShipSpawned) { yield break; }
                CmdRequestRespawn(CurrentShipClassChoice);
                if (ShipSpawned) { yield break; }
                else
                {
                    // linear backoff
                    float WaitDelay = (i == 0 ? 1f : RespawnDelay * i);
                    yield return new WaitForSeconds(WaitDelay);
                }
            }
            throw new Exception(
                CreateMaxRespawnRequestAttemptsExceededException()
            );
        }

        /// <summary>
        /// Event handler
        /// 
        /// Only executed on the server.
        /// This change in state is communicated back to the client
        /// via the SyncEvent firing
        /// </summary>
        /// <param name="new_health"></param>
        private void shipHealthChanged(double new_health)
        {
            //Debug.Log("Incorporeal controller recceived event from ship controller");
            if (hasAuthority)
            {
                ShipHealthChangeHandler handler = LocalPlayerShipHealthChanged;
                if (oem.shouldTriggerEvent(handler))
                {
                    handler(new_health);
                    //Debug.Log("Incorporeal controller propagated event");
                }
            }
            else
            {
                //Debug.Log("Ship health changed for a non-player ship");
            }
        }

        private string CreateMaxRespawnRequestAttemptsExceededException()
        {
            return "Requested to respawn this player's spaceship "
                + RespawnRequestMaxAttempts
                + " times, but the server refused all of them.";
        }
    }
}


