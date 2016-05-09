using UnityEngine;
using System;
using System.Collections.Generic;       //Allows us to use Lists. 
using UnityEngine.Networking;

namespace SpaceBattles
{
    public class ClientManager : MonoBehaviour
    {
        public OrbitingBodyBackgroundGameObject current_nearest_orbiting_body;

        public static readonly double orbit_distance = 7000000.0 / OrbitingBodyBackgroundGameObject.NEAREST_PLANET_SCALE_TO_METRES; // 7,000km
        public static readonly String SOLAR_SYSTEM_LAYER_NAME = "PlanetsMoonsAndStars";
        public static readonly String NEAREST_PLANET_LAYER_NAME = "NearestPlanetScale";
        public InertialPlayerCamera player_camera;
        public LargeScaleCamera solar_system_camera;
        public LargeScaleCamera nearest_planet_camera;
        public PlayerShipController player_controller;
        public Light sunlight;
        public UIManager UI_manager;


        private enum ClientState { MAIN_MENU, MULTIPLAYER_MATCH };
        private ClientState client_state = ClientState.MAIN_MENU;
        private bool warping = false;

        //Awake is always called before any Start functions
        void Awake()
        {
            Console.WriteLine("Game manager awakened");

            //Sets this to not be destroyed when reloading scene
            DontDestroyOnLoad(gameObject);

            //Call the InitGame function to initialize the first level 
            InitGame();
        }

        //Initializes the game for each level.
        void InitGame ()
        {
            
        }

        //Update is called every frame
        void Update()
        {
        }

        public void setNearestPlanet (OrbitingBodyMathematics.ORBITING_BODY nearest_planet)
        {
            current_nearest_orbiting_body = nearest_planet;

        }

        public void warpTo(OrbitingBodyBackgroundGameObject warp_target)
        {
            if (warping) { print("Already warping! Not warping again"); return; }

            warping = true;
            // Basically does the warp conversions for each of the frames of reference
            Vector3 planet_direction_vector = warp_target.transform.position.normalized; // direction from origin
            float distance = System.Convert.ToSingle(warp_target.transform.position.magnitude - ORBIT_DISTANCE);
            Vector3 orbit_coordinates
                = planet_direction_vector
                * distance
                * System.Convert.ToSingle(OrbitingBodyMathematics.DISTANCE_SCALE_TO_METRES);

            // Solar System Warps
            current_nearest_orbiting_body.changeToSolarSystemReferenceFrame();
            solar_system_camera.warpTo(orbit_coordinates);

            // Orbital Warps
            warp_target.changeToOrbitalReferenceFrame();
            warp_target.updateSunDirection(sunlight);
            //nearest_planet_camera.warpTo(orbit_coordinates);

            // Playable Area Warp
            transform.position = new Vector3(0, 0, 0);

            current_nearest_orbiting_body = warp_target;

            warping = false;
        }
    }
}