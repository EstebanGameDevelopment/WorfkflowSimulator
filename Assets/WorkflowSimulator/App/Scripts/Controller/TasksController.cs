using yourvrexperience.Utils;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Maything.UI.CalendarSchedulerUI;
using static yourvrexperience.WorkDay.TaskItemData;

namespace yourvrexperience.WorkDay
{
	public class TasksController : MonoBehaviour
	{
		public const string EventTasksControllerAddTask = "EventTasksControllerAddTask";
		public const string EventTasksControllerRequestTask = "EventTasksControllerRequestTask";
		public const string EventTasksControllerResponseTask = "EventTasksControllerResponseTask";
		public const string EventTasksControllerStartTask = "EventTasksControllerStartTask";
		public const string EventTasksControllerStartedTask = "EventTasksControllerStartedTask";
		public const string EventTasksControllerStartCustomTask = "EventTasksControllerStartCustomTask";
		public const string EventTasksControllerStoppedTask = "EventTasksControllerStoppedTask";
		public const string EventTasksControllerDeleteTask = "EventTasksControllerDeleteTask";
		public const string EventTasksControllerDeletedTaskConfirmation = "EventTasksControllerDeletedTaskConfirmation";

        public const string EventTasksControllerAddNewBoard = "EventTasksControllerAddNewBoard";
        public const string EventTasksControllerDeleteBoard = "EventTasksControllerDeleteBoard";
        public const string EventTasksControllerRefreshBoard = "EventTasksControllerRefreshBoard";        

        public const string SubEventExitGameLogCommentStopWorkingInTask = "SubEventExitGameLogCommentStopWorkingInTask";
		public const string SubEventTaskControllerConfirmationDeletionTask = "SubEventTaskControllerConfirmationDeletionTask";
		public const string SubEventTaskControllerConfirmationDeletionBoard = "SubEventTaskControllerConfirmationDeletionBoard";
		public const string SubEventTaskControllerConfirmationDeletionLocalDocs = "SubEventTaskControllerConfirmationDeletionLocalDocs";

		private static TasksController _instance;

