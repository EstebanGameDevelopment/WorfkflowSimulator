using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
using yourvrexperience.VR;
using static yourvrexperience.WorkDay.TaskItemData;

namespace yourvrexperience.WorkDay
{
	public class ScreenTaskManagerView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenTaskManagerView";
		
		public const string EventScreenTaskManagerViewRefreshTasks = "EventScreenTaskManagerViewRefreshTasks";
		public const string EventScreenTaskManagerViewRefreshMembersWorking = "EventScreenTaskManagerViewRefreshMembersWorking";
		public const string EventScreenTaskManagerViewShowTaskInformation = "EventScreenTaskManagerViewShowTaskInformation";
		public const string EventScreenTaskManagerViewDeleteLocalDocs = "EventScreenTaskManagerViewDeleteLocalDocs";

		[SerializeField] private TextMeshProUGUI titleProject;
		[SerializeField] private TextMeshProUGUI titleBoard;
		[SerializeField] private Button buttonCancel;
		[SerializeField] private Button buttonDelete;
		[SerializeField] private GameObject taskViewPrefab;
		[SerializeField] private SlotManagerView[] slotManagers;
		[SerializeField] private TextMeshProUGUI[] textSizeCalculator;
		[SerializeField] private TextMeshProUGUI[] textTitles;
		[SerializeField] private Button[] addNewTask;
		[SerializeField] private TMP_Dropdown DropDownMembers;
		[SerializeField] private TextMeshProUGUI feedbackInformation;
		[SerializeField] private IconColorView iconMemberFilter;
		
		private string _boardName;
		private BoardData _board;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			_boardName = (string)parameters[0];

			buttonCancel.onClick.AddListener(OnCancel);
			buttonDelete.onClick.AddListener(OnDeleteLocalDocs);

			_board = WorkDayData.Instance.CurrentProject.GetBoardFor(_boardName);
			titleProject.text = WorkDayData.Instance.CurrentProject.GetProject(_board.ProjectId)?.Name;
			titleBoard.text = _board.BoardName + " : " + Utilities.ShortenText(_board.Description, 60);

			ProjectInfoData project = WorkDayData.Instance.CurrentProject.GetProject(_board.ProjectId);
			_content.GetComponent<Image>().color = project.GetColor();

			addNewTask[(int)TaskStates.TODO].onClick.AddListener(OnAddToDo);
			addNewTask[(int)TaskStates.DOING].onClick.AddListener(OnAddDoing);
			addNewTask[(int)TaskStates.DONE].onClick.AddListener(OnAddDone);
			addNewTask[(int)TaskStates.VERIFIED].onClick.AddListener(OnAddVerified);

			textTitles[0].text = LanguageController.Instance.GetText("task.state." + TaskStates.TODO.ToString().ToLower());
			textTitles[1].text = LanguageController.Instance.GetText("task.state." + TaskStates.DOING.ToString().ToLower());
			textTitles[2].text = LanguageController.Instance.GetText("task.state." + TaskStates.DONE.ToString().ToLower());
			textTitles[3].text = LanguageController.Instance.GetText("task.state." + TaskStates.VERIFIED.ToString().ToLower());
            
            feedbackInformation.text = "";

