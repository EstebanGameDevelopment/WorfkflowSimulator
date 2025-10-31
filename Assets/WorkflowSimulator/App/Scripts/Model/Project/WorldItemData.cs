using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    [System.Serializable]
    public class AreaData
    {
        public string Name;
        public Vector3 StartPosition;
        public Vector3 EndPosition;
        public int Type;

        public AreaData(string name, Vector3 startPosition, Vector3 endPosition, int type)
        {
            Name = name;
            StartPosition = startPosition;
            EndPosition = endPosition;
            Type = type;
        }

        public AreaData Clone()
        {
            return new AreaData(Name, StartPosition, EndPosition, Type);
        }

        public Vector3 GetCenter()
        {
            return (StartPosition + EndPosition) / 2;
        }
    }

    public class TimeWorkingDataDisplay
    {
        public int TaskUID;
        public DateTime StartTime;
        public DateTime EndTime;
        public string Data;        
        public string Owner;
        public bool InProgress;
        public float TotalDisplayTime;

        public TimeWorkingDataDisplay(int taskUID, DateTime startTime, DateTime endTime, string data)
        {
            TaskUID = taskUID;
            StartTime = startTime;
            EndTime = endTime;
            Data = data;
            InProgress = false;
        }

        public TimeWorkingDataDisplay(int taskUID, DateTime startTime, DateTime endTime, string data, string owner)
        {
            TaskUID = taskUID;
            StartTime = startTime;
            EndTime = endTime;
            Data = data;
            Owner = owner;
            InProgress = false;
        }

        public TimeWorkingDataDisplay(int taskUID, DateTime startTime, DateTime endTime, string data, string owner, bool inProgress)
        {
            TaskUID = taskUID;
            StartTime = startTime;
            EndTime = endTime;
            Data = data;
            Owner = owner;
            InProgress = inProgress;
        }


        public TimeSpan GetTotalTime()
        {
            return EndTime - StartTime;
        }

        public float GetTotalMinutes()
        {
            return (float)GetTotalTime().TotalMinutes;
        }

        public float GetTotalHours()
        {
            return (float)(GetTotalMinutes() / 60f);
        }

    }

    [System.Serializable]
    public class TimeWorkingData
    {
        public string StartTime;
        public string EndTime;
        public string Data;

        public TimeWorkingData(DateTime startTime, DateTime endTime, string data)
        {
            SetStartTime(startTime);
            SetEndTime(endTime);
            Data = data;
        }

        public void SetStartTime(DateTime time)
        {
            StartTime = time.ToString("o");
        }

        public DateTime GetStartTime()
        {
            return DateTime.Parse(StartTime);
        }

        public void SetEndTime(DateTime time)
        {
            EndTime = time.ToString("o");
        }

        public DateTime GetEndTime()
        {
            return DateTime.Parse(EndTime);
        }

        public TimeSpan GetTotalHours()
        {
            return GetEndTime() - GetStartTime();
        }
    }

    [System.Serializable]
    public class CurrentDocumentInProgress : IEquatable<CurrentDocumentInProgress>
    {
        public enum StateCurrentDoc { TODO = 0, DOING = 1, DONE = 2, MEETING = 3, NONE = 4 }

        public const string EventCurrentDocumentInProgressStateStarted = "EventCurrentDocumentInProgressStateStarted";
        public const string EventCurrentDocumentInProgressStateStopped = "EventCurrentDocumentInProgressStateStopped";
        public const string EventCurrentDocumentInProgressStateRequestGenerateDoc = "EventCurrentDocumentInProgressStateRequestGenerateDoc";

        public int UID;
        public int ProjectID;
        public string MeetingID;
        public int TaskID;
        public string Name;
        public string Persons;
        public string Dependency;
        public string Type;
        public int Time;
        public float TimeDone;
        public string Description;
        public string Data;
        public bool Working;
        public bool Requested;
        public int Depth;
        public string DateCreated;
        public string DateStarted;
        public string DateDone;
        public bool IsImage;
        public bool IsForHuman;

        public void SetCreatedTime(DateTime time) { DateCreated = time.ToString("o"); }
        public DateTime GetCreatedTime() { return DateTime.Parse(DateCreated); }

        public void SetStartedTime(DateTime time) { DateStarted = time.ToString("o"); }
        public DateTime GetStartTime() { return DateTime.Parse(DateStarted); }

        public void SetDoneTime(DateTime time) { DateDone = time.ToString("o"); }
        public DateTime GetDoneTime() { return DateTime.Parse(DateDone); }

        public CurrentDocumentInProgress()
        {

        }

        public CurrentDocumentInProgress(int uid, int projectID, string meetingID, int taskID, string name, string persons, string dependency, string type, int time, string description, DateTime timeCreation)
        {
            UID = uid;
            ProjectID = projectID;
            MeetingID = meetingID;
            TaskID = taskID;
            Name = name;
            Persons = persons;
            Dependency = dependency;
            Type = type;
            Time = time;
            TimeDone = 0;
            Description = description;
            Data = "";
            Working = false;
            Requested = false;
            Depth = -1;
            IsImage = false;
            IsForHuman = false;
            SetCreatedTime(timeCreation);
        }

        public bool Equals(CurrentDocumentInProgress other)
        {            
            return Name.Equals(other.Name) &&  (TaskID == other.TaskID) && (ProjectID == other.ProjectID) && (MeetingID == other.MeetingID);
        }

        public string GetOnePerson()
        {
            string[] peopleWorking = Persons.Split(",");
            if (peopleWorking.Length > 0)
            {
                return peopleWorking[0].Trim();
            }
            else
            {
                return "";
            }
        }

        public bool ContainsPerson(string targetPerson)
        {
            List<string> persons = GetPersons();
            foreach (string person in persons)
            {
                if (person.ToLower().Equals(targetPerson.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }

        public List<string> GetPersons()
        {
            List<string> cleanedPersons = new List<string>();
            string[] peopleWorking = Persons.Split(",");
            for (int i = 0; i < peopleWorking.Length; i++)
            {
                cleanedPersons.Add(peopleWorking[i].Trim());
            }
            return cleanedPersons;
        }


        public override string ToString()
        {
            return Name + ";" + Persons + ";" + Dependency.ToUpper() + ";" + Type + ";" + Time + "h;" + Description;
        }

        public bool StartWorking()
        {
            if (!Working)
            {
                Working = true;
                if ((DateStarted == null) || (DateStarted.Length == 0))
                {
                    SetStartedTime(WorkDayData.Instance.CurrentProject.GetCurrentTime());
                }
                SystemEventController.Instance.DispatchSystemEvent(EventCurrentDocumentInProgressStateStarted, this);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsAssignedToHumanControlled()
        {
            if (ApplicationController.Instance.HumanPlayer == null)
            {
                return false;
            }
            else
            {
                string namePlayer = ApplicationController.Instance.HumanPlayer.NameHuman.ToLower();
                List<string> peopleWorking = GetPersons();
                foreach (string person in peopleWorking)
                {
                    if (StringSimilarity.CalculateSimilarityPercentage(namePlayer, person.ToLower()) > 85)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool AnyFreeHuman()
        {
            bool isThereAnyWorkerFree = false;
            List<string> peopleWorking = GetPersons();
            foreach(string person in peopleWorking)
            {                
                MeetingData meeting = MeetingController.Instance.GetMeetingOfHuman(person);
                if (meeting == null)
                {
                    WorldItemData humanData = WorkDayData.Instance.CurrentProject.GetItemByName(person);
                    if (humanData == null)
                    {
                        isThereAnyWorkerFree = true;
                    }
                    else
                    {
                        if (humanData.IsAvailable)
                        {
                            isThereAnyWorkerFree = true;
                        }
                    }
                }
            }

            return isThereAnyWorkerFree;
        }

        public void RequestCreateDocument()
        {
            if (!Requested)
            {
                Requested = true;
                SystemEventController.Instance.DispatchSystemEvent(EventCurrentDocumentInProgressStateRequestGenerateDoc, this);
            }
        }

        public void StopWorking(string data = "", string summary = "", string owner="")
        {
            if (Working)
            {                
                Working = false;
                if (data.Length > 0)
                {
                    if ((DateDone == null) || (DateDone.Length == 0))
                    {
                        SetDoneTime(WorkDayData.Instance.CurrentProject.GetCurrentTime());
                    }
                    Data = data;
                    if (TaskID != -1)
                    {
                        var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(TaskID);
                        if (taskItemData != null)
                        {
                            List<DocumentData> docs = taskItemData.GetData();
                            HTMLData htmlData = new HTMLData();
                            htmlData.html = Data;
                            string finalSummary = Description;
                            if (summary.Length > 0) finalSummary = summary;
                            string finalOwner = owner;
                            if (finalOwner.Length == 0) finalOwner = GetOnePerson();
                            if (IsImage)
                            {                                
                                docs.Add(new DocumentData(WorkDayData.Instance.CurrentProject.GetDocumentNextID(), ProjectID, Name, Description, finalOwner, htmlData, false, IsImage, finalSummary, false, -1, TaskID));
                            }
                            else
                            {
                                docs.Add(new DocumentData(WorkDayData.Instance.CurrentProject.GetDocumentNextID(), ProjectID, Name, Description, finalOwner, htmlData, false, false, finalSummary, false, -1, TaskID));
                            }
                            taskItemData.SetData(docs.ToArray());
                        }
                    }
                    else
                    {
                        if (MeetingID.Length > 0)
                        {
                            MeetingData meeting = WorkDayData.Instance.CurrentProject.GetMeetingByUID(MeetingID);
                            if (meeting != null)
                            {
                                List<DocumentData> docs = meeting.GetData();
                                HTMLData htmlData = new HTMLData();
                                htmlData.html = Data;
                                string finalOwner = owner;
                                if (finalOwner.Length == 0) finalOwner = GetOnePerson();
                                if (IsImage)
                                {
                                    string finalSummary = Description;
                                    if (summary.Length > 0) finalSummary = summary;
                                    docs.Add(new DocumentData(WorkDayData.Instance.CurrentProject.GetDocumentNextID(), ProjectID, Name, Description, finalOwner, htmlData, false, IsImage, finalSummary, false, -1, -1));
                                }
                                else
                                {
                                    docs.Add(new DocumentData(WorkDayData.Instance.CurrentProject.GetDocumentNextID(), ProjectID, Name, Description, finalOwner, htmlData, false, false, "", false, -1, -1));
                                }
                                meeting.SetData(docs.ToArray());
                            }
                        }
                    }
                }
                SystemEventController.Instance.DispatchSystemEvent(EventCurrentDocumentInProgressStateStopped, this);
            }
        }

        public DocumentData GetDocumentCreated()
        {
            if (TaskID != -1)
            {
                var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(TaskID);
                if (taskItemData != null)
                {
                    List<DocumentData> docs = taskItemData.GetData();
                    foreach (DocumentData doc in docs)
                    {
                        if (doc.Name.ToLower().Trim().Equals(Name.ToLower().Trim()))
                        {
                            return doc;
                        }
                    }
                }
            }
            else
            {
                if (MeetingID.Length > 0)
                {
                    MeetingData meeting = WorkDayData.Instance.CurrentProject.GetMeetingByUID(MeetingID);
                    if (meeting != null)
                    {
                        List<DocumentData> docs = meeting.GetData();
                        foreach (DocumentData doc in docs)
                        {
                            if (doc.Name.ToLower().Trim().Equals(Name.ToLower().Trim()))
                            {
                                return doc;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public string GetGroupID()
        {
            return ProjectID + MeetingID + TaskID;
        }

        public string GetDocUniqueID()
        {
            return ProjectID + MeetingID + TaskID + Name;
        }

        public bool IsDone()
        {
            return (Data.Length > 0);
        }

        public void CancelCreation()
        {
            Working = false;
            Data = "CANCELLED";
            SystemEventController.Instance.DispatchSystemEvent(EventCurrentDocumentInProgressStateStopped, this);
        }

        public CurrentDocumentInProgress Clone()
        {
            CurrentDocumentInProgress cloned = new CurrentDocumentInProgress();
            cloned.UID = UID;
            cloned.ProjectID = ProjectID;
            cloned.MeetingID = MeetingID;
            cloned.TaskID = TaskID;
            cloned.Name = Name;
            cloned.Persons = Persons;
            cloned.Dependency = Dependency;
            cloned.Type = Type;
            cloned.Time = Time;
            cloned.TimeDone = TimeDone;
            cloned.Description = Description;
            cloned.Data = Data;
            cloned.Working = Working;
            cloned.Requested = Requested;
            cloned.Depth = Depth;
            cloned.DateCreated = DateCreated;
            cloned.DateStarted = DateStarted;
            cloned.DateDone = DateDone;
            cloned.IsImage = IsImage;
            cloned.IsForHuman = IsForHuman;
            return cloned;
        }
    }

    [System.Serializable]
    public class TaskProgressData : IEquatable<TaskProgressData>
    {
        public int ProjectUID;
        public int TaskUID;
        public bool Working;
        public string StartTime;
        public string Human;
        public int[] CurrentDocProgress;
        public TimeWorkingData[] LoggedTime;

        public TaskProgressData(int projectUID, int taskUID, DateTime startTime, string human)
        {
            ProjectUID = projectUID;
            TaskUID = taskUID;
            Working = true;
            Human = human;
            SetStartTime(startTime);
        }

        public List<int> GetCurrentDocProgress()
        {
            if (CurrentDocProgress != null)
            {
                return CurrentDocProgress.ToList<int>();
            }
            else
            {
                return new List<int>();
            }
        }

        public void SetCurrentDocProgress(List<int> docs)
        {
            CurrentDocProgress = docs.ToArray();
        }

        public bool RemoveCurrentDocProgress(int uid)
        {
            List<int> docs = GetCurrentDocProgress();
            bool output = docs.Remove(uid);
            if (output)
            {
                SetCurrentDocProgress(docs);
            }
            return output;
        }

        public bool AddCurrentDocProgressUID(int uid)
        {
            List<int> docs = GetCurrentDocProgress();
            if (!docs.Contains(uid))
            {
                docs.Add(uid);
                SetCurrentDocProgress(docs);
                return true;
            }
            else
            {
                return false;
            }
        }

        public DateTime GetStartTime()
        {
            return DateTime.Parse(StartTime);
        }

        public void SetStartTime(DateTime startTime)
        {
            StartTime = startTime.ToString("o");
        }

        public List<TimeWorkingData> GetLoggedTime()
        {
            if (LoggedTime != null)
            {
                return LoggedTime.ToList<TimeWorkingData>();
            }
            else
            {
                return new List<TimeWorkingData>();
            }
        }

        public void SetLoggedTime(List<TimeWorkingData> loggedTime)
        {
            LoggedTime = loggedTime.ToArray();
        }

        public float GetTotalHoursLogged()
        {
            float totalTime = 0;
            if (LoggedTime != null)
            {
                foreach (TimeWorkingData log in LoggedTime)
                {
                    totalTime += (float)log.GetTotalHours().TotalHours;
                }
            }
            if (Working)
            {
                int totalDays = (int)((WorkDayData.Instance.CurrentProject.GetCurrentTime() - GetStartTime()).TotalDays);
                int totalWorkingHours = (WorkDayData.Instance.CurrentProject.EndingHour - WorkDayData.Instance.CurrentProject.StartingHour);
                int totalOffHours = 24 - totalWorkingHours;
                int currentHours = (int)((WorkDayData.Instance.CurrentProject.GetCurrentTime() - GetStartTime()).TotalHours) - (totalDays * totalOffHours);
                totalTime += currentHours;
            }
            return (float)totalTime;
        }

        public float GetTotalDecimalHoursLogged()
        {
            float totalTime = 0;
            if (LoggedTime != null)
            {
                foreach (TimeWorkingData log in LoggedTime)
                {
                    totalTime += ((float)log.GetTotalHours().TotalMinutes / 60f);
                }
            }
            if (Working)
            {
                DateTime startTime = GetStartTime();
                DateTime currTime = WorkDayData.Instance.CurrentProject.GetCurrentTime();

                int startingDay = startTime.DayOfYear;
                int endingDay = currTime.DayOfYear;

                if (startingDay < endingDay)
                {
                    // REGISTER PREVIOUS DAY
                    for (int k = startingDay; k < endingDay; k++)
                    {
                        if (k == startingDay)
                        {
                            DateTime endDay = new DateTime(startTime.Year, startTime.Month, startTime.Day, WorkDayData.Instance.CurrentProject.EndingHour, 0, 0);
                            totalTime += (float)((endDay - startTime).TotalMinutes / 60f);
                        }
                        else
                        {
                            DateTime anchorDay = new DateTime(startTime.Year, startTime.Month, startTime.Day, WorkDayData.Instance.CurrentProject.StartingHour, 0, 0);
                            anchorDay = anchorDay.Add(new TimeSpan(24 * (k - startingDay), 0, 0));
                            if (!WorkDayData.Instance.CurrentProject.IsFreeDay(anchorDay))
                            {
                                DateTime endDay = new DateTime(anchorDay.Year, anchorDay.Month, anchorDay.Day, WorkDayData.Instance.CurrentProject.EndingHour, 0, 0);
                                totalTime += (float)((endDay - anchorDay).TotalMinutes / 60f);
                            }
                        }
                    }
                    DateTime todayStart = new DateTime(currTime.Year, currTime.Month, currTime.Day, WorkDayData.Instance.CurrentProject.StartingHour, 0, 0);
                    totalTime += (float)((currTime - todayStart).TotalMinutes / 60f);
                }
                else
                {
                    totalTime += (float)((currTime - startTime).TotalMinutes/60f);
                }
            }
            return totalTime;
        }

        public void StopProgress(string logComment, DateTime stoppedWorkingInTask, bool registerLog = true, bool reportEvent = true)
        {
            if (Working)
            {
                Working = false;
                DateTime startedWorkingInTask = GetStartTime();

                if (registerLog)
                {
                    int startingDay = startedWorkingInTask.DayOfYear;
                    int endingDay = stoppedWorkingInTask.DayOfYear;

                    if (startingDay < endingDay)
                    {
                        // REGISTER PREVIOUS DAY
                        for (int k = startingDay; k < endingDay; k++)
                        {
                            if (k == startingDay)
                            {
                                DateTime endDay = new DateTime(startedWorkingInTask.Year, startedWorkingInTask.Month, startedWorkingInTask.Day, WorkDayData.Instance.CurrentProject.EndingHour, 0, 0);
                                AddLogTime(logComment, startedWorkingInTask, endDay);
                            }
                            else
                            {
                                DateTime anchorDay = new DateTime(startedWorkingInTask.Year, startedWorkingInTask.Month, startedWorkingInTask.Day, WorkDayData.Instance.CurrentProject.StartingHour, 0, 0);
                                anchorDay = anchorDay.Add(new TimeSpan(24 * (k - startingDay), 0, 0));
                                if (!WorkDayData.Instance.CurrentProject.IsFreeDay(anchorDay))
                                {
                                    DateTime endDay = new DateTime(anchorDay.Year, anchorDay.Month, anchorDay.Day, WorkDayData.Instance.CurrentProject.EndingHour, 0, 0);
                                    AddLogTime(logComment, anchorDay, endDay);
                                }
                            }
                        }
                        DateTime todayStart = new DateTime(stoppedWorkingInTask.Year, stoppedWorkingInTask.Month, stoppedWorkingInTask.Day, WorkDayData.Instance.CurrentProject.StartingHour, 0, 0);
                        SetStartTime(todayStart);
                    }

                    // REGISTER CURRENT DAY
                    AddLogTime(logComment, GetStartTime(), stoppedWorkingInTask);
                }

                if (reportEvent)
                {
                    SystemEventController.Instance.DispatchSystemEvent(TasksController.EventTasksControllerStoppedTask, TaskUID, Human);
                }                
            }
        }

        public void AddLogTime(string logComment, DateTime startedWorking, DateTime stoppedWorking)
        {
            TimeWorkingData newLog = new TimeWorkingData(startedWorking, stoppedWorking, logComment);

            List<TimeWorkingData> loggedTime = GetLoggedTime();
            loggedTime.Add(newLog);
            SetLoggedTime(loggedTime);
        }

        public bool Equals(TaskProgressData other)
        {
            return TaskUID.Equals(other.TaskUID);
        }
    }

    [System.Serializable]
    public class WorldItemData : IEquatable<WorldItemData>
    {
        public int Id;
        public int CatalogId;
        public string Name;        
        public string[] Boards;        
        public string Owner;
        public Vector3 Cell;
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 InitialPosition;
        public Vector3 InitialRotation;
        public string Data;
        public bool IsChair;
        public bool IsHuman;
        public bool IsPlayer;
        public bool IsAsshole;
        public bool IsClient;
        public bool IsLead;
        public bool IsSenior;        
        public TaskProgressData[] LoggedWork;
        public bool IsAvailable;
        public bool HasRested;
        public bool IsMan;

        public bool Equals(WorldItemData other)
        {
            return Id == other.Id;
        }

        public string GetSkills()
        {
            if (Data != null)
            {
                int indexOfPersonality = Data.IndexOf(LanguageController.Instance.GetText("word.personality"));
                if (indexOfPersonality != -1)
                {
                    return Data.Substring(0, indexOfPersonality).Trim();
                }
                else
                {
                    return Data;
                }
            }
            else
            {
                return Data;
            }
        }

        public bool RemoveDocProgress(CurrentDocumentInProgress docToDelete)
        {
            if (LoggedWork != null)
            {
                foreach (TaskProgressData taskProgress in LoggedWork)
                {
                    if (taskProgress.CurrentDocProgress != null)
                    {
                        if (taskProgress.RemoveCurrentDocProgress(docToDelete.UID))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public List<TaskProgressData> GetLoggedWork()
        {
            if (LoggedWork != null)
            {
                return LoggedWork.ToList<TaskProgressData>();
            }
            else
            {
                return new List<TaskProgressData>();
            }
        }

        public void SetLoggedWork(List<TaskProgressData> loggedWork)
        {
            LoggedWork = loggedWork.ToArray();
        }

        public List<TimeWorkingDataDisplay> GetAllLoggedWorkRecords(int idProject)
        {
            List<TimeWorkingDataDisplay> loggedRecordsForAllTasks = new List<TimeWorkingDataDisplay>();
            List<TaskProgressData> taskProgressData = GetLoggedWork();
            foreach (TaskProgressData taskProgress in taskProgressData)
            {
                if ((taskProgress.ProjectUID == idProject) || (idProject == -1))
                {
                    List<TimeWorkingData> loggedRecordsForTask = taskProgress.GetLoggedTime();
                    List<TimeWorkingData> sortedList = loggedRecordsForTask.OrderBy(task => task.StartTime).ToList();
                    foreach (TimeWorkingData log in sortedList)
                    {
                        loggedRecordsForAllTasks.Add(new TimeWorkingDataDisplay(taskProgress.TaskUID, log.GetStartTime(), log.GetEndTime(), log.Data));
                    }
                }
            }
            return loggedRecordsForAllTasks;
        }

        public TimeWorkingDataDisplay GetCurrentTaskProgress(int idProject)
        {
            List<TaskProgressData> taskProgressData = GetLoggedWork();
            TaskProgressData currentWorkingTask = null;
            foreach (TaskProgressData taskProgress in taskProgressData)
            {
                if ((taskProgress.ProjectUID == idProject) || (idProject == -1))
                {
                    if (taskProgress.Working)
                    {
                        currentWorkingTask = taskProgress;
                    }
                }
            }
            if (currentWorkingTask == null)
            {
                return null;
            }
            else
            {
                float totalTimeDone = currentWorkingTask.GetTotalDecimalHoursLogged();
                return new TimeWorkingDataDisplay(currentWorkingTask.TaskUID, currentWorkingTask.GetStartTime(), WorkDayData.Instance.CurrentProject.GetCurrentTime(), "");
            }
        }

        public int GetTotalHoursProgressForTask(int projectID, int taskUID)
        {
            List<TaskProgressData> taskProgressData = GetLoggedWork();
            foreach (TaskProgressData taskProgress in taskProgressData)
            {
                if ((taskProgress.ProjectUID == projectID) || (projectID == -1))
                {
                    if (taskProgress.TaskUID == taskUID)
                    {
                        return (int)taskProgress.GetTotalHoursLogged();
                    }
                }
            }
            return 0;
        }

        public float GetTotalDecimalHoursProgressForTask(int projectID, int taskUID)
        {
            List<TaskProgressData> taskProgressData = GetLoggedWork();
            foreach (TaskProgressData taskProgress in taskProgressData)
            {
                if ((taskProgress.ProjectUID == projectID) || (projectID == -1))
                {
                    if (taskProgress.TaskUID == taskUID)
                    {
                        return taskProgress.GetTotalDecimalHoursLogged();
                    }
                }
            }
            return 0;
        }

        public float GetLoggedTimeForTask(int projectID, int taskUID, string owner)
        {
            List<TimeWorkingDataDisplay> unfilteredLogWorks = GetAllLogsWorkForTask(projectID, taskUID, owner);
            List<TimeWorkingDataDisplay> logWorks = new List<TimeWorkingDataDisplay>();
            for (int i = 0; i < unfilteredLogWorks.Count; i++)
            {
                string logComment = unfilteredLogWorks[i].Data;
                string ownerComment = unfilteredLogWorks[i].Owner;
                bool include = true;
                foreach (TimeWorkingDataDisplay log in logWorks)
                {
                    if (log.Data.Equals(logComment) && log.Owner.Equals(ownerComment))
                    {
                        include = false;
                    }
                }
                if (include)
                {
                    logWorks.Add(unfilteredLogWorks[i]);
                }
            }

            int totalWorkingHours = (WorkDayData.Instance.CurrentProject.EndingHour - WorkDayData.Instance.CurrentProject.StartingHour);
            int totalOffHours = 24 - totalWorkingHours;

            for (int i = 0; i < logWorks.Count; i++)
            {
                TimeWorkingDataDisplay currentLog = logWorks[i];
                DateTime earliestStartDate = currentLog.StartTime;
                DateTime latestEndDate = currentLog.EndTime;
                foreach (TimeWorkingDataDisplay unfilteredLog in unfilteredLogWorks)
                {
                    if (unfilteredLog != currentLog)
                    {
                        if (unfilteredLog.Data.Equals(currentLog.Data))
                        {
                            if (unfilteredLog.StartTime < earliestStartDate) earliestStartDate = unfilteredLog.StartTime;
                            if (unfilteredLog.EndTime > latestEndDate) latestEndDate = unfilteredLog.EndTime;
                        }
                    }
                }

                currentLog.StartTime = earliestStartDate;
                currentLog.EndTime = latestEndDate;
                float totalTimeHours = (float)((currentLog.EndTime - currentLog.StartTime).TotalMinutes / 60f);

                int totalDays = latestEndDate.DayOfYear - earliestStartDate.DayOfYear;
                currentLog.TotalDisplayTime = totalTimeHours - (totalDays * totalOffHours);
            }

            float totalTimeLogged = 0;
            for (int i = 0; i < logWorks.Count; i++)
            {
                totalTimeLogged += logWorks[i].TotalDisplayTime;
            }
            return totalTimeLogged;
        }

        public List<TimeWorkingDataDisplay> GetAllLogsWorkForTask(int projectID, int taskUID, string owner)
        {
            List<TimeWorkingDataDisplay> logsWorks = new List<TimeWorkingDataDisplay>();
            List<TaskProgressData> taskProgressData = GetLoggedWork();
            foreach (TaskProgressData taskProgress in taskProgressData)
            {
                if ((taskProgress.ProjectUID == projectID) || (projectID == -1))
                {
                    if (taskProgress.TaskUID == taskUID)
                    {
                        if (taskProgress.Working)
                        {
                            logsWorks.Add(new TimeWorkingDataDisplay(taskUID, taskProgress.GetStartTime(), WorkDayData.Instance.CurrentProject.GetCurrentTime(), LanguageController.Instance.GetText("text.current.working.on.this.task"), owner, true));
                        }
                        List<TimeWorkingData> logs = taskProgress.GetLoggedTime();
                        foreach (TimeWorkingData log in logs)
                        {
                            logsWorks.Add(new TimeWorkingDataDisplay(taskUID, log.GetStartTime(), log.GetEndTime(), log.Data, owner));
                        }
                    }
                }
            }
            return logsWorks;
        }

        public WorldItemData Clone()
        {
            WorldItemData cloned = new WorldItemData();
            cloned.Id = Id;
            cloned.CatalogId = CatalogId;
            cloned.Name = Name;
            cloned.Owner = Owner;
            if ((Boards != null) && (Boards.Length > 0))
            {
                cloned.Boards = new string[Boards.Length];
                for (int i = 0; i < Boards.Length;i++)
                {
                    cloned.Boards[i] = Boards[i];
                }
            }
            cloned.Cell = Cell;
            cloned.Position = Position;
            cloned.Rotation = Rotation;
            cloned.Data = Data;
            cloned.IsChair = IsChair;
            cloned.IsHuman = IsHuman;
            cloned.IsAsshole = IsAsshole;
            cloned.IsPlayer = IsPlayer;
            cloned.IsClient = IsClient;
            cloned.IsAvailable = IsAvailable;            
            cloned.HasRested = HasRested;
            cloned.IsMan = IsMan;
            return cloned;
        }

        public void Copy(WorldItemData source)
        {
            Id = source.Id;
            CatalogId = source.CatalogId;
            Name = source.Name;
            Owner = source.Owner;
            if ((source.Boards != null) && (source.Boards.Length > 0))
            {
                Boards = new string[source.Boards.Length];
                for (int i = 0; i < source.Boards.Length; i++)
                {
                    Boards[i] = source.Boards[i];
                }
            }
            Cell = source.Cell;
            Position = source.Position;
            Rotation = source.Rotation;
            InitialPosition = source.InitialPosition;
            InitialRotation = source.InitialRotation;
            Data = source.Data;
            IsChair = source.IsChair;
            IsHuman = source.IsHuman;
            IsAsshole = source.IsAsshole;
            IsPlayer = source.IsPlayer;
            IsLead = source.IsLead;
            IsSenior = source.IsSenior;
            IsClient = source.IsClient;
            IsAvailable = source.IsAvailable;
            HasRested = source.HasRested;
            IsMan = source.IsMan;
        }

        public void SetBoards(List<string> boards)
        {
            Boards = boards.ToArray();
        }

        public List<string> GetBoards()
        {
            List<string> boards = new List<string>();
            if (Boards != null)
            {
                for (int i = 0; i < Boards.Length; i++)
                {
                    boards.Add(Boards[i]);
                }
            }
            return boards;
        }

        public void DeleteWorkingLogs(int projectID, int taskUID)
        {
            List<TaskProgressData> loggedWork = GetLoggedWork();
            int initial = loggedWork.Count;
            loggedWork.RemoveAll(t => (t.TaskUID == taskUID) && ((t.ProjectUID == projectID) || (projectID == -1)));
            SetLoggedWork(loggedWork);
            int final = GetLoggedWork().Count;
        }

        public TaskProgressData GetActiveTask()
        {
            List<TaskProgressData> loggedWork = GetLoggedWork();
            foreach (TaskProgressData log in loggedWork)
            {
                if (log.Working)
                {
                    return log;
                }
            }
            return null;
        }

        public void StopProgressInAllActiveTasks(string logStoppedTask, DateTime startingTime)
        {
            List<TaskProgressData> loggedWork = GetLoggedWork();
            foreach (TaskProgressData log in loggedWork)
            {
                if (log.Working)
                {
                    log.StopProgress(logStoppedTask, startingTime);
                }
            }
        }

        public TaskProgressData GetTaskProgressByID(int taskID)
        {
            List<TaskProgressData> loggedWork = GetLoggedWork();
            foreach (TaskProgressData log in loggedWork)
            {
                if (log.TaskUID == taskID)
                {
                    return log;
                }
            }
            return null;
        }

        public void Reset()
        {
            SetLoggedWork(new List<TaskProgressData>());
            IsAvailable = true;
            HasRested = false;
            IsAsshole = false;
        }

        public bool RemoveCurrentDocProgress(int uid)
        {
            List<TaskProgressData> loggedWork = GetLoggedWork();
            foreach (TaskProgressData log in loggedWork)
            {
                if (log.RemoveCurrentDocProgress(uid))
                {
                    return true;
                }
            }
            return false;
        }

        public bool RemoveCurrentDocsTask(int taskID)
        {
            bool isWorking = false;
            List<TaskProgressData> loggedWork = GetLoggedWork();
            foreach (TaskProgressData log in loggedWork)
            {
                if (log.TaskUID == taskID)
                {
                    List<int> idDocs = log.GetCurrentDocProgress();
                    foreach (int idDoc in idDocs)
                    {
                        isWorking = true;
                        log.RemoveCurrentDocProgress(idDoc);
                    }                    
                }
            }
            return isWorking;
        }
    }
}