		public static TasksController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(TasksController)) as TasksController;
				}
				return _instance;
			}
		}

        private TaskProgressData _taskInProgress;
        private TaskItemData _currentTask;


        public void Initialize()
		{
			SystemEventController.Instance.Event += OnSystemEvent;
			UIEventController.Instance.Event += OnUIEvent;
		}

        void OnDestroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
		}

        public void StartProgressForHuman(string nameHuman, int taskUID, DateTime startingTime, bool isSocialMeeting, bool emptyLog = false)
        {
            WorldItemData itemData = WorkDayData.Instance.CurrentProject.GetItemByName(nameHuman);
            if (itemData.IsHuman)
            {
                if (!isSocialMeeting) itemData.IsAvailable = false;
                TaskProgressData currentTask = itemData.GetActiveTask();
                if (currentTask == null)
                {
                    if (taskUID != -1)
                    {
                        SystemEventController.Instance.DispatchSystemEvent(TasksController.EventTasksControllerStartCustomTask, nameHuman, taskUID, startingTime);
                    }
                }
                else
                {
                    if (taskUID != -1)
                    {
                        if (currentTask.TaskUID != taskUID)
                        {
                            var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(taskUID);
                            string logStoppedTask = "";
                            if (!emptyLog)
                            {
                                logStoppedTask = LanguageController.Instance.GetText("text.progress.stopped.and.started.for.task", taskItemData.Name);
                            }
                            currentTask.StopProgress(logStoppedTask, startingTime);
                            SystemEventController.Instance.DispatchSystemEvent(TasksController.EventTasksControllerStartCustomTask, nameHuman, taskUID, startingTime);
                        }
                    }
                }
            }
        }

        public string GetInformationMembersWorking(int taskUID)
        {
            // MEMBERS WORKING
            List<string> membersWorking = WorkDayData.Instance.CurrentProject.GetHumansWorkingInTask(taskUID);
            string totalMembersWorking = "";
            foreach (string memberWorking in membersWorking)
            {
                if (totalMembersWorking.Length > 0) totalMembersWorking += ", ";
                totalMembersWorking += memberWorking;
            }

            // MEMBERS ASSIGNED
            string totalMembersAssigned = "";
            var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(taskUID);
            if (taskItemData != null)
            {
                List<string> membersAssigned = taskItemData.GetMembers();
                foreach (string memberAssigned in membersAssigned)
                {
                    if (!membersWorking.Contains(memberAssigned))
                    {
                        if (totalMembersAssigned.Length > 0) totalMembersAssigned += ", ";
                        totalMembersAssigned += memberAssigned;
                    }
                }
            }

            string finalMessage = "";
            if (totalMembersWorking.Length > 0)
            {
                finalMessage += LanguageController.Instance.GetText("word.working") + " (" + totalMembersWorking + ") ";
            }
            if (totalMembersAssigned.Length > 0)
            {
                finalMessage += LanguageController.Instance.GetText("word.assigned") +" (" + totalMembersAssigned + ") ";
            }

            return finalMessage;
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
		{
            if (nameEvent.Equals(CalendarSchedulerItem.EventCalendarSchedulerItemTaskProgress))
            {
                int taskUID = int.Parse((string)parameters[0]);
                var (taskData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(taskUID);
                ScreenController.Instance.CreateScreen(ScreenTaskView.ScreenName, false, false, taskData, boardName, true);
            }
            if (nameEvent.Equals(SubEventTaskControllerConfirmationDeletionBoard))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
                    string nameBoardToDelete = (string)parameters[2];
                    if (WorkDayData.Instance.CurrentProject.RemoveBoardByName(nameBoardToDelete))
                    {
                        UIEventController.Instance.DispatchUIEvent(EventTasksControllerRefreshBoard);
                    }
                }
            }
            if (nameEvent.Equals(SubEventTaskControllerConfirmationDeletionTask))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
                    var (taskItemToDelete, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(int.Parse((string)parameters[2]));                    
                    int stateToDelete = taskItemToDelete.State;
                    BoardData board = WorkDayData.Instance.CurrentProject.GetBoardFor(boardName);
                    if (board.DeleteTask(taskItemToDelete))
                    {
                        board.ResetDepth();
                        board.CalculateDepth();
                        WorkDayData.Instance.CurrentProject.DeleteMeetingsLinkedToTaskID(taskItemToDelete.UID);
                        UIEventController.Instance.DispatchUIEvent(ScreenTaskManagerView.EventScreenTaskManagerViewRefreshTasks, stateToDelete);
                    }
                }
            }
            if (nameEvent.Equals(SubEventTaskControllerConfirmationDeletionLocalDocs))
            {                
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
                    List<BoardData> boards = WorkDayData.Instance.CurrentProject.GetAllBoards();
                    foreach (BoardData board in boards)
                    {
                        List<TaskItemData> tasks = WorkDayData.Instance.CurrentProject.GetAllTasks(board);
                        foreach (TaskItemData task in tasks)
                        {
                            task.ClearData();
                        }
                    }
                }
            }
            if (nameEvent.Equals(SubEventExitGameLogCommentStopWorkingInTask))
            {
                if (ApplicationController.Instance.SelectedHuman != null)
                {
                    if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                    {
                        string commentLogWork = (string)parameters[2];
                        if (_taskInProgress != null)
                        {
                            _taskInProgress.StopProgress(commentLogWork, WorkDayData.Instance.CurrentProject.GetCurrentTime());
                            if ((_currentTask != null) && (_taskInProgress.TaskUID != _currentTask.UID))
                            {
                                WorkDayData.Instance.CurrentProject.StartProgressTask(ApplicationController.Instance.SelectedHuman.NameHuman, _currentTask.UID, WorkDayData.Instance.CurrentProject.GetCurrentTime());
                                SystemEventController.Instance.DispatchSystemEvent(EventTasksControllerResponseTask, _taskInProgress.TaskUID, false);
                                SystemEventController.Instance.DispatchSystemEvent(EventTasksControllerResponseTask, _currentTask.UID, true, new List<string>() { ApplicationController.Instance.SelectedHuman.NameHuman });
                            }
                            else
                            {
                                SystemEventController.Instance.DispatchSystemEvent(EventTasksControllerResponseTask, _taskInProgress.TaskUID, false);
                            }
                        }
                        _taskInProgress = null;
                        _currentTask = null;
                    }
                }
            }
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
		{
            if (nameEvent.Equals(ScreenTaskManagerView.EventScreenTaskManagerViewDeleteLocalDocs))
            {
                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("text.do.you.want.to.delete.all.local.docs.task"), SubEventTaskControllerConfirmationDeletionLocalDocs);
            }
            if (nameEvent.Equals(EventTasksControllerDeleteBoard))
            {
                string nameBoardToDelete = (string)parameters[0];
                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmationInput, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("text.do.you.want.to.delete.this.board", nameBoardToDelete) + " : " + nameBoardToDelete, SubEventTaskControllerConfirmationDeletionBoard);
                UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, nameBoardToDelete);
            }
            if (nameEvent.Equals(EventTasksControllerAddNewBoard))
            {
                string nameNewBoard = (string)parameters[0];
                string descriptionNewBoard = (string)parameters[1];
                List<BoardData> boards = WorkDayData.Instance.CurrentProject.GetAllBoards();
                if (!boards.Contains(new BoardData(nameNewBoard, descriptionNewBoard, WorkDayData.Instance.CurrentProject.ProjectInfoSelected)))
                {
                    BoardData newBoard = new BoardData(nameNewBoard, descriptionNewBoard, WorkDayData.Instance.CurrentProject.ProjectInfoSelected);
                    boards.Add(newBoard);
                    WorkDayData.Instance.CurrentProject.SetBoards(boards.ToArray());
                    UIEventController.Instance.DispatchUIEvent(EventTasksControllerRefreshBoard);
                }
                else
                {
                    ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("run.state.project.name.board.existing.try.other"));
                }
            }
            if (nameEvent.Equals(EventTasksControllerRequestTask))
            {
                TaskItemData task = (TaskItemData)parameters[0];
                bool isInProgress = false;
                List<string> humansWorkingInTask = WorkDayData.Instance.CurrentProject.GetHumansWorkingInTask(task.UID);
                if (parameters.Length > 1)
                {
                    string targetHuman = (string)parameters[1];
                    var (itemGO, itemData) = ApplicationController.Instance.LevelView.GetItemByName(targetHuman);
                    if ((itemData != null) && (itemData.IsHuman))
                    {
                        TaskProgressData taskInProgress = itemData.GetActiveTask();
                        if (taskInProgress != null)
                        {
                            if (taskInProgress.TaskUID == task.UID)
                            {
                                isInProgress = true;
                            }
                        }
                    }
                    SystemEventController.Instance.DispatchSystemEvent(EventTasksControllerResponseTask, task.UID, isInProgress, new List<string>() { targetHuman }, humansWorkingInTask);
                }
                else
                {                    
                    if ((humansWorkingInTask != null) && (humansWorkingInTask.Count > 0))
                    {
                        SystemEventController.Instance.DispatchSystemEvent(EventTasksControllerResponseTask, task.UID, true, humansWorkingInTask);
                    }
                    else
                    {
                        SystemEventController.Instance.DispatchSystemEvent(EventTasksControllerResponseTask, task.UID, false);
                    }
                }
            }
            if (nameEvent.Equals(EventTasksControllerStartTask))
            {
                TaskItemData task = (TaskItemData)parameters[0];
                if (ApplicationController.Instance.SelectedHuman == null)
                {
                    if (!ApplicationController.Instance.IsPlayMode)
                    {
                        ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("text.task.cannot.start.without.employee.selected"));
                        return;
                    }
                    else
                    {
                        if (ApplicationController.Instance.HumanPlayer != null)
                        {
                            SystemEventController.Instance.DispatchSystemEvent(HumanView.EventHumanViewForceSelection, ApplicationController.Instance.HumanPlayer.NameHuman);
                        }
                    }
                }
                if (ApplicationController.Instance.SelectedHuman != null)
                {
                    if (task.Linked != -1)
                    {
                        var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(task.Linked);
                        if (taskItemData != null)
                        {
                            if (!taskItemData.IsTaskCompleted())
                            {
                                string description = LanguageController.Instance.GetText("text.cannot.start.before.finishing.task") + " " + taskItemData.Name;
                                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), description);
                                return;
                            }
                        }
                    }
                    TaskProgressData taskInProgress = ApplicationController.Instance.SelectedHuman.GetActiveTask();
                    if (taskInProgress != null)
                    {
                        _taskInProgress = taskInProgress;
                        _currentTask = task;
                        var (previousTask, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_taskInProgress.TaskUID);
                        string description = LanguageController.Instance.GetText("text.log.work.description") + " " + previousTask.Name;
                        ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenMediumInput, null, LanguageController.Instance.GetText("text.log.work.title"), description, SubEventExitGameLogCommentStopWorkingInTask);
                    }
                    else
                    {
                        List<CurrentDocumentInProgress> allProgressDocs = AICommandsController.Instance.GetAllDocuments();
                        bool foundProgressDocs = false;
                        foreach (CurrentDocumentInProgress currProgress in allProgressDocs)
                        {
                            if (currProgress.TaskID == task.UID)
                            {
                                foundProgressDocs = true;
                            }
                        }
                        if (foundProgressDocs)
                        {
                            WorkDayData.Instance.CurrentProject.StartProgressTask(ApplicationController.Instance.SelectedHuman.NameHuman, task.UID, WorkDayData.Instance.CurrentProject.GetCurrentTime());
                            SystemEventController.Instance.DispatchSystemEvent(EventTasksControllerResponseTask, task.UID, true, new List<string>() { ApplicationController.Instance.SelectedHuman.NameHuman });
                        }
                        else
                        {
                            List<MeetingData> meetingsLinkedWithTask = WorkDayData.Instance.CurrentProject.GetMeetingsByTaskUID(task.UID);
                            if ((meetingsLinkedWithTask == null) || (meetingsLinkedWithTask.Count == 0))
                            {
                                AICommandsController.Instance.CalculateDocsTODO(task);
                            }
                            else
                            {
                                string descriptionLinkedMeeting = LanguageController.Instance.GetText("text.cannot.start.task.because.linked.meeting", meetingsLinkedWithTask[0].Name);
                                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), descriptionLinkedMeeting);
                            }
                        }
                    }
                }
            }
            if (nameEvent.Equals(EventTasksControllerStartCustomTask))
            {
                string nameHuman = (string)parameters[0];
                int taskUID = (int)parameters[1];
                DateTime startingTime = (DateTime)parameters[2];

                WorkDayData.Instance.CurrentProject.StartProgressTask(nameHuman, taskUID, startingTime);
                SystemEventController.Instance.DispatchSystemEvent(EventTasksControllerResponseTask, taskUID, true, new List<string>() { nameHuman });
            }
            if (nameEvent.Equals(EventTasksControllerAddTask))
            {
                TaskItemData task = null;
                if (parameters[0] == null)
                {
                    string nameBoard = (string)parameters[1];
                    string nameTask = (string)parameters[2];
                    string descriptionTask = (string)parameters[3];
                    DocumentData[] dataTask = (DocumentData[])parameters[4];
                    int estimatedTime = (int)parameters[5];
                    int stateTask = (int)parameters[6];
                    int linkedTask = (int)parameters[7];
                    int featureTask = (int)parameters[8];

                    task = new TaskItemData(WorkDayData.Instance.CurrentProject.GetTaskNextID(), nameTask, descriptionTask, dataTask, estimatedTime, stateTask, linkedTask, featureTask, (string[])parameters[9]);
                    task.IsUserCreated = ApplicationController.Instance.IsPlayMode;
                    BoardData board = WorkDayData.Instance.CurrentProject.GetBoardFor(nameBoard);
                    if (!board.ExistTask(nameTask))
                    {
                        List<TaskItemData> taskEntries = board.GetTasks();
                        taskEntries.Add(task);
                        board.SetTasks(taskEntries.ToArray());
                        board.ResetDepth();
                        board.CalculateDepth();
                    }

                    UIEventController.Instance.DispatchUIEvent(ScreenTaskManagerView.EventScreenTaskManagerViewRefreshTasks, stateTask);
                }
                else
                {
                    task = (TaskItemData)parameters[0];
                    int previousStateTask = task.State;
                    string nameBoard = (string)parameters[1];
                    task.Name = (string)parameters[2];
                    task.Description = (string)parameters[3];
                    task.Data = (DocumentData[])parameters[4];
                    task.EstimatedTime = (int)parameters[5];
                    task.State = (int)parameters[6];
                    task.Linked = (int)parameters[7];
                    task.Feature = (int)parameters[8];
                    task.SetMembers(((string[])parameters[9]).ToList<string>());

                    BoardData board = WorkDayData.Instance.CurrentProject.GetBoardFor(nameBoard);
                    if (board != null)
                    {
                        board.ResetDepth();
                        board.CalculateDepth();
                    }

                    if (((TaskStates)task.State == TaskStates.DONE) && (previousStateTask != task.State))
                    {
                        AICommandsController.Instance.AddNewAICommand(new AICommandSummarizeTask(), task.HasHumanPlayer(), task, "");
                    }

                    UIEventController.Instance.DispatchUIEvent(ScreenTaskManagerView.EventScreenTaskManagerViewRefreshTasks, task.State);
                }
                UIEventController.Instance.DispatchUIEvent(ScreenMeetingView.EventScreenMeetingViewReloadData);
            }
            if (nameEvent.Equals(EventTasksControllerDeleteTask))
            {
                TaskItemData task = (TaskItemData)parameters[0];
                string boardName = (string)parameters[1];

                List<string> membersWorkingInTask = WorkDayData.Instance.CurrentProject.GetHumansWorkingInTask(task.UID);
                if (membersWorkingInTask.Count > 0)
                {
                    ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("text.cannot.delete.task.in.progress"));
                }
                else
                {
                    ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmationInput, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("text.do.you.want.to.delete.to.this.task", task.Name) + " : " + task.Name, SubEventTaskControllerConfirmationDeletionTask);
                    UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, task.UID.ToString());
                }
            }
            if (nameEvent.Equals(ApplicationController.EventMainControllerReleaseGameResources))
			{
                _taskInProgress = null;
                _currentTask = null;
            }
            if (nameEvent.Equals(SystemEventController.EventSystemEventControllerReleaseAllResources))
			{
				_instance = null;
                _taskInProgress = null;
                _currentTask = null;
                GameObject.Destroy(this.gameObject);
			}
			if (nameEvent.Equals(SystemEventController.EventSystemEventControllerDontDestroyOnLoad))
			{
				if (Instance)
				{
					DontDestroyOnLoad(Instance.gameObject);
				}
			}
		}
	}
}