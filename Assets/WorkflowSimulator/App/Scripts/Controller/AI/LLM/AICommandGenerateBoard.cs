using UnityEngine;
using yourvrexperience.Utils;
using System;
using static yourvrexperience.WorkDay.ApplicationController;
using System.Collections.Generic;
using InGameCodeEditor;
using static yourvrexperience.WorkDay.TaskItemData;
using static yourvrexperience.WorkDay.ScreenCalendarView;

namespace yourvrexperience.WorkDay
{
    public class AICommandGenerateBoard : IAICommand
    {
        private const int TOTAL_FEATURES_SPRINT = 3;
        private const int TOTAL_WEEKS_SPRINT = 2;

        ////////////////////////////////////////////////////////////////////////////
        /// 1. GENERATE FEATURES
        ////////////////////////////////////////////////////////////////////////////
        private const string EventAICommandGenerateBoardThemeRequest = "EventAICommandGenerateBoardThemeRequest";
        private const string EventAICommandGenerateBoardThemeResponse = "EventAICommandGenerateBoardThemeResponse";
        private const string EventAICommandGenerateBoardThemePromptConfirmation = "EventAICommandGenerateBoardThemePromptConfirmation";
        private const string EventAICommandGenerateBoardThemePromptRetry = "EventAICommandGenerateBoardThemePromptRetry";

        ////////////////////////////////////////////////////////////////////////////
        /// 2. DEFINE SPRINT BOARD
        ////////////////////////////////////////////////////////////////////////////
        private const string EventAICommandSprintBoardDefinitionRequest = "EventAICommandSprintBoardDefinitionRequest";
        private const string EventAICommandSprintBoardDefinitionResponse = "EventAICommandSprintBoardDefinitionResponse";
        private const string EventAICommandSprintBoardDefinitionPromptConfirmation = "EventAICommandSprintBoardDefinitionPromptConfirmation";
        private const string EventAICommandSprintBoardDefinitionPromptRetry = "EventAICommandSprintBoardDefinitionPromptRetry";

        ////////////////////////////////////////////////////////////////////////////
        /// 3. GENERATE TASKS
        ////////////////////////////////////////////////////////////////////////////
        private const string EventAICommandGenerateBoardTasksRequest = "EventAICommandGenerateBoardTasksRequest";
        private const string EventAICommandGenerateBoardTasksResponse = "EventAICommandGenerateBoardTasksResponse";
        private const string EventAICommandGenerateBoardTasksPromptConfirmation = "EventAICommandGenerateBoardTasksPromptConfirmation";
        private const string EventAICommandGenerateBoardTasksPromptRetry = "EventAICommandGenerateBoardTasksPromptRetry";

        enum TypeExperience { Normal = 0, Senior = 1, Lead }

        private bool _isCompleted = false;
        private string _outputEvent;
        private PromptBuilder _promptBuilder;
        private Color _projectColor = Color.white;
        private string _projectName = "";
        private string _projectInfoDescription = "";
        private bool _confirmation;
        private FeatureDescriptionListJSON _featuresDescriptions;
        private SprintBoardDefinitionJSON _sprintBoardDefinition;
        private Dictionary<string, List<string>> _membersForFeature;
        private int _indexFeatureToProcess = 0;
        private List<TasksSprintListJSON> _tasksGenerated = new List<TasksSprintListJSON>();

        private Dictionary<string, int> _idLinkedFeatureID = new Dictionary<string, int>();
        private string _currentNameFeature = "";

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

            ProjectInfoData project = WorkDayData.Instance.CurrentProject.GetProject(WorkDayData.Instance.CurrentProject.ProjectInfoSelected);
            if (project != null)
            {
                _projectColor = project.GetColor();
                ApplicationController.Instance.LastProjectColor = project.GetColor();
                _projectName = project.Name;
                ApplicationController.Instance.LastProjectFeedback = _projectName;
                _projectInfoDescription = project.Description;
            }

