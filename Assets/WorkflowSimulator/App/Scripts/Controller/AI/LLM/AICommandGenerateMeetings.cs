using UnityEngine;
using yourvrexperience.Utils;
using System;
using System.Collections.Generic;
using InGameCodeEditor;
using System.Linq;

namespace yourvrexperience.WorkDay
{
    public class AICommandGenerateMeetings : IAICommand
    {
        public const string EventAICommandMeetingsDefinitionRequest = "EventAICommandMeetingsDefinitionRequest";
        public const string EventAICommandMeetingsDefinitionResponse = "EventAICommandMeetingsDefinitionResponse";
        public const string EventAICommandMeetingsDefinitionPromptConfirmation = "EventAICommandMeetingsDefinitionPromptConfirmation";
        public const string EventAICommandMeetingsDefinitionPromptRetry = "EventAICommandMeetingsDefinitionPromptRetry";
        public const string EventAICommandMeetingsDefinitionCompleted = "EventAICommandMeetingsDefinitionCompleted";

        public const bool DEBUG = false;

        private bool _isCompleted = false;
        private string _outputEvent;
        private PromptBuilder _promptBuilder;
        private bool _confirmation;
        private ProjectInfoData _project;
        private Dictionary<int, List<TaskItemData>> _uidFeatures;
        private Dictionary<int, MeetingForTaskListJSON> _meetingsDefined;
        private List<int> _uidFeatureToProcess;
        private int _currentUIDFeature = 0;
        private int _iterationsToCancel = WorkDayData.TOTAL_RETRIES_AI_FAILED;

        private Dictionary<MeetingForTaskJSON, TaskItemData> _linkedMeetings = new Dictionary<MeetingForTaskJSON, TaskItemData>();

        private Dictionary<int, TimeMeetingLog> _lastTimeForFeature = new Dictionary<int, TimeMeetingLog>();

        class TimeMeetingLog
        {
            public DateTime startingDay;
            public DateTime timeMeeting;
            public DateTime lunchHour;
            public DateTime endLunchHour;
            public DateTime endingHour;
        }

        public string Name
        {
            get
            {
                return "GenerateBoard";
            }
        }

        public bool IsCompleted()
        {
            return _isCompleted;
        }

