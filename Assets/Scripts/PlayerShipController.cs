using System.Collections;
using System.IO;
using UnityEngine.Networking;
using UnityEngine; // for Lights
using System;

namespace SpaceBattles
{
    public class PlayerShipController : NetworkBehaviour
    {
        // -- Constant Fields --
        public const double MAX_HEALTH = 10.0;
        private const float PHASER_BOLT_FORCE = 300.0f;
        private const string LASER_SPAWN_LOCATION_UNINITIALISED_ERRMSG
            = "The Player Ship Controller's spawn location for laser bolts "
            + "has not been set yet.\nIt needs to be initialised with "
            + "initialiseLaserSpawnLocalLocation before it can be read.";
        private const string SSCLASS_NULL_ERRMSG
            = "setSpaceshipClass should not set the new class to NONE (used as an analogue to null)";

        // -- (Variable) Fields --
        // The following are set in the editor,
        // so should be left unassigned here
        [Header("Physics Properties")]
        public float max_speed;
        public float engine_power;
        public float rotation_power;
        [Tooltip("The speed of the ship's pitch will be multiplied "
                +"by this factor after all other calculations")]
        public float pitch_fudge_factor = 1.0f;
        [Header("Graphical Objects")]
        public GameObject phaser_bolt_prefab;
        [Header("Gameplay Objects")]
        [Tooltip(
            "The location where projectiles will spawn. Note: "
            + "this ought to already be a child of the ship "
            + "gameObject if you want the spawn location to "
            + "follow the ship."
         )]
        public Transform projectile_spawn_location;

        private Vector3 acceleration_direction; // in LOCAL coordinates
        private bool accelerating = false;
        private bool braking = false;
        private bool warping = false;
        private float rotate_roll = 0.0f;
        private float rotate_pitch = 0.0f;
        private OrbitingBodyBackgroundGameObject current_nearest_orbiting_body;
        private SpaceShipClass current_spaceship_class = SpaceShipClass.NONE;
        private Rigidbody physics_body;
        private OptionalEventModule oem = null;
        [SyncVar] private double health;

        // -- Delegates --
        public delegate void LocalPlayerStartHandler();
        public delegate void HealthChangeHandler(double new_health);
        public delegate void DeathHandler(Vector3 death_location);

        // -- Events --
        public event LocalPlayerStartHandler StartLocalPlayer;
        [SyncEvent] public event HealthChangeHandler EventHealthChanged;
        [SyncEvent] public event DeathHandler EventDeath;

        // -- Enums --
        // -- Properties --
        private string SSCLASS_ALREADY_SET_ERRMSG
        {
            get
            {
                return "setSpaceShipClass is attempting to set a value for spaceShipClass "
                 + "when the PlayerShipController already has a SpaceShipClass set: "
                 + current_spaceship_class.ToString();
            }
        }

        // -- Methods --
        public void Awake ()
        {
            // init health
            health = MAX_HEALTH;
        }

        override
        public void OnStartClient ()
        {
            oem = new OptionalEventModule();
            oem.allow_no_event_listeners = false;
            physics_body = GetComponent<Rigidbody>();
        }

        // updates for phsyics
        public void FixedUpdate()
        {
            if (!hasAuthority)
                return;

            Vector3 torque
                = new Vector3(rotate_pitch * pitch_fudge_factor,
                              0.0f,
                              rotate_roll);
            physics_body.AddRelativeTorque(torque * rotation_power * Time.deltaTime);

            if (accelerating)
            {
                Vector3 global_acceleration_direction
                    = transform.TransformDirection(acceleration_direction);
                physics_body.AddForce(global_acceleration_direction * engine_power * Time.deltaTime);
            }
            if (braking)
            {
                //GetComponent<Rigidbody>().AddForce(-body.velocity.normalized * engine_power);
            }
            if (physics_body.velocity.magnitude > max_speed)
            {
                physics_body.velocity = physics_body.velocity.normalized * max_speed;
            }
        }

        // Input direction is in local space coordinates
        public void accelerate(Vector3 direction)
        {
            acceleration_direction = direction;
            accelerating = true;
            braking = false;
        }

        public void brake ()
        {
            accelerating = false;
            braking = true;
        }

        public double getHealth ()
        {
            return health;
        }

        public Vector3 get_projectile_spawn_location()
        {
            if (projectile_spawn_location == null)
            {
                throw new InvalidOperationException(
                    LASER_SPAWN_LOCATION_UNINITIALISED_ERRMSG
                );
            }
            return projectile_spawn_location.position;
        }

        /// <summary>
        /// This is a command sent directly from the player controller
        /// to the server.
        /// 
        /// Spawns phaser bolt at shot_origin's location & rotation
        /// giving it an initial force defined in PassthroughNetworkManager.
        /// 
        /// Also sets the bolt to automatically disappear after an arbitrary length of time
        /// </summary>
        /// <param name="shot_origin">
        /// A Transform object used to set the bolt's initial position & rotation
        /// </param>
        /// <param name="bolt_owner">Currently unused</param>
        [Command]
        public void CmdFirePhaser()
        {
            // Create the bolt locally
            GameObject bolt = (GameObject)Instantiate(
                 phaser_bolt_prefab,
                 projectile_spawn_location.position,
                 transform.rotation);
            bolt.GetComponent<Rigidbody>()
                .velocity = (PHASER_BOLT_FORCE * bolt.transform.forward);

            // Spawn the bullet on the clients
            NetworkServer.Spawn(bolt);
            // Set self-destruct timer
            Destroy(bolt, 2.0f);
        }
        /// <summary>
        /// I would expand this to include a new parameter
        /// of enum HitType with value of PHASER_BOLT etc.
        /// </summary>
        public void onProjectileHit()
        {
            // Stops useless messages propagating
            // Easier to reason about with one path
            if (isServer)
            {
                takeDamage(1.0);
            }
        }

        public SpaceShipClass getSpaceshipClass ()
        {
            // This is a value type so safe to return directly
            return current_spaceship_class;
        }

        public void setSpaceshipClass (SpaceShipClass ss_class)
        {
            if (current_spaceship_class == SpaceShipClass.NONE)
            {
                current_spaceship_class = ss_class;
            }
            else if (ss_class == SpaceShipClass.NONE)
            {
                throw new ArgumentException(SSCLASS_NULL_ERRMSG, "ss_class");
            }
            else if (current_spaceship_class != SpaceShipClass.NONE)
            {
                throw new InvalidOperationException(
                    SSCLASS_ALREADY_SET_ERRMSG
                );
            }
        }

        public void setRoll (float new_roll)
        {
            rotate_roll = new_roll;
        }

        public void setPitch (float new_pitch)
        {
            rotate_pitch = new_pitch;
        }

        /// <summary>
        /// The result is propagated via the SyncEvent
        /// EventHealthChanged(int new_health);
        /// </summary>
        /// <param name="amount"></param>
        [Server]
        private void takeDamage(double amount)
        {
            if (amount >= health)
            {
                health = 0;
                killThisUnit();
            }
            else
            {
                health -= amount;
                HealthChangeHandler handler = EventHealthChanged;
                if (oem.shouldTriggerEvent(handler))
                {
                    handler(health);
                }
            }
        }

        private void killThisUnit()
        {
            Debug.Log("Player is dead!");
            if (transform.position == null)
            {
                throw new InvalidOperationException(
                    "For some reason the player's position is null"
                );
            }
            DeathHandler handler = EventDeath;
            if (oem.shouldTriggerEvent(handler))
            {
                handler(transform.position);
            }
        }
    }
}
