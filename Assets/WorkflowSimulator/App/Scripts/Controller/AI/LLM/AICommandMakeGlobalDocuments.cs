using UnityEngine;
using yourvrexperience.Utils;
using System.Collections.Generic;
using InGameCodeEditor;

namespace yourvrexperience.WorkDay
{
    public class AICommandMakeGlobalDocuments : IAICommand
    {
        public const string EventAICommandMakeGlobalDocumentsStart_DEFINITION = "EventAICommandMakeGlobalDocumentsStart";
        public const string EventAICommandMakeGlobalDocumentsRequest_DEFINITION = "EventAICommandMakeGlobalDocumentsRequest";
        public const string EventAICommandMakeGlobalDocumentsResponse_DEFINITION = "EventAICommandMakeGlobalDocumentsResponse";
        public const string EventAICommandMakeGlobalDocumentsPromptConfirmation_DEFINITION = "EventAICommandMakeGlobalDocumentsPromptConfirmation";
        public const string EventAICommandMakeGlobalDocumentsPromptRetry_DEFINITION = "EventAICommandMakeGlobalDocumentsPromptRetry";
        public const string EventAICommandMakeGlobalDocumentsCompleted = "EventAICommandMakeGlobalDocumentsCompleted";

        private string EventAICommandMakeGlobalDocumentsStart = "";
        private string EventAICommandMakeGlobalDocumentsRequest = "";
        private string EventAICommandMakeGlobalDocumentsResponse = "";
        private string EventAICommandMakeGlobalDocumentsPromptConfirmation = "";
        private string EventAICommandMakeGlobalDocumentsPromptRetry = "";

        private bool _isCompleted = false;
        private TaskItemData _task;
        private int _projectID;
        private string _outputEvent;
        private PromptBuilder _promptBuilder;
        private bool _confirmation;
        private int _iterationsToCancel = WorkDayData.TOTAL_RETRIES_AI_FAILED;

        public string Name
        {
            get
            {
                if (_task != null)
                {
                    return "MakeGlobals(" + Utilities.ShortenText(_task.Name, 20) + ")";
                }
                else
                {
                    return "MakeGlobals(NO documents)";
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
            _outputEvent = (string)parameters[1];

            EventAICommandMakeGlobalDocumentsStart = EventAICommandMakeGlobalDocumentsStart_DEFINITION + _task.UID;
            EventAICommandMakeGlobalDocumentsRequest = EventAICommandMakeGlobalDocumentsRequest_DEFINITION + _task.UID;
            EventAICommandMakeGlobalDocumentsResponse = EventAICommandMakeGlobalDocumentsResponse_DEFINITION  + _task.UID;
            EventAICommandMakeGlobalDocumentsPromptConfirmation = EventAICommandMakeGlobalDocumentsPromptConfirmation_DEFINITION + _task.UID;
            EventAICommandMakeGlobalDocumentsPromptRetry = EventAICommandMakeGlobalDocumentsPromptRetry_DEFINITION + _task.UID;

            BuildPromptMakeGlobalDocuments();
            SystemEventController.Instance.DispatchSystemEvent(EventAICommandMakeGlobalDocumentsStart, 0.1f);
        }

        public void Destroy()
        {
            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
            if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

            SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, true);

            if ((_task.Summary == null) || (_task.Summary.Length == 0))
            {
                if (_outputEvent.Length > 0)
                {
                    SystemEventController.Instance.DispatchSystemEvent(_outputEvent, false, _task);
                }
                else
                {
                    SystemEventController.Instance.DispatchSystemEvent(EventAICommandMakeGlobalDocumentsCompleted, false, _task);
                }
            }
            else
            {
                if (_outputEvent.Length > 0)
                {
                    SystemEventController.Instance.DispatchSystemEvent(_outputEvent, true, _task);
                }
                else
                {
                    SystemEventController.Instance.DispatchSystemEvent(EventAICommandMakeGlobalDocumentsCompleted, true, _task);
                }
            }

            _promptBuilder = null;
            _task = null;            
        }

        private void AskLLM(string prompt)
        {
            if (_confirmation)
            {
                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("ai.processing.now.please.wait"));
            }
            WorkDayData.Instance.AskWorkDayAIMakeGlobals(prompt, true, EventAICommandMakeGlobalDocumentsResponse);
        }