        public void Request(bool confirmation, params object[] parameters)
        {
            _confirmation = confirmation;

            SystemEventController.Instance.Event += OnSystemEvent;
            UIEventController.Instance.Event += OnUIEvent;

            _outputEvent = (string)parameters[0];

            _uidFeatures = new Dictionary<int, List<TaskItemData>>();
            _meetingsDefined = new Dictionary<int, MeetingForTaskListJSON>();
            _uidFeatureToProcess = new List<int>();
            _project = WorkDayData.Instance.CurrentProject.GetProject(WorkDayData.Instance.CurrentProject.ProjectInfoSelected);
            List<BoardData> boards = WorkDayData.Instance.CurrentProject.GetAllBoards();
            foreach (BoardData board in boards)
            {
                if (board.ProjectId == _project.Id)
                {
                    board.CalculateDepth();
                    List<TaskItemData> tasks = board.GetTasks();
                    foreach (TaskItemData task in tasks)
                    {
                        List<TaskItemData> tasksForFeature = GetListTaskByFeatureUID(task.Feature);
                        if (!tasksForFeature.Contains(task))
                        {
                            tasksForFeature.Add(task);
                            if (!_uidFeatureToProcess.Contains(task.Feature))
                            {
                                _uidFeatureToProcess.Add(task.Feature);
                            }
                        }
                    }
                }
            }

            if (DEBUG)
            {
                Debug.Log("++++++++++++++++ TOTAL FEATURES[" + _uidFeatures.Count + "/" + _uidFeatures.Count + "]");
                foreach (KeyValuePair<int, List<TaskItemData>> item in _uidFeatures)
                {
                    Debug.Log("++++++++++++++++ FEATURE[" + item.Key + "] WITH TASKS[" + item.Value.Count + "]");
                    foreach (TaskItemData task in item.Value)
                    {
                        Debug.Log("++++ TASK[" + task.Name + "]");
                    }
                }
            }

            BuildPromptMeetingsDefinition();
            if (_confirmation)
            {
                string title = LanguageController.Instance.GetText("ai.title.generate.meetings.for.tasks");
                SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
                ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandMeetingsDefinitionPromptConfirmation);
                UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewInitValues, _promptBuilder);
                UIEventController.Instance.DelayUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRefreshInput, 0.1f);
                SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, false);
            }
            else
            {
                AskMeetingsLLM(_promptBuilder.BuildPrompt());
            }
        }

        private bool IntervalsIntersect(DateTime startA, DateTime endA, DateTime startB, DateTime endB)
        {
            return startA < endB && startB < endA;
        }

        private List<TaskItemData> GetListTaskByFeatureUID(int featureUID)
        {
            List<TaskItemData> tasksForFeature;
            if (_uidFeatures.TryGetValue(featureUID, out tasksForFeature))
            {
                return tasksForFeature;
            }
            else
            {
                tasksForFeature = new List<TaskItemData>();
                _uidFeatures.Add(featureUID, tasksForFeature);
                return tasksForFeature;
            }
        }

        public void Destroy()
        {
            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
            if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

            SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, true);
            if (_meetingsDefined != null)
            {
                DateTime featureStartTime = WorkDayData.Instance.CurrentProject.GetCurrentTime();

                List<MeetingData> meetingsProject = WorkDayData.Instance.CurrentProject.GetMeetings(true);
                if (meetingsProject.Count > 0)
                {
                    List<MeetingData> sortedMeetings = meetingsProject.OrderBy(m => m.GetTimeEnd()).ToList();
                    featureStartTime = sortedMeetings[sortedMeetings.Count - 1].GetTimeEnd();
                    featureStartTime += new TimeSpan(1, 0, 0);
                }

                foreach (KeyValuePair<int, MeetingForTaskListJSON> item in _meetingsDefined)
                {
                    DocumentData docFeature = WorkDayData.Instance.CurrentProject.GetDocumentByFeatureID(item.Key);
                    MeetingForTaskListJSON meetingsCreated = item.Value;
                    List<TaskItemData> tasksForFeature = GetListTaskByFeatureUID(item.Key);
                    foreach (MeetingForTaskJSON meeting in meetingsCreated.meetings)
                    {
                        TaskItemData targetTask = null;
                        int similarityMax = 0;
                        foreach (TaskItemData task in tasksForFeature)
                        {
                            float similarityCurrent = StringSimilarity.CalculateSimilarityPercentage(task.Name.Trim(), meeting.task.Trim());
                            if (similarityCurrent > 80)
                            {
                                if (similarityCurrent > similarityMax)
                                {
                                    similarityMax = (int)similarityCurrent;
                                    targetTask = task;
                                }
                            }
                        }
                        if (targetTask != null)
                        {
                            _linkedMeetings.Add(meeting, targetTask);
                        }
                    }
                }

                List<TaskItemData> finalListAllTasks = new List<TaskItemData>();
                foreach (KeyValuePair<MeetingForTaskJSON,TaskItemData> item in _linkedMeetings)
                {
                    if (!finalListAllTasks.Contains(item.Value))
                    {
                        finalListAllTasks.Add(item.Value);
                    }
                }

                DateTime currentTimeO = featureStartTime;
                DateTime startingDayO = new DateTime(currentTimeO.Year, currentTimeO.Month, currentTimeO.Day, WorkDayData.Instance.CurrentProject.StartingHour, 30, 0);                    
                DateTime timeMeetingO = new DateTime(currentTimeO.Year, currentTimeO.Month, currentTimeO.Day, WorkDayData.Instance.CurrentProject.StartingHour, 30, 0);
                DateTime lunchHourO = new DateTime(currentTimeO.Year, currentTimeO.Month, currentTimeO.Day, WorkDayData.Instance.CurrentProject.LunchHour, 0, 0);
                DateTime endLunchHourO = new DateTime(currentTimeO.Year, currentTimeO.Month, currentTimeO.Day, WorkDayData.Instance.CurrentProject.LunchHour, 0, 0) + new TimeSpan(0, ClockController.TotalLunchTime, 0);
                DateTime endingHourO = new DateTime(currentTimeO.Year, currentTimeO.Month, currentTimeO.Day, WorkDayData.Instance.CurrentProject.EndingHour, 0, 0);
                List<TaskItemData> orderedTasks = finalListAllTasks.OrderBy(d => d.Depth).ToList();

                foreach (TaskItemData task in orderedTasks)
                {
                    foreach (KeyValuePair<MeetingForTaskJSON, TaskItemData> element in _linkedMeetings)
                    {
                        if (element.Value == task)
                        {
                            TimeMeetingLog timeLog;
                            if (!_lastTimeForFeature.TryGetValue(task.Feature, out timeLog))
                            {
                                timeLog = new TimeMeetingLog();
                                timeLog.startingDay = new DateTime(currentTimeO.Year, currentTimeO.Month, currentTimeO.Day, WorkDayData.Instance.CurrentProject.StartingHour, 30, 0);
                                timeLog.timeMeeting = new DateTime(currentTimeO.Year, currentTimeO.Month, currentTimeO.Day, WorkDayData.Instance.CurrentProject.StartingHour, 30, 0);
                                timeLog.lunchHour = new DateTime(currentTimeO.Year, currentTimeO.Month, currentTimeO.Day, WorkDayData.Instance.CurrentProject.LunchHour, 0, 0);
                                timeLog.endLunchHour = new DateTime(currentTimeO.Year, currentTimeO.Month, currentTimeO.Day, WorkDayData.Instance.CurrentProject.LunchHour, 0, 0) + new TimeSpan(0, ClockController.TotalLunchTime, 0);
                                timeLog.endingHour = new DateTime(currentTimeO.Year, currentTimeO.Month, currentTimeO.Day, WorkDayData.Instance.CurrentProject.EndingHour, 0, 0);
                                _lastTimeForFeature.Add(task.Feature, timeLog);
                            }
                            MeetingForTaskJSON meetingToCreate = element.Key;
                            int timeMinutes = meetingToCreate.time;

                            // FIX THE ASSISTING MEMBERS
                            string[] meetingAssistants = meetingToCreate.persons.Split(",");
                            List<string> finalAssistants = meetingAssistants.ToList<string>();
                            List<string> initiallyAssigned = task.GetMembers();
                            foreach (string initialAssistant in initiallyAssigned)
                            {
                                string assist = initialAssistant.Trim();
                                if (!finalAssistants.Contains(assist))
                                {
                                    string found = StringSimilarity.GetInList(initialAssistant, finalAssistants, 90);
                                    if (found != null)
                                    {
                                        finalAssistants.Add(found);
                                    }
                                }
                            }
                            for (int k = 0; k < finalAssistants.Count; k++)
                            {
                                finalAssistants[k] = finalAssistants[k].Trim();
                                string assistant = finalAssistants[k];
                                WorldItemData humanData = WorkDayData.Instance.CurrentProject.GetItemByName(assistant);
                                if ((humanData == null) || ((humanData != null) && (!humanData.IsHuman)))
                                {
                                    string closestName = WorkDayData.Instance.CurrentProject.GetClosestName(assistant, 90, true);
                                    if (closestName == null)
                                    {
                                        finalAssistants.RemoveAt(k);
                                        k--;
                                    }
                                    else
                                    {
                                        finalAssistants[k] = closestName;
                                        humanData = WorkDayData.Instance.CurrentProject.GetItemByName(closestName);
                                        if ((humanData == null) || ((humanData != null) && (!humanData.IsHuman)))
                                        {
                                            finalAssistants.RemoveAt(k);
                                            k--;
                                        }
                                    }
                                }
                            }

                            finalAssistants = StringSimilarity.RemoveDuplicates(finalAssistants);

                            CorrectTime(timeLog, timeMinutes);

                            // IF THE TASK IS LINKED TO THE COMPLETION OF ANOTHER, CONSIDER THE ESTIMATION
                            if (task.Linked != -1)
                            {
                                DateTime initTimeMeeting = timeLog.timeMeeting;
                                var (taskItemData, boardNM) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(task.Linked);
                                if (taskItemData != null)
                                {
                                    int hoursEstimated = taskItemData.EstimatedTime;
                                    List<MeetingData> meetingsForTask = WorkDayData.Instance.CurrentProject.GetMeetingsByTaskUID(task.Linked);
                                    if ((meetingsForTask != null) && (meetingsForTask.Count > 0))
                                    {
                                        List<MeetingData> sortedMeetingsForTask = meetingsForTask.OrderBy(meeting => meeting.GetTimeStart()).ToList();
                                        MeetingData lastMeeting = sortedMeetingsForTask[sortedMeetingsForTask.Count - 1];
                                        DateTime lastTimeMeeting = lastMeeting.GetTimeEnd();
                                        lastTimeMeeting += new TimeSpan(hoursEstimated, 0, 0);
                                        if (timeLog.timeMeeting < lastTimeMeeting)
                                        {
                                            if ((initTimeMeeting < timeLog.lunchHour) && (lastTimeMeeting > timeLog.lunchHour))
                                            {
                                                timeLog.timeMeeting = lastMeeting.GetTimeEnd() + new TimeSpan(hoursEstimated + 1, 0, 0);
                                            }
                                            else
                                            {
                                                timeLog.timeMeeting = lastTimeMeeting;
                                            }
                                        }
                                    }

                                    if (timeLog.timeMeeting > timeLog.endingHour)
                                    {
                                        int hoursConsumed = timeLog.endingHour.Hour - initTimeMeeting.Hour;
                                        do
                                        {
                                            timeLog.startingDay = timeLog.startingDay + new TimeSpan(24, 0, 0);
                                            timeLog.timeMeeting = new DateTime(timeLog.startingDay.Year, timeLog.startingDay.Month, timeLog.startingDay.Day, WorkDayData.Instance.CurrentProject.StartingHour, 30, 0);
                                            timeLog.lunchHour = new DateTime(timeLog.startingDay.Year, timeLog.startingDay.Month, timeLog.startingDay.Day, WorkDayData.Instance.CurrentProject.LunchHour, 0, 0);
                                            timeLog.endLunchHour = new DateTime(timeLog.startingDay.Year, timeLog.startingDay.Month, timeLog.startingDay.Day, WorkDayData.Instance.CurrentProject.LunchHour, 0, 0) + new TimeSpan(0, ClockController.TotalLunchTime, 0);
                                            timeLog.endingHour = new DateTime(timeLog.startingDay.Year, timeLog.startingDay.Month, timeLog.startingDay.Day, WorkDayData.Instance.CurrentProject.EndingHour, 0, 0);
                                        } while (WorkDayData.Instance.CurrentProject.IsFreeDay(timeLog.startingDay));

                                        int extendedStartNextDay = (hoursEstimated - hoursConsumed);
                                        if (extendedStartNextDay > 0)
                                        {
                                            timeLog.timeMeeting += new TimeSpan(extendedStartNextDay, 0, 0);
                                        }
                                    }
                                    else
                                    {
                                        CorrectTime(timeLog, timeMinutes);
                                    }
                                }
                            }

                            CorrectTime(timeLog, timeMinutes);

                            // IS PRESENT IN ANOTHER MEETING AT THE SAME TIME
                            List<MeetingData> existingMeetings = WorkDayData.Instance.CurrentProject.GetMeetings(true);
                            if (existingMeetings.Count > 0)
                            {
                                foreach (MeetingData token in existingMeetings)
                                {
                                    DateTime sTime = token.GetTimeStart();
                                    DateTime eTime = token.GetTimeEnd();
                                    if (sTime.Day == timeLog.timeMeeting.Day)
                                    {
                                        DateTime newTimeStart = new DateTime(timeLog.timeMeeting.Year, timeLog.timeMeeting.Month, timeLog.timeMeeting.Day, timeLog.timeMeeting.Hour, timeLog.timeMeeting.Minute, 0);
                                        DateTime newTimeEnd = newTimeStart + new TimeSpan(0, timeMinutes, 0);
                                        if (IntervalsIntersect(sTime, eTime, newTimeStart, newTimeEnd))                                                
                                        {
                                            string packedAssistants = string.Join(",", finalAssistants.Select(x => x));
                                            List<string> mems = token.GetMembers();                                            
                                            bool isPresentInConcurrentMeeting = false;
                                            string humanIntersect = "";
                                            foreach (string mem in mems)
                                            {
                                                if (finalAssistants.Contains(mem))
                                                {
                                                    isPresentInConcurrentMeeting = true;
                                                    humanIntersect = mem;
                                                }
                                            }
                                            if (isPresentInConcurrentMeeting)
                                            {
                                                timeLog.timeMeeting = eTime;
                                            }
                                        }
                                    }
                                }
                            }

                            CorrectTime(timeLog, timeMinutes);

                            DateTime dateTimeStart = new DateTime(timeLog.timeMeeting.Year, timeLog.timeMeeting.Month, timeLog.timeMeeting.Day, timeLog.timeMeeting.Hour, timeLog.timeMeeting.Minute, 0);
                            DateTime dateTimeEnd = dateTimeStart + new TimeSpan(0, timeMinutes, 0);
                            timeLog.timeMeeting = dateTimeEnd;

                            SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerAddMeeting, null, task.UID, meetingToCreate.name, meetingToCreate.description, new List<DocumentData>(), dateTimeStart, dateTimeEnd, WorkDayData.Instance.CurrentProject.ProjectInfoSelected, finalAssistants.ToArray(), false, true, true, true, false);
                        }
                    }
                }
            }

            if ((_outputEvent != null) && (_outputEvent.Length > 0)) SystemEventController.Instance.DispatchSystemEvent(_outputEvent);            
            SystemEventController.Instance.DispatchSystemEvent(EventAICommandMeetingsDefinitionCompleted);

            _project = null;
            _uidFeatures = null;
            _meetingsDefined = null;
            _uidFeatureToProcess = null;
            _promptBuilder = null;
        }

        private void CorrectTime(TimeMeetingLog timeLog, int timeMinutes)
        {
            // CHANGE HOUR IF MEETING INSIDE LUNCH HOUR
            DateTime dateTimeCheckLunchHour = timeLog.timeMeeting;
            if ((dateTimeCheckLunchHour >= timeLog.lunchHour) && (dateTimeCheckLunchHour <= timeLog.endLunchHour))
            {
                timeLog.timeMeeting = new DateTime(timeLog.timeMeeting.Year, timeLog.timeMeeting.Month, timeLog.timeMeeting.Day, WorkDayData.Instance.CurrentProject.LunchHour, 0, 0) + new TimeSpan(0, ClockController.TotalLunchTime + 5, 0);
            }
            else
            {
                dateTimeCheckLunchHour = timeLog.timeMeeting + new TimeSpan(0, (2 * timeMinutes) / 3, 0);
                if ((dateTimeCheckLunchHour >= timeLog.lunchHour) && (dateTimeCheckLunchHour <= timeLog.endLunchHour))
                {
                    timeLog.timeMeeting = new DateTime(timeLog.timeMeeting.Year, timeLog.timeMeeting.Month, timeLog.timeMeeting.Day, WorkDayData.Instance.CurrentProject.LunchHour, 0, 0) + new TimeSpan(0, ClockController.TotalLunchTime + 5, 0);
                }
            }

            // CHANGE DAY IF MEETING INSIDE HOUR TO LEAVE
            DateTime dateTimeCheckEndDay = timeLog.timeMeeting + new TimeSpan(0, timeMinutes + 20, 0);
            if ((dateTimeCheckEndDay > timeLog.endingHour) || (dateTimeCheckEndDay.Day != timeLog.startingDay.Day))
            {
                do
                {
                    timeLog.startingDay = timeLog.startingDay + new TimeSpan(24, 0, 0);
                    timeLog.timeMeeting = new DateTime(timeLog.startingDay.Year, timeLog.startingDay.Month, timeLog.startingDay.Day, WorkDayData.Instance.CurrentProject.StartingHour, 30, 0);
                    timeLog.lunchHour = new DateTime(timeLog.startingDay.Year, timeLog.startingDay.Month, timeLog.startingDay.Day, WorkDayData.Instance.CurrentProject.LunchHour, 0, 0);
                    timeLog.endLunchHour = new DateTime(timeLog.startingDay.Year, timeLog.startingDay.Month, timeLog.startingDay.Day, WorkDayData.Instance.CurrentProject.LunchHour, 0, 0) + new TimeSpan(0, ClockController.TotalLunchTime, 0);
                    timeLog.endingHour = new DateTime(timeLog.startingDay.Year, timeLog.startingDay.Month, timeLog.startingDay.Day, WorkDayData.Instance.CurrentProject.EndingHour, 0, 0);
                } while (WorkDayData.Instance.CurrentProject.IsFreeDay(timeLog.startingDay));
            }

            // CHANGE HOUR IF ENTERING OFFICE
            DateTime dateTimeInitialHour = new DateTime(timeLog.startingDay.Year, timeLog.startingDay.Month, timeLog.startingDay.Day, WorkDayData.Instance.CurrentProject.StartingHour, 30, 0);
            if ((timeLog.timeMeeting > timeLog.startingDay) && (timeLog.timeMeeting < dateTimeInitialHour))
            {
                timeLog.timeMeeting = new DateTime(timeLog.startingDay.Year, timeLog.startingDay.Month, timeLog.startingDay.Day, WorkDayData.Instance.CurrentProject.StartingHour, 30, 0);
            }
        }

        private void AskMeetingsLLM(string prompt)
        {
            if (_confirmation)
            {
                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("ai.processing.now.please.wait"));
            }
            WorkDayData.Instance.AskWorkDayAIMeetingsDefinition(prompt, true, EventAICommandMeetingsDefinitionResponse);
        }

        private bool BuildPromptMeetingsDefinition()
        {
            if (_uidFeatureToProcess.Count == 0)
            {
                return false;
            }
            _currentUIDFeature = _uidFeatureToProcess[0];
            _uidFeatureToProcess.RemoveAt(0);

            string question = PromptController.Instance.GetText("ai.command.generation.meetings.definition.for.tasks",
                                                                "<" + PromptController.Instance.GetText("xml.tag.tasks") + ">",
                                                                "<" + PromptController.Instance.GetText("xml.tag.employees") + ">",
                                                                "<" + PromptController.Instance.GetText("xml.tag.feature") + ">",
                                                                "<" + PromptController.Instance.GetText("xml.tag.project") + ">");

            switch (PromptController.Instance.CodeLanguage)
            {
                case PromptController.CodeLanguageEnglish:
                    question += JSONDataEnglish.meetingForTaskJsonString;
                    break;
                case PromptController.CodeLanguageSpanish:
                    question += JSONDataSpanish.meetingForTaskJsonString;
                    break;
                case PromptController.CodeLanguageGerman:
                    question += JSONDataGerman.meetingForTaskJsonString;
                    break;
                case PromptController.CodeLanguageFrench:
                    question += JSONDataFrench.meetingForTaskJsonString;
                    break;
                case PromptController.CodeLanguageItalian:
                    question += JSONDataItalian.meetingForTaskJsonString;
                    break;
                case PromptController.CodeLanguageRussian:
                    question += JSONDataRussian.meetingForTaskJsonString;
                    break;
                case PromptController.CodeLanguageCatalan:
                    question += JSONDataCatalan.meetingForTaskJsonString;
                    break;
            }

            _promptBuilder = new PromptBuilder(question);
            _promptBuilder.SetPromptColor(_project.GetColor());
            _promptBuilder.SetProjectFeedback(_project.Name + " : " + LanguageController.Instance.GetText("text.generate.meetings.for.tasks"));

            // EMPLOYEES "xml.tag.tasks"
            string totalTasks = "\n";
            List<TaskItemData> tasks = GetListTaskByFeatureUID(_currentUIDFeature);
            foreach (TaskItemData task in tasks)
            {
                totalTasks += "<" + PromptController.Instance.GetText("xml.tag.task");
                totalTasks += " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + task.Name + "\"";
                totalTasks += " " + PromptController.Instance.GetText("xml.tag.description") + "=\"" + task.Description + "\"";
                string packMembers = task.PackHumanMembers();
                totalTasks += " " + PromptController.Instance.GetText("xml.tag.assigned") + "=\"" + packMembers + "\"";
                totalTasks += "/>";
                totalTasks += "\n";
            }
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.tasks") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.tasks") + ">",
                                                "</" + PromptController.Instance.GetText("xml.tag.tasks") + ">"),
                                                totalTasks);

            // EMPLOYEES "xml.tag.employees"
            List<GroupInfoData> groups = WorkDayData.Instance.CurrentProject.GetGroups();
            List<string> employees = new List<string>();
            foreach (GroupInfoData group in groups)
            {
                List<string> members = group.GetMembers();
                foreach (string member in members)
                {
                    if (!employees.Contains(member))
                    {
                        employees.Add(member);
                    }
                }
            }
            string participantsContent = "\n";
            foreach (string assistant in employees)
            {
                WorldItemData humanData = WorkDayData.Instance.CurrentProject.GetItemByName(assistant);

                string category = PromptController.Instance.GetText("xml.tag.category.normal");
                if (humanData.IsLead)
                {
                    category = PromptController.Instance.GetText("xml.tag.category.lead");
                }
                else
                {
                    if (humanData.IsSenior)
                    {
                        category = PromptController.Instance.GetText("xml.tag.category.senior");
                    }
                }

                GroupInfoData groupOfAssistant = WorkDayData.Instance.CurrentProject.GetGroupOfMember(assistant);
                string humanDescription = humanData.GetSkills();
                if (groupOfAssistant != null)
                {
                    participantsContent += "<" + PromptController.Instance.GetText("xml.tag.employee")
                                + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + assistant + "\""
                                + " " + PromptController.Instance.GetText("xml.tag.group") + "=\"" + groupOfAssistant.Name + "\""
                                + " " + PromptController.Instance.GetText("xml.tag.category") + "=\"" + category + "\""
                                + ">";
                    participantsContent += humanDescription;
                    participantsContent += "</" + PromptController.Instance.GetText("xml.tag.participant") + ">";
                    participantsContent += "\n";
                }
                else
                {
                    participantsContent += "<" + PromptController.Instance.GetText("xml.tag.employee")
                                + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + assistant + "\""
                                + " " + PromptController.Instance.GetText("xml.tag.category") + "=\"" + category + "\""
                                + ">";
                    participantsContent += humanDescription;
                    participantsContent += "</" + PromptController.Instance.GetText("xml.tag.employee") + ">";
                    participantsContent += "\n";
                }
            }
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.employees") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.employees") + ">",
                                                "</" + PromptController.Instance.GetText("xml.tag.employees") + ">"),
                                                participantsContent);

            // FEATURE "xml.tag.feature"
            DocumentData docFeature = WorkDayData.Instance.CurrentProject.GetDocumentByFeatureID(_currentUIDFeature);
            if (docFeature != null)
            {
                string summaryFeature = docFeature.Data.GetHTML();
                if ((docFeature.Summary!=null) && (docFeature.Summary.Length > 0))
                {
                    summaryFeature = docFeature.Summary;
                }
                _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.feature") + ">",
                                        "<" + PromptController.Instance.GetText("xml.tag.feature") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + docFeature.Name + "\">",
                                        "</" + PromptController.Instance.GetText("xml.tag.feature") + ">"),
                                        summaryFeature);
            }
            else
            {
                _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.feature") + ">",
                                        "<" + PromptController.Instance.GetText("xml.tag.feature") + ">",
                                        "</" + PromptController.Instance.GetText("xml.tag.feature") + ">"),
                                        "");
            }

            // PROJECT "xml.tag.project"
            ProjectInfoData project = WorkDayData.Instance.CurrentProject.GetProject(WorkDayData.Instance.CurrentProject.ProjectInfoSelected);
            if (project != null)
            {
                _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                                                    "<" + PromptController.Instance.GetText("xml.tag.project") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + project.Name + "\">",
                                                    "</" + PromptController.Instance.GetText("xml.tag.project") + ">"),
                                                    project.Description);
            }
            else
            {
                _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                                                    "<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                                                    "</" + PromptController.Instance.GetText("xml.tag.project") + ">"),
                                                    "");
            }

            return true;
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventAICommandMeetingsDefinitionRequest))
            {
                AskMeetingsLLM((string)parameters[0]);
            }
            if (nameEvent.Equals(EventAICommandMeetingsDefinitionResponse))
            {
                if (_confirmation)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                }
                if ((bool)parameters[0])
                {
                    string jsonString = (string)parameters[1];
                    string meetingsDefinitionJsonString = "{\"meetings\":" + jsonString + "}";
#if UNITY_EDITOR
                    Debug.Log("MEETINGS DEFINITION RECEIVED=" + meetingsDefinitionJsonString);
#endif

                    if (!JSONDataFormatValidator.ValidateJsonList<MeetingForTaskJSON>("{\"items\":" + jsonString + "}"))
                    {
                        if (!_confirmation)
                        {
                            _iterationsToCancel--;
                            if (_iterationsToCancel <= 0)
                            {
                                _isCompleted = true;
                            }
                            else
                            {
                                _confirmation = true;  // IF THE PROMPT FAILS AGAIN, THEN WE WILL ASK THE USER SO HE HAS THE OPTION TO CHOOSE A BETTER LLM MODEL
                                SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
                                SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, false);                                

                                AskMeetingsLLM(_promptBuilder.BuildPrompt());
                            }
                        }
                        else
                        {
                            string title = LanguageController.Instance.GetText("ai.title.error.prompt");
                            ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLongInput, null, title, "", EventAICommandMeetingsDefinitionPromptRetry);
                            UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, jsonString);
                            if (GameObject.FindAnyObjectByType<CodeEditor>() != null)
                            {
                                GameObject.FindAnyObjectByType<CodeEditor>().Refresh();
                            }
                        }
                        return;
                    }

                    MeetingForTaskListJSON meetingsDefined = JsonUtility.FromJson<MeetingForTaskListJSON>(meetingsDefinitionJsonString);

                    _meetingsDefined.Add(_currentUIDFeature, meetingsDefined);
