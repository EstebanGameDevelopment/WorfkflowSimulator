using InGameCodeEditor;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.TaskItemData;

namespace yourvrexperience.WorkDay
{
	public class ScreenTaskView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenTaskView";

		public const string EventScreenTaskViewReloadData = "EventScreenTaskViewReloadData";
		public const string EventScreenTaskViewForceShowDocsTODO = "EventScreenTaskViewForceShowDocsTODO";
		public const string EventScreenTaskViewForceShowData = "EventScreenTaskViewForceShowData";

		public const string SubEventScreenTaskViewData = "SubEventScreenTaskViewData";
		public const string SubEventScreenTaskViewInputName = "SubEventScreenTaskViewInputName";
		public const string SubEventScreenTaskViewInputLogWork = "SubEventScreenTaskViewInputLogWork";
		public const string SubEventScreenTaskViewInputEditorDescription = "SubEventScreenTaskViewInputEditorDescription";

		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private TextMeshProUGUI titleName;
		[SerializeField] private TextMeshProUGUI titleDescription;
		[SerializeField] private TextMeshProUGUI titleData;
		[SerializeField] private TextMeshProUGUI titleDataTODO;
		[SerializeField] private TextMeshProUGUI titleEstimation;
		[SerializeField] private TextMeshProUGUI titleState;
		[SerializeField] private TextMeshProUGUI titleActivelyWorking;
		[SerializeField] private TextMeshProUGUI titleTotalHoursDone;
		[SerializeField] private TextMeshProUGUI titleProject;
		[SerializeField] private TextMeshProUGUI titleMeetings;
		[SerializeField] private TextMeshProUGUI titleDependency;

		[SerializeField] private TextMeshProUGUI nameProject;
		[SerializeField] private CustomInput inputName;
		[SerializeField] private CustomInput inputDescription;
		[SerializeField] private Button buttonData;
		[SerializeField] private Button buttonDataTODO;
		[SerializeField] private Button buttonAddMeeting;
		[SerializeField] private CustomInput inputEstimation;
		[SerializeField] private TextMeshProUGUI valueTotalHoursDone;

		[SerializeField] private Button buttonLogWork;
		[SerializeField] private Button buttonCheckLogs;
		[SerializeField] private Button buttonEditorLong;

		[SerializeField] private Button buttonOk;
		[SerializeField] private Button buttonCancel;
		[SerializeField] private Button buttonClose;

		[SerializeField] private TMP_Dropdown DropDownDependency;

		[SerializeField] private SlotManagerView SlotManagerMembers;
		[SerializeField] private GameObject PrefabMember;

		[SerializeField] private SlotManagerView SlotManagerMeetings;
		[SerializeField] private GameObject PrefabMeeting;

		[SerializeField] private TMP_Dropdown DropDownMembers;
		[SerializeField] private Button buttonAddMember;

		[SerializeField] private TMP_Dropdown DropDownState;

		[SerializeField] private IconColorView iconColorSelection;
		
		[SerializeField] private Image iconWorking;

		private TaskItemData _task;
		private string _boardName;
		private TaskStates _state;
		private TaskStates _newStateTask;
		private List<DocumentData> _data;
		private List<string> _members;
		private string _memberToAdd;
		private bool _showMeetings;
		private bool _taskInProgress;		
		private List<string> _humansWorkingInTask;
		private List<TaskItemData> _tasksDependencies;
		private int _finalLinkedTask = -1;
		private Color _projectColor = Color.white;

        private bool TaskInProgress
        {
			get { return _taskInProgress; }
			set
            {
				_taskInProgress = value;
				if (_taskInProgress)
                {
					iconWorking.color = Color.red;
				}
				else
                {
					iconWorking.color = Color.white;
				}
            }
        }

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			buttonDataTODO.onClick.AddListener(OnTODODataDocuments);

