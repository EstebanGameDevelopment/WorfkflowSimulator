using yourvrexperience.Utils;
using UnityEngine;
using System.Collections.Generic;
using static yourvrexperience.WorkDay.EditionSubStateAreas;

namespace yourvrexperience.WorkDay
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
	public class HumanView : MonoBehaviour
	{
        public const string EventHumanViewCancelActions = "EventHumanViewCancelActions";
        public const string EventHumanViewForceSelection = "EventHumanViewForceSelection";
        public const string EventHumanViewGroupUpdated = "EventHumanViewGroupUpdated";
        public const string EventHumanViewReachedDestination = "EventHumanViewReachedDestination";
        public const string EventHumanViewStopWorkingOnTask = "EventHumanViewStopWorkingOnTask";
        
        public const string EventHumanViewHideWorking = "EventHumanViewHideWorking";
        public const string EventHumanViewShowWorking = "EventHumanViewShowWorking";

        public const string AnimationIdle = "Idle";
        public const string AnimationWalk = "Walk";
        public const string AnimationSit = "Sit";

        public const float HumanSpeed = 1.5f;
        public const float HumanRotate = 0.25f;
        public const float HumanDistance = WorkDayData.SIZE_CELL;

        public enum TargetDestination { Position = 0, Human = 1, Chair = 2}

        public delegate void HumanDestinationReached(GameObject human);

        public event HumanDestinationReached DestinationReachedEvent;

        public void DispatchDestinationReachedEvent(GameObject human)
        {
            if (DestinationReachedEvent != null)
            {
                DestinationReachedEvent(human);
            }
        }

        private GameObject _targetChair;
		private GameObject _targetHuman;
		private Vector3 _targetPosition;

        private Rigidbody _rigidBody;
        private Collider _collider;
        private PathFindingMovement _navigation;
        private AnimatorSystem _animatorSystem;

        private string _nameHuman;
        private ChairView _currentChair;

        private WorldItemData _itemData;

        private HumanInfoLabelView _infoLabel;

        private float _floorPosition = 0;

        public string NameHuman
        {
            get { return _nameHuman; }
            set { _nameHuman = value;
                RefreshLabel();
            }
        }
        public ChairView CurrentChair
        {
            get { return _currentChair; }
        }
        public WorldItemData ItemData
        {
            get { return _itemData; }
        }
        
        public void Initialize(WorldItemData itemData)
        {
            _itemData = itemData;
            _rigidBody = this.GetComponent<Rigidbody>();
            _collider = this.GetComponent<Collider>();
            _navigation = this.GetComponent<PathFindingMovement>();
            if (_navigation == null)
            {
                _navigation = this.gameObject.AddComponent<PathFindingMovement>();
            }
            _navigation.Initialization(0, false, true);
            _animatorSystem = this.GetComponentInChildren<AnimatorSystem>();
            _rigidBody.isKinematic = true;
            _nameHuman = _itemData.Name;
            _currentChair = null;
            
            _floorPosition = ApplicationController.Instance.LevelView.InitialPosition.transform.position.y + (_collider.bounds.size.y / 2);

            SystemEventController.Instance.Event += OnSystemEvent;
            UIEventController.Instance.Event += OnUIEvent;

            if (_itemData.IsHuman && _itemData.IsPlayer)
            {
                ApplicationController.Instance.HumanPlayer = this;
            }

            if (_itemData.IsHuman)
            {
                _infoLabel = LevelEditionController.Instance.CreateInfoHuman().GetComponent<HumanInfoLabelView>();
                _infoLabel.transform.parent = this.transform;
                _infoLabel.transform.localPosition = new Vector3(0, 1.5f, 0);
                _infoLabel.transform.localScale = new Vector3(10, 5, 10);

                RefreshLabel();
            }
        }

        private void OnDestroy()
        {
            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
            if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

            _itemData = null;
            _currentChair = null;
        }

        private void PrepareBodyToMove()
        {
            _animatorSystem.transform.localEulerAngles = new Vector3(0, 90, 0);
        }

        private void PrepareBodyToIdle()
        {
            _animatorSystem.transform.localEulerAngles = new Vector3(0, 0, 0);
        }

        private void ChangeAnimation(string targetAnimation)
        {
            if (_animatorSystem == null) return;

            switch (targetAnimation)
            {
                case AnimationIdle:
                    _animatorSystem.transform.localPosition = new Vector3(0, -1, 0);
                    PrepareBodyToIdle();
                    break;

                case AnimationWalk:
                    _animatorSystem.transform.localPosition = new Vector3(0, -1, 0);
                    PrepareBodyToMove();
                    break;

                case AnimationSit:
                    _animatorSystem.transform.localPosition = new Vector3(0, -0.75f, 0);
                    PrepareBodyToIdle();
                    break;
            }

            _animatorSystem.ChangeAnimation(targetAnimation);
        }

        public bool IsMoving()
        {
            return _navigation.IsMoving();
        }

        public TaskProgressData GetActiveTask()
        {
            return _itemData.GetActiveTask();
        }

        public List<TimeWorkingDataDisplay> GetAllTimeWorkedLogs(int idProject)
        {
            return _itemData.GetAllLoggedWorkRecords(idProject);
        }

        public TimeWorkingDataDisplay GetCurrentTaskProgress(int idProject)
        {
            return _itemData.GetCurrentTaskProgress(idProject);
        }

        public void Teleport(Vector3 position)
        {
            this.gameObject.transform.position = position;
            _navigation.Force();
            ChangeAnimation(AnimationIdle);
        }

        public void SetInitialChair(GameObject targetChair)
        {
            this.gameObject.transform.position = targetChair.transform.position;
            this.gameObject.transform.eulerAngles = targetChair.transform.eulerAngles;
        }

        public void SetChair(GameObject targetChair)
        {
            if (_currentChair != null)
            {
                _currentChair.SetHuman(null);
            }
            _currentChair = null;
            _targetChair = targetChair;
            _targetPosition = Vector3.zero;
            _targetHuman = null;

            _currentChair = _targetChair.GetComponent<ChairView>();
            _currentChair.SetHuman(this);
        }

        public void StopMovement()
        {
            _navigation.Stop();
            if (_animatorSystem != null)
            {
                if (_animatorSystem.CurrentTriggerAnimation != null)
                {
                    if (!_animatorSystem.CurrentTriggerAnimation.Equals(AnimationSit))
                    {
                        ChangeAnimation(AnimationIdle);
                    }
                }
            }
        }

        public void GoToChair(GameObject targetChair)
        {
            this.transform.position = new Vector3(this.transform.position.x, _floorPosition, this.transform.position.z);

            if (_currentChair != null)
            {
                _currentChair.SetHuman(null);
            }
            _currentChair = null;
            _targetChair = targetChair;
            _targetPosition = Vector3.zero;
            _targetHuman = null;

            if (_targetChair != null)
            {
                if (_navigation.GoTo(this.transform.position, targetChair.transform.position, HumanSpeed, HumanRotate, HumanDistance))
                {
                    ChangeAnimation(AnimationWalk);
                    _currentChair = _targetChair.GetComponent<ChairView>();
                    if (_currentChair != null)
                    {
                        _currentChair.SetHuman(this);
                    }
                }
                else
                {
                    _targetChair = null;
                    ChangeAnimation(AnimationSit);
                }
            }
        }

        public void GoToPosition(Vector3 position)
        {            
            this.transform.position = new Vector3(this.transform.position.x, _floorPosition, this.transform.position.z);

            if (_currentChair != null)
            {
                _currentChair.SetHuman(null);
            }
            _currentChair = null;
            _targetChair = null;
            _targetPosition = position;
            _targetHuman = null;

            if (_navigation.GoTo(this.transform.position, _targetPosition, HumanSpeed, HumanRotate, HumanDistance))
            {
                ChangeAnimation(AnimationWalk);
                _currentChair = _targetChair.GetComponent<ChairView>();
                if (_currentChair != null)
                {
                    _currentChair.SetHuman(this);
                }
            }
            else
            {
                _targetChair = null;
                ChangeAnimation(AnimationIdle);
            }
        }

        public void GoToHuman(GameObject targetHuman)
        {
            this.transform.position = new Vector3(this.transform.position.x, _floorPosition, this.transform.position.z);

            if (_currentChair != null)
            {
                _currentChair.SetHuman(null);
            }
            _currentChair = null;
            _targetHuman = targetHuman;
            _targetChair = null;
            _targetPosition = Vector3.zero;
            _navigation.enabled = true;
            ChangeAnimation(AnimationWalk);

            if (_navigation.GoTo(this.transform.position, _targetHuman.transform.position, HumanSpeed, HumanRotate, HumanDistance))
            {
                ChangeAnimation(AnimationWalk);
                _currentChair = _targetChair.GetComponent<ChairView>();
                if (_currentChair != null)
                {
                    _currentChair.SetHuman(this);
                }
            }
            else
            {
                _targetChair = null;
                ChangeAnimation(AnimationIdle);
            }
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(ScreenDialogView.EventScreenDialogViewAddAIInteraction))
            {
                if (parameters.Length > 1)
                {
                    MeetingData meeting = (MeetingData)parameters[0];
                    if (meeting.IsAssistingMember(NameHuman))
                    {
                        InteractionData dataDialog = (InteractionData)parameters[1];
                        if (_infoLabel != null)
                        {
                            if (_itemData.Name.Equals(dataDialog.NameActor))
                            {
                                _infoLabel.ShowDialog(dataDialog.Text);
                            }
                            else
                            {
                                _infoLabel.HideDialog();
                            }
                        }
                    }
                }
            }
            if (nameEvent.Equals(MeetingController.EventMeetingControllerMeetingStopped))
            {
                if (_infoLabel != null)
                {
                    _infoLabel.HideDialog();
                }                
            }
            if (nameEvent.Equals(HumanView.EventHumanViewStopWorkingOnTask))
            {
                int taskID = (int)parameters[0];
                TaskProgressData currentTask = _itemData.GetTaskProgressByID(taskID);
                if (currentTask != null)
                {
                    currentTask.StopProgress("", WorkDayData.Instance.CurrentProject.GetCurrentTime());
                }
            }
            if (nameEvent.Equals(MeetingController.EventMeetingControllerMeetingsLoaded))
            {
                MeetingData meetingInProgress = MeetingController.Instance.GetMeetingOfHuman(_itemData.Name);
                if (meetingInProgress == null)
                {
                    if (WorkDayData.Instance.CurrentProject.HasDayStarted())
                    {
                        if (CommandsController.Instance.CurrentCommandState == CommandsController.CommandStates.Idle)
                        {
                            var (itemGO, itemData) = ApplicationController.Instance.LevelView.GetItemByOwner(NameHuman);
                            if (itemGO != null)
                            {                                
                                CommandGoToOwnChair cmdGoToYourChair = new CommandGoToOwnChair();
                                cmdGoToYourChair.Initialize(NameHuman);
                                SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdGoToYourChair);
                            }
                            else
                            {
                                this.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
            if (nameEvent.Equals(EditionSubStateIdle.EventSubStateIdleResponseToGoPosition))
            {
                if (ApplicationController.Instance.HumanPlayer != null)
                {
                    if (ApplicationController.Instance.HumanPlayer.NameHuman.Equals(NameHuman))
                    {
                        if (ApplicationController.Instance.SelectedHuman == ApplicationController.Instance.HumanPlayer)
                        {
                            if (_navigation.Move())
                            {
                                SystemEventController.Instance.DispatchSystemEvent(LevelView.EventLevelViewLinesRequestDestroy);
                                if (parameters[0] is Vector3)
                                {
                                    Vector3 target = (Vector3)parameters[0];
                                    GoToPosition(target);
                                }
                                else
                                {
                                    GameObject itemTarget = (GameObject)parameters[0];
                                    if (itemTarget.GetComponent<HumanView>() != null)
                                    {
                                        GoToHuman(itemTarget);
                                    }
                                    else
                                    {
                                        GoToChair(itemTarget);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (nameEvent.Equals(MeetingController.EventMeetingControllerMeetingStarted)
                || nameEvent.Equals(MeetingController.EventMeetingControllerMeetingStopped)
                || nameEvent.Equals(ClockController.EventClockControllerHour)
                || nameEvent.Equals(EventHumanViewGroupUpdated)
                || nameEvent.Equals(TasksController.EventTasksControllerStartCustomTask)
                || nameEvent.Equals(TasksController.EventTasksControllerStartTask)
                || nameEvent.Equals(TasksController.EventTasksControllerStoppedTask)
                || nameEvent.Equals(TasksController.EventTasksControllerStartedTask))
            {
                if (_itemData != null)
                {                    
                    if (_itemData.IsHuman)
                    {
                        RefreshLabel();
                    }
                }
            }
            if (nameEvent.Equals(TimeHUD.EventTimeHUDUpdateCurrentTime))
            {
                Invoke("RefreshLabel", 0.1f);
            }
            if (nameEvent.Equals(ScreenInfoItemView.EventScreenInfoItemViewHumanPlayerAssigned))
            {
                if (_itemData.IsHuman && _itemData.IsPlayer)
                {
                    ApplicationController.Instance.HumanPlayer = this;
                }
            }
            if (nameEvent.Equals(EventHumanViewCancelActions))
            {
                _navigation.Stop();
                ChangeAnimation(AnimationIdle);
            }
            if (nameEvent.Equals(EventHumanViewForceSelection))
            {                                
                if (NameHuman.Equals((string)parameters[0]))
                {
                    bool shouldCenterCamera = false;
                    if (parameters.Length > 1)
                    {
                        shouldCenterCamera = (bool)parameters[1];
                    }
                    SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDSelectionObject, this.gameObject, _itemData);
                    if (shouldCenterCamera)
                    {
                        CommandsController.Instance.ClearAllMovementCommands();
                        CommandMoveCameraToPosition cmdMoveCameraToHuman = new CommandMoveCameraToPosition();
                        cmdMoveCameraToHuman.Initialize(this.gameObject.transform.position, 1f, 5f);
                        SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdMoveCameraToHuman);
                    }
                }                
            }
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(ScreenCalendarView.EventScreenCalendarViewSetNewDate))
            {
                ChangeAnimation(AnimationIdle);
            }
            if (nameEvent.Equals(EventHumanViewHideWorking))
            {
                if (NameHuman.Equals((string)parameters[0]))
                {
                    _infoLabel.SetWorking(false);
                }
            }
            if (nameEvent.Equals(EventHumanViewShowWorking))
            {
                if (NameHuman.Equals((string)parameters[0]))
                {
                    _infoLabel.SetWorking(true);
                }
            }            
            if (nameEvent.Equals(ScreenCalendarView.EventScreenCalendarViewSetNewDate))
            {
                if (!(bool)parameters[0])
                {
                    _itemData.Reset();
                }
            }
        }

        private void RefreshLabel()
        {
            TimeWorkingDataDisplay taskProgress = _itemData.GetCurrentTaskProgress(-1);
            if (taskProgress == null)
            {                
                _infoLabel.SetData(_itemData.Name, LanguageController.Instance.GetText("text.hud.no.task.assigned"), Color.grey);
                _infoLabel.SetWorking(false);
            }
            else
            {
                var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(taskProgress.TaskUID);
                if (taskItemData == null)
                {
                    _infoLabel.SetData(_itemData.Name, LanguageController.Instance.GetText("text.hud.no.task.assigned"), Color.grey);
                    _infoLabel.SetWorking(false);
                }
                else
                {
                    float totalHoursDone = _itemData.GetTotalDecimalHoursProgressForTask(-1, taskProgress.TaskUID);
                    Color finalColor = Color.black;
                    if (taskItemData.EstimatedTime < totalHoursDone)
                    {
                        finalColor = Color.red;
                    }
                    _infoLabel.SetData(_itemData.Name, Utilities.ShortenText(taskItemData.Name, 20) + " " + Utilities.CeilDecimal(totalHoursDone, 1) + "h", finalColor);
                    _infoLabel.SetWorking(true);
                }
            }
        }

        private void Update()
        {
            if (_targetPosition != Vector3.zero)
            {
                if (_navigation.Move())
                {
                    _targetPosition = Vector3.zero;
                    _navigation.Stop();
                    _targetHuman = null;
                    _targetChair = null;
                    ChangeAnimation(AnimationIdle);

                    DispatchDestinationReachedEvent(this.gameObject);

                    SystemEventController.Instance.DispatchSystemEvent(EventHumanViewReachedDestination, this, _itemData.Name, TargetDestination.Position);
                }
            }
            if (_targetHuman != null)
            {
                if (_navigation.Move())
                {
                    GameObject targetHuman = _targetHuman;
                    _navigation.Stop();
                    _targetHuman = null;
                    _targetChair = null;
                    ChangeAnimation(AnimationIdle);

                    DispatchDestinationReachedEvent(this.gameObject);

                    SystemEventController.Instance.DispatchSystemEvent(EventHumanViewReachedDestination, this, _itemData.Name, TargetDestination.Human, targetHuman);
                }
            }
            if (_targetChair != null)
            {
                if (_navigation.Move())
                {
                    if ((_currentChair != null) && (_currentChair.Area != null))
                    {
                        if ((AreaMode)_currentChair.Area.Type == AreaMode.Exit)
                        {
                            _currentChair.OpenDoor();
                            this.gameObject.SetActive(false);
                        }
                        else
                        {
                            // SIT THE HUMAN IN THE CHAIR
                            this.gameObject.transform.position = _targetChair.transform.position + new Vector3(0, 0.35f, 0);
                            this.gameObject.transform.eulerAngles = _targetChair.transform.eulerAngles + new Vector3(0, -90, 0);
                        }
                    }
                    else
                    {
                        // SIT THE HUMAN IN THE CHAIR
                        this.gameObject.transform.position = _targetChair.transform.position + new Vector3(0, 0.35f, 0);
                        this.gameObject.transform.eulerAngles = _targetChair.transform.eulerAngles + new Vector3(0, -90, 0);
                    }
                    _targetChair = null;
                    ChangeAnimation(AnimationSit);

                    if (ApplicationController.Instance.HumanPlayer != null)
                    {
                        if (ApplicationController.Instance.HumanPlayer == ApplicationController.Instance.SelectedHuman)
                        {
                            SystemEventController.Instance.DelaySystemEvent(TimeHUD.EventTimeHUDUpdateResetBoundingBox, 0.1f);
                        }
                    }

                    DispatchDestinationReachedEvent(this.gameObject);

                    SystemEventController.Instance.DispatchSystemEvent(EventHumanViewReachedDestination, this, _itemData.Name, TargetDestination.Chair);
                }
            }
        }
    }
}