			// FILTER MEMBERS
			DropDownMembers.ClearOptions();
			DropDownMembers.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("screen.calendar." + WorkDayData.RESERVED_NAME_ALL)));
			List<WorldItemData> humans = ApplicationController.Instance.LevelView.GetHumans();
			foreach (WorldItemData human in humans)
			{
				DropDownMembers.options.Add(new TMP_Dropdown.OptionData(human.Name));
			}
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
			if (ApplicationController.Instance.SelectedHuman != null)
			{
				List<TMP_Dropdown.OptionData> optionsLoaded = DropDownMembers.options;
				for (int k = 0; k < optionsLoaded.Count; k++)
				{
					if (optionsLoaded[k].text.Equals(ApplicationController.Instance.SelectedHuman.NameHuman))
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

			LoadAllTasksCategories();

			UIEventController.Instance.Event += OnUIEvent;

			CameraXRController.Instance.GameCamera.farClipPlane = 1f;

			if (ApplicationController.Instance.IsPlayMode)
            {
				buttonDelete.interactable = false;
			}
		}

        public override void Destroy()
		{
			base.Destroy();
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

			_board = null;

			CameraXRController.Instance.GameCamera.farClipPlane = 1000;
		}

		private void OnDeleteLocalDocs()
		{
			SystemEventController.Instance.DispatchSystemEvent(EventScreenTaskManagerViewDeleteLocalDocs);
		}

		private void LoadAllTasksCategories()
		{
			LoadTasksCategory(TaskStates.TODO);
			LoadTasksCategory(TaskStates.DOING);
			LoadTasksCategory(TaskStates.DONE);
			LoadTasksCategory(TaskStates.VERIFIED);

			iconMemberFilter.Refresh();
		}

		private void OnSelectedHuman(int value)
		{
			string selectedHuman = DropDownMembers.options[value].text;
			if (value == 0)
			{
				SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDCancelSelectionObject);
			}
			else
			{
				SystemEventController.Instance.DispatchSystemEvent(HumanView.EventHumanViewForceSelection, selectedHuman);
			}
			LoadAllTasksCategories();
		}

		private void LoadTasksCategory(TaskStates state)
		{
			List<TaskItemData> tasksForState = _board.GetTasks((int)state);
			List<TaskItemData> orderedDocs = tasksForState.OrderBy(d => d.Depth).ToList();
			LoadItems(slotManagers[(int)state], orderedDocs, textSizeCalculator[(int)state]);
		}

		private void LoadItems(SlotManagerView slotManager, List<TaskItemData> tasks, TextMeshProUGUI calculator)
		{
			slotManager.ClearCurrentGameObject(true);
			slotManager.Initialize(0, new List<ItemMultiObjectEntry>(), taskViewPrefab);

			for (int i = 0; i < tasks.Count; i++)
			{
				bool addItem = true;
				if (ApplicationController.Instance.IsPlayMode)
                {
					if (ApplicationController.Instance.SelectedHuman != null)
                    {
						if (!tasks[i].IsMemberOfTask(ApplicationController.Instance.SelectedHuman.NameHuman))
                        {
							addItem = false;
						}
					}
				}
				if (addItem)
                {
					slotManager.AddItem(new ItemMultiObjectEntry(this.gameObject, slotManager.Data.Count, tasks[i], calculator));
				}				
			}
		}

		private void OnAddToDo()
		{
			ScreenController.Instance.CreateScreen(ScreenTaskView.ScreenName, false, false, TaskStates.TODO, _boardName);
		}

		private void OnAddDoing()
		{
			ScreenController.Instance.CreateScreen(ScreenTaskView.ScreenName, false, false, TaskStates.DOING, _boardName);
		}
		private void OnAddDone()
		{
			ScreenController.Instance.CreateScreen(ScreenTaskView.ScreenName, false, false, TaskStates.DONE, _boardName);
		}

		private void OnAddVerified()
		{
			ScreenController.Instance.CreateScreen(ScreenTaskView.ScreenName, false, false, TaskStates.VERIFIED, _boardName);
		}

		private void OnCancel()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventScreenTaskManagerViewShowTaskInformation))
            {
				TaskItemData task = (TaskItemData)parameters[0];
				if (task != null)
                {
					float totalHoursDone = WorkDayData.Instance.CurrentProject.GetTotalLoggedTimeForTask(-1, task.UID);
					feedbackInformation.text = task.Name + " "+LanguageController.Instance.GetText("text.estimated")+"("+ task.EstimatedTime + "H) - "+LanguageController.Instance.GetText("task.state.done") +"("+ Utilities.CeilDecimal(totalHoursDone,1) + "h) "+LanguageController.Instance.GetText("word.assigned") +"("+ task.PackMembers() + ")";
                }
            }
			if (nameEvent.Equals(EventScreenTaskManagerViewRefreshTasks))
			{
				LoadAllTasksCategories();
			}
			if (nameEvent.Equals(ItemTaskView.EventItemTaskViewEdit))
			{
				ScreenController.Instance.CreateScreen(ScreenTaskView.ScreenName, false, false, (TaskItemData)parameters[2], _boardName);
			}
			if (nameEvent.Equals(ItemTaskView.EventItemTaskViewDelete))
			{				
				SystemEventController.Instance.DispatchSystemEvent(TasksController.EventTasksControllerDeleteTask, (TaskItemData)parameters[2], _boardName);
			}
			if (nameEvent.Equals(EventScreenTaskManagerViewRefreshMembersWorking))
            {
				int taskUID = (int)parameters[0];
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
	}
}