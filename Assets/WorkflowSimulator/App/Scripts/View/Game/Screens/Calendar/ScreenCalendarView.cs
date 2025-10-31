using Maything.UI.CalendarSchedulerUI;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
using yourvrexperience.VR;

namespace yourvrexperience.WorkDay
{
	public class ScreenCalendarView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenCalendarView";
		
		public const string EventScreenCalendarViewSetNewDate = "EventScreenCalendarViewSetNewDate";
		public const string EventScreenCalendarViewNewDateConfirmed = "EventScreenCalendarViewNewDateConfirmed";
		public const string EventScreenCalendarViewDelayedLoadData = "EventScreenCalendarViewDelayedLoadData";
		public const string EventScreenCalendarViewMonthChanged = "EventScreenCalendarViewMonthChanged";
		public const string EventScreenCalendarViewGenerationCompleted = "EventScreenCalendarViewGenerationCompleted";

		public const string EventScreenCalendarViewCreateMeeting = "EventScreenCalendarViewCreateMeeting";
		public const string EventScreenCalendarViewUpdateMeeting = "EventScreenCalendarViewUpdateMeeting";
		public const string EventScreenCalendarViewRemoveMeeting = "EventScreenCalendarViewRemoveMeeting";
		public const string EventScreenCalendarViewRefreshMeetings = "EventScreenCalendarViewRefreshMeetings";
		
		public const string EventScreenCalendarViewOpened = "EventScreenCalendarViewOpened";
		
		public enum CalendarOption { NORMAL = 0, SELECT_GLOBAL_DATE, SELECT_MEETING_DATE }
		public enum ShowOptions { ALL = 0, CALENDAR, TASKS, PROGRESS }

		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private TextMeshProUGUI titleProjects;
		[SerializeField] private TextMeshProUGUI titleMembers;
		[SerializeField] private TextMeshProUGUI titleMeetings;
		[SerializeField] private TextMeshProUGUI titleSundayFirst;
		[SerializeField] private TextMeshProUGUI titleFeedback;

		[SerializeField] private Button closeButton;
		[SerializeField] private Button deleteButton;
		[SerializeField] private Button generateButton;
		[SerializeField] private CalendarSchedulerUI calendarScheduler;
		[SerializeField] private ScrollRect calendarContent;

		[SerializeField] private TMP_Dropdown DropDownProjects;
		[SerializeField] private TMP_Dropdown DropDownMembers;
		[SerializeField] private TMP_Dropdown DropDownOptions;

		[SerializeField] private IconColorView iconColorHuman;
		[SerializeField] private IconColorProjectView iconColorProject;

		[SerializeField] private Toggle toggleFormatCalendar;
		[SerializeField] private Toggle toggleIncludeSocial;

		private HumanView _human;
		private string _selectedHuman;
		private string _selectedMember;
		private List<ProjectInfoData> _projectsData;
		private int _idSelectedProject;

		private ShowOptions _showOptions = ShowOptions.ALL;
		private DateTime _currentDate;

		private CalendarOption _calendarOption = CalendarOption.NORMAL;
		private bool _isToggleSocial = false;
		private float _timeToRefresh = 0;
		private string _idMeeting = "";

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			_calendarOption = (CalendarOption)parameters[0];
			bool showCurrentProject = true;
			bool showProgressHuman = false;
			_idMeeting = "";
			if (_calendarOption == CalendarOption.NORMAL)
            {
				showCurrentProject = (bool)parameters[1];
				if (parameters.Length > 2)
				{
					showProgressHuman = (bool)parameters[2];
				}
			}
			else
            {
				_idMeeting = (string)parameters[1];
			}

			ApplicationController.Instance.IsSocialMeetingEnabled = false;

			titleProjects.text = LanguageController.Instance.GetText("text.projects");
            titleMembers.text = LanguageController.Instance.GetText("text.members");
            titleMeetings.text = LanguageController.Instance.GetText("text.meetings");
            titleSundayFirst.text = LanguageController.Instance.GetText("screen.calendar.is.sunday.first");

            closeButton.onClick.AddListener(OnCloseButton);
			deleteButton.onClick.AddListener(OnDeleteButton);
			generateButton.onClick.AddListener(OnGenerateButton);

