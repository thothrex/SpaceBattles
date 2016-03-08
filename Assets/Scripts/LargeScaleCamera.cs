using System;
using UnityEngine;
using System.Collections;

namespace SpaceBattles
{
    public class LargeScaleCamera : MonoBehaviour
    {
        public Transform followTransform;

        public Vector3 camera_offset = new Vector3(0f, 2.5f, -5f);
        // this is the "warp" adjustment
        // relative to the warped origin
        public Vector3 reference_frame_internal_warp = new Vector3(0, 0, 0);
        // this is what origin the camera's scale is based around
        // relative to any input coordinates
        public Vector3 reference_frame_origin_warp = new Vector3(0, 0, 0);
        public float moveSpeed = 100f;
        public float turnSpeed = 1f;
        public float cameraScale = 1;
        public Vector3 desiredEulerRotation = new Vector3(0, 0, 0);

        public enum PRESET_SCALE { NONE, SOLAR_SYSTEM, NEAREST_PLANET }
        public PRESET_SCALE using_preset_scale;

        Vector3 goalPos;
        Quaternion goalRot;

        // Use this for initialization
        void Start()
        {
            if (!followTransform)
            {
                followTransform = GameObject.FindGameObjectWithTag("Player").transform;
                if (!followTransform) // if no tagged object exists
                    this.enabled = false;
            }

            if (using_preset_scale != PRESET_SCALE.NONE)
            {
                switch (using_preset_scale)
                {
                    case PRESET_SCALE.SOLAR_SYSTEM:
                        cameraScale = Convert.ToSingle(OrbitingBodyMathematics.DISTANCE_SCALE_TO_METRES);
                        break;
                    case PRESET_SCALE.NEAREST_PLANET:
                        cameraScale = Convert.ToSingle(OrbitingBodyBackgroundGameObject.NEAREST_PLANET_SCALE_TO_METRES);
                        break;
                }
            }
        }

        void FixedUpdate()
        {
            if (enabled)
            {
                goalPos = followTransform.TransformDirection(camera_offset) + (followTransform.position / cameraScale) + reference_frame_internal_warp;
                goalRot = followTransform.rotation * Quaternion.Euler(desiredEulerRotation); // product combines quaternions
                transform.position = goalPos;
                transform.rotation = Quaternion.Lerp(transform.rotation, goalRot, Time.deltaTime * turnSpeed);
            }
        }

        /// <summary>
        /// Pass in coordinates in base units
        /// </summary>
        /// <param name="warp_coordinates"></param>
        public void warpTo(Vector3 warp_coordinates)
        {
            reference_frame_internal_warp = warp_coordinates / cameraScale;
        }
    }
}
