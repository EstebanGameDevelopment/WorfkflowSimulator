using yourvrexperience.Utils;
using UnityEngine;
using System.Collections.Generic;
using System;
using static yourvrexperience.WorkDay.EditionSubStateAreas;

namespace yourvrexperience.WorkDay
{
	public class CommandsController : MonoBehaviour
	{
        public const string EventCommandsControllerAddCommand = "EventCommandsControllerAddCommand";
        public const string EventCommandsControllerActorsInTheWay = "EventCommandsControllerActorsInTheWay";
        public const string EventCommandsControllerConfirmationExit = "EventCommandsControllerConfirmationExit";
        public const string EventCommandsControllerUpdatedCommands = "EventCommandsControllerUpdatedCommands";
        public const string EventCommandsControllerAllHumansReady = "EventCommandsControllerAllHumansReady";
        public const string EventCommandsControllerEnteringOffice = "EventCommandsControllerEnteringOffice";
        public const string EventCommandsControllerEndingDay = "EventCommandsControllerEndingDay";
        public const string EventCommandsControllerRecheckRestore = "EventCommandsControllerRecheckRestore";

        public enum CommandStates { Idle = 0, EnteringOffice = 1, LunchTime = 2, LeavingOffice = 3}

		private static CommandsController _instance;

		public static CommandsController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(CommandsController)) as CommandsController;
				}
				return _instance;
			}
		}

        private Dictionary<MeetingData, List<IGameCommand>> _meetingCommands = new Dictionary<MeetingData, List<IGameCommand>>();
        private List<IGameCommand> _singleCommands = new List<IGameCommand>();
        private List<string> _actorsInTheWay = new List<string>();
        private List<string> _membersInLunchTime = new List<string>();
        private CommandStates _currentCommandState = CommandStates.Idle;
        private MeetingData _lunchMeeting;
        private string _nameMeetingLunch = null;

        public CommandStates CurrentCommandState
        {
            get { return _currentCommandState; }
        }

        public void Initialize()
		{
			SystemEventController.Instance.Event += OnSystemEvent;
			UIEventController.Instance.Event += OnUIEvent;
		}

        void OnDestroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
            _lunchMeeting = null;
        }

        private void ClearAllCommands()
        {
            foreach (KeyValuePair<MeetingData, List<IGameCommand>> commandsForMeeting in _meetingCommands)
            {
                foreach(IGameCommand command in commandsForMeeting.Value)
                {
                    if (command != null)
                    {
                        command.Destroy();
                    }
                }
            }
            _meetingCommands.Clear();
            for (int j = 0; j < _singleCommands.Count; j++)
            {
                IGameCommand command = _singleCommands[j];
                if (command != null)
                {
                    command.Destroy();
                }                
            }
            _singleCommands.Clear();
        }

        public List<IGameCommand> GetAllCommands()
        {
            List<IGameCommand> finalCommands = new List<IGameCommand>();
            foreach (KeyValuePair<MeetingData, List<IGameCommand>> commandsForMeeting in _meetingCommands)
            {
                foreach (IGameCommand command in commandsForMeeting.Value)
                {
                    if (command != null)
                    {
                        finalCommands.Add(command);
                    }
                }
            }
            for (int j = 0; j < _singleCommands.Count; j++)
            {
                IGameCommand command = _singleCommands[j];
                if (command != null)
                {
                    finalCommands.Add(command);
                }
            }
            return finalCommands;
        }

        private int CountAllCommands()
        {
            int total = 0;
            foreach (KeyValuePair<MeetingData, List<IGameCommand>> commandsForMeeting in _meetingCommands)
            {
                total += commandsForMeeting.Value.Count;
            }
            total += _singleCommands.Count;
            return total;
        }

        public void ClearAllMovementCommands()
        {
            for (int j = 0; j < _singleCommands.Count; j++)
            {
                if (_singleCommands[j] is CommandMoveCameraToPosition)
                {
                    _singleCommands[j].Destroy();
                    _singleCommands.RemoveAt(j);
                    j--;
                }
            }
            _singleCommands.Clear();
        }

        public bool CheckExistingCommandGoToChair(string member)
        {
            for (int j = 0; j < _singleCommands.Count; j++)
            {
                IGameCommand command = _singleCommands[j];
                if ((command.Member != null) && (command.Member.Length > 0) && (command.Member.Equals(member)))
                {
                    if (command is CommandGoToOwnChair)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void RequestDestruction(IGameCommand command, string member)
        {
            if (command != null)
            {
                if (((command.Member != null) && (command.Member.Length > 0) && command.Member.Equals(member)))
                {
                    if (!command.Prioritary)
                    {
                        command.RequestDestruction = true;
                    }
                }
            }
        }

        private void DestroyNonPriorityCommandsFor(string member)
        {
            foreach (KeyValuePair<MeetingData, List<IGameCommand>> commandsForMeeting in _meetingCommands)
            {
                foreach (IGameCommand command in commandsForMeeting.Value)
                {
                    if (command != null)
                    {
                        RequestDestruction(command, member);
                    }
                }
            }
            for (int j = 0; j < _singleCommands.Count; j++)
            {
                IGameCommand command = _singleCommands[j];
                if (command != null)
                {
                    RequestDestruction(command, member);
                }
            }
        }

        private List<string> GetFreeHumans(bool onlyAssholes, bool considerRested)
        {
            List<WorldItemData> humans = ApplicationController.Instance.LevelView.GetHumans();

            List<string> humansFree = new List<string>();
            foreach (WorldItemData human in humans)
            {
                if (human.IsAvailable && !human.IsPlayer)
                {
                    if (MeetingController.Instance.GetMeetingOfHuman(human.Name) == null)
                    {
                        if (!_actorsInTheWay.Contains(human.Name))
                        {
                            var (ownedChairGO, ownedChairData) = ApplicationController.Instance.LevelView.GetItemByOwner(human.Name);
                            if (ownedChairGO != null)
                            {
                                if (onlyAssholes)
                                {
                                    if (human.IsAsshole)
                                    {
                                        humansFree.Add(human.Name);
                                    }
                                }
                                else
                                {
                                    if (considerRested)
                                    {
                                        if (!human.HasRested)
                                        {
                                            humansFree.Add(human.Name);
                                        }
                                        else
                                        {
                                            human.HasRested = false;
                                        }
                                    }
                                    else
                                    {
                                        humansFree.Add(human.Name);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return humansFree;
        }

        public void CommandRandomAssholeInterrupts()
        {
            List<string> assholesFree = GetFreeHumans(true, false);

            if (assholesFree.Count > 0)
            {
                string selectedAsshole = assholesFree[UnityEngine.Random.Range(0, assholesFree.Count)];
                CommandInterruption cmdInterruption = new CommandInterruption();
                cmdInterruption.Initialize(selectedAsshole);
                SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdInterruption);
            }
        }

        public void CommandRandomHumanGoesBreak()
        {
            List<string> humansFree = GetFreeHumans(false, true);

            if (humansFree.Count > 0)
            {
                int totalHumansTakingBreak = UnityEngine.Random.Range(2, 5);
                if (humansFree.Count < totalHumansTakingBreak) totalHumansTakingBreak = humansFree.Count - 1;
                if (totalHumansTakingBreak > 1)
                {
                    DateTime dateTimeStart = WorkDayData.Instance.CurrentProject.GetCurrentTime();
                    DateTime dateTimeEnd = dateTimeStart.AddSeconds(CommandGoToBreak.TimeToBreak * 60);
                    string nameMeetingBreak = LanguageController.Instance.GetText("text.office.break") + " " + dateTimeStart.ToShortTimeString();

                    List<string> assistantsToBreak = new List<string>();
                    while (totalHumansTakingBreak > 0)
                    {
                        int humanToGoToBreak = UnityEngine.Random.Range(0, humansFree.Count);
                        string nameSelected = humansFree[humanToGoToBreak];
                        humansFree.RemoveAt(humanToGoToBreak);
                        assistantsToBreak.Add(nameSelected);
                        CommandGoToBreak cmdGoToBreak = new CommandGoToBreak();
                        cmdGoToBreak.Initialize(nameSelected, nameMeetingBreak);
                        SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdGoToBreak);
                        totalHumansTakingBreak--;
                    }

                    SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);

                    // CREATE SOCIAL BREAK MEETING
                    SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerAddMeeting, null, -1, nameMeetingBreak, LanguageController.Instance.GetText("text.office.take.break.with"), new List<DocumentData>(), dateTimeStart, dateTimeEnd, -1, assistantsToBreak.ToArray(), false, true, false, false, false);
                }
            }
        }

        public void CommandRandomHumanGoesBathroom()
        {
            List<WorldItemData> humans = ApplicationController.Instance.LevelView.GetHumans();

            List<string> humansFree = new List<string>();
            foreach (WorldItemData human in humans)
            {
                if (!human.IsPlayer)
                {
                    if (MeetingController.Instance.GetMeetingOfHuman(human.Name) == null)
                    {
                        if (!_actorsInTheWay.Contains(human.Name))
                        {
                            var (ownedChairGO, ownedChairData) = ApplicationController.Instance.LevelView.GetItemByOwner(human.Name);
                            if (ownedChairGO != null)
                            {
                                humansFree.Add(human.Name);
                            }
                        }
                    }
                }
            }

            if (humansFree.Count > 0)
            {
                int humanToGoToBathroom = UnityEngine.Random.Range(0, humansFree.Count);
                CommandGoToBathroom cmdGoToBathroom = new CommandGoToBathroom();
                cmdGoToBathroom.Initialize(humansFree[humanToGoToBathroom]);
                SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdGoToBathroom);
            }
        }

        public void CommandHumansToEnterOffice(int timeSpeed, int rangeMinutes)
		{
            _currentCommandState = CommandStates.EnteringOffice;
            UIEventController.Instance.DispatchUIEvent(TimeHUD.EventTimeHUDUpdateEnableInteraction, false);
            UIEventController.Instance.DispatchUIEvent(TimeHUD.EventTimeHUDShowMessage, LanguageController.Instance.GetText("text.entering.office"));
            ApplicationController.Instance.TimeHUD.LockedInteraction = true;

            ApplicationController.Instance.TimeHUD.IncrementTime = new TimeSpan(0, 0, timeSpeed);

            UIEventController.Instance.DelayUIEvent(TimeHUD.EventTimeHUDEnableSpeedUpToggle, 1, true);

            foreach (KeyValuePair<GameObject, WorldItemData> item in ApplicationController.Instance.LevelView.Items)
			{
				if (item.Value != null)
				{
					if (item.Value.IsHuman)
					{
                        var (chairGO, chairData) = ApplicationController.Instance.LevelView.GetItemByOwner(item.Value.Name);                        
                        if (chairGO != null)
                        {
                            CommandEnterToOffice cmdEnterTheOffice = new CommandEnterToOffice();
                            cmdEnterTheOffice.Initialize(item.Value.Name, rangeMinutes);
                            SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdEnterTheOffice);
                        }
                        else
                        {
                            item.Key.SetActive(false);
                        }
                    }
				}
			}
            SystemEventController.Instance.DispatchSystemEvent(EventCommandsControllerEnteringOffice);
		}

		public void CommandHumansToLeaveOffice(int timeSpeed, int rangeMinutes)
		{
            _currentCommandState = CommandStates.LeavingOffice;
            ApplicationController.Instance.LastProjectFeedback = "";
            ApplicationController.Instance.LastProjectColor = Color.white;

            UIEventController.Instance.DispatchUIEvent(TimeHUD.EventTimeHUDUpdateEnableInteraction, false);
            UIEventController.Instance.DispatchUIEvent(TimeHUD.EventTimeHUDShowMessage, LanguageController.Instance.GetText("text.leaving.office"));
            ApplicationController.Instance.TimeHUD.LockedInteraction = true;
            
            UIEventController.Instance.DelayUIEvent(TimeHUD.EventTimeHUDEnableSpeedUpToggle, 1, true);

            CommandFixTimeDuring cmdFixTime = new CommandFixTimeDuring();
			cmdFixTime.Initialize(timeSpeed, rangeMinutes * 60);
			SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdFixTime);

			SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerPoliteRequestToEndRunningMeetings);
            SystemEventController.Instance.DispatchSystemEvent(EventCommandsControllerEndingDay);

			foreach (KeyValuePair<GameObject, WorldItemData> item in ApplicationController.Instance.LevelView.Items)
			{
				if (item.Value != null)
				{
					if (item.Value.IsHuman)
					{
                        if (item.Key.activeSelf)
						{
                            item.Value.IsAvailable = false;
							CommandGoToExit cmdExitTheOffice = new CommandGoToExit();
							cmdExitTheOffice.Initialize(item.Value.Name, rangeMinutes, EventCommandsControllerConfirmationExit);
                            cmdExitTheOffice.Prioritary = true;
                            SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdExitTheOffice);
						}
					}
				}
			}
		}

		public void CommandHumansToGoLunch(int timeSpeed, int totalTimeLunch)
		{
            _currentCommandState = CommandStates.LunchTime;
            ApplicationController.Instance.LastProjectFeedback = "";
            ApplicationController.Instance.LastProjectColor = Color.white;

            UIEventController.Instance.DispatchUIEvent(TimeHUD.EventTimeHUDUpdateEnableInteraction, false);
            ApplicationController.Instance.TimeHUD.LockedInteraction = true;
            UIEventController.Instance.DispatchUIEvent(TimeHUD.EventTimeHUDShowMessage, LanguageController.Instance.GetText("text.lunch.time"));
            _membersInLunchTime = new List<string>();

            UIEventController.Instance.DelayUIEvent(TimeHUD.EventTimeHUDEnableSpeedUpToggle, 1, true);

            // FREEZE TIME LUNCH
            CommandFixTimeDuring cmdFixTime = new CommandFixTimeDuring();
            int totalSecondsLunch = totalTimeLunch * 60;
            cmdFixTime.Initialize(timeSpeed, totalSecondsLunch);
            SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdFixTime);
            
            // CREATE LUNCH MEETING
            DateTime dateTimeStart = WorkDayData.Instance.CurrentProject.GetCurrentTime();
            DateTime dateTimeEnd = dateTimeStart.AddSeconds(totalSecondsLunch);
            List<string> members = new List<string>();
            if ((WorkDayData.Instance.CurrentProject.Groups == null) || (WorkDayData.Instance.CurrentProject.Groups.Length == 0))
            {
                List<WorldItemData> humans = ApplicationController.Instance.LevelView.GetHumans();
                foreach(WorldItemData human in humans)
                {
                    members.Add(human.Name);
                }
            }
            else
            {
                foreach (GroupInfoData group in WorkDayData.Instance.CurrentProject.Groups)
                {
                    bool shouldIncludeGroup = true;
                    foreach (string memberInGroup in group.Members)
                    {
                        var (chairGO, chairData) = ApplicationController.Instance.LevelView.GetItemByOwner(memberInGroup);
                        if (chairGO == null)
                        {
                            shouldIncludeGroup = false;
                        }
                    }
                    if (shouldIncludeGroup)
                    {
                        members.Add(group.Name);
                    }
                }
            }
            _nameMeetingLunch = PromptController.Instance.GetNameLunch(dateTimeStart);
            MeetingData existingMeeting = WorkDayData.Instance.CurrentProject.GetMeeting(_nameMeetingLunch, dateTimeStart);
            if (existingMeeting != null)
            {
                _lunchMeeting = existingMeeting;
                _nameMeetingLunch = null;
                UIEventController.Instance.DelayUIEvent(MeetingController.EventMeetingControllerUIRequestToStartMeeting, 0.1F, existingMeeting, -1);
            }
            else
            {
                SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerAddMeeting, null, -1, _nameMeetingLunch, LanguageController.Instance.GetText("text.lunch.time.description"), new List<DocumentData>(), dateTimeStart, dateTimeEnd, -1, members.ToArray(), false, true, false, true, false);
            }

            // COMMAND TO ALL HUMANS TO LUNCH
            foreach (KeyValuePair<GameObject, WorldItemData> item in ApplicationController.Instance.LevelView.Items)
			{
				if (item.Value != null)
				{
					if (item.Value.IsHuman)
					{
                        if (item.Key.activeSelf)
                        {
                            var (chairGO, chairData) = ApplicationController.Instance.LevelView.GetItemByOwner(item.Value.Name);
                            if (chairGO != null)
                            {
                                _membersInLunchTime.Add(item.Value.Name);
                                CommandLunchTime cmdLunchTime = new CommandLunchTime();
                                cmdLunchTime.Initialize(item.Value.Name, 15, totalTimeLunch - 5);
                                SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdLunchTime);
                            }
                        }
					}
				}
			}
		}

        public AreaData FindRoomForMeeting(MeetingData meeting, string[] namesHumans)
        {
            AreaData meetingRoom = WorkDayData.Instance.CurrentProject.GetFreeMeetingRoomFor(AreaMode.Meeting, namesHumans.Length);

            if (meetingRoom != null)
            {
                meeting.RoomName = meetingRoom.Name;

                if (ApplicationController.Instance.HumanPlayer != null)
                {
                    if (meeting.IsAssistingMember(ApplicationController.Instance.HumanPlayer.NameHuman))
                    {
                        CommandMoveCameraToPosition cmdMoveCameraToMeetingRoom = new CommandMoveCameraToPosition();
                        cmdMoveCameraToMeetingRoom.Initialize(meetingRoom.GetCenter(), 3f, 5f);
                        SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdMoveCameraToMeetingRoom);
                    }
                }
            }

            foreach (string nameHuman in namesHumans)
            {
                if (meetingRoom != null)
                {
                    CommandGoToAreaChair cmdGoToChairMeetingRoom = new CommandGoToAreaChair();
                    cmdGoToChairMeetingRoom.Initialize(nameHuman, meetingRoom.Name);
                    SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdGoToChairMeetingRoom);
                }
                else
                {
                    CommandGoToOwnChair cmdGoToYourChair = new CommandGoToOwnChair();
                    cmdGoToYourChair.Initialize(nameHuman);
                    SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdGoToYourChair);
                }
            }

            return meetingRoom;
        }

        private void RestoreStateAfterActorsInWayCompleted()
        {
            if (_actorsInTheWay.Count == 0)
            {
                if (GameObject.FindAnyObjectByType<ScreenDialogView>() == null)
                {
                    UIEventController.Instance.DispatchUIEvent(TimeHUD.EventTimeHUDUpdateEnableInteraction, true);
                }
                UIEventController.Instance.DispatchUIEvent(TimeHUD.EventTimeHUDShowMessage, "");
                SystemEventController.Instance.DispatchSystemEvent(PlayerView.EventPlayerAppEnableMovement, true);
                SystemEventController.Instance.DispatchSystemEvent(EditionSubStateBase.EventSubStateBaseEnableMovement, true);
                SystemEventController.Instance.DelaySystemEvent(ScreenInfoItemView.EventScreenInfoItemViewDestroy, 0.2f);
                UIEventController.Instance.DispatchUIEvent(TimeHUD.EventTimeHUDShowSlider);
                SystemEventController.Instance.DelaySystemEvent(EventCommandsControllerRecheckRestore, 2);
            }
        }

        private void AddActorsInWay(List<string> actors)
        {
            foreach (string actor in actors)
            {
                if (!_actorsInTheWay.Contains(actor))
                {
                    _actorsInTheWay.Add(actor);
                }
            }            
        }

        public bool IsActorsInWay(string actor)
        {
            return _actorsInTheWay.Contains(actor);
        }

        public void RemoveMeetingCommands(List<IGameCommand> commands)
        {
            if (commands == null) return;

            for (int j = 0; j < commands.Count; j++)
            {
                IGameCommand command = commands[j];
                if (command != null)
                {
                    command.Destroy();
                }
            }
            commands.Clear();
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
		{
            if (nameEvent.Equals(ScreenCalendarView.EventScreenCalendarViewCreateMeeting))
            {
                MeetingData meetingCreated = (MeetingData)parameters[0];
                if ((_nameMeetingLunch != null) && (_nameMeetingLunch.Length > 0))
                {
                    if (meetingCreated.Name.Equals(_nameMeetingLunch))
                    {
                        _lunchMeeting = meetingCreated;
                        _nameMeetingLunch = null;
                        UIEventController.Instance.DispatchUIEvent(MeetingController.EventMeetingControllerUIRequestToStartMeeting, meetingCreated, -1);
                    }
                }
            }
            if (nameEvent.Equals(TimeHUD.EventTimeHUDShowClickedFeedback))
            {
                switch (_currentCommandState)
                {
                    case CommandStates.LunchTime:
                        if (_lunchMeeting != null)
                        {
                            if (!_lunchMeeting.Completed)
                            {
                                if (GameObject.FindAnyObjectByType<ScreenDialogView>() == null)
                                {
                                    if (ApplicationController.Instance.HumanPlayer != null)
                                    {
                                        // JOIN AS MEMBER
                                        SystemEventController.Instance.DispatchSystemEvent(HumanView.EventHumanViewForceSelection, ApplicationController.Instance.HumanPlayer.NameHuman);
                                        SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerJoinMeeting, _lunchMeeting, -1, WorkDayData.Instance.CurrentProject.GetCurrentTime(), ApplicationController.Instance.HumanPlayer.NameHuman);
                                    }
                                    else
                                    {
                                        // JOIN AS A OBSERVER
                                        ScreenController.Instance.DestroyScreens();
                                        ScreenController.Instance.CreateScreen(ScreenDialogView.ScreenName, false, true, _lunchMeeting);
                                        UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, ScreenDialogView.ScreenName, 10);
                                    }
                                }                                
                            }
                        }
                        break;
                }
            }
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
		{
            if (nameEvent.Equals(EventCommandsControllerRecheckRestore))
            {
                if (_actorsInTheWay.Count == 0)
                {
                    UIEventController.Instance.DispatchUIEvent(TimeHUD.EventTimeHUDUpdateEnableInteraction, true);
                }
            }
            if (nameEvent.Equals(EventCommandsControllerAddCommand))
            {
                IGameCommand newCommand = (IGameCommand)parameters[0];
                if (newCommand.Prioritary)
                {
                    DestroyNonPriorityCommandsFor(newCommand.Member);
                }
                if (newCommand.Meeting == null)
                {
                    _singleCommands.Add(newCommand);
                }
                else
                {
                    List<IGameCommand> commandsMeeting;
                    if (!_meetingCommands.TryGetValue(newCommand.Meeting, out commandsMeeting))
                    {
                        commandsMeeting = new List<IGameCommand>();
                        _meetingCommands.Add(newCommand.Meeting, commandsMeeting);
                    }
                    commandsMeeting.Add(newCommand);
                }                
                ApplicationController.Instance.TotalNumberOfCommands = CountAllCommands();
            }
            if (nameEvent.Equals(EventCommandsControllerActorsInTheWay))
            {
                MeetingData meeting = (MeetingData)parameters[0];
                AddActorsInWay((List<string>)parameters[1]);
                if (_actorsInTheWay != null)
                {
                    if (_actorsInTheWay.Count > 0)
                    {
                        UIEventController.Instance.DelayUIEvent(TimeHUD.EventTimeHUDUpdateEnableInteraction, 0.2f, false);                        
                        SystemEventController.Instance.DelaySystemEvent(ScreenInfoItemView.EventScreenInfoItemViewDestroy, 0.2f);
                        if (parameters.Length < 3)
                        {
                            SystemEventController.Instance.DispatchSystemEvent(PlayerView.EventPlayerAppEnableMovement, false);
                            SystemEventController.Instance.DispatchSystemEvent(EditionSubStateBase.EventSubStateBaseEnableMovement, false);
                            UIEventController.Instance.DispatchUIEvent(TimeHUD.EventTimeHUDShowMessage, LanguageController.Instance.GetText("text.starting.meeting"), true);
                        }
                        else
                        {
                            UIEventController.Instance.DispatchUIEvent(TimeHUD.EventTimeHUDShowMessage, LanguageController.Instance.GetText("text.back.to.work"), true);
                        }
                    }
                }
            }
            if (nameEvent.Equals(CommandGoToAreaChair.EventCommandGoToAreaChairDestinationReached))
            {
                if (_actorsInTheWay != null)
                {
                    if (_actorsInTheWay.Count > 0)
                    {
                        if (_actorsInTheWay.Remove((string)parameters[0]))
                        {
                            RestoreStateAfterActorsInWayCompleted();
                        }
                    }
                }
            }
            if (nameEvent.Equals(CommandEnterToOffice.EventCommandEnterToOfficeReachedChair))
            {
                if (ApplicationController.Instance.LevelView.CheckHumansInTheirOwnChair())
                {
                    if (_currentCommandState != CommandStates.Idle)
                    {
                        _currentCommandState = CommandStates.Idle;
                        ApplicationController.Instance.TimeHUD.LockedInteraction = false;
                        SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
                        UIEventController.Instance.DispatchUIEvent(TimeHUD.EventTimeHUDUpdateEnableInteraction, true);
                        UIEventController.Instance.DispatchUIEvent(TimeHUD.EventTimeHUDShowMessage, "");                        
                        SystemEventController.Instance.DispatchSystemEvent(EventCommandsControllerAllHumansReady);
                    }
                }
            }            
            if (nameEvent.Equals(CommandLunchTime.EventCommandLunchTimeCompleted))
            {
                string memberLunched = (string)parameters[0];
                if (_membersInLunchTime.Remove(memberLunched))
                {
                    if (_membersInLunchTime.Count == 0)
                    {
                        _currentCommandState = CommandStates.Idle;
                        ApplicationController.Instance.TimeHUD.LockedInteraction = false;
                        SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
                        UIEventController.Instance.DispatchUIEvent(TimeHUD.EventTimeHUDUpdateEnableInteraction, true);
                        UIEventController.Instance.DispatchUIEvent(TimeHUD.EventTimeHUDShowMessage, "");
                        SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerForceEndRunningSocialMeetings);                        
                        _lunchMeeting = null;                        
                    }
                }
            }
            if (nameEvent.Equals(CommandsController.EventCommandsControllerConfirmationExit))
            {
                if (ApplicationController.Instance.LevelView.HasEveryoneLeft())
                {
                    _currentCommandState = CommandStates.Idle;
                    ApplicationController.Instance.TimeHUD.ChangeToNextDay();
                }
            }
            if (nameEvent.Equals(ClockController.EventClockControllerChangedDay))
            {
                ClearAllCommands();
            }
        }

        private void Update()
        {
            if (_singleCommands != null)
            {
                // COMMAND INDEPENDENT FROM MEETINGS
                for (int i = 0; i < _singleCommands.Count; i++)
                {
                    IGameCommand command = _singleCommands[i];
                    command.Run();
                    if (command.IsCompleted() || command.RequestDestruction)
                    {
                        command.Destroy();
                        _singleCommands.RemoveAt(i);
                        ApplicationController.Instance.TotalNumberOfCommands = CountAllCommands();
                        i--;
                    }
                    else
                    {
                        if (command.IsBlocking())
                        {
                            break;
                        }
                    }
                }

                // COMMAND DEPENDENT FROM MEETINGS
                foreach (KeyValuePair<MeetingData, List<IGameCommand>> commandsForMeeting in _meetingCommands)
                {
                    if (commandsForMeeting.Key == null)
                    {
                        RemoveMeetingCommands(commandsForMeeting.Value);
                        _meetingCommands.Remove(commandsForMeeting.Key);
                        ApplicationController.Instance.TotalNumberOfCommands = CountAllCommands();
                        return;
                    }
                    if (commandsForMeeting.Key.Completed)
                    {
                        RemoveMeetingCommands(commandsForMeeting.Value);
                        _meetingCommands.Remove(commandsForMeeting.Key);
                        ApplicationController.Instance.TotalNumberOfCommands = CountAllCommands();
                        return;
                    }

                    List<IGameCommand> meetingCommands = commandsForMeeting.Value;
                    for (int j = 0; j < meetingCommands.Count; j++)
                    {
                        IGameCommand command = meetingCommands[j];
                        if (command.Meeting != null)
                        {
                            command.Run();
                            if (command.IsCompleted() || command.RequestDestruction)
                            {
                                command.Destroy();
                                meetingCommands.RemoveAt(j);
                                ApplicationController.Instance.TotalNumberOfCommands = CountAllCommands();
                                j--;
                            }
                            else
                            {
                                if (command.IsBlocking())
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}