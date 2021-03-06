using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBattles
{
    public class OrreryUIManager : MonoBehaviour
    {
        // -- Fields --
        public DateTimePicker DateTimePicker;
        public ScalePicker ScalePicker;
        public ExplicitLayoutGroup DesktopLayout;
        public ExplicitLayoutGroup TabletLayout;
        public ExplicitLayoutGroup MobileLayout;
        public List<Slider> ZoomSliders;
        public float ZoomButtonIncrementFactor = 0.1f;
        public static readonly float InitialZoom = 350;
        
        private readonly float FullRotation = Convert.ToSingle(Math.PI * 2);
        private OptionalEventModule oem = new OptionalEventModule();
        private Camera PlanetCamera = null;
        private InertialCameraController PlanetCameraController = null;
        private Vector2 LastRotation = new Vector2(0, 0);
        private float DesiredCameraOrbitRadius;

        // -- Delegates --
        public delegate void ExplicitDateTimeSetHandler (DateTime newTime);
        public delegate void LinearScaleSetHandler (float scale);
        public delegate void LogScaleSetHandler (float logBase, float innerMultiplier, float outerMultiplier);

        // -- Events --
        public event ExplicitDateTimeSetHandler DateTimeSet;
        public event LinearScaleSetHandler PlanetLinearScaleSet;
        public event LogScaleSetHandler PlanetLogarithmicScaleSet;

        // -- Properties --
        public OrreryManager OrreryManager { private get; set; }

        // -- Methods --
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputAngles">
        /// In Degrees
        /// </param>
        /// <param name="desiredOrbitRadius"></param>
        /// <returns></returns>
        public static Vector3
        CalculateNewCameraOffset
            (Vector2 inputAngles, float desiredOrbitRadius)
        {
            //Debug.Log(
            //    "Received desiredAngles: "
            //    + desiredAngles.ToString()
            //);

            float DesiredXAxisRotation = ConvertToRadians(inputAngles.y);
            Vector3 NewPosition = new Vector3(0, 0, 0);
            NewPosition.y
                = Convert.ToSingle(Math.Cos(DesiredXAxisRotation))
                * desiredOrbitRadius;
            float UpperRadius
                = Convert.ToSingle(Math.Sin(DesiredXAxisRotation))
                * desiredOrbitRadius;

            // Order matters here
            // (rotations are NOT commutative)
            //
            // We want to rotate on the y axis, along a plane paralell
            // to the plane of the galaxy.
            //
            // We use the existing z value as the new radius
            // of a projected circle from the old coordinates
            // onto the new plane.
            float DesiredYAxisRotation = ConvertToRadians(inputAngles.x);
            NewPosition.x
                = Convert.ToSingle(Math.Sin(DesiredYAxisRotation))
                * UpperRadius;
            NewPosition.z
                = Convert.ToSingle(Math.Cos(DesiredYAxisRotation))
                * UpperRadius;

            return NewPosition;
        }

        public static float ConvertToRadians(float degrees)
        {
            return Convert.ToSingle(degrees * (Math.PI / 180.0));
        }

        public void Awake ()
        {
            DesiredCameraOrbitRadius = InitialZoom;
            ScalePicker.ScaleSet.AddListener(BroadcastNewScale);
        }

        public void Start ()
        {
            ResetCamera();
            ResetSliders();
            // Context: New scale broadcast requested by ScalePicker
            //          in its Start() via ScalePicker.ScaleSet event
        }

        public void BroadcastNewDateTime ()
        {
            MyContract.RequireFieldNotNull(DateTimePicker, "DateTimePicker");
            if (oem.shouldTriggerEvent(DateTimeSet))
            {
                DateTimeSet(DateTimePicker.CurrentStoredValue);
            }
        }

        public void BroadcastNewScale ()
        {
            MyContract.RequireFieldNotNull(ScalePicker, "ScalePicker");
            Debug.Log(
                "Broadcasting new "
                + ScalePicker.CurrentScaleType
                + " scale"
            );

            ScalePicker.ScaleOption ScaleOption = ScalePicker.CurrentScaleType;
            switch (ScaleOption)
            {
                case ScalePicker.ScaleOption.Linear:
                    if (oem.shouldTriggerEvent(PlanetLinearScaleSet)
                    &&  ScalePicker.CurrentLinearScale != float.NaN
                    &&  !float.IsInfinity(ScalePicker.CurrentLinearScale))
                    {
                        PlanetLinearScaleSet(
                            ScalePicker.CurrentLinearScale
                        );
                    }
                    break;
                case ScalePicker.ScaleOption.Logarithmic:
                    if (oem.shouldTriggerEvent(PlanetLogarithmicScaleSet)
                    && ScalePicker.CurrentLogBase != float.NaN
                    && !float.IsInfinity(ScalePicker.CurrentLogBase)
                    && ScalePicker.CurrentLogBase != 1
                    && ScalePicker.CurrentLogInnerMultiplier != float.NaN
                    && !float.IsInfinity(ScalePicker.CurrentLogInnerMultiplier)
                    && ScalePicker.CurrentLogOuterMultiplier != float.NaN
                    && !float.IsInfinity(ScalePicker.CurrentLogOuterMultiplier))
                    {
                        //Debug.Log(
                        //    "Passing on log values: "
                        //    + "log base "
                        //    + ScalePicker.CurrentLogBase
                        //    + ", inner multiplier "
                        //    + ScalePicker.CurrentLogInnerMultiplier
                        //    + ", outer multiplier"
                        //    + ScalePicker.CurrentLogOuterMultiplier
                        //);
                        PlanetLogarithmicScaleSet(
                            ScalePicker.CurrentLogBase,
                            ScalePicker.CurrentLogInnerMultiplier,
                            ScalePicker.CurrentLogOuterMultiplier
                        );
                    }
                    break;
                default:
                    throw new UnexpectedEnumValueException
                        <ScalePicker.ScaleOption>(ScaleOption);
            }
            
            
        }

        public void ProvidePlanetCamera (Camera newCamera)
        {
            MyContract.RequireArgumentNotNull(newCamera, "newCamera");
            PlanetCamera = newCamera;
            PlanetCameraController =
                    PlanetCamera.GetComponent<InertialCameraController>();
            MyContract.RequireFieldNotNull(
                PlanetCameraController,
                "PlanetCameraController"
            );
        }

        public void SetZoom (float newZoom)
        {
            MyContract.RequireFieldNotNull(
                PlanetCameraController, "PlanetCameraController"
            );
            DesiredCameraOrbitRadius = newZoom;
            ResetCameraOffset();
        }

        public void IncrementZoom (bool positive)
        {
            MyContract.RequireFieldNotNull(
                PlanetCameraController, "PlanetCameraController"
            );

            int sign = positive ? 1 : -1;
            DesiredCameraOrbitRadius
                += (DesiredCameraOrbitRadius * ZoomButtonIncrementFactor * sign);
            ResetSliders();

            ResetCameraOffset();
        }

        /// <summary>
        /// Silently fails if PlanetCameraController
        /// has not been set up yet
        /// </summary>
        /// <param name="desiredAngles">
        /// In radians
        /// </param>
        public void RotateCamera (Vector2 desiredAngles)
        {
            if (PlanetCameraController != null
            &&  desiredAngles != LastRotation)
            {
                Vector3 NewOffset = CalculateNewOffset(desiredAngles);
                PlanetCameraController.offset = NewOffset;
                ResetCameraRotation();
                LastRotation = desiredAngles;
            }
        }

        private Vector3 CalculateNewOffset (Vector2 rotationAngles)
        {
            MyContract.RequireFieldNotNull(
                PlanetCameraController, "PlanetCameraController"
            );
            MyContract.RequireFieldNotNull(
                PlanetCameraController.FollowTransform,
                "Follow Transform"
            );
            Vector3 CalculatedOffset
                = CalculateNewCameraOffset(
                    rotationAngles,
                    DesiredCameraOrbitRadius
                );
            Vector3 NewOffset
                = PlanetCameraController
                .FollowTransform
                .InverseTransformDirection(CalculatedOffset);
            return NewOffset;
        }

        /// <summary>
        /// For the Unity Editor to view,
        /// SHOULD NOT BE USED in code
        /// </summary>
        /// <param name="newFocus"></param>
        [EnumAction(typeof(OrbitingBody))]
        public void SetPlanetFocus (int newFocus)
        {
            SetPlanetFocus((OrbitingBody)newFocus);
        }
        
        /// <summary>
        /// This is fine to use in code
        /// </summary>
        /// <param name="newFocus"></param>
        public void SetPlanetFocus (OrbitingBody newFocus)
        {
            MyContract.RequireFieldNotNull(
                PlanetCameraController, "PlanetCameraController"
            );
            MyContract.RequireFieldNotNull(
                OrreryManager, "OrreryManager"
            );
            PlanetCameraController.FollowTransform
                = OrreryManager.GetOrbitingBodyTransform(newFocus);
            ResetCamera();
        }

        public void SetLayoutToMobile ()
        {
            //Debug.Log("OrrerUIManager: changing layout to mobile");
            MobileLayout.applyLayout();
        }

        public void SetLayoutToTablet()
        {
            //Debug.Log("OrrerUIManager: changing layout to tablet");
            TabletLayout.applyLayout();
        }

        public void SetLayoutToDesktop()
        {
            //Debug.Log("OrrerUIManager: changing layout to desktop");
            DesktopLayout.applyLayout();
        }


        private void ResetCameraRotation ()
        {
            Vector3 RotationVector
                = Vector3.Normalize(-PlanetCameraController.offset);
            
            Quaternion LookAtTarget
                = Quaternion.LookRotation(
                    RotationVector,
                    PlanetCameraController.FollowTransform.up
                  );

            Vector3 CalculatedEulerAngles = LookAtTarget.eulerAngles;

            PlanetCameraController.desiredEulerRotation = CalculatedEulerAngles;
        }

        private void ResetCameraOffset ()
        {
            Vector3 NewOffset = CalculateNewOffset(LastRotation);
            PlanetCameraController.offset = NewOffset;
        }

        private void ResetCamera ()
        {
            ResetCameraOffset();
            ResetCameraRotation();
        }

        /// <summary>
        /// Keep sliders up-to-date
        /// </summary>
        private void ResetSliders()
        {
            foreach (Slider s in ZoomSliders)
            {
                s.value = DesiredCameraOrbitRadius;
            }
        }
    }
}