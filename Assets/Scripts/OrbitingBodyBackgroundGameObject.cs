using System;
using UnityEngine;

namespace SpaceBattles
{
    public class
    OrbitingBodyBackgroundGameObject
        : MonoBehaviour,
          IGameObjectRegistryKeyComponent
    {
        // -- Const Fields --
        private const double DEFAULT_SCALE_VALUE = 1.0;
        private const float DEFAULT_MATHS_UPDATE_RATE = 1.0f;

        // -- Fields --
        public OrbitingBody PlanetNumber;
        /// <summary>
        /// In km i.e. nearest_planet_scale. 
        /// 
        /// The Maths does not use radius at all (only mass)
        /// so it is okay to be a bit loose with it.
        /// </summary>
        [Tooltip("In km i.e. NearestPlanetScale")]
        public double Radius;
        [Tooltip("In seconds (as a decimal)")]
        public float OrbitingBodyMathsUpdateInterval = DEFAULT_MATHS_UPDATE_RATE;
        public Vector3 AssetInitialPitchAndRoll;
        public Vector3 ObjectBaseScale;
        
        private OrbitingBodyMathematics Maths;
        
        private bool IsNearestPlanet = false;
        private bool ShouldUpdateRotation = false;
        private bool ShouldUpdatePosition = false;
        private bool TimePausedBackingValue = false;
        private TimeSpan TimeOffset = new TimeSpan(0);
        private double ExplicitScale = DEFAULT_SCALE_VALUE;
        private float TimeSinceLastMathsUpdate = 0.0f;

        // -- Properties --
        public int Key
        {
            get { return (int)PlanetNumber; }
        }

        private bool TimePaused
        {
            get
            {
                return TimePausedBackingValue;
            }
            set
            {
                TimePausedBackingValue = value;
                ShouldUpdatePosition = !value;
            }
        }

        // -- Enums --


        // -- Methods --
        public void Awake()
        {
            if (PlanetNumber != OrbitingBody.SUN)
            {
                Maths = OrbitingBodyMathematics.generate_planet(PlanetNumber);
            }
            if (transform.root != transform)
            {
                // expects a uniform scale e.g. (1,1,1)
                ExplicitScale = transform.localScale.magnitude;
            }
        }

        public void Start()
        {
            // If not using an explicit scale value
            if (ExplicitScale.Equals(DEFAULT_SCALE_VALUE))
            {
                if (IsNearestPlanet)
                {
                    ChangeToNearestRadius();
                }
                else
                {
                    ChangeToSolarSystemRadius();
                }
            }

            ShouldUpdatePosition = (PlanetNumber != OrbitingBody.SUN);
            ShouldUpdateRotation = Maths.has_rotation_data();
            transform.localEulerAngles = Maths.default_rotation_tilt_euler_angle;
        }

        public void Update ()
        {
            if (ShouldUpdatePosition)
            {
                TimeSinceLastMathsUpdate += Time.deltaTime;
                if (TimeSinceLastMathsUpdate >= OrbitingBodyMathsUpdateInterval)
                {
                    UpdatePosition();
                    if (ShouldUpdateRotation)
                    {
                        UpdateRotation();
                    }
                    TimeSinceLastMathsUpdate = 0.0f;
                }
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
            MyContract.RequireField(PlanetNumber != OrbitingBody.SUN,
                                    "this must not be the sun",
                                    "PlanetNumber");
            MyContract.RequireFieldNotNull(Maths, "Maths");

            Vector3 current_position = Maths.current_location_game_coordinates();
            Quaternion Rotation = Quaternion.LookRotation(current_position);
            sunlight.transform.rotation = Rotation;
        }

        public Vector3 GetCurrentGameSolarSystemCoordinates()
        {
            MyContract.RequireFieldNotNull(Maths, "Maths");
            return Maths.current_location_game_coordinates();
        }

        /// <summary>
        /// Sets the orbiting body's size relative to its radius
        /// i.e. a true scaling of the orbiting body
        /// </summary>
        /// <param name="relativeScale"></param>
        public void SetRelativeLinearScaleExplicitly (double relativeScale)
        {
            //Debug.Log("Setting relative scale explicitly to " + relativeScale);
            double NewScale = relativeScale * Radius;
            SetScale(NewScale);
            ExplicitScale = NewScale;
        }

        /// <summary>
        /// Sets the orbiting body's size relative to its radius
        /// i.e. a true scaling of the orbiting body
        /// </summary>
        /// <param name="relativeScale"></param>
        public void
        SetRelativeLogarithmicScaleExplicitly
            (double logBase, double innerMultiplier, double outerMultiplier)
        {
            //Debug.Log("Setting relative scale explicitly to " + relativeScale);
            double NewScale = outerMultiplier * Math.Log(Radius * innerMultiplier, logBase);
            SetScale(NewScale);
            ExplicitScale = NewScale;
        }

        /// <summary>
        /// NB: does not alter the camera rendering layer
        /// or periodic transform updates
        /// (use ChangeToXReferenceFrame for full functionality)
        /// </summary>
        /// <param name="targetScale"></param>
        public void SetScaleToPredefinedScale(Scale targetScale)
        {
            //Debug.Log("Setting scale to " + targetScale);
            double ScaledRadius
                = Scale.NearestPlanet
                .ConvertMeasurementTo(targetScale, Radius);
            SetScale(ScaledRadius);
        }

        public void UseExplicitDateTime (DateTime explicitTime)
        {
            // TODO: Allow pausing time
            TimeOffset = explicitTime - DateTime.Now;
        }

        /// <summary>
        /// NB: does not alter the camera rendering layer
        /// or periodic transform updates
        /// (use ChangeToXReferenceFrame for full functionality)
        /// </summary>
        /// <param name="scaledRadius"></param>
        private void SetScale(double scaledRadius)
        {
            //Debug.Log("Setting scale internally to " + scaledRadius);
            float fScaledRadius
                = Convert.ToSingle(scaledRadius);
            transform.localScale
                = fScaledRadius * ObjectBaseScale;
        }

        private void UpdatePosition()
        {
            UpdatePosition(DateTime.Now);
        }

        private void UpdatePosition (DateTime time)
        {
            MyContract.RequireFieldNotNull(Maths, "Maths");
            if (IsNearestPlanet)
            {
                transform.position = new Vector3(0, 0, 0);
            }
            else
            {
                transform.localPosition
                    = Maths.current_location_game_coordinates(
                        time + TimeOffset
                      );
            }
        }

        private void UpdateRotation ()
        {
            UpdateRotation(DateTime.Now);
        }

        private void UpdateRotation (DateTime time)
        {
            MyContract.RequireFieldNotNull(Maths, "Maths");

            DateTime TargetTime = time + TimeOffset;
            double RotationDegrees =
                  Maths.current_stellar_day_rotation_progress(TargetTime)
                * 360.0;
            // negative because anti-clockwise
            float TargetRotation = Convert.ToSingle(-RotationDegrees);
            TargetRotation += AssetInitialPitchAndRoll.y;
            //Debug.Log("Target rotation: " + TargetRotation);
            float NeededRotation = TargetRotation
                                  - transform.localEulerAngles.y;
            transform.Rotate(transform.up, NeededRotation, Space.World);
        }

        private void ChangeToNearestRadius()
        {
            SetScaleToPredefinedScale(Scale.NearestPlanet);
        }

        private void ChangeToSolarSystemRadius()
        {
            SetScaleToPredefinedScale(Scale.SolarSystem);
        }
    }
}
