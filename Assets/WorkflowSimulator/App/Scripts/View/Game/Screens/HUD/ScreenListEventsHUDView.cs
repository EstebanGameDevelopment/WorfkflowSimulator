using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class ScreenListEventsHUDView : BaseScreenView, IScreenView
	{
		public const string EventScreenListEventsHUDViewDestroy = "EventScreenListEventsHUDViewDestroy";
		public const string EventScreenListEventsHUDViewSelectedEmployee = "EventScreenListEventsHUDViewSelectedEmployee";

		public const string ScreenName = "ScreenListEventsHUDView";
		public const string ScreenPeopleName = "ScreenListEventsPeopleView";

		public enum TypeLateralInfo { MEETINGS = 0, TASKS, PERSONS}

		[SerializeField] private TextMeshProUGUI titleType;		
		[SerializeField] private TextMeshProUGUI titleSelection;		
		[SerializeField] private TextMeshProUGUI feedbackInformation;		
		[SerializeField] private Button buttonCancel;

		[SerializeField] private GameObject MeetingViewPrefab;
		[SerializeField] private GameObject TaskViewPrefab;
		[SerializeField] private GameObject EmployeeViewPrefab;
		[SerializeField] private SlotManagerView SlotManagerElements;

		private TypeLateralInfo _typeInfo;
		private bool _employeeSelection = false;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			_typeInfo = (TypeLateralInfo)parameters[0];
			if (parameters.Length > 1)
            {
				_employeeSelection = (bool)parameters[1];
			}

			buttonCancel.onClick.AddListener(OnCancel);
			
			LoadData();

			SystemEventController.Instance.Event += OnSystemEvent;
			UIEventController.Instance.Event += OnUIEvent;
		}

        public override void Destroy()
		{
			base.Destroy();

			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

			SystemEventController.Instance.DispatchSystemEvent(LevelView.EventLevelViewLinesRequestDestroy);
		}

		private void OnCancel()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void LoadData()
        {
			if (feedbackInformation != null)
            {
				feedbackInformation.text = "";
			}

			switch (_typeInfo)
            {
				case TypeLateralInfo.MEETINGS:
					titleType.text = LanguageController.Instance.GetText("text.meetings").ToUpper();
					LoadMeetings();
					break;

				case TypeLateralInfo.TASKS:
                    titleType.text = LanguageController.Instance.GetText("text.tasks").ToUpper();
                    LoadTasks();
					break;

				case TypeLateralInfo.PERSONS:
                    titleType.text = LanguageController.Instance.GetText("text.employees").ToUpper();
					if (titleSelection != null) titleSelection.text = "";
					LoadEmployees();
					break;
            }
		}

		private void LoadTasks()
        {
			if (ApplicationController.Instance.SelectedHuman != null)
            {
				titleSelection.text = ApplicationController.Instance.SelectedHuman.NameHuman;
			}
			else
            {
				titleSelection.text = LanguageController.Instance.GetText("text.everyone");
			}

			SlotManagerElements.ClearCurrentGameObject(true);
			SlotManagerElements.Initialize(0, new List<ItemMultiObjectEntry>(), TaskViewPrefab);

			List<TaskItemData> tasksProject = WorkDayData.Instance.CurrentProject.GetAllTasks(null);
			foreach (TaskItemData task in tasksProject)
			{
				List<string> humansWorking = WorkDayData.Instance.CurrentProject.GetHumansWorkingInTask(task.UID);
				if (humansWorking.Count > 0)
                {
					if (ApplicationController.Instance.SelectedHuman != null)
					{
						if (humansWorking.Contains(ApplicationController.Instance.SelectedHuman.NameHuman))
						{
							SlotManagerElements.AddItem(new ItemMultiObjectEntry(this.gameObject, SlotManagerElements.Data.Count, task));
						}
					}
					else
					{
						SlotManagerElements.AddItem(new ItemMultiObjectEntry(this.gameObject, SlotManagerElements.Data.Count, task));
					}
				}
			}

			foreach (TaskItemData task in tasksProject)
			{
				List<string> humansWorking = WorkDayData.Instance.CurrentProject.GetHumansWorkingInTask(task.UID);
				if (humansWorking.Count == 0)
                {
					List<string> humansAssigned = WorkDayData.Instance.CurrentProject.GetHumansAssignedToTask(task.UID);
					if ((humansAssigned != null) && (humansAssigned.Count > 0))
					{
						if (ApplicationController.Instance.SelectedHuman != null)
						{
							if (humansAssigned.Contains(ApplicationController.Instance.SelectedHuman.NameHuman))
							{
								SlotManagerElements.AddItem(new ItemMultiObjectEntry(this.gameObject, SlotManagerElements.Data.Count, task));
							}
						}
						else
						{
							SlotManagerElements.AddItem(new ItemMultiObjectEntry(this.gameObject, SlotManagerElements.Data.Count, task));
						}
					}
				}
			}

			SlotManagerElements.SetVerticalScroll(1);
		}

		private void LoadMeetings()
        {
			SlotManagerElements.ClearCurrentGameObject(true);
			SlotManagerElements.Initialize(0, new List<ItemMultiObjectEntry>(), MeetingViewPrefab);

			string selectedMember = null;
			if (ApplicationController.Instance.SelectedHuman != null)
            {
				selectedMember = ApplicationController.Instance.SelectedHuman.NameHuman;
				titleSelection.text = selectedMember;
			}
			else
            {
				titleSelection.text = LanguageController.Instance.GetText("text.everyone");
			}

			List<MeetingData> allMeetingsForHuman = WorkDayData.Instance.CurrentProject.GetMeetings();
			List<MeetingData> meetingsProject = new List<MeetingData>();

			if (allMeetingsForHuman != null)
			{
				for (int i = 0; i < allMeetingsForHuman.Count; i++)
				{
					MeetingData meeting = (MeetingData)allMeetingsForHuman[i];
					if (!meeting.Completed)
                    {
						if (meeting.ProjectId != -1)
						{
							if (selectedMember == null)
							{
								meetingsProject.Add(meeting);
							}
							else
							{
								if (meeting.IsMemberInMeeting(selectedMember))
								{
									if (!meeting.IsSocialMeeting())
									{
										meetingsProject.Add(meeting);
									}
								}
							}
						}
					}
				}
			}

			List<MeetingData> sortedMeetings = meetingsProject.OrderBy(m => m.GetTimeStart()).ToList();
			foreach (MeetingData meeting in sortedMeetings)
			{
				if (meeting.GetTimeEnd() >= WorkDayData.Instance.CurrentProject.GetCurrentTime())
                {
					SlotManagerElements.AddItem(new ItemMultiObjectEntry(this.gameObject, SlotManagerElements.Data.Count, meeting));
				}				
			}

			SlotManagerElements.SetVerticalScroll(1);
		}

		private void LoadEmployees()
        {
			List<WorldItemData> humans = ApplicationController.Instance.LevelView.GetHumans();
			SlotManagerElements.ClearCurrentGameObject(true);
			SlotManagerElements.Initialize(0, new List<ItemMultiObjectEntry>(), EmployeeViewPrefab);

			foreach (WorldItemData human in humans)
			{
				SlotManagerElements.AddItem(new ItemMultiObjectEntry(this.gameObject, SlotManagerElements.Data.Count, human));
			}

			SlotManagerElements.SetVerticalScroll(1);
		}

		public override void ActivateContent(bool value)
		{
			base.ActivateContent(value);

			if (_typeInfo != TypeLateralInfo.PERSONS)
			{
				LoadData();
			}
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ScreenTaskManagerView.EventScreenTaskManagerViewRefreshMembersWorking))
			{
				int taskUID = (int)parameters[0];
				if (feedbackInformation != null)
                {
					if (taskUID == -1)
					{
						feedbackInformation.text = "";
					}
					else
					{
						feedbackInformation.text = TasksController.Instance.GetInformationMembersWorking(taskUID);
					}
				}
			}
			if (nameEvent.Equals(ItemEmployeeHUDView.EventItemEmployeeHUDViewSelected))
			{
				if (this.gameObject == (GameObject)parameters[0])
				{
					if (!_employeeSelection)
					{
						SystemEventController.Instance.DispatchSystemEvent(LevelView.EventLevelViewLinesRequestDestroy);
						if ((int)parameters[2] == -1)
						{
							SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDCancelSelectionObject);
						}
						else
						{
							WorldItemData humanData = (WorldItemData)parameters[3];
							SystemEventController.Instance.DispatchSystemEvent(HumanView.EventHumanViewForceSelection, humanData.Name, true);
						}
					}
					else
                    {
						if ((int)parameters[2] != -1)
						{
							WorldItemData humanData = (WorldItemData)parameters[3];							
							UIEventController.Instance.DispatchUIEvent(EventScreenListEventsHUDViewSelectedEmployee, humanData.Name);
							OnCancel();
						}
					}
				}
			}
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (!_employeeSelection)
            {
				if (nameEvent.Equals(ClockController.EventClockControllerPlayChanged))
				{
					OnCancel();
				}
				if (nameEvent.Equals(EventScreenListEventsHUDViewDestroy))
				{
					OnCancel();
				}
				if (nameEvent.Equals(ApplicationController.EventMainControllerSelectedHuman))
				{
					if (_typeInfo != TypeLateralInfo.PERSONS)
					{
						LoadData();
					}
				}
				if (nameEvent.Equals(RunStateRun.EventRunStateRunDeleteHuman))
				{
					OnCancel();
				}
			}
		}
	}
}