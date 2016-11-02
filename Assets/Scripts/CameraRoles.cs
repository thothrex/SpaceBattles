using System;
using UnityEngine;

namespace SpaceBattles
{
    [Flags]
    public enum CameraRoles
    {
        None = 0,
        NearestPlanet = 1,
        SolarSystem = 2,
        FixedUi = 4,
        ShipSelection = 8,
        MainMenuAndOrrery = 16,
        Player = 32,

        GameplayCameras = NearestPlanet | SolarSystem | Player | FixedUi
    }
}

