using InGameCodeEditor;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.ScreenCalendarView;

namespace yourvrexperience.WorkDay
{
	public class ScreenMeetingView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenMeetingView";

		public const string EventScreenMeetingViewConfirmation = "EventScreenMeetingViewConfirmation";
		public const string EventScreenMeetingViewReloadData = "EventScreenMeetingViewReloadData";
		public const string EventScreenMeetingViewDisableBecauseRunningMeeting = "EventScreenMeetingViewDisableBecauseRunningMeeting";
		public const string EventScreenMeetingViewForceDataButton = "EventScreenMeetingViewForceDataButton";

		public const string SubEventScreenMeetingViewData = "SubEventScreenMeetingViewData";
		public const string SubEventScreenMeetingViewEditLongDescription = "SubEventScreenMeetingViewEditLongDescription";
		public const string SubEventScreenMeetingViewEditLongSummary = "SubEventScreenMeetingViewEditLongSummary";

		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private TextMeshProUGUI titleDate;
		[SerializeField] private TextMeshProUGUI titleProject;
		[SerializeField] private TextMeshProUGUI titleName;
		[SerializeField] private TextMeshProUGUI titleDescription;
		[SerializeField] private TextMeshProUGUI titleData;
		[SerializeField] private TextMeshProUGUI titleStartTime;
		[SerializeField] private TextMeshProUGUI titleEndTime;
		[SerializeField] private TextMeshProUGUI titleCanStop;
		[SerializeField] private TextMeshProUGUI titleFindRoom;
		[SerializeField] private TextMeshProUGUI titleSummary;

		[SerializeField] private TextMeshProUGUI nameProject;
		[SerializeField] private TextMeshProUGUI valueDate;
		[SerializeField] private TextMeshProUGUI titleConversationLog;
		[SerializeField] private TextMeshProUGUI titleTask;
		
		[SerializeField] private CustomInput inputName;
		[SerializeField] private CustomInput inputDescription;
		[SerializeField] private Button buttonData;
		[SerializeField] private Button buttonConversationLog;
		[SerializeField] private Button buttonTask;
		[SerializeField] private Button buttonEditorLong;
		[SerializeField] private Button buttonSummary;
		[SerializeField] private TimePicker pickTimeStart;
		[SerializeField] private TimePicker pickTimeEnd;

		[SerializeField] private Button buttonOk;
		[SerializeField] private Button buttonCancel;
		[SerializeField] private Button buttonClose;
		[SerializeField] private Button buttonSetNewDate;

		[SerializeField] private SlotManagerView SlotManagerMembers;
		[SerializeField] private GameObject PrefabMember;

		[SerializeField] private TMP_Dropdown DropDownMembers;
		[SerializeField] private Button buttonAddMember;

		[SerializeField] private IconColorView iconColorSelection;

		[SerializeField] private Toggle toggleCanStop;
		[SerializeField] private Toggle toggleFindRoom;

		private MeetingData _meeting;

		private DateTime _dayMeeting;
		private List<string> _members;
		private string _memberToAdd;
		private int _taskId;
		private List<DocumentData> _data = null;
        private Color _projectColor = Color.white;

        public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			bool enableTaskView = true;

			titleSummary.gameObject.SetActive(false);
			buttonSummary.gameObject.SetActive(false);
			buttonSummary.onClick.AddListener(OnEditSummary);

