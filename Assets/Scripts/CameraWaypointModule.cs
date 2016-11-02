using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBattles
{
    public class CameraWaypointModule : MonoBehaviour
    {
        // -- Const Fields --
        private const string NO_WAYPOINT_EXCMSG
            = "has at least one waypoint registered";

        // -- Fields --
        public Camera CameraToMove;
        [Tooltip("These are expected to be in order")]
        public List<Transform> Waypoints;

        //private static readonly float AcceptableTransformProximity = 0.001f;
        private static readonly float DefaultMovementDuration = 1.5f;
        private bool moving = false;
        private Vector3 InitialLocation;
        private Vector3 CurrentTarget;
        private float TimeElapsedMoving = 0.0f;
        private float TotalMovementDuration = 0.0f;

        // -- Methods --
        public void Update ()
        {
            if (moving)
            {
                TimeElapsedMoving += Time.deltaTime;
                if (TimeElapsedMoving >= TotalMovementDuration)
                {
                    moving = false;
                }
                else
                {
                    float Progress = TimeElapsedMoving
                                   / TotalMovementDuration;
                    CameraToMove.transform.position
                        = Vector3.Lerp(InitialLocation,
                                       CurrentTarget,
                                       Progress);
                }
            }
        }

        public void ReturnToStart ()
        {
            MyContract.RequireFieldNotNull(Waypoints, "Waypoints");
            MyContract.RequireFieldNotNull(CameraToMove, "CameraToMove");
            MyContract.RequireField(Waypoints.Count > 0,
                                    NO_WAYPOINT_EXCMSG,
                                    "Waypoints");
            InertialCameraController ICC
                = CameraToMove.GetComponent<InertialCameraController>();
            ICC.FollowTransform = Waypoints[0];
        }

        /*
        private void MoveToLocation (Vector3 targetLocation, float duration)
        {
            InitialLocation = CameraToMove.transform.position;
            CurrentTarget = targetLocation;
            TotalMovementDuration = duration;
            TimeElapsedMoving = 0.0f;
            moving = true;
        }
        */
    }
}

