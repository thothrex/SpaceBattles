using System.Collections;
using System.IO;
using UnityEngine.Networking;
using UnityEngine; // for Lights
using System;

namespace SpaceBattles
{
    public class PlayerShipController : NetworkBehaviour
    {
        public delegate void LocalPlayerStartHandler();
        public event LocalPlayerStartHandler StartLocalPlayer;

        // The following are set in the editor,
        // so should be left unassigned here
        public UIManager UI_manager;
        public float max_speed;

        public float engine_power;
        public float rotation_power;
        public const double MAX_HEALTH = 10.0;

        public GameObject phaser_bolt_prefab;

        private const float PHASER_BOLT_FORCE = 300.0f;

        private Vector3 local_projectile_spawn_location;
        private Vector3 acceleration_direction; // in LOCAL coordinates
        private bool accelerating = false;
        private bool braking = false;
        private bool warping = false;
        private OrbitingBodyBackgroundGameObject current_nearest_orbiting_body;
        [SyncVar]
        private double health;

        public void Awake ()
        {
            // Set projectile spawn location
            Vector3 ship_extents = GetComponent<Renderer>().bounds.extents;
            // spawn projectiles at the front of the ship plus 10% of the ship length
            // (10% of ship length = 20% of ship extent as extent is from centre)
            float projectile_spawn_distance = ship_extents.y + ship_extents.y * 0.2f;
            local_projectile_spawn_location = new Vector3(0, projectile_spawn_distance, 0);

            // init health
            health = MAX_HEALTH;
        }

        public void Start ()
        {
        }

        override
        public void OnStartAuthority()
        {
            // We need a set place to send messages to
            // in case authority is transferred,
            // i.e. when there is no local creator entity
            ProgramInstanceManager instance_manager
                    = GameObject.Find("ProgramInstanceManager").GetComponent<ProgramInstanceManager>();
            instance_manager.playerShipCreatedHandler(this);
        }

        public void Update ()
        {
            
        }

        // updates for phsyics
        void FixedUpdate()
        {
            if (!hasAuthority)
                return;

            // accel for accelerometer, input for keyboard
            float rotate_roll = -Input.acceleration.x * 0.5f + (-Input.GetAxis("Roll"));
            float rotate_pitch = -Input.acceleration.z * 0.5f + Input.GetAxis("Pitch");

            Vector3 torque = new Vector3(rotate_pitch, 0.0f, rotate_roll);

            Rigidbody body = GetComponent<Rigidbody>();
            body.AddRelativeTorque(torque * rotation_power * Time.deltaTime);

            if (accelerating)
            {
                Vector3 global_acceleration_direction
                    = transform.TransformDirection(acceleration_direction);
                GetComponent<Rigidbody>().AddForce(global_acceleration_direction * engine_power);
            }
            if (braking)
            {
                //GetComponent<Rigidbody>().AddForce(-body.velocity.normalized * engine_power);
            }
            if (body.velocity.magnitude > max_speed)
            {
                body.velocity = body.velocity.normalized * max_speed;
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

        private void takeDamage (double amount)
        {
            if (amount >= health)
            {
                health = 0;
                killThisUnit();
            }
            else
            {
                health -= amount;
            }

            if (this.isLocalPlayer)
            {
                UI_manager.setCurrentPlayerHealth(health);
            }
        }

        private void killThisUnit ()
        {
            // TODO: kill player properly
            Debug.Log("Player is dead!");
        }

        public Vector3 get_projectile_spawn_location()
        {
            return transform.TransformPoint(local_projectile_spawn_location);
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
                 transform.TransformPoint(local_projectile_spawn_location),
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
            takeDamage(1.0);
        }
    }
}
