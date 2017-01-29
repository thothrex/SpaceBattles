using System;
using UnityEngine;

namespace SpaceBattles
{
    public class OrreryUIManager : MonoBehaviour
    {
        // -- Fields --
        public DateTimePicker DateTimePicker;
        public ScalePicker ScalePicker;

        private readonly float ZoomButtonIncrementFactor = 0.02f;
        private readonly float FullRotation = Convert.ToSingle(Math.PI * 2);
        private OptionalEventModule oem = new OptionalEventModule();
        private Camera PlanetCamera = null;
        private InertialCameraController PlanetCameraController = null;
        private Vector2 LastRotation = new Vector2(0, 0);
        private float DesiredCameraOrbitRadius = 4000;
        private Vector2 DesiredAngles;

        // -- Delegates --
        public delegate void ExplicitDateTimeSetHandler (DateTime newTime);
        public delegate void LinearScaleSetHandler (float scale);
        public delegate void LogScaleSetHandler (float logBase, float innerMultiplier, float outerMultiplier);

        // -- Events --
        public event ExplicitDateTimeSetHandler DateTimeSet;
        public event LinearScaleSetHandler PlanetLinearScaleSet;
        public event LogScaleSetHandler PlanetLogarithmicScaleSet;
        
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
        CalculateNewCameraPosition
            (Vector2 inputAngles, float desiredOrbitRadius)
        {
            //Debug.Log(
            //    "Received desiredAngles: "
            //    + desiredAngles.ToString()
            //);

            float DesiredXAxisRotation = ConvertToRadians(inputAngles.y);
            Vector3 NewPosition = new Vector3(0, 0, 0);
            NewPosition.z
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
            float DesiredZAxisRotation = ConvertToRadians(inputAngles.x);
            NewPosition.x
                = Convert.ToSingle(Math.Sin(DesiredZAxisRotation))
                * UpperRadius;
            NewPosition.y
                = Convert.ToSingle(Math.Cos(DesiredZAxisRotation))
                * UpperRadius;

            return NewPosition;
        }

        public static float ConvertToRadians(float degrees)
        {
            return Convert.ToSingle(degrees * (Math.PI / 180.0));
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

            //PlanetCameraController.offset.z = DesiredCameraOrbitRadius;
            ResetCameraTransform();
        }

        public void IncrementZoom (bool positive)
        {
            MyContract.RequireFieldNotNull(
                PlanetCameraController, "PlanetCameraController"
            );
            
            int sign = positive ? 1 : -1;
            DesiredCameraOrbitRadius
                += (DesiredCameraOrbitRadius * ZoomButtonIncrementFactor * sign);

            //PlanetCameraController.offset.z = DesiredCameraOrbitRadius;
            ResetCameraTransform();
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
                Vector3 NewPosition
                    = CalculateNewCameraPosition(
                        desiredAngles,
                        DesiredCameraOrbitRadius
                      );
                PlanetCameraController.offset = NewPosition;
                ResetCameraTransform();
                LastRotation = desiredAngles;
            }
        }

        private void ResetCameraTransform ()
        {
            Vector3 PCCDesiredPosition
                    = PlanetCameraController.offset;
            //Debug.Log(
            //    "Desired Position: "
            //    + PCCDesiredPosition
            //);
            Vector3 PCCTargetPosition
                = PlanetCameraController.FollowTransform.position;
            //Debug.Log(
            //    "Target Position: "
            //    + PCCTargetPosition
            //);

            Vector3 RotationVector
                = Vector3.Normalize(PCCTargetPosition - PCCDesiredPosition);
            Quaternion Rotation = Quaternion.LookRotation(RotationVector, Vector3.forward);
            Vector3 CalculatedEulerAngles = Rotation.eulerAngles;
            //CalculatedEulerAngles.z -= 90;
            
            PlanetCameraController.desiredEulerRotation = CalculatedEulerAngles;
        }
    }
}