            BuildPromptBoardTheme();
            ApplicationController.Instance.TimeHUD.BlockTimeInteraction = true;
            if (_confirmation)
            {
                string title = LanguageController.Instance.GetText("ai.title.generate.board.theme");
                SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
                ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandGenerateBoardThemePromptConfirmation);
                UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewInitValues, _promptBuilder);
                UIEventController.Instance.DelayUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRefreshInput, 0.1f);
                SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, false);                
            }
            else
            {
                AskThemeLLM(_promptBuilder.BuildPrompt());
            }
        }

        public void Destroy()
        {
            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
            if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

            _featuresDescriptions = null;
            _sprintBoardDefinition = null;
            _membersForFeature = null;
            _tasksGenerated = null;

            _promptBuilder = null;

            ApplicationController.Instance.TimeHUD.BlockTimeInteraction = false;
        }

        private void AskThemeLLM(string prompt)
        {
            if (_confirmation)
            {
                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("ai.processing.now.please.wait"));
            }
            WorkDayData.Instance.AskWorkDayAIFeaturesDescription(prompt, true, EventAICommandGenerateBoardThemeResponse);
        }

        private void AskSprintBoardDefinitionLLM(string prompt)
        {
            if (_confirmation)
            {
                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("ai.processing.now.please.wait"));
            }
            WorkDayData.Instance.AskWorkDayAISprintBoardDefinition(prompt, true, EventAICommandSprintBoardDefinitionResponse);
        }

        private void AskGenerateTasksLLM(string prompt)
        {
            if (_confirmation)
            {
                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("ai.processing.now.please.wait"));
            }
            WorkDayData.Instance.AskWorkDayAICreateTasks(prompt, true, EventAICommandGenerateBoardTasksResponse);
        }

        private string GetTaskClosestName(string nameTask, int percentage)
        {
            foreach (TasksSprintListJSON tasks in _tasksGenerated)
            {
                foreach (TaskSprintJSON task in tasks.tasks)
                {
                    if (StringSimilarity.CalculateSimilarityPercentage(task.name, nameTask) > percentage)
                    {
                        return task.name;
                    }
                }
            }
            return "";
        }

        private void ProcessFinalResults()
        {
            if ((_featuresDescriptions != null) && (_sprintBoardDefinition != null))
            {
                if (_tasksGenerated.Count > 0)
                {
                    string boardName = _sprintBoardDefinition.name;

                    SystemEventController.Instance.DispatchSystemEvent(TasksController.EventTasksControllerAddNewBoard, boardName, _sprintBoardDefinition.description);

                    
                    foreach (TasksSprintListJSON tasks in _tasksGenerated)
                    {
                        int indexFeature = -1;
                        if (_idLinkedFeatureID.TryGetValue(tasks.name, out indexFeature))
                        {
                            foreach (TaskSprintJSON task in tasks.tasks)
                            {
                                string[] members = task.employees.Split(",");
                                for (int i = 0; i < members.Length; i++) members[i] = members[i].Trim();
                                SystemEventController.Instance.DispatchSystemEvent(TasksController.EventTasksControllerAddTask, null, boardName, task.name, task.data, null, task.time, (int)TaskStates.TODO, -1, indexFeature, members);
                            }
                        }
                    }

                    // FIX DEPENDENCY NAMES
                    foreach (TasksSprintListJSON tasks in _tasksGenerated)
                    {
                        foreach (TaskSprintJSON task in tasks.tasks)
                        {
                            var (taskItemData, boardNM) = WorkDayData.Instance.CurrentProject.GetTaskItemDataName(task.name);
                            if (taskItemData != null)
                            {
                                if ((task.dependency != null) && (task.dependency.Length > 0))
                                {
                                    task.dependency = GetTaskClosestName(task.dependency, 80);
                                }
                            }
                        }
                    }

                    // ESTABLISH THE FINAL LINK
                    foreach (TasksSprintListJSON tasks in _tasksGenerated)
                    {
                        foreach (TaskSprintJSON task in tasks.tasks)
                        {
                            var (taskItemData, boardNM) = WorkDayData.Instance.CurrentProject.GetTaskItemDataName(task.name);
                            if (taskItemData != null)
                            {
                                var (taskLinkedItemData, boardLinkedNM) = WorkDayData.Instance.CurrentProject.GetTaskItemDataName(task.dependency);
                                if (taskLinkedItemData != null)
                                {
                                    taskItemData.Linked = taskLinkedItemData.UID;
                                }
                            }
                        }
                    }

                    // LUNCH MEETINGS
                    DateTime currTime = WorkDayData.Instance.CurrentProject.GetCurrentTime();
                    List<string> membersLunch = new List<string>();
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
                            membersLunch.Add(group.Name);
                        }
                    }

                    for (int i = 0; i < 14; i++)
                    {
                        if (!WorkDayData.Instance.CurrentProject.IsFreeDay(currTime))
                        {
                            string nameMeetingLunch = PromptController.Instance.GetNameLunch(currTime);                            
                            MeetingData existingMeeting = WorkDayData.Instance.CurrentProject.GetMeeting(nameMeetingLunch, currTime);
                            if (existingMeeting == null)
                            {
                                string descriptionMeetingLunch = LanguageController.Instance.GetText("text.lunch.time.description");
                                DateTime dateTimeStart = new DateTime(currTime.Year, currTime.Month, currTime.Day, WorkDayData.Instance.CurrentProject.LunchHour, 0, 0);
                                DateTime dateTimeEnd = dateTimeStart + new TimeSpan(0, ClockController.TotalLunchTime, 0);
                                SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerAddMeeting, null, -1, nameMeetingLunch, descriptionMeetingLunch, new List<DocumentData>(), dateTimeStart, dateTimeEnd, -1, membersLunch.ToArray(), false, true, false, true, false);
                            }
                        }
                        currTime += new TimeSpan(24, 0, 0);
                    }

                    SaveGlobalFeaturesDefinitions();
                }
            }
        }

        private int GetTotalNumberForGroups(string groupName, TypeExperience experience)
        {
            int counter = 0;
            List<GroupInfoData> groups = WorkDayData.Instance.CurrentProject.GetGroups();
            foreach (GroupInfoData group in groups)
            {
                if (group.Name.Equals(groupName))
                {
                    List<string> members = group.GetMembers();
                    foreach (string member in members)
                    {
                        WorldItemData memberData = WorkDayData.Instance.CurrentProject.GetItemByName(member);
                        switch (experience)
                        {
                            case TypeExperience.Normal:
                                if (!memberData.IsLead && !memberData.IsSenior)
                                {
                                    counter++;
                                }
                                break;
                            case TypeExperience.Senior:
                                if (!memberData.IsLead && memberData.IsSenior)
                                {
                                    counter++;
                                }
                                break;
                            case TypeExperience.Lead:
                                if (memberData.IsLead)
                                {
                                    counter++;
                                }
                                break;
                        }
                    }
                }
            }
            return counter;
        }

        private string GetAnyMemberForGroup(string groupName, TypeExperience experience, List<string> exceptions)
        {
            List<GroupInfoData> groups = WorkDayData.Instance.CurrentProject.GetGroups();
            foreach (GroupInfoData group in groups)
            {
                if (group.Name.Equals(groupName))
                {
                    List<string> members = group.GetMembers();
                    foreach (string member in members)
                    {
                        WorldItemData memberData = WorkDayData.Instance.CurrentProject.GetItemByName(member);
                        switch (experience)
                        {
                            case TypeExperience.Normal:
                                if (!memberData.IsLead && !memberData.IsSenior)
                                {
                                    if (!exceptions.Contains(member))
                                    {
                                        return member;
                                    }
                                }
                                break;
                            case TypeExperience.Senior:
                                if (!memberData.IsLead && memberData.IsSenior)
                                {
                                    if (!exceptions.Contains(member))
                                    {
                                        return member;
                                    }
                                }
                                break;
                            case TypeExperience.Lead:
                                if (memberData.IsLead)
                                {
                                    if (!exceptions.Contains(member))
                                    {
                                        return member;
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            return null;
        }

        private void ProposeMembersForFeature(List<string> proposedMembers, Dictionary<string, List<string>> reservedMembers)
        {
            List<string> usedMembers = new List<string>();
            foreach (KeyValuePair<string, List<string>> feature in reservedMembers)
            {
                if (feature.Value != null)
                {
                    if (feature.Value.Count > 0)
                    {
                        foreach(string member in feature.Value)
                        {
                            if (!usedMembers.Contains(member))
                            {
                                usedMembers.Add(member);
                            }
                        }
                    }
                }
            }

            List<GroupInfoData> groups = WorkDayData.Instance.CurrentProject.GetGroups();
            Dictionary<string, (List<string>, List<string>, List<string>)> availableGroups = new Dictionary<string, (List<string>, List<string>, List<string>)>();
            foreach (GroupInfoData group in groups)
            {
                List<string> members = group.GetMembers();
                List<string> employeesNormal = new List<string>();
                List<string> employeesSenior = new List<string>();
                List<string> employeesLead = new List<string>();
                foreach (string member in members)
                {
                    WorldItemData memberData = WorkDayData.Instance.CurrentProject.GetItemByName(member);
                    if (memberData.IsLead)
                    {
                        if (!usedMembers.Contains(member))
                        {
                            employeesLead.Add(member);
                        }                        
                    }
                    else
                    {
                        if (memberData.IsSenior)
                        {
                            if (!usedMembers.Contains(member))
                            {
                                employeesSenior.Add(member);
                            }
                        }
                        else
                        {
                            if (!usedMembers.Contains(member))
                            {
                                employeesNormal.Add(member);
                            }
                        }
                    }
                }
                availableGroups.Add(group.Name, (employeesLead, employeesSenior, employeesNormal));
            }

            // ADD A LEAD FROM EACH GROUP
            foreach (KeyValuePair<string, (List<string>, List<string>, List<string>)> item in availableGroups)
            {
                string groupName = item.Key;
                List<string> leadsAvailable = item.Value.Item1;
                List<string> seniorsAvailable = item.Value.Item2;
                List<string> normalsAvailable = item.Value.Item3;

                int totalLeadsForGroup = GetTotalNumberForGroups(groupName, TypeExperience.Lead);
                int totalSeniorsForGroup = GetTotalNumberForGroups(groupName, TypeExperience.Senior);
                int totalNormalsForGroup = GetTotalNumberForGroups(groupName, TypeExperience.Normal);

                // LEAD FOR GROUP X
                if (totalLeadsForGroup > 0)
                {
                    if (leadsAvailable.Count > 0)
                    {
                        proposedMembers.Add(leadsAvailable[0]);
                    }
                    else
                    {
                        string memberProposed = GetAnyMemberForGroup(groupName, TypeExperience.Lead, proposedMembers);
                        if (memberProposed != null)
                        {
                            proposedMembers.Add(memberProposed);
                        }
                    }
                }

                // SENIORS FOR GROUP X
                if (totalSeniorsForGroup > 0)
                {
                    int totalSeniors = 1;
                    if (totalSeniorsForGroup > 1)
                    {
                        if (UnityEngine.Random.Range(0, 100) < 25) totalSeniors = 2;
                    }

                    for (int i = 0; i < totalSeniors; i++)
                    {
                        if (seniorsAvailable.Count > 0)
                        {
                            proposedMembers.Add(seniorsAvailable[0]);
                            seniorsAvailable.RemoveAt(0);
                        }
                        else
                        {
                            string memberProposed = GetAnyMemberForGroup(groupName, TypeExperience.Senior, proposedMembers);
                            if (memberProposed != null)
                            {
                                proposedMembers.Add(memberProposed);
                            }
                        }
                    }
                }

                // NORMALS FOR GROUP X
                if (totalNormalsForGroup > 0)
                {
                    int totalNormals = 1;
                    if (totalNormalsForGroup > 1)
                    {
                        if (UnityEngine.Random.Range(0, 100) < 50) totalNormals = 2;
                    }

                    for (int i = 0; i < totalNormals; i++)
                    {
                        if (normalsAvailable.Count > 0)
                        {
                            proposedMembers.Add(normalsAvailable[0]);
                            normalsAvailable.RemoveAt(0);
                        }
                        else
                        {
                            string memberProposed = GetAnyMemberForGroup(groupName, TypeExperience.Normal, proposedMembers);
                            if (memberProposed != null)
                            {
                                proposedMembers.Add(memberProposed);
                            }
                        }
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        /// 1. GENERATE FEATURES
        ////////////////////////////////////////////////////////////////////////////
        private void BuildPromptBoardTheme()
        {
            string nameWeeks = PromptController.Instance.GetText("xml.tag.weeks");
            if (TOTAL_WEEKS_SPRINT == 1)
            {
                nameWeeks = PromptController.Instance.GetText("xml.tag.week");
            }
            string nameFeatures = PromptController.Instance.GetText("xml.tag.features");
            if (TOTAL_FEATURES_SPRINT == 1)
            {
                nameFeatures = PromptController.Instance.GetText("xml.tag.feature");
            }
            string question = PromptController.Instance.GetText("ai.command.generation.board.theme",
                                                                TOTAL_FEATURES_SPRINT, nameFeatures,
                                                                TOTAL_WEEKS_SPRINT, nameWeeks,
                                                                "<" + PromptController.Instance.GetText("xml.tag.employees") + ">",
                                                                "<" + PromptController.Instance.GetText("xml.tag.project") + ">");

            switch (PromptController.Instance.CodeLanguage)
            {
                case PromptController.CodeLanguageEnglish:
                    question += JSONDataEnglish.featureDescriptionJsonString;
                    break;
                case PromptController.CodeLanguageSpanish:
                    question += JSONDataSpanish.featureDescriptionJsonString;
                    break;
                case PromptController.CodeLanguageGerman:
                    question += JSONDataGerman.featureDescriptionJsonString;
                    break;
                case PromptController.CodeLanguageFrench:
                    question += JSONDataFrench.featureDescriptionJsonString;
                    break;
                case PromptController.CodeLanguageItalian:
                    question += JSONDataItalian.featureDescriptionJsonString;
                    break;
                case PromptController.CodeLanguageRussian:
                    question += JSONDataRussian.featureDescriptionJsonString;
                    break;
                case PromptController.CodeLanguageCatalan:
                    question += JSONDataCatalan.featureDescriptionJsonString;
                    break;
            }

            _promptBuilder = new PromptBuilder(question);
            _promptBuilder.SetPromptColor(_projectColor);
            _promptBuilder.SetProjectFeedback(_projectName + " : " + LanguageController.Instance.GetText("text.request.feature.definition"));

            List<GroupInfoData> groups = WorkDayData.Instance.CurrentProject.GetGroups();
            List<string> employees = new List<string>();
            foreach (GroupInfoData group in groups)
            {
                List<string> members = group.GetMembers();
                foreach(string member in members)
                {
                    if (!employees.Contains(member))
                    {
                        employees.Add(member);
                    }
                }
            }
            // PARTICIPANTS "xml.tag.participants"
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

            if (_projectInfoDescription != null)
            {
                _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                                                    "<" + PromptController.Instance.GetText("xml.tag.project") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + _projectName + "\">",
                                                    "</" + PromptController.Instance.GetText("xml.tag.project") + ">"),
                                                    _projectInfoDescription);
            }
            else
            {
                _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                                                    "<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                                                    "</" + PromptController.Instance.GetText("xml.tag.project") + ">"),
                                                    "");
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        /// 2. DEFINE SPRINT BOARD
        ////////////////////////////////////////////////////////////////////////////
        private void BuildPromptSprintBoardDefinition()
        {
            string nameFeatures = PromptController.Instance.GetText("xml.tag.features");
            if (TOTAL_FEATURES_SPRINT == 1)
            {
                nameFeatures = PromptController.Instance.GetText("xml.tag.feature");
            }
            string question = PromptController.Instance.GetText("ai.command.generation.sprint.board.naming",
                                                                nameFeatures,
                                                                "<" + PromptController.Instance.GetText("xml.tag.features") + ">",
                                                                "<" + PromptController.Instance.GetText("xml.tag.project") + ">");

            switch (PromptController.Instance.CodeLanguage)
            {
                case PromptController.CodeLanguageEnglish:
                    question += JSONDataEnglish.documentTextSprintBoarDefinition;
                    break;
                case PromptController.CodeLanguageSpanish:
                    question += JSONDataSpanish.documentTextSprintBoarDefinition;
                    break;
                case PromptController.CodeLanguageGerman:
                    question += JSONDataGerman.documentTextSprintBoarDefinition;
                    break;
                case PromptController.CodeLanguageFrench:
                    question += JSONDataFrench.documentTextSprintBoarDefinition;
                    break;
                case PromptController.CodeLanguageItalian:
                    question += JSONDataItalian.documentTextSprintBoarDefinition;
                    break;
                case PromptController.CodeLanguageRussian:
                    question += JSONDataRussian.documentTextSprintBoarDefinition;
                    break;
                case PromptController.CodeLanguageCatalan:
                    question += JSONDataCatalan.documentTextSprintBoarDefinition;
                    break;
            }

            _promptBuilder = new PromptBuilder(question);
            _promptBuilder.SetPromptColor(_projectColor);
            _promptBuilder.SetProjectFeedback(_projectName + " : " + LanguageController.Instance.GetText("text.request.sprint.board.naming"));

            // FEATURES "xml.tag.features"
            string contentFeatures = "\n";
            foreach(FeatureDescriptionJSON feature in _featuresDescriptions.features)
            {
                contentFeatures += "<" + PromptController.Instance.GetText("xml.tag.feature") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + feature.name + "\">";
                contentFeatures += feature.description;
                contentFeatures += "</" + PromptController.Instance.GetText("xml.tag.feature") + ">";
                contentFeatures += "\n";
            }
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.features") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.features") + ">",
                                                "</" + PromptController.Instance.GetText("xml.tag.features") + ">"),
                                                contentFeatures);

            // PROJECT "xml.tag.project"
            if (_projectInfoDescription != null)
            {
                _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                                                    "<" + PromptController.Instance.GetText("xml.tag.project") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + _projectName + "\">",
                                                    "</" + PromptController.Instance.GetText("xml.tag.project") + ">"),
                                                    _projectInfoDescription);
            }
            else
            {
                _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                                                    "<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                                                    "</" + PromptController.Instance.GetText("xml.tag.project") + ">"),
                                                    "");
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        /// 3. GENERATE TASKS
        ////////////////////////////////////////////////////////////////////////////
        private void BuildPromptListTasks()
        {
            string nameWeeks = PromptController.Instance.GetText("xml.tag.weeks");
            if (TOTAL_WEEKS_SPRINT == 1)
            {
                nameWeeks = PromptController.Instance.GetText("xml.tag.week");
            }
            string question = PromptController.Instance.GetText("ai.command.generation.create.list.tasks",
                TOTAL_WEEKS_SPRINT, nameWeeks,
                "<" + PromptController.Instance.GetText("xml.tag.feature") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.employees") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.project") + ">");

            switch (PromptController.Instance.CodeLanguage)
            {
                case PromptController.CodeLanguageEnglish:
                    question += JSONDataEnglish.definitionTasksSprintJsonString;
                    break;
                case PromptController.CodeLanguageSpanish:
                    question += JSONDataSpanish.definitionTasksSprintJsonString;
                    break;
                case PromptController.CodeLanguageGerman:
                    question += JSONDataGerman.definitionTasksSprintJsonString;
                    break;
                case PromptController.CodeLanguageFrench:
                    question += JSONDataFrench.definitionTasksSprintJsonString;
                    break;
                case PromptController.CodeLanguageItalian:
                    question += JSONDataItalian.definitionTasksSprintJsonString;
                    break;
                case PromptController.CodeLanguageRussian:
                    question += JSONDataRussian.definitionTasksSprintJsonString;
                    break;
                case PromptController.CodeLanguageCatalan:
                    question += JSONDataCatalan.definitionTasksSprintJsonString;
                    break;
            }

            _promptBuilder = new PromptBuilder(question);
            _promptBuilder.SetPromptColor(_projectColor);
            _promptBuilder.SetProjectFeedback(_projectName + " : " + LanguageController.Instance.GetText("text.request.tasks.definition") + " " + (_indexFeatureToProcess + 1) + "/" + _featuresDescriptions.features.Count);

            // FEATURE "xml.tag.feature"
            var feature = _featuresDescriptions.features[_indexFeatureToProcess];
            _currentNameFeature = feature.name;
            _idLinkedFeatureID.Add(feature.name, (WorkDayData.Instance.CurrentProject.ProjectInfoSelected * 10) + (_indexFeatureToProcess + 1));
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.feature") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.feature") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + feature.name + "\">",
                                                "</" + PromptController.Instance.GetText("xml.tag.feature") + ">"),
                                                feature.description);

            // PARTICIPANTS "xml.tag.participants"
            List<string> employees = new List<string>();
            string participantsContent = "\n";
            if (_membersForFeature.TryGetValue(feature.name, out employees))
            {                
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
                    if (groupOfAssistant != null)
                    {
                        participantsContent += "<" + PromptController.Instance.GetText("xml.tag.employee")
                                    + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + assistant + "\""
                                    + " " + PromptController.Instance.GetText("xml.tag.group") + "=\"" + groupOfAssistant.Name + "\""
                                    + " " + PromptController.Instance.GetText("xml.tag.category") + "=\"" + category + "\""
                                    + ">";
                        participantsContent += humanData.Data;
                        participantsContent += "</" + PromptController.Instance.GetText("xml.tag.employee") + ">";
                        participantsContent += "\n";
                    }
                    else
                    {
                        participantsContent += "<" + PromptController.Instance.GetText("xml.tag.employee")                            
                                    + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + assistant + "\""
                                    + " " + PromptController.Instance.GetText("xml.tag.category") + "=\"" + category + "\""
                                    + ">";
                        participantsContent += humanData.Data;
                        participantsContent += "</" + PromptController.Instance.GetText("xml.tag.employee") + ">";
                        participantsContent += "\n";
                    }
                }
            }
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.employees") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.employees") + ">",
                                                "</" + PromptController.Instance.GetText("xml.tag.employees") + ">"),
                                                participantsContent);

            // PROJECT "xml.tag.project"
            if (_projectInfoDescription != null)
            {
                _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                                                    "<" + PromptController.Instance.GetText("xml.tag.project") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + _projectName + "\">",
                                                    "</" + PromptController.Instance.GetText("xml.tag.project") + ">"),
                                                    _projectInfoDescription);
            }
            else
            {
                _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                                                    "<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                                                    "</" + PromptController.Instance.GetText("xml.tag.project") + ">"),
                                                    "");
            }
        }

        private void SaveGlobalFeaturesDefinitions()
        {
            List<DocumentData> globalDocs = WorkDayData.Instance.CurrentProject.GetDocuments();

            foreach (var feature in _featuresDescriptions.features)
            {
                int indexFeature = -1;
                if (_idLinkedFeatureID.TryGetValue(feature.name, out indexFeature))
                {
                    DocumentData newDocument = new DocumentData(WorkDayData.Instance.CurrentProject.GetDocumentNextID(),
                                                            WorkDayData.Instance.CurrentProject.ProjectInfoSelected,
                                                            feature.name,
                                                            LanguageController.Instance.GetText("text.sprint.feature.description"),
                                                            "", new HTMLData() { html = feature.description }, true, false, "", true,
                                                            indexFeature, 
                                                            -1);
                    globalDocs.Add(newDocument);
                }
            }
            UIEventController.Instance.DispatchUIEvent(ScreenDocumentsDataView.EventScreenDocumentsDataViewUpdateGlobalData, globalDocs);
        }

        private void RequestTaskForSingleFeature()
        {
            BuildPromptListTasks();
            if (_confirmation)
            {
                string title = LanguageController.Instance.GetText("ai.title.generate.board.tasks");
                SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
                ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandGenerateBoardTasksPromptConfirmation);
                UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewInitValues, _promptBuilder);
                UIEventController.Instance.DelayUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRefreshInput, 0.1f);
                SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, false);
            }
            else
            {
                AskGenerateTasksLLM(_promptBuilder.BuildPrompt());
            }
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(AICommandSummarizeDocs.EventAICommandSummarizeDocsAllCompleted))
            {
                if (_confirmation)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                }
                SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDCancelSelectionObject);
                ScreenController.Instance.CreateScreen(ScreenCalendarView.ScreenName, true, false, CalendarOption.NORMAL, true);
                AICommandsController.Instance.AddNewAICommand(new AICommandGenerateMeetings(), _confirmation, "");
            }
            if (nameEvent.Equals(AICommandGenerateMeetings.EventAICommandMeetingsDefinitionCompleted))
            {
                _isCompleted = true;
                ApplicationController.Instance.TimeHUD.BlockTimeInteraction = false;
                
                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformationImage, null, LanguageController.Instance.GetText("ai.generate.board.planning.title.completed"), LanguageController.Instance.GetText("ai.generate.board.planning.description.completed"), "", "", "", ApplicationController.Instance.GetContentImage(ImagesIndex.TaskCompleted));
            }
            if (nameEvent.Equals(EventAICommandGenerateBoardThemeRequest))
            {
                AskThemeLLM((string)parameters[0]);
            }
            if (nameEvent.Equals(EventAICommandSprintBoardDefinitionRequest))
            {
                AskSprintBoardDefinitionLLM((string)parameters[0]);
            }
            if (nameEvent.Equals(EventAICommandGenerateBoardTasksRequest))
            {
                AskGenerateTasksLLM((string)parameters[0]);
            }
            ////////////////////////////////////////////////////////////////////////////
            /// 1. GENERATE FEATURES
            ////////////////////////////////////////////////////////////////////////////
            if (nameEvent.Equals(EventAICommandGenerateBoardThemeResponse))
            {
                if (_confirmation)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                }                
                if ((bool)parameters[0])
                {
                    string jsonString = (string)parameters[1];
                    string featureDescriptionJsonString = "{\"features\":" + jsonString + "}";
#if UNITY_EDITOR
                    Debug.Log("FEATURE DESCRIPTION RECEIVED=" + jsonString);
#endif

                    if (!JSONDataFormatValidator.ValidateJsonList<FeatureDescriptionJSON>("{\"items\":" + jsonString + "}"))
                    {
                        string title = LanguageController.Instance.GetText("ai.title.error.prompt");
                        ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLongInput, null, title, "", EventAICommandGenerateBoardThemePromptRetry);
                        UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, jsonString);
                        if (GameObject.FindAnyObjectByType<CodeEditor>() != null)
                        {
                            GameObject.FindAnyObjectByType<CodeEditor>().Refresh();
                        }
                        return;
                    }

                    _featuresDescriptions = JsonUtility.FromJson<FeatureDescriptionListJSON>(featureDescriptionJsonString);
                    _membersForFeature = new Dictionary<string, List<string>>();
                    foreach (FeatureDescriptionJSON feature in _featuresDescriptions.features)
                    {
                        List<string> membersData = new List<string>();
                        _membersForFeature.Add(feature.name, membersData);
                        ProposeMembersForFeature(membersData, _membersForFeature);
                    }

                    BuildPromptSprintBoardDefinition();
                    if (_confirmation)
                    {
                        string title = LanguageController.Instance.GetText("ai.title.generate.sprint.name.board");
                        SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
                        ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandSprintBoardDefinitionPromptConfirmation);
                        UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewInitValues, _promptBuilder);
                        UIEventController.Instance.DelayUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRefreshInput, 0.1f);
                        SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, false);
                    }
                    else
                    {
                        AskSprintBoardDefinitionLLM(_promptBuilder.BuildPrompt());
                    }
                }
                else
                {
                    string title = LanguageController.Instance.GetText("text.error");
                    string description = LanguageController.Instance.GetText("ai.title.error.prompt");
                    ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, title, description, EventAICommandGenerateBoardThemePromptRetry);
                    _isCompleted = false;
                }
            }
            ////////////////////////////////////////////////////////////////////////////
            /// 2. DEFINE SPRINT BOARD
            ////////////////////////////////////////////////////////////////////////////
            if (nameEvent.Equals(EventAICommandSprintBoardDefinitionResponse))
            {
                if (_confirmation)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                }
                if ((bool)parameters[0])
                {
                    string defineBoardResponse = (string)parameters[1];
#if UNITY_EDITOR
                    Debug.LogError("DEFINE BOARD RESPONSE RECEIVED=" + defineBoardResponse);
#endif

                    if (!JSONDataFormatValidator.ValidateJsonItem<SprintBoardDefinitionJSON>(defineBoardResponse))
                    {
                        string title = LanguageController.Instance.GetText("ai.title.error.prompt");
                        ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLongInput, null, title, "", EventAICommandSprintBoardDefinitionPromptRetry);
                        UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, defineBoardResponse);
                        if (GameObject.FindAnyObjectByType<CodeEditor>() != null)
                        {
                            GameObject.FindAnyObjectByType<CodeEditor>().Refresh();
                        }
                        return;
                    }

                    _sprintBoardDefinition = JsonUtility.FromJson<SprintBoardDefinitionJSON>(defineBoardResponse);

                    _indexFeatureToProcess = 0;
                    RequestTaskForSingleFeature();
                }
                else
                {
                    string title = LanguageController.Instance.GetText("text.error");
                    string description = LanguageController.Instance.GetText("ai.title.error.prompt");
                    ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, title, description, EventAICommandSprintBoardDefinitionPromptRetry);
                    _isCompleted = false;
                }
            }

            ////////////////////////////////////////////////////////////////////////////
            /// 3. GENERATE TASKS
            ////////////////////////////////////////////////////////////////////////////
            if (nameEvent.Equals(EventAICommandGenerateBoardTasksResponse))
            {
                if (_confirmation)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                }
                if ((bool)parameters[0])
                {
                    string generatedTasks = (string)parameters[1];
#if UNITY_EDITOR
                    Debug.Log("GENERATED TASKS RECEIVED=" + generatedTasks);
#endif

                    if (!JSONDataFormatValidator.ValidateJsonItem<TasksSprintListJSON>(generatedTasks))
                    {
                        string title = LanguageController.Instance.GetText("ai.title.error.prompt");
                        ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLongInput, null, title, "", EventAICommandGenerateBoardTasksPromptRetry);
                        UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, generatedTasks);
                        if (GameObject.FindAnyObjectByType<CodeEditor>() != null)
                        {
                            GameObject.FindAnyObjectByType<CodeEditor>().Refresh();
                        }
                        return;
                    }

                    TasksSprintListJSON tasksGenerated = JsonUtility.FromJson<TasksSprintListJSON>(generatedTasks);
                    tasksGenerated.name = _currentNameFeature;
                    _tasksGenerated.Add(tasksGenerated);
                    _indexFeatureToProcess++;
                    if (_indexFeatureToProcess < _featuresDescriptions.features.Count)
                    {
                        RequestTaskForSingleFeature();
                    }
                    else
                    {
                        ProcessFinalResults();
                    }
                }
                else
                {
                    string title = LanguageController.Instance.GetText("text.error");
                    string description = LanguageController.Instance.GetText("ai.title.error.prompt");
                    ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, title, description, EventAICommandGenerateBoardTasksPromptRetry);
                    _isCompleted = false;
                }
            }
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            ////////////////////////////////////////////////////////////////////////////
            /// 1. GENERATE FEATURES
            ////////////////////////////////////////////////////////////////////////////
            if (nameEvent.Equals(EventAICommandGenerateBoardThemePromptConfirmation))
            {                
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
#if !SHORTCUT_LLM_SERVER
                    WorkDayData.Instance.AddTimeSession();
#endif
                    SystemEventController.Instance.DelaySystemEvent(EventAICommandGenerateBoardThemeRequest, 0.2f, (string)parameters[2]);
                }
                else
                {
                    _isCompleted = true;
                }
            }
            if (nameEvent.Equals(EventAICommandGenerateBoardThemePromptRetry))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
                    string title = LanguageController.Instance.GetText("ai.title.generate.board.theme");
                    ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandGenerateBoardThemePromptConfirmation);
                    UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewInitValues, _promptBuilder);
                    UIEventController.Instance.DelayUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRefreshInput, 0.1f);
                }
                else
                {
                    _isCompleted = true;
                }
            }
            ////////////////////////////////////////////////////////////////////////////
            /// 2. DEFINE SPRINT BOARD
            ////////////////////////////////////////////////////////////////////////////
            if (nameEvent.Equals(EventAICommandSprintBoardDefinitionPromptConfirmation))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
