using System;
using UnityEngine;

namespace SpaceBattles
{
    public class OrbitingBodyBackgroundGameObject : MonoBehaviour
    {
        // -- Fields --
        public OrbitingBodyMathematics.ORBITING_BODY PlanetNumber;
        /// <summary>
        /// In km i.e. nearest_planet_scale. 
        /// 
        /// The Maths does not use radius at all (only mass)
        /// so it is okay to be a bit loose with it.
        /// </summary>
        [Tooltip("In km i.e. NearestPlanetScale")]
        public double Radius;

        private Vector3 CurrentPlanetsNormalPitchAndRoll;
        private OrbitingBodyMathematics Maths;
        private bool IsNearestPlanet = false;

        // -- Enums --


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

        private void UpdatePosition()
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

        private void UpdateRotation()
        {
            // negative because anti-clockwise
            float TargetRotation
                = -Convert.ToSingle(
                    Maths.current_daily_rotation_progress()
                    / OrbitingBodyMathematics.DEG_TO_RAD)
                ;
            float NeededRotation = TargetRotation
                                  - transform.localEulerAngles.y;
            transform.Rotate(transform.up, NeededRotation, Space.World);
        }

        private void ChangeToNearestRadius()
        {
            SetScale(Scale.NearestPlanet);
        }

        private void ChangeToSolarSystemRadius()
        {
            SetScale(Scale.SolarSystem);
        }

        /// <summary>
        /// NB: does not alter the camera rendering layer
        /// or periodic transform updates
        /// (use ChangeToXReferenceFrame for full functionality)
        /// </summary>
        /// <param name="targetScale"></param>
        public void SetScale(Scale targetScale)
        {
            double ScaledRadius
                = Scale.NearestPlanet
                .ConvertMeasurementTo(targetScale, Radius);
            SetScale(ScaledRadius);
        }

        /// <summary>
        /// NB: does not alter the camera rendering layer
        /// or periodic transform updates
        /// (use ChangeToXReferenceFrame for full functionality)
        /// </summary>
        /// <param name="scaledRadius"></param>
        public void SetScale(double scaledRadius)
        {
            float fScaledRadius
                = Convert.ToSingle(scaledRadius);
            transform.localScale
                = new Vector3(fScaledRadius, fScaledRadius, fScaledRadius);
        }
    }
}
