
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;       //Allows us to use Lists. 
using UnityEngine;
using UnityEngine.Networking;
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
        public List<Camera> CameraPrefabs;
        public Light nearest_planet_sunlight_prefab;
        public GameObject basic_phaser_bolt_prefab;
        public List<GameObject> PlanetPrefabs;
        public GameObject UI_manager_prefab;
        
        // Created by the editor
        public PassthroughNetworkManager NetworkManager;
        public PassthroughNetworkDiscovery NetworkDiscoverer;
        public SpaceShipClassManager SpaceshipClassManager;
        public GameObject UI_manager_obj;
        public bool dont_destroy_on_load;
        public float GameFinderSearchDuration = 1.5f;

        // Code-defined components
        public OrbitingBodyBackgroundGameObject CurrentNearestOrbitingBody;
        public Light nearest_planet_sunlight;

        private readonly float MinimumLoadScreenDisplayDuration = 3.0f;

        private static ProgramInstanceManager instance = null;

        private bool warping = false;
        /// <summary>
        /// Might need to change this to an int or timestamp
        /// so that I can compare whether or not the game that was found
        /// was the same as the one I was looking for
        /// 
        /// e.g. A super-delayed OnServerDetected thread could detect that 
        /// FoundGame was false, yet this was because the 7th game I've played
        /// today has finished, and I started that thread after the 3rd game.
        /// Maybe I didn't want to start another game after that 7th game but
        /// the thread would detect FoundGame as false and try to connect
        /// to a server with the information it received, which is probably
        /// out-of-date by then.
        /// </summary>
        private bool FoundGame = false;
        private bool OrreryLoaded = false;
        // Hopefully protected by the SceneLoadLock
        private bool SceneLoadInProgress = false;

        private UIManager UIManager;
        private NetworkedPlayerController PlayerController;
        private ClientState client_state = ClientState.MAIN_MENU;
        private System.Object SceneLoadLock = new System.Object();
        private System.Object LookingForGameLock = new System.Object();
        private System.Object FoundGameLock = new System.Object();
        // TODO: Actually let the player choose their ship class
        private SpaceShipClass PlayerShipClassChoiceBackingValue;
        // might need this to avoid garbage collection (maybe I'm just dumb)
        private NetworkClient NetClient = null;
        private GameObjectRegistry PlanetRegistry
            = new GameObjectRegistry();
        private CameraRegistry CameraRegistry
            = new CameraRegistry();

        private bool FadeToBlackComplete = false;
        private bool ServerConnectionComplete = false;
        private bool LoadScreenComplete = false;

        // DEBUG
        //private GameObject OrreryCamera = null;

        // -- Delegates --
        public delegate void SceneLoadedCallback();

        // -- Events --

        // -- Enums --
        private enum ClientState { MAIN_MENU, MULTIPLAYER_MATCH };

        // -- Properties --
        private SpaceShipClass PlayerShipClassChoice
        {
            get
            {
                return PlayerShipClassChoiceBackingValue;
            }
            set
            {
                PlayerShipClassChoiceBackingValue = value;
                if (PlayerController != null)
                {
                    PlayerController.setCurrentShipChoice(value);
                }
            }
        }

        public bool FinishedLoading { get; private set; }

        //Awake is always called before any Start functions
        public void Awake()
        {
            //Debug.Log("Program instance manager awakened");

            //Check if instance already exists
            if (instance == null)
            {
                //if not, set instance to this & do first load operations
                instance = this;
                FinishedLoading = false;
                //Sets this to not be destroyed when reloading scene
                UnityEngine.Object.DontDestroyOnLoad(gameObject);
                UnityEngine.Object.DontDestroyOnLoad(this);
                //Debug.Log("Program instance manager prevented from being destroyed on load");
                
                UI_manager_obj = GameObject.Instantiate(UI_manager_prefab);
                UIManager = UI_manager_obj.GetComponent<UIManager>();
                // TODO: Let player choose ship class
                PlayerShipClassChoice = SpaceShipClass.FIGHTER;

                // The UIManager is dependant on these before it starts AFAIK
                //Debug.Log("Instantiating cameras");
                CameraRegistry.PersistThroughScenes = true;
                CameraRegistry.KeyEnum = typeof(CameraRoles);
                CameraRegistry.InitialiseAndRegisterGenericPrefabs(CameraPrefabs);
                CameraRegistry.ActivateAllGameObjects(false);
                CameraRegistry.CoroutineHost = this;

                // DEBUG
                //OrreryCamera = CameraRegistry[(int)CameraRoles.MainMenuAndOrrery];
            }
            else if (instance != this) // If instance already exists and it's not this:
            {
                //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
                Destroy(gameObject);
            }
        }

        public void Start ()
        {
            // Register event handlers
            UIManager.EnterOrreryInputEvent.AddListener(EnterOrrery);
            UIManager.ExitProgramInputEvent.AddListener(ExitProgram);
            UIManager.ExitNetGameInputEvent.AddListener(OnExitNetworkGameInput);
            UIManager.PlayGameButtonPress.AddListener(startPlayingGame);
            UIManager.PitchInputEvent += handlePitchInput;
            UIManager.RollInputEvent += handleRollInput;
            UIManager.CameraDestroyed += OnCameraDestroyed;

            Camera MainMenuAndOrreryCamera
                = CameraRegistry[(int)CameraRoles.MainMenuAndOrrery];
            UIManager.ProvideCamera(MainMenuAndOrreryCamera);
            // DEBUG
            DontDestroyOnLoad(MainMenuAndOrreryCamera.gameObject);
            // END DEBUG
            
            // Can be removed if the build works fine without it
            //bool Initialised = NetworkDiscoverer.Initialize();
            //if (!Initialised)
            //{
            //    throw new Exception("NetworkDiscoverer failed to Initialise");
            //}
            NetworkDiscoverer.ServerDetected
                += new PassthroughNetworkDiscovery
                      .ServerDetectedEventHandler(OnServerDetected);

            NetworkManager.LocalPlayerStarted += LocalPlayerControllerCreatedHandler;
            NetworkManager.ClientDisconnected += OnClientDisconnect;
            FinishedLoading = true;

            // debug
            // TODO: remove
            // Debug
            object DebugCheckObject
                = CameraRegistry[(int)CameraRoles.MainMenuAndOrrery];
            string OutString
                = "MainMenuAndOrreryCamera registry entry is ";
            if (DebugCheckObject != null)
            {
                Debug.Log(OutString + "not null");
                GameObject DebugCheckGameObject
                    = CameraRegistry[(int)CameraRoles.MainMenuAndOrrery].gameObject;
                Debug.Log("MainMenuAndOrreryCamera GameObject is "
                        + (DebugCheckGameObject == null ? "null" : "not null"));
            }
            else
            {
                Debug.Log(OutString + "null");
            }
        }

        /// <summary>
        /// When the incorporeal player controller is created by the server
        /// (the main controller for networked interaction)
        /// 
        /// N.B. Slightly hacky edge-case - I'm leaving deletion of these objects to the scene change
        /// (from online to offline scene)
        /// </summary>
        public void LocalPlayerControllerCreatedHandler (NetworkedPlayerController IPC)
        {
            Debug.Log("Local player object created");
            GenerateLocalScene(); // N.B. there are dependencies from here
            InitialisePlayerController(IPC);
            SetupGameCameras(PlayerController.transform);
            SetupInGameUI(IPC);
            PlayerController.CmdSpawnStartingSpaceShip(PlayerShipClassChoice);
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
                // Debug
                object DebugCheckObject
                    = CameraRegistry[(int)CameraRoles.MainMenuAndOrrery];
                string OutString
                    = "MainMenuAndOrreryCamera registry entry is ";
                if (DebugCheckObject != null)
                {
                    Debug.Log(OutString + "not null");
                    GameObject DebugCheckGameObject
                        = CameraRegistry[(int)CameraRoles.MainMenuAndOrrery].gameObject;
                    Debug.Log("MainMenuAndOrreryCamera GameObject is "
                            + (DebugCheckGameObject == null ? "null" : "not null"));
                }
                else
                {
                    Debug.Log(OutString + "null");
                }
                
                StartCoroutine(
                    SceneLoadedCallbackCoroutine(
                        SceneIndex.Orrery,
                        LoadOrreryScene
                    )
                );
            }
            else
            {
                Debug.Log("Swapping to already-loaded Orrery");
            }
            UIManager.EnteringOrrery();
        }

        public void setNearestPlanet (OrbitingBody nearest_planet)
        {
            throw new NotImplementedException("setNearestPlanet disabled until it's actually used");
            //current_nearest_orbiting_body = nearest_planet;
        }

        public void OnServerDetected (string fromAddress, string data)
        {
            Debug.Log("Server detected at address " + fromAddress);
            // I don't even know if this will do anything
            if (NetClient != null) return;
            lock (FoundGameLock)
            {
                Debug.Log("Acquired FoundGameLock");
                if (!FoundGame) // If we are the first responder
                {
                    FoundGame = true;
                    NetworkManager.networkAddress = fromAddress;
                    BeginEnterOnlineScene(delegate ()
                    {
                        Debug.Log("PIM: Server detected - joining as a client");
                        NetClient = NetworkManager.StartClient();
                        // this number is scoped to the connection
                        // i.e. if I only ever want one player
                        // per connection, this is fine
                            ClientScene.AddPlayer(NetClient.connection, 0);
                        if (NetClient.connection != null) // non-local connection
                        {
                            NetClient.connection.logNetworkMessages = true;
                        }
                    });
                    NetworkDiscoverer.StopBroadcast();
                }
            }
            Debug.Log("Released FoundGameLock");
        }

        public void warpTo (OrbitingBody orbiting_body)
        {
            WarpTo(getPlanet(orbiting_body));
        }

        /// <summary>
        /// Warps to warp_target, at ORBIT_DISTANCE in the direction of the sun
        /// (i.e. warps to the sunny side of the target)
        /// </summary>
        /// <param name="warpTarget"></param>
        public void WarpTo(OrbitingBodyBackgroundGameObject warpTarget)
        {
            // shitty lock - DO NOT RELY ON THIS
            if (warping) { Debug.Log("Already warping! Not warping again"); return; }

            warping = true;
            // direction from origin
            Vector3 current_target_vector
                = warpTarget
                .GetCurrentGameSolarSystemCoordinates();
            Vector3 normalised_target_vector = current_target_vector.normalized;
            
            double orbit_distance_in_solar_system_scale
                = ORBIT_DISTANCE_IN_METRES / OrbitingBodyMathematics.DISTANCE_SCALE_TO_METRES;
            // var name is in capitals - Solar system scale DISTANCE
            float sdistance = System.Convert.ToSingle(
                current_target_vector.magnitude - orbit_distance_in_solar_system_scale
            );
            Vector3 SolarScaleOrbitCoordinates = normalised_target_vector * sdistance;

            double orbit_distance_in_nearest_planet_scale
                = Scale.NearestPlanet
                .ConvertMeasurementFromMetres(ORBIT_DISTANCE_IN_METRES);
            // var name is in capitals - Nearest planet scale DISTANCE
            float ndistance = System.Convert.ToSingle(orbit_distance_in_nearest_planet_scale);
            // need to go backwards i.e. towards the sun to be on the sunny side
            Vector3 NearestPlanetScaleOrbitCoordinates
                = -normalised_target_vector * ndistance;

            // Solar System Warps
            CurrentNearestOrbitingBody.ChangeToSolarSystemReferenceFrame();
            CameraRegistry[(int)CameraRoles.SolarSystem]
                .GetComponent<LargeScaleCamera>()
                .WarpTo(SolarScaleOrbitCoordinates);

            // Orbital Warps
            warpTarget.ChangeToOrbitalReferenceFrame();
            warpTarget.UpdateSunDirection(nearest_planet_sunlight);
            CameraRegistry[(int)CameraRoles.NearestPlanet]
                .GetComponent<LargeScaleCamera>()
                .WarpTo(NearestPlanetScaleOrbitCoordinates);

            // Playable Area Warp
            //transform.position = new Vector3(0, 0, 0);
            //Debug.Log("TODO: warp player position (maybe?) (CURRENTLY DOES NOTHING)");

            CurrentNearestOrbitingBody = warpTarget;

            warping = false;
        }

        /// <summary>
        /// Called when this client disconnects from the server
        /// </summary>
        public void OnClientDisconnect()
        {
            PlayerController = null;
            ExitNetworkGame(false);
        }

        public void OnCameraDestroyed (CameraRoles role)
        {
            // If we have a good copy, use that
            if (CameraRegistry.Contains(role))
            {
                Camera OurCamera = CameraRegistry[role];
                if (OurCamera != null)
                {
                    UIManager.ProvideCamera(OurCamera);
                }
            }

            // If we don't have a good copy,
            // try to find the prefab and build a new one
            foreach (Camera cam in CameraPrefabs)
            {
                CameraIdentifier ID = cam.GetComponent<CameraIdentifier>();
                MyContract.RequireFieldNotNull(ID, "Camera Identifier");
                CameraRoles PrefabRole = ID.Role;
                if (ID.Equals(role))
                {
                    List<Camera> ArgList = new List<Camera>();
                    ArgList.Add(cam);
                    CameraRegistry.InitialiseAndRegisterGenericPrefabs(ArgList);
                    Camera CreatedCamera = CameraRegistry[role];
                    UIManager.ProvideCamera(CreatedCamera);
                    return;
                }
            }
            
            // If we don't have the prefab, we can't do anything
        }

        private OrbitingBodyBackgroundGameObject
        getPlanet(OrbitingBody orbitingBody)
        {
            GameObject BodyObj = PlanetRegistry[(int)orbitingBody];
            var BodyBackgroundObj
                = BodyObj.GetComponent<OrbitingBodyBackgroundGameObject>();
            return BodyBackgroundObj;
        }

        private void InstantiateSunlight()
        {
            nearest_planet_sunlight
                = UnityEngine.Object.Instantiate(nearest_planet_sunlight_prefab);
        }

        private void ActivatePlanets()
        {
            if (PlanetRegistry.Count() == 0)
            {
                PlanetRegistry
                    .InitialiseAndRegisterGenericPrefabs(PlanetPrefabs);
            }
            else
            {
                int[] BodiesToActivate = {
                    (int)OrbitingBody.SUN,
                    (int)OrbitingBody.EARTH,
                    (int)OrbitingBody.MOON,
                    (int)OrbitingBody.MARS
                };

                PlanetRegistry.ActivateGameObjects(true, BodiesToActivate);
            }
        }

        private void
        SetupGameCameras
            (Transform initialFollowTransform)
        {
            //Debug.Log("Setting-up game cameras");
            MyContract.RequireFieldNotNull(
               SpaceshipClassManager, "SpaceshipClassManager"
            );

            // instantiate with initial values
            Vector3 CameraOffset
                = SpaceshipClassManager
                .getCameraOffset(PlayerShipClassChoice);
            InertialCameraController PlayerCamController
                = CameraRegistry[(int)CameraRoles.Player]
                .GetComponent<InertialCameraController>();
            LargeScaleCamera NearestPlanetCamController
                = CameraRegistry[(int)CameraRoles.NearestPlanet]
                .GetComponent<LargeScaleCamera>();
            LargeScaleCamera SolarSystemCamController
                = CameraRegistry[(int)CameraRoles.SolarSystem]
                .GetComponent<LargeScaleCamera>();

            PlayerCamController.offset = CameraOffset;
            NearestPlanetCamController.CameraOffset = CameraOffset;
            SolarSystemCamController.CameraOffset = CameraOffset;

            // These shouldn't be different,
            // but they're manually set in the (Unity) editor
            // so the (human) editor could be lazy about it
            // and only change one setting
            // and forget the other 2
            float MoveSpeed = PlayerCamController.moveSpeed;
            NearestPlanetCamController.MoveSpeed = MoveSpeed;
            SolarSystemCamController.MoveSpeed   = MoveSpeed;
            float TurnSpeed = PlayerCamController.turnSpeed;
            NearestPlanetCamController.TurnSpeed = TurnSpeed;
            SolarSystemCamController.TurnSpeed = TurnSpeed;

            CameraRegistry.SetAllFollowTransforms(initialFollowTransform);

            WarpTo(CurrentNearestOrbitingBody);
        }

        private void SetPlayerCamerasActive (bool active)
        {
            CameraRegistry.ActivateGameObjects(
                active,
                (int)CameraRoles.Player,
                (int)CameraRoles.NearestPlanet,
                (int)CameraRoles.SolarSystem
            );
        }

        private void startPlayingGame ()
        {
            Debug.Log("Play game button pressed");
            // Relies on the below coroutine
            // checking the lock states appropriately
            StartCoroutine(PlayGameCoroutine());
            // Initialize needs to be called every time before a
            // StartAsClient or StartAsServer is called,
            // i.e. we need to re-initialize after stopping.
            bool initialized = NetworkDiscoverer.Initialize();
            if (!initialized)
            {
                Debug.LogWarning("NetworkDiscoverer failed to initialize (presumably due to the desired port being unavailable)");
            }
            NetworkDiscoverer.StartAsClient();
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
                SceneLoadInProgress = false;
                callback();
            }
        }

        private void LoadOrreryScene ()
        {
            Debug.Log("Orrery Scene Loading");
            UIManager.InitialiseOrreryUi();
            Scene OrreryScene
                = SceneManager.GetSceneByName(SceneIndex.Orrery.SceneName());
            Scene SceneSwappingFrom = SceneManager.GetActiveScene();
            CameraRegistry.EnsureObjectsStayAlive();
            SceneManager.SetActiveScene(OrreryScene);
            AsyncOperation Unloading = SceneManager.UnloadSceneAsync(SceneSwappingFrom);
            UIManager.TransitionToUIElements(
                UiElementTransitionType.Tracked,
                UIElements.OrreryUI
            );
            // Debug
            //DebugCheckObject
            //    = CameraRegistry[(int)CameraRoles.MainMenuAndOrrery].gameObject;
            //Debug.Log("MainMenuAndOrreryCamera is "
            //        + (DebugCheckObject == null ? "null" : "not null")
            //        + "\nCamera Registry: "
            //        + CameraRegistry.PrintDebugDestroyedRegisteredObjectCheck()
            //        + "\nPlanet Registry: "
            //        + PlanetRegistry.PrintDebugDestroyedRegisteredObjectCheck()
            //        + "\nUIManager is "
            //        + (UIManager == null ? "null" : "not null"));
            //GameObject OrreryCameraAfter
            //    = GameObject.Find("Main Menu Background Camera(Clone)");
            //Debug.Log(( OrreryCameraAfter == null ? "Could not find" : "Found")
            //          + " the main menu background/orrery camera");
            //UIManager.DebugLogRegistryStatus();
            // end debug
            UIManager.CameraTransition(CameraRoles.FixedUi
                                     | CameraRoles.MainMenuAndOrrery);
            GameObject OrreryManagerHost = GameObject.Find("OrreryManager");
            OrreryManager OrreryManager
                = OrreryManagerHost.GetComponent<OrreryManager>();
            UIManager.SetOrreryManager(OrreryManager);

            Camera MainMenuBackgroundCamera
                = CameraRegistry[(int)CameraRoles.MainMenuAndOrrery]
                .GetComponent<Camera>();
            MyContract.RequireFieldNotNull(MainMenuBackgroundCamera,
                                           "MainMenuBackgroundCamera");
            OrreryManager.UseProvidedCameraAsMainCamera(
                MainMenuBackgroundCamera
            );
        }

        private void TransitionToOrreryScene () { }

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

        /// <summary>
        /// This method needs to check locks
        /// as it could be called many times erroneously
        /// </summary>
        /// <returns></returns>
        private IEnumerator PlayGameCoroutine ()
        {
            // Lock probably not necessary
            // but hopefully guarantees sensible behaviour of LookingForGame
            lock (LookingForGameLock)
            {
                //Debug.Log("Acquired LookingForGameLock");
                bool FoundGameEarly = false;
                lock (FoundGameLock)
                {
                    //Debug.Log("Acquired FoundGameLock");
                    FoundGameEarly = FoundGame;
                }
                //Debug.Log("Released FoundGameLock");
                if (!FoundGameEarly)
                {
                    yield return new WaitForSeconds(GameFinderSearchDuration);
                    lock (FoundGameLock)
                    {
                        //Debug.Log("Acquired FoundGameLock");
                        if (!FoundGame)
                        {
                            NetworkDiscoverer.StopBroadcast();
                            Debug.Log("Game not found - starting server");
                            FoundGame = true;
                            BeginEnterOnlineScene(StartOnlineGameServerConnection);
                        }
                    }
                    //Debug.Log("Released FoundGameLock");
                }
            }
            //Debug.Log("Released LookingForGameLock");
        }

        /// <summary>
        /// Makes this program instance a game server
        /// </summary>
        private void StartOnlineGameServerConnection ()
        {
            Debug.Log("PIM: Start server callback");
            NetClient = NetworkManager.StartHost();
            // We need to re-init this due to stopping the client search
            bool initialized = NetworkDiscoverer.Initialize();
            if (!initialized)
            {
                Debug.LogWarning("NetworkDiscoverer failed to initialize (presumably due to the desired port being unavailable)");
            }
            NetworkDiscoverer.StartAsServer();
        }

        private IEnumerator ExitProgramAfterSceneLoadCoroutine ()
        {
            yield return new WaitWhile(() => SceneLoadInProgress);
            Application.Quit();
        }

        private void OnExitNetworkGameInput ()
        {
            ExitNetworkGame(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stopHost">
        /// Should be true if we need to stop the hosting aspects of the program
        /// </param>
        private void ExitNetworkGame (bool stopHost)
        {
            lock (FoundGameLock)
            {
                FoundGame = false;
                // I'm not sure if this is always safe to use,
                // as the documentation is very slim
                Debug.Log("Stopping game");
                SetPlayerCamerasActive(false);
                if (stopHost)
                {
                    GameStateManager GSM = GameStateManager.FindCurrentGameManager();
                    MyContract.RequireFieldNotNull(GSM, "Game State Manager");
                    NetworkManager.PlayerDisconnected -= GSM.OnPlayerDisconnect;
                    NetworkManager.StopHost();
                }
                if (NetworkDiscoverer.running)
                {
                    // This stops both server broadcast and client listening
                    NetworkDiscoverer.StopBroadcast();
                }
                UIManager.SetPlayerConnectState(
                    UIManager.PlayerConnectState.IDLE
                );
                UIManager.EnterMainMenuRoot();
            }
        }

        private void localPlayerShipDestroyedHandler ()
        {
            if (PlayerController != null)
            {
                CameraRegistry.SetAllFollowTransforms(PlayerController.transform);
            }
        }

        private void handlePitchInput (float pitch_input)
        {
            if (PlayerController != null)
            {
                PlayerController.setPitch(pitch_input);
            }
        }

        private void handleRollInput(float roll_input)
        {
            if (PlayerController != null)
            {
                PlayerController.setRoll(roll_input);
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

        /// <summary>
        /// This function covers the aesthetic aspect of the change in scenes,
        /// with the passed-in function (doConnect) representing the
        /// actual connection logic
        /// </summary>
        /// <param name="doConnect"></param>
        private void BeginEnterOnlineScene(Action doConnect)
        {
            UIManager.SetPlayerConnectState(
                UIManager.PlayerConnectState.JOINING_SERVER
            );
            FadeToBlackComplete = false;
            ServerConnectionComplete = false;
            LoadScreenComplete = false;
            FoundGame = true;
            UIManager.FadeCamera(true, EnterOnlineSceneFadeOutComplete(doConnect));
            Debug.Log("Fading out due to BeginEnterOnlineScene");
            NetworkManager.LocalPlayerStarted += EnterOnlineSceneConnectionComplete;
        }

        private Action EnterOnlineSceneFadeOutComplete (Action doConnect)
        {
            return delegate ()
            {
                FadeToBlackComplete = true;

                doConnect();

                UIManager.TransitionToUIElements(
                    UiElementTransitionType.Fresh,
                    UIElements.MultiplayerLoadingScreen
                );
                UIManager.PauseUITransitions(true);
                CameraRegistry.ActivateGameObject((int)CameraRoles.MainMenuAndOrrery, false);
                UIManager.FadeCamera(false, null);
                Debug.Log("Fading in due to EnterOnlineSceneFadeOutComplete");
                StartCoroutine(MinimumLoadScreenDurationCoroutine());
                Debug.Log("PIM: Fade completed");
                FinishEnterOnlineSceneIfReady();
            };
        }

        private IEnumerator MinimumLoadScreenDurationCoroutine ()
        {
            yield return new WaitForSecondsRealtime(
                MinimumLoadScreenDisplayDuration
            );
            LoadScreenComplete = true;
            Debug.Log("PIM: Load screen completed");
            Debug.Log("Fading out due to MinimumLoadScreenDuration");
            UIManager.FadeCamera(true, FinishEnterOnlineSceneIfReady);
        }

        private void
        EnterOnlineSceneConnectionComplete
            (NetworkedPlayerController playerController)
        {
            ServerConnectionComplete = true;
            NetworkManager.LocalPlayerStarted -= EnterOnlineSceneConnectionComplete;
            Debug.Log("PIM: Online connection complete");
            GameStateManager GSM = GameStateManager.FindCurrentGameManager();
            MyContract.RequireFieldNotNull(GSM, "Game State Manager");
            NetworkManager.PlayerDisconnected += GSM.OnPlayerDisconnect;
            FinishEnterOnlineSceneIfReady();
        }

        private void FinishEnterOnlineSceneIfReady ()
        {
            Debug.Log("PIM: in FinishEnterOnlineSceneIfReady");
            if (ServerConnectionComplete
            &&  FadeToBlackComplete
            &&  LoadScreenComplete)
            {
                FinishEnterOnlineScene();
            }
        }

        private void FinishEnterOnlineScene ()
        {
            Debug.Log("PIM: Finishing online scene entry");
            UIManager.PauseUITransitions(false);
            CameraRegistry.ActivateAllGameObjects(true);
            UIManager.TransitionToUIElements(
                UiElementTransitionType.Fresh,
                UIElements.GameplayUI
            );
            Debug.Log("Fading in due to FinishEnterOnlineScene");
            UIManager.FadeCamera(false, null);
        }

        private void SetupInGameUI (NetworkedPlayerController NPC)
        {
            UIManager.SetPlayerController(PlayerController);
            UIManager.EnteringMultiplayerGame(NetworkManager.networkAddress);
            NPC.EventScoreUpdated += UIManager.OnScoreUpdate;
            NPC.EventPlayerRemovedFromScoreboard
                += UIManager.OnPlayerRemovedFromScoreboard;
            NPC.LocalPlayerShipDestroyed += delegate (PlayerIdentifier killer)
            {
                UIManager.OnLocalPlayerShipDestroyed(killer, NPC.RespawnDelay);
            };
            NPC.CmdSendScoreboardStateToUI();
        }

        private void GenerateLocalScene()
        {
            PlanetRegistry.PersistThroughScenes = true;
            ActivatePlanets();
            InstantiateSunlight();

            // TODO: replace this with a query to server about what planet we are near
            CurrentNearestOrbitingBody = getPlanet(OrbitingBody.EARTH);
        }

        private void InitialisePlayerController(NetworkedPlayerController IPC)
        {
            this.PlayerController = IPC;

            PlayerController.setCurrentShipChoice(PlayerShipClassChoice);
            PlayerController.initialiseShipClassManager(SpaceshipClassManager);

            // Hook up spaceship spawn event
            PlayerController.LocalPlayerShipSpawned += localPlayerShipCreatedHandler;
            PlayerController.LocalPlayerShipSpawned += UIManager.OnLocalPlayerShipSpawned;
            // as this is the local player,
            // there is no need to check for existing ship spawns
            // (they would have been observed directly through the RPC in the IPC)
            PlayerController.LocalPlayerShipHealthChanged += UIManager.SetCurrentPlayerHealth;
        }

        private void localPlayerShipCreatedHandler(PlayerShipController ship_controller)
        {
            CameraRegistry.SetAllFollowTransforms(ship_controller.transform);
        }
    }
}