			_showMeetings = true;
			TaskStates initialSelectedState = TaskStates.TODO;
			if (parameters[0] is TaskStates)
            {
				_state = (TaskStates)parameters[0];
				_boardName = (string)parameters[1];
				_data = new List<DocumentData>();
				_task = null;
				_members = new List<string>();
				_newStateTask = _state;
				initialSelectedState = _state;
			}
			else
            {
				if (parameters[0] is TaskItemData)
                {
					_task = (TaskItemData)parameters[0];
					initialSelectedState = (TaskStates)_task.State;
					_boardName = (string)parameters[1];
					if (parameters.Length > 2)
                    {
						_showMeetings = false;
					}
					
					inputName.text = _task.Name;
					inputDescription.text = _task.Description;
					_data = _task.GetData();
					if (_data == null)
                    {
						_data = new List<DocumentData>();
					}
					_members = _task.GetMembers();
					_newStateTask = (TaskStates)_task.State;
					
					inputEstimation.text = _task.EstimatedTime.ToString();
					float totalHoursDone = WorkDayData.Instance.CurrentProject.GetTotalLoggedTimeForTask(-1, _task.UID);
					valueTotalHoursDone.text = Utilities.CeilDecimal(totalHoursDone, 1) + "h";
				}
			}
			BoardData currentBoardOrigin = WorkDayData.Instance.CurrentProject.GetBoardFor(_boardName);
			ProjectInfoData project = WorkDayData.Instance.CurrentProject.GetProject(currentBoardOrigin.ProjectId);
			nameProject.text = project.Name;
			_projectColor = project.GetColor();
			_content.GetComponent<Image>().color = _projectColor;

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

			LoadMeetings();

