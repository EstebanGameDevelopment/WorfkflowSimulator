using Crosstales.FB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using yourvrexperience.ai;
using yourvrexperience.Networking;
using yourvrexperience.UserManagement;
using yourvrexperience.Utils;
using yourvrexperience.VR;
using static yourvrexperience.WorkDay.AskBaseaCreateSessionChatGPTHTTP;

namespace yourvrexperience.WorkDay
{
    public class ApplicationController : MonoBehaviour
    {
        public const bool DebugMode = true;

        public const float DELAY_SECONDS_FOR_WARMUP_PROCESS_IN_SERVER = 30f;

        public const string PLAYERPREFS_LOCAL_ENCRYPTION = "sEcREt-fTay-UseR-MaNAgEmeNT";
        public const string EncryptionLocalAESKey = "TKerBtVopIiIZZcy";

        public const string EventMainControllerReleaseGameResources = "EventMainControllerReleaseGameResources";

        public const string EventMainControllerChangeState = "EventMainControllerChangeState";
        public const string EventMainControllerRequestState = "EventMainControllerRequestState";
        public const string EventMainControllerResponseState = "EventMainControllerResponseState";
        public const string EventMainControllerRequestLoadLevel = "EventMainControllerRequestLoadLevel";
        public const string EventMainControllerResponseLoadLevel = "EventMainControllerResponseLoadLevel";
        public const string EventMainControllerChangeCurrentLevel = "EventMainControllerChangeCurrentLevel";

        public const string EventMainControllerAllPlayerViewReadyToStartGame = "EventMainControllerAllPlayerViewReadyToStartGame";
        public const string EventMainControllerGameReadyToStart = "EventMainControllerGameReadyToStart";
        public const string EventMainControllerDelayedServerInited = "EventMainControllerDelayedServerInited";

        public const string EventMainControllerSelectedHuman = "EventMainControllerSelectedHuman";

        public const string SubEventMainControllerDisconnectionConfirmation = "SubEventMainControllerDisconnectionConfirmation";

        public enum StatesGame { None = 0, Splash, Download, MainMenu, Settings, EditOptions, PlayOptions, Network, Connecting, Loading, Run, ReleaseMemory }
        public enum ImagesIndex { Uploading = 0, Uploaded, TaskCompleted, NewDay }

        private static ApplicationController _instance;