        private void BuildPromptMakeGlobalDocuments()
        {
            string question = PromptController.Instance.GetText("ai.command.summarize.make.globals.for.task",
                "<" + PromptController.Instance.GetText("xml.tag.documents") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.task") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.future.tasks") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.sprint") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.project") + ">");

            switch (PromptController.Instance.CodeLanguage)
            {
                case PromptController.CodeLanguageEnglish:
                    question += JSONDataEnglish.globalDocumentsJsonString;
                    break;
                case PromptController.CodeLanguageSpanish:
                    question += JSONDataSpanish.globalDocumentsJsonString;
                    break;
                case PromptController.CodeLanguageGerman:
                    question += JSONDataGerman.globalDocumentsJsonString;
                    break;
                case PromptController.CodeLanguageFrench:
                    question += JSONDataFrench.globalDocumentsJsonString;
                    break;
                case PromptController.CodeLanguageItalian:
                    question += JSONDataItalian.globalDocumentsJsonString;
                    break;
                case PromptController.CodeLanguageRussian:
                    question += JSONDataRussian.globalDocumentsJsonString;
                    break;
                case PromptController.CodeLanguageCatalan:
                    question += JSONDataCatalan.globalDocumentsJsonString;
                    break;
            }

            _promptBuilder = new PromptBuilder(question);

            var (tmpTaskData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_task.UID);
            BoardData boardTask = WorkDayData.Instance.CurrentProject.GetBoardFor(boardName);
            ProjectInfoData projectInfo = WorkDayData.Instance.CurrentProject.GetProject(boardTask.ProjectId);
            _projectID = projectInfo.Id;

            // DOCUMENTS "xml.tag.documents"
            List<DocumentData> localDocuments = _task.GetData();
            string documentsContent = "\n";
            foreach (DocumentData document in localDocuments)
            {
                string summary = document.Summary;
                if ((summary == null) || (summary.Length == 0))
                {
                    summary = document.Data.GetHTML();
                }
                documentsContent += "<" + PromptController.Instance.GetText("xml.tag.doc")
                            + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + document.Name
                            + "\" " + PromptController.Instance.GetText("xml.tag.description") + "=\"" + summary
                            + "\"/>";
                documentsContent += "\n";
            }
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.documents") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.documents") + ">",
                                                "</" + PromptController.Instance.GetText("xml.tag.documents") + ">"),
                                                documentsContent);
            // TASK "xml.tag.task"
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.task") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.task") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + _task.Name + "\">",
                                                "</" + PromptController.Instance.GetText("xml.tag.task") + ">"),
                                                PromptController.Instance.ReplaceConflictiveCharacters(_task.Summary));

            // FUTURE TASKS "xml.tag.future.tasks"
            string dataFutureTasks = WorkDayData.Instance.CurrentProject.PackTasks(PromptController.Instance.GetText("xml.tag.future.task"),
                                                                                    projectInfo.Id,
                                                                                    TaskItemData.TaskStates.TODO,
                                                                                    _task.UID);
            if (dataFutureTasks.Length > 0)
            {
                dataFutureTasks = "\n" + dataFutureTasks;
            }
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.future.tasks") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.future.tasks") + ">",
                                                "</" + PromptController.Instance.GetText("xml.tag.future.tasks") + ">"),
                                                dataFutureTasks);

            // PROJECT "xml.tag.project"
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.project") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + projectInfo.Name + "\">",
                                                "</" + PromptController.Instance.GetText("xml.tag.project") + ">"),
                                                projectInfo.Description);

            // SPRINT (FEATURES) "xml.tag.sprint"
            question += "</" + PromptController.Instance.GetText("xml.tag.sprint") + ">";
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.sprint") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.sprint") + ">",
                                                "</" + PromptController.Instance.GetText("xml.tag.sprint") + ">"),
                                                WorkDayData.Instance.CurrentProject.PackBoardsXML(projectInfo.Id));            
        }
        
        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventAICommandMakeGlobalDocumentsStart))
            {
                if (_confirmation)
                {
                    string title = LanguageController.Instance.GetText("ai.title.task.summarize");
                    SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
                    ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandMakeGlobalDocumentsPromptConfirmation);
                    UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewInitValues, _promptBuilder);
                    UIEventController.Instance.DelayUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRefreshInput, 0.1f);
                    SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, false);
                }
                else
                {
                    AskLLM(_promptBuilder.BuildPrompt());
                }
            }
            if (nameEvent.Equals(EventAICommandMakeGlobalDocumentsRequest))
            {
                AskLLM((string)parameters[0]);
            }
            if (nameEvent.Equals(EventAICommandMakeGlobalDocumentsResponse))
            {
                if (_confirmation)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                }                
                if ((bool)parameters[0])
                {
                    string jsonString = (string)parameters[1];
                    string documentGlobalsJsonString = "{\"documents\":" + jsonString + "}";
#if UNITY_EDITOR
                    Debug.Log("GLOBAL DOCUMENTS RECEIVED=" + documentGlobalsJsonString);
#endif

                    if (!JSONDataFormatValidator.ValidateJsonList<GlobalDocumentJSON>("{\"items\":" + jsonString + "}"))
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
                            ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLongInput, null, title, "", EventAICommandMakeGlobalDocumentsPromptRetry);
                            UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, jsonString);
                            if (GameObject.FindAnyObjectByType<CodeEditor>() != null)
                            {
                                GameObject.FindAnyObjectByType<CodeEditor>().Refresh();
                            }
                        }
                        return;
                    }

                    GlobalDocumentListJSON globalDocuments = JsonUtility.FromJson<GlobalDocumentListJSON>(documentGlobalsJsonString);

