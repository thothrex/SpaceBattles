using UnityEngine;
using System;
using System.Collections.Generic;       //Allows us to use Lists. 
using UnityEngine.Networking;
using System.Linq;
using System.Collections;

namespace SpaceBattles
{
    public class ProgramInstanceManager : MonoBehaviour
    {
        // Constants
        public static readonly String SOLAR_SYSTEM_LAYER_NAME = "PlanetsMoonsAndStars";
        public static readonly String NEAREST_PLANET_LAYER_NAME = "NearestPlanetScale";
        public static readonly double ORBIT_DISTANCE_IN_METRES = 7000000.0; // 7,000km
        private enum ClientState { MAIN_MENU, MULTIPLAYER_MATCH };

        // Editor-definable components

        // Prefabs
        public InertialPlayerCameraController player_camera_prefab;
        public LargeScaleCamera nearest_planet_camera_prefab;
        public LargeScaleCamera solar_system_camera_prefab;

        public Light nearest_planet_sunlight_prefab;

        // (NB: because each planet has so many unique elements
        //      it's cleaner to have these be editor-defined)
        public GameObject sun_prefab;
        public GameObject mercury_prefab;
        public GameObject venus_prefab;
        public GameObject earth_prefab;
        public GameObject moon_prefab;
        public GameObject mars_prefab;
        public GameObject jupiter_prefab;
        public GameObject saturn_prefab;
        public GameObject uranus_prefab;
        public GameObject neptune_prefab;
        
        public UIManager UI_manager;
        public PassthroughNetworkManager network_manager;
        public PassthroughNetworkDiscovery network_discoverer;
        
        public bool dont_destroy_on_load;
        public GameObject basic_phaser_bolt_prefab;

        // Code-defined components
        private static ProgramInstanceManager instance = null;
        public GameObject player_object;
        public OrbitingBodyBackgroundGameObject current_nearest_orbiting_body;
        private List<GameObject> orbital_bodies;
        public InertialPlayerCameraController player_camera;
        public LargeScaleCamera nearest_planet_camera;
        public LargeScaleCamera solar_system_camera;
        public PlayerShipController player_controller;
        public Light nearest_planet_sunlight;
        private ClientState client_state = ClientState.MAIN_MENU;
        private bool warping = false;
        private bool looking_for_game = false;
        private bool found_game = false;

        // need this to avoid garbage collection!
        private NetworkClient net_client = null;

        //Awake is always called before any Start functions
        void Awake()
        {
            Debug.Log("Program instance manager awakened");

            //Check if instance already exists
            if (instance == null)
            {
                //if not, set instance to this
                instance = this;
            }
            //If instance already exists and it's not this:
            else if (instance != this)
            {
                //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
                Destroy(gameObject);
            }

            if (dont_destroy_on_load)
            {
                //Sets this to not be destroyed when reloading scene
                UnityEngine.Object.DontDestroyOnLoad(gameObject);
                UnityEngine.Object.DontDestroyOnLoad(this);
                Debug.Log("Program instance manager prevented from being destroyed on load");
            }

            // Register event handlers
            UI_manager.PlayGameButtonPress += startPlayingGame;
            
        }

        void Start ()
        {
            network_discoverer.ServerDetected
                += new PassthroughNetworkDiscovery.ServerDetectedEventHandler(OnServerDetected);
            network_discoverer.Initialize();
        }

        //Initializes the game for each level.
        private void InitGame ()
        {
            
        }

        //Update is called every frame
        void Update()
        {
        }

        public void localPlayerStarted (GameObject player_object)
        {
            this.ship_object = player_object;
        }

