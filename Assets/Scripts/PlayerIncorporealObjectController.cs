using System;
using UnityEngine;
using UnityEngine.Networking;

namespace SpaceBattles
{
    public class PlayerIncorporealObjectController : NetworkBehaviour
    {
        public delegate void LocalPlayerStartHandler();
        public event LocalPlayerStartHandler StartLocalPlayer;

        public enum SpaceShipClass { FIGHTER };

        // The following are set in the editor,
        // so should be left unassigned here
        public GameObject PrefabSpaceShipFighter;

        private bool warping = false;
        private OrbitingBodyBackgroundGameObject current_nearest_orbiting_body;
        private GameObject player_body;
        private SpaceShipClass player_ship_class_choice = SpaceShipClass.FIGHTER;
        private GameObject current_spaceship;

        public void Start()
        {
        }

        /// <summary>
        /// Not as horrible as I'd first though.
        /// There has to be a fixed point of reference
        /// i.e. a guaranteed-to-exist object
        /// in order for objects which have their authority transferred
        /// to report this change to.
        /// 
        /// We use the Program Instance Manager (PIM) as this fixed reference.
        /// </summary>
        override
        public void OnStartAuthority ()
        {
            ProgramInstanceManager pim
                    = GameObject.Find("ProgramInstanceManager").GetComponent<ProgramInstanceManager>();
            pim.PlayerControllerCreatedHandler(this);
            // Get Cameras to follow this object (i.e. viewpoint is the spawn location)
            // until the player chooses their ship.
            //
            // Game cameras will be inactive until a ship is chosen
            // at which point the ship choice UI camera will be deactivated
            // and the game cameras will be activated.

            // The ship registers itself with the PIM
            CmdSpawnSpaceShip(player_ship_class_choice);
        }

        public void Update()
        {

        }

        [Command]
        public void CmdSpawnSpaceShip (SpaceShipClass ss_type)
        {
            GameObject spaceship_prefab = null;
            switch (ss_type)
            {
                case SpaceShipClass.FIGHTER:
                    spaceship_prefab = PrefabSpaceShipFighter;
                    break;
            }
            // Create the ship locally (local to the server)
            GameObject spaceship = (GameObject)Instantiate(
                 spaceship_prefab,
                 transform.TransformPoint(new Vector3(0,0,0)),
                 transform.rotation);

            // Spawn the ship on the clients
            NetworkServer.SpawnWithClientAuthority(spaceship, connectionToClient);
        }

        /// <summary>
        /// Event handler from an event propagated upward from whatever the current
        /// "body" of the player is (e.g. space ship).
        /// Probably should have a different one for other objects owned by this player.
        /// </summary>
        private void playerBodyKilled()
        {
            // TODO: kill player properly
            Debug.Log("Player is dead!");
            // Maybe have a hook onto the "body" so that we can tell
            // when its death animation is finished?
            // (assuming death anim should be handled by the body
            //  rather than the incorporeal controller)
            // If so this function will do very little
            // unless we want to enforce a specific respawn time
        }
    }
}