#if UNITY_EDITOR
                    Debug.Log("GLOBAL DOCUMENT COUNT=" + globalDocuments.documents.Count);
#endif
                    List<DocumentData> docsLocal = _task.GetData();
                    List<DocumentData> globalDocs = WorkDayData.Instance.CurrentProject.GetDocuments();
                    bool newGlobalDocs = false;
                    foreach (var doc in globalDocuments.documents)
                    {
                        foreach (DocumentData localDoc in docsLocal)
                        {
                            if (localDoc.Name.ToLower().Equals(doc.name.ToLower()))
                            {
                                DocumentData docFound = WorkDayData.Instance.CurrentProject.GetDocumentByName(doc.name);
                                if (docFound == null)
                                {
                                    localDoc.IsGlobal = true;
                                    localDoc.IsChanged = false;
                                    globalDocs.Add(localDoc.Clone());
                                    newGlobalDocs = true;
                                    Debug.LogError("MAKE GLOBAL THE DOCUMENT=" + doc.name);
                                }
                            }
                        }
                    }
                    if (newGlobalDocs)
                    {
                        UIEventController.Instance.DispatchUIEvent(ScreenDocumentsDataView.EventScreenDocumentsDataViewUpdateGlobalData, globalDocs);
                    }

                    _isCompleted = true;
                }
                else
                {
                    if (_confirmation)
                    {
                        string title = LanguageController.Instance.GetText("text.error");
                        string description = LanguageController.Instance.GetText("ai.title.error.prompt");
                        ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, title, description, EventAICommandMakeGlobalDocumentsPromptRetry);
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
            if (nameEvent.Equals(EventAICommandMakeGlobalDocumentsPromptConfirmation))
            {                
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
#if !SHORTCUT_LLM_SERVER
                    WorkDayData.Instance.AddTimeSession();
#endif
                    SystemEventController.Instance.DelaySystemEvent(EventAICommandMakeGlobalDocumentsRequest, 0.2f, (string)parameters[2]);
                }
                else
                {
                    _isCompleted = true;
                }
            }
            if (nameEvent.Equals(EventAICommandMakeGlobalDocumentsPromptRetry))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
                    string title = LanguageController.Instance.GetText("ai.title.meeting.reply");
                    ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandMakeGlobalDocumentsPromptConfirmation);
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