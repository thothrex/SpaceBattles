using System;
using UnityEngine;

namespace SpaceBattles
{
    public class CameraIdentifier : MonoBehaviour, IGameObjectRegistryKeyComponent
    {
        // -- Fields --
        [Tooltip("Expected to be only a single role")]
        public CameraRoles Role;

        // -- Properties --
        public int Key
        {
            get
            {
                return (int)Role;
            }
        }
    }
}

