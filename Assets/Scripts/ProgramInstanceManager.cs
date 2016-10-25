using UnityEngine;
using System;
using System.Collections.Generic;       //Allows us to use Lists. 
using UnityEngine.Networking;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;

namespace SpaceBattles
{
    public class ProgramInstanceManager : MonoBehaviour
    {
        // -- Constants/Readonly --
        public static readonly String NO_IPC_ERRMSG
            = "Player object does not have an Incorporeal Player Controller attached.";
        public static readonly String SOLAR_SYSTEM_LAYER_NAME = "PlanetsMoonsAndStars";
        public static readonly String NEAREST_PLANET_LAYER_NAME = "NearestPlanetScale";
        public static readonly double ORBIT_DISTANCE_IN_METRES = 7000000.0; // 7,000km

        // -- Fields --
        // Editor-definable components
        // Prefabs
        public InertialPlayerCameraController player_camera_prefab;
        public LargeScaleCamera nearest_planet_camera_prefab;
        public LargeScaleCamera solar_system_camera_prefab;
        public Light nearest_planet_sunlight_prefab;
        public GameObject basic_phaser_bolt_prefab;

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
        
        // Created by the editor
        public PassthroughNetworkManager NetworkManager;
        public PassthroughNetworkDiscovery NetworkDiscoverer;
        public SpaceShipClassManager spaceship_class_manager;
        public GameObject UI_manager_obj;
        public bool dont_destroy_on_load;
        

        // Code-defined components
        public OrbitingBodyBackgroundGameObject current_nearest_orbiting_body;
        public InertialPlayerCameraController player_camera = null;
        public LargeScaleCamera nearest_planet_camera = null;
        public LargeScaleCamera solar_system_camera = null;
        public Light nearest_planet_sunlight;

        private static ProgramInstanceManager instance = null;

        private UIManager UIManager;
        private IncorporealPlayerController player_controller;
        private List<GameObject> orbital_bodies;
        private ClientState client_state = ClientState.MAIN_MENU;
        private bool warping = false;
        private bool looking_for_game = false;
        private bool found_game = false;
        private bool OrreryLoaded = false;
        // Hopefully protected by the SceneLoadLock
        private bool SceneLoadInProgress = false;
        private System.Object SceneLoadLock = new System.Object();
        // TODO: Actually let the player choose their ship class
        private SpaceShipClass player_ship_class_choice_hidden_value;
        // might need this to avoid garbage collection (maybe I'm just dumb)
        private NetworkClient net_client = null;

        // -- Delegates --
        public delegate void SceneLoadedCallback();

        // -- Events --

        // -- Enums --
        private enum ClientState { MAIN_MENU, MULTIPLAYER_MATCH };

        // -- Properties --
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
                UIManager = UI_manager_obj.GetComponent<UIManager>();
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
            UIManager.EnterOrreryInputEvent += EnterOrrery;
            UIManager.ExitProgramInputEvent += ExitProgram;
            UIManager.ExitNetGameInputEvent += ExitNetworkGame;
            UIManager.PlayGameButtonPress += startPlayingGame;
            UIManager.PitchInputEvent += handlePitchInput;
            UIManager.RollInputEvent += handleRollInput;
            

            NetworkDiscoverer.ServerDetected
                += new PassthroughNetworkDiscovery.ServerDetectedEventHandler(OnServerDetected);
            NetworkDiscoverer.Initialize();

            NetworkManager.LocalPlayerStarted += LocalPlayerControllerCreatedHandler;
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
            player_controller.LocalPlayerShipHealthChanged += UIManager.setCurrentPlayerHealth;

            // Camera setup
            if (nearest_planet_camera == null && player_camera == null && solar_system_camera == null)
            {
                InstantiateCameras(player_controller.transform);
                setCamerasFollowTransform(player_controller.transform);
            }
            warpTo(current_nearest_orbiting_body);
            UIManager.setPlayerCamera(player_camera.GetComponent<Camera>());
            // Player controller should be set after the camera
            // because the UI manager does some setup afterwards
            UIManager.setPlayerController(player_controller);
            UIManager.enteringMultiplayerGame();

