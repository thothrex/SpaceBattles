using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace SpaceBattles
{
    public class LargeScaleCamera : MonoBehaviour
    {
        public Transform FollowTransform;

        public Vector3 CameraOffset = new Vector3(0f, 2.5f, -5f);
        // this is the "warp" adjustment
        // relative to the warped origin
        public Vector3 ReferenceFrameInternalWarp = new Vector3(0, 0, 0);
        // this is what origin the camera's scale is based around
        // relative to any input coordinates
        public Vector3 ReferenceFrameOriginWarp = new Vector3(0, 0, 0);
        public float MoveSpeed = 100f;
        public float TurnSpeed = 1f;
        public float CameraScale = 1;
        public Vector3 DesiredEulerRotation = new Vector3(0, 0, 0);

        public bool UsingPresetScale;
        public Scale PresetScale;

        Vector3 GoalPosition;
        Quaternion GoalRotation;

        public void Start ()
        {
            if (!FollowTransform)
            {
                this.enabled = false;
            }

            if (UsingPresetScale)
            {
                CameraScale
                    = Convert.ToSingle(PresetScale.MetresMultiplier());
            }
        }

        void FixedUpdate ()
        {
            // could also use null-conditional
            // of the form FollowTransform?.rotation
            if (enabled && FollowTransform != null)
            {
                GoalPosition
                    = FollowTransform.TransformDirection(CameraOffset)
                    + (FollowTransform.position / CameraScale)
                    + ReferenceFrameInternalWarp;
                GoalRotation
                    = FollowTransform.rotation
                    * Quaternion.Euler(DesiredEulerRotation);
                transform.position
                    = GoalPosition;
                transform.rotation
                    = Quaternion.Lerp(transform.rotation,
                                      GoalRotation, 
                                      Time.deltaTime * TurnSpeed);
            }
        }

        /// <summary>
        /// Pass in coordinates in base units
        /// </summary>
        /// <param name="warpCoordinates"></param>
        public void WarpTo (Vector3 warpCoordinates)
        {
            ReferenceFrameInternalWarp = warpCoordinates / CameraScale;
        }
    }
}
