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
        public static readonly String NO_IPC_ERRMSG
            = "Player object does not have an Incorporeal Player Controller attached.";
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
        public GameObject UI_manager_prefab;
        public GameObject UI_manager_obj;
        
        // Created by the editor
        public PassthroughNetworkManager network_manager;
        public PassthroughNetworkDiscovery network_discoverer;
        public SpaceShipClassManager spaceship_class_manager;
        
        public bool dont_destroy_on_load;
        public GameObject basic_phaser_bolt_prefab;

        // Code-defined components
        
        public OrbitingBodyBackgroundGameObject current_nearest_orbiting_body;
        public InertialPlayerCameraController player_camera = null;
        public LargeScaleCamera nearest_planet_camera = null;
        public LargeScaleCamera solar_system_camera = null;
        public Light nearest_planet_sunlight;

        private static ProgramInstanceManager instance = null;
        private UIManager UI_manager;
        private IncorporealPlayerController player_controller;
        private List<GameObject> orbital_bodies;
        private ClientState client_state = ClientState.MAIN_MENU;
        private bool warping = false;
        private bool looking_for_game = false;
        private bool found_game = false;
        // TODO: Actually let the player choose their ship class
        private SpaceShipClass player_ship_class_choice_hidden_value;
        private SpaceShipClass player_ship_class_choice
        {
            get
            {
                return player_ship_class_choice_hidden_value;
            }
            set
            {
                player_ship_class_choice_hidden_value = value;
                if (player_controller != null)
                {
                    player_controller.setCurrentShipChoice(value);
                }
            }
        }
        // might need this to avoid garbage collection (maybe I'm just dumb)
        private NetworkClient net_client = null;

        //Awake is always called before any Start functions
        void Awake()
        {
            Debug.Log("Program instance manager awakened");

            //Check if instance already exists
            if (instance == null)
            {
                //if not, set instance to this & do first load operations
                instance = this;
                //Sets this to not be destroyed when reloading scene
                UnityEngine.Object.DontDestroyOnLoad(gameObject);
                UnityEngine.Object.DontDestroyOnLoad(this);
                Debug.Log("Program instance manager prevented from being destroyed on load");

                UI_manager_obj = GameObject.Instantiate(UI_manager_prefab);
                UI_manager = UI_manager_obj.GetComponent<UIManager>();
                // TODO: Let player choose ship class
                player_ship_class_choice = SpaceShipClass.CRUISER;
            }
            else if (instance != this) // If instance already exists and it's not this:
            {
                //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
                Destroy(gameObject);
            }

        }

        void Start ()
        {
            // Register event handlers
            UI_manager.PlayGameButtonPress += startPlayingGame;
            UI_manager.ExitNetGameInputEvent += exitNetworkGame;

            network_discoverer.ServerDetected
                += new PassthroughNetworkDiscovery.ServerDetectedEventHandler(OnServerDetected);
            network_discoverer.Initialize();

            network_manager.LocalPlayerStarted += LocalPlayerControllerCreatedHandler;
        }

        //Initializes the game for each level.
        private void InitGame ()
        {
            
        }

        //Update is called every frame
        void Update()
        {
        }

        /// <summary>
        /// When the incorporeal player controller is created by the server
        /// (the main controller for networked interaction)
        /// 
        /// N.B. Slightly hacky edge-case - I'm leaving deletion of these objects to the scene change
        /// (from online to offline scene)
        /// </summary>
        public void LocalPlayerControllerCreatedHandler (IncorporealPlayerController IPC)
        {
            Debug.Log("Local player object created");

            // Generate local scene (including play space entities)
            // i.e. there are dependencies from here
            InstantiatePlanets();
            InstantiateSunlight();

            // TODO: replace this with a query to server about what planet we are near
            current_nearest_orbiting_body = getPlanet(OrbitingBodyMathematics.ORBITING_BODY.EARTH);
            // this is a Network player controller, not a SpaceBattles player controller!
            this.player_controller = IPC;

            player_controller.setCurrentShipChoice(player_ship_class_choice);
            player_controller.initialiseShipClassManager(spaceship_class_manager);

            // Hook up spaceship spawn event
            player_controller.LocalPlayerShipSpawned += localPlayerShipCreatedHandler;
            // as this is the local player,
            // there is no need to check for existing ship spawns
            // (they would have been observed directly through the RPC in the IPC)
            player_controller.LocalPlayerShipHealthChanged += UI_manager.setCurrentPlayerHealth;

            // Camera setup
            if (nearest_planet_camera == null && player_camera == null && solar_system_camera == null)
            {
                InstantiateCameras(player_controller.transform);
                setCamerasFollowTransform(player_controller.transform);
            }
            warpTo(current_nearest_orbiting_body);
            UI_manager.setPlayerCamera(player_camera.GetComponent<Camera>());
            // Player controller should be set after the camera
            // because the UI manager does some setup afterwards
            UI_manager.setPlayerController(player_controller);
            UI_manager.enteringMultiplayerGame();

            player_controller.CmdSpawnStartingSpaceShip(player_ship_class_choice);
        }

        private void localPlayerShipCreatedHandler (PlayerShipController ship_controller)
        {
            UI_manager.setCurrentPlayerMaxHealth(PlayerShipController.MAX_HEALTH);
            UI_manager.setCurrentPlayerHealth(PlayerShipController.MAX_HEALTH);

            setCamerasFollowTransform(ship_controller.transform);
        }

        public void OnDisconnectedFromServer(NetworkDisconnection info)
        {
            player_controller = null;
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

        private OrbitingBodyBackgroundGameObject getPlanet(OrbitingBodyMathematics.ORBITING_BODY orbiting_body)
        {
            GameObject body_obj = orbital_bodies[(int)orbiting_body];
            var body_background_obj = body_obj.GetComponent<OrbitingBodyBackgroundGameObject>();
            return body_background_obj;
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

        private void InstantiateCameras (Transform initial_follow_transform)
        {
            Debug.Log("Instantiating cameras");
            player_camera = Instantiate(player_camera_prefab);
            nearest_planet_camera = Instantiate(nearest_planet_camera_prefab);
            nearest_planet_camera.using_preset_scale = LargeScaleCamera.PRESET_SCALE.NEAREST_PLANET;
            solar_system_camera = Instantiate(solar_system_camera_prefab);
            solar_system_camera.using_preset_scale = LargeScaleCamera.PRESET_SCALE.SOLAR_SYSTEM;


            // instantiate with default values (arbitrary)
            player_camera.offset
                = spaceship_class_manager.getCameraOffset(player_ship_class_choice);
            setCamerasFollowTransform(initial_follow_transform);

            //TODO: move this activation somewhere else(?)
            setPlayerCamerasActive(true);
            
            Debug.Log("Cameras created?");
        }

        private void setPlayerCamerasActive (bool active)
        {
            player_camera.enabled = active;
            nearest_planet_camera.enabled = active;
            solar_system_camera.enabled = active;
        }

        private void startPlayingGame ()
        {
            Debug.Log("Play game button pressed");
            // shitty lock DO NOT TRUST
            if (!looking_for_game)
            {
                StartCoroutine(playGameCoroutine());
                network_discoverer.StartAsClient();
            }
        }

        IEnumerator playGameCoroutine ()
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
            ClientScene.AddPlayer(0); // this number is scoped to the connection
                                      // i.e. if I only ever want one player
                                      // per connection, this is fine
        }

        private void exitNetworkGame ()
        {
            // I'm not sure if this is always safe to use,
            // as the documentation is very slim
            Debug.Log("Stopping game");
            network_manager.StopHost();
            if (network_discoverer.running)
            {
                network_discoverer.StopBroadcast();
            }
            UI_manager.enteringMainMenu();
        }

        private void setCamerasFollowTransform (Transform follow_transform)
        {
            if (follow_transform == null)
            {
                throw new ArgumentNullException();
            }
            player_camera.followTransform         = follow_transform;
            nearest_planet_camera.followTransform = follow_transform;
            solar_system_camera.followTransform   = follow_transform;
        }

        private void localPlayerShipDestroyedHandler ()
        {
            if (player_controller != null)
            {
                setCamerasFollowTransform(player_controller.transform);
            }
        }

        private void registerHostSpawnHandlers ()
        {

        }

        private void deregisterHostSpawnHandlers ()
        {

        }
    }
}