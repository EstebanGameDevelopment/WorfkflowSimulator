using UnityEngine;
using yourvrexperience.Utils;
using System;
using static yourvrexperience.WorkDay.ApplicationController;
using System.Collections.Generic;
using InGameCodeEditor;

namespace yourvrexperience.WorkDay
{
    public class AICommandSummarizeDocs : IAICommand
    {
        public const string EventAICommandSummarizeDocsActivate_DEFINITION = "EventAICommandSummarizeDocsActivate";
        public const string EventAICommandSummarizeDocsRequestDocSummary_DEFINITION = "EventAICommandSummarizeDocsRequestDocSummary";
        public const string EventAICommandSummarizeDocsResponseDocSummary_DEFINITION = "EventAICommandSummarizeDocsResponseDocSummary";
        public const string EventAICommandSummarizeDocsCompleted = "EventAICommandSummarizeDocsCompleted";
        public const string EventAICommandSummarizeDocsAllCompleted = "EventAICommandSummarizeDocsAllCompleted";
        
        public const string EventAICommandSummarizeRequestDocsPromptConfirmation_DEFINITION = "EventAICommandSummarizeRequestDocsPromptConfirmation";
        public const string EventAICommandSummarizeRequestDocsRetryPrompt_DEFINITION = "EventAICommandSummarizeRequestDocsRetryPrompt";

        private string EventAICommandSummarizeDocsActivate = "";
        private string EventAICommandSummarizeDocsRequestDocSummary = "";
        private string EventAICommandSummarizeDocsResponseDocSummary = "";

        private string EventAICommandSummarizeRequestDocsPromptConfirmation = "";
        private string EventAICommandSummarizeRequestDocsRetryPrompt = "";

        private bool _isCompleted = false;
        private bool _activated = false;
        private string _finalQuestion = "";
        private string _outputEvent = "";

        List<DocumentData> _docsToSummarize;
        private DocumentData _processDocument = null;
        private PromptBuilder _promptBuilder = null;
        private bool _confirmation = false;
        private int _uniqueID = 0;
        private int _projectID;
        private int _iterationsToCancel = WorkDayData.TOTAL_RETRIES_AI_FAILED;

        public string Name
        {
            get
            {
                if (_processDocument != null)
                {
                    return "Summarize(" + Utilities.ShortenText(_processDocument.Name, 10) + ")["+ Utilities.ShortenText(_processDocument.Data.GetHTML(), 30) + "]";
                }
                else
                {
                    return "Summarize(NO MEETING)";
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

            List<DocumentData> docsChanged = (List<DocumentData>)parameters[0];
            _outputEvent = (string)parameters[1];
            _docsToSummarize = new List<DocumentData>();
            _uniqueID = 0;            
            foreach (DocumentData doc in docsChanged)
            {
                doc.IsChanged = false;
                _docsToSummarize.Add(doc);
                _uniqueID += doc.Id;
                _projectID = doc.ProjectId;
            }
            if (docsChanged.Count > 0)
            {
                _isCompleted = false;
            }
            else
            {
                _isCompleted = true;
            }

            EventAICommandSummarizeDocsActivate = EventAICommandSummarizeDocsActivate_DEFINITION + _uniqueID;
            EventAICommandSummarizeDocsRequestDocSummary = EventAICommandSummarizeDocsRequestDocSummary_DEFINITION + _uniqueID;
            EventAICommandSummarizeDocsResponseDocSummary = EventAICommandSummarizeDocsResponseDocSummary_DEFINITION + _uniqueID;

            EventAICommandSummarizeRequestDocsPromptConfirmation = EventAICommandSummarizeRequestDocsPromptConfirmation_DEFINITION  + _uniqueID;
            EventAICommandSummarizeRequestDocsRetryPrompt = EventAICommandSummarizeRequestDocsRetryPrompt_DEFINITION + _uniqueID;

            if (!_isCompleted)
            {
                if (_confirmation)
                {
                    SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
                    ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("ai.preparing.to.summarize"));
                    SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, false);
                }
                SystemEventController.Instance.DelaySystemEvent(EventAICommandSummarizeDocsActivate, 1);
            }
        }

        public void Destroy()
        {
            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
            if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

            SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, true);
            SystemEventController.Instance.DispatchSystemEvent(EventAICommandSummarizeDocsAllCompleted);

            _finalQuestion = "";
            _promptBuilder = null;
            _processDocument = null;
        }

