using System;
using UnityEngine;

namespace SpaceBattles
{
    public class OrbitingBodyBackgroundGameObject : MonoBehaviour
    {
        // -- Fields --
        public static readonly double NEAREST_PLANET_SCALE_TO_METRES = 1000000.0; // using planetary scale of 1,000km
        public static readonly double NEAREST_PLANET_SCALE_TO_ORBITING_BODY_SCALE
            = NEAREST_PLANET_SCALE_TO_METRES / OrbitingBodyMathematics.DISTANCE_SCALE_TO_METRES;
        public static readonly double ORBITING_BODY_SCALE_TO_NEAREST_PLANET_SCALE
            = OrbitingBodyMathematics.DISTANCE_SCALE_TO_METRES / NEAREST_PLANET_SCALE_TO_METRES;
        
        public OrbitingBodyMathematics.ORBITING_BODY PlanetNumber;
        /// <summary>
        /// In km i.e. nearest_planet_scale
        /// </summary>
        [Tooltip("In km i.e. nearest_planet_scale")]
        public double Radius;

        private Vector3 CurrentPlanetsNormalPitchAndRoll;
        private OrbitingBodyMathematics Maths;
        private bool IsNearestPlanet = false;
        

        // -- Methods --
        public void Awake()
        {
            Maths = OrbitingBodyMathematics.generate_planet(PlanetNumber);
        }

        public void Start()
        {
            if (IsNearestPlanet)
            {
                ChangeToNearestRadius();
            }
            else
            {
                ChangeToSolarSystemRadius();
            }

            InvokeRepeating("UpdatePosition", 0.05f, 1.0f);
            
            if (Maths.has_rotation_data())
            {
                transform.localEulerAngles = Maths.default_rotation_tilt_euler_angle;
                InvokeRepeating("UpdateRotation", 0.05f, 1.0f);
            }
        }

        /// <summary>
        /// Scales radius of this object and, changes rendering layer and moves to origin
        /// </summary>
        public void ChangeToOrbitalReferenceFrame()
        {
            ChangeToNearestRadius();
            IsNearestPlanet = true;
            gameObject.layer = LayerMask.NameToLayer(ProgramInstanceManager.NEAREST_PLANET_LAYER_NAME);
        }

        /// <summary>
        /// Scales radius of this object, changes rendering layer and sets coordinates
        /// </summary>
        public void ChangeToSolarSystemReferenceFrame()
        {
            ChangeToSolarSystemRadius();
            IsNearestPlanet = false;
            gameObject.layer = LayerMask.NameToLayer(ProgramInstanceManager.SOLAR_SYSTEM_LAYER_NAME);
        }

        public void UpdateSunDirection(Light sunlight)
        {
            Debug.Assert(Maths != null);
            Vector3 current_position = Maths.current_location_game_coordinates();
            var Rotation = Quaternion.LookRotation(current_position);
            sunlight.transform.rotation = Rotation;
        }

        public Vector3 GetCurrentGameSolarSystemCoordinates()
        {
            return Maths.current_location_game_coordinates();
        }

        private void UpdatePosition ()
        {
            if (IsNearestPlanet)
            {
                transform.position = new Vector3(0, 0, 0);
            }
            else
            {
                transform.position = Maths.current_location_game_coordinates();
            }
        }

        private void UpdateRotation ()
        {
            // negative because anti-clockwise
            float TargetRotation
                = - Convert.ToSingle(
                    Maths.current_daily_rotation_progress()
                    / OrbitingBodyMathematics.DEG_TO_RAD)
                ;
            float NeededRotation = TargetRotation 
                                  - transform.localEulerAngles.y;
            transform.Rotate(transform.up, NeededRotation, Space.World);
        }

        private void ChangeToNearestRadius ()
        {
            float fRadius = Convert.ToSingle(Radius);
            transform.localScale = new Vector3(fRadius, fRadius, fRadius);
        }

        private void ChangeToSolarSystemRadius ()
        {
            double ScaledRadius = Radius * NEAREST_PLANET_SCALE_TO_ORBITING_BODY_SCALE;
            float fScaledRadius = Convert.ToSingle(ScaledRadius);
            transform.localScale = new Vector3(fScaledRadius, fScaledRadius, fScaledRadius);
        }
    }
}