        /// <summary>
        /// Slightly hacky edge-case - I'm leaving deletion of these objects to the scene change
        /// (from online to offline scene)
        /// </summary>
        public void OnConnectedToServer (NetworkConnection conn)
        {
            Debug.Log("Client has connected to the server");
            InstantiateSunlight();
            InstantiatePlanets();
            // TODO: replace this with a query to server about what planet we are near
            current_nearest_orbiting_body = getPlanet(OrbitingBodyMathematics.ORBITING_BODY.EARTH);
            // this is a Network player controller, not a SpaceBattles player controller!
            this.player_controller = player_controller;            

            UI_manager.setPlayerCamera(player_camera.GetComponent<Camera>());
            UI_manager.setPlayerShip(player_object);
            UI_manager.enteringMultiplayerGame();
            UI_manager.setCurrentPlayerMaxHealth(PlayerShipController.MAX_HEALTH);
            UI_manager.setCurrentPlayerHealth(PlayerShipController.MAX_HEALTH);

            // TODO: This is a bit dirty making them both talk to each other
            //       one should probably be considered authoratative over the other
            //       or communicate through more neutral events (preferable option)
            player_controller.UI_manager = UI_manager;
        }

        void OnDisconnectedFromServer(NetworkDisconnection info)
        {
            UI_manager.enteringMainMenu();
            if (Network.isServer)
            {
                Debug.Log("Local server connection disconnected");
            }
            else
            {
                if (info == NetworkDisconnection.LostConnection)
                {
                    Debug.Log("Lost connection to the server");
                }
                else
                {
                    Debug.Log("Successfully diconnected from the server");
                }
            }
               
        }

        public void setNearestPlanet (OrbitingBodyMathematics.ORBITING_BODY nearest_planet)
        {
            throw new NotImplementedException("setNearestPlanet disabled until it's actually used");
            //current_nearest_orbiting_body = nearest_planet;
        }

        private OrbitingBodyBackgroundGameObject getPlanet(OrbitingBodyMathematics.ORBITING_BODY orbiting_body)
        {
            GameObject body_obj = orbital_bodies[(int)orbiting_body];
            var body_background_obj = body_obj.GetComponent<OrbitingBodyBackgroundGameObject>();
            return body_background_obj;
        }

        public void warpTo(OrbitingBodyMathematics.ORBITING_BODY orbiting_body)
        {
            warpTo(getPlanet(orbiting_body));
        }

        /// <summary>
        /// Warps to warp_target, at ORBIT_DISTANCE in the direction of the sun
        /// (i.e. warps to the sunny side of the target)
        /// </summary>
        /// <param name="warp_target"></param>
        public void warpTo(OrbitingBodyBackgroundGameObject warp_target)
        {
            // shitty lock - DO NOT RELY ON THIS
            if (warping) { Debug.Log("Already warping! Not warping again"); return; }

            warping = true;
            // direction from origin
            Vector3 current_target_vector
                = warp_target
                .getCurrentGameSolarSystemCoordinates();
            Vector3 normalised_target_vector = current_target_vector.normalized;
            
            double orbit_distance_in_solar_system_scale
                = ORBIT_DISTANCE_IN_METRES / OrbitingBodyMathematics.DISTANCE_SCALE_TO_METRES;
            // var name is in capitals - Solar system scale DISTANCE
            float sdistance = System.Convert.ToSingle(
                current_target_vector.magnitude - orbit_distance_in_solar_system_scale
            );
            Vector3 solar_scale_orbit_coordinates = normalised_target_vector * sdistance;

            double orbit_distance_in_nearest_planet_scale
                = ORBIT_DISTANCE_IN_METRES / OrbitingBodyBackgroundGameObject.NEAREST_PLANET_SCALE_TO_METRES;
            // var name is in capitals - Nearest planet scale DISTANCE
            float ndistance = System.Convert.ToSingle(orbit_distance_in_nearest_planet_scale);
            // need to go backwards i.e. towards the sun to be on the sunny side
            Vector3 nearest_planet_scale_orbit_coordinates = -normalised_target_vector * ndistance;

            // Solar System Warps
            current_nearest_orbiting_body.changeToSolarSystemReferenceFrame();
            solar_system_camera.warpTo(solar_scale_orbit_coordinates);

            // Orbital Warps
            warp_target.changeToOrbitalReferenceFrame();
            warp_target.updateSunDirection(nearest_planet_sunlight);
            nearest_planet_camera.warpTo(nearest_planet_scale_orbit_coordinates);

            // Playable Area Warp
            //transform.position = new Vector3(0, 0, 0);
            Debug.Log("TODO: warp player position (maybe?) (CURRENTLY DOES NOTHING)");

            current_nearest_orbiting_body = warp_target;

            warping = false;
        }

