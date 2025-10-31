using UnityEngine;
using yourvrexperience.Utils;
using System.Collections.Generic;
using yourvrexperience.ai;

namespace yourvrexperience.WorkDay
{
    public class AICommandSummarizeTask : IAICommand
    {
        public const string EventAICommandSumarizeTaskStart_DEFINITION = "EventAICommandSumarizeTaskStart";
        public const string EventAICommandSumarizeTaskRequest_DEFINITION = "EventAICommandSumarizeTaskRequest";
        public const string EventAICommandSumarizeTaskResponse_DEFINITION = "EventAICommandSumarizeTaskResponse";
        public const string EventAICommandSumarizeTaskPromptConfirmation_DEFINITION = "EventAICommandSumarizeTaskPromptConfirmation";
        public const string EventAICommandSumarizeTaskPromptRetry_DEFINITION = "EventAICommandSumarizeTaskPromptRetry";
        public const string EventAICommandSumarizeTaskCompleted = "EventAICommandSumarizeTaskCompleted";

        private string EventAICommandSumarizeTaskStart = "";
        private string EventAICommandSumarizeTaskRequest = "";
        private string EventAICommandSumarizeTaskResponse = "";
        private string EventAICommandSumarizeTaskPromptConfirmation = "";
        private string EventAICommandSumarizeTaskPromptRetry = "";

        private bool _isCompleted = false;
        private TaskItemData _task;
        private string _outputEvent;
        private PromptBuilder _promptBuilder;
        private bool _confirmation;
        private int _projectID;
        private int _iterationsToCancel = WorkDayData.TOTAL_RETRIES_AI_FAILED;

        public string Name
        {
            get
            {
                if (_task != null)
                {
                    return "SummarizeTask(" + Utilities.ShortenText(_task.Name, 20) + ")";
                }
                else
                {
                    return "SummarizeTask(NO TASK)";
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

            _task.Summary = "";

            var (taskItemDT, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_task.UID);
            BoardData boardData = WorkDayData.Instance.CurrentProject.GetBoardFor(boardName);
            _projectID = boardData.ProjectId;
            
            EventAICommandSumarizeTaskStart = EventAICommandSumarizeTaskStart_DEFINITION + _task.UID;
            EventAICommandSumarizeTaskRequest = EventAICommandSumarizeTaskRequest_DEFINITION + _task.UID;
            EventAICommandSumarizeTaskResponse = EventAICommandSumarizeTaskResponse_DEFINITION  + _task.UID;
            EventAICommandSumarizeTaskPromptConfirmation = EventAICommandSumarizeTaskPromptConfirmation_DEFINITION + _task.UID;
            EventAICommandSumarizeTaskPromptRetry = EventAICommandSumarizeTaskPromptRetry_DEFINITION + _task.UID;

            BuildPromptSummarizeTask();
            SystemEventController.Instance.DispatchSystemEvent(EventAICommandSumarizeTaskStart, 0.5f);
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
                    SystemEventController.Instance.DispatchSystemEvent(EventAICommandSumarizeTaskCompleted, false, _task);
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
                    SystemEventController.Instance.DispatchSystemEvent(EventAICommandSumarizeTaskCompleted, true, _task);
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
            GameAIData.Instance.AskGenericQuestionAI("-1", prompt, false, EventAICommandSumarizeTaskResponse);
        }

        private void BuildPromptSummarizeTask()
        {
            string question = PromptController.Instance.GetText("ai.command.summarize.task.completed",
                "<" + PromptController.Instance.GetText("xml.tag.task") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.documents") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.participants") + ">",                                
                "<" + PromptController.Instance.GetText("xml.tag.sprint") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.project") + ">");

            _promptBuilder = new PromptBuilder(question);

            // TASK "xml.tag.task"
            List<DocumentData> localDocuments = null;
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.task") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.task") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + _task.Name + "\">",
                                                "</" + PromptController.Instance.GetText("xml.tag.task") + ">"),
                                                PromptController.Instance.ReplaceConflictiveCharacters(_task.Description));
            localDocuments = _task.GetData();

            // DOCUMENTS "xml.tag.documents"
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

            // PARTICIPANTS "xml.tag.participants"
            string participantsContent = "\n";
            List<string> assistants = _task.GetHumanMembers();
            foreach (string assistant in assistants)
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
                    participantsContent += "<" + PromptController.Instance.GetText("xml.tag.participant")
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
                    participantsContent += "<" + PromptController.Instance.GetText("xml.tag.participant")
                                + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + assistant + "\""
                                + " " + PromptController.Instance.GetText("xml.tag.category") + "=\"" + category + "\""
                                + ">";
                    participantsContent += humanDescription;
                    participantsContent += "</" + PromptController.Instance.GetText("xml.tag.participant") + ">";
                    participantsContent += "\n";
                }
            }
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.participants") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.participants") + ">",
                                                "</" + PromptController.Instance.GetText("xml.tag.participants") + ">"),
                                                participantsContent);

            // PROJECT "xml.tag.project"
            var (tmpTaskData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_task.UID);
            BoardData boardTask = WorkDayData.Instance.CurrentProject.GetBoardFor(boardName);
            ProjectInfoData projectInfo = WorkDayData.Instance.CurrentProject.GetProject(boardTask.ProjectId);

            _promptBuilder.SetProjectFeedback(projectInfo.Name + " : " + _task.Name);
            _promptBuilder.SetPromptColor(projectInfo.GetColor());

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
            if (nameEvent.Equals(EventAICommandSumarizeTaskStart))
            {
                if (_confirmation)
                {
                    string title = LanguageController.Instance.GetText("ai.title.task.summarize");
                    SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
                    ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandSumarizeTaskPromptConfirmation);
                    UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewInitValues, _promptBuilder);
                    UIEventController.Instance.DelayUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRefreshInput, 0.1f);
                    SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, false);
                }
                else
                {
                    AskLLM(_promptBuilder.BuildPrompt());
                }
            }
            if (nameEvent.Equals(EventAICommandSumarizeTaskRequest))
            {
                AskLLM((string)parameters[0]);
            }
            if (nameEvent.Equals(EventAICommandSumarizeTaskResponse))
            {
                if (_confirmation)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                }                
                if ((bool)parameters[0])
                {
                    string summaryTask = (string)parameters[1];
#if UNITY_EDITOR
                    Debug.Log("TASK SUMMARY RECEIVED=" + summaryTask);
#endif

                    _task.Summary = summaryTask;
                    _isCompleted = true;
                }
                else
                {
                    if (_confirmation)
                    {
                        string title = LanguageController.Instance.GetText("text.error");
                        string description = LanguageController.Instance.GetText("ai.title.error.prompt");
                        ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, title, description, EventAICommandSumarizeTaskPromptRetry);
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
            if (nameEvent.Equals(EventAICommandSumarizeTaskPromptConfirmation))
            {                
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
#if !SHORTCUT_LLM_SERVER
                    WorkDayData.Instance.AddTimeSession();
#endif
                    SystemEventController.Instance.DelaySystemEvent(EventAICommandSumarizeTaskRequest, 0.2f, (string)parameters[2]);
                }
                else
                {
                    _isCompleted = true;
                }
            }
            if (nameEvent.Equals(EventAICommandSumarizeTaskPromptRetry))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
                    string title = LanguageController.Instance.GetText("ai.title.meeting.reply");
                    ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandSumarizeTaskPromptConfirmation);
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