            player_controller.CmdSpawnStartingSpaceShip(player_ship_class_choice);
        }

        private void localPlayerShipCreatedHandler (PlayerShipController ship_controller)
        {
            UIManager.setCurrentPlayerMaxHealth(PlayerShipController.MAX_HEALTH);
            UIManager.setCurrentPlayerHealth(PlayerShipController.MAX_HEALTH);

            setCamerasFollowTransform(ship_controller.transform);
        }

        public void OnDisconnectedFromServer (NetworkDisconnection info)
        {
            player_controller = null;
            UIManager.EnterMainMenuRoot();
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

        /// <summary>
        /// Exiting during a scene load can apparently cause a nasty crash
        /// </summary>
        public void ExitProgram ()
        {
#if UNITY_EDITOR
            Debug.Log("Exiting program");
#endif
            if (!SceneLoadInProgress)
            {
                Application.Quit();
            }
            else
            {
                StartCoroutine(ExitProgramAfterSceneLoadCoroutine());
            }
        }

        public void EnterOrrery ()
        {
            if (!OrreryLoaded)
            {
                Debug.Log("Loading Orrery");
                StartCoroutine(
                    SceneLoadedCallbackCoroutine(
                        SceneIndex.ORRERY,
                        InitialiseOrreryScene
                    )
                );
            }
            else
            {
                Debug.Log("Swapping to Orrery");
            }
        }

        public void setNearestPlanet (OrbitingBodyMathematics.ORBITING_BODY nearest_planet)
        {
            throw new NotImplementedException("setNearestPlanet disabled until it's actually used");
            //current_nearest_orbiting_body = nearest_planet;
        }

        public void OnServerDetected(string fromAddress, string data)
        {
            Debug.Log("Server detected at address " + fromAddress);
            // I don't even know if this will do anything
            if (net_client != null) return;
            found_game = true;
            // TODO: implement properly
            // wait 1 second for more games
            UIManager.SetPlayerConnectState(
                UIManager.PlayerConnectState.JOINING_SERVER
            );
            NetworkDiscoverer.StopBroadcast();
            NetworkManager.networkAddress = fromAddress;
            net_client = NetworkManager.StartClient();
            ClientScene.AddPlayer(0); // this number is scoped to the connection
                                      // i.e. if I only ever want one player
                                      // per connection, this is fine
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
                .GetCurrentGameSolarSystemCoordinates();
            Vector3 normalised_target_vector = current_target_vector.normalized;
            
            double orbit_distance_in_solar_system_scale
                = ORBIT_DISTANCE_IN_METRES / OrbitingBodyMathematics.DISTANCE_SCALE_TO_METRES;
            // var name is in capitals - Solar system scale DISTANCE
            float sdistance = System.Convert.ToSingle(
                current_target_vector.magnitude - orbit_distance_in_solar_system_scale
            );
            Vector3 solar_scale_orbit_coordinates = normalised_target_vector * sdistance;

            double orbit_distance_in_nearest_planet_scale
                = ORBIT_DISTANCE_IN_METRES / Scale.NearestPlanet.MetresMultiplier();
            // var name is in capitals - Nearest planet scale DISTANCE
            float ndistance = System.Convert.ToSingle(orbit_distance_in_nearest_planet_scale);
            // need to go backwards i.e. towards the sun to be on the sunny side
            Vector3 nearest_planet_scale_orbit_coordinates = -normalised_target_vector * ndistance;

            // Solar System Warps
            current_nearest_orbiting_body.ChangeToSolarSystemReferenceFrame();
            solar_system_camera.WarpTo(solar_scale_orbit_coordinates);

            // Orbital Warps
            warp_target.ChangeToOrbitalReferenceFrame();
            warp_target.UpdateSunDirection(nearest_planet_sunlight);
            nearest_planet_camera.WarpTo(nearest_planet_scale_orbit_coordinates);

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
            nearest_planet_camera.PresetScale = Scale.NearestPlanet;
            solar_system_camera = Instantiate(solar_system_camera_prefab);
            solar_system_camera.PresetScale = Scale.SolarSystem;


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
                NetworkDiscoverer.StartAsClient();
            }
        }