        private void InstantiateSunlight()
        {
            nearest_planet_sunlight
                = UnityEngine.Object.Instantiate(nearest_planet_sunlight_prefab);
        }

        private void InstantiatePlanets()
        {
            orbital_bodies = new List<GameObject>();
            int number_of_orbiting_bodies
                = Enum.GetNames(typeof(OrbitingBodyMathematics.ORBITING_BODY)).Length;
            orbital_bodies.Capacity = number_of_orbiting_bodies;

            GameObject sun = Instantiate(sun_prefab);
            GameObject mercury = Instantiate(mercury_prefab);
            GameObject venus = Instantiate(venus_prefab);
            GameObject earth = Instantiate(earth_prefab);
            GameObject moon = Instantiate(moon_prefab);
            moon.transform.parent = earth.transform;
            GameObject mars = Instantiate(mars_prefab);
            GameObject jupiter = Instantiate(jupiter_prefab);
            GameObject saturn = Instantiate(saturn_prefab);
            GameObject uranus = Instantiate(uranus_prefab);
            GameObject neptune = Instantiate(neptune_prefab);

            orbital_bodies.Add(sun);
            orbital_bodies.Add(mercury);
            orbital_bodies.Add(venus);
            orbital_bodies.Add(earth);
            orbital_bodies.Add(moon);
            orbital_bodies.Add(mars);
            orbital_bodies.Add(jupiter);
            orbital_bodies.Add(saturn);
            orbital_bodies.Add(uranus);
            orbital_bodies.Add(neptune);

            Debug.Assert(orbital_bodies.Count == number_of_orbiting_bodies);
        }

        private void InstantiateCameras()
        {
            Debug.Log("Instantiating cameras");
            player_camera = Instantiate(player_camera_prefab);
            nearest_planet_camera = Instantiate(nearest_planet_camera_prefab);
            nearest_planet_camera.using_preset_scale = LargeScaleCamera.PRESET_SCALE.NEAREST_PLANET;
            solar_system_camera = Instantiate(solar_system_camera_prefab);
            solar_system_camera.using_preset_scale = LargeScaleCamera.PRESET_SCALE.SOLAR_SYSTEM;

            player_camera.followTransform = player_object.transform;
            // instantiate with default values (arbitrary)
            player_camera.offset = new Vector3(0, 7, -30);
            nearest_planet_camera.followTransform = player_object.transform;
            solar_system_camera.followTransform = player_object.transform;

            player_camera.enabled = true;
            nearest_planet_camera.enabled = true;
            solar_system_camera.enabled = true;
            Debug.Log("Cameras created?");
        }

        public void startPlayingGame()
        {
            Debug.Log("Play game button pressed");
            // shitty lock DO NOT TRUST
            if (!looking_for_game)
            {
                StartCoroutine(playGameCoroutine());
                network_discoverer.StartAsClient();
            }
        }

        IEnumerator playGameCoroutine()
        {
            // shitty lock DO NOT TRUST
            looking_for_game = true;
            found_game = false;
            yield return new WaitForSeconds(1.0f);
            if (found_game) { yield break; }
            else
            {
                network_discoverer.StopBroadcast();
                Debug.Log("Game not found - starting server");
                UI_manager.setPlayerConnectState(
                    UIManager.PlayerConnectState.CREATING_SERVER
                );
                net_client = network_manager.StartHost();
                network_discoverer.StartAsServer();
            }
            looking_for_game = false;
        }

        public void OnServerDetected (string fromAddress, string data)
        {
            Debug.Log("Server detected at address " + fromAddress);
            // I don't even know if this will do anything
            if (net_client != null) return;
            found_game = true;
            // TODO: implement properly
            // wait 1 second for more games
            UI_manager.setPlayerConnectState(
                UIManager.PlayerConnectState.JOINING_SERVER
            );
            network_discoverer.StopBroadcast();
            network_manager.networkAddress = fromAddress;
            net_client = network_manager.StartClient();
        }

    }
}