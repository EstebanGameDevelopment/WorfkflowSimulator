using UnityEngine;
using yourvrexperience.Utils;
using System;
using System.Collections.Generic;
using InGameCodeEditor;
using static yourvrexperience.WorkDay.MeetingController;

namespace yourvrexperience.WorkDay
{
    public class AICommandReplyText : IAICommand
    {
        public const int LIMIT_TOTAL_CONVERSATION_CONTRIBUTIONS = 10;

        public const string EventAICommandReplyTextRequest_DEFINITION = "EventAICommandReplyTextRequest";
        public const string EventAICommandReplyTextResponse_DEFINITION = "EventAICommandReplyTextResponse";
        public const string EventAICommandReplyTextPromptConfirmation_DEFINITION = "EventAICommandReplyTextPromptConfirmation";
        public const string EventAICommandReplyTextPromptRetry_DEFINITION = "EventAICommandReplyTextPromptRetry";
        public const string EventAICommandReplyAdditionalDocumentGenerated_DEFINITION = "EventAICommandReplyAdditionalDocumentGenerated";

        private string EventAICommandReplyTextRequest = "";
        private string EventAICommandReplyTextResponse = "";
        private string EventAICommandReplyTextPromptConfirmation = "";
        private string EventAICommandReplyTextPromptRetry = "";
        private string EventAICommandReplyAdditionalDocumentGenerated = "";

        private bool _isCompleted = false;
        private MeetingData _meeting;
        private string _outputEvent;
        private PromptBuilder _promptBuilder;
        private bool _confirmation;
        private string _eventDocumentCompleted;
        private ReplyMeetingJSON _replyCreated;
        private DocumentMeetingJSON _docAdditional;
        private int _iterationsToCancel = WorkDayData.TOTAL_RETRIES_AI_FAILED;

        public string Name
        {
            get
            {
                if (_meeting != null)
                {
                    return "ReplyText(" + Utilities.ShortenText(_meeting.Name, 20) + ")";
                }
                else
                {
                    return "ReplyText(NO MEETING)";
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
            _replyCreated = null;
            _docAdditional = null;

            EventAICommandReplyTextRequest = EventAICommandReplyTextRequest_DEFINITION  + _meeting.GetUID();
            EventAICommandReplyTextResponse = EventAICommandReplyTextResponse_DEFINITION + _meeting.GetUID();
            EventAICommandReplyTextPromptConfirmation = EventAICommandReplyTextPromptConfirmation_DEFINITION + _meeting.GetUID();
            EventAICommandReplyTextPromptRetry = EventAICommandReplyTextPromptRetry_DEFINITION + _meeting.GetUID();
            EventAICommandReplyAdditionalDocumentGenerated = EventAICommandReplyAdditionalDocumentGenerated_DEFINITION + _meeting.GetUID();

            string title = LanguageController.Instance.GetText("ai.title.meeting.reply");
            BuildPromptReplyText();
            if (_confirmation)
            {
                SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
                ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandReplyTextPromptConfirmation);
                UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewInitValues, _promptBuilder);
                UIEventController.Instance.DelayUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRefreshInput, 0.1f);
            }
            else
            {
                List<string> existingTags = null;
                if (_promptBuilder.GetMeetingUID() != null)
                {
                    MeetingInProgress meetingIn = MeetingController.Instance.GetMeetingInProgressByUID(_promptBuilder.GetMeetingUID());
                    if (meetingIn != null)
                    {
                        existingTags = meetingIn.Tags;
                    }
                }
                List<string> allTags = _promptBuilder.GetAllTags();
                if (existingTags != null)
                {
                    for (int i = 0; i < allTags.Count; i++)
                    {
                        string currTag = allTags[i];
                        if (!existingTags.Contains(currTag))
                        {
                            _promptBuilder.RemoveTag(currTag);
                        }
                    }
                }
                AskLLM(_promptBuilder.BuildPrompt());
            }
        }

