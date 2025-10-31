using UnityEngine;
using yourvrexperience.Utils;
using System.Collections.Generic;
using InGameCodeEditor;

namespace yourvrexperience.WorkDay
{
    public class AICommandGenerateDocsForTask : IAICommand
    {
        public const string EventAICommandGenerateDocsForTaskRequest_DEFINITION = "EventAICommandGenerateDocsForTaskRequest";
        public const string EventAICommandGenerateDocsForTaskResponse_DEFINITION = "EventAICommandGenerateDocsForTaskResponse";
        public const string EventAICommandGenerateDocsForTaskPromptConfirmation_DEFINITION = "EventAICommandGenerateDocsForTaskPromptConfirmation";
        public const string EventAICommandGenerateDocsForTaskPromptRetry_DEFINITION = "EventAICommandGenerateDocsForTaskPromptRetry";
        
        public const string EventAICommandGenerateDocsForTaskCompleted = "EventAICommandGenerateDocsForTaskCompleted";

        private string EventAICommandGenerateDocsForTaskRequest = "";
        private string EventAICommandGenerateDocsForTaskResponse = "";
        private string EventAICommandGenerateDocsForTaskPromptConfirmation = "";
        private string EventAICommandGenerateDocsForTaskPromptRetry = "";

        private bool _isCompleted = false;
        private string _outputEvent;
        private TaskItemData _task;
        private ProjectInfoData _project;
        private BoardData _board;
        private PromptBuilder _promptBuilder;
        private bool _confirmation;
        private TasksDocumentsJSON _tasksDocuments;
        private int _iterationsToCancel = WorkDayData.TOTAL_RETRIES_AI_FAILED;