			DropDownState.ClearOptions();
			DropDownState.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("task.state." + TaskStates.TODO.ToString().ToLower())));
			DropDownState.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("task.state." + TaskStates.DOING.ToString().ToLower())));
            DropDownState.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("task.state." + TaskStates.DONE.ToString().ToLower())));
            DropDownState.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("task.state." + TaskStates.VERIFIED.ToString().ToLower())));
            DropDownState.value = ((_task == null)?(int)initialSelectedState:_task.State);
			DropDownState.onValueChanged.AddListener(OnChangeStateTask);

			DropDownDependency.ClearOptions();
			DropDownDependency.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("screen.calendar." + WorkDayData.RESERVED_NAME_NONE)));
			BoardData boardTask = WorkDayData.Instance.CurrentProject.GetBoardFor(_boardName);
			_finalLinkedTask = -1;			
			if (_task != null)
            {
				_tasksDependencies = WorkDayData.Instance.CurrentProject.GetAllTasks(boardTask, _task.UID);
				_finalLinkedTask = _task.Linked;
			}
			else
            {
				_tasksDependencies = WorkDayData.Instance.CurrentProject.GetAllTasks(boardTask);
			}
			int defaultDependency = 0;
			for (int i = 0; i < _tasksDependencies.Count; i++ )
            {
				if (_tasksDependencies[i].UID == _finalLinkedTask)
                {
					defaultDependency = i + 1;
				}
				DropDownDependency.options.Add(new TMP_Dropdown.OptionData(_tasksDependencies[i].Name));
			}
			DropDownDependency.value = defaultDependency;
			DropDownDependency.onValueChanged.AddListener(OnLinkedTaskChanged);

			buttonClose.onClick.AddListener(OnCloseButton);
			buttonData.onClick.AddListener(OnDataButton);
			buttonEditorLong.onClick.AddListener(OnEditorDescriptionButton);

			buttonOk.onClick.AddListener(OnConfirmButton);
			buttonCancel.onClick.AddListener(OnCloseButton);
			
			UIEventController.Instance.Event += OnUIEvent;
			SystemEventController.Instance.Event += OnSystemEvent;

			buttonLogWork.onClick.AddListener(OnButtonStartWorkingInTask);
			buttonCheckLogs.onClick.AddListener(OnButtonChekLogs);

			titleScreen.text = LanguageController.Instance.GetText("screen.task.title");
            titleName.text = LanguageController.Instance.GetText("text.name");
            titleDescription.text = LanguageController.Instance.GetText("text.description");
            titleData.text = LanguageController.Instance.GetText("text.data");
            titleDataTODO.text = LanguageController.Instance.GetText("text.TODO");
            titleState.text = LanguageController.Instance.GetText("text.state");
            titleProject.text = LanguageController.Instance.GetText("text.project");
            titleMeetings.text = LanguageController.Instance.GetText("text.meetings");
            titleDependency.text = LanguageController.Instance.GetText("text.dependencies");
            titleEstimation.text = LanguageController.Instance.GetText("text.estimation");
            titleTotalHoursDone.text = LanguageController.Instance.GetText("text.total.hours.done");

            buttonOk.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.confirm");
            buttonCancel.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.cancel");

            titleActivelyWorking.text = "";
			if (_task == null)
			{
				buttonAddMeeting.interactable = false;
				buttonData.interactable = false;
				buttonLogWork.interactable = false;
				buttonCheckLogs.interactable = false;
				buttonLogWork.interactable = false;
			}
			else
            {
				if (ApplicationController.Instance.SelectedHuman != null)
				{
					buttonLogWork.interactable = true;
					SystemEventController.Instance.DispatchSystemEvent(TasksController.EventTasksControllerRequestTask, _task, ApplicationController.Instance.SelectedHuman.NameHuman);
				}
				else
				{
					buttonLogWork.interactable = false;
					SystemEventController.Instance.DispatchSystemEvent(TasksController.EventTasksControllerRequestTask, _task);
				}
			}

			if (!_showMeetings)
			{
				buttonAddMeeting.interactable = false;
				inputName.interactable = false;
				buttonOk.interactable = false;
				inputEstimation.interactable = false;
				buttonData.interactable = false;
				DropDownState.interactable = false;
				DropDownMembers.interactable = false;
				DropDownDependency.interactable = false;
				buttonLogWork.interactable = false;
			}
			else
			{
				buttonAddMeeting.onClick.AddListener(OnAddMeeting);
			}

			if (ApplicationController.Instance.IsPlayMode)
			{
				if ((_task != null) && (!_task.IsUserCreated))
				{
					inputName.interactable = false;
					inputDescription.interactable = false;
					inputEstimation.interactable = false;
					DropDownDependency.interactable = false;
					DropDownState.interactable = false;
					buttonOk.interactable = false;

					UIEventController.Instance.DispatchUIEvent(ItemMemberView.EventItemMemberViewDisableDelete);
				}
				if (_task != null)
                {
					buttonLogWork.interactable = _task.HasHumanPlayer();
				}
			}

			if (!ApplicationController.Instance.TimeHUD.IsPlayingTime)
			{
				buttonLogWork.interactable = false;
				buttonDataTODO.interactable = false;
			}
		}

		public override void Destroy()
		{
			base.Destroy();

			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;

			_task = null;
			_tasksDependencies = null;
			_data = null;
		}

		private void OnTODODataDocuments()
		{
			List<CurrentDocumentInProgress> docsToDo = AICommandsController.Instance.ExistsDocumentsToDoForTask(_task.UID);
			if ((docsToDo != null) && (docsToDo.Count > 0))
			{
				GameObject screenGO = ScreenController.Instance.CreateScreen(ScreenDocsTODOView.ScreenName, false, false, _task);
				UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, screenGO, _canvas.sortingOrder + 1);
			}
			else
            {
				bool createCurrentDocs = true;
				var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_task.Linked);
				if (taskItemData != null)
				{
					if (!taskItemData.IsTaskCompleted())
					{
						createCurrentDocs = false;
						string description = LanguageController.Instance.GetText("text.cannot.start.before.finishing.task") + " " + taskItemData.Name;
						ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), description);
					}
				}
				if (createCurrentDocs)
                {
					GameObject screenGO = ScreenController.Instance.CreateScreen(ScreenDocsTODOView.ScreenName, false, false, _task);
					UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, screenGO, _canvas.sortingOrder + 1);
				}
			}
		}

		private void OnEditorDescriptionButton()
		{
			string title = LanguageController.Instance.GetText("text.description");
			GameObject screenGO = ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLongInput, null, title, "", SubEventScreenTaskViewInputEditorDescription);
			UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, inputDescription.text);
			GameObject.FindAnyObjectByType<CodeEditor>().Refresh();
            UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetColor, screenGO, _projectColor);
        }

		private void OnLinkedTaskChanged(int value)
		{
			if (value == 0)
            {
				_finalLinkedTask = -1;
            }
			else
            {
				_finalLinkedTask = _tasksDependencies[value - 1].UID;
			}
		}

		private void OnButtonStartWorkingInTask()
		{
			SystemEventController.Instance.DispatchSystemEvent(TasksController.EventTasksControllerStartTask, _task);
		}

		private void OnButtonChekLogs()
		{
			if (_task != null)
            {
				ScreenController.Instance.CreateScreen(ScreenWorkLogsView.ScreenName, false, false, _task.UID);
				UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, ScreenWorkLogsView.ScreenName, _canvas.sortingOrder + 1);
			}			
		}

		private void OnChangeStateTask(int value)
		{
			_newStateTask = (TaskStates)value;
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
			List<ItemMultiObjectEntry> itemsMembers = new List<ItemMultiObjectEntry>();
			for (int i = 0; i < _members.Count; i++)
			{
				itemsMembers.Add(new ItemMultiObjectEntry(this.gameObject, i, _members[i], _showMeetings));
			}
			SlotManagerMembers.Initialize(itemsMembers.Count, itemsMembers, PrefabMember);
			UIEventController.Instance.DispatchUIEvent(ItemTaskView.EventItemTaskViewRefresh, _task);
		}

		private void OnAddMeeting()
		{
			ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInput, null, LanguageController.Instance.GetText("screen.task.name.new.task"), "", SubEventScreenTaskViewInputName);
		}

		private void LoadMeetings()
		{
			SlotManagerMeetings.ClearCurrentGameObject(true);
			if (_task != null)
			{
				List<MeetingData> meetings = WorkDayData.Instance.CurrentProject.GetMeetingsByTaskUID(_task.UID);
				if (meetings.Count > 0)
				{
					List<ItemMultiObjectEntry> itemsMeetings = new List<ItemMultiObjectEntry>();
					for (int i = 0; i < meetings.Count; i++)
					{
						itemsMeetings.Add(new ItemMultiObjectEntry(this.gameObject, i, meetings[i], _showMeetings));
					}
					SlotManagerMeetings.Initialize(itemsMeetings.Count, itemsMeetings, PrefabMeeting);
				}
			}
		}

		private void OnDataButton()
		{
			string taskDocumentsTitleName = "";
			if (WorkDayData.Instance.CurrentProject.ProjectInfoSelected != -1)
			{
				ProjectInfoData projectInfo = WorkDayData.Instance.CurrentProject.GetProject(WorkDayData.Instance.CurrentProject.ProjectInfoSelected);
				if (projectInfo != null)
				{
					taskDocumentsTitleName = LanguageController.Instance.GetText("text.task.documents.for") + _task.Name + " (" + projectInfo.Name + ")";
				}
			}
			ScreenInformationView.CreateScreenInformation(ScreenDocumentsDataView.ScreenName, null, taskDocumentsTitleName, "", SubEventScreenTaskViewData);
			UIEventController.Instance.DispatchUIEvent(ScreenDocumentsDataView.EventScreenDocumentsDataViewInitialization, WorkDayData.Instance.CurrentProject.ProjectInfoSelected, false, _data, WorkDayData.Instance.CurrentProject.GetDocuments());
			CodeEditor codeEditor = GameObject.FindAnyObjectByType<CodeEditor>();
			if (codeEditor != null)
			{
				codeEditor.Refresh(true);
			}
		}

		private void OnCloseButton()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnConfirmButton()
		{
			string nameTask = inputName.text;
			string descriptionTask = inputDescription.text;
			if (inputEstimation.text.Length == 0)
			{
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.task.view.no.time.estimation"));
				return;
			}
			if (_members.Count == 0)
            {
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.task.view.task.has.to.have.one.member"));
				return;
			}

			int estimatedTime = int.Parse(inputEstimation.text);

			string[] members = _members.ToArray();
			SystemEventController.Instance.DispatchSystemEvent(TasksController.EventTasksControllerAddTask, _task, _boardName, nameTask, descriptionTask, _data.ToArray(), estimatedTime, (int)_newStateTask, _finalLinkedTask, -1, members);

			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ScreenDocsTODOView.EventScreenDocsTODOViewClosed))
            {
				inputEstimation.text = _task.EstimatedTime.ToString();
			}
			if (nameEvent.Equals(EventScreenTaskViewForceShowDocsTODO))
            {				
				if (_task != null)
				{
					OnTODODataDocuments();
				}
			}
			if (nameEvent.Equals(EventScreenTaskViewForceShowData))
            {
				if (_task != null)
				{
					OnDataButton();
					string nameDocToSelect = (string)parameters[0];
					UIEventController.Instance.DispatchUIEvent(ItemDataView.EventItemDataViewForceNameSelection, nameDocToSelect);
				}
			}
			if (nameEvent.Equals(EventScreenTaskViewReloadData))
            {				
				if (_task != null)
                {
					_data = _task.GetData();
				}
            }
			if (nameEvent.Equals(SubEventScreenTaskViewData))
			{
				if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
				{
					_data = (List<DocumentData>)parameters[2];
					SystemEventController.Instance.DispatchSystemEvent(DocumentController.EventDocumentControllerUpdateTaskDocs, _task, _data.ToArray());
				}
			}
			if (nameEvent.Equals(SubEventScreenTaskViewInputEditorDescription))
			{
				if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
				{
					if (!ApplicationController.Instance.IsPlayMode)
                    {
						inputDescription.text = (string)parameters[2];
						_task.Description = inputDescription.text;
					}
					else
                    {
						if (_task.IsUserCreated)
                        {
							inputDescription.text = (string)parameters[2];
							_task.Description = inputDescription.text;
						}
					}
				}
			}
			if (nameEvent.Equals(SubEventScreenTaskViewInputName))
            {
				if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
				{
					string nameMeeting = (string)parameters[2];
					string descriptionMeeting = "";

					DateTime dateMeeting = WorkDayData.Instance.CurrentProject.GetCurrentTime() + new TimeSpan(0, 15, 0);

					DateTime dateTimeStart = new DateTime(dateMeeting.Year, dateMeeting.Month, dateMeeting.Day, dateMeeting.Hour, dateMeeting.Minute, 0);
					DateTime dateTimeEnd = new DateTime(dateMeeting.Year, dateMeeting.Month, dateMeeting.Day, dateMeeting.Hour + 1, dateMeeting.Minute, 0);

					string[] members = new string[0];
					if (_members != null)
                    {
						members = _members.ToArray();
					}

					SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerAddMeeting, null, _task.UID, nameMeeting, descriptionMeeting, _data, dateTimeStart, dateTimeEnd, WorkDayData.Instance.CurrentProject.ProjectInfoSelected, members, true, true, true, true, false);

					LoadMeetings();
				}
			}
			if (nameEvent.Equals(ItemMeetingView.EventItemMeetingViewDelete))
			{
				SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerDeleteMeeting, ((MeetingData)parameters[2]).GetUID().ToString());
			}
			if (nameEvent.Equals(ScreenCalendarView.EventScreenCalendarViewRemoveMeeting))
            {
				LoadMeetings();
			}
			if (nameEvent.Equals(ItemMeetingView.EventItemMeetingViewEdit))
			{
				string meetingUID = ((MeetingData)parameters[2]).GetUID();
				MeetingData meeting = WorkDayData.Instance.CurrentProject.GetMeetingByUID(meetingUID);
				if (meeting != null)
                {
					ScreenController.Instance.CreateScreen(ScreenMeetingView.ScreenName, false, false, meeting.TaskId, meeting, true);
					UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, ScreenMeetingView.ScreenName, _canvas.sortingOrder + 1);
				}
			}
			if (nameEvent.Equals(ItemMemberView.EventItemMemberViewDelete))
            {
				if (_members.Remove((string)parameters[2]))
                {
					LoadMembers();
				}
			}
			if (nameEvent.Equals(ScreenMeetingView.EventScreenMeetingViewConfirmation))
            {
				LoadMeetings();
			}
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(AICommandGenerateDoc.EventAICommandGenerateDocRefreshState))
            {
				_data = _task.GetData();
			}
			if (nameEvent.Equals(TasksController.EventTasksControllerResponseTask))
			{
				int taskUID = (int)parameters[0];
				bool isInProgress = (bool)parameters[1];
				if (_task != null)
				{
					if (_task.UID == taskUID)
					{
						TaskInProgress = isInProgress;
						if (!isInProgress)
                        {
							if (ApplicationController.Instance.SelectedHuman != null)
                            {
								titleActivelyWorking.text = LanguageController.Instance.GetText("text.start.working");
							}
							else
                            {
								titleActivelyWorking.text = LanguageController.Instance.GetText("text.nobody.working");
							}								
						}
						else
                        {
							if ((isInProgress) && (parameters.Length > 2))
							{
								_humansWorkingInTask = (List<string>)parameters[2];
								string totalHumans = "";
								foreach (string nameHuman in _humansWorkingInTask)
								{
									if (totalHumans.Length > 0) totalHumans += ", ";
									totalHumans += nameHuman;
								}
								titleActivelyWorking.text = totalHumans;
								var worldItemData = WorkDayData.Instance.CurrentProject.GetItemByName(totalHumans);
								if (worldItemData != null)
                                {
									float totalHoursDone = worldItemData.GetTotalDecimalHoursProgressForTask(-1, _task.UID);
									titleActivelyWorking.text += "("+ Utilities.CeilDecimal(totalHoursDone, 1) + ")";
								}
								if (parameters.Length > 3)
                                {
									List<string> fullTotalHumans = (List<string>)parameters[3];
									string totalFullHumans = "";
									foreach (string nameFHuman in fullTotalHumans)
									{
										if (totalFullHumans.Length > 0) totalFullHumans += ", ";
										var worldFItemData = WorkDayData.Instance.CurrentProject.GetItemByName(nameFHuman);
										string fName = nameFHuman;
										if (worldFItemData != null)
										{
											float totalFHoursDone = worldItemData.GetTotalDecimalHoursProgressForTask(-1, _task.UID);
											fName += "(" + Utilities.CeilDecimal(totalFHoursDone, 1) + ")";
										}
										totalFullHumans += fName;
									}
									titleTotalHoursDone.text = totalFullHumans;
								}								
							}                            
                        }
					}
				}
			}
		}

		private void Update()
        {
			if (TaskInProgress)
			{
				iconWorking.transform.localEulerAngles += new Vector3(0, 0, 90 * Time.deltaTime);
			}
		}
	}
}