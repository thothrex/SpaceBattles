using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBattles
{
    /// <summary>
    /// Should be used as an empty object which stays in the scene.
    /// Attach it to the PIM as a Unity "module" (am I using that right?)
    /// </summary>
    public class SpaceShipClassManager : MonoBehaviour
    {
        // Set in the editor
        public List<GameObject> SpaceShipPrefabs;
        public List<Vector3> CameraTransformOffsets;
        public List<Vector3> CameraEulerRotationOffsets;

        public GameObject getSpaceShipPrefab(SpaceShipClass ss_class)
        {
            return SpaceShipPrefabs[(int)ss_class];
        }

        public Vector3 getCameraOffset(SpaceShipClass ss_class)
        {
            return CameraTransformOffsets[(int)ss_class];
        }

        public Vector3 getCameraEulerRotation (SpaceShipClass ss_class)
        {
            return CameraEulerRotationOffsets[(int)ss_class];
        }
    }
}