			_taskId = (int)parameters[0];
			if (parameters[1] is DateTime)
            {
				_dayMeeting = (DateTime)parameters[1];
				_meeting = null;
				_members = new List<string>();
				_data = new List<DocumentData>();
				valueDate.text = _dayMeeting.ToShortDateString();
				toggleCanStop.isOn = true;
				toggleFindRoom.isOn = true;
			}
			else
            {
				if (parameters[1] is MeetingData)
                {
					_meeting = (MeetingData)parameters[1];
					enableTaskView = !(parameters.Length > 2);
					_dayMeeting = _meeting.GetTimeStart();
					_data = new List<DocumentData>();
					int projectId = -1;
					if (_taskId != -1)
					{
						var (task, board) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_taskId);
						if (task != null)
						{
							_data = task.GetData();
							BoardData boardData = WorkDayData.Instance.CurrentProject.GetBoardFor(board);
							projectId = boardData.ProjectId;
						}
					}
					else
					{
						_data = _meeting.GetData();
						projectId = _meeting.ProjectId;
					}
					if (projectId != -1)
                    {
                        _projectColor = WorkDayData.Instance.CurrentProject.GetProject(projectId).GetColor();
						_content.GetComponent<Image>().color = _projectColor;
                    }
					inputName.text = _meeting.Name;
					inputDescription.text = _meeting.Description;
					DateTime dateStartMeeting = _meeting.GetTimeStart();
					DateTime dateEndMeeting = _meeting.GetTimeEnd();
					_members = _meeting.GetMembers();
					valueDate.text = _meeting.GetTimeStart().ToShortDateString();
					toggleCanStop.isOn = _meeting.CanClose;
					toggleFindRoom.isOn = _meeting.FindRoom;

					pickTimeStart.ChangeCurrentSelectedHour(dateStartMeeting.Hour);
					pickTimeStart.ChangeCurrentSelectedMinute(dateStartMeeting.Minute);
					pickTimeStart.IsAM = (dateStartMeeting.Hour <= 12);
					pickTimeStart.UpdateTimeDisplayed();
					pickTimeStart.SelectHour(48 - ((pickTimeStart.CurrentSelectedHour % 12) * 30));
					pickTimeStart.SelectMinute(48 -(((float)pickTimeStart.CurrentSelectedMinute / 5) * 30f));

					pickTimeEnd.ChangeCurrentSelectedHour(dateEndMeeting.Hour);
					pickTimeEnd.ChangeCurrentSelectedMinute(dateEndMeeting.Minute);
					pickTimeEnd.IsAM = (dateEndMeeting.Hour <= 12);
					pickTimeEnd.UpdateTimeDisplayed();
					pickTimeEnd.SelectHour(48 - ((pickTimeEnd.CurrentSelectedHour % 12) * 30));
					pickTimeEnd.SelectMinute(48 - (((float)pickTimeEnd.CurrentSelectedMinute / 5) * 30f));

					if ((_meeting.Summary != null) && (_meeting.Summary.Length > 2))
                    {
						titleSummary.gameObject.SetActive(true);
						buttonSummary.gameObject.SetActive(true);
					}
				}
			}
			ProjectInfoData project;
			if (_meeting != null)
            {
				project = WorkDayData.Instance.CurrentProject.GetProject(_meeting.ProjectId);
			}
			else
            {
				if (ApplicationController.Instance.IsSocialMeetingEnabled)
                {
					project = null;
				}
				else
                {
					project = WorkDayData.Instance.CurrentProject.GetProject(WorkDayData.Instance.CurrentProject.ProjectInfoSelected);
				}				
			}			
			if (project != null)
            {
				nameProject.text = project.Name;
			}		
			else
            {
				nameProject.text = LanguageController.Instance.GetText("text.social.meeting");
            }


			titleScreen.text = LanguageController.Instance.GetText("screen.meeting.title");
            titleDate.text = LanguageController.Instance.GetText("text.date");
            titleProject.text = LanguageController.Instance.GetText("text.project");
            titleName.text = LanguageController.Instance.GetText("text.name");
            titleDescription.text = LanguageController.Instance.GetText("text.description");
            titleData.text = LanguageController.Instance.GetText("text.data");
            titleStartTime.text = LanguageController.Instance.GetText("text.start.time");
            titleEndTime.text = LanguageController.Instance.GetText("text.end.time");
            titleCanStop.text = LanguageController.Instance.GetText("text.can.stop");
            titleFindRoom.text = LanguageController.Instance.GetText("text.find.room");
            titleSummary.text = LanguageController.Instance.GetText("text.summary");
            titleConversationLog.text = LanguageController.Instance.GetText("text.conversation");
            titleTask.text = LanguageController.Instance.GetText("text.task");

            List<WorldItemData> humans = ApplicationController.Instance.LevelView.GetHumans();
			if ((humans == null) || (humans.Count == 0))
            {
				DropDownMembers.gameObject.SetActive(false);
				buttonAddMember.gameObject.SetActive(false);
				SlotManagerMembers.gameObject.SetActive(false);
			}
			else
            {
				DropDownMembers.ClearOptions();
				DropDownMembers.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("text.NONE")));
				List<GroupInfoData> groups = WorkDayData.Instance.CurrentProject.GetGroups();
				foreach (GroupInfoData group in groups)
				{
					DropDownMembers.options.Add(new TMP_Dropdown.OptionData(group.Name));
				}
				foreach (WorldItemData human in humans)
                {
					DropDownMembers.options.Add(new TMP_Dropdown.OptionData(human.Name));
				}
				DropDownMembers.value = 0;
				DropDownMembers.onValueChanged.AddListener(OnSelectedHuman);
				iconColorSelection.Refresh();

				buttonAddMember.onClick.AddListener(OnAddNewMember);
				buttonAddMember.interactable = false;
				LoadMembers();
			}

			buttonClose.onClick.AddListener(OnCloseButton);

			buttonData.onClick.AddListener(OnDataButton);
			buttonConversationLog.onClick.AddListener(OnStartMeeting);
			buttonTask.onClick.AddListener(OnTaskButton);
			buttonEditorLong.onClick.AddListener(OnEditorLongButton);

			buttonOk.onClick.AddListener(OnConfirmButton);
			buttonCancel.onClick.AddListener(OnCloseButton);
			buttonSetNewDate.onClick.AddListener(OnSetNewDateButton);
			buttonSetNewDate.interactable = false;

			buttonOk.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.confirm");
			buttonCancel.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.cancel");

            if (_taskId == -1)
            {
				titleTask.gameObject.SetActive(false);
				buttonTask.gameObject.SetActive(false);
			}
			else
            {
				if (!enableTaskView)
				{
					titleTask.gameObject.SetActive(false);
					buttonTask.gameObject.SetActive(false);
				}
				else
                {
					var (taskData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_taskId);
					if (taskData == null)
                    {
						titleTask.gameObject.SetActive(false);
						buttonTask.gameObject.SetActive(false);
					}
				}
			}

			if (_meeting == null)
            {
				buttonConversationLog.interactable = false;
				buttonData.interactable = false;
			}
			else
            {
				buttonSetNewDate.interactable = true;
				if (ApplicationController.Instance.SelectedHuman == null)
				{
					if (_meeting.Completed)
					{
						DisableMeetingCompleted();
					}
					else
                    {
						if (_meeting.InProgress)
						{
							if (_meeting.HasPlayer(false))
                            {
								titleConversationLog.text = LanguageController.Instance.GetText("text.join.ongoing.meeting");
							}
							else
                            {
								titleConversationLog.text = LanguageController.Instance.GetText("text.join.as.observer");
							}							
							buttonConversationLog.interactable = true;
							buttonSetNewDate.interactable = false;
						}
						else
                        {
							titleConversationLog.gameObject.SetActive(false);
							buttonConversationLog.gameObject.SetActive(false);
						}
					}					
				}
				else
                {
					if (_meeting.Completed)
					{
						DisableMeetingCompleted();
					}
					else
					{
						if (_meeting.InProgress)
                        {
							titleConversationLog.text = LanguageController.Instance.GetText("text.join.ongoing.meeting");
                            buttonConversationLog.interactable = true;
							buttonSetNewDate.interactable = false;
						}
					}
				}
			}

			if (_meeting != null)
            {
				if (_meeting.IsSocialMeeting())
				{
					if (_meeting.IsInterruptionMeeting())
                    {
						titleData.gameObject.SetActive(false);
						buttonData.gameObject.SetActive(false);
						titleTask.gameObject.SetActive(false);
						buttonTask.gameObject.SetActive(false);
					}
					else
                    {
						titleTask.gameObject.SetActive(false);
						buttonTask.gameObject.SetActive(false);
					}
				}
			}

			UIEventController.Instance.Event += OnUIEvent;
			SystemEventController.Instance.Event += OnSystemEvent;

			if (ApplicationController.Instance.IsPlayMode)
			{
				if ((_meeting != null) && (!_meeting.IsUserCreated))
				{
					inputName.interactable = false;
					inputDescription.interactable = false;

					pickTimeStart.BtnChangeHour.interactable = false;
					pickTimeEnd.BtnChangeHour.interactable = false;

					buttonSetNewDate.interactable = false;

					DropDownMembers.interactable = false;
					buttonAddMember.interactable = false;

					toggleCanStop.interactable = false;
					toggleFindRoom.interactable = false;

					buttonOk.interactable = false;

					UIEventController.Instance.DispatchUIEvent(ItemMemberView.EventItemMemberViewDisableDelete);
				}
			}
		}

		public override void Destroy()
		{
			base.Destroy();

			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;

			_meeting = null;
		}

		private void OnCloseButton()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnEditSummary()
		{
			string title = LanguageController.Instance.GetText("text.summary");
			ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLongInput, null, title, "", SubEventScreenMeetingViewEditLongSummary);
			UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, _meeting.Summary);
			GameObject.FindAnyObjectByType<CodeEditor>().Refresh();
		}

		private void DisableMeetingCompleted()
        {
			titleConversationLog.text = LanguageController.Instance.GetText("text.check.meeting.log");
			buttonConversationLog.interactable = true;
			buttonSetNewDate.interactable = false;
			inputName.interactable = false;
			pickTimeStart.enabled = false;
			pickTimeEnd.enabled = false;
			DropDownMembers.interactable = false;
			buttonAddMember.interactable = false;
			SlotManagerMembers.ApplyGenericAction(false);
			pickTimeStart.BtnChangeHour.interactable = false;
			pickTimeEnd.BtnChangeHour.interactable = false;
			toggleCanStop.interactable = false;
			toggleFindRoom.interactable = false;
			buttonEditorLong.interactable = false;
			inputDescription.interactable = false;
		}

		private void OnCanCloseMeeting(bool value)
		{

		}

		private void OnSetNewDateButton()
		{
			GameObject calendarGO = ScreenController.Instance.CreateScreen(ScreenCalendarView.ScreenName, false, false, CalendarOption.SELECT_MEETING_DATE, _meeting.GetUID());
			UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, calendarGO, _canvas.sortingOrder + 1);
		}

		private void OnEditorLongButton()
		{
			string title = LanguageController.Instance.GetText("text.description");
			GameObject screenGO = ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLongInput, null, title, "", SubEventScreenMeetingViewEditLongDescription);
			UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, inputDescription.text);
			GameObject.FindAnyObjectByType<CodeEditor>().Refresh();
            UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetColor, screenGO, _projectColor);
        }

		private void OnAddNewMember()
        {
			if (!_members.Contains(_memberToAdd))
            {
				_members.Insert(0, _memberToAdd);
				buttonAddMember.interactable = false;
				LoadMembers();
			}
		}

        private void OnSelectedHuman(int value)
        {
            if (value == 0)
            {
				buttonAddMember.interactable = false;
			}
			else
            {
				string nameSelected = DropDownMembers.options[value].text;
				if (!_members.Contains(nameSelected))
                {
					_memberToAdd = DropDownMembers.options[value].text;
					buttonAddMember.interactable = true;

					iconColorSelection.Refresh();
				}
				else
                {
					buttonAddMember.interactable = false;
				}
			}
        }

		private void LoadMembers()
		{
			SlotManagerMembers.ClearCurrentGameObject(true);
			List<ItemMultiObjectEntry> itemsUserStories = new List<ItemMultiObjectEntry>();
			for (int i = 0; i < _members.Count; i++)
			{
				itemsUserStories.Add(new ItemMultiObjectEntry(this.gameObject, i, _members[i]));
			}
			SlotManagerMembers.Initialize(itemsUserStories.Count, itemsUserStories, PrefabMember);
		}

		private void OnDataButton()
		{
			string meetingDocumentsTitleName = "";
			if (WorkDayData.Instance.CurrentProject.ProjectInfoSelected != -1)
			{
				ProjectInfoData projectInfo = WorkDayData.Instance.CurrentProject.GetProject(WorkDayData.Instance.CurrentProject.ProjectInfoSelected);
				if (projectInfo != null)
				{
					meetingDocumentsTitleName = LanguageController.Instance.GetText("text.meeting.documents.for") + _meeting.Name + " (" + projectInfo.Name + ")";
				}
			}
			ScreenInformationView.CreateScreenInformation(ScreenDocumentsDataView.ScreenName, null, meetingDocumentsTitleName, "", SubEventScreenMeetingViewData);
			UIEventController.Instance.DispatchUIEvent(ScreenDocumentsDataView.EventScreenDocumentsDataViewInitialization, _meeting.ProjectId, false, _data, WorkDayData.Instance.CurrentProject.GetDocuments());
			CodeEditor codeEditor = GameObject.FindAnyObjectByType<CodeEditor>();
			if (codeEditor != null)
            {
				codeEditor.Refresh(true);
			}			
		}

		private bool RefreshDataMeeting()
        {
			string nameMeeting = inputName.text;
			string descriptionMeeting = inputDescription.text;

			int hourStart = pickTimeStart.getSelectedHour();
			int minuteStart = pickTimeStart.getSelectedMinute();
			int hourEnd = pickTimeEnd.getSelectedHour();
			int minuteEnd = pickTimeEnd.getSelectedMinute();

			DateTime dateTimeStart = new DateTime(_dayMeeting.Year, _dayMeeting.Month, _dayMeeting.Day, hourStart, minuteStart, 0);
			DateTime dateTimeEnd = new DateTime(_dayMeeting.Year, _dayMeeting.Month, _dayMeeting.Day, hourEnd, minuteEnd, 0);

			string[] members = _members.ToArray();
			int projectID = -1;
			if (!ApplicationController.Instance.IsSocialMeetingEnabled)
            {
				projectID = WorkDayData.Instance.CurrentProject.ProjectInfoSelected;
			}
			if (hourStart < WorkDayData.Instance.CurrentProject.StartingHour)
            {
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.meeting.you.cannot.set.meeting.before.journey.starts"));
				return false;
            }
			if ((hourStart > WorkDayData.Instance.CurrentProject.EndingHour)) // WE ALLOW MEETING THAT CAN FINISH AFTER THE ENDING HOUR
			{
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.meeting.you.cannot.set.meeting.after.journey.ends"));
				return false;
			}
			if ((hourStart > hourEnd) || ((hourStart == hourEnd) && (minuteStart > minuteEnd)))
			{
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.meeting.you.cannot.set.meeting.in.negative.hours"));
				return false;
			}
			if ((hourStart == hourEnd) && ((minuteEnd - minuteStart) < 10))
			{
				bool displayForbiddenMessage = true;
				if (_meeting != null)
                {
					if (_meeting.IsSocialMeeting())
                    {
						displayForbiddenMessage = false;
					}
                }
				if (displayForbiddenMessage)
                {
					ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.meeting.you.cannot.set.meeting.more.than.10.minutes.at.least"));
					return false;
				}
			}
			if (_members.Count == 0)
            {
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.meeting.you.cannot.set.meeting.without.members"));
				return false;
			}
			else
            {
				int countTotalMembers = 0;
				foreach(string memberInList in _members)
                {
					GroupInfoData groupMeeting = WorkDayData.Instance.CurrentProject.GetGroupByName(memberInList);
					if (groupMeeting != null)
                    {
						List<string> membersInTheGroup = groupMeeting.GetMembers();
						countTotalMembers += membersInTheGroup.Count;
					}
					else
                    {
						countTotalMembers++;
					}
				}
				if (countTotalMembers <= 1)
                {
					ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.meeting.you.cannot.set.meeting.without.members"));
					return false;
				}
			}
			if ((nameMeeting.Length < 1) || (descriptionMeeting.Length < 5))
            {
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.meeting.you.cannot.set.meeting.without.description"));
				return false;
			}
			SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerAddMeeting, _meeting, _taskId, nameMeeting, descriptionMeeting, _data, dateTimeStart, dateTimeEnd, projectID, members, toggleCanStop.isOn, true, toggleFindRoom.isOn, true, false);
			return true;
		}

		private void OnConfirmButton()
		{
			if (RefreshDataMeeting())
            {
				UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);

				UIEventController.Instance.DispatchUIEvent(EventScreenMeetingViewConfirmation);
			}
		}

		private void OnTaskButton()
		{
			// OPEN TASK VIEW
			var (taskData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_taskId);

			ScreenController.Instance.CreateScreen(ScreenTaskView.ScreenName, false, false, taskData, boardName, true);
			UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, ScreenTaskView.ScreenName, _canvas.sortingOrder + 1);
		}

		private void OnStartMeeting()
		{
            if (!ApplicationController.Instance.TimeHUD.IsPlayingTime)
            {
                SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, true);
            }
            // OPEN CONVERSATION VIEW				
            RefreshDataMeeting();
			UIEventController.Instance.DispatchUIEvent(MeetingController.EventMeetingControllerUIRequestToStartMeeting, _meeting, _taskId, _canvas.sortingOrder + 1);
		}

		private void DebugData(List<DocumentData> docsReceived)
        {
			List<DocumentData> docs = null;
			if (docsReceived != null)
            {
				docs = docsReceived;
            }
			else
            {
				if (_taskId != -1)
				{
					var (task, board) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_taskId);
					if (task != null)
					{
						docs = task.GetData();
					}
				}
				else
				{
					if (_meeting != null)
                    {
						docs = _meeting.GetData();
					}					
				}
			}

			if (docs == null)
			{
				Debug.LogError("LIST OF DOCUMENTS FOR MEETING is null");
			}
			else
			{
				Debug.LogError("LIST OF DOCUMENTS FOR MEETING[" + docs.Count + "]");
				for (int i = 0; i < docs.Count; i++)
				{
					Debug.LogError("DOCUMENT[" + i + "]=" + docs[i].Name);
				}
			}
		}
		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(MeetingController.EventMeetingControllerUpdatedMeeting))
			{
				MeetingData meeting = (MeetingData)parameters[0];
				if (_meeting == meeting)
				{
					_dayMeeting = _meeting.GetTimeStart();
					valueDate.text = _meeting.GetTimeStart().ToShortDateString();
				}
			}
			if (nameEvent.Equals(MeetingController.EventMeetingControllerStopMeeting))
			{
				MeetingData meeting = (MeetingData)parameters[0];
				if (_meeting == meeting)
				{
					titleConversationLog.text = LanguageController.Instance.GetText("text.check.meeting.log");
                    buttonConversationLog.interactable = true;
				}
			}
			if (nameEvent.Equals(MeetingController.EventMeetingControllerMeetingsRefreshData))
            {
				if (_meeting == (MeetingData)parameters[0])
                {
					_members = _meeting.GetMembers();
					LoadMembers();
				}
			}
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventScreenMeetingViewForceDataButton))
            {
				OnDataButton();
            }
			if (nameEvent.Equals(EventScreenMeetingViewDisableBecauseRunningMeeting))
            {
				if (this.gameObject == (GameObject)parameters[0])
                {
					inputName.interactable = false;
					buttonData.interactable = true;

					buttonConversationLog.interactable = false;
					pickTimeStart.enabled = false;
					pickTimeEnd.enabled = false;

					buttonOk.interactable = false;
					buttonCancel.interactable = true;
					buttonClose.interactable = true;

					DropDownMembers.interactable = false;
					buttonAddMember.interactable = false;
					buttonSetNewDate.interactable = false;

					SlotManagerMembers.ApplyGenericAction(false);

					pickTimeStart.BtnChangeHour.interactable = false;
					pickTimeEnd.BtnChangeHour.interactable = false;

					toggleCanStop.interactable = false;
					toggleFindRoom.interactable = false;
				}
			}
			if (nameEvent.Equals(EventScreenMeetingViewReloadData))
            {
				if (_taskId != -1)
				{
					var (task, board) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_taskId);
					if (task != null)
					{
						_data = task.GetData();
					}
				}
				else
				{
					_data = _meeting.GetData();
				}
			}
			if (nameEvent.Equals(TimePicker.EventTimePickerOpened))
			{
				if (pickTimeStart == (TimePicker)parameters[0])
                {
					pickTimeEnd.gameObject.SetActive(false);
				}
				else
                {
					pickTimeStart.gameObject.SetActive(false);					
				}
			}
			if (nameEvent.Equals(TimePicker.EventTimePickerClosed))
            {
				pickTimeStart.gameObject.SetActive(true);
				pickTimeEnd.gameObject.SetActive(true);
			}
			if (nameEvent.Equals(SubEventScreenMeetingViewData))
			{
				if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
				{
					_data = (List<DocumentData>)parameters[2];
					if (_meeting.TaskId == -1)
                    {
						SystemEventController.Instance.DispatchSystemEvent(DocumentController.EventDocumentControllerUpdateMeetingDocs, _meeting, _data.ToArray());
					}
					else
                    {
						var (task, board) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_taskId);
						if (task != null)
                        {
							SystemEventController.Instance.DispatchSystemEvent(DocumentController.EventDocumentControllerUpdateTaskDocs, task, _data.ToArray());
						}						
					}					
				}
			}
			if (nameEvent.Equals(SubEventScreenMeetingViewEditLongDescription))
			{
				if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
				{
					if (!ApplicationController.Instance.IsPlayMode)
                    {
						inputDescription.text = (string)parameters[2];
						_meeting.Description = inputDescription.text;
					}
					else
                    {
						if (_meeting.IsUserCreated)
                        {
							inputDescription.text = (string)parameters[2];
							_meeting.Description = inputDescription.text;
						}
					}
				}
			}
			if (nameEvent.Equals(SubEventScreenMeetingViewEditLongSummary))
            {
				if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
				{
					_meeting.Summary = (string)parameters[2];
				}
			}
			if (nameEvent.Equals(ItemMemberView.EventItemMemberViewDelete))
            {
				if (_members.Remove((string)parameters[2]))
                {
					LoadMembers();
				}
			}
		}
	}
}