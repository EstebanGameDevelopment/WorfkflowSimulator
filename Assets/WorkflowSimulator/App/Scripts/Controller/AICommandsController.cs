using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using yourvrexperience.ai;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.CurrentDocumentInProgress;
using static yourvrexperience.WorkDay.TaskItemData;

namespace yourvrexperience.WorkDay
{
	public class AICommandsController : MonoBehaviour
	{
        public const string EventCommandsControllerAddAICommand = "EventCommandsControllerAddAICommand";
        public const string EventCommandsControllerCostUpdated = "EventCommandsControllerCostUpdated";
        public const string EventAICommandsControllerUpdatedCommands = "EventAICommandsControllerUpdatedCommands";
        public const string EventAICommandsControllerEvaluateDocsToWork = "EventAICommandsControllerEvaluateDocsToWork";
        public const string EventAICommandsControllerTaskDocumentsCreated = "EventAICommandsControllerTaskDocumentsCreated";
        public const string EventAICommandsControllerDocumentTODODelete = "EventAICommandsControllerDocumentTODODelete";
        public const string EventAICommandsControllerDocumentsTaskDelete = "EventAICommandsControllerDocumentsTaskDelete";
        public const string EventAICommandsControllerTaskToCompleteSummary = "EventAICommandsControllerTaskToCompleteSummary";
        public const string EventAICommandsControllerTaskToCompleteGlobalDocs = "EventAICommandsControllerTaskToCompleteGlobalDocs";
        public const string EventAICommandsControllerEvaluationDocsCompleted = "EventAICommandsControllerEvaluationDocsCompleted";
        public const string EventAICommandsControllerForceCompleteCurrentTaskProgress = "EventAICommandsControllerForceCompleteCurrentTaskProgress";
        public const string EventAICommandsControllerReportEvaluationDone = "EventAICommandsControllerReportEvaluationDone";

        public const string SubEventAICommandsControllerConfirmForceComplete = "SubEventAICommandsControllerConfirmForceComplete";

        public const bool DEBUG_DOC_EVALUATION = false;
        public const bool DEBUG_COMMANDS = false;

        private static AICommandsController _instance;

