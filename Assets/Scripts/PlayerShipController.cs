using System.Collections;
using System.IO;
using UnityEngine.Networking;
using UnityEngine; // for Lights

namespace SpaceBattles
{
    public class PlayerShipController : NetworkBehaviour
    {
        public InertialPlayerCamera player_camera_prefab;
        public LargeScaleCamera nearest_planet_camera_prefab;
        public LargeScaleCamera solar_system_camera_prefab;
        public UIManager UI_manager;

        public InertialPlayerCamera player_camera;
        public LargeScaleCamera nearest_planet_camera;
        public LargeScaleCamera solar_system_camera;
        public Light sunlight;
        public float maxSpeed = 1000;

        private const float engine_power = 1000;
        private const float rotation_power = 100;
        private Vector3 acceleration_direction; // in LOCAL coordinates
        private bool accelerating = false;
        private bool braking = false;
        private bool warping = false;
        private OrbitingBodyBackgroundGameObject current_nearest_orbiting_body;

        void Start()
        {
            
        }

        override
        public void OnStartLocalPlayer()
        {
            // Remove - should be in ClientManager
            Debug.Log("Creating player controller");
            if (hasAuthority)
            {
                Debug.Log("Creator has authority");
                initGameCameras(gameObject.transform);
            }
        }

        public void initGameCameras(Transform transform)
        {
            player_camera = Instantiate(player_camera_prefab);
            nearest_planet_camera = Instantiate(nearest_planet_camera_prefab);
            nearest_planet_camera.using_preset_scale = LargeScaleCamera.PRESET_SCALE.NEAREST_PLANET;
            solar_system_camera = Instantiate(solar_system_camera_prefab);
            solar_system_camera.using_preset_scale = LargeScaleCamera.PRESET_SCALE.SOLAR_SYSTEM;

            player_camera.followTransform = transform;
            // instantiate with default values (arbitrary)
            player_camera.offset = new Vector3(0,7,-30);
            nearest_planet_camera.followTransform = transform;
            solar_system_camera.followTransform = transform;

            player_camera.enabled = true;
            nearest_planet_camera.enabled = true;
            solar_system_camera.enabled = true;
        }

        // TODO: Remove - should be in UI Manager
        public void Update ()
        {
            if (Input.GetKeyDown("space"))
            {
                Debug.Log("spacebar pressed");
                accelerate(new Vector3(0, 0, 1));
            }
            else if (Input.GetKeyUp("space"))
            {
                Debug.Log("spacebar released");
                brake();
            }

            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    accelerate(new Vector3(0, 0, 1));
                    break;
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    brake();
                    break;
                }
            }
        }

        // updates for phsyics
        void FixedUpdate()
        {
            if (!isLocalPlayer)
                return;

            // accel for accelerometer, input for keyboard
            float rotate_roll = -Input.acceleration.x * 0.5f + (-Input.GetAxis("Horizontal"));
            float rotate_pitch = -Input.acceleration.z * 0.5f + Input.GetAxis("Vertical");

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
                GetComponent<Rigidbody>().AddForce(-body.velocity.normalized * engine_power);
            }
            if (body.velocity.magnitude > maxSpeed)
            {
                body.velocity = body.velocity.normalized * maxSpeed;
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
    }
}