        private void BuildPromptSummaryText(DocumentData doc)
        {
            string question = PromptController.Instance.GetText("ai.command.summarize.doc",
                "<" + PromptController.Instance.GetText("xml.tag.doc") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.sprint") + ">");
            switch (PromptController.Instance.CodeLanguage)
            {
                case PromptController.CodeLanguageEnglish:
                    question += JSONDataEnglish.documentSummaryJsonString;
                    break;
                case PromptController.CodeLanguageSpanish:
                    question += JSONDataSpanish.documentSummaryJsonString;
                    break;
                case PromptController.CodeLanguageGerman:
                    question += JSONDataGerman.documentSummaryJsonString;
                    break;
                case PromptController.CodeLanguageFrench:
                    question += JSONDataFrench.documentSummaryJsonString;
                    break;
                case PromptController.CodeLanguageItalian:
                    question += JSONDataItalian.documentSummaryJsonString;
                    break;
                case PromptController.CodeLanguageRussian:
                    question += JSONDataRussian.documentSummaryJsonString;
                    break;
                case PromptController.CodeLanguageCatalan:
                    question += JSONDataCatalan.documentSummaryJsonString;
                    break;
            }

            // DOCUMENT
            question += "\n\n";
            question += "<" + PromptController.Instance.GetText("xml.tag.doc") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + doc.Name + "\">";
            question += "\n";
            question += doc.Data.GetHTML();
            question += "\n";
            question += "</" + PromptController.Instance.GetText("xml.tag.doc") + ">";
            _promptBuilder = new PromptBuilder(question);

            
            // PROJECT
            ProjectInfoData projectInfo = WorkDayData.Instance.CurrentProject.GetProject(doc.ProjectId);
            _promptBuilder.SetPromptColor(projectInfo.GetColor());
            _promptBuilder.SetProjectFeedback(projectInfo.Name + " : " + doc.Name);
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.project") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + projectInfo.Name + "\">",
                                                "</" + PromptController.Instance.GetText("xml.tag.project") + ">"),
                                        projectInfo.Description);

