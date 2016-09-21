using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SpaceBattles
{
    /// <summary>
    /// This class is the representation of the player on the server.
    /// Crucially, this means it should be the only source of [Command] and [ClientRPC] methods
    /// as it is the point where the network and the client interact.
    /// </summary>
    public class IncorporealPlayerController : NetworkBehaviour
    {
        // -- Constants --
        public const float RESPAWN_DELAY           = 2.0f;
        public const float SPACESHIP_DESTROY_DELAY = 0.5f;
        private const string SHIP_CONTROLLER_NOT_SET_ERRMSG
            = "The ship controller has not been set yet.";
        private const string LOCAL_SHIP_SPAWN_NO_LISTENERS_ERRMSG
            = "There are no listeners for the local ship spawning event.";
        private const string LOCAL_PLAYER_SPAWN_NO_LISTENERS_ERRMSG
            = "There are no listeners for the local player controller spawning event.";
        private const string NONLOCAL_PLAYER_SPAWN_NO_LISTENERS_ERRMSG
            = "There are no listeners for the non-local player controller spawning event.";

        // -- Fields --

        // The following are set in the editor,
        // so should be left unassigned here
        public GameObject explosion_prefab;

        private bool warping = false;
        private bool setup_complete = false;
        // Cannot be synced - use with caution
        private PlayerShipController ship_controller = null;
        private OrbitingBodyBackgroundGameObject current_nearest_orbiting_body;
        // Only valid for the server
        private NetworkStartPosition[] spawnPoints;
        // Write-only variable (written by the pim from UI manager etc.)
        private SpaceShipClass current_ship_choice = SpaceShipClass.NONE;
        private SpaceShipClassManager spaceship_class_manager = null;
        private OptionalEventModule oem = null;
        // NB SyncVars ALWAYS sync from server->client,
        //    even for client-authoritative objects (such as this)
        [SyncVar]
        private GameObject current_spaceship = null;
        [SyncVar]
        private bool player_ship_spawned = false;

        // -- Delegates --
        public delegate void LocalPlayerStartHandler    (IncorporealPlayerController IPC);
        public delegate void ShipSpawnedHandler         (PlayerShipController        player_ship_controller);
        public delegate void ShipHealthChangeHandler    (double                      new_health);
        public delegate void LocalShipDestroyedHandler  ();

        // -- Events --
        public event LocalPlayerStartHandler    LocalPlayerStarted;
        public event ShipSpawnedHandler         LocalPlayerShipSpawned;
        public event ShipHealthChangeHandler    LocalPlayerShipHealthChanged;
        public event LocalShipDestroyedHandler  LocalShipDestroyed;

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
            if (!player_ship_spawned)
            {
                player_ship_spawned = true;
                spawnSpaceShip(ss_type);
            }
        }

        /// <summary>
        /// Needs to be called by the authoritative client.
        /// </summary>
        /// <param name="new_ship_class"></param>
        [Command]
        public void CmdRequestRespawn(SpaceShipClass new_ship_class)
        {
            throw new NotImplementedException();
            // do some checks, then
            // respawnShipWithDelay
            // TODO: Implement
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
            oem.allow_no_event_listeners = false;
        }

        /// <summary>
        /// I need some setup to be done before I want
        /// to trigger the event
        /// </summary>
        override
        public void OnStartAuthority ()
        {
            Debug.Log("IPC authority started");
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
            spaceship_class_manager = ss_manager;
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
            if (ship_controller != null)
            {
                ship_controller.accelerate(direction);
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
            if (ship_controller != null)
            {
                ship_controller.brake();
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
            if (ship_controller != null)
            {
                ship_controller.CmdFirePhaser();
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
            if (player_ship_spawned)
            {
                playerShipSpawnedHandler(current_spaceship);
            }
        }

        public void setCurrentShipChoice(SpaceShipClass new_choice)
        {
            current_ship_choice = new_choice;
        }

        public void setRoll(float new_roll)
        {
            if (ship_controller != null)
            {
                ship_controller.setRoll(new_roll);
            }
        }

        public void setPitch(float new_pitch)
        {
            if (ship_controller != null)
            {
                ship_controller.setPitch(new_pitch);
            }
        }

        /// <summary>
        /// Mainly filters the trigger so that the event only occurs on
        /// the authoritative client.
        /// 
        /// Minorly, propagates the ship controller change
        /// </summary>
        [ClientRpc]
        private void RpcPlayerShipSpawned (GameObject spawned_spaceship)
        {
            playerShipSpawnedHandler(spawned_spaceship);
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
                if (ship_controller == null)
                {
                    throw new InvalidOperationException(
                        SHIP_CONTROLLER_NOT_SET_ERRMSG
                    );
                }
                ShipSpawnedHandler handler = LocalPlayerShipSpawned;
                if (oem.shouldTriggerEvent(handler))
                {
                    handler(ship_controller);
                }
            }
        }

        private void playerShipSpawnedHandler(GameObject spawned_spaceship)
        {
            Debug.Log("Player Ship spawn registered on this client");
            ship_controller = spawned_spaceship.GetComponent<PlayerShipController>();
            ship_controller.EventDeath += playerBodyKilled;
            ship_controller.EventHealthChanged += shipHealthChanged;
        }

        private IEnumerator respawnShipWithDelay(SpaceShipClass new_ship_class)
        {
            yield return new WaitForSeconds(RESPAWN_DELAY);
            spawnSpaceShip(new_ship_class);
        }

        /// <summary>
        /// Helper function which spawns a ship.
        /// 
        /// Needs to be invoked via a [Command] from the authoritative client.
        /// </summary>
        /// <param name="ss_type"></param>
        [Server]
        private void spawnSpaceShip(SpaceShipClass ss_type)
        {
            GameObject spaceship_prefab
                = spaceship_class_manager.getSpaceShipPrefab(ss_type);

            // Should not remain null unless Unity.Instantiate can return null
            GameObject server_spaceship = null;
            if (current_spaceship != null
            && ship_controller.getSpaceshipClass() == ss_type)
            {
                // current_spaceship was just despawned, not destroyed,
                // so it simply needs to be respawned
                server_spaceship = current_spaceship;
                server_spaceship.SetActive(true);
            }
            else
            {
                // Create the ship locally (local to the server)
                // NB: the ship will be moved to an appropriate NetworkStartPosition
                //     by the server so the location specified here is irrelevant
                server_spaceship = (GameObject)Instantiate(
                    spaceship_prefab,
                    transform.TransformPoint(chooseSpawnLocation()),
                    transform.rotation);
            }

            // Spawn the ship on the clients
            NetworkServer.SpawnWithClientAuthority(server_spaceship, connectionToClient);
            // Update [SyncVar]s
            current_spaceship = server_spaceship;
            ship_controller = server_spaceship.GetComponent<PlayerShipController>();
            ship_controller.setSpaceshipClass(ss_type);
            // Send RPC to clients
            RpcPlayerShipSpawned(current_spaceship);

            ship_controller.EventDeath += shipDestroyedServerAction;
        }
        /// <summary>
        /// </summary>
        /// <param name="death_location">Ignored for this function</param>
        [Server]
        private void shipDestroyedServerAction(Vector3 death_location)
        {
            Debug.Log("Ship destroyed - taking server action");
            Vector3 respawn_location = chooseSpawnLocation();
            StartCoroutine(destroyShipWithDelayCoroutine());
        }

        [Server]
        private IEnumerator destroyShipWithDelayCoroutine()
        {
            yield return new WaitForSeconds(SPACESHIP_DESTROY_DELAY);
            Debug.Log("Unspawning spaceship");
            current_spaceship.SetActive(false);
            NetworkServer.UnSpawn(current_spaceship);
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
        /// <param name="death_location">
        /// Used to set camera transforms during respawn period
        /// </param>
        private void playerBodyKilled(Vector3 death_location)
        {
            Debug.Log("A player is dead!");

            // Create the explosion locally
            GameObject explosion = (GameObject)Instantiate(
                 explosion_prefab,
                 current_spaceship.transform.position,
                 current_spaceship.transform.rotation);

            // The spaceship gameObject is destroyed by the server
            // as server is still technically the source
            // of client-authoritative objects (slightly confusingly)

            // Set self-destruct timer
            Destroy(explosion, 2.0f);

            if (hasAuthority)
            {
                Debug.Log("Our player is dead!");
                this.transform.position = death_location;
                //LocalShipDestroyed(); // TODO: Listen to this event
                //CmdRequestRespawn(current_ship_choice);
            }
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
            Debug.Log("Incorporeal controller recceived event from ship controller");
            if (hasAuthority)
            {
                ShipHealthChangeHandler handler = LocalPlayerShipHealthChanged;
                if (oem.shouldTriggerEvent(handler))
                {
                    handler(new_health);
                    Debug.Log("Incorporeal controller propagated event");
                }
            }
            else
            {
                Debug.Log("Ship health changed for a non-player ship");
            }
        }
    }
}


