using UnityEngine;
using yourvrexperience.Utils;
using System.Collections.Generic;
using InGameCodeEditor;

namespace yourvrexperience.WorkDay
{
    public class AICommandSummarizeMeeting : IAICommand
    {
        public const string EventAICommandSumarizeMeetingRequest_DEFINITION = "EventAICommandSumarizeMeetingRequest";
        public const string EventAICommandSumarizeMeetingResponse_DEFINITION = "EventAICommandSumarizeMeetingResponse";
        public const string EventAICommandSumarizeMeetingPromptConfirmation_DEFINITION = "EventAICommandSumarizeMeetingPromptConfirmation";
        public const string EventAICommandSumarizeMeetingPromptRetry_DEFINITION = "EventAICommandSumarizeMeetingPromptRetry";
        public const string EventAICommandSumarizeMeetingCompleted = "EventAICommandSumarizeMeetingCompleted";

        private string EventAICommandSumarizeMeetingRequest = "";
        private string EventAICommandSumarizeMeetingResponse = "";
        private string EventAICommandSumarizeMeetingPromptConfirmation = "";
        private string EventAICommandSumarizeMeetingPromptRetry = "";

        private bool _isCompleted = false;
        private MeetingData _meeting;
        private string _outputEvent;
        private PromptBuilder _promptBuilder;
        private bool _confirmation;
        private List<InteractionData> _interactions;
        private MeetingSummaryJSON _meetingSummary;
        private int _iterationsToCancel = WorkDayData.TOTAL_RETRIES_AI_FAILED;

