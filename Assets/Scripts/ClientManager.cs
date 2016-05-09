using UnityEngine;
using System;
using System.Collections.Generic;       //Allows us to use Lists. 
using UnityEngine.Networking;

namespace SpaceBattles
{
    public class ClientManager : NetworkClient
    {
        public OrbitingBodyBackgroundGameObject current_nearest_orbiting_body;

        public GameObject gameObject;
        public InertialPlayerCamera player_camera_prefab;
        public LargeScaleCamera solar_system_camera_prefab;
        public LargeScaleCamera nearest_planet_camera_prefab;

        public static readonly double ORBIT_DISTANCE
            = 7000000.0 / OrbitingBodyBackgroundGameObject.NEAREST_PLANET_SCALE_TO_METRES; // 7,000km
        
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
            Debug.Log("Client manager awakened");

            //Sets this to not be destroyed when reloading scene
            UnityEngine.Object.DontDestroyOnLoad(gameObject);

            RegisterHandler(MsgType.Connect, OnConnectedToServer);

            //Call the InitGame function to initialize the first level 
            InitGame();
        }

        //Initializes the game for each level.
        void InitGame ()
        {
            
        }

        /// <summary>
        /// Slightly hacky edge-case - I'm leaving deletion of these objects to the scene change
        /// (from online to offline scene)
        /// </summary>
        /// <param name="netMsg"></param>
        public void OnConnectedToServer(NetworkMessage netMsg)
        {
            player_camera = UnityEngine.Object.Instantiate(player_camera_prefab);
            nearest_planet_camera = UnityEngine.Object.Instantiate(nearest_planet_camera_prefab);
            solar_system_camera = UnityEngine.Object.Instantiate(solar_system_camera_prefab);
        }

        //Update is called every frame
        void Update()
        {
        }

        public void setNearestPlanet (OrbitingBodyMathematics.ORBITING_BODY nearest_planet)
        {
            Debug.Log("Trying to set nearest planet (DOES NOTHING)");
            //current_nearest_orbiting_body = nearest_planet;

        }

        public void warpTo(OrbitingBodyBackgroundGameObject warp_target)
        {
            if (warping) { Debug.Log("Already warping! Not warping again"); return; }

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
            //transform.position = new Vector3(0, 0, 0);
            Debug.Log("TODO: warp player position (maybe?) (CURRENTLY DOES NOTHING)");

            current_nearest_orbiting_body = warp_target;

            warping = false;
        }
    }
}