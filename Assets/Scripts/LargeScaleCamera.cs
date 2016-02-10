﻿using System;
using UnityEngine;
using System.Collections;

namespace SpaceBattles
{
    public class LargeScaleCamera : MonoBehaviour
    {
        public Transform followTransform;

        public Vector3 offset = new Vector3(0f, 2.5f, -5f);
        public float moveSpeed = 1;
        public float turnSpeed = 1;
        public float cameraScale = 1000.0f;
        public Vector3 desiredEulerRotation = new Vector3(0, 0, 0);

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
        }

        void FixedUpdate()
        {
            if (enabled)
            {
                goalPos = (followTransform.position / cameraScale) + followTransform.TransformDirection(offset);
                goalRot = followTransform.rotation * Quaternion.Euler(desiredEulerRotation); // product combines quaternions
                transform.position = Vector3.Lerp(transform.position, goalPos, Time.deltaTime * moveSpeed);
                transform.rotation = Quaternion.Lerp(transform.rotation, goalRot, Time.deltaTime * turnSpeed);
            }
        }
    }
}