        public string Name
        {
            get
            {
                if (_meeting != null)
                {
                    return "SummarizeMeeting(" + Utilities.ShortenText(_meeting.Name, 20) + ")";
                }
                else
                {
                    return "SummarizeMeeting(NO MEETING)";
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

            _meeting = (MeetingData)parameters[0];
            _outputEvent = (string)parameters[1];

            EventAICommandSumarizeMeetingRequest = EventAICommandSumarizeMeetingRequest_DEFINITION  + _meeting.GetUID();
            EventAICommandSumarizeMeetingResponse = EventAICommandSumarizeMeetingResponse_DEFINITION  + _meeting.GetUID();
            EventAICommandSumarizeMeetingPromptConfirmation = EventAICommandSumarizeMeetingPromptConfirmation_DEFINITION + _meeting.GetUID();
            EventAICommandSumarizeMeetingPromptRetry = EventAICommandSumarizeMeetingPromptRetry_DEFINITION + _meeting.GetUID();

            _interactions = _meeting.GetInteractions();            
            if (_interactions.Count > 0)
            {
                string title = LanguageController.Instance.GetText("ai.title.meeting.summarize");
                BuildPromptSummarizeMeeting();
                if (_confirmation)
                {
                    SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
                    ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandSumarizeMeetingPromptConfirmation);
                    UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewInitValues, _promptBuilder);
                    UIEventController.Instance.DelayUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRefreshInput, 0.1f);
                    SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, false);
                }
                else
                {
                    AskLLM(_promptBuilder.BuildPrompt());
                }
            }
            else
            {
                _isCompleted = true;
            }
        }

        public void Destroy()
        {
            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
            if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

            SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, true);

            if (_meetingSummary == null)
            {
                if ((_outputEvent != null) && (_outputEvent.Length > 0))
                {
                    SystemEventController.Instance.DispatchSystemEvent(_outputEvent, _meeting, null);
                }
                else
                {
                    SystemEventController.Instance.DispatchSystemEvent(EventAICommandSumarizeMeetingCompleted, _meeting, null);
                }
            }
            else
            {
                if (UnityEngine.Random.Range(0,100) > _meeting.ShouldCreateDocuments)
                {
                    _meetingSummary.documents = new List<DocumentMeetingJSON>();
                }

                if ((_outputEvent != null) && (_outputEvent.Length > 0))
                {
                    SystemEventController.Instance.DispatchSystemEvent(_outputEvent, _meeting, _meetingSummary);
                }
                else
                {
                    SystemEventController.Instance.DispatchSystemEvent(EventAICommandSumarizeMeetingCompleted, _meeting, _meetingSummary);
                }
            }

            _promptBuilder = null;
            _meeting = null;
            _meetingSummary = null;
            _interactions = null;
        }

        private void AskLLM(string prompt)
        {
            if (_confirmation)
            {
                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("ai.processing.now.please.wait"));
            }
            WorkDayData.Instance.AskWorkDayAIMeetingSummary(prompt, true, EventAICommandSumarizeMeetingResponse);
        }

        private void BuildPromptSummarizeMeeting()
        {
            TaskItemData task = null;
            TaskItemData linkedTask = null;
            if (_meeting.TaskId != -1)
            {
                var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_meeting.TaskId);
                task = taskItemData;
                if ((task != null) && (task.Linked != -1))
                {
                    var (taskLinkedData, boardLinkedName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(task.Linked);
                    linkedTask = taskLinkedData;
                }
            }

            string question = PromptController.Instance.GetText("ai.command.summarize.meeting.conversation",
                "<" + PromptController.Instance.GetText("xml.tag.conversation") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.participants") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.meeting") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.task") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.previous.task") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.documents") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.sprint") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.project") + ">");

            switch (PromptController.Instance.CodeLanguage)
            {
                case PromptController.CodeLanguageEnglish:
                    question += JSONDataEnglish.summaryMeetingJsonString;
                    break;
                case PromptController.CodeLanguageSpanish:
                    question += JSONDataSpanish.summaryMeetingJsonString;
                    break;
                case PromptController.CodeLanguageGerman:
                    question += JSONDataGerman.summaryMeetingJsonString;
                    break;
                case PromptController.CodeLanguageFrench:
                    question += JSONDataFrench.summaryMeetingJsonString;
                    break;
                case PromptController.CodeLanguageItalian:
                    question += JSONDataItalian.summaryMeetingJsonString;
                    break;
                case PromptController.CodeLanguageRussian:
                    question += JSONDataRussian.summaryMeetingJsonString;
                    break;
                case PromptController.CodeLanguageCatalan:
                    question += JSONDataCatalan.summaryMeetingJsonString;
                    break;
            }

            _promptBuilder = new PromptBuilder(question);

            // CONVERSATION LOG "xml.tag.conversation.log"
            string interactionContent = "\n\n";
            List<InteractionData> interactions = _meeting.GetInteractions();
            for (int k = 0; k < interactions.Count; k++)
            {
                InteractionData interaction = interactions[k];
                interactionContent += "<" + PromptController.Instance.GetText("xml.tag.reply")
                            + " " + PromptController.Instance.GetText("xml.tag.participant") + "=\"" + interaction.NameActor + "\""
                            + " " + PromptController.Instance.GetText("xml.tag.order") + "=\"" + k + "\""
                            + " " + PromptController.Instance.GetText("xml.tag.reply") + "=\"" + PromptController.Instance.ReplaceConflictiveCharacters(interaction.Text) + "\""
                            + " " + PromptController.Instance.GetText("xml.tag.additional.data") + "=\"" + PromptController.Instance.ReplaceConflictiveCharacters(interaction.Summary) + "\""
                            + "/>";
                interactionContent += "\n";
            }
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.conversation") + ">", 
                                                "<" + PromptController.Instance.GetText("xml.tag.conversation") + ">",
                                                "</" + PromptController.Instance.GetText("xml.tag.conversation") + ">"),
                                                interactionContent);

            // PARTICIPANTS "xml.tag.participants"
            string participantsContent = "\n";
            List<string> assistants = _meeting.GetAssistingMembers(true);
            foreach(string assistant in assistants)
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
                string humanDescription = humanData.Data;
                if (!_meeting.IsSocialMeeting()) humanDescription = humanData.GetSkills();
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

            // MEETING "xml.tag.meeting"
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.meeting") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.meeting") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + _meeting.Name + "\">",
                                                "</" + PromptController.Instance.GetText("xml.tag.meeting") + ">"),
                                                PromptController.Instance.ReplaceConflictiveCharacters(_meeting.Description));

            // TASK "xml.tag.task"
            List<DocumentData> localDocuments = null;
            if (task != null)
            {
                _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.task") + ">",
                                                    "<" + PromptController.Instance.GetText("xml.tag.task") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + task.Name + "\">",
                                                    "</" + PromptController.Instance.GetText("xml.tag.task") + ">"),
                                                    PromptController.Instance.ReplaceConflictiveCharacters(task.Description));
                localDocuments = task.GetData();

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
            }
            else
            {
                localDocuments = _meeting.GetData();

                _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.previous.task") + ">",
                                                    "<" + PromptController.Instance.GetText("xml.tag.previous.task") + ">",
                                                    "</" + PromptController.Instance.GetText("xml.tag.previous.task") + ">"),
                                                    "");
                _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.task") + ">",
                                                    "<" + PromptController.Instance.GetText("xml.tag.task") + ">",
                                                    "</" + PromptController.Instance.GetText("xml.tag.task") + ">"),
                                                    "");
            }

            // DOCUMENTS "xml.tag.documents"
            List<DocumentData> globalDocs = WorkDayData.Instance.CurrentProject.GetDocuments();
            string documentsContent = "\n";
            foreach (DocumentData document in globalDocs)
            {
                if (_meeting.ProjectId == document.ProjectId)
                {
                    bool shouldAdd = true;
                    if (task != null)
                    {
                        if (task.Feature > 0)
                        {
                            if ((document.FeatureID > 0) && (task.Feature != document.FeatureID))
                            {
                                shouldAdd = false;
                            }
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
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.documents") + ">", 
                                                "<" + PromptController.Instance.GetText("xml.tag.documents") + ">",
                                                "</" + PromptController.Instance.GetText("xml.tag.documents") + ">"),
                                                documentsContent);

            // PROJECT "xml.tag.project"
            ProjectInfoData projectInfo = WorkDayData.Instance.CurrentProject.GetProject(_meeting.ProjectId);
            _promptBuilder.SetPromptColor(Color.white);
            _promptBuilder.SetProjectFeedback("");
            if (projectInfo != null)
            {
                _promptBuilder.SetPromptColor(projectInfo.GetColor());
                _promptBuilder.SetProjectFeedback(projectInfo.Name + " : " + _meeting.Name);
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
            else
            {
                _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                                                    "<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                                                    "</" + PromptController.Instance.GetText("xml.tag.project") + ">"),
                                                    "");

                // SPRINT (FEATURES) "xml.tag.sprint"
                question += "</" + PromptController.Instance.GetText("xml.tag.sprint") + ">";
                _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.sprint") + ">",
                                                    "<" + PromptController.Instance.GetText("xml.tag.sprint") + ">",
                                                    "</" + PromptController.Instance.GetText("xml.tag.sprint") + ">"),
                                                    "");
            }
        }
        
        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventAICommandSumarizeMeetingRequest))
            {
                AskLLM((string)parameters[0]);
            }
            if (nameEvent.Equals(EventAICommandSumarizeMeetingResponse))
            {
                if (_confirmation)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                }                
                if ((bool)parameters[0])
                {
                    string summaryMeeting = (string)parameters[1];
#if UNITY_EDITOR
                    Debug.Log("MEETING SUMMARY RECEIVED=" + summaryMeeting);
#endif                     
                    if (!JSONDataFormatValidator.ValidateJsonItem<MeetingSummaryJSON>(summaryMeeting))
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
                            ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLongInput, null, title, "", EventAICommandSumarizeMeetingPromptRetry);
                            UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, summaryMeeting);
                            if (GameObject.FindAnyObjectByType<CodeEditor>() != null)
                            {
                                GameObject.FindAnyObjectByType<CodeEditor>().Refresh();
                            }
                        }
                        return;
                    }
                    
                    _meetingSummary = JsonUtility.FromJson<MeetingSummaryJSON>(summaryMeeting);
                    
                    // FIX WRONG NAMES BECAUSE AI WILL FAIL
                    foreach (DocumentMeetingJSON doc in _meetingSummary.documents)
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
                        ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, title, description, EventAICommandSumarizeMeetingPromptRetry);
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
            if (nameEvent.Equals(EventAICommandSumarizeMeetingPromptConfirmation))
            {                
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
#if !SHORTCUT_LLM_SERVER
                    WorkDayData.Instance.AddTimeSession();
#endif
                    SystemEventController.Instance.DelaySystemEvent(EventAICommandSumarizeMeetingRequest, 0.2f, (string)parameters[2]);
                }
                else
                {
                    _isCompleted = true;
                }
            }
            if (nameEvent.Equals(EventAICommandSumarizeMeetingPromptRetry))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
                    string title = LanguageController.Instance.GetText("ai.title.meeting.reply");
                    ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandSumarizeMeetingPromptConfirmation);
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