#if !SHORTCUT_LLM_SERVER
                    WorkDayData.Instance.AddTimeSession();
#endif
                    SystemEventController.Instance.DelaySystemEvent(EventAICommandSprintBoardDefinitionRequest, 0.2f, (string)parameters[2]);
                }
                else
                {
                    _isCompleted = true;
                }
            }
            if (nameEvent.Equals(EventAICommandSprintBoardDefinitionPromptRetry))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
                    string title = LanguageController.Instance.GetText("ai.title.generate.sprint.name.board");
                    ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandSprintBoardDefinitionPromptConfirmation);
                    UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewInitValues, _promptBuilder);
                    UIEventController.Instance.DelayUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRefreshInput, 0.1f);
                }
                else
                {
                    _isCompleted = true;
                }
            }
            ////////////////////////////////////////////////////////////////////////////
            /// 3. GENERATE TASKS
            ////////////////////////////////////////////////////////////////////////////
            if (nameEvent.Equals(EventAICommandGenerateBoardTasksPromptConfirmation))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
#if !SHORTCUT_LLM_SERVER
                    WorkDayData.Instance.AddTimeSession();
#endif
                    SystemEventController.Instance.DelaySystemEvent(EventAICommandGenerateBoardTasksRequest, 0.2f, (string)parameters[2]);
                }
                else
                {
                    _isCompleted = true;
                }
            }
            if (nameEvent.Equals(EventAICommandGenerateBoardTasksPromptRetry))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
                    string title = LanguageController.Instance.GetText("ai.title.generate.board.tasks");
                    ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandGenerateBoardTasksPromptConfirmation);
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