        public static ApplicationController Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType(typeof(ApplicationController)) as ApplicationController;
                }
                return _instance;
            }
        }

        [SerializeField] private WorkDayData workDayData;
        [SerializeField] private AssetsCatalogData assetsCatalogData;
        [SerializeField] private GameAIData gameAIData;
        [SerializeField] private PromptController promptController;
        [SerializeField] private FormData formData;
        [SerializeField] private GameObject CameraFade;
        [SerializeField] private GameObject desktopPlayer;
        [SerializeField] private GameObject timeHUD;
        [SerializeField] private GameObject selectionBox;
        [SerializeField] private Light sunLight;
        [SerializeField] private Sprite[] images;

        public FormData FormData
        {
            get { return formData; }
        }

        public Light SunLight
        {
            get { return sunLight; }
        }

        private PlayerView _playerView;
        private LevelView _levelView;
        private Dictionary<PlayerView, int> _players = new Dictionary<PlayerView, int>();
        private bool _requestCreation = false;
        private string _roomName = "default_room";
        private bool _hasStartedSession = false;

        private IBasicState _gameState;
        private IInputController _inputController;
        private StatesGame _state;
        private StatesGame _previousState;
        private CameraFader _cameraFader;

        private bool _serverAIInited = false;
        private bool _inputInited = false;
        private bool _screenInited = false;
        private bool _isMultiplayer = false;
        private bool _isFreeMode = true;
        private int _numberClients = 1;
        private bool _isPlayMode = false;

        private TimeHUD _timeHUD;
        private GameObject _selectionBox;
        private string _eventReportSelectedFile;

        private HumanView _selectedHuman;
        private HumanView _humanPlayer;
        
        private bool _isSocialMeetingEnabled = false;
        private bool _isCasualMeetingEnabled = false;
        private bool _isInterruptionMeetingEnabled = false;
        private bool _isInfoScreenDisplayed = false;
        private int _totalNumberOfCommands = 0;
        private int _totalNumberOfAICommands = 0;
        private Color _lastProjectColor = Color.white;
        private string _lastProjectFeedback = "";

        private TeamCompanyListJSON _teamCompany = null;

        private bool _exitAfterSave = false;

        public PlayerView PlayerView
        {
            get { return _playerView; }
        }
        public LevelView LevelView
        {
            get { return _levelView; }
        }        
        public StatesGame State
        {
            get { return _state; }
        }
        public StatesGame PreviousState
        {
            get { return _previousState; }
        }
        public bool IsMultiplayer
        {
            get { return _isMultiplayer; }
            set { _isMultiplayer = value; }
        }
        public bool IsPlayMode
        {
            get { return _isPlayMode; }
            set { _isPlayMode = value; }
        }        
        public int NumberClients
        {
            get { return _numberClients; }
            set { _numberClients = value; }
        }
        public bool HasStartedSession
        {
            get { return _hasStartedSession; }
            set { _hasStartedSession = value; }
        }
        public string RoomName
        {
            get { return _roomName; }
            set { _roomName = value; }
        }
        public bool IsFreeMode
        {
            get { return _isFreeMode; }
        }
        public TimeHUD TimeHUD
        {
            get { return _timeHUD; }
        }
        public GameObject SelectionBox
        {
            get
            {
                if (_selectionBox == null)
                {
                    _selectionBox = Instantiate(selectionBox);
                }
                if (!_selectionBox.activeSelf)
                {
                    _selectionBox.SetActive(true);
                }
                return _selectionBox;
            }
        }
        public HumanView SelectedHuman
        {
            get { return _selectedHuman; }
            set {
                HumanView previousHuman = _selectedHuman;
                _selectedHuman = value;
                if (previousHuman != _selectedHuman)
                {
                    SystemEventController.Instance.DispatchSystemEvent(EventMainControllerSelectedHuman);
                }
            }
        }
        public HumanView HumanPlayer
        {
            get { return _humanPlayer; }
            set { _humanPlayer = value; }
        }
        public bool IsSocialMeetingEnabled
        {
            get { return _isSocialMeetingEnabled; }
            set { _isSocialMeetingEnabled = value; }
        }
        public bool IsCasualMeetingEnabled
        {
            get { return _isCasualMeetingEnabled; }
            set { _isCasualMeetingEnabled = value; }
        }
        public bool IsInterruptionMeetingEnabled
        {
            get { return _isInterruptionMeetingEnabled; }
            set { _isInterruptionMeetingEnabled = value; }
        }
        public bool IsInfoScreenDisplayed
        {
            get { return _isInfoScreenDisplayed; }
            set { _isInfoScreenDisplayed = value; }
        }
        public int TotalNumberOfCommands
        {
            get { return _totalNumberOfCommands; }
            set {
                int prevCommands = _totalNumberOfCommands;
                _totalNumberOfCommands = value; 
                if (prevCommands != _totalNumberOfCommands)
                {
                    SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerUpdatedCommands);
                }
            }
        }
        public int TotalNumberOfAICommands
        {
            get { return _totalNumberOfAICommands; }
            set
            {
                int prevAICommands = _totalNumberOfAICommands;
                _totalNumberOfAICommands = value;
                if (prevAICommands != _totalNumberOfAICommands)
                {
                    SystemEventController.Instance.DispatchSystemEvent(AICommandsController.EventAICommandsControllerUpdatedCommands);
                }
            }
        }
        public Color LastProjectColor
        {
            get { return _lastProjectColor; }
            set { _lastProjectColor = value; }
        }
        public string LastProjectFeedback
        {
            get { return _lastProjectFeedback; }
            set { _lastProjectFeedback = value; }
        }
        public TeamCompanyListJSON TeamCompany
        {
            get { return _teamCompany; }
            set { _teamCompany = value; }
        }

        private void Awake()
        {
            workDayData.Initialize();
            assetsCatalogData.Initialize();
            gameAIData.Initialize();
            promptController.Initialize();
            CommController.Instance.Init();
            UsersController.Instance.Initialize(PLAYERPREFS_LOCAL_ENCRYPTION, EncryptionLocalAESKey);
            CameraXRController.Instance.Initialize();

            SystemEventController.Instance.Event += OnSystemEvent;
            UIEventController.Instance.Event += OnUIEvent;

            Application.runInBackground = true;            
        }

        public void Start()
        {
            if (DebugMode)
            {
                Debug.Log("YourVRUIScreenController::Start::First class to initialize for the whole system to work");
            }

            ScreenController.Instance.Initialize();
            NetworkController.Instance.NetworkEvent += OnNetworkEvent;
            FileBrowser.Instance.OnOpenFilesComplete += OnOpenFilesComplete;
            ImageDatabaseController.Instance.Initialize();
            MeetingController.Instance.Initialize();
            DocumentController.Instance.Initialize();
            TasksController.Instance.Initialize();
            LevelEditionController.Instance.Initialize();
            CommandsController.Instance.Initialize();
            AICommandsController.Instance.Initialize();
            CheckoutController.Instance.Initialize();
            PathFindingController.Instance.Initialize();

            CreateCameraFader();
        }

        void OnDestroy()
        {
            Destroy();
        }

        void OnApplicationFocus(bool hasFocus)
        {
#if !UNITY_EDITOR
            if (_state == StatesGame.Run)
            {
                if (hasFocus)
                {
                    StartCoroutine(CheckBackend());
                }
            }
#endif
        }

        void OnApplicationPause(bool isPaused)
        {
#if !UNITY_EDITOR
            if (_state == StatesGame.Run)
            {
                if (!isPaused)
                {
                    StartCoroutine(CheckBackend());
                }
            }
#endif
        }

        IEnumerator CheckBackend()
        {
            UnityWebRequest request = UnityWebRequest.Get(GameAIData.Instance.ServerChatGPT + "status");
            request.timeout = 5;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                string titleWarning = LanguageController.Instance.GetText("text.warning");
                string descriptionDisconnected = LanguageController.Instance.GetText("text.disconnected.from.server");
                string saveAndExit = LanguageController.Instance.GetText("text.save.and.exit");
                string exitWithoutSaving = LanguageController.Instance.GetText("text.exit.without.saving");
                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, titleWarning, descriptionDisconnected, SubEventMainControllerDisconnectionConfirmation, saveAndExit);
            }
        }

        public void InstantiateHUD()
        {
            if (_timeHUD == null)
            {
                _timeHUD = Instantiate(timeHUD).GetComponent<TimeHUD>();
            }
        }

        public Sprite GetContentImage(ImagesIndex indexImage)
        {
            if ((int)indexImage < images.Length)
            {
                return images[(int)indexImage];
            }
            else
            {
                return null;
            }
        }

        public void ApplyColor(Renderer renderer, Color color)
        {
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = color;
        }

        public void DestroyHUD()
        {
            if (_timeHUD != null)
            {
                GameObject.Destroy(_timeHUD.gameObject);
                _timeHUD = null;
            }
        }

        public void Destroy()
        {
            if (_instance != null)
            {
                ApplicationController instanceUsers = _instance;
                _instance = null;

                if (CommController.Instance != null) CommController.Instance.Destroy();
                if (UsersController.Instance != null) UsersController.Instance.Destroy();

                if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
                if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
                if (NetworkController.Instance != null) NetworkController.Instance.NetworkEvent -= OnNetworkEvent;

                if (FileBrowser.Instance != null) FileBrowser.Instance.OnOpenFilesComplete -= OnOpenFilesComplete;

                _selectedHuman = null;

                Destroy(instanceUsers);
            }
        }

        public bool IsPlayerReadyToGo()
        {
            return ((GameObject.FindAnyObjectByType<ScreenInfoItemView>() == null) && (GameObject.FindAnyObjectByType<ScreenListEventsHUDView>() == null));
        }

        public void CreateFileBrowser(string title, string eventName, params string[] extensions)
        {
            _eventReportSelectedFile = eventName;
            FileBrowser.Instance.OpenSingleFileAsync(title, "", "", extensions);
        }

        private void OnOpenFilesComplete(bool selected, string singleFile, string[] files)
        {
            if (selected)
            {
                SystemEventController.Instance.DispatchSystemEvent(_eventReportSelectedFile, true, singleFile);
            }
            else
            {
                SystemEventController.Instance.DispatchSystemEvent(_eventReportSelectedFile, false);
            }
        }

        public void CreateGameElementsView()
        {
            if (_requestCreation) return;
            _requestCreation = true;

            if (_playerView == null)
            {
                if (!_isMultiplayer)
                {
                    Instantiate(desktopPlayer, Vector3.zero, Quaternion.identity);
                }
                else
                {
                    NetworkController.Instance.CreateNetworkPrefab(false, desktopPlayer.name, desktopPlayer.gameObject, "Game\\Actors\\" + desktopPlayer.name, Vector3.zero, Quaternion.identity, 0);
                }
            }
        }

        private void InitCurrentGameLevel()
        {
            if (_levelView == null)
            {
                GameObject newLevel = AssetBundleController.Instance.CreateGameObject("Office");
                _levelView = newLevel.GetComponent<LevelView>();
            }
            _levelView.transform.position = Vector3.zero;
        }

        public void FadeOutCamera()
        {
            if (_cameraFader != null)
            {
                _cameraFader.FadeOut();
            }
        }

        public void CreateCameraFader()
        {
            if (_cameraFader == null)
            {
                _cameraFader = (Instantiate(CameraFade) as GameObject).GetComponent<CameraFader>();
            }
            if ((_inputController != null) && (_inputController.Camera != null))
            {
                _cameraFader.transform.parent = _inputController.Camera.gameObject.transform;
            }
            else
            {
                if (Camera.main != null)
                {
                    _cameraFader.transform.parent = Camera.main.transform;
                }
            }
            _cameraFader.transform.localPosition = Vector3.zero;
        }

        private void InitializeSystem(bool force)
        {
            if (((_state == StatesGame.None) && (_inputInited) && (_screenInited)) || force)
            {
                CreateCameraFader();
                ChangeGameState(StatesGame.Splash);
            }
        }

        public void ChangeGameState(StatesGame newGameState)
        {
            if (_state == newGameState)
            {
                return;
            }
            if (_gameState != null)
            {
                _gameState.Destroy();
            }
            _gameState = null;
            _previousState = _state;
            _state = newGameState;

            switch (_state)
            {
                case StatesGame.Splash:
                    _gameState = new MenuStateSplash();
                    break;

                case StatesGame.Download:
                    _gameState = new MenuStateDownload();
                    break;

                case StatesGame.MainMenu:
                    _gameState = new MenuStateMainMenu();
                    break;

                case StatesGame.Settings:
                    _gameState = new MenuStateSettings();
                    break;

                case StatesGame.EditOptions:
                    _gameState = new MenuStateEditOptions();
                    break;

                case StatesGame.PlayOptions:
                    _gameState = new MenuStatePlayOptions();
                    break;

                case StatesGame.Network:
                    _gameState = new MenuStateNetwork();
                    break;

                case StatesGame.Connecting:
                    _gameState = new MenuStateConnecting();
                    break;

                case StatesGame.Loading:
                    _gameState = new MenuStateLoad();
                    break;

                case StatesGame.Run:
                    _gameState = new MenuStateRun();
                    break;

                case StatesGame.ReleaseMemory:
                    _gameState = new MenuStateReleaseMemory();
                    break;
            }
            if (_gameState != null)
            {
                _gameState.Initialize();
            }
        }

        public void SetUpAISession()
        {
#if UNITY_EDITOR
            Debug.Log("SetUpAISession");
#endif
#if SHORTCUT_LLM_SERVER
            if (_serverAIInited)
            {
                SystemEventController.Instance.DelaySystemEvent(InitAPIKeysHTTP.EventInitAPIKeysHTTPCompleted, 0.1f);
            }
            else
            {
                _serverAIInited = true;
                if (LanguageController.Instance.CodeLanguage == LanguageController.CodeLanguageEnglish)
                {
                    GameAIData.Instance.ServerChatGPT = WorkDayData.Instance.serverScreenSession + ":5001/ai/";
                }
                else
                {
                    GameAIData.Instance.ServerChatGPT = WorkDayData.Instance.serverScreenSession + ":5002/ai/";
                }
                // GameAIData.Instance.ServerChatGPT = WorkDayData.Instance.serverScreenSession + "/ai/";
                GameAIData.Instance.InitAPIKeysAI(WorkDayData.Instance.apiKeyOpenAI,
                                                        WorkDayData.Instance.apiKeyMistral,
                                                        WorkDayData.Instance.apiKeyDeepSeek,
                                                        WorkDayData.Instance.apiKeyGemini,
                                                        WorkDayData.Instance.apiKeyGrok,
                                                        WorkDayData.Instance.apiKeyOpenRouter,
                                                        WorkDayData.Instance.apiKeyStability,
                                                        "",
                                                        "",
                                                        "",
                                                        "",
                                                        "",
                                                        "",
                                                        "",
                                                        WorkDayData.Instance.serverImageSession,
                                                        "");

                GameAIData.Instance.ChatGPTUsername = UsersController.Instance.CurrentUser.Id + "_" + UsersController.Instance.CurrentUser.Nickname;
                
                if (LanguageController.Instance.CodeLanguage == LanguageController.CodeLanguageEnglish)
                {
                    GameAIData.Instance.ChatGPTPassword = SHAEncryption.GetUniqueId(GameAIData.Instance.ChatGPTUsername + "_" + 5001).ToString();
                    WorkDayData.Instance.PortNumber = 5001;
                }
                else
                {
                    GameAIData.Instance.ChatGPTPassword = SHAEncryption.GetUniqueId(GameAIData.Instance.ChatGPTUsername + "_" + 5002).ToString();
                    WorkDayData.Instance.PortNumber = 5002;
                }                
            }
#else
            if (_serverAIInited)
            {
                SystemEventController.Instance.DelaySystemEvent(InitAPIKeysHTTP.EventInitAPIKeysHTTPCompleted, 0.1f);
            }
            else
            {
                _serverAIInited = true;
			    WorkDayData.Instance.CreateServerSession(LanguageController.Instance.CodeLanguage);
            }
#endif
        }

        public void ConfirmSessionCreated(SessionResponseJSON sessionResponse)
        {
#if UNITY_EDITOR
            Debug.Log("CREATED SESSION SERVER::PORT ASSIGNED="+ sessionResponse.port_number);
#endif
            GameAIData.Instance.ChatGPTUsername = UsersController.Instance.CurrentUser.Id + "_" + UsersController.Instance.CurrentUser.Nickname;
            GameAIData.Instance.ChatGPTPassword = SHAEncryption.GetUniqueId(GameAIData.Instance.ChatGPTUsername + "_" + sessionResponse.port_number).ToString();
            WorkDayData.Instance.PortNumber = sessionResponse.port_number;
#if ENABLE_REMOTE_CORS_SERVER
            WorkDayData.Instance.ServerState = WorkDayData.Instance.GetServerScreenSession() + "/service/" + sessionResponse.port_number;
            GameAIData.Instance.ServerChatGPT = WorkDayData.Instance.GetServerScreenSession() + "/service/" + sessionResponse.port_number + "/ai/";
#else
            WorkDayData.Instance.ServerState = WorkDayData.Instance.serverScreenSession + ":" + sessionResponse.port_number;
            GameAIData.Instance.ServerChatGPT = WorkDayData.Instance.serverScreenSession + ":" + sessionResponse.port_number + "/ai/";
#endif

            GameAIData.Instance.InitAPIKeysAI(WorkDayData.Instance.apiKeyOpenAI,
                            WorkDayData.Instance.apiKeyMistral,
                            WorkDayData.Instance.apiKeyDeepSeek,
                            WorkDayData.Instance.apiKeyGemini,
                            WorkDayData.Instance.apiKeyGrok,
                            WorkDayData.Instance.apiKeyOpenRouter,
                            WorkDayData.Instance.apiKeyStability,
                            "",
                            "",
                            "",
                            "",
                            "",
                            "",
                            "",
                            WorkDayData.Instance.serverImageSession,
                            "");
        }

        public DateTime GetNextMonday(DateTime day)
        {
            DateTime today = day;
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
            daysUntilMonday = daysUntilMonday == 0 ? 7 : daysUntilMonday; // Ensure it's *next* Monday, not today
            return today.AddDays(daysUntilMonday);
        }

        public void SetBackgroundScreensColor()
        {            
            ProjectInfoData project = WorkDayData.Instance.CurrentProject.GetProject(WorkDayData.Instance.CurrentProject.ProjectInfoSelected);
            if (project != null)
            {
                ApplicationController.Instance.LastProjectColor = project.GetColor();
                ApplicationController.Instance.LastProjectFeedback = project.Name;
            }
        }

        public bool IsImageAuthorized()
        {
            return (WorkDayData.Instance.CurrentProject.GetLevel() > 1);
        }

        private void OnNetworkEvent(string nameEvent, int originNetworkID, int targetNetworkID, object[] parameters)
        {

        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(ScreenInformationView.EventScreenInformationInstantiated))
            {
                if (_timeHUD != null)
                {
                    string screenName = (string)parameters[0];
                    GameObject screenGO = (GameObject)parameters[1];
                    if ((screenName.IndexOf(ScreenInformationView.ScreenLoading) != -1) ||
                        (screenName.IndexOf(ScreenInformationView.ScreenConfirmation) != -1) ||
                        (screenName.IndexOf(ScreenInformationView.ScreenInformation) != -1))
                    {
                        UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetColor, screenGO, _lastProjectColor);
                        UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetFeedbackText, screenGO, _lastProjectFeedback);
                        UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAIView.EventScreenFinalRequestAIViewOpened);
                    }
                }
            }
            if (nameEvent.Equals(ScreenInformationView.EventScreenInformationDestroyed))
            {
                if (_timeHUD != null)
                {
                    string screenName = (string)parameters[0];
                    if ((screenName.IndexOf(ScreenInformationView.ScreenLoading) != -1) ||
                        (screenName.IndexOf(ScreenInformationView.ScreenConfirmation) != -1) ||
                        (screenName.IndexOf(ScreenInformationView.ScreenInformation) != -1))
                    {
                        UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAIView.EventScreenFinalRequestAIViewClosed);
                    }
                }
            }
            if (nameEvent.Equals(SubEventMainControllerDisconnectionConfirmation))
            {
                ScreenInformationResponses userResponse = (ScreenInformationResponses)parameters[1];
                if (userResponse == ScreenInformationResponses.Confirm)
                {
                    _exitAfterSave = true;
                    UIEventController.Instance.DispatchUIEvent(TimeHUD.EventTimeHUDSaveProject);
                }
            }
        }

        private SessionResponseJSON _sessionData;

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(UpdateProjectDataHTTP.EventUpdateProjectDataHTTPCompleted))
            {
                if (_exitAfterSave)
                {
                    _exitAfterSave = false;
                    ApplicationController.Instance.ChangeGameState(ApplicationController.StatesGame.ReleaseMemory);
                }                
            }
            if (nameEvent.Equals(AskBaseaCreateSessionChatGPTHTTP.EventAskBaseaCreateSessionChatGPTHTTPCompleted))
            {
                if ((bool)parameters[0])
                {
                    UIEventController.Instance.DelayUIEvent(ScreenWaitProgressView.EventScreenWaitProgressViewSetDescription, 3, LanguageController.Instance.GetText("text.ai.server.warming.up"));

#if SHORTCUT_LLM_SERVER
                    ConfirmSessionCreated((SessionResponseJSON)parameters[1]);
#else
#if ENABLE_REMOTE_CORS_SERVER
                    _sessionData = (SessionResponseJSON)parameters[1];
                    SystemEventController.Instance.DelaySystemEvent(EventMainControllerDelayedServerInited, DELAY_SECONDS_FOR_WARMUP_PROCESS_IN_SERVER);                    
#else
                    ConfirmSessionCreated((SessionResponseJSON)parameters[1]);
#endif
#endif
                }
            }
            if (nameEvent.Equals(EventMainControllerDelayedServerInited))
            {
                if (_sessionData != null)
                {
                    ConfirmSessionCreated(_sessionData);
                    _sessionData = null;
                }                
            }
            if (nameEvent.Equals(InitAPIKeysHTTP.EventInitAPIKeysHTTPCompleted))
            {
#if UNITY_EDITOR
                Debug.LogError("InitLLMProvider");
#endif
                WorkDayData.Instance.InitLLMProvider();
            }
            if (nameEvent.Equals(EventMainControllerReleaseGameResources))
            {
                _levelView = null;
                _playerView = null;
                _selectedHuman = null;
                _requestCreation = false;
                _hasStartedSession = false;
                _serverAIInited = false;
                _players.Clear();
                if ((bool)parameters[0])
                {
                    ApplicationController.Instance.TeamCompany = null;
                }
                if (_selectionBox != null)
                {
                    GameObject.Destroy(_selectionBox);
                    _selectionBox = null;
                }
                DestroyHUD();
            }
            if (nameEvent.Equals(ScreenController.EventScreenControllerStarted))
            {
                _screenInited = true;
                InitializeSystem(false);
            }
            if (nameEvent.Equals(InputController.EventInputControllerHasStarted))
            {
                _inputController = ((GameObject)parameters[0]).GetComponent<IInputController>();
                _inputController.Initialize();
                _inputInited = true;
                InitializeSystem(false);
            }
            if (nameEvent.Equals(PlayerView.EventPlayerAppHasStarted))
            {
                PlayerView player = (PlayerView)parameters[0];
                if (!_isMultiplayer)
                {
                    _playerView = player;
                    player.Initialize(_isFreeMode);

                    InitCurrentGameLevel();
                    SystemEventController.Instance.DelaySystemEvent(EventMainControllerAllPlayerViewReadyToStartGame, 1);
                }
                else
                {
                    if (!player.NetworkGameIDView.AmOwner())
                    {
                        player.Initialize(_isFreeMode);
                    }
                    else
                    {
                        _playerView = player;

                        if (_playerView != null)
                        {
                            _playerView.Initialize(_isFreeMode);

                            InitCurrentGameLevel();
                            NetworkController.Instance.DispatchNetworkEvent(EventMainControllerAllPlayerViewReadyToStartGame, -1, -1);
                        }
                    }
                    if (!_players.ContainsKey(player))
                    {
                        _players.Add(player, 0);
                    }
                }
            }
        }

        void Update()
        {
            if (_gameState != null)
            {
                _gameState.Run();
            }
        }
    }
}