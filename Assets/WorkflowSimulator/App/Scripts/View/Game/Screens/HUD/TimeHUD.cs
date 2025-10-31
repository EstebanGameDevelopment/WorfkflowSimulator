using InGameCodeEditor;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.CommandsController;
using static yourvrexperience.WorkDay.MeetingController;
using static yourvrexperience.WorkDay.ScreenCalendarView;
using static yourvrexperience.WorkDay.ScreenListEventsHUDView;

namespace yourvrexperience.WorkDay
{
    public class TimeHUD : MonoBehaviour
    {
#if UNITY_EDITOR
        public const float MAX_SPEED_TIME = 300;
#else
        public const float MAX_SPEED_TIME = 300;
#endif

        public const string EventTimeHUDSelectionObject = "EventTimeHUDSelectionObject";
        public const string EventTimeHUDCancelSelectionObject = "EventTimeHUDSelectionObject";
        public const string EventTimeHUDSaveProject = "EventTimeHUDSaveProject";
        public const string EventTimeHUDExitGame = "EventTimeHUDExitGame";
        public const string EventTimeHUDUpdateCurrentTime = "EventTimeHUDUpdateCurrentTime";
        public const string EventTimeHUDUpdateEnableInteraction = "EventTimeHUDUpdateEnableInteraction";
        public const string EventTimeHUDUpdateResetBoundingBox = "EventTimeHUDUpdateResetBoundingBox";
        public const string EventTimeHUDUpdatePlayTime = "EventTimeHUDUpdatePlayTime";        
        public const string EventTimeHUDShowMessage = "EventTimeHUDShowMessage";
        public const string EventTimeHUDShowClickedFeedback = "EventTimeHUDShowClickedFeedback";
        public const string EventTimeHUDEnableSpeedUpToggle = "EventTimeHUDEnableSpeedUpToggle";
        public const string EventTimeHUDShortcutAction = "EventTimeHUDShortcutAction";
        public const string EventTimeHUDShowSlider = "EventTimeHUDShowSlider";

        public const string EventTimeHUDResetTimeIncrement = "EventTimeHUDResetTimeIncrement";

        public const string SubEventTimeHUDUpdatedCompanyDescription = "SubEventTimeHUDUpdatedCompanyDescription";

        [SerializeField] private ClockController clockController;
        [SerializeField] private PanelProgressEvents panelProgressEvents;
        
        [SerializeField] private GameObject bgNormalSpeed;
        [SerializeField] private GameObject bgSpeedUp;

        [SerializeField] private GameObject panelMinimized;
        [SerializeField] private CustomButton collapseTimeHUD;
        [SerializeField] private Button expandTimeHUD;
        [SerializeField] private Button buttonFeedback;
        [SerializeField] private Text textHour;
        [SerializeField] private Text textMinute;

        [SerializeField] private CustomButton buttonBuild;
        [SerializeField] private CustomButton buttonCalendar;
        [SerializeField] private CustomButton buttonTasks;
        [SerializeField] private CustomButton buttonDocuments;
        [SerializeField] private CustomButton buttonAIChat;
        [SerializeField] private CustomButton buttonGroups;
        [SerializeField] private CustomButton buttonExpand;
        [SerializeField] private CustomButton buttonShowMeetings;
        [SerializeField] private CustomButton buttonShowTasks;
        [SerializeField] private CustomButton buttonSave;
        [SerializeField] private CustomButton buttonExit;
        [SerializeField] private CustomButton buttonRotateLeft;
        [SerializeField] private CustomButton buttonRotateRight;
        [SerializeField] private CustomButton buttonCompanyInfo;
        [SerializeField] private CustomButton buttonSetDate;
        [SerializeField] private CustomButton buttonClockSpeed;
        [SerializeField] private CustomButton buttonProjectTitle;
        [SerializeField] private CustomButton buttonCurrentTask;
        [SerializeField] private CustomButton buttonTitleSelection;
        [SerializeField] private CustomButton buttonCurrentMeeting;
        [SerializeField] private CustomButton buttonPlayTime;
        [SerializeField] private CustomButton buttonCheckCosts;

        [SerializeField] private TextMeshProUGUI titleProject;
        [SerializeField] private TextMeshProUGUI titleSelection;
        [SerializeField] private TextMeshProUGUI titleFeedback;        
        [SerializeField] private TextMeshProUGUI titleCurrentTaks;
        [SerializeField] private TextMeshProUGUI titleCurrentMeeting;
        [SerializeField] private GameObject containerFeedback;
        [SerializeField] private GameObject hightlightChangeSpeed;
        
        [SerializeField] private GameObject iconPlayTime;
        [SerializeField] private GameObject iconPauseTime;

        [SerializeField] private CustomToggle toggleSpeedUp;
        [SerializeField] private Slider sliderSpeed;

        [SerializeField] private TextMeshProUGUI debugMeetings;
        [SerializeField] private TextMeshProUGUI debugCommands;
        [SerializeField] private TextMeshProUGUI debugAICommands;

        private GameObject _selectedGO;
        private AreaData _selectedArea;
        private ProjectInfoData _selectedProject;
        private WorldItemData _selectionWorldItemData;
        private bool _editionMode = false;

        private bool _highlightSpeedActivate = false;
        
        private bool _expandedSelection = false;
        private bool _expandedInformationForSelection = false;
        private string _nextMeetingID = null;
        private bool _isMeetingInProgress = false;
        private bool _collapsed = false;
        private string _showingMessage = "";
        private bool _lockedInteraction = false;
        private bool _enableShorcutAction = false;
        private bool _speedUpActivated = false;
        private bool _blockTimeInteraction = false;

