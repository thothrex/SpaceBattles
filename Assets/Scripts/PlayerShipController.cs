using System.Collections;
using System.IO;
using UnityEngine.Networking;
using UnityEngine; // for Lights
using System;

namespace SpaceBattles
{
    public class PlayerShipController : NetworkBehaviour
    {
        public UIManager UI_manager;
        public float maxSpeed = 1000;
        public float engine_power = 1000;
        public float rotation_power = 100;
        public const double MAX_HEALTH = 10.0;

        private Vector3 acceleration_direction; // in LOCAL coordinates
        private bool accelerating = false;
        private bool braking = false;
        private bool warping = false;
        private OrbitingBodyBackgroundGameObject current_nearest_orbiting_body;
        [SyncVar]
        private double health;

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
                //GetComponent<Rigidbody>().AddForce(-body.velocity.normalized * engine_power);
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
                if (this.isLocalPlayer)
                {
                    UI_manager.setCurrentPlayerHealth(health);
                }
            }
        }

        private void killThisUnit ()
        {
            throw new NotImplementedException("Units can't die yet");
        }
    }
}