            // SPRINT (FEATURES)
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.sprint") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.sprint") + ">",
                                                "</" + PromptController.Instance.GetText("xml.tag.sprint") + ">"),
                                        WorkDayData.Instance.CurrentProject.PackBoardsXML(projectInfo.Id));
        }

        private void BuildPromptTestImage(DocumentData doc)
        {
            string question = PromptController.Instance.GetText("ai.command.describe.test.image");
            question += "\n";
            switch (PromptController.Instance.CodeLanguage)
            {
                case PromptController.CodeLanguageEnglish:
                    question += JSONDataEnglish.documentSummaryJsonString;
                    break;
                case PromptController.CodeLanguageSpanish:
                    question += JSONDataSpanish.documentSummaryJsonString;
                    break;
                case PromptController.CodeLanguageGerman:
                    question += JSONDataGerman.documentSummaryJsonString;
                    break;
                case PromptController.CodeLanguageFrench:
                    question += JSONDataFrench.documentSummaryJsonString;
                    break;
                case PromptController.CodeLanguageItalian:
                    question += JSONDataItalian.documentSummaryJsonString;
                    break;
                case PromptController.CodeLanguageRussian:
                    question += JSONDataRussian.documentSummaryJsonString;
                    break;
                case PromptController.CodeLanguageCatalan:
                    question += JSONDataCatalan.documentSummaryJsonString;
                    break;
            }
            _promptBuilder = new PromptBuilder(question);
        }

        private void BuildPromptSummaryImage(DocumentData doc)
        {
            string question = PromptController.Instance.GetText("ai.command.summarize.image",
                "<" + PromptController.Instance.GetText("xml.tag.image") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.sprint") + ">");
            question += "\n";
            switch (PromptController.Instance.CodeLanguage)
            {
                case PromptController.CodeLanguageEnglish:
                    question += JSONDataEnglish.documentSummaryJsonString;
                    break;
                case PromptController.CodeLanguageSpanish:
                    question += JSONDataSpanish.documentSummaryJsonString;
                    break;
                case PromptController.CodeLanguageGerman:
                    question += JSONDataGerman.documentSummaryJsonString;
                    break;
                case PromptController.CodeLanguageFrench:
                    question += JSONDataFrench.documentSummaryJsonString;
                    break;
                case PromptController.CodeLanguageItalian:
                    question += JSONDataItalian.documentSummaryJsonString;
                    break;
                case PromptController.CodeLanguageRussian:
                    question += JSONDataRussian.documentSummaryJsonString;
                    break;
                case PromptController.CodeLanguageCatalan:
                    question += JSONDataCatalan.documentSummaryJsonString;
                    break;
            }

            // IMAGE USER'S DESCRIPTION
            question += "\n\n";
            question += "<" + PromptController.Instance.GetText("xml.tag.image") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + doc.Name + "\" " + PromptController.Instance.GetText("xml.tag.description") + "=\"" + PromptController.Instance.ReplaceConflictiveCharacters(doc.Description) + "\"/>";
            question += "\n\n";
            _promptBuilder = new PromptBuilder(question);

            // PROJECT
            ProjectInfoData projectInfo = WorkDayData.Instance.CurrentProject.GetProject(doc.ProjectId);
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.project") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + projectInfo.Name + "\">",
                                                "</" + PromptController.Instance.GetText("xml.tag.project") + ">"),
                                        PromptController.Instance.ReplaceConflictiveCharacters(projectInfo.Description));

            // SPRINT (FEATURES)
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.sprint") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.sprint") + ">",
                                                "</" + PromptController.Instance.GetText("xml.tag.sprint") + ">"),
                                        WorkDayData.Instance.CurrentProject.PackBoardsXML(projectInfo.Id));
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventAICommandSummarizeDocsActivate))
            {
                if (_confirmation)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                }                
                _activated = true;
            }
            if (nameEvent.Equals(ImageDatabaseController.EventImageDatabaseControllerAvailableImage))
            {
                byte[] dataImage = ImageDatabaseController.Instance.GetImageDataByID(_processDocument.GetImageID());
                string base64String = Convert.ToBase64String(dataImage);
                WorkDayData.Instance.AskWorkDayAIDocSummaryImage(_finalQuestion, base64String, true, EventAICommandSummarizeDocsResponseDocSummary + _uniqueID);
            }
            if (nameEvent.Equals(EventAICommandSummarizeDocsRequestDocSummary))
            {
                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("ai.processing.now.please.wait"));
                
                if (_processDocument.IsImage)
                {
                    _finalQuestion = (string)parameters[0];
                    SystemEventController.Instance.DispatchSystemEvent(ImageDatabaseController.EventImageDatabaseControllerDownloadImage, _processDocument.GetImageID(), true);
                }
                else
                {
                    WorkDayData.Instance.AskWorkDayAIDocSummaryText((string)parameters[0], true, EventAICommandSummarizeDocsResponseDocSummary + _uniqueID);
                }                
            }
            if (nameEvent.Equals(EventAICommandSummarizeDocsResponseDocSummary + _uniqueID))
            {
                if (_confirmation)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                }                
                if ((bool)parameters[0])
                {
                    string jsonString = (string)parameters[1];
                    // string summaryData = JSONDataFormatValidator.CleanJsonResponse(jsonString);
                    string summaryData = jsonString;
#if UNITY_EDITOR
                    Debug.Log("SUMMARY RECEIVED=" + summaryData);
#endif
                    if (!JSONDataFormatValidator.ValidateJsonItem<DocumentSummaryJSON>(summaryData))
                    {
                        if (!_confirmation)
                        {
                            _iterationsToCancel--;
                            if (_iterationsToCancel <= 0)
                            {
                                _isCompleted = true;
                                _processDocument = null;
                                if (_outputEvent.Length > 0)
                                {
                                    SystemEventController.Instance.DispatchSystemEvent(_outputEvent, false);
                                }
                                else
                                {
                                    SystemEventController.Instance.DispatchSystemEvent(EventAICommandSummarizeDocsCompleted, false);
                                }
                            }
                            else
                            {
                                _confirmation = true;  // IF THE PROMPT FAILS AGAIN, THEN WE WILL ASK THE USER SO HE HAS THE OPTION TO CHOOSE A BETTER LLM MODEL
                                SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
                                SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, false);

                                _docsToSummarize.Insert(0, _processDocument);
                                _processDocument = null;
                                _isCompleted = false;
                            }
                            return;
                        }
                        else
                        {
                            string title = LanguageController.Instance.GetText("ai.title.error.prompt");
                            ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLongInput, null, title, "", EventAICommandSummarizeRequestDocsRetryPrompt);
                            UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, summaryData);
                            if (GameObject.FindAnyObjectByType<CodeEditor>() != null)
                            {
                                GameObject.FindAnyObjectByType<CodeEditor>().Refresh();
                            }
                        }
                        return;
                    }
                    
                    DocumentSummaryJSON summary = JsonUtility.FromJson<DocumentSummaryJSON>(summaryData);
                    _processDocument.Summary = summary.description;
#if UNITY_EDITOR
                    Debug.Log("=================SUMMARY SUCCESS[" + _processDocument.Name + "]=" + _processDocument.Summary);