        public TimeSpan IncrementTime
        {
            get { return clockController.IncrementTime; }
            set { 
                clockController.IncrementTime = value;
                if (clockController.IncrementTime.TotalSeconds == 1)
                {
                    _speedUpActivated = false;
                    UpdateVisibilityBGSpeed();
                }                
            }
        }
        public bool LockedInteraction
        {
            get { return _lockedInteraction; }
            set { _lockedInteraction = value; }
        }
        public bool SpeedUpToggleOn
        {
            get { return toggleSpeedUp.isOn; }
        }
        public bool IsPlayingTime
        {
            get { return clockController.TimePlaying; }
        }
        public bool BlockTimeInteraction
        {
            get { return _blockTimeInteraction; }
            set { _blockTimeInteraction = value; }
        }

        public void Initialize()
        {
            clockController.Initialize();
            panelProgressEvents.Initialize();

            buttonExit.onClick.AddListener(OnExitGame);
            buttonSave.onClick.AddListener(OnSaveProject);

            buttonBuild.gameObject.SetActive(ApplicationController.Instance.IsFreeMode);
            buttonBuild.onClick.AddListener(OnEditionPanel);

            buttonCalendar.gameObject.SetActive(true);
            buttonCalendar.onClick.AddListener(OnCalendarCommand);

            buttonTasks.gameObject.SetActive(true);
            buttonTasks.onClick.AddListener(OnTasksCommand);

            buttonDocuments.gameObject.SetActive(true);
            buttonDocuments.onClick.AddListener(OnDocumentsCommand);

            buttonAIChat.gameObject.SetActive(true);
            buttonAIChat.onClick.AddListener(OnAIChatScreen);

            buttonProjectTitle.gameObject.SetActive(true);
            buttonProjectTitle.onClick.AddListener(OnProjectsCommand);

            buttonGroups.gameObject.SetActive(true);
            buttonGroups.onClick.AddListener(OnGroupsCommand);

            buttonExpand.gameObject.SetActive(false);
            buttonExpand.onClick.AddListener(OnExpandSelection);

            buttonTitleSelection.gameObject.SetActive(true);
            buttonTitleSelection.onClick.AddListener(OnExpandSelection);

            buttonRotateLeft.gameObject.SetActive(true);
            buttonRotateLeft.onClick.AddListener(OnRotateLeft);

            buttonRotateRight.gameObject.SetActive(true);
            buttonRotateRight.onClick.AddListener(OnRotateRigth);

            buttonCompanyInfo.gameObject.SetActive(true);
            buttonCompanyInfo.onClick.AddListener(OnCompanyInformation);

            buttonCurrentTask.gameObject.SetActive(true);
            buttonCurrentTask.onClick.AddListener(OnCurrentTask);

            buttonCheckCosts.gameObject.SetActive(true);
            buttonCheckCosts.onClick.AddListener(OnCheckCosts);

            buttonCurrentMeeting.gameObject.SetActive(true);
            buttonCurrentMeeting.onClick.AddListener(OnNextMeeting);

            buttonPlayTime.gameObject.SetActive(true);
            buttonPlayTime.onClick.AddListener(OnPlayPauseTime);

            buttonShowMeetings.gameObject.SetActive(true);
            buttonShowMeetings.onClick.AddListener(OnShowNextMeetings);

            buttonShowTasks.gameObject.SetActive(true);
            buttonShowTasks.onClick.AddListener(OnShowNextTasks);

            buttonSetDate.onClick.AddListener(OnSetDate);

            buttonFeedback.onClick.AddListener(OnButtonFeedback);

            UpdateProject();

            buttonBuild.PointerEnterButton += OnButtonBuildEnter;
            buttonBuild.PointerExitButton += OnFeedbackReset;

            buttonCalendar.PointerEnterButton += OnButtonCalendarEnter;
            buttonCalendar.PointerExitButton += OnFeedbackReset;
                        
            buttonTasks.PointerEnterButton += OnButtonTasksEnter;
            buttonTasks.PointerExitButton += OnFeedbackReset;

            buttonDocuments.PointerEnterButton += OnButtonDocumentsEnter;
            buttonDocuments.PointerExitButton += OnFeedbackReset;

            buttonAIChat.PointerEnterButton += OnButtonAIChatEnter;
            buttonAIChat.PointerExitButton += OnFeedbackReset;
            
            buttonProjectTitle.PointerEnterButton += OnButtonProjectsEnter;
            buttonProjectTitle.PointerExitButton += OnFeedbackReset;

            buttonGroups.PointerEnterButton += OnButtonGroupsEnter;
            buttonGroups.PointerExitButton += OnFeedbackReset;

            buttonExpand.PointerEnterButton += OnButtonExpandEnter;
            buttonExpand.PointerExitButton += OnFeedbackReset;

            buttonTitleSelection.PointerEnterButton += OnButtonPersonEnter;
            buttonTitleSelection.PointerExitButton += OnFeedbackReset;

            buttonSave.PointerEnterButton += OnButtonSaveEnter;
            buttonSave.PointerExitButton += OnFeedbackReset;

            buttonExit.PointerEnterButton += OnButtonExitEnter;
            buttonExit.PointerExitButton += OnFeedbackReset;

            buttonRotateLeft.PointerEnterButton += OnButtonRotateLeftEnter;
            buttonRotateLeft.PointerExitButton += OnFeedbackReset;

            buttonRotateRight.PointerEnterButton += OnButtonRotateRigthEnter;
            buttonRotateRight.PointerExitButton += OnFeedbackReset;

            buttonCompanyInfo.PointerEnterButton += OnButtonCompanyInfoEnter;
            buttonCompanyInfo.PointerExitButton += OnFeedbackReset;

            buttonSetDate.PointerEnterButton += OnButtonSetDateEnter;
            buttonSetDate.PointerExitButton += OnFeedbackReset;

            buttonCurrentTask.PointerEnterButton += OnButtonCurrentTaskEnter;
            buttonCurrentTask.PointerExitButton += OnFeedbackReset;

            buttonCurrentMeeting.PointerEnterButton += OnButtonNextMeetingEnter;
            buttonCurrentMeeting.PointerExitButton += OnFeedbackReset;

            collapseTimeHUD.PointerEnterButton += OnButtonCollapseHUDEnter;
            collapseTimeHUD.PointerExitButton += OnFeedbackReset;

            buttonPlayTime.PointerEnterButton += OnButtonPlayPauseEnter;
            buttonPlayTime.PointerExitButton += OnFeedbackReset;

            buttonShowMeetings.PointerEnterButton += OnButtonShowMeetingEnter;
            buttonShowMeetings.PointerExitButton += OnFeedbackReset;

            buttonShowTasks.PointerEnterButton += OnButtonShowTasksEnter;
            buttonShowTasks.PointerExitButton += OnFeedbackReset;

            buttonCheckCosts.PointerEnterButton += OnButtonCheckCostsEnter;
            buttonCheckCosts.PointerExitButton += OnFeedbackReset;

            toggleSpeedUp.PointerEnterButton += OnToggleSpeedUpEnter;
            toggleSpeedUp.PointerExitButton += OnFeedbackToggleReset;

            hightlightChangeSpeed.SetActive(false);
            _highlightSpeedActivate = false;

            containerFeedback.SetActive(false);

            titleSelection.text = LanguageController.Instance.GetText("text.no.selection");
            titleCurrentTaks.text = "";
            titleCurrentMeeting.text = "";

            SystemEventController.Instance.Event += OnSystemEvent;
            UIEventController.Instance.Event += OnUIEvent;

            collapseTimeHUD.onClick.AddListener(OnCollapseTimeHUD);
            expandTimeHUD.onClick.AddListener(OnExpandTimeHUD);

            toggleSpeedUp.isOn = false;
            toggleSpeedUp.onValueChanged.AddListener(OnSpeedUpChanged);

            UpdateVisibilityCollapse(false);
            UpdateVisibilityTimePlaying();
            UpdateVisibilityBGSpeed();

            sliderSpeed.gameObject.SetActive(false);
            sliderSpeed.onValueChanged.AddListener(OnSliderSpeed);

            if (ApplicationController.Instance.IsPlayMode)
            {
                buttonSetDate.interactable = false;
                buttonBuild.interactable = false;
            }
        }