#if UNITY_EDITOR
                    Debug.Log("MEETINGS COUNT=" + meetingsDefined.meetings.Count);
#endif
            
                    if (BuildPromptMeetingsDefinition())
                    {
                        if (_confirmation)
                        {
                            string title = LanguageController.Instance.GetText("ai.title.generate.meetings.for.tasks");
                            ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandMeetingsDefinitionPromptConfirmation);
                            UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewInitValues, _promptBuilder);
                            UIEventController.Instance.DelayUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRefreshInput, 0.1f);
                        }
                        else
                        {
                            AskMeetingsLLM(_promptBuilder.BuildPrompt());
                        }
                    }
                    else
                    {
                        _isCompleted = true;
                    }                    
                }
                else
                {
                    if (_confirmation)
                    {
                        string title = LanguageController.Instance.GetText("text.error");
                        string description = LanguageController.Instance.GetText("ai.title.error.prompt");
                        ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, title, description, EventAICommandMeetingsDefinitionPromptRetry);
                        _isCompleted = false;
                    }
                    else
                    {
                        _iterationsToCancel--;
                        if (_iterationsToCancel <= 0)
                        {
                            _isCompleted = true;
                        }
                        else
                        {
                            _confirmation = true;  // IF THE PROMPT FAILS AGAIN, THEN WE WILL ASK THE USER SO HE HAS THE OPTION TO CHOOSE A BETTER LLM MODEL
                            SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
                            SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, false);

                            AskMeetingsLLM(_promptBuilder.BuildPrompt());
                        }
                    }
                }
            }
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventAICommandMeetingsDefinitionPromptConfirmation))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
#if !SHORTCUT_LLM_SERVER
                    WorkDayData.Instance.AddTimeSession();
#endif
                    SystemEventController.Instance.DelaySystemEvent(EventAICommandMeetingsDefinitionRequest, 0.2f, (string)parameters[2]);
                }
                else
                {
                    _isCompleted = true;
                }
            }
            if (nameEvent.Equals(EventAICommandMeetingsDefinitionPromptRetry))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
                    string title = LanguageController.Instance.GetText("ai.title.generate.meetings.for.tasks");
                    ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandMeetingsDefinitionPromptConfirmation);
                    UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewInitValues, _promptBuilder);
                    UIEventController.Instance.DelayUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRefreshInput, 0.1f);
                }
                else
                {
                    _isCompleted = true;
                }
            }
        }

        public void Run()
        {

        }
    }
}