        private IEnumerator
        SceneLoadedCallbackCoroutine
        (SceneIndex sceneIndex, SceneLoadedCallback callback)
        {
            lock (SceneLoadLock)
            {
                SceneLoadInProgress = true;
                AsyncOperation SceneLoad
                    = SceneManager.LoadSceneAsync((int)sceneIndex, LoadSceneMode.Additive);
                ConfirmSceneLoadNotNull(sceneIndex, SceneLoad);
                yield return new WaitUntil(() => SceneLoad.isDone);
            }
            
            callback();
        }

        private void InitialiseOrreryScene ()
        {
            Debug.Log("Orrery Scene Loaded");
        }

        private void
        ConfirmSceneLoadNotNull
        (SceneIndex sceneIndex, AsyncOperation SceneLoad)
        {
            if (SceneLoad == null)
            {
                if (SceneManager.sceneCount < (int)sceneIndex)
                {
                    throw new ArgumentOutOfRangeException(
                        "sceneIndex",
                        sceneIndex,
                        CreateInsufficientScenesExceptionMessage(
                            SceneManager.sceneCount, (int)sceneIndex
                        )
                    );
                }
                else
                {
                    throw new InvalidOperationException(
                        "LoadSceneAsync returned null"
                    );
                }
            }
        }

        private IEnumerator playGameCoroutine ()
        {
            // shitty lock DO NOT TRUST
            looking_for_game = true;
            found_game = false;
            yield return new WaitForSeconds(1.0f);
            if (found_game) { yield break; }
            else
            {
                NetworkDiscoverer.StopBroadcast();
                Debug.Log("Game not found - starting server");
                UIManager.SetPlayerConnectState(
                    UIManager.PlayerConnectState.CREATING_SERVER
                );
                net_client = NetworkManager.StartHost();
                NetworkDiscoverer.StartAsServer();
            }
            looking_for_game = false;
        }

        private IEnumerator ExitProgramAfterSceneLoadCoroutine ()
        {
            yield return new WaitWhile(() => SceneLoadInProgress);
            Application.Quit();
        }

        private void ExitNetworkGame ()
        {
            // I'm not sure if this is always safe to use,
            // as the documentation is very slim
            Debug.Log("Stopping game");
            NetworkManager.StopHost();
            if (NetworkDiscoverer.running)
            {
                NetworkDiscoverer.StopBroadcast();
            }
            UIManager.EnterMainMenuRoot();
        }

        private void setCamerasFollowTransform (Transform follow_transform)
        {
            if (follow_transform == null)
            {
                throw new ArgumentNullException();
            }
            player_camera.followTransform         = follow_transform;
            nearest_planet_camera.FollowTransform = follow_transform;
            solar_system_camera.FollowTransform   = follow_transform;
        }

        private void localPlayerShipDestroyedHandler ()
        {
            if (player_controller != null)
            {
                setCamerasFollowTransform(player_controller.transform);
            }
        }

        private void handlePitchInput (float pitch_input)
        {
            if (player_controller != null)
            {
                player_controller.setPitch(pitch_input);
            }
        }

        private void handleRollInput(float roll_input)
        {
            if (player_controller != null)
            {
                player_controller.setRoll(roll_input);
            }
        }

        private string
        CreateInsufficientScenesExceptionMessage
            (int buildScenes, int sceneIndex)
        {
            return "There are fewer scenes specified in the project's "
                 + "build settings than the index of the scene you are "
                 + "trying to load.\n"
                 + "Total Scenes: "
                 + buildScenes.ToString()
                 + "\tAttempted Loading Scene Index: "
                 + sceneIndex.ToString();
        }
    }
}