			DateTime nextMonday = ApplicationController.Instance.GetNextMonday(WorkDayData.Instance.CurrentProject.GetCurrentTime().AddDays(-7));
			calendarScheduler.year = nextMonday.Year;
			calendarScheduler.month = nextMonday.Month;
			calendarScheduler.day = nextMonday.Day;
			calendarScheduler.Initialization(true);
			calendarScheduler.onDateTimeChanged.AddListener(OnCalendarChange);

			UIEventController.Instance.Event += OnUIEvent;
			SystemEventController.Instance.Event += OnSystemEvent;

			toggleFormatCalendar.isOn = WorkDayData.Instance.CurrentProject.IsSundayFirst;
			toggleFormatCalendar.onValueChanged.AddListener(OnFormatCalendarChanged);

			toggleIncludeSocial.isOn = false;
			toggleIncludeSocial.onValueChanged.AddListener(OnIncludeSocial);
			
			if (_calendarOption != CalendarOption.NORMAL)
            {
				titleProjects.gameObject.SetActive(false);
				titleMembers.gameObject.SetActive(false);
				titleMeetings.gameObject.SetActive(false);

				DropDownProjects.gameObject.SetActive(false);
				DropDownMembers.gameObject.SetActive(false);
				DropDownOptions.gameObject.SetActive(false);

				toggleIncludeSocial.gameObject.SetActive(false);

				SystemEventController.Instance.DelaySystemEvent(CalendarSchedulerUI.EventCalendarSchedulerDaysDaySelection, 0.3f, calendarScheduler, _idMeeting);
			}
			else
            {
				InitializeHumanSelected(true);

				ReloadOptionsFilter(showProgressHuman);
				if (showProgressHuman)
                {
					_showOptions = ShowOptions.PROGRESS;
				}

				// FILTER PROJECT
				DropDownProjects.ClearOptions();
				DropDownProjects.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("screen.calendar." + WorkDayData.RESERVED_NAME_ALL)));
				DropDownProjects.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("screen.calendar." + WorkDayData.RESERVED_NAME_SOCIAL)));
				DropDownProjects.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("screen.calendar." + WorkDayData.RESERVED_NAME_CASUAL)));
				DropDownProjects.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("screen.calendar." + WorkDayData.RESERVED_NAME_INTERRUPTIONS)));
				_projectsData = WorkDayData.Instance.CurrentProject.GetProjects();
				int indexProjectSelected = 0;
				for (int i = 0; i < _projectsData.Count; i++)
				{
					ProjectInfoData project = _projectsData[i];
					DropDownProjects.options.Add(new TMP_Dropdown.OptionData(project.Name));
					if (project.Id == WorkDayData.Instance.CurrentProject.ProjectInfoSelected)
					{
						_idSelectedProject = project.Id;
						indexProjectSelected = i + 4;
					}
				}				
				if (showProgressHuman)
				{
					_idSelectedProject = -1;
					DropDownProjects.value = 0;
				}
				else
                {
					if (showCurrentProject)
                    {
						DropDownProjects.value = indexProjectSelected;
					}
					else
                    {
						_idSelectedProject = -1;
						DropDownProjects.value = 0;
					}
				}
				DropDownProjects.onValueChanged.AddListener(OnSelectedProject);
				if (_idSelectedProject != -1)
                {
					ProjectInfoData project = WorkDayData.Instance.CurrentProject.GetProject(_idSelectedProject);
					_content.GetComponent<Image>().color = project.GetColor();
				}

				// FILTER MEMBERS
				DropDownMembers.ClearOptions();
				DropDownMembers.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("screen.calendar." + WorkDayData.RESERVED_NAME_ALL)));
				List<GroupInfoData> groups = WorkDayData.Instance.CurrentProject.GetGroups();
				foreach (GroupInfoData group in groups)
				{
					DropDownMembers.options.Add(new TMP_Dropdown.OptionData(group.Name));
				}
				List<WorldItemData> humans = ApplicationController.Instance.LevelView.GetHumans();
				foreach (WorldItemData human in humans)
				{
					DropDownMembers.options.Add(new TMP_Dropdown.OptionData(human.Name));
				}
				if (_selectedHuman != null)
				{
					List<TMP_Dropdown.OptionData> optionsLoaded = DropDownMembers.options;
					for (int k = 0; k < optionsLoaded.Count; k++)
					{
						if (optionsLoaded[k].text.Equals(_selectedHuman))
						{
							DropDownMembers.value = k;
							break;
						}
					}
				}
				else
				{
					DropDownMembers.value = 0;
				}
				DropDownMembers.onValueChanged.AddListener(OnSelectedHuman);

				UIEventController.Instance.DelayUIEvent(EventScreenCalendarViewDelayedLoadData, 0.3f, true);
			}

			SystemEventController.Instance.DispatchSystemEvent(PlayerView.EventPlayerAppEnableMovement, false);

			UpdateTime();

			titleFeedback.text = "";
			
			CameraXRController.Instance.GameCamera.farClipPlane = 1f;

			if (ApplicationController.Instance.IsPlayMode)
            {
				deleteButton.interactable = false;
				generateButton.interactable = false;
			}
			else
            {
				List<BoardData> boards = WorkDayData.Instance.CurrentProject.GetAllBoards();
				if (boards.Count > 0)
                {
					List<TaskItemData> tasks = WorkDayData.Instance.CurrentProject.GetAllTasks(boards[0]);
					if (tasks.Count > 0)
                    {
						generateButton.interactable = true;
					}
					else
                    {
						generateButton.interactable = false;
					}
                }
				else
                {
					generateButton.interactable = false;
				}
			}

            UIEventController.Instance.DispatchUIEvent(EventScreenCalendarViewOpened);
		}

		public override void Destroy()
		{
			base.Destroy();
			
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;

			calendarScheduler.onDateTimeChanged.RemoveListener(OnCalendarChange);

			_human = null;
			_selectedHuman = null;
			_projectsData = null;

			ApplicationController.Instance.IsCasualMeetingEnabled = false;
			ApplicationController.Instance.IsSocialMeetingEnabled = false;
			ApplicationController.Instance.IsInterruptionMeetingEnabled = false;

			SystemEventController.Instance.DispatchSystemEvent(PlayerView.EventPlayerAppEnableMovement, true);

			CameraXRController.Instance.GameCamera.farClipPlane = 1000;
		}

		private void OnFormatCalendarChanged(bool value)
		{
			WorkDayData.Instance.CurrentProject.IsSundayFirst = value;
			calendarScheduler.sundayIsFirst = value;

			calendarScheduler.onDateTimeChanged.RemoveAllListeners();
			calendarScheduler.ClearItems();
			calendarScheduler.Initialization(true);
			calendarScheduler.onDateTimeChanged.AddListener(OnCalendarChange);

			LoadData();

			SystemEventController.Instance.DispatchSystemEvent(CalendarSchedulerUI.EventCalendarSchedulerDaysResetDay);
			SystemEventController.Instance.DispatchSystemEvent(CalendarSchedulerUI.EventCalendarSchedulerDaysCurrentDay, _currentDate);
		}

		private void InitializeHumanSelected(bool force)
        {
			if (force)
            {
				if (ApplicationController.Instance.IsPlayMode)
				{
					if (ApplicationController.Instance.SelectedHuman != ApplicationController.Instance.HumanPlayer)
					{
						if (ApplicationController.Instance.HumanPlayer != null)
						{
							SystemEventController.Instance.DispatchSystemEvent(HumanView.EventHumanViewForceSelection, ApplicationController.Instance.HumanPlayer.NameHuman);
						}
					}
				}
			}
			if (ApplicationController.Instance.SelectedHuman != null)
			{
				_human = ApplicationController.Instance.SelectedHuman;
				_selectedHuman = _human.NameHuman;
			}
			else
			{
				_human = null;
				_selectedHuman = null;
			}
		}

		private void ReloadOptionsFilter(bool showProgress)
        {
			DropDownOptions.onValueChanged.RemoveAllListeners();
			DropDownOptions.ClearOptions();
			DropDownOptions.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("screen.calendar.option.show.all.meetings")));
			DropDownOptions.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("screen.calendar.option.show.calendar.meetings")));
			DropDownOptions.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("screen.calendar.option.show.tasks.meetings")));
			if (_human != null)
			{
				DropDownOptions.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("screen.calendar.option.show.progress")));
			}
			DropDownOptions.value = (showProgress?3:0);
			DropDownOptions.RefreshShownValue();
			DropDownOptions.onValueChanged.AddListener(OnOptionsChanged);
		}

		private void OnOptionsChanged(int value)
		{
			_showOptions = (ShowOptions)value;
			UIEventController.Instance.DelayUIEvent(EventScreenCalendarViewDelayedLoadData, 0.1f);
		}

		private void OnCloseButton()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnDeleteButton()
		{
			SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerDeleteAllMeetings);
		}

		private void OnGenerateButton()
		{
			AICommandsController.Instance.AddNewAICommand(new AICommandGenerateMeetings(), true, EventScreenCalendarViewGenerationCompleted);
		}

		private void OnIncludeSocial(bool value)
		{
			_isToggleSocial = value;
			LoadData();
		}


		private void OnCalendarChange(DateTime date)
		{
			
		}

		private void OnSelectedHuman(int value)
		{
			HumanView previousHuman = ApplicationController.Instance.SelectedHuman;
			_selectedHuman = DropDownMembers.options[value].text;
			_selectedMember = _selectedHuman;
			if (value == 0)
            {
				SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDCancelSelectionObject);
			}
			else
            {
				GroupInfoData groupSelected = WorkDayData.Instance.CurrentProject.GetGroupByName(_selectedHuman);
				if (groupSelected != null)
                {
					SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDCancelSelectionObject);
				}
				else
                {
					SystemEventController.Instance.DispatchSystemEvent(HumanView.EventHumanViewForceSelection, _selectedHuman);
				}
			}
			InitializeHumanSelected(false);
			if (((previousHuman != null) && (ApplicationController.Instance.SelectedHuman == null))
				|| ((previousHuman == null) && (ApplicationController.Instance.SelectedHuman != null)))
			{
				ReloadOptionsFilter(false);
				_showOptions = ShowOptions.ALL;
			}			
			LoadData();
		}

		private void OnSelectedProject(int value)
		{
			ApplicationController.Instance.IsSocialMeetingEnabled = false;
			ApplicationController.Instance.IsCasualMeetingEnabled = false;
			ApplicationController.Instance.IsInterruptionMeetingEnabled = false;
			if (value < 4)
            {
				_idSelectedProject = -1;
				_content.GetComponent<Image>().color = Color.white;
				iconColorProject.ApplyColor(Color.white);
				switch (value)
                {
					case 1:
						ApplicationController.Instance.IsSocialMeetingEnabled = true;
						break;

					case 2:
						ApplicationController.Instance.IsCasualMeetingEnabled = true;
						break;

					case 3:
						ApplicationController.Instance.IsInterruptionMeetingEnabled = true;
						break;
                }
			}
			else
            {
				ProjectInfoData currentProject = _projectsData[value - 4];
				_idSelectedProject = currentProject.Id;
				_content.GetComponent<Image>().color = currentProject.GetColor();
				SystemEventController.Instance.DispatchSystemEvent(ScreenProjectsView.EventScreenProjectsViewLoadProject, currentProject, false);
			}			
			LoadData();
		}

		private void AddEntryToCalendar(CalendarSchedulerData entry)
        {
			calendarScheduler.AddItem(entry);
		}

		private void LoadData()
        {
			calendarScheduler.ClearItems();

			if (ApplicationController.Instance.IsSocialMeetingEnabled 
				|| ApplicationController.Instance.IsCasualMeetingEnabled
				|| ApplicationController.Instance.IsInterruptionMeetingEnabled)
			{
				DropDownOptions.gameObject.SetActive(false);

				List<MeetingData> allMeetingsForHuman = WorkDayData.Instance.CurrentProject.GetMeetings();
				if (allMeetingsForHuman != null)
				{
					List<MeetingData> meetingsProject = new List<MeetingData>();
					GroupInfoData groupSelection = null;
					if (_selectedMember != null)
					{
						groupSelection = WorkDayData.Instance.CurrentProject.GetGroupByName(_selectedMember);
					}					
					for (int i = 0; i < allMeetingsForHuman.Count; i++)
					{
						MeetingData meeting = (MeetingData)allMeetingsForHuman[i];

						bool addSocialMeeting = false;
						bool isIndividual = false;
						if (meeting.ProjectId == -1)
						{
							bool addMeetingConfirmation = true;
							if (ApplicationController.Instance.IsCasualMeetingEnabled)
                            {
								if ((meeting.GetTotalMinutes() > 15) || !meeting.CanLeave)
								{
									addMeetingConfirmation = false;
								}
                            }
							if (ApplicationController.Instance.IsInterruptionMeetingEnabled)
							{
								if ((meeting.GetTotalMinutes() > 15) || meeting.CanLeave)
								{
									addMeetingConfirmation = false;
								}
							}
							if (ApplicationController.Instance.IsSocialMeetingEnabled)
							{
								if (meeting.GetTotalMinutes() <= 15)
								{
									addMeetingConfirmation = false;
								}
							}

							if (addMeetingConfirmation)
                            {
								if (groupSelection != null)
								{
									if (meeting.IsThereAnyMemberOfGroup(groupSelection))
									{
										meetingsProject.Add(meeting);
										addSocialMeeting = true;
										isIndividual = true;
									}
								}
								else
								{
									if (_selectedHuman == null)
									{
										meetingsProject.Add(meeting);
										addSocialMeeting = true;
									}
									else
									{
										if (_selectedHuman.Equals(LanguageController.Instance.GetText("screen.calendar." + WorkDayData.RESERVED_NAME_ALL)))
										{
											meetingsProject.Add(meeting);
											addSocialMeeting = true;
										}
										else
										if (meeting.IsMemberInMeeting(_selectedHuman))
										{
											meetingsProject.Add(meeting);
											addSocialMeeting = true;
											isIndividual = true;
										}
									}
								}
							}
						}
						if (addSocialMeeting)
						{
							CalendarSchedulerData renderMeeting = new CalendarSchedulerData(meeting.Name, Color.red, meeting.GetTimeStart().Year, meeting.GetTimeStart().Month, meeting.GetTimeStart().Day, false, meeting.GetTimeStart().Hour, meeting.GetTimeStart().Minute, true, meeting.GetUID(), "", 0, meeting.GetTotalMinutes(), meeting.ProjectId, isIndividual);
							AddEntryToCalendar(renderMeeting);
						}
					}
				}
				iconColorHuman.Refresh();

				if (ApplicationController.Instance.IsCasualMeetingEnabled)
                {
					SystemEventController.Instance.DispatchSystemEvent(CalendarSchedulerUI.EventCalendarSchedulerDaysEnableAddMeeting, false);
				}
				else
                {
					if (ApplicationController.Instance.IsSocialMeetingEnabled)
					{
						SystemEventController.Instance.DispatchSystemEvent(CalendarSchedulerUI.EventCalendarSchedulerDaysEnableAddMeeting, true);
					}
				}
			}
			else
			{
				DropDownOptions.gameObject.SetActive(true);

				if (_showOptions == ShowOptions.PROGRESS)
				{
					List<TimeWorkingDataDisplay> tasksProgress = ApplicationController.Instance.SelectedHuman.GetAllTimeWorkedLogs(_idSelectedProject);
					var currentTaskInProgress = ApplicationController.Instance.SelectedHuman.GetCurrentTaskProgress(_idSelectedProject);
					if (currentTaskInProgress != null)
					{
						int startingDay = currentTaskInProgress.StartTime.DayOfYear;
						int endingDay = WorkDayData.Instance.CurrentProject.GetCurrentTime().DayOfYear;
						if (startingDay < endingDay)
						{
							for (int k = startingDay; k < endingDay; k++)
							{
								if (k == startingDay)
								{
									DateTime endDay = new DateTime(currentTaskInProgress.StartTime.Year, currentTaskInProgress.StartTime.Month, currentTaskInProgress.StartTime.Day, WorkDayData.Instance.CurrentProject.EndingHour, 0, 0);
									tasksProgress.Add(new TimeWorkingDataDisplay(currentTaskInProgress.TaskUID, currentTaskInProgress.StartTime, endDay, ""));
								}
								else
								{
									DateTime anchorDay = new DateTime(currentTaskInProgress.StartTime.Year, currentTaskInProgress.StartTime.Month, currentTaskInProgress.StartTime.Day, WorkDayData.Instance.CurrentProject.StartingHour, 0, 0);
									anchorDay = anchorDay.Add(new TimeSpan(24 * (k - startingDay), 0, 0));
									if (!WorkDayData.Instance.CurrentProject.IsFreeDay(anchorDay))
                                    {
										DateTime endDay = new DateTime(anchorDay.Year, anchorDay.Month, anchorDay.Day, WorkDayData.Instance.CurrentProject.EndingHour, 0, 0);
										tasksProgress.Add(new TimeWorkingDataDisplay(currentTaskInProgress.TaskUID, anchorDay, endDay, ""));
									}
								}
							}
							DateTime todayDay = WorkDayData.Instance.CurrentProject.GetCurrentTime();
							DateTime todayStart = new DateTime(todayDay.Year, todayDay.Month, todayDay.Day, WorkDayData.Instance.CurrentProject.StartingHour, 0, 0);
							currentTaskInProgress.StartTime = todayStart;
						}
						tasksProgress.Add(currentTaskInProgress);
					}
					List<TimeWorkingDataDisplay> sortedList = tasksProgress.OrderBy(task => task.StartTime).ToList();
					for (int i = 0; i < sortedList.Count; i++)
					{
						TimeWorkingDataDisplay taskProgress = sortedList[i];
						var (taskItem, boarName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(taskProgress.TaskUID);
						ProjectInfoData project = WorkDayData.Instance.CurrentProject.GetProjectByTaskItemUID(taskProgress.TaskUID);
						int projectId = -1;
						if (project != null)
						{
							projectId = project.Id;
						}
						if (taskItem != null)
                        {
							CalendarSchedulerData renderTask = new CalendarSchedulerData(taskItem.Name, Color.red, taskProgress.StartTime.Year, taskProgress.StartTime.Month, taskProgress.StartTime.Day, false, taskProgress.StartTime.Hour, taskProgress.StartTime.Minute, false, taskProgress.TaskUID.ToString(), taskProgress.Data, (int)taskProgress.GetTotalMinutes(), (int)taskProgress.GetTotalMinutes(), projectId, false);
							AddEntryToCalendar(renderTask);
						}
					}
				}
				else
				{
					bool isIndividual = false;
					List<MeetingData> allMeetingsForHuman = WorkDayData.Instance.CurrentProject.GetMeetings();

					if (allMeetingsForHuman != null)
					{
						List<MeetingData> meetingsProject = new List<MeetingData>();
						GroupInfoData groupSelection = null;
						if (_selectedMember != null)
						{
							groupSelection = WorkDayData.Instance.CurrentProject.GetGroupByName(_selectedMember);
						}
						for (int i = 0; i < allMeetingsForHuman.Count; i++)
						{
							MeetingData meeting = (MeetingData)allMeetingsForHuman[i];

							if ((meeting.ProjectId == _idSelectedProject) || (_idSelectedProject == -1))
							{
								bool includeFMeeting = true;
								if (((meeting.ProjectId == -1) && (_idSelectedProject == -1)))
								{
									// DON'T SHOW ANYTHING
									if (!_isToggleSocial)
                                    {
										includeFMeeting = false;
									}
									else
                                    {
										if (!meeting.CanLeave)
                                        {
											includeFMeeting = false;
										}
									}
								}
								if (includeFMeeting)
								{ 
									if (groupSelection != null)
									{
										if (meeting.IsThereAnyMemberOfGroup(groupSelection))
										{
											meetingsProject.Add(meeting);
											isIndividual = true;
										}
									}
									else
									{
										if (_selectedHuman == null)
										{
											meetingsProject.Add(meeting);
										}
										else
										{
											if (_selectedHuman.Equals(LanguageController.Instance.GetText("screen.calendar." + WorkDayData.RESERVED_NAME_ALL)))
											{
												meetingsProject.Add(meeting);
											}
											else
											if (meeting.IsMemberInMeeting(_selectedHuman))
											{
												meetingsProject.Add(meeting);
												isIndividual = true;
											}
										}
									}
								}
							}
						}

						for (int i = 0; i < meetingsProject.Count; i++)
						{
							MeetingData meeting = (MeetingData)meetingsProject[i];
							bool addToCalendar = true;
							switch (_showOptions)
							{
								case ShowOptions.ALL:
									break;

								case ShowOptions.CALENDAR:
									addToCalendar = (meeting.TaskId == -1);
									break;

								case ShowOptions.TASKS:
									addToCalendar = (meeting.TaskId != -1);
									break;

								case ShowOptions.PROGRESS:
									addToCalendar = false;
									break;
							}
							if (addToCalendar)
							{
								CalendarSchedulerData renderMeeting = new CalendarSchedulerData(meeting.Name, Color.red, meeting.GetTimeStart().Year, meeting.GetTimeStart().Month, meeting.GetTimeStart().Day, false, meeting.GetTimeStart().Hour, meeting.GetTimeStart().Minute, true, meeting.GetUID(), "", 0, meeting.GetTotalMinutes(), meeting.ProjectId, isIndividual);
								AddEntryToCalendar(renderMeeting);
							}
						}
					}
				}
				iconColorHuman.Refresh();
				iconColorProject.Refresh();

				switch (_showOptions)
				{
					case ShowOptions.ALL:
						SystemEventController.Instance.DispatchSystemEvent(CalendarSchedulerUI.EventCalendarSchedulerDaysEnableAddMeeting, true);
						break;

					case ShowOptions.CALENDAR:
						SystemEventController.Instance.DispatchSystemEvent(CalendarSchedulerUI.EventCalendarSchedulerDaysEnableAddMeeting, true);
						break;

					case ShowOptions.TASKS:
						SystemEventController.Instance.DispatchSystemEvent(CalendarSchedulerUI.EventCalendarSchedulerDaysEnableAddMeeting, false);
						break;

					case ShowOptions.PROGRESS:
						SystemEventController.Instance.DispatchSystemEvent(CalendarSchedulerUI.EventCalendarSchedulerDaysEnableAddMeeting, false);
						break;
				}
			}
		}
		
		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ScreenCalendarView.EventScreenCalendarViewSetNewDate))
            {
				if (_calendarOption != CalendarOption.NORMAL)
                {
					OnCloseButton();
				}
            }
			if (nameEvent.Equals(CalendarSchedulerItem.EventCalendarSchedulerItemEnterButton))
            {
				bool isMeeting = (bool)parameters[0];
				if (!isMeeting)
                {
					int taskUID = int.Parse((string)parameters[1]);
					var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(taskUID);
					if (taskItemData == null)
					{
						titleFeedback.text = "";
					}
					else
					{ 
						taskItemData.GetMembers();
						string totalHumans = "";
						List<string> membersOverTask = taskItemData.GetMembers();
						foreach (string member in membersOverTask)
						{
							GroupInfoData groupHumans = WorkDayData.Instance.CurrentProject.GetGroupByName(member);
							if (groupHumans != null)
							{
								if (totalHumans.Length > 0) totalHumans += ", ";
								totalHumans += member + "(" + groupHumans.GetMembers().Count + ")";
							}
							else
							{
								if (totalHumans.Length > 0) totalHumans += ", ";
								totalHumans += member;
							}
						}
						float totalHoursDone = WorkDayData.Instance.CurrentProject.GetTotalLoggedTimeForTask(-1, taskUID);
						titleFeedback.text = taskItemData.Name + " "+LanguageController.Instance.GetText("text.estimated.time") + + taskItemData.EstimatedTime + "h /" + LanguageController.Instance.GetText("text.done.task") + Utilities.CeilDecimal(totalHoursDone,1) + "h // " + totalHumans;
					}
				}
				else
                {
					MeetingData meetingOver = WorkDayData.Instance.CurrentProject.GetMeetingByUID((string)parameters[1]);
					if (meetingOver == null)
					{
						titleFeedback.text = "";
					}
					else
					{
						string totalHumans = "";
						List<string> membersOverMeeting = meetingOver.GetMembers();
						foreach (string member in membersOverMeeting)
						{
							GroupInfoData groupHumans = WorkDayData.Instance.CurrentProject.GetGroupByName(member);
							if (groupHumans != null)
							{
								if (totalHumans.Length > 0) totalHumans += ", ";
								totalHumans += member + "(" + groupHumans.GetMembers().Count + ")";
							}
							else
							{
								if (totalHumans.Length > 0) totalHumans += ", ";
								totalHumans += member;
							}
						}
						if (meetingOver.TaskId != -1)
                        {
							var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(meetingOver.TaskId);
							if (taskItemData != null)
                            {
								titleFeedback.text = meetingOver.Name + " " + meetingOver.GetTimeStart().ToShortTimeString() + " - " + meetingOver.GetTimeEnd().ToShortTimeString() + " // "+LanguageController.Instance.GetText("text.task") +": " + taskItemData.Name + " // " + totalHumans;
							}								
							else
                            {
								titleFeedback.text = meetingOver.Name + " " + meetingOver.GetTimeStart().ToShortTimeString() + " - " + meetingOver.GetTimeEnd().ToShortTimeString() + " // " + totalHumans;
							}
						}
						else
                        {
							titleFeedback.text = meetingOver.Name + " " + meetingOver.GetTimeStart().ToShortTimeString() + " - " + meetingOver.GetTimeEnd().ToShortTimeString() + " // " + totalHumans;
						}						
					}
				}
			}
			if (nameEvent.Equals(EventScreenCalendarViewDelayedLoadData))
            {
				LoadData();
				_currentDate = WorkDayData.Instance.CurrentProject.GetCurrentTime();
				if (parameters.Length > 0)
                {
					int weekNumber = WorkDayData.Instance.GetWeekOfMonth(_currentDate) - 1;
					if (weekNumber > 1)
                    {
						calendarContent.verticalNormalizedPosition = 1 - (weekNumber * 0.2f);
					}
					else
                    {
						calendarContent.verticalNormalizedPosition = 1 - (weekNumber * 0.215f);
					}
				}
				SystemEventController.Instance.DispatchSystemEvent(CalendarSchedulerUI.EventCalendarSchedulerDaysResetDay);
				SystemEventController.Instance.DispatchSystemEvent(CalendarSchedulerUI.EventCalendarSchedulerDaysCurrentDay, _currentDate);
			}
			if (nameEvent.Equals(EventScreenCalendarViewMonthChanged))
            {				
				titleFeedback.text = "";
				_currentDate = WorkDayData.Instance.CurrentProject.GetCurrentTime();
				if (_calendarOption != CalendarOption.NORMAL)
				{
					titleProjects.gameObject.SetActive(false);
					titleMembers.gameObject.SetActive(false);
					titleMeetings.gameObject.SetActive(false);

					DropDownProjects.gameObject.SetActive(false);
					DropDownMembers.gameObject.SetActive(false);
					DropDownOptions.gameObject.SetActive(false);

					SystemEventController.Instance.DelaySystemEvent(CalendarSchedulerUI.EventCalendarSchedulerDaysDaySelection, 0.3f, calendarScheduler, _idMeeting);
				}
				else
				{
                    LoadData();
                }
                calendarContent.verticalNormalizedPosition = 1;
                SystemEventController.Instance.DispatchSystemEvent(CalendarSchedulerUI.EventCalendarSchedulerDaysResetDay);
                SystemEventController.Instance.DispatchSystemEvent(CalendarSchedulerUI.EventCalendarSchedulerDaysCurrentDay, _currentDate);
            }
            if (nameEvent.Equals(EventScreenCalendarViewCreateMeeting))
            {
				LoadData();
			}
			if (nameEvent.Equals(EventScreenCalendarViewUpdateMeeting))
            {
				LoadData();
			}
			if (nameEvent.Equals(EventScreenCalendarViewRemoveMeeting))
            {
				calendarScheduler.RemoveItem((string)parameters[0]);
			}
			if (nameEvent.Equals(EventScreenCalendarViewRefreshMeetings))
            {				
				LoadData();
            }
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(MeetingController.EventMeetingControllerMeetingStopped))
			{
				LoadData();
			}
			if (nameEvent.Equals(TimeHUD.EventTimeHUDUpdateCurrentTime))
			{
				_currentDate = WorkDayData.Instance.CurrentProject.GetCurrentTime();
				SystemEventController.Instance.DispatchSystemEvent(CalendarSchedulerUI.EventCalendarSchedulerDaysResetDay);
				SystemEventController.Instance.DispatchSystemEvent(CalendarSchedulerUI.EventCalendarSchedulerDaysCurrentDay, _currentDate);
			}
			if (nameEvent.Equals(TasksController.EventTasksControllerResponseTask))
			{
				LoadData();
			}
			if (nameEvent.Equals(ClockController.EventClockControllerChangedDay))
            {
				_currentDate = WorkDayData.Instance.CurrentProject.GetCurrentTime();
				SystemEventController.Instance.DispatchSystemEvent(CalendarSchedulerUI.EventCalendarSchedulerDaysResetDay);
				SystemEventController.Instance.DispatchSystemEvent(CalendarSchedulerUI.EventCalendarSchedulerDaysCurrentDay, _currentDate);
			}
		}

		private void UpdateTime()
        {
			DateTime currTime = WorkDayData.Instance.CurrentProject.GetCurrentTime();
			titleScreen.text = currTime.ToShortDateString() + " " + currTime.ToShortTimeString();
		}

		void Update()
        {
			_timeToRefresh += Time.deltaTime;
			if (_timeToRefresh > 1)
            {
				_timeToRefresh = 0;
				UpdateTime();
			}			
		}
	}
}