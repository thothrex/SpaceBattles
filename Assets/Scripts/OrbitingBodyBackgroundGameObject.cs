using System;
using UnityEngine;

namespace SpaceBattles
{
    public class OrbitingBodyBackgroundGameObject : MonoBehaviour
    {
        public static readonly double NEAREST_PLANET_SCALE_TO_METRES = 1000000.0; // using planetary scale of 1,000km
        public static readonly double NEAREST_PLANET_SCALE_TO_ORBITING_BODY_SCALE
            = NEAREST_PLANET_SCALE_TO_METRES / OrbitingBodyMathematics.DISTANCE_SCALE_TO_METRES;
        public static readonly double ORBITING_BODY_SCALE_TO_NEAREST_PLANET_SCALE
            = OrbitingBodyMathematics.DISTANCE_SCALE_TO_METRES / NEAREST_PLANET_SCALE_TO_METRES;

        private OrbitingBodyMathematics maths;
        public OrbitingBodyMathematics.ORBITING_BODY planet_number;

        private Vector3 current_planets_normal_pitch_and_roll;

        private bool is_nearest_planet = false;
        public double radius; // in km i.e. nearest_planet_scale

        /// <summary>
        /// Current use-case constructor - will expand if necessary
        /// </summary>
        void Awake()
        {
            maths = OrbitingBodyMathematics.generate_planet(planet_number);
        }

        void Start()
        {
            InvokeRepeating("updatePosition", 0.05f, 1.0f);
            if (is_nearest_planet)
            {
                changeToNearestRadius();
            }
            else
            {
                changeToSolarSystemRadius();
            }
            
            if (maths.has_rotation_data())
            {
                transform.localEulerAngles = maths.default_rotation_tilt_euler_angle;
                InvokeRepeating("updateRotation", 0.05f, 1.0f);
            }
        }

        void updatePosition ()
        {
            if (is_nearest_planet)
            {
                transform.position = new Vector3(0, 0, 0);
            }
            else
            {
                transform.position = maths.current_location_game_coordinates();
            }
        }

        void updateRotation ()
        {
            // negative because anti-clockwise
            float target_rotation
                = - Convert.ToSingle(
                    maths.current_daily_rotation_progress()
                    / OrbitingBodyMathematics.DEG_TO_RAD)
                ;
            float needed_rotation = target_rotation 
                                  - transform.localEulerAngles.y;
            transform.Rotate(transform.up, needed_rotation, Space.World);
        }

        private void changeToNearestRadius ()
        {
            float radiusf = Convert.ToSingle(radius);
            transform.localScale = new Vector3(radiusf, radiusf, radiusf);
        }

        private void changeToSolarSystemRadius ()
        {
            double scaled_radius = radius * NEAREST_PLANET_SCALE_TO_ORBITING_BODY_SCALE;
            float scaled_radiusf = Convert.ToSingle(scaled_radius);
            transform.localScale = new Vector3(scaled_radiusf, scaled_radiusf, scaled_radiusf);
        }

        /// <summary>
        /// Scales radius of this object and, changes rendering layer and moves to origin
        /// </summary>
        public void changeToOrbitalReferenceFrame ()
        {
            changeToNearestRadius();
            is_nearest_planet = true;
            gameObject.layer = LayerMask.NameToLayer(ProgramInstanceManager.NEAREST_PLANET_LAYER_NAME);
        }

        /// <summary>
        /// Scales radius of this object, changes rendering layer and sets coordinates
        /// </summary>
        public void changeToSolarSystemReferenceFrame ()
        {
            changeToSolarSystemRadius();
            is_nearest_planet = false;
            gameObject.layer = LayerMask.NameToLayer(ProgramInstanceManager.SOLAR_SYSTEM_LAYER_NAME);
        }

        public void updateSunDirection (Light sunlight)
        {
            Debug.Assert(maths != null);
            Vector3 current_position = maths.current_location_game_coordinates();
            var rotation = Quaternion.LookRotation(current_position);
            sunlight.transform.rotation = rotation;
        }

        public Vector3 getCurrentGameSolarSystemCoordinates ()
        {
            return maths.current_location_game_coordinates();
        }
    }
}