        private void OnDestroy()
        {
            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
            if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

            ResetReferences();
            panelProgressEvents.Destroy();
            clockController.Destroy();
            clockController = null;
            panelProgressEvents = null;
        }

        private bool _enableSliderSpeed = false;

        private void OnSliderSpeed(float value)
        {
            if (!_enableSliderSpeed) return;

            clockController.SpeedUpTime((int)(MAX_SPEED_TIME * value));
        }

        private void UpdateVisibilityBGSpeed()
        {
            bgNormalSpeed.SetActive(!_speedUpActivated);
            bgSpeedUp.SetActive(_speedUpActivated);
            if ((CommandsController.Instance.CurrentCommandState == CommandsController.CommandStates.Idle) 
                || (CommandsController.Instance.CurrentCommandState == CommandStates.LunchTime))
            {
                if (_speedUpActivated)
                {
                    if (WorkDayData.Instance.CurrentProject.StartDayTrigger)
                    {
                        sliderSpeed.gameObject.SetActive((MeetingController.Instance.MeetingsInProgress.Count == 0));
                    }
                    else
                    {
                        sliderSpeed.gameObject.SetActive(false);
                    }                    
                }
                else
                {
                    sliderSpeed.gameObject.SetActive(false);
                }
            }
            else
            {
                sliderSpeed.gameObject.SetActive(false);
            }
        }

        private void OnAIChatScreen()
        {
            ScreenController.Instance.CreateScreen(ScreenAIOperationView.ScreenName, true, false);            
        }

        private void OnCheckCosts()
        {
            ScreenController.Instance.CreateScreen(ScreenSystemCostsView.ScreenName, false, false);
        }

        private void OnSpeedUpChanged(bool value)
        {
            if (!clockController.TimePlaying)
            {
                OnPlayPauseTime();
            }
            if (value)
            {
                _speedUpActivated = true;
                UpdateVisibilityBGSpeed();                
                _enableSliderSpeed = false;
                if (MeetingController.Instance.MeetingsInProgress.Count > 0)
                {
                    sliderSpeed.value = (float)30 / (float)MAX_SPEED_TIME;
                    clockController.SpeedUpTime(30);
                }
                else
                {
                    sliderSpeed.value = (float)60 / (float)MAX_SPEED_TIME;
                    clockController.SpeedUpTime(60);
                }                
                _enableSliderSpeed = true;
                SystemEventController.Instance.DispatchSystemEvent(ClockController.EventClockControllerTimeSpeedUp, true);
            }
            else
            {
                _speedUpActivated = false;
                sliderSpeed.gameObject.SetActive(false);
                UpdateVisibilityBGSpeed();
                clockController.ResetIncrementTime();
                SystemEventController.Instance.DispatchSystemEvent(ClockController.EventClockControllerTimeSpeedUp, false);
            }
        }

        public void ChangeToNextDay()
        {
            clockController.ChangeToNextDay();
        }

        private void OnShowNextTasks()
        {
            SystemEventController.Instance.DispatchSystemEvent(ScreenListEventsHUDView.EventScreenListEventsHUDViewDestroy);
            ScreenController.Instance.CreateScreen(ScreenListEventsHUDView.ScreenName, false, false, TypeLateralInfo.TASKS);
        }

        private void OnShowNextMeetings()
        {
            SystemEventController.Instance.DispatchSystemEvent(ScreenListEventsHUDView.EventScreenListEventsHUDViewDestroy);
            ScreenController.Instance.CreateScreen(ScreenListEventsHUDView.ScreenName, false, false, TypeLateralInfo.MEETINGS);
        }


