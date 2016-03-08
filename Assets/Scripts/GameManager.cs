using UnityEngine;
using System;
using System.Collections.Generic;       //Allows us to use Lists. 

namespace SpaceBattles
{
    public class GameManager : MonoBehaviour
    {
        public OrbitingBodyBackgroundGameObject current_nearest_orbiting_body;
        public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.
        public static readonly double orbit_distance = 7000000.0 / OrbitingBodyBackgroundGameObject.NEAREST_PLANET_SCALE_TO_METRES; // 7,000km
        public static readonly String SOLAR_SYSTEM_LAYER_NAME = "PlanetsMoonsAndStars";
        public static readonly String NEAREST_PLANET_LAYER_NAME = "NearestPlanetScale";
        public LargeScaleCamera solar_system_camera;
        public LargeScaleCamera nearest_planet_camera;
        public PlayerController player_controller;
        public Light sunlight;
        public UIManager UI_manager;

        private bool warping = false;

        //Awake is always called before any Start functions
        void Awake()
        {
            //Check if instance already exists
            if (instance == null)

                //if not, set instance to this
                instance = this;

            //If instance already exists and it's not this:
            else if (instance != this)

                //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
                Destroy(gameObject);

            //Sets this to not be destroyed when reloading scene
            DontDestroyOnLoad(gameObject);

            //Call the InitGame function to initialize the first level 
            InitGame();
        }

        //Initializes the game for each level.
        void InitGame ()
        {
        }

        public void warpTo (OrbitingBodyBackgroundGameObject warp_target)
        {
            if (warping) { print("Already warping! Not warping again"); return; }

            warping = true;
            // Basically does the warp conversions for each of the frames of reference
            Vector3 planet_direction_vector = warp_target.transform.position.normalized; // direction from origin
            float distance = Convert.ToSingle(warp_target.transform.position.magnitude - orbit_distance);
            Vector3 orbit_coordinates
                = planet_direction_vector
                * distance
                * Convert.ToSingle(OrbitingBodyMathematics.DISTANCE_SCALE_TO_METRES);

            // Solar System Warps
            current_nearest_orbiting_body.changeToSolarSystemReferenceFrame();
            solar_system_camera.warpTo(orbit_coordinates);

            // Orbital Warps
            warp_target.changeToOrbitalReferenceFrame();
            warp_target.updateSunDirection(sunlight);
            //nearest_planet_camera.warpTo(orbit_coordinates);

            // Playable Area Warp
            player_controller.warp();

            current_nearest_orbiting_body = warp_target;

            warping = false;
        }

        //Update is called every frame
        void Update()
        {
            if (Input.GetKeyDown("escape"))
            {
                UI_manager.toggleInGameMenu();
            }
        }
    }
}