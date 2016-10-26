using System;
using UnityEngine;

namespace SpaceBattles
{
    [Flags]
    public enum CameraRoles
    {
        Player = 0,
        NearestPlanet = 1,
        SolarSystem = 2,
        FixedUi = 4,
        ShipSelection = 8
    }
}