        private void OnPlayPauseTime()
        {
            clockController.TimePlaying = !clockController.TimePlaying;
            UpdateVisibilityTimePlaying();
        }

        private void UpdateVisibilityTimePlaying()
        {
            iconPlayTime.SetActive(!clockController.TimePlaying);
            iconPauseTime.SetActive(clockController.TimePlaying);
        }

        private void UpdateVisibilityCollapse(bool collapsed)
        {
            _collapsed = collapsed;
            clockController.gameObject.SetActive(!_collapsed);
            panelMinimized.SetActive(_collapsed);
            if (_collapsed)
            {
                _expandedSelection = true;
            }
        }
        private void OnExpandTimeHUD()
        {
            UpdateVisibilityCollapse(false);
        }

        private void OnCollapseTimeHUD()
        {
            UpdateVisibilityCollapse(true);
            if (_highlightSpeedActivate) OnActivateHightlight();
        }

        private void OnCurrentTask()
        {
            if (ApplicationController.Instance.SelectedHuman != null)
            {
                TaskProgressData taskProgress = ApplicationController.Instance.SelectedHuman.GetActiveTask();
                if (taskProgress == null)
                {
                    if (ApplicationController.Instance.SelectedHuman == null) ResetReferences();
                    ScreenController.Instance.CreateScreen(ScreenBoardsView.ScreenName, true, false);
                }
                else
                {
                    var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(taskProgress.TaskUID);

                    ProjectInfoData projectMeeting = WorkDayData.Instance.CurrentProject.GetProject(taskProgress.ProjectUID);
                    SystemEventController.Instance.DispatchSystemEvent(ScreenProjectsView.EventScreenProjectsViewLoadProject, projectMeeting, false);
                    
                    GameObject screenTaskManagerGO = ScreenController.Instance.CreateScreen(ScreenTaskManagerView.ScreenName, true, true, boardName);
                    GameObject screenTaskGO = ScreenController.Instance.CreateScreen(ScreenTaskView.ScreenName, false, false, taskItemData, boardName);

                    UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, screenTaskGO, screenTaskManagerGO.GetComponent<Canvas>().sortingOrder + 1);
                }
            }
        }