        public void Destroy()
        {
            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
            if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

            SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, true);

            if (_replyCreated != null)
            {
                string participant = _replyCreated.participant;
                string reply = _replyCreated.reply;
                string data = "";
                if (_docAdditional != null)
                {
                    data = _docAdditional.data;
                }
                if (_replyCreated.end == 1)
                {
                    if (_meeting.HasPlayer(true))
                    {
                        SystemEventController.Instance.DispatchSystemEvent(ScreenDialogView.EventScreenDialogViewDisableBecauseMeetingFinished, _meeting);
                    }
                    else
                    {
                        _meeting.Iterations = 1000000000;
                    }                    
                }
                SystemEventController.Instance.DispatchSystemEvent(_outputEvent, _meeting, (_replyCreated.end == 1), participant, reply, data, "");
            }
            else
            {
                SystemEventController.Instance.DispatchSystemEvent(_outputEvent, _meeting);
            }

            _promptBuilder = null;
            _meeting = null;
            _replyCreated = null;
            _docAdditional = null;
        }

        private void AskLLM(string prompt)
        {
            if (_confirmation)
            {
                GameObject loadingGO = ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("ai.processing.now.please.wait"));
                UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationIgnoreDestruction, loadingGO, true);                                
            }
            string sentenceIterations = PromptController.Instance.GetText("ai.command.reply.meeting.iterations.sentence");
            int indexOfIterations = prompt.IndexOf(sentenceIterations);
            if (indexOfIterations != -1)
            {
                string bufferIterations = prompt.Substring(indexOfIterations + sentenceIterations.Length, 5);
                bufferIterations = bufferIterations.Trim();
                string finalIterations = "";
                for (int i = 0; i < bufferIterations.Length; i++)
                {
                    string sdigit = "" + bufferIterations[i];
                    int valueInt = 0;
                    if (int.TryParse(sdigit, out valueInt))
                    {
                        finalIterations += sdigit;
                    }
                }
                int totalFinalIterations = 0;
                if (int.TryParse(finalIterations, out totalFinalIterations))
                {
                    if (totalFinalIterations > LIMIT_TOTAL_CONVERSATION_CONTRIBUTIONS)
                    {
                        totalFinalIterations = LIMIT_TOTAL_CONVERSATION_CONTRIBUTIONS;
                    }
                    _meeting.TotalIterations = totalFinalIterations;
                }
            }
            WorkDayData.Instance.AskWorkDayAIReplyTextMeeting(prompt, true, EventAICommandReplyTextResponse + _meeting.GetUID());
        }

        private void BuildPromptReplyText()
        {
            DateTime currTime = WorkDayData.Instance.CurrentProject.GetCurrentTime();

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

            string meetingType = "";
            if (_meeting.IsSocialMeeting())
            {
                meetingType = PromptController.Instance.GetText("xml.tag.social.meeting");                
            }
            else
            {
                meetingType = PromptController.Instance.GetText("xml.tag.work.meeting");
            }

            string question = PromptController.Instance.GetText("ai.command.reply.meeting.of.task",
                meetingType,
                "<" + PromptController.Instance.GetText("xml.tag.meeting") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.conversation") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.participants") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.previous.meetings") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.task") + ">",                
                "<" + PromptController.Instance.GetText("xml.tag.previous.task") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.documents") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.sprint") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                _meeting.TotalIterations.ToString()
                );

            switch (PromptController.Instance.CodeLanguage)
            {
                case PromptController.CodeLanguageEnglish:
                    question += JSONDataEnglish.replyMeetingJsonString;
                    break;
                case PromptController.CodeLanguageSpanish:
                    question += JSONDataSpanish.replyMeetingJsonString;
                    break;
                case PromptController.CodeLanguageGerman:
                    question += JSONDataGerman.replyMeetingJsonString;
                    break;
                case PromptController.CodeLanguageFrench:
                    question += JSONDataFrench.replyMeetingJsonString;
                    break;
                case PromptController.CodeLanguageItalian:
                    question += JSONDataItalian.replyMeetingJsonString;
                    break;
                case PromptController.CodeLanguageRussian:
                    question += JSONDataRussian.replyMeetingJsonString;
                    break;
                case PromptController.CodeLanguageCatalan:
                    question += JSONDataCatalan.replyMeetingJsonString;
                    break;
            }

            _promptBuilder = new PromptBuilder(question);
            _promptBuilder.SetMeetingUID(_meeting.GetUID());

            // MEETING "xml.tag.meeting"
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.meeting") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.meeting") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + _meeting.Name + "\" " + PromptController.Instance.GetText("xml.tag.date") + " =\"" + currTime.ToLongDateString() + "\">",
                                                "</" + PromptController.Instance.GetText("xml.tag.meeting") + ">"),
                                                PromptController.Instance.ReplaceConflictiveCharacters(_meeting.Description));

            // CONVERSATION LOG "xml.tag.conversation.log"
            string interactionContent = "";
            List<InteractionData> interactions = _meeting.GetInteractions();
            if (interactions.Count == 0)
            {
                interactionContent += PromptController.Instance.GetText("ai.command.reply.meeting.findout.who.starts", _meeting.GetMembersPacket());
                interactionContent += "\n";
            }
            else
            {
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
            }
            string finalInteractionContent = interactionContent;
            if (finalInteractionContent.Length > 0)
            {
                finalInteractionContent = "\n\n" + finalInteractionContent;
            }
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.conversation") + ">", 
                                                "<" + PromptController.Instance.GetText("xml.tag.conversation") + ">",
                                                "</" + PromptController.Instance.GetText("xml.tag.conversation") + ">"),
                                                finalInteractionContent);

            // PARTICIPANTS "xml.tag.participants"
            string participantsContent = "";
            List<string> assistants = _meeting.GetAssistingMembers(true);
            foreach (string assistant in assistants)
            {
                WorldItemData humanData = WorkDayData.Instance.CurrentProject.GetItemByName(assistant);

                bool humanControlled = false;
                if (ApplicationController.Instance.HumanPlayer != null)
                {
                    if (ApplicationController.Instance.HumanPlayer.NameHuman.Equals(assistant))
                    {
                        humanControlled = true;
                    }
                }

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
                if (humanData.IsAsshole) humanDescription = humanData.Data;
                if (groupOfAssistant != null)
                {
                    participantsContent += "<" + PromptController.Instance.GetText("xml.tag.participant")
                                + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + assistant + "\""
                                + " " + PromptController.Instance.GetText("xml.tag.group") + "=\"" + groupOfAssistant.Name + "\""
                                + " " + PromptController.Instance.GetText("xml.tag.category") + "=\"" + category + "\""
                                + " " + PromptController.Instance.GetText("xml.tag.human.controlled") + "=\"" + humanControlled + "\""
                                + " " + PromptController.Instance.GetText("xml.tag.description") + "=\"" + PromptController.Instance.ReplaceConflictiveCharacters(humanDescription) + "\""
                                + "/>";
                    participantsContent += "\n";
                }
                else
                {
                    participantsContent += "<" + PromptController.Instance.GetText("xml.tag.participant")
                                + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + assistant + "\""
                                + " " + PromptController.Instance.GetText("xml.tag.category") + "=\"" + category + "\""
                                + " " + PromptController.Instance.GetText("xml.tag.human.controlled") + "=\"" + humanControlled + "\""
                                + " " + PromptController.Instance.GetText("xml.tag.description") + "=\"" + PromptController.Instance.ReplaceConflictiveCharacters(humanDescription) + "\""
                                + "/>";
                    participantsContent += "\n";
                }
            }
            string finalParticipantsContent = participantsContent;
            if (finalParticipantsContent.Length > 0)
            {
                finalParticipantsContent = "\n" + finalParticipantsContent;
            }
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.participants") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.participants") + ">",
                                                "</" + PromptController.Instance.GetText("xml.tag.participants") + ">"),
                                                finalParticipantsContent);

            // DATE "xml.tag.date"
            if (!_meeting.IsSocialMeeting())
            {
                if (task != null)
                {
                    _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.previous.meetings") + ">",
                                                        "<" + PromptController.Instance.GetText("xml.tag.previous.meetings") + ">",
                                                        "</" + PromptController.Instance.GetText("xml.tag.previous.meetings") + ">"),
                                                        "");
                }
                else
                {
                    // PREVIOUS MEETINGS "xml.tag.previous.meetings"            
                    string packMeetings = WorkDayData.Instance.CurrentProject.PackMeetings(PromptController.Instance.GetText("xml.tag.meeting"),
                                                                                            true, 
                                                                                            false,
                                                                                            false,
                                                                                            currTime);
                    string finalPackMeetings = packMeetings;
                    if (finalPackMeetings.Length > 0)
                    {
                        finalPackMeetings = "\n" + finalPackMeetings;
                    }
                    _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.previous.meetings") + ">",
                                                        "<" + PromptController.Instance.GetText("xml.tag.previous.meetings") + ">",
                                                        "</" + PromptController.Instance.GetText("xml.tag.previous.meetings") + ">"),
                                                        finalPackMeetings);
                }
            }
            else
            {
                // PREVIOUS MEETINGS "xml.tag.previous.meetings"            
                string packMeetings = WorkDayData.Instance.CurrentProject.PackMeetings(PromptController.Instance.GetText("xml.tag.meeting"),
                                                                                        true, 
                                                                                        false,
                                                                                        true,
                                                                                        currTime);
                string finalPackMeetings = packMeetings;
                if (finalPackMeetings.Length > 0)
                {
                    finalPackMeetings = "\n" + finalPackMeetings;
                }
                _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.previous.meetings") + ">",
                                                    "<" + PromptController.Instance.GetText("xml.tag.previous.meetings") + ">",
                                                    "</" + PromptController.Instance.GetText("xml.tag.previous.meetings") + ">"),
                                                    finalPackMeetings);
            }

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

                // DOCUMENTS "xml.tag.documents"
                List<DocumentData> globalDocs = WorkDayData.Instance.CurrentProject.GetDocuments();
                string documentsContent = "\n";
                foreach (DocumentData document in globalDocs)
                {
                    if (_meeting.ProjectId == document.ProjectId)
                    {
                        documentsContent += "<" + PromptController.Instance.GetText("xml.tag.doc")
                                    + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + document.Name
                                    + "\" " + PromptController.Instance.GetText("xml.tag.description") + "=\"" + PromptController.Instance.ReplaceConflictiveCharacters(document.Summary)
                                    + "\"/>";
                        documentsContent += "\n";
                    }
                }
                foreach (DocumentData document in localDocuments)
                {
                    if (!globalDocs.Contains(document))
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
                
                if ((localDocuments != null) && (localDocuments.Count > 0))
                {
                    string documentsContent = "\n";
                    foreach (DocumentData document in localDocuments)
                    {
                        documentsContent += "<" + PromptController.Instance.GetText("xml.tag.doc")
                                    + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + document.Name
                                    + "\" " + PromptController.Instance.GetText("xml.tag.description") + "=\"" + PromptController.Instance.ReplaceConflictiveCharacters(document.Summary)
                                    + "\"/>";
                        documentsContent += "\n";
                    }
                    _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.documents") + ">",
                                                        "<" + PromptController.Instance.GetText("xml.tag.documents") + ">",
                                                        "</" + PromptController.Instance.GetText("xml.tag.documents") + ">"),
                                                        documentsContent);
                }
                else
                {
                    _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.documents") + ">",
                                                        "<" + PromptController.Instance.GetText("xml.tag.documents") + ">",
                                                        "</" + PromptController.Instance.GetText("xml.tag.documents") + ">"),
                                                        "");
                }
            }

            // PROJECT "xml.tag.project"
            ProjectInfoData projectInfo = WorkDayData.Instance.CurrentProject.GetProject(_meeting.ProjectId);
            _promptBuilder.SetPromptColor(Color.white);
            _promptBuilder.SetProjectFeedback("");
            if ((projectInfo != null) && !_meeting.IsSocialMeeting())
            {
                _promptBuilder.SetPromptColor(projectInfo.GetColor());
                _promptBuilder.SetProjectFeedback(projectInfo.Name + " : " + _meeting.Name);

                _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                                                    "<" + PromptController.Instance.GetText("xml.tag.project") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + projectInfo.Name + "\">",
                                                    "</" + PromptController.Instance.GetText("xml.tag.project") + ">"),
                                                    PromptController.Instance.ReplaceConflictiveCharacters(projectInfo.Description));

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

            // INITIALIZE ALL TAGS FOR THE MEETING
            if (interactions.Count == 0)
            {
                List<string> enabledTagsInitial = _promptBuilder.GetEnabledTags();
                foreach (string tag in enabledTagsInitial)
                {
                    SystemEventController.Instance.DispatchSystemEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewAddedTag, _meeting.GetUID(), tag);
                }
            }
        }
        
        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventAICommandReplyTextRequest))
            {
                AskLLM((string)parameters[0]);
            }
            if (nameEvent.Equals(EventAICommandReplyTextResponse + _meeting.GetUID()))
            {
                if (_confirmation)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                }                
                if ((bool)parameters[0])
                {
                    string jsonString = (string)parameters[1];
                    // string summaryData = JSONDataFormatValidator.CleanJsonResponse(jsonString);
                    string replyData = jsonString;
#if UNITY_EDITOR
                    Debug.Log("REPLY RECEIVED=" + replyData);
#endif
                    if (!JSONDataFormatValidator.ValidateJsonItem<ReplyMeetingJSON>(replyData))
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
                            ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLongInput, null, title, "", EventAICommandReplyTextPromptRetry);
                            UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, replyData);
                            if (GameObject.FindAnyObjectByType<CodeEditor>() != null)
                            {
                                GameObject.FindAnyObjectByType<CodeEditor>().Refresh();
                            }
                        }
                        return;
                    }

                    _replyCreated = JsonUtility.FromJson<ReplyMeetingJSON>(replyData);
                    _isCompleted = true;
                }
                else
                {
                    if (_confirmation)
                    {
                        string title = LanguageController.Instance.GetText("text.error");
                        string description = LanguageController.Instance.GetText("ai.title.error.prompt");
                        ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, title, description, EventAICommandReplyTextPromptRetry);
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
            if (nameEvent.Equals(_eventDocumentCompleted))
            {
                if (_meeting == (MeetingData)parameters[0])
                {
                    string docDataAdditional = "";
                    if ((bool)parameters[1])
                    {
                        DocumentMeetingJSON docAdditional = (DocumentMeetingJSON)parameters[2];
                        docDataAdditional = docAdditional.data;
                    }
                    SystemEventController.Instance.DispatchSystemEvent(CommandTalkInMeeting.EventCommandTalkInMeetingResponse_DEFINITION + _meeting.GetUID(), _meeting, _replyCreated.participant, _replyCreated.reply, docDataAdditional, "");
                    _isCompleted = true;
                }
            }
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventAICommandReplyTextPromptConfirmation))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
#if !SHORTCUT_LLM_SERVER
                    WorkDayData.Instance.AddTimeSession();
#endif
                    SystemEventController.Instance.DelaySystemEvent(EventAICommandReplyTextRequest, 0.2f, (string)parameters[2]);
                }
                else
                {
                    _isCompleted = true;
                }
            }
            if (nameEvent.Equals(EventAICommandReplyTextPromptRetry))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
                    string title = LanguageController.Instance.GetText("ai.title.meeting.reply");
                    ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandReplyTextPromptConfirmation);
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