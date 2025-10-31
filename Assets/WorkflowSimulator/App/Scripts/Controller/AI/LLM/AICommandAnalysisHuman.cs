using System.Collections.Generic;
using UnityEngine;
using yourvrexperience.ai;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class AICommandAnalysisHuman : IAICommand
    {
        private const string EventAICommandAnalysisHumanRequest = "EventAICommandAnalysisHumanRequest";
        private const string EventAICommandAnalysisHumanResponse = "EventAICommandAnalysisHumanResponse";
        private const string EventAICommandAnalysisHumanPromptConfirmation = "EventAICommandAnalysisHumanPromptConfirmation";
        private const string EventAICommandAnalysisHumanPromptRetry = "EventAICommandAnalysisHumanPromptRetry";
        public const string EventAICommandAnalysisHumanResults = "EventAICommandAnalysisHumanResults";
        
        private bool _isCompleted = false;
        private PromptBuilder _promptBuilder;
        private string _history;
        private string _responseAnalysis = "";

        public string Name
        {
            get
            {
                return "AnalysisHuman";
            }
        }

        public bool IsCompleted()
        {
            return _isCompleted;
        }

        public void Request(bool confirmation, params object[] parameters)
        {
            SystemEventController.Instance.Event += OnSystemEvent;
            UIEventController.Instance.Event += OnUIEvent;

            _history = (string)parameters[0];

            BuildPromptAnalysisHuman();

            string title = LanguageController.Instance.GetText("ai.title.task.summarize");
            SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
            ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandAnalysisHumanPromptConfirmation);
            UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewInitValues, _promptBuilder);
            UIEventController.Instance.DelayUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRefreshInput, 0.1f);
            SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, false);
        }

        public void Destroy()
        {
            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
            if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

            SystemEventController.Instance.DispatchSystemEvent(EventAICommandAnalysisHumanResults, _responseAnalysis, _history);
            SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, true);

            _promptBuilder = null;
        }

        private void AskLLM(string prompt)
        {
            ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("ai.processing.now.please.wait"));
            GameAIData.Instance.AskGenericQuestionAI("-1", prompt, false, EventAICommandAnalysisHumanResponse);
        }

        private void BuildPromptAnalysisHuman()
        {
            string question = PromptController.Instance.GetText("ai.command.analyse.human.history",
                ApplicationController.Instance.HumanPlayer.NameHuman,
                "<" + PromptController.Instance.GetText("xml.tag.contributions") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.tasks") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.documents") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.project") + ">");
            
            _promptBuilder = new PromptBuilder(question);

            ProjectInfoData projectInfo = WorkDayData.Instance.CurrentProject.GetProject(WorkDayData.Instance.CurrentProject.ProjectInfoSelected);

            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.contributions") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.contributions") + ">",
                                                "</" + PromptController.Instance.GetText("xml.tag.contributions") + ">"),
                                                _history);

            // TASKS "xml.tag.tasks"
            List<BoardData> boards = WorkDayData.Instance.CurrentProject.GetAllBoards();
            string tasksContent = "\n";
            foreach (BoardData board in boards)
            {
                List<TaskItemData> tasks = board.GetTasks();
                foreach (TaskItemData task in tasks)
                {
                    if (task.IsMemberOfTask(ApplicationController.Instance.HumanPlayer.NameHuman))
                    {
                        tasksContent += "<" + PromptController.Instance.GetText("xml.tag.task")
                                + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + task.Name
                                + "\" " + PromptController.Instance.GetText("xml.tag.description") + "=\"" + PromptController.Instance.ReplaceConflictiveCharacters(task.Description)
                                + "\"/>";
                        tasksContent += "\n";
                    }
                }                
            }
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.tasks") + ">",
                                                        "<" + PromptController.Instance.GetText("xml.tag.tasks") + ">",
                                                        "</" + PromptController.Instance.GetText("xml.tag.tasks") + ">"),
                                                        tasksContent);
            
            // DOCUMENTS "xml.tag.documents"
            List<DocumentData> globalDocs = WorkDayData.Instance.CurrentProject.GetDocuments();
            string documentsContent = "\n";
            foreach (DocumentData document in globalDocs)
            {
                if (projectInfo.Id == document.ProjectId)
                {
                    documentsContent += "<" + PromptController.Instance.GetText("xml.tag.doc")
                                + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + document.Name
                                + "\" " + PromptController.Instance.GetText("xml.tag.description") + "=\"" + PromptController.Instance.ReplaceConflictiveCharacters(document.Summary)
                                + "\"/>";
                    documentsContent += "\n";
                }
            }
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.documents") + ">",
                                                                    "<" + PromptController.Instance.GetText("xml.tag.documents") + ">",
                                                                    "</" + PromptController.Instance.GetText("xml.tag.documents") + ">"),
                                                                    documentsContent);

            // PROJECT "xml.tag.project"
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.project") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + projectInfo.Name + "\">",
                                                "</" + PromptController.Instance.GetText("xml.tag.project") + ">"),
                                                projectInfo.Description);
        }
        
        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventAICommandAnalysisHumanRequest))
            {
                AskLLM((string)parameters[0]);
            }
            if (nameEvent.Equals(EventAICommandAnalysisHumanResponse))
            {
                UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                if ((bool)parameters[0])
                {
                    _responseAnalysis = (string)parameters[1];
#if UNITY_EDITOR
                    Debug.Log("ANALYSIS RECEIVED=" + _responseAnalysis);
#endif
                    _isCompleted = true;
                }
                else
                {
                    string title = LanguageController.Instance.GetText("text.error");
                    string description = LanguageController.Instance.GetText("ai.title.error.prompt");
                    ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, title, description, EventAICommandAnalysisHumanPromptRetry);
                    _isCompleted = false;
                }
            }
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventAICommandAnalysisHumanPromptConfirmation))
            {                
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
#if !SHORTCUT_LLM_SERVER
                    WorkDayData.Instance.AddTimeSession();
#endif
                    SystemEventController.Instance.DelaySystemEvent(EventAICommandAnalysisHumanRequest, 0.2f, (string)parameters[2]);
                }
                else
                {
                    _isCompleted = true;
                }
            }
            if (nameEvent.Equals(EventAICommandAnalysisHumanPromptRetry))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
                    string title = LanguageController.Instance.GetText("ai.title.meeting.reply");
                    ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandAnalysisHumanPromptConfirmation);
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