        private void OnNextMeeting()
        {
            if (ApplicationController.Instance.SelectedHuman != null)
            {
                GameObject calendarGO = ScreenController.Instance.CreateScreen(ScreenCalendarView.ScreenName, false, false, CalendarOption.NORMAL, false);
                if ((_nextMeetingID != null) && (_nextMeetingID.Length > 0))
                {
                    MeetingData meeting = WorkDayData.Instance.CurrentProject.GetMeetingByUID(_nextMeetingID);
                    GameObject meetingGO = ScreenController.Instance.CreateScreen(ScreenMeetingView.ScreenName, false, false, meeting.TaskId, meeting);
                    UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, meetingGO, calendarGO.GetComponent<Canvas>().sortingOrder + 1);
                }
            }
        }

        private void OnCompanyInformation()
        {
            ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLongInput, null, LanguageController.Instance.GetText("text.company.info"), "", SubEventTimeHUDUpdatedCompanyDescription);
            UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, WorkDayData.Instance.CurrentProject.Company);
            if (GameObject.FindAnyObjectByType<CodeEditor>() != null)
            {
                GameObject.FindAnyObjectByType<CodeEditor>().Refresh();
            }

            if (ApplicationController.Instance.IsPlayMode)
            {
                UIEventController.Instance.DelayUIEvent(ScreenInformationView.EventScreenInformationEnableInputText, 0.1f, false);
            }
        }

        private void OnButtonCompanyInfoEnter(CustomButton value)
        {
            ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.company.info").ToUpper());
        }

        private void OnButtonAIChatEnter(CustomButton value)
        {
            ShowTitleFeedbackMessage(WorkDayData.Instance.GetLLMProviderName(WorkDayData.Instance.LlmProvider));
        }

        private void OnFeedbackToggleReset(CustomToggle value)
        {
            ShowTitleFeedbackMessage(null);
        }

        private void OnToggleSpeedUpEnter(CustomToggle value)
        {
            if (toggleSpeedUp.isOn)
            {
                ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.change.to.normal.speed").ToUpper() + " ");
            }
            else
            {
                ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.speed.up").ToUpper());
            }
        }
        
        private void OnButtonCheckCostsEnter(CustomButton value)
        {
            ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.check.ai.costs").ToUpper());
        }

        private void OnButtonShowEmployeesEnter(CustomButton obj)
        {
            ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.show.employees").ToUpper());
        }

        private void OnButtonShowTasksEnter(CustomButton value)
        {
            ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.show.next.tasks").ToUpper());
        }

        private void OnButtonShowMeetingEnter(CustomButton value)
        {
            ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.show.next.meetings").ToUpper());
        }

        private void OnButtonPlayPauseEnter(CustomButton value)
        {
            if (clockController.TimePlaying)
            {
                ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.pause.time").ToUpper());
            }
            else
            {
                ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.play.time").ToUpper());
            }
            containerFeedback.SetActive(true);
        }

        private void OnButtonFeedback()
        {
            if (_enableShorcutAction)
            {
                SystemEventController.Instance.DispatchSystemEvent(EventTimeHUDShortcutAction);
            }
            else
            {
                UIEventController.Instance.DispatchUIEvent(EventTimeHUDShowClickedFeedback);
            }            
        }

        private void OnButtonCollapseHUDEnter(CustomButton value)
        {
            ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.collapse.hud").ToUpper());
        }

        private void OnButtonCurrentTaskEnter(CustomButton value)
        {
            ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.task.assigned").ToUpper());
        }

        private void OnButtonNextMeetingEnter(CustomButton value)
        {
            if (!_isMeetingInProgress)
            {
                ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.next.meeting").ToUpper());
            }
            else
            {
                ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.current.meeting").ToUpper());
            }            
            containerFeedback.SetActive(true);
        }

        private void OnButtonSetDateEnter(CustomButton value)
        {
            ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.set.date").ToUpper());
        }

        private void OnFeedbackReset(CustomButton value)
        {
            ShowTitleFeedbackMessage("");            
        }

        private void OnButtonBuildEnter(CustomButton value)
        {
            ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.build").ToUpper());
        }

        private void OnButtonExitEnter(CustomButton value)
        {
            ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.exit.to.menu").ToUpper());
        }

        private void OnButtonSaveEnter(CustomButton value)
        {
            ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.save").ToUpper());
        }

        private void OnButtonPersonEnter(CustomButton value)
        {
            ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.person.selected").ToUpper());
        }

        private void OnButtonExpandEnter(CustomButton value)
        {
            ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.information.person").ToUpper());
        }

        private void OnButtonGroupsEnter(CustomButton value)
        {
            ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.groups").ToUpper());
        }

        private void OnButtonProjectsEnter(CustomButton value)
        {
            ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.projects").ToUpper());
        }

        private void OnButtonDocumentsEnter(CustomButton value)
        {
            ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.documents").ToUpper());
        }

        private void OnButtonTasksEnter(CustomButton value)
        {
            ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.tasks").ToUpper());
        }

        private void OnButtonCalendarEnter(CustomButton value)
        {
            ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.calendar").ToUpper());
        }

        private void OnButtonRotateRigthEnter(CustomButton value)
        {
            ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.rotate.right").ToUpper());
        }

        private void OnButtonRotateLeftEnter(CustomButton value)
        {
            ShowTitleFeedbackMessage(LanguageController.Instance.GetText("text.rotate.left").ToUpper());
        }

        private void UpdateProject()
        {
            _selectedProject = WorkDayData.Instance.CurrentProject.GetProject(WorkDayData.Instance.CurrentProject.ProjectInfoSelected);
            titleProject.text = Utilities.ShortenText(_selectedProject.Name, 20);
        }

        private void OnCalendarCommand()
        {
            if (ApplicationController.Instance.SelectedHuman == null) ResetReferences();
            ScreenController.Instance.CreateScreen(ScreenCalendarView.ScreenName, true, false, CalendarOption.NORMAL, true);
        }

        private void OnTasksCommand()
        {
            if (ApplicationController.Instance.SelectedHuman == null) ResetReferences();
            ScreenController.Instance.CreateScreen(ScreenBoardsView.ScreenName, true, false);
        }

        private void OnDocumentsCommand()
        {
            if (ApplicationController.Instance.SelectedHuman == null) ResetReferences();

            string projectTitleName = "";
            if (WorkDayData.Instance.CurrentProject.ProjectInfoSelected != -1)
            {
               ProjectInfoData projectInfo = WorkDayData.Instance.CurrentProject.GetProject(WorkDayData.Instance.CurrentProject.ProjectInfoSelected);
                if (projectInfo != null)
                {
                    projectTitleName = LanguageController.Instance.GetText("text.global.documents.for") + projectInfo.Name;
                }
            }
            ScreenInformationView.CreateScreenInformation(ScreenDocumentsDataView.ScreenName, null, projectTitleName, "");
            UIEventController.Instance.DispatchUIEvent(ScreenDocumentsDataView.EventScreenDocumentsDataViewInitialization, WorkDayData.Instance.CurrentProject.ProjectInfoSelected, true, WorkDayData.Instance.CurrentProject.GetDocuments());
        }

        private void OnProjectsCommand()
        {
            if (ApplicationController.Instance.SelectedHuman == null) ResetReferences();
            ScreenController.Instance.CreateScreen(ScreenProjectsView.ScreenName, true, false);
        }

        private void OnGroupsCommand()
        {
            if (ApplicationController.Instance.SelectedHuman == null) ResetReferences();
            SystemEventController.Instance.DispatchSystemEvent(ScreenListEventsHUDView.EventScreenListEventsHUDViewDestroy);
            ScreenController.Instance.CreateScreen(ScreenGroupsView.ScreenName, false, false, false);
        }

        private void OnEditionPanel()
        {
            ResetReferences();
            ScreenController.Instance.CreateScreen(ScreenPanelEditionView.ScreenName, true, false);
            UIEventController.Instance.DispatchUIEvent(ScreenPanelEditionView.EventScreenPanelEditionResetToIdle);
        }

        private void OnRotateRigth()
        {
            SystemEventController.Instance.DispatchSystemEvent(EditionSubStateIdle.EventSubStateIdleRotateCamera, false);
        }

        private void OnRotateLeft()
        {
            SystemEventController.Instance.DispatchSystemEvent(EditionSubStateIdle.EventSubStateIdleRotateCamera, true);
        }

        private void OnExpandSelection()
        {
            if (GameObject.FindAnyObjectByType<ScreenDialogView>() != null) return;

            SystemEventController.Instance.DispatchSystemEvent(ScreenInfoItemView.EventScreenInfoItemViewOnlyDestroyView);
            if (_editionMode)
            {
                _expandedSelection = false;                
                if (_selectionWorldItemData != null)
                {
                    ScreenController.Instance.CreateScreen(ScreenInfoItemView.ScreenName, false, false, _selectedGO, _selectionWorldItemData);
                }
                else
                {
                    ScreenController.Instance.CreateScreen(ScreenInfoItemView.ScreenName, false, false, _selectedGO, _selectedArea);
                }
            }
            else
            {
                _expandedSelection = true;
                if (_selectionWorldItemData != null)
                {
                    ScreenController.Instance.CreateScreen(ScreenInfoItemView.ScreenName, false, false, _selectedGO, _selectionWorldItemData);
                }
                else
                {
                    ScreenController.Instance.CreateScreen(ScreenInfoItemView.ScreenName, false, false, _selectedGO, _selectedArea);
                }
            }
        }

        private void OnSaveProject()
        {
            ApplicationController.Instance.LastProjectFeedback = "";
            ApplicationController.Instance.LastProjectColor = Color.white;
            UIEventController.Instance.DispatchUIEvent(EventTimeHUDSaveProject);
        }

        private void OnExitGame()
        {
            UIEventController.Instance.DispatchUIEvent(EventTimeHUDExitGame);
        }

        private void OnSetDate()
        {
            ScreenController.Instance.CreateScreen(ScreenCalendarView.ScreenName, true, false, CalendarOption.SELECT_GLOBAL_DATE, "");
        }

        private void ResetReferences()
        {
            _selectedGO = null;
            ApplicationController.Instance.SelectedHuman = null;
            _selectedArea = null;
            _selectionWorldItemData = null;
            buttonExpand.gameObject.SetActive(false);
            buttonTitleSelection.gameObject.SetActive(false);
            titleSelection.text = LanguageController.Instance.GetText("text.no.selection");
            titleCurrentTaks.text = "";
            titleCurrentMeeting.text = "";
        }

        private void SelectHuman(GameObject selecteItemGO, WorldItemData human)
        {
            ApplicationController.Instance.SelectionBox.transform.parent = null;
            Utilities.MatchSize(ApplicationController.Instance.SelectionBox, selecteItemGO);
            ApplicationController.Instance.SelectionBox.transform.parent = selecteItemGO.transform;
            ApplicationController.Instance.SelectionBox.transform.localPosition = Vector3.zero;
            ApplicationController.Instance.SelectionBox.transform.eulerAngles = Vector3.zero;

            _selectionWorldItemData = human;
            if (_selectionWorldItemData.IsHuman)
            {
                ApplicationController.Instance.SelectedHuman = (HumanView)_selectedGO.GetComponent<HumanView>();
                if (ApplicationController.Instance.SelectedHuman != null)
                {
                    titleSelection.text = ApplicationController.Instance.SelectedHuman.NameHuman;
                    RefreshCurrentTask();
                    RefreshCurrentMeeting();
                }
            }
        }

        private void RefreshCurrentTask()
        {
            if (ApplicationController.Instance.SelectedHuman != null)
            {
                TaskProgressData taskProgress = ApplicationController.Instance.SelectedHuman.GetActiveTask();
                if (taskProgress == null)
                {
                    titleCurrentTaks.text = LanguageController.Instance.GetText("text.no.task.assigned").ToUpper();
                }
                else
                {
                    var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(taskProgress.TaskUID);
                    if (taskItemData == null)
                    {
                        titleCurrentTaks.text = LanguageController.Instance.GetText("text.no.task.assigned").ToUpper();
                    }
                    else
                    {
                        titleCurrentTaks.text = Utilities.ShortenText(taskItemData.Name, 30);
                    }
                }
            }
        }

        private void RefreshCurrentMeeting()
        {
            if (ApplicationController.Instance.SelectedHuman != null)
            {
                List<MeetingData> orderedMeetings = WorkDayData.Instance.CurrentProject.GetMeetingsForHuman(WorkDayData.Instance.CurrentProject.GetCurrentTime(), ApplicationController.Instance.SelectedHuman.NameHuman);
                if (orderedMeetings.Count == 0)
                {
                    titleCurrentMeeting.text = "";
                }
                else
                {
                    MeetingData nextMeet = orderedMeetings[0];
                    _nextMeetingID = nextMeet.GetUID();
                    _isMeetingInProgress = nextMeet.InProgress;
                    titleCurrentMeeting.text = nextMeet.GetTimeStart().ToShortTimeString() + " - " + Utilities.ShortenText(nextMeet.Name, 30);
                }
            }
        }

        private void OnActivateHightlight()
        {
            if (!clockController.TimePlaying)
            {
                OnPlayPauseTime();
            }

            _highlightSpeedActivate = !_highlightSpeedActivate;
            hightlightChangeSpeed.SetActive(_highlightSpeedActivate);
            if (_highlightSpeedActivate)
            {
                SystemEventController.Instance.DispatchSystemEvent(EditionSubStateBase.EventSubStateBaseEnableMovement, false);
            }
            else
            {
                SystemEventController.Instance.DispatchSystemEvent(EditionSubStateBase.EventSubStateBaseEnableMovement, true);
            }
        }

        private void ShowTitleFeedbackMessage(string message)
        {
            if (_showingMessage.Length > 0)
            {
                titleFeedback.text = _showingMessage;
            }
            else
            {
                titleFeedback.text = message;
            }
            if (titleFeedback.text != null)
            {
                containerFeedback.SetActive(titleFeedback.text.Length > 0);
            }
            else
            {
                containerFeedback.SetActive(false);                
            }            
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(SubEventTimeHUDUpdatedCompanyDescription))
            {
                ScreenInformationResponses userResponse = (ScreenInformationResponses)parameters[1];
                if (userResponse == ScreenInformationResponses.Confirm)
                {
                    WorkDayData.Instance.CurrentProject.Company = (string)parameters[2];
                }
            }
            if (nameEvent.Equals(EventTimeHUDEnableSpeedUpToggle))
            {
                toggleSpeedUp.interactable = (bool)parameters[0];
            }
            if (nameEvent.Equals(EventTimeHUDShowSlider))
            {
                if (_speedUpActivated)
                {
                    if (WorkDayData.Instance.CurrentProject.StartDayTrigger)
                    {
                        sliderSpeed.gameObject.SetActive((MeetingController.Instance.MeetingsInProgress.Count == 0));
                    }
                    else
                    {
                        sliderSpeed.gameObject.SetActive(false);
                    }
                }
            }
            if (nameEvent.Equals(EventTimeHUDShowMessage))
            {
                _showingMessage = (string)parameters[0];
                if (parameters.Length > 1)
                {
                    _enableShorcutAction = (bool)parameters[1];
                }
                else
                {
                    _enableShorcutAction = false;
                }
                ShowTitleFeedbackMessage(_showingMessage);
            }
            if (nameEvent.Equals(ItemTaskView.EventItemTaskViewAllRefresher))
            {
                RefreshCurrentTask();
            }
            if (nameEvent.Equals(ScreenInfoItemView.EventScreenInfoItemViewReportExpandedInfo))
            {
                _expandedInformationForSelection = (bool)parameters[0];
            }
            if (nameEvent.Equals(EventTimeHUDUpdateEnableInteraction))
            {
                if (_lockedInteraction) return;

                bool interaction = (bool)parameters[0];
                if (ApplicationController.Instance.LevelView.IsReadyToPlay() != LevelView.CodeLevelReady.Ready)
                {
                    interaction = false;
                }
                if (CommandsController.Instance.CurrentCommandState != CommandsController.CommandStates.Idle)
                {
                    interaction = false;
                }
                buttonBuild.interactable = interaction;
                buttonCalendar.interactable = interaction;
                buttonTasks.interactable = interaction;
                buttonDocuments.interactable = interaction;
                buttonAIChat.interactable = interaction;
                buttonGroups.interactable = interaction;
                buttonExpand.interactable = interaction;
                buttonSave.interactable = interaction;
                buttonExit.interactable = interaction;
                buttonCheckCosts.interactable = interaction;
                buttonRotateLeft.interactable = true;
                buttonRotateRight.interactable = true;
                buttonCompanyInfo.interactable = interaction;
                buttonSetDate.interactable = interaction;
                buttonProjectTitle.interactable = interaction;
                buttonCurrentTask.interactable = interaction;
                buttonTitleSelection.interactable = interaction;
                buttonCurrentMeeting.interactable = interaction;
                buttonPlayTime.interactable = interaction;
                buttonShowMeetings.interactable = interaction;
                buttonShowTasks.interactable = interaction;
                toggleSpeedUp.interactable = interaction;
                if (ApplicationController.Instance.LevelView.IsReadyToPlay() != LevelView.CodeLevelReady.Ready)
                {
                    buttonBuild.interactable = true;
                    buttonSave.interactable = true;
                    buttonExit.interactable = true;
                }
                if (CommandsController.Instance.CurrentCommandState != CommandsController.CommandStates.Idle)
                {
                    toggleSpeedUp.interactable = true;
                }
                if (ApplicationController.Instance.IsPlayMode)
                {
                    buttonBuild.interactable = false;
                }
            }
            if (nameEvent.Equals(ScreenCalendarView.EventScreenCalendarViewCreateMeeting))
            {
                SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
            }
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(WorkDayData.EventWorkDayDataUpdatedLLMProvider))
            {
                ShowTitleFeedbackMessage(WorkDayData.Instance.GetLLMProviderName(WorkDayData.Instance.LlmProvider));
            }
            if (nameEvent.Equals(CommandsController.EventCommandsControllerEnteringOffice)
                || nameEvent.Equals(CommandsController.EventCommandsControllerEndingDay))
            {
                sliderSpeed.gameObject.SetActive(false);
            }
            if (nameEvent.Equals(EventMeetingControllerMeetingStopped))
            {
                if (_speedUpActivated)
                {
                    sliderSpeed.gameObject.SetActive((MeetingController.Instance.MeetingsInProgress.Count == 0));
                }
            }
            if (nameEvent.Equals(EventMeetingControllerMeetingStarted))
            {
                if (_speedUpActivated)
                {
                    sliderSpeed.gameObject.SetActive((MeetingController.Instance.MeetingsInProgress.Count == 0));
                }
            }
            if (nameEvent.Equals(CommandsController.EventCommandsControllerUpdatedCommands))
            {
                List<IGameCommand> commands = CommandsController.Instance.GetAllCommands();
                string summaryCommands = "TOTAL COMMANDS[" + commands.Count + "]";
                foreach (IGameCommand command in commands)
                {
                    summaryCommands += "\n" + command.Name + ":" + ((command.Member != null) ? command.Member : "");
                }
                debugCommands.text = summaryCommands;
            }
            if (nameEvent.Equals(AICommandsController.EventAICommandsControllerUpdatedCommands))
            {
                List<IAICommand> aicommands = AICommandsController.Instance.Commands;
                string summaryAICommands = "TOTAL AI COMMANDS[" + aicommands.Count + "]";
                foreach (IAICommand command in aicommands)
                {
                    summaryAICommands += "\n" + command.Name;
                }
                debugAICommands.text = summaryAICommands;
            }
            if (nameEvent.Equals(MeetingController.EventMeetingControllerMeetingStarted)
                || nameEvent.Equals(MeetingController.EventMeetingControllerMeetingStopped)
                || nameEvent.Equals(MeetingController.EventMeetingControllerRunAction))
            {
                List<MeetingInProgress> meetings = MeetingController.Instance.MeetingsInProgress;                
                string summaryMeetings = "TOTAL MEETINGS[" + meetings.Count + "]";
                foreach (MeetingInProgress meeting in meetings)
                {                    
                    summaryMeetings += "\n" + meeting.Meeting.Name + "("+ meeting.Meeting.Iterations + "/" + meeting.Meeting.TotalIterations + "):" + meeting.Meeting.ExtraData;
                }
                debugMeetings.text = summaryMeetings;
            }            
            if (nameEvent.Equals(ClockController.EventClockControllerTimeSpeedUp))
            {                
                toggleSpeedUp.onValueChanged.RemoveListener(OnSpeedUpChanged);
                toggleSpeedUp.isOn = (bool)parameters[0];
                toggleSpeedUp.onValueChanged.AddListener(OnSpeedUpChanged);
            }
            if (nameEvent.Equals(EventTimeHUDUpdatePlayTime))
            {
                if (_blockTimeInteraction) return;
                clockController.TimePlaying = (bool)parameters[0];
                UpdateVisibilityTimePlaying();
            }
            if (nameEvent.Equals(ClockController.EventClockControllerEndingDay))
            {
                if (_highlightSpeedActivate)
                {
                    OnActivateHightlight();
                }
            }
            if (nameEvent.Equals(ScreenDialogView.EventScreenDialogViewStarted))
            {
                _expandedSelection = false;
                _expandedInformationForSelection = false;                
                if (_highlightSpeedActivate)
                {
                    OnActivateHightlight();
                }
            }
            if (nameEvent.Equals(EventTimeHUDResetTimeIncrement))
            {
                _speedUpActivated = false;
                UpdateVisibilityBGSpeed();
                clockController.ResetIncrementTime();
                toggleSpeedUp.onValueChanged.RemoveListener(OnSpeedUpChanged);
                toggleSpeedUp.isOn = false;
                toggleSpeedUp.onValueChanged.AddListener(OnSpeedUpChanged);
            }
            if (nameEvent.Equals(MeetingController.EventMeetingControllerMeetingStarted)
                || nameEvent.Equals(MeetingController.EventMeetingControllerMeetingStopped))
            {
                RefreshCurrentMeeting();
            }
            if (nameEvent.Equals(ScreenInfoItemView.EventScreenInfoItemViewReportDestroyed))
            {
                _expandedSelection = false;
            }
            if (nameEvent.Equals(EventTimeHUDUpdateCurrentTime))
            {
                WorkDayData.Instance.CurrentProject.StartDayTrigger = false;
                WorkDayData.Instance.CurrentProject.LunchDayTrigger = false;
                WorkDayData.Instance.CurrentProject.EndDayTrigger = false;
                clockController.TimePlaying = false;
                UpdateVisibilityTimePlaying();
                clockController.Initialize();
            }
            if (nameEvent.Equals(ScreenPanelEditionView.EventScreenPanelEditionActivation))
            {
                _editionMode = (bool)parameters[0];
                if (_editionMode)
                {
                    ApplicationController.Instance.SunLight.transform.rotation = Quaternion.Euler(50, -30, 0);
                    if (clockController.TimePlaying)
                    {
                        clockController.TimePlaying = false;
                        UpdateVisibilityTimePlaying();

                        clockController.ResetIncrementTime();
                        _speedUpActivated = false;
                        UpdateVisibilityBGSpeed();
                    }
                }
                clockController.gameObject.SetActive(!_editionMode);
            }
            if (nameEvent.Equals(ScreenProjectsView.EventScreenProjectsViewLoadProject))
            {
                _selectedProject = (ProjectInfoData)parameters[0];                
                titleProject.text = Utilities.ShortenText(_selectedProject.Name, 20);
            }
            if (nameEvent.Equals(EventTimeHUDCancelSelectionObject))
            {
                ResetReferences();
                ApplicationController.Instance.SelectedHuman = null;
                ApplicationController.Instance.SelectionBox.SetActive(false);
            }
            if (nameEvent.Equals(RunStateRun.EventRunStateRunDeleteHuman))
            {
                ResetReferences();
            }
            if (nameEvent.Equals(EventTimeHUDUpdateResetBoundingBox))
            {
                ApplicationController.Instance.SelectionBox.transform.eulerAngles = Vector3.zero;
            }
            if (nameEvent.Equals(EventTimeHUDSelectionObject))
            {
                SystemEventController.Instance.DispatchSystemEvent(ScreenInfoItemView.EventScreenInfoItemViewOnlyDestroyView);
                if (parameters.Length == 0)
                {
                    ApplicationController.Instance.SelectionBox.SetActive(false);
                    ResetReferences();
                }
                else
                {
                    ResetReferences();
                    _selectedGO = (GameObject)parameters[0];
                    buttonExpand.gameObject.SetActive(true);
                    buttonTitleSelection.gameObject.SetActive(true);
                    titleSelection.text = LanguageController.Instance.GetText("text.no.selection");
                    titleCurrentTaks.text = LanguageController.Instance.GetText("text.selection.no.current.task");
                    titleCurrentMeeting.text = "";
                    if (parameters[1] is WorldItemData)
                    {
                        WorldItemData sItem = (WorldItemData)parameters[1];
                        if (!_editionMode)
                        {
                            if (!sItem.IsHuman && !sItem.IsChair)
                            {
                                buttonExpand.gameObject.SetActive(false);
                                buttonTitleSelection.gameObject.SetActive(false);
                                _selectedGO = null;
                                return;
                            }
                        }

                        SelectHuman(_selectedGO, (WorldItemData)parameters[1]);
                    }
                    else
                    {
                        if (parameters[1] is AreaData)
                        {
                            _selectedArea = (AreaData)parameters[1];
                            titleSelection.text = _selectedArea.Name;
                        }
                    }
                    if (_editionMode || _expandedSelection)
                    {
                        OnExpandSelection();
                        if (_expandedInformationForSelection)
                        {
                            UIEventController.Instance.DispatchUIEvent(ScreenInfoItemView.EventScreenInfoItemViewRequestExpandedInfo);
                        }
                    }
                }
            }
        }


        public void Run()
        {
            clockController.Run();

            if (panelMinimized.activeSelf)
            {
                textHour.text = clockController.HourText;
                textMinute.text = clockController.MinuteText;
            }

            if (_highlightSpeedActivate)
            {
                if (Input.mouseScrollDelta.y > 0)
                {
                    _speedUpActivated = true;
                    UpdateVisibilityBGSpeed();
                    clockController.IncreaseTimeSpan(20);
                }
                if (Input.mouseScrollDelta.y < 0)
                {
                    clockController.IncreaseTimeSpan(-20);

                    if (clockController.IncrementTime.TotalSeconds == 1)
                    {
                        _speedUpActivated = false;
                        UpdateVisibilityBGSpeed();
                    }
                }
            }
        }
    }
}