		public static AICommandsController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(AICommandsController)) as AICommandsController;
				}
				return _instance;
			}
		}

        private List<IAICommand> _commands = new List<IAICommand>();

        public List<IAICommand> Commands
        {
            get { return _commands; }
        }

        private string _lastCostMessage = "";
        private string _customCostMessage = "";

        private List<CurrentDocumentInProgress> _currentDocumentsInProgress = new List<CurrentDocumentInProgress>();
        private float _timerUpdateProgress = 0;

        private float _timerFallbackCompleted = 0;
        
        private List<(TaskItemData,BoardData,ProjectInfoData)> _taskReadyToStart;
        private TaskItemData _taskInProgressToDefine = null;
        private BoardData _boardInProgressToDefine = null;
        private ProjectInfoData _projectInProgressToDefine = null;

        private List<CurrentDocumentInProgress> _documentsToGenerate = new List<CurrentDocumentInProgress>();
        private CurrentDocumentInProgress _documentBeingGenerated = null;

        private TaskItemData _taskToBeCompleted = null;
        private string _eventTaskToBeCompletedSummary = "";
        private string _eventTaskToBeCompletedGlobalDocs = "";

        private bool _isDialogInProgress = false;
        private CurrentDocumentInProgress _docTargetToComplete;

        public void Initialize()
		{
            SystemEventController.Instance.Event += OnSystemEvent;
			UIEventController.Instance.Event += OnUIEvent;
		}

        void OnDestroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

            _taskInProgressToDefine = null;
            _boardInProgressToDefine = null;
            _projectInProgressToDefine = null;
            _docTargetToComplete = null;
        }

        private void ClearAllCommands()
        {
            for (int j = 0; j < _commands.Count; j++)
            {
                IAICommand command = _commands[j];
                if (command != null)
                {
                    command.Destroy();
                }                
            }
            _commands.Clear();
        }

        public void AddNewAICommand(IAICommand command, bool confirmation, params object[] parameters)
        {
            _commands.Add(command);
            command.Request(confirmation, parameters);
            ApplicationController.Instance.TotalNumberOfAICommands = _commands.Count;
        }

        public List<CurrentDocumentInProgress> GetAllDocuments(StateCurrentDoc state = StateCurrentDoc.NONE)
        {
            List<CurrentDocumentInProgress> docsToDo = new List<CurrentDocumentInProgress>();
            List<CurrentDocumentInProgress> currDocProgress = WorkDayData.Instance.CurrentProject.GetCurrentDocProgress();
            if (currDocProgress != null)
            {
                foreach (CurrentDocumentInProgress currDoc in currDocProgress)
                {
                    switch (state)
                    {
                        case StateCurrentDoc.TODO:
                            if (!currDoc.Working && !currDoc.IsDone())
                            {
                                docsToDo.Add(currDoc);
                            }
                            break;

                        case StateCurrentDoc.DOING:
                            if (currDoc.Working)
                            {
                                docsToDo.Add(currDoc);
                            }                                
                            break;

                        case StateCurrentDoc.DONE:
                            if (!currDoc.Working && currDoc.IsDone())
                            {
                                docsToDo.Add(currDoc);
                            }
                            break;

                        case StateCurrentDoc.NONE:
                            docsToDo.Add(currDoc);
                            break;
                    }
                }
            }
            return docsToDo;
        }

        public List<CurrentDocumentInProgress> ExistsDocumentsToDoForTask(int taskUID)
        {
            List<CurrentDocumentInProgress> docsToDo = new List<CurrentDocumentInProgress>();            
            List<CurrentDocumentInProgress> currDocProgress = WorkDayData.Instance.CurrentProject.GetCurrentDocProgress();
            if (currDocProgress != null)
            {
                foreach (CurrentDocumentInProgress currDoc in currDocProgress)
                {
                    if (currDoc.TaskID == taskUID)
                    {
                        docsToDo.Add(currDoc);
                    }
                }
            }
            return docsToDo;
        }

        public bool IsDocumentsWorkingForTask(int taskUID, string uniqueDocID = "")
        {
            List<CurrentDocumentInProgress> currDocProgress = WorkDayData.Instance.CurrentProject.GetCurrentDocProgress();
            if (currDocProgress != null)
            {
                foreach (CurrentDocumentInProgress currDoc in currDocProgress)
                {
                    if (currDoc.TaskID == taskUID)
                    {
                        if (uniqueDocID.Length > 0)
                        {
                            if (currDoc.GetDocUniqueID().Equals(uniqueDocID))
                            {
                                if (currDoc.Working)
                                {
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            if (currDoc.Working)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public int PendingDocumentToCompleteForTask(int taskUID)
        {
            int totalDocumentsTODO = 0;
            List<CurrentDocumentInProgress> allDocProgress = WorkDayData.Instance.CurrentProject.GetCurrentDocProgress();
            if (allDocProgress != null)
            {
                foreach (CurrentDocumentInProgress currDoc in allDocProgress)
                {
                    if (!currDoc.IsDone() && (currDoc.TaskID == taskUID))
                    {
                        totalDocumentsTODO++;
                    }
                }
            }
            return totalDocumentsTODO;
        }

        public int TotalProgressDocumentForTask(int taskUID)
        {
            int totalDocuments = 0;
            List<CurrentDocumentInProgress> allDocProgress = WorkDayData.Instance.CurrentProject.GetCurrentDocProgress();
            if (allDocProgress != null)
            {
                foreach (CurrentDocumentInProgress currDoc in allDocProgress)
                {
                    if (currDoc.TaskID == taskUID)
                    {
                        totalDocuments++;
                    }
                }
            }
            return totalDocuments;
        }

        public int WorkingDocumentsToCompleteForTask(int taskUID)
        {
            int totalDocumentsInProgress = 0;
            List<CurrentDocumentInProgress> allDocProgress = WorkDayData.Instance.CurrentProject.GetCurrentDocProgress();
            if (allDocProgress != null)
            {
                foreach (CurrentDocumentInProgress currDoc in allDocProgress)
                {
                    if (currDoc.Working && (currDoc.TaskID == taskUID))
                    {
                        totalDocumentsInProgress++;
                    }
                }
            }
            return totalDocumentsInProgress;
        }

        public int TotalPeopleWorkingInTask(int taskUID)
        {
            int totalHumansWorkingInTask = 0;
            List<WorldItemData> humans = ApplicationController.Instance.LevelView.GetHumans();
            foreach (WorldItemData human in humans)
            {
                TaskProgressData currentTaskWorking = human.GetActiveTask();
                if ((currentTaskWorking != null) && (currentTaskWorking.TaskUID == taskUID))
                {
                    totalHumansWorkingInTask++;
                }
            }
            return totalHumansWorkingInTask;
        }

        public bool IsThereAnyHumanWorkingInTask(int taskUID)
        {
            List<WorldItemData> humans = ApplicationController.Instance.LevelView.GetHumans();
            foreach (WorldItemData humanWorking in humans)
            {
                TaskProgressData progress = humanWorking.GetActiveTask();
                if (progress != null)
                {
                    if (progress.TaskUID == taskUID)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void StopWorkingDocumentsForTask(int taskUID, bool ignoreHumansWorking)
        {
            List<CurrentDocumentInProgress> currDocProgress = WorkDayData.Instance.CurrentProject.GetCurrentDocProgress();
            if (currDocProgress != null)
            {
                foreach (CurrentDocumentInProgress currDoc in currDocProgress)
                {                    
                    if (!currDoc.IsDone() && (currDoc.TaskID == taskUID))
                    {
                        if (ignoreHumansWorking)
                        {
                            currDoc.StopWorking();
                        }
                        else
                        {
                            // IS THERE ANY HUMAN WORKING
                            if (!IsThereAnyHumanWorkingInTask(taskUID))
                            {
                                currDoc.StopWorking();
                            }
                        }
                    }
                }
            }
        }

        public bool StartWorkingDocumentsForTask(WorldItemData human, int taskUID)
        {
            List<CurrentDocumentInProgress> currDocProgress = WorkDayData.Instance.CurrentProject.GetCurrentDocProgress();
            if (currDocProgress != null)
            {
                foreach (CurrentDocumentInProgress currDoc in currDocProgress)
                {
                    if (!currDoc.IsDone() 
                        && (currDoc.TaskID == taskUID))
                    {
                        // CHECK DEPENDENCY COMPLETED
                        if (currDoc.Dependency.Length > 0)
                        {
                            bool isDependencyCompleted = true;
                            foreach (CurrentDocumentInProgress checkDoc in currDocProgress)
                            {
                                if (!checkDoc.Equals(currDoc))
                                {
                                    if (checkDoc.Name.Equals(currDoc.Dependency))
                                    {
                                        isDependencyCompleted = checkDoc.IsDone();
                                    }
                                }
                            }
                            if (isDependencyCompleted)
                            {
                                currDoc.StartWorking();
                                return true;                                
                            }
                        }
                        else
                        {
                            currDoc.StartWorking();
                            return true;                            
                        }
                    }
                }
            }
            return false;
        }

        public int CountDependencies(string groupID, string nameDoc, List<string> visited)
        {
            List<WorldItemData> humans = ApplicationController.Instance.LevelView.GetHumans();
            List<CurrentDocumentInProgress> currDocProgress = WorkDayData.Instance.CurrentProject.GetCurrentDocProgress();
            if (currDocProgress != null)
            {
                foreach (CurrentDocumentInProgress currDoc in currDocProgress)
                {
                    if (!visited.Contains(currDoc.Name))
                    {
                        if (currDoc.GetGroupID().Equals(groupID))
                        {
                            if (StringSimilarity.CalculateSimilarityPercentage(currDoc.Name.ToLower(), nameDoc.ToLower()) > 80)
                            {
                                if ((currDoc.Dependency == null) || (currDoc.Dependency.Length == 0))
                                {
                                    return 1;
                                }
                                else
                                {
                                    visited.Add(currDoc.Name);
                                    return 1 + CountDependencies(groupID, currDoc.Dependency, visited);
                                }
                            }
                        }
                    }
                }
            }
            return 0;
        }

        public void CalculateDepthOfDocuments()
        {
            List<CurrentDocumentInProgress> currDocProgress = WorkDayData.Instance.CurrentProject.GetCurrentDocProgress();
            if (currDocProgress != null)
            {
                foreach (CurrentDocumentInProgress currDoc in currDocProgress)
                {
                    if (currDoc.Depth == -1)
                    {
                        if ((currDoc.Dependency == null) && (currDoc.Dependency.Length == 0))
                        {
                            currDoc.Depth = 0;
                        }
                        else
                        {
                            currDoc.Depth = CountDependencies(currDoc.GetGroupID(), currDoc.Dependency, new List<string>());
                        }
                    }
                }
            }
        }

        public void CalculateDocsTODO(TaskItemData task)
        {
            var (currTaskItem, currBoardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(task.UID);
            BoardData currBoard = WorkDayData.Instance.CurrentProject.GetBoardFor(currBoardName);
            ProjectInfoData currProject = WorkDayData.Instance.CurrentProject.GetProject(currBoard.ProjectId);
            AICommandsController.Instance.AddTaskToListTasksCanStart(task, currBoard, currProject);
        }

        public void AddTaskToListTasksCanStart(TaskItemData taskItemData, BoardData boardData, ProjectInfoData projectData)
        {
            if (_taskReadyToStart == null)
            {
                _taskReadyToStart = new List<(TaskItemData, BoardData, ProjectInfoData)>();
            }
            bool shouldAdd = true;
            foreach ((TaskItemData, BoardData, ProjectInfoData) item in _taskReadyToStart)
            {
                if (item.Item1.UID == taskItemData.UID)
                {
                    shouldAdd = false;
                    break;
                }
            }
            if (shouldAdd)
            {
                _taskReadyToStart.Add((taskItemData, boardData, projectData));
            }
        }

        private void RemoveDocTODO(CurrentDocumentInProgress docTODODelete)
        {
            if (_taskReadyToStart == null)
            {
                _taskReadyToStart = new List<(TaskItemData, BoardData, ProjectInfoData)>();
            }            
            List<WorldItemData> humans = ApplicationController.Instance.LevelView.GetHumans();
            foreach (WorldItemData human in humans)
            {
                if (!human.IsClient)
                {
                    if (human.RemoveCurrentDocProgress(docTODODelete.UID))
                    {
                        if (docTODODelete.Working)
                        {
                            TaskProgressData currentTask = human.GetActiveTask();
                            if (currentTask != null)
                            {
                                if (docTODODelete.TaskID == currentTask.TaskUID)
                                {
                                    currentTask.StopProgress("", WorkDayData.Instance.CurrentProject.GetCurrentTime());
                                    UIEventController.Instance.DispatchUIEvent(ScreenTaskManagerView.EventScreenTaskManagerViewRefreshTasks);
                                }
                            }                            
                        }
                    }
                }
            }
            WorkDayData.Instance.CurrentProject.RemoveDocProgress(docTODODelete);
        }

        private void RemoveDocsTask(int taskID)
        {
            if (_taskReadyToStart == null)
            {
                _taskReadyToStart = new List<(TaskItemData, BoardData, ProjectInfoData)>();
            }
            List<WorldItemData> humans = ApplicationController.Instance.LevelView.GetHumans();
            foreach (WorldItemData human in humans)
            {
                if (!human.IsClient)
                {
                    human.RemoveCurrentDocsTask(taskID);
                }
            }
            WorkDayData.Instance.CurrentProject.RemoveDocProgressTask(taskID);
        }

        private void HumansStartToWorkInExistingTasks()
        {
            if (_taskReadyToStart == null)
            {
                _taskReadyToStart = new List<(TaskItemData, BoardData, ProjectInfoData)>();
            }            
            List<WorldItemData> humans = ApplicationController.Instance.LevelView.GetHumans();
            foreach (WorldItemData human in humans)
            {
                if (!human.IsClient)
                {
                    if (human.GetActiveTask() == null)
                    {
                        List<(TaskItemData, BoardData)> tasksAssigned = WorkDayData.Instance.CurrentProject.GetAllTasksAssignedTo(human.Name);
                        List<(TaskItemData, BoardData)> canStartTasks = new List<(TaskItemData, BoardData)>();
                        for (int i = 0; i < tasksAssigned.Count; i++)
                        {
                            var (taskItemData, boardData) = tasksAssigned[i];
                            if (taskItemData != null)
                            {
                                if (taskItemData.IsTaskToDo())
                                {
                                    ProjectInfoData projectData = WorkDayData.Instance.CurrentProject.GetProject(boardData.ProjectId);
                                    List<MeetingData> meetingsForTask = WorkDayData.Instance.CurrentProject.GetMeetingsByTaskUID(taskItemData.UID);
                                    if (meetingsForTask.Count == 0)
                                    {
                                        canStartTasks.Add((taskItemData, boardData));
                                    }
                                }
                            }
                        }
                        for (int j = 0; j < canStartTasks.Count; j++)
                        {
                            TaskItemData taskItemData = canStartTasks[j].Item1;
                            BoardData boardData = canStartTasks[j].Item2;
                            ProjectInfoData projectData = WorkDayData.Instance.CurrentProject.GetProject(boardData.ProjectId);
                            bool shouldAddReady = true;
                            if (taskItemData.Linked != -1)
                            {
                                var (taskLinked, boarName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(taskItemData.Linked);
                                if (taskLinked != null)
                                {
                                    if (!taskLinked.IsTaskCompleted())
                                    {
                                        shouldAddReady = false;
                                    }
                                }
                            }
                            if (shouldAddReady)
                            {
                                AddTaskToListTasksCanStart(taskItemData, boardData, projectData);
                            }                            
                        }
                    }
                }
            }
        }

        private bool UpdateStateTaskInBoard(int taskUID)
        {
            var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(taskUID);
            if (taskItemData != null)
            {
                int totalDocsTODO = PendingDocumentToCompleteForTask(taskUID);
                if (totalDocsTODO == 0)
                {
                    taskItemData.State = (int)TaskStates.DONE;
                    return true;
                }
                else
                {
                    int totalDocsInProgress = WorkingDocumentsToCompleteForTask(taskUID);
                    if (totalDocsInProgress == 0)
                    {
                        taskItemData.State = (int)TaskStates.TODO;
                    }
                    else
                    {
                        taskItemData.State = (int)TaskStates.DOING;
                    }
                }
            }
            return false;
        }

        private void FixDependencies(List<TaskDocumentJSON> docs)
        {
            if (docs == null) return;
            if (docs.Count == 0) return;

            bool forceNullDependency = true;
            foreach (TaskDocumentJSON docIni in docs)
            {
                if ((docIni.dependency == null) || (docIni.dependency.Length == 0))
                {
                    forceNullDependency = false;
                }
            }

            if (forceNullDependency)
            {
                docs[0].dependency = "";
            }

            foreach (TaskDocumentJSON docMeet in docs)
            {
                if ((docMeet.dependency != null) && (docMeet.dependency.Length > 0))
                {
                    bool hasBeenFound = false;
                    foreach (TaskDocumentJSON docCheck in docs)
                    {
                        if (docMeet != docCheck)
                        {
                            if (StringSimilarity.CalculateSimilarityPercentage(docCheck.name.ToLower(), docMeet.dependency.ToLower()) > 85)
                            {
                                docMeet.dependency = docCheck.name;
                                hasBeenFound = true;
                                break;
                            }
                        }
                    }
                    if (!hasBeenFound)
                    {
                        docMeet.dependency = "";
                    }
                }
            }
        }

        private bool IsDocumentPendingToBeCreated(CurrentDocumentInProgress doc)
        {
            if (_documentBeingGenerated != null)
            {
                if (_documentBeingGenerated.Name == doc.Name)
                {
                    return true;
                }
            }

            foreach (CurrentDocumentInProgress docProgress in _documentsToGenerate)
            {
                if (docProgress != null)
                {
                    if (docProgress.Name == doc.Name)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void StopAllDocsProgressWithoutAnyoneAssigned()
        {
            List<CurrentDocumentInProgress> docsInProgress = WorkDayData.Instance.CurrentProject.GetCurrentDocProgress();
            for (int i = 0; i < docsInProgress.Count; i++)
            {
                CurrentDocumentInProgress currDoc = docsInProgress[i];
                if (currDoc != null)
                {
                    if (currDoc.Working)
                    {
                        string[] bufPersons = currDoc.Persons.Split(",");
                        bool anyoneWorking = false;
                        foreach (string person in bufPersons)
                        {
                            string finalPerson = person.Trim();
                            WorldItemData finalHumanData = WorkDayData.Instance.CurrentProject.GetItemByName(finalPerson);
                            if (finalHumanData != null)
                            {
                                TaskProgressData progressData = finalHumanData.GetActiveTask();
                                if (progressData != null)
                                {
                                    if (progressData.TaskUID == currDoc.TaskID)
                                    {
                                        anyoneWorking = true;
                                    }
                                }
                            }
                        }

                        if (!anyoneWorking)
                        {
                            currDoc.StopWorking();
                        }
                    }
                }
            }
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
		{
            if (nameEvent.Equals(ScreenCalendarView.EventScreenCalendarViewSetNewDate))
            {
                ClearAllCommands();
                _currentDocumentsInProgress.Clear();
                _timerUpdateProgress = 0;
            }
            if (nameEvent.Equals(EventAICommandsControllerForceCompleteCurrentTaskProgress))
            {
                CurrentDocumentInProgress docTarget = (CurrentDocumentInProgress)parameters[0];
                if (docTarget != null)
                {
                    _docTargetToComplete = docTarget;
                    ApplicationController.Instance.SetBackgroundScreensColor();

                    string titleComplete = LanguageController.Instance.GetText("text.warning");
                    string descriptionComplete = LanguageController.Instance.GetText("screen.force.complete.current.progress");                    
                    ScreenInformationView.CreateScreenInformation(ScreenInformationToggleView.ScreenName, null, titleComplete, descriptionComplete, SubEventAICommandsControllerConfirmForceComplete);
                    string toggleComplete = LanguageController.Instance.GetText("screen.force.complete.by.ai.or.human");
                    UIEventController.Instance.DispatchUIEvent(ScreenInformationToggleView.EventScreenInformationToggleViewSetToggle, toggleComplete, false);
                }
            }
            if (nameEvent.Equals(SubEventAICommandsControllerConfirmForceComplete))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
                    bool useAI = (bool)parameters[2];

                    bool isCurrentProgressCompleted = false;
                    if ((_docTargetToComplete.Dependency == null) || (_docTargetToComplete.Dependency.Length == 0))
                    {
                        isCurrentProgressCompleted = true;
                    }
                    else
                    {
                        isCurrentProgressCompleted = WorkDayData.Instance.CurrentProject.IsCurrentDocProgressCompleted(_docTargetToComplete.Dependency);
                    }
                    if (isCurrentProgressCompleted)
                    {
                        CurrentDocumentInProgress docTargetToComplete = _docTargetToComplete;
                        _docTargetToComplete = null;
                        docTargetToComplete.IsForHuman = !useAI;
                        _documentsToGenerate.Add(docTargetToComplete);
                    }
                    else
                    {
                        ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.force.complete.cannot.be.done.because.dependencies"));
                    }
                }
                else
                {
                    _docTargetToComplete = null;
                }
            }
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
		{
            if (nameEvent.Equals(EventAICommandsControllerDocumentsTaskDelete))
            {
                int taksUID = (int)parameters[0];
                StopWorkingDocumentsForTask(taksUID, true);
                RemoveDocsTask(taksUID);
            }
            if (nameEvent.Equals(EventAICommandsControllerDocumentTODODelete))
            {
                RemoveDocTODO((CurrentDocumentInProgress)parameters[0]);
            }
            if (nameEvent.Equals(CommandsController.EventCommandsControllerAllHumansReady))
            {
                HumansStartToWorkInExistingTasks();
            }
            if (nameEvent.Equals(EventAICommandsControllerTaskDocumentsCreated))
            {
                if (_taskInProgressToDefine == (TaskItemData)parameters[0])
                {
                    if (parameters[1] != null)
                    {
                        TasksDocumentsJSON tasksDocuments = (TasksDocumentsJSON)parameters[1];

                        if (tasksDocuments.documents != null)
                        {
                            FixDependencies(tasksDocuments.documents);

                            foreach (TaskDocumentJSON docTask in tasksDocuments.documents)
                            {
                                string[] persons = docTask.persons.Split(",");
                                int uidProgress = WorkDayData.Instance.CurrentProject.AddDocProgress(new CurrentDocumentInProgress(WorkDayData.Instance.CurrentProject.GetCurrentProgressNextID(), _projectInProgressToDefine.Id, "", _taskInProgressToDefine.UID, docTask.name, docTask.persons, docTask.dependency, docTask.type, docTask.time, docTask.data, WorkDayData.Instance.CurrentProject.GetCurrentTime()));
                                if (uidProgress == -1)
                                {
                                    Debug.Log("ERROR INSERTING NEW PROGRESS IN PROJECT DATA");
                                }
                                else
                                {
                                    foreach (string person in persons)
                                    {
                                        string personAssigned = person.Trim();
                                        WorldItemData humanData = WorkDayData.Instance.CurrentProject.GetItemByName(personAssigned);
                                        if (humanData != null)
                                        {
                                            TasksController.Instance.StartProgressForHuman(humanData.Name, _taskInProgressToDefine.UID, WorkDayData.Instance.CurrentProject.GetCurrentTime(), false, true);
                                            TaskProgressData currTaskInProgress = humanData.GetTaskProgressByID(_taskInProgressToDefine.UID);
                                            if (currTaskInProgress != null)
                                            {
                                                currTaskInProgress.AddCurrentDocProgressUID(uidProgress);
                                            }                                            
                                        }
                                    }
                                }                                
                            }
                        }
                        _taskInProgressToDefine = null;                        
                        SystemEventController.Instance.DispatchSystemEvent(AICommandsController.EventAICommandsControllerEvaluateDocsToWork);
                    }
                    _taskInProgressToDefine = null;
                }
            }
            if (nameEvent.Equals(TasksController.EventTasksControllerDeletedTaskConfirmation))
            {
                int projectID = (int)parameters[0];
                int taskUID = (int)parameters[1];
                for (int i = 0; i < _currentDocumentsInProgress.Count; i++)
                {
                    CurrentDocumentInProgress currDoc = _currentDocumentsInProgress[i];
                    if (currDoc != null)
                    {
                        if (currDoc.TaskID == taskUID)
                        {
                            _currentDocumentsInProgress.RemoveAt(i);
                            i--;
                        }
                    }
                }
                WorkDayData.Instance.CurrentProject.DeleteTaskWorkLogs(projectID, taskUID);
            }
            if (nameEvent.Equals(TasksController.EventTasksControllerStartedTask))
            {
                WorldItemData human = (WorldItemData)parameters[0];
                TaskItemData taskStarted = (TaskItemData)parameters[1];
                if ((human != null) && (taskStarted != null))
                {
                    StartWorkingDocumentsForTask(human, taskStarted.UID);
                }
            }
            if (nameEvent.Equals(TasksController.EventTasksControllerStoppedTask))
            {
                int taskUIDStopped = (int)parameters[0];
                string nameHuman = (string)parameters[1];
                WorldItemData human = WorkDayData.Instance.CurrentProject.GetItemByName(nameHuman);
                if (human != null)
                {
                    StopWorkingDocumentsForTask(taskUIDStopped, false);
                    UpdateStateTaskInBoard(taskUIDStopped);
                }
            }
            if (nameEvent.Equals(RunStateLoading.EventRunStateLoadingCompleted))
            {
                List<CurrentDocumentInProgress> currDocProgress = WorkDayData.Instance.CurrentProject.GetCurrentDocProgress();
                if (currDocProgress != null)
                {
                    foreach (CurrentDocumentInProgress currDoc in currDocProgress)
                    {
                        if (currDoc.Working)
                        {
                            currDoc.Working = false;
                            currDoc.StartWorking();
                        }
                    }
                }
                SystemEventController.Instance.DispatchSystemEvent(AICommandsController.EventAICommandsControllerEvaluateDocsToWork);
            }
            if (nameEvent.Equals(EventAICommandsControllerEvaluateDocsToWork))
            {
                _timerFallbackCompleted = 0;

                HumansStartToWorkInExistingTasks();

                CalculateDepthOfDocuments();

                List<CurrentDocumentInProgress> docsInProgress = WorkDayData.Instance.CurrentProject.GetCurrentDocProgress();
                List<CurrentDocumentInProgress> docsTodo = new List<CurrentDocumentInProgress>();
                List<CurrentDocumentInProgress> docsReadyEmpty = new List<CurrentDocumentInProgress>();
                List<CurrentDocumentInProgress> docsReadyDependency = new List<CurrentDocumentInProgress>();
                List<(string, int)> namesDocumentsDone = new List<(string, int)>();

                if (DEBUG_DOC_EVALUATION) Debug.Log("EVALUATE DOCS::STEP 1:TOTAL DOCS IN PROGRESS["+ docsInProgress.Count + "]");
                foreach (CurrentDocumentInProgress docInProgress in docsInProgress)
                {
                    DocumentData docData = docInProgress.GetDocumentCreated();
                    
                    if (docData == null)
                    {
                        if (docInProgress.Dependency.Length == 0)
                        {
                            docsReadyEmpty.Add(docInProgress);
                        }
                        else
                        {
                            docsTodo.Add(docInProgress);
                        }
                    }
                    else
                    {
                        namesDocumentsDone.Add((docData.Name, docData.TaskID));
                    }
                }

                foreach (CurrentDocumentInProgress doc in docsTodo)
                {
                    if (doc.Dependency.Length > 0)
                    {
                        List<string> documentsDoneForTask = new List<string>();
                        foreach ((string,int) item in namesDocumentsDone)
                        {
                            if (item.Item2 == doc.TaskID)
                            {
                                documentsDoneForTask.Add(item.Item1);
                            }
                        }
                        if (StringSimilarity.ContainsInList(doc.Dependency, documentsDoneForTask, 90))
                        {
                            string namesTemp = "";
                            foreach (string nameDone in documentsDoneForTask)
                            {
                                namesTemp += nameDone + ",";
                            }
                            docsReadyDependency.Add(doc);
                        }
                    }
                }
                if (DEBUG_DOC_EVALUATION)
                {
                    Debug.Log("EVALUATE DOCS::STEP 2:TOTAL DOCS docsReadyEmpty[" + docsReadyEmpty.Count + "]");
                    Debug.Log("EVALUATE DOCS::STEP 3:TOTAL DOCS docsTodo[" + docsTodo.Count + "]");
                    Debug.Log("EVALUATE DOCS::STEP 4:TOTAL DOCS namesDocumentsDone[" + namesDocumentsDone.Count + "]");
                    Debug.Log("EVALUATE DOCS::STEP 5:DEPENDENCY COMPLETED[" + docsReadyDependency.Count + "]");
                }

                List<(string, int)> idlePeopleStartsTask = new List<(string, int)> ();
                if (docsReadyEmpty.Count > 0)
                {
                    foreach (CurrentDocumentInProgress doc in docsReadyEmpty)
                    {
                        List<string> persons = doc.GetPersons();
                        if (doc.TaskID != -1)
                        {
                            foreach (string person in persons)
                            {
                                WorldItemData humanData = WorkDayData.Instance.CurrentProject.GetItemByName(person);
                                if (humanData != null)
                                {
                                    TaskProgressData taskProgressHuman = humanData.GetActiveTask();
                                    if (taskProgressHuman != null)
                                    {
                                        if (taskProgressHuman.TaskUID == doc.TaskID)
                                        {
                                            doc.StartWorking();
                                        }
                                        else
                                        {
                                            var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(taskProgressHuman.TaskUID);
                                            if (taskItemData == null)
                                            {
                                                idlePeopleStartsTask.Add((humanData.Name, doc.TaskID));
                                            }
                                            else
                                            {
                                                if (taskItemData.IsTaskCompleted())
                                                {
                                                    idlePeopleStartsTask.Add((humanData.Name, doc.TaskID));
                                                }
                                                else
                                                {
                                                    if (!IsDocumentsWorkingForTask(taskProgressHuman.TaskUID))
                                                    {
                                                        idlePeopleStartsTask.Add((humanData.Name, doc.TaskID));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        idlePeopleStartsTask.Add((humanData.Name, doc.TaskID));
                                    }
                                }
                            }
                        }
                    }
                }
                if (docsReadyDependency.Count > 0)
                {
                    foreach (CurrentDocumentInProgress doc in docsReadyDependency)
                    {
                        List<string> persons = doc.GetPersons();
                        if (doc.TaskID != -1)
                        {
                            foreach (string person in persons)
                            {
                                WorldItemData humanData = WorkDayData.Instance.CurrentProject.GetItemByName(person);
                                if (humanData != null)
                                {
                                    TaskProgressData taskProgressHuman = humanData.GetActiveTask();
                                    if (taskProgressHuman != null)
                                    {
                                        if (taskProgressHuman.TaskUID == doc.TaskID)
                                        {
                                            doc.StartWorking();
                                        }
                                        else
                                        {
                                            var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(taskProgressHuman.TaskUID);
                                            if (taskItemData == null)
                                            {
                                                idlePeopleStartsTask.Add((humanData.Name, doc.TaskID));
                                            }
                                            else
                                            {
                                                if (taskItemData.IsTaskCompleted())
                                                {
                                                    idlePeopleStartsTask.Add((humanData.Name, doc.TaskID));
                                                }
                                                else
                                                {
                                                    if (!IsDocumentsWorkingForTask(taskProgressHuman.TaskUID))
                                                    {
                                                        idlePeopleStartsTask.Add((humanData.Name, doc.TaskID));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        idlePeopleStartsTask.Add((humanData.Name, doc.TaskID));
                                    }
                                }
                            }
                        }
                    }
                }

                var distinctPeopleList = idlePeopleStartsTask
                                    .GroupBy(x => x.Item1)   // Group by the first element (string)
                                    .Select(g => g.First())  // Take the first occurrence from each group
                                    .ToList();



                SystemEventController.Instance.DelaySystemEvent(EventAICommandsControllerEvaluationDocsCompleted, 0.1f, distinctPeopleList);
            }
            if (nameEvent.Equals(EventAICommandsControllerEvaluationDocsCompleted))
            {
                List<(string, int)> idlePeopleToStartTask = (List<(string, int)>)parameters[0];
                if (DEBUG_DOC_EVALUATION) Debug.Log("IDLE PEOPLE TO START[" + idlePeopleToStartTask.Count + "]");
                foreach ((string, int) idleToStart in idlePeopleToStartTask)
                {
                    if (DEBUG_DOC_EVALUATION) Debug.Log("START task["+ idleToStart.Item2 + "] for human[" + idleToStart.Item1 + "]");
                    WorldItemData finalHumanData = WorkDayData.Instance.CurrentProject.GetItemByName(idleToStart.Item1);
                    TaskProgressData progressData = finalHumanData.GetActiveTask();
                    if (progressData != null)
                    {
                        progressData.StopProgress("", WorkDayData.Instance.CurrentProject.GetCurrentTime(), true, false);
                    }    
                    WorkDayData.Instance.CurrentProject.StartProgressTask(idleToStart.Item1, idleToStart.Item2, WorkDayData.Instance.CurrentProject.GetCurrentTime());
                }
                StopAllDocsProgressWithoutAnyoneAssigned();
                SystemEventController.Instance.DelaySystemEvent(EventAICommandsControllerReportEvaluationDone, 0.1f);
            }
            if (nameEvent.Equals(CurrentDocumentInProgress.EventCurrentDocumentInProgressStateStarted))
            {
                CurrentDocumentInProgress currentWorking = (CurrentDocumentInProgress)parameters[0];
                bool found = false;
                foreach (CurrentDocumentInProgress doc in _currentDocumentsInProgress)
                {
                    if (doc.GetDocUniqueID().Equals(currentWorking.GetDocUniqueID()))
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    if (DEBUG_DOC_EVALUATION) Debug.Log("START DOC::STEP 1:CONFIRMATION ADDED::["+ currentWorking.Name + "," + currentWorking.Persons.ToUpper() + "]");
                    _currentDocumentsInProgress.Add(currentWorking);
                }
            }
            if (nameEvent.Equals(CurrentDocumentInProgress.EventCurrentDocumentInProgressStateStopped))
            {
                CurrentDocumentInProgress currentWorking = (CurrentDocumentInProgress)parameters[0];
                List<int> tasksIDToStop = new List<int>();
                for (int i = 0; i < _currentDocumentsInProgress.Count; i++)
                {
                    CurrentDocumentInProgress sdoc = _currentDocumentsInProgress[i];
                    if (sdoc.GetDocUniqueID().Equals(currentWorking.GetDocUniqueID()))
                    {
                        if (DEBUG_DOC_EVALUATION) Debug.Log("STOP DOC::STEP 1:CONFIRMATION DELETED::[" + currentWorking.Name + "," + currentWorking.Persons.ToUpper() + "]");
                        _currentDocumentsInProgress.RemoveAt(i);
                        int pendingDocsToCompleteTask = PendingDocumentToCompleteForTask(currentWorking.TaskID);
                        if (pendingDocsToCompleteTask == 0)
                        {
                            if (!tasksIDToStop.Contains(currentWorking.TaskID))
                            {
                                tasksIDToStop.Add(currentWorking.TaskID);
                            }
                        }
                        i--;
                    }
                }

                foreach (int taskIDToStop in tasksIDToStop)
                {
                    SystemEventController.Instance.DispatchSystemEvent(HumanView.EventHumanViewStopWorkingOnTask, taskIDToStop);
                    UpdateStateTaskInBoard(taskIDToStop);
                }
            }
            if (nameEvent.Equals(CurrentDocumentInProgress.EventCurrentDocumentInProgressStateRequestGenerateDoc))
            {
                CurrentDocumentInProgress completeDocWorking = (CurrentDocumentInProgress)parameters[0];
                if (DEBUG_DOC_EVALUATION) Debug.Log("AI CREATE DOC::REQUESTING THE AI TO CREATE THE DOCUMENT [" + completeDocWorking.ToString() + "] AND STOP ANY OTHER INSTANCE OF IT");

                if (_documentBeingGenerated == null)
                {
                    _documentBeingGenerated = completeDocWorking;

                    // ++AI++ GENERATE THE FINAL DOCUMENT
                    AddNewAICommand(new AICommandGenerateDoc(), true, completeDocWorking);
                }
                else
                {
                    _documentsToGenerate.Add(completeDocWorking);
                }
            }
            if (nameEvent.Equals(AICommandGenerateDoc.EventAICommandGenerateDocGenerationCompleted))
            {
                UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationDestroyAllEvenIgnored);
                CurrentDocumentInProgress completedDocument = (CurrentDocumentInProgress)parameters[1];
                _documentBeingGenerated = null;
                bool requestDocEvaluation = true;
                var (taskDocCompleted, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(completedDocument.TaskID);
                if (DEBUG_DOC_EVALUATION) Debug.Log("taskDocCompleted=" + ((TaskStates)taskDocCompleted.State).ToString());
                if ((TaskStates)taskDocCompleted.State == TaskStates.DONE)
                {
                    requestDocEvaluation = false;
                    _taskToBeCompleted = taskDocCompleted;
                    _eventTaskToBeCompletedSummary = EventAICommandsControllerTaskToCompleteSummary + _taskToBeCompleted.UID;
                    _eventTaskToBeCompletedGlobalDocs = EventAICommandsControllerTaskToCompleteGlobalDocs + _taskToBeCompleted.UID;
                    AddNewAICommand(new AICommandSummarizeTask(), true, taskDocCompleted, _eventTaskToBeCompletedSummary);                    
                }
                if (requestDocEvaluation)
                {
                    SystemEventController.Instance.DelaySystemEvent(EventAICommandsControllerEvaluateDocsToWork, 0.1f);
                }
            }
            if (nameEvent.Equals(_eventTaskToBeCompletedSummary))
            {
                if (_taskToBeCompleted != null)
                {
                    SystemEventController.Instance.DelaySystemEvent(HumanView.EventHumanViewStopWorkingOnTask, 0.1f, _taskToBeCompleted.UID);
                }                
                _taskToBeCompleted = null;
                SystemEventController.Instance.DelaySystemEvent(EventAICommandsControllerEvaluateDocsToWork, 0.2f);
            }
            if (nameEvent.Equals(_eventTaskToBeCompletedGlobalDocs))
            {
                if ((bool)parameters[0])
                {
                    if (DEBUG_DOC_EVALUATION) Debug.Log("GLOBAL DOCS FOR TASK[" + _taskToBeCompleted.UID + "]");
                }
                _taskToBeCompleted = null;
                SystemEventController.Instance.DelaySystemEvent(EventAICommandsControllerEvaluateDocsToWork, 0.1f);
            }
            if (nameEvent.Equals(GameAIData.EventGameAIDataCostAIRequest))
            {
                string operation = (string)parameters[0];
                int inputTokens = 0;
                string llm = "";
                if (parameters.Length > 1)
                {
                    inputTokens = (int)parameters[1];
                }
                int outputTokens = 0;
                if (parameters.Length > 2)
                {
                    string outputString = (string)parameters[2];
                    string prettyJson = outputString;
                    bool foundFormat = false;
                    try
                    {
                        JArray jsonArray = JArray.Parse(outputString);
                        prettyJson = jsonArray.ToString(Newtonsoft.Json.Formatting.Indented);
                        foundFormat = true;
                    }
                    catch (Exception err) { };
                    if (!foundFormat)
                    {
                        try
                        {
                            JObject jsonArray = JObject.Parse(outputString);
                            prettyJson = jsonArray.ToString(Newtonsoft.Json.Formatting.Indented);
                            foundFormat = true;
                        }
                        catch (Exception err) { };
                    }
                    outputTokens = prettyJson.Split(' ').Length;
                }
                llm = LanguageController.Instance.GetText("llm.provider." + WorkDayData.Instance.LlmProvider.ToString());
                if (operation.Equals(AskGenericImageGPTHTTP.IMAGE))
                {
                    llm = _lastCostMessage;
                }
                GameAIData.Instance.AskLastOperationCostAI(operation, llm, inputTokens, outputTokens);
            }
            if (nameEvent.Equals(GameAIData.EventGameAIDataCostAIResponse))
            {
                float currentCallCost = (float)parameters[0];
                if (currentCallCost > 0)
                {
                    string operation = (string)parameters[1];
                    string llmProvider = (string)parameters[2];
                    if ((llmProvider == null) || (llmProvider.Length == 0))
                    {
                        llmProvider = LanguageController.Instance.GetText("llm.provider." + WorkDayData.Instance.LlmProvider.ToString());
                    }
                    int inputTokens = (int)parameters[3];
                    int outputTokens = (int)parameters[4];
                    WorkDayData.Instance.CurrentProject.AddNewCost(currentCallCost, operation, llmProvider, inputTokens, outputTokens);
                    float finalCost = currentCallCost;
                    float newTotalCost = WorkDayData.Instance.CurrentProject.GetTotalCost();
                    UIEventController.Instance.DispatchUIEvent(EventCommandsControllerCostUpdated, newTotalCost);
                }
            }
            if (nameEvent.Equals(ApplicationController.EventMainControllerReleaseGameResources))
            {
                ClearAllCommands();
            }
            if (nameEvent.Equals(SystemEventController.EventSystemEventControllerReleaseAllResources))
            {
                _instance = null;
                ClearAllCommands();
                GameObject.Destroy(this.gameObject);
            }
            if (nameEvent.Equals(SystemEventController.EventSystemEventControllerDontDestroyOnLoad))
            {
                if (Instance)
                {
                    DontDestroyOnLoad(Instance.gameObject);
                }
            }
            if (nameEvent.Equals(ScreenDialogView.EventScreenDialogViewStarted))
            {
                _isDialogInProgress = true;
            }
            if (nameEvent.Equals(ScreenDialogView.EventScreenDialogViewClosed))
            {
                _isDialogInProgress = false;
            }            
        }

        private void Update()
        {
            if (!_isDialogInProgress)
            {
                if ((_documentBeingGenerated == null) && (_taskToBeCompleted == null))
                {
                    if (_documentsToGenerate.Count > 0)
                    {
                        _documentBeingGenerated = _documentsToGenerate[0];
                        _documentsToGenerate.RemoveAt(0);

                        // ++AI++ GENERATE THE FINAL DOCUMENT
                        AddNewAICommand(new AICommandGenerateDoc(), true, _documentBeingGenerated);
                    }
                }

                if ((_taskInProgressToDefine == null) && (_documentBeingGenerated == null) && (_taskToBeCompleted == null))
                {
                    if (_taskReadyToStart != null)
                    {
                        if (_taskReadyToStart.Count > 0)
                        {
                            _taskInProgressToDefine = _taskReadyToStart[0].Item1;
                            _boardInProgressToDefine = _taskReadyToStart[0].Item2;
                            _projectInProgressToDefine = _taskReadyToStart[0].Item3;
                            _taskReadyToStart.RemoveAt(0);
                            AddNewAICommand(new AICommandGenerateDocsForTask(), true, _taskInProgressToDefine, _boardInProgressToDefine, _projectInProgressToDefine, EventAICommandsControllerTaskDocumentsCreated);
                        }
                    }
                }

                if ((_taskInProgressToDefine == null) && (_documentBeingGenerated == null) && (_taskToBeCompleted == null))
                {
                    if (ApplicationController.Instance.TimeHUD != null)
                    {
                        if (ApplicationController.Instance.TimeHUD.IsPlayingTime)
                        {
                            if  (_currentDocumentsInProgress.Count == 0)
                            {                                
                                _timerFallbackCompleted += Time.deltaTime;
                                if (_timerFallbackCompleted > 1)
                                {
                                    _timerFallbackCompleted = 0;
                                    List<BoardData> boards = WorkDayData.Instance.CurrentProject.GetAllBoards();
                                    foreach (BoardData board in boards)
                                    {
                                        List<TaskItemData> tasks = WorkDayData.Instance.CurrentProject.GetAllTasks(board);
                                        foreach (TaskItemData task in tasks)
                                        {
                                            if (task.State == (int)TaskStates.DOING)
                                            {
                                                int totalProgressTask = TotalProgressDocumentForTask(task.UID);
                                                if (totalProgressTask > 0)
                                                {
                                                    int pendingDocsToCompleteTask = PendingDocumentToCompleteForTask(task.UID);
                                                    if (pendingDocsToCompleteTask == 0)
                                                    {
                                                        SystemEventController.Instance.DispatchSystemEvent(HumanView.EventHumanViewStopWorkingOnTask, task.UID);
                                                        UpdateStateTaskInBoard(task.UID);
                                                        SystemEventController.Instance.DispatchSystemEvent(AICommandsController.EventAICommandsControllerEvaluateDocsToWork);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // ++AI++ WORK IN DOCUMENTS
            if (ApplicationController.Instance.TimeHUD != null)
            {
                if (ApplicationController.Instance.TimeHUD.IsPlayingTime)
                {
                    if ((_taskInProgressToDefine == null) && (_documentBeingGenerated == null) && (_taskToBeCompleted == null))
                    {
                        if (CommandsController.Instance.CurrentCommandState == CommandsController.CommandStates.Idle)
                        {
                            _timerUpdateProgress += Time.deltaTime;
                            if (_timerUpdateProgress > 1)
                            {
                                _timerUpdateProgress -= 1;

                                float timeIncrement = (float)ApplicationController.Instance.TimeHUD.IncrementTime.TotalMinutes;
                                for (int k = 0; k < _currentDocumentsInProgress.Count; k++)
                                {
                                    CurrentDocumentInProgress currDocInProgress = _currentDocumentsInProgress[k];
                                    if (currDocInProgress != null)
                                    {
                                        if (currDocInProgress.Working)
                                        {
                                            if (currDocInProgress.AnyFreeHuman())
                                            {
                                                currDocInProgress.TimeDone += timeIncrement;
                                                if ((currDocInProgress.TimeDone / 60) >= currDocInProgress.Time)
                                                {
                                                    if (!_isDialogInProgress)
                                                    {
                                                        currDocInProgress.RequestCreateDocument();
                                                        if (currDocInProgress.Requested && !currDocInProgress.IsDone())
                                                        {
                                                            if (!IsDocumentPendingToBeCreated(currDocInProgress))
                                                            {
                                                                currDocInProgress.Requested = false;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


            if (_commands != null)
            {
                // COMMAND INDEPENDENT FROM MEETINGS
                for (int i = 0; i < _commands.Count; i++)
                {
                    IAICommand command = _commands[i];
                    command.Run();
                    if (command.IsCompleted())
                    {
                        command.Destroy();
                        _commands.RemoveAt(i);
                        i--;
                        ApplicationController.Instance.TotalNumberOfAICommands = _commands.Count;
                        if (DEBUG_COMMANDS) Debug.Log("AI COMMAND DESTROYED::TOTAL[" + _commands.Count + "]");
                    }
                }
            }
        }
    }
}