#endif
                    _processDocument = null;                    
                    if (_outputEvent.Length > 0)
                    {
                        SystemEventController.Instance.DispatchSystemEvent(_outputEvent, true);
                    }
                    else
                    {                        
                        SystemEventController.Instance.DispatchSystemEvent(EventAICommandSummarizeDocsCompleted, true);
                    }                    
                }
                else
                {
                    if (_confirmation)
                    {
                        string title = LanguageController.Instance.GetText("text.error");
                        string description = LanguageController.Instance.GetText("ai.title.error.prompt");
                        ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, title, description, EventAICommandSummarizeRequestDocsRetryPrompt);
                        _isCompleted = false;
                    }
                    else
                    {
                        _iterationsToCancel--;
                        if (_iterationsToCancel <= 0)
                        {
                            _isCompleted = true;
                            _processDocument = null;
                            if (_outputEvent.Length > 0)
                            {
                                SystemEventController.Instance.DispatchSystemEvent(_outputEvent, false);
                            }
                            else
                            {
                                SystemEventController.Instance.DispatchSystemEvent(EventAICommandSummarizeDocsCompleted, false);
                            }
                        }
                        else
                        {
                            _docsToSummarize.Insert(0, _processDocument);
                            _processDocument = null;
                            _isCompleted = false;
                        }
                    }
                }
            }
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventAICommandSummarizeRequestDocsPromptConfirmation))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {                    
#if !SHORTCUT_LLM_SERVER
                    WorkDayData.Instance.AddTimeSession();
#endif
                    SystemEventController.Instance.DelaySystemEvent(EventAICommandSummarizeDocsRequestDocSummary, 0.2f, (string)parameters[2]);
                }
                else
                {
                    _processDocument = null;
                    if (_outputEvent.Length > 0)
                    {
                        SystemEventController.Instance.DispatchSystemEvent(_outputEvent, false);
                    }
                    else
                    {
                        SystemEventController.Instance.DispatchSystemEvent(EventAICommandSummarizeDocsCompleted, false);
                    }
                    _isCompleted = true;
                }
            }
            if (nameEvent.Equals(EventAICommandSummarizeRequestDocsRetryPrompt))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
                    string title = LanguageController.Instance.GetText("ai.title.summarize.doc.text");
                    ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandSummarizeRequestDocsPromptConfirmation);
                    UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewInitValues, _promptBuilder);
                    UIEventController.Instance.DelayUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRefreshInput, 0.1f);
                }
                else
                {
                    _processDocument = null;
                    if (_outputEvent.Length > 0)
                    {
                        SystemEventController.Instance.DispatchSystemEvent(_outputEvent, false);
                    }
                    else
                    {
                        SystemEventController.Instance.DispatchSystemEvent(EventAICommandSummarizeDocsCompleted, false);
                    }
                    _isCompleted = true;
                }
            }
        }

        public void Run()
        {
            if (_activated)
            {
                if (!_isCompleted)
                {
                    if ((_docsToSummarize.Count == 0) && (_processDocument == null))
                    {
                        _isCompleted = true;
                        if (_confirmation)
                        {
                            ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformationImage, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("ai.title.completed.summary.process"), "", "", "", ApplicationController.Instance.GetContentImage(ImagesIndex.TaskCompleted));
                        }                        
                        return;
                    }

                    if ((_docsToSummarize.Count > 0) && (_processDocument == null))
                    {
                        _processDocument = _docsToSummarize[0];
                        _docsToSummarize.RemoveAt(0);

                        if (_processDocument.IsImage)
                        {
                            BuildPromptSummaryImage(_processDocument);

                            if (_confirmation)
                            {
                                string title = LanguageController.Instance.GetText("ai.title.summarize.doc.image");
                                ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandSummarizeRequestDocsPromptConfirmation);
                                UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewInitValues, _promptBuilder);
                                UIEventController.Instance.DelayUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRefreshInput, 0.1f);
                            }
                            else
                            {
                                _finalQuestion = _promptBuilder.BuildPrompt();
                                SystemEventController.Instance.DispatchSystemEvent(ImageDatabaseController.EventImageDatabaseControllerDownloadImage, _processDocument.GetImageID(), true);
                            }
                        }
                        else
                        {
                            BuildPromptSummaryText(_processDocument);

                            if (_confirmation)
                            {
                                string title = LanguageController.Instance.GetText("ai.title.summarize.doc.text");
                                ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandSummarizeRequestDocsPromptConfirmation);
                                UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewInitValues, _promptBuilder);
                                UIEventController.Instance.DelayUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRefreshInput, 0.1f);
                            }
                            else
                            {
                                WorkDayData.Instance.AskWorkDayAIDocSummaryText(_promptBuilder.BuildPrompt(), true, EventAICommandSummarizeDocsResponseDocSummary + _uniqueID);
                            }
                        }
                    }
                }
            }
        }
    }
}