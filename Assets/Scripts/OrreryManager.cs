﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBattles
{
    public class OrreryManager : MonoBehaviour
    {
        // -- Const Fields --
        private static readonly string PlanetManagerName
            = "A planet's OrbitingBodyBackgroundGameObject";

        // -- Fields --
        public float CameraRadius;
        public float CameraAspectRatio; // width / height
        public float CameraVerticalFieldOfView;
        public Transform MapViewCameraWaypoint;
        public List<GameObject> Planets;
        public CameraWaypointModule CameraWaypointModule;

        private Camera MainCamera = null;
        private GameObjectRegistry PlanetRegistry
            = new GameObjectRegistry();
        
        // -- Methods --
        public void Start ()
        {
            PlanetRegistry.RegisterObjects(Planets);
        }

        public void UseProvidedCameraAsMainCamera (Camera providedCamera)
        {
            MainCamera = providedCamera;
            CameraWaypointModule.CameraToMove = providedCamera;
            CameraWaypointModule.ReturnToStart();
        }

        public void SetExplicitDateTime (DateTime explicitTime)
        {
            Debug.Log("Setting Explicit DateTime in OrreryManager");
            foreach (GameObject PlanetObj in Planets)
            {
                OrbitingBodyBackgroundGameObject OBBGO
                    = PlanetObj.GetComponent<OrbitingBodyBackgroundGameObject>();
                MyContract.RequireFieldNotNull(
                    OBBGO, "OrbitingBodyBackgroundGameObject"
                );
                OBBGO.UseExplicitDateTime(explicitTime);
            }
        }

        public void SetLinearScale (float scaleMultiplier)
        {
            ChangePlanetsScale(Planets, scaleMultiplier);
        }

        public void
        SetLogarithmicScale
            (float logBase, float innerMultiplier, float outerMultiplier)
        {
            ChangePlanetsScale(
                Planets,
                logBase,
                innerMultiplier,
                outerMultiplier
            );
        }

        public Transform GetOrbitingBodyTransform (OrbitingBody target)
        {
            return PlanetRegistry[(int)target].transform;
        }

        private Vector3 GenerateMainOrreryCameraPosition ()
        {
            double MinimumCameraHeight
                = CalculateCameraHeightByRadius(CameraVerticalFieldOfView,
                                                CameraAspectRatio,
                                                CameraRadius);
            Vector3 CurrentCentreVector = MapViewCameraWaypoint.position;

            return new Vector3(CurrentCentreVector.x,
                               Convert.ToSingle(MinimumCameraHeight),
                               CurrentCentreVector.z);
        }

        /// <summary>
        /// Calculating how to ensure the camera sees planets with
        /// an orbit distance &lt; radius from a given aspect ratio
        /// and vertical field of view
        /// </summary>
        /// <param name="verticalFieldOfView"></param>
        /// <param name="aspectRatio"></param>
        /// <param name="targetRadius"></param>
        /// <returns></returns>
        private double 
        CalculateCameraHeightByRadius
            (double verticalFieldOfView,
             double aspectRatio,
             double targetRadius)
        {
            double VerticalFieldOfViewInRadians
                = verticalFieldOfView * (Math.PI / 180.0);
            double MinimumHeightFromVerticalFieldOfView
                = targetRadius / Math.Tan(0.5 * verticalFieldOfView);
            // In Unity this is a derived value so I'm also taking this as
            // a derived value here rather than an explicit parameter
            double HorizontalFieldOfView
                = verticalFieldOfView
                * aspectRatio
                * (Math.PI / 180.0);
            double MinimumHeightFromHorizontalFieldOfView
                = targetRadius / Math.Tan(0.5 * HorizontalFieldOfView);
            double MinimumCameraHeight
                = Math.Max(MinimumHeightFromHorizontalFieldOfView,
                           MinimumHeightFromVerticalFieldOfView);

            return MinimumCameraHeight;
        }

        private void
        ChangePlanetsScale
            (List<GameObject> planets, float targetLinearScale)
        {
            foreach (GameObject Planet in planets)
            {
                OrbitingBodyBackgroundGameObject PlanetManager
                    = Planet.GetComponent<OrbitingBodyBackgroundGameObject>();
                MyContract.RequireArgumentNotNull(PlanetManager, PlanetManagerName);
                PlanetManager.SetRelativeLinearScaleExplicitly(targetLinearScale);
            }
        }

        private void
        ChangePlanetsScale
            (List<GameObject> planets,
             float logBase,
             float innerMultiplier,
             float outerMultiplier)
        {
            foreach (GameObject Planet in planets)
            {
                OrbitingBodyBackgroundGameObject PlanetManager
                    = Planet.GetComponent<OrbitingBodyBackgroundGameObject>();
                MyContract.RequireArgumentNotNull(
                    PlanetManager, PlanetManagerName
                );
                PlanetManager.SetRelativeLogarithmicScaleExplicitly(
                    logBase, innerMultiplier, outerMultiplier
                );
            }
        }

        private void
        ChangePlanetsScale
            (List<GameObject> planets, Scale targetScale)
        {
            foreach (GameObject Planet in planets)
            {
                OrbitingBodyBackgroundGameObject PlanetManager
                    = Planet.GetComponent<OrbitingBodyBackgroundGameObject>();
                MyContract.RequireArgumentNotNull(PlanetManager, PlanetManagerName);
                PlanetManager.SetScaleToPredefinedScale(targetScale);
            }
        }
    }
}