        public string Name
        {
            get
            {
                if (_task != null)
                {
                    return "TaskDocuments(" + Utilities.ShortenText(_task.Name, 20) + ")";
                }
                else
                {
                    return "TaskDocuments(NO TASK)";
                }                
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

            _task = (TaskItemData)parameters[0];
            _board = (BoardData)parameters[1];
            _project = (ProjectInfoData)parameters[2];
            _outputEvent = (string)parameters[3];

            EventAICommandGenerateDocsForTaskRequest = EventAICommandGenerateDocsForTaskRequest_DEFINITION + _task.UID;
            EventAICommandGenerateDocsForTaskResponse = EventAICommandGenerateDocsForTaskResponse_DEFINITION + _task.UID;
            EventAICommandGenerateDocsForTaskPromptConfirmation = EventAICommandGenerateDocsForTaskPromptConfirmation_DEFINITION + _task.UID;
            EventAICommandGenerateDocsForTaskPromptRetry = EventAICommandGenerateDocsForTaskPromptRetry_DEFINITION + _task.UID;

            string title = LanguageController.Instance.GetText("ai.title.tasks.documents");
            BuildPromptTasksDocuments();
            if (_confirmation)
            {
                SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
                ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandGenerateDocsForTaskPromptConfirmation);
                UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewInitValues, _promptBuilder);
                UIEventController.Instance.DelayUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRefreshInput, 0.1f);
                SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, false);
            }
            else
            {
                AskLLM(_promptBuilder.BuildPrompt());
            }
        }

        public void Destroy()
        {
            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
            if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

            SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, true);
            if (_tasksDocuments == null)
            {
                if ((_outputEvent != null) && (_outputEvent.Length > 0))
                {
                    SystemEventController.Instance.DispatchSystemEvent(_outputEvent, _task, null);
                }
                else
                {
                    SystemEventController.Instance.DispatchSystemEvent(EventAICommandGenerateDocsForTaskCompleted, _task, null);
                }
            }
            else
            {
                foreach(TaskDocumentJSON entry in _tasksDocuments.documents)
                {
                    DocumentData checkDocument = new DocumentData(WorkDayData.Instance.CurrentProject.GetDocumentNextID(), _project.Id, entry.name, "", "", new HTMLData(), false, false, "", false, -1, -1);
                    List<DocumentData> docsTask = _task.GetData();
                    if (docsTask.Contains(checkDocument))
                    {
                        entry.name = entry.name + "_2";
                    }                    
                    else
                    {
                        List<DocumentData> globalDocs = WorkDayData.Instance.CurrentProject.GetDocuments();
                        if (globalDocs.Contains(checkDocument))
                        {
                            entry.name = entry.name + "_2";
                        }
                    }
                }

                if ((_outputEvent != null) && (_outputEvent.Length > 0))
                {
                    SystemEventController.Instance.DispatchSystemEvent(_outputEvent, _task, _tasksDocuments);
                }
                else
                {
                    SystemEventController.Instance.DispatchSystemEvent(EventAICommandGenerateDocsForTaskCompleted, _task, _tasksDocuments);
                }
            }

            _promptBuilder = null;
            _task = null;
            _project = null;
            _board = null;
        }

        private void AskLLM(string prompt)
        {
            if (_confirmation)
            {
                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("ai.processing.now.please.wait"));
            }
            WorkDayData.Instance.AskWorkDayAITasksDocuments(prompt, true, EventAICommandGenerateDocsForTaskResponse);
        }

        private void BuildPromptTasksDocuments()
        {
            string question = PromptController.Instance.GetText("ai.command.tasks.create.documents",
                "<" + PromptController.Instance.GetText("xml.tag.task") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.previous.task") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.employees") + ">",                
                "<" + PromptController.Instance.GetText("xml.tag.documents") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.sprint") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.project") + ">");

            switch (PromptController.Instance.CodeLanguage)
            {
                case PromptController.CodeLanguageEnglish:
                    question += JSONDataEnglish.summaryTasksJsonString;
                    break;
                case PromptController.CodeLanguageSpanish:
                    question += JSONDataSpanish.summaryTasksJsonString;
                    break;
                case PromptController.CodeLanguageGerman:
                    question += JSONDataGerman.summaryTasksJsonString;
                    break;
                case PromptController.CodeLanguageFrench:
                    question += JSONDataFrench.summaryTasksJsonString;
                    break;
                case PromptController.CodeLanguageItalian:
                    question += JSONDataItalian.summaryTasksJsonString;
                    break;
                case PromptController.CodeLanguageRussian:
                    question += JSONDataRussian.summaryTasksJsonString;
                    break;
                case PromptController.CodeLanguageCatalan:
                    question += JSONDataCatalan.meetingForTaskJsonString;
                    break;
            }
            _promptBuilder = new PromptBuilder(question);
            _promptBuilder.SetPromptColor(_project.GetColor());
            _promptBuilder.SetProjectFeedback(_project.Name + " : " + _task.Name);

            // TASK "xml.tag.task"
            List<DocumentData> localDocuments = null;
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.task") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.task") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + _task.Name + "\">",
                                                "</" + PromptController.Instance.GetText("xml.tag.task") + ">"),
                                                PromptController.Instance.ReplaceConflictiveCharacters(_task.Description));
            localDocuments = _task.GetData();

            TaskItemData linkedTask = null;
            if (_task.Linked != -1)
            {
                var (taskLinkedData, boardLinkedName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_task.Linked);
                linkedTask = taskLinkedData;
            }

            if (linkedTask != null)
            {
                _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.previous.task") + ">",
                                                    "<" + PromptController.Instance.GetText("xml.tag.previous.task") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + linkedTask.Name + "\">",
                                                    "</" + PromptController.Instance.GetText("xml.tag.previous.task") + ">"),
                                                    PromptController.Instance.ReplaceConflictiveCharacters(linkedTask.Summary));
            }
            else
            {
                _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.previous.task") + ">", 
                                                    "<" + PromptController.Instance.GetText("xml.tag.previous.task") + ">",
                                                    "</" + PromptController.Instance.GetText("xml.tag.previous.task") + ">"),
                                                    "");
            }

            // PARTICIPANTS "xml.tag.participants"
            string participantsContent = "\n";
            List<string> assistants = _task.GetHumanMembers();
            foreach (string assistant in assistants)
            {
                WorldItemData humanData = WorkDayData.Instance.CurrentProject.GetItemByName(assistant);

                if (humanData != null)
                {
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

            // DOCUMENTS "xml.tag.documents"
            List<DocumentData> globalDocs = WorkDayData.Instance.CurrentProject.GetDocuments();
            string documentsContent = "\n";
            foreach (DocumentData document in globalDocs)
            {
                if (_board.ProjectId == document.ProjectId)
                {
                    bool shouldAdd = true;
                    if (_task.Feature > 0)
                    {
                        if ((document.FeatureID > 0) && (_task.Feature != document.FeatureID))
                        {
                            shouldAdd = false;
                        }
                    }
                    if (shouldAdd)
                    {
                        documentsContent += "<" + PromptController.Instance.GetText("xml.tag.doc")
                                    + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + document.Name + "\""
                                    + " " + PromptController.Instance.GetText("xml.tag.type") + "=\"" + PromptController.Instance.GetText("xml.tag.project.document") + "\""
                                    + " " + PromptController.Instance.GetText("xml.tag.description") + "=\"" + document.Summary + "\""
                                    + "/>";
                        documentsContent += "\n";
                    }
                }
            }
            if (localDocuments != null)
            {
                foreach (DocumentData document in localDocuments)
                {
                    if (!globalDocs.Contains(document))
                    {
                        documentsContent += "<" + PromptController.Instance.GetText("xml.tag.doc")
                                    + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + document.Name + "\""
                                    + " " + PromptController.Instance.GetText("xml.tag.type") + "=\"" + PromptController.Instance.GetText("xml.tag.task.document") + "\""
                                    + " " + PromptController.Instance.GetText("xml.tag.description") + "=\"" + document.Summary + "\""
                                    + "/>";
                        documentsContent += "\n";
                    }
                }
            }
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.documents") + ">", 
                                                "<" + PromptController.Instance.GetText("xml.tag.documents") + ">",
                                                "</" + PromptController.Instance.GetText("xml.tag.documents") + ">"),
                                                documentsContent);

            // PROJECT "xml.tag.project"
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.project") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + _project.Name + "\">",
                                                "</" + PromptController.Instance.GetText("xml.tag.project") + ">"),
                                                _project.Description);

            // SPRINT (FEATURES) "xml.tag.sprint"
            question += "</" + PromptController.Instance.GetText("xml.tag.sprint") + ">";
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.sprint") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.sprint") + ">",
                                                "</" + PromptController.Instance.GetText("xml.tag.sprint") + ">"),
                                                WorkDayData.Instance.CurrentProject.PackBoardsXML(_project.Id));
        }
        
        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventAICommandGenerateDocsForTaskRequest))
            {
                AskLLM((string)parameters[0]);
            }
            if (nameEvent.Equals(EventAICommandGenerateDocsForTaskResponse))
            {
                if (_confirmation)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                }                
                if ((bool)parameters[0])
                {
                    string summaryTasks = (string)parameters[1];
#if UNITY_EDITOR
                    Debug.Log("MEETING SUMMARY RECEIVED=" + summaryTasks);
#endif

                    if (!JSONDataFormatValidator.ValidateJsonItem<TasksDocumentsJSON>(summaryTasks))
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

                                AskLLM(_promptBuilder.BuildPrompt());
                            }
                        }
                        else
                        {
                            string title = LanguageController.Instance.GetText("ai.title.error.prompt");
                            ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLongInput, null, title, "", EventAICommandGenerateDocsForTaskPromptRetry);
                            UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, summaryTasks);
                            if (GameObject.FindAnyObjectByType<CodeEditor>() != null)
                            {
                                GameObject.FindAnyObjectByType<CodeEditor>().Refresh();
                            }
                        }
                        return;
                    }
                    
                    _tasksDocuments = JsonUtility.FromJson<TasksDocumentsJSON>(summaryTasks);
                    // FIX WRONG NAMES BECAUSE AI WILL FAIL
                    foreach (TaskDocumentJSON doc in _tasksDocuments.documents)
                    {
                        string[] personList = doc.persons.Split(",");
                        string finalPersonList = "";
                        foreach (string person in personList)
                        {
                            if (finalPersonList.Length > 0) finalPersonList += ",";
                            finalPersonList += WorkDayData.Instance.CurrentProject.GetClosestName(person.Trim(), true);
                        }
                        doc.persons = finalPersonList;
                    }

                    _isCompleted = true;
                }
                else
                {
                    if (_confirmation)
                    {
                        string title = LanguageController.Instance.GetText("text.error");
                        string description = LanguageController.Instance.GetText("ai.title.error.prompt");
                        ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, title, description, EventAICommandGenerateDocsForTaskPromptRetry);
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

                            AskLLM(_promptBuilder.BuildPrompt());
                        }
                    }
                }
            }
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventAICommandGenerateDocsForTaskPromptConfirmation))
            {                
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
#if !SHORTCUT_LLM_SERVER
                    WorkDayData.Instance.AddTimeSession();
#endif
                    SystemEventController.Instance.DelaySystemEvent(EventAICommandGenerateDocsForTaskRequest, 0.2f, (string)parameters[2]);
                }
                else
                {
                    _isCompleted = true;
                }
            }
            if (nameEvent.Equals(EventAICommandGenerateDocsForTaskPromptRetry))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
                    string title = LanguageController.Instance.GetText("ai.title.meeting.reply");
                    ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandGenerateDocsForTaskPromptConfirmation);
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