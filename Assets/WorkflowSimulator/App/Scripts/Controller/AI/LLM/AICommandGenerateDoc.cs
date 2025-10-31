using InGameCodeEditor;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using yourvrexperience.ai;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.ScreenImageGenerationView;

namespace yourvrexperience.WorkDay
{
    public class AICommandGenerateDoc : IAICommand
    {
        public const string EventAICommandGenerateDocRequestGeneration_DEFINITION = "EventAICommandGenerateDocRequestGeneration";
        public const string EventAICommandGenerateDocResponseGeneration_DEFINITION = "EventAICommandGenerateDocResponseGeneration";
        public const string EventAICommandGenerateDocGenerationCompleted = "EventAICommandGenerateDocGenerationCompleted";
        
        public const string EventAICommandGenerateDocPromptConfirmation_DEFINITION = "EventAICommandGenerateDocPromptConfirmation";
        public const string EventAICommandGenerateDocRetryPrompt_DEFINITION = "EventAICommandGenerateDocRetryPrompt";
        public const string EventAICommandGenerateDocSummary_DEFINITION = "EventAICommandGenerateDocSummary";

        public const string EventAICommandGenerateDocRefreshState = "EventAICommandGenerateDocRefreshState";

        public const string SubEventAICommandGenerateDocByHuman_DEFINITION = "SubEventAICommandGenerateDocByHuman";

        private string EventAICommandGenerateDocRequestGeneration = "";
        private string EventAICommandGenerateDocResponseGeneration = "";
        
        private string EventAICommandGenerateDocPromptConfirmation = "";
        private string EventAICommandGenerateDocRetryPrompt = "";
        private string EventAICommandGenerateDocSummary = "";

        public string SubEventAICommandGenerateDocByHuman = "";

        private bool _isCompleted = false;

        private PromptBuilder _promptBuilder = null;
        private bool _confirmation = false;
        private CurrentDocumentInProgress _docDefinition;
        private DocumentGeneratedJSON _docGenerated;

        private DocumentData _docByPlayer = null;
        private int _iterationsToCancel = WorkDayData.TOTAL_RETRIES_AI_FAILED;

        public string Name
        {
            get
            {
                if (_docDefinition != null)
                {
                    return "GenerateDoc(" + Utilities.ShortenText(_docDefinition.Name, 10) + ")["+ Utilities.ShortenText(_docDefinition.Description, 30) + "]";
                }
                else
                {
                    return "GenerateDoc(NO DOC)";
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
            _docDefinition = (CurrentDocumentInProgress)parameters[0];
            _docDefinition.IsImage = false;

            EventAICommandGenerateDocRequestGeneration = EventAICommandGenerateDocRequestGeneration_DEFINITION + _docDefinition.GetDocUniqueID();
            EventAICommandGenerateDocResponseGeneration = EventAICommandGenerateDocResponseGeneration_DEFINITION + _docDefinition.GetDocUniqueID();

            EventAICommandGenerateDocPromptConfirmation = EventAICommandGenerateDocPromptConfirmation_DEFINITION + _docDefinition.GetDocUniqueID();
            EventAICommandGenerateDocRetryPrompt = EventAICommandGenerateDocRetryPrompt_DEFINITION + _docDefinition.GetDocUniqueID();

            SystemEventController.Instance.Event += OnSystemEvent;
            UIEventController.Instance.Event += OnUIEvent;

            BuildPromptGenerateText();

            if (_docDefinition.IsAssignedToHumanControlled() || _docDefinition.IsForHuman)
            {
                string titleCreateDoc = LanguageController.Instance.GetText("ai.command.title.generate.doc.for") + " " + _docDefinition.Name;
                SubEventAICommandGenerateDocByHuman = SubEventAICommandGenerateDocByHuman_DEFINITION + _docDefinition.TaskID;
                GameObject screenEditionDoc = ScreenInformationView.CreateScreenInformation(ScreenDocumentsDataView.ScreenName, null, titleCreateDoc, _docDefinition.Description, SubEventAICommandGenerateDocByHuman);
                UIEventController.Instance.DispatchUIEvent(ScreenDocumentsDataView.EventScreenDocumentsDataViewInitialization, _docDefinition.ProjectID, false, new List<DocumentData>());
                UIEventController.Instance.DispatchUIEvent(ScreenDocumentsDataView.EventScreenDocumentsDataViewSetUpDataDocument, _docDefinition.Name, _docDefinition.Description, "", "");
                UIEventController.Instance.DispatchUIEvent(ScreenDocumentsDataView.EventScreenDocumentsDataViewFixAuthor, ApplicationController.Instance.HumanPlayer.NameHuman);
                UIEventController.Instance.DispatchUIEvent(ScreenDocumentsDataView.EventScreenDocumentsDataViewForceAutomaticUpload);

                SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
                SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, false);

                string titleCompleteTask = LanguageController.Instance.GetText("text.complete.task.title");
                string descritionCompleteTask = LanguageController.Instance.GetText("text.complete.task.description");
                GameObject screenInfo = ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, titleCompleteTask, descritionCompleteTask + " " + _docDefinition.Description);
                UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, screenInfo, screenEditionDoc.GetComponent<BaseScreenView>().Canvas.sortingOrder + 1);
            }
            else
            {
                if (_confirmation)
                {
                    SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
                    string title = LanguageController.Instance.GetText("text.request.generation.doc.for.task");
                    ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandGenerateDocPromptConfirmation);
                    UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewInitValues, _promptBuilder);
                    UIEventController.Instance.DelayUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRefreshInput, 0.1f);
                    SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, false);
                }
                else
                {
                    AskLLM(_promptBuilder.BuildPrompt());
                }
            }
        }

        public void Destroy()
        {
            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
            if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

            SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, true);            
            SystemEventController.Instance.DispatchSystemEvent(EventAICommandGenerateDocRefreshState);

            _docDefinition = null;
            _promptBuilder = null;
            _docDefinition = null;
            _docByPlayer = null;
        }
        
        private void AskLLM(string prompt)
        {
            if (_confirmation)
            {
                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("ai.processing.now.please.wait"));
            }
            WorkDayData.Instance.AskWorkDayAIGenerateDocText(prompt, true, EventAICommandGenerateDocResponseGeneration);
        }

        private void BuildPromptGenerateText()
        {
            string question = PromptController.Instance.GetText("ai.command.tasks.create.single.full.document",
                "<" + PromptController.Instance.GetText("xml.tag.short.description") + ">",
                PromptController.Instance.GetText("xml.tag.requirements"),
                PromptController.Instance.GetText("xml.tag.design"),
                PromptController.Instance.GetText("xml.tag.code"),
                PromptController.Instance.GetText("xml.tag.testing"),
                "<" + PromptController.Instance.GetText("xml.tag.task") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.documents") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.sprint") + ">");
            switch (PromptController.Instance.CodeLanguage)
            {
                case PromptController.CodeLanguageEnglish:
                    question += JSONDataEnglish.documentTextGeneratedJsonString;
                    break;
                case PromptController.CodeLanguageSpanish:
                    question += JSONDataSpanish.documentTextGeneratedJsonString;
                    break;
                case PromptController.CodeLanguageGerman:
                    question += JSONDataGerman.documentTextGeneratedJsonString;
                    break;
                case PromptController.CodeLanguageFrench:
                    question += JSONDataFrench.documentTextGeneratedJsonString;
                    break;
                case PromptController.CodeLanguageItalian:
                    question += JSONDataItalian.documentTextGeneratedJsonString;
                    break;
                case PromptController.CodeLanguageRussian:
                    question += JSONDataRussian.documentTextGeneratedJsonString;
                    break;
                case PromptController.CodeLanguageCatalan:
                    question += JSONDataCatalan.documentTextGeneratedJsonString;
                    break;
            }

            // SHORT DESCRIPTION DOCUMENT
            question += "\n\n";
            question += "<" + PromptController.Instance.GetText("xml.tag.short.description") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + _docDefinition.Name + "\" " + PromptController.Instance.GetText("xml.tag.type") + "=\"" + _docDefinition.Type + "\">";
            question += "\n";
            question += PromptController.Instance.ReplaceConflictiveCharacters(_docDefinition.Description);
            question += "\n";
            question += "</" + PromptController.Instance.GetText("xml.tag.short.description") + ">";
            _promptBuilder = new PromptBuilder(question);

            // TASK DESCRIPTION
            var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_docDefinition.TaskID);
            string descriptionTask = taskItemData.Description;
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.task") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.task") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + taskItemData.Name + "\">",
                                                "</" + PromptController.Instance.GetText("xml.tag.task") + ">"),
                                                PromptController.Instance.ReplaceConflictiveCharacters(descriptionTask));


            // DOCUMENTS "xml.tag.documents"
            List<DocumentData> globalDocs = WorkDayData.Instance.CurrentProject.GetDocuments();
            string documentsContent = "\n";
            foreach (DocumentData document in globalDocs)
            {
                if (_docDefinition.ProjectID == document.ProjectId)
                {
                    bool shouldAdd = true;
                    if (taskItemData.Feature > 0)
                    {
                        if ((document.FeatureID > 0) && (taskItemData.Feature != document.FeatureID))
                        {
                            shouldAdd = false;
                        }
                    }
                    if (shouldAdd)
                    {
                        documentsContent += "<" + PromptController.Instance.GetText("xml.tag.doc")
                                + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + document.Name
                                + "\" " + PromptController.Instance.GetText("xml.tag.description") + "=\"" + document.Summary
                                + "\"/>";
                        documentsContent += "\n";
                    }
                }
            }
            List<DocumentData> localDocuments = taskItemData.GetData();
            if (localDocuments != null)
            {
                foreach (DocumentData document in localDocuments)
                {
                    if (!globalDocs.Contains(document))
                    {
                        documentsContent += "<" + PromptController.Instance.GetText("xml.tag.doc")
                                    + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + document.Name
                                    + "\" " + PromptController.Instance.GetText("xml.tag.description") + "=\"" + document.Summary
                                    + "\"/>";
                        documentsContent += "\n";
                    }
                }
            }
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.documents") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.documents") + ">",
                                                "</" + PromptController.Instance.GetText("xml.tag.documents") + ">"),
                                                documentsContent);

            ProjectInfoData projectData = WorkDayData.Instance.CurrentProject.GetProject(_docDefinition.ProjectID);
            _promptBuilder.SetPromptColor(projectData.GetColor());
            _promptBuilder.SetProjectFeedback(projectData.Name + " : " + _docDefinition.Name);

            // PROJECT
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.project") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.project") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + projectData.Name + "\">",
                                                "</" + PromptController.Instance.GetText("xml.tag.project") + ">"),
                                        PromptController.Instance.ReplaceConflictiveCharacters(projectData.Description));

            // SPRINT (FEATURES)
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.sprint") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.sprint") + ">",
                                                "</" + PromptController.Instance.GetText("xml.tag.sprint") + ">"),
                                        WorkDayData.Instance.CurrentProject.PackBoardsXML(projectData.Id));
        }

        private string BuildPromptGenerateImage(DocumentGeneratedJSON docGenerated)
        {
            string question = PromptController.Instance.GetText("ai.command.tasks.create.single.full.image",
                "<" + PromptController.Instance.GetText("xml.tag.image") + ">",
                "<" + PromptController.Instance.GetText("xml.tag.project") + ">");
            question += "\n";

            // IMAGE USER'S DESCRIPTION
            question += "\n\n";
            question += "<" + PromptController.Instance.GetText("xml.tag.image") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + docGenerated.name + "\">";
            question += PromptController.Instance.ReplaceConflictiveCharacters(docGenerated.data);
            question += "<" + PromptController.Instance.GetText("xml.tag.image") + ">";
            question += "\n\n";

            // PROJECT
            ProjectInfoData projectInfo = WorkDayData.Instance.CurrentProject.GetProject(_docDefinition.ProjectID);
            question += "<" + PromptController.Instance.GetText("xml.tag.project") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + projectInfo.Name + "\">";
            question += PromptController.Instance.ReplaceConflictiveCharacters(projectInfo.Description);
            question += "</" + PromptController.Instance.GetText("xml.tag.project") + ">";

            return question;
        }

        private void ConcludeDocGeneration(string data, string description, string owner)
        {
            _docDefinition.StopWorking(data, description, owner);
            SystemEventController.Instance.DispatchSystemEvent(EventAICommandGenerateDocGenerationCompleted, true, _docDefinition);
            _isCompleted = true;
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventAICommandGenerateDocRequestGeneration))
            {
                string finalQuestion = (string)parameters[0];
                AskLLM(finalQuestion);
            }
            if (nameEvent.Equals(ScreenImageGenerationView.EventScreenImageGenerationViewImageCancelled))
            {
                if (_confirmation)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                }
                _docDefinition.StopWorking(_docGenerated.data);
                SystemEventController.Instance.DispatchSystemEvent(EventAICommandGenerateDocGenerationCompleted, true, _docDefinition);
                _isCompleted = true;
            }
            if (nameEvent.Equals(ScreenImageGenerationView.EventScreenImageGenerationViewImageCompleted))
            {
                if (_docGenerated == null) return;

                if (_confirmation)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                }
                if ((bool)parameters[0])
                {
                    if (_confirmation)
                    {
                        ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("ai.processing.now.please.wait"));
                    }
                    byte[] imageBytes = (byte[])parameters[1];
                    int idImage = -1;
                    string finalName = _docDefinition.Name;
                    WorkDayData.Instance.UploadImageData(idImage, WorkDayData.Instance.CurrentProject.ProjectInfoSelected, _docGenerated.name, imageBytes);
                }
                else
                {
                    _docDefinition.IsImage = false;
                    ConcludeDocGeneration(_docGenerated.data, _docDefinition.Description, "");
                }
            }
            if (nameEvent.Equals(UploadImageDataHTTP.EventUploadImageDataHTTPCompleted))
            {
                if (_docGenerated == null) return;

                if (_confirmation)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                }
                if ((bool)parameters[0])
                {
                    int idImage = (int)parameters[1];
                    _docDefinition.IsImage = true;
                    ConcludeDocGeneration(idImage.ToString(), _docGenerated.data, "");
                    ScreenController.Instance.CreateScreen(ScreenImageView.ScreenName, false, false, idImage);                    
                }
                else
                {
                    _docDefinition.IsImage = false;
                    ConcludeDocGeneration(_docGenerated.data, _docDefinition.Description, "");
                }
            }
            if (nameEvent.Equals(EventAICommandGenerateDocResponseGeneration))
            {
                if (_confirmation)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                }
                if ((bool)parameters[0])
                {
                    string jsonString = (string)parameters[1];
                    string docGeneratedData = jsonString;
#if UNITY_EDITOR
                    Debug.Log("DOC GENERATED RECEIVED=" + docGeneratedData);
#endif
                    if (!JSONDataFormatValidator.ValidateJsonItem<DocumentGeneratedJSON>(docGeneratedData))
                    {
                        DocumentGeneratedJSON fixedJSON = DocumentNormalizer.Normalize(docGeneratedData);
                        docGeneratedData = JsonConvert.SerializeObject(fixedJSON, new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
                    }                    
                    if (!JSONDataFormatValidator.ValidateJsonItem<DocumentGeneratedJSON>(docGeneratedData))
                    {
                        if (!_confirmation)
                        {
                            _iterationsToCancel--;
                            if (_iterationsToCancel <= 0)
                            {
                                _isCompleted = true;
                                SystemEventController.Instance.DispatchSystemEvent(EventAICommandGenerateDocGenerationCompleted, false, _docDefinition);                                
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
                            ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLongInput, null, title, "", EventAICommandGenerateDocRetryPrompt);
                            UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, docGeneratedData);
                            if (GameObject.FindAnyObjectByType<CodeEditor>() != null)
                            {
                                GameObject.FindAnyObjectByType<CodeEditor>().Refresh();
                            }
                        }
                        return;
                    }

                    _docGenerated = JsonUtility.FromJson<DocumentGeneratedJSON>(docGeneratedData);
                    if (StringSimilarity.CalculateSimilarityPercentage(PromptController.Instance.GetText("xml.tag.design"), _docDefinition.Type) > 80)
                    {
                        if (ApplicationController.Instance.IsImageAuthorized())
                        {
                            if (_confirmation)
                            {
                                UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                                ScreenController.Instance.CreateScreen(ScreenImageGenerationView.ScreenName, false, false, BuildPromptGenerateImage(_docGenerated), _docDefinition.ProjectID);
                            }
                            else
                            {
                                int cyclesQuality = 25;
                                int widthImage = 512;
                                int heightImage = 512;
                                GameAIData.Instance.AskGenericImageAI((int)AIImageProvider.Dalle2, BuildPromptGenerateImage(_docGenerated), "", cyclesQuality, widthImage, heightImage, ScreenImageGenerationView.EventScreenImageGenerationViewImageCompleted);
                            }
                        }
                        else
                        {
                            ConcludeDocGeneration(_docGenerated.data, _docDefinition.Description, "");
                        }
                    }
                    else
                    {
                        ConcludeDocGeneration(_docGenerated.data, _docDefinition.Description, "");
                    }
                }
                else
                {
                    if (_confirmation)
                    {
                        string title = LanguageController.Instance.GetText("text.error");
                        string description = LanguageController.Instance.GetText("ai.title.error.prompt");
                        ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, title, description, EventAICommandGenerateDocRetryPrompt);
                        _isCompleted = false;
                    }
                    else
                    {
                        _iterationsToCancel--;
                        if (_iterationsToCancel <= 0)
                        {
                            _isCompleted = true;
                            SystemEventController.Instance.DispatchSystemEvent(EventAICommandGenerateDocGenerationCompleted, false, _docDefinition);
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
            if (nameEvent.Equals(EventAICommandGenerateDocSummary))
            {
                if (_docByPlayer != null)
                {
#if UNITY_EDITOR
                    Debug.Log("DOC SUMMARY=" + _docByPlayer.Summary);
#endif
                    int idImage = -1;
                    if (int.TryParse(_docByPlayer.Data.GetHTML(), out idImage))
                    {
                        _docByPlayer.IsImage = true;
                        _docDefinition.IsImage = true;
                    }
                    ConcludeDocGeneration(_docByPlayer.Data.GetHTML(), _docByPlayer.Summary, ApplicationController.Instance.HumanPlayer.NameHuman);
                    _docByPlayer = null;
                }                
            }
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(SubEventAICommandGenerateDocByHuman))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
                    List<DocumentData> docs = (List<DocumentData>)parameters[2];
                    if ((docs != null) && (docs.Count > 0))
                    {
                        DocumentData docByPlayer = docs[0];
                        _docByPlayer = docByPlayer;
                        EventAICommandGenerateDocSummary = EventAICommandGenerateDocSummary_DEFINITION + _docDefinition.UID;
                        AICommandsController.Instance.AddNewAICommand(new AICommandSummarizeDocs(), true, new List<DocumentData>() { docByPlayer }, EventAICommandGenerateDocSummary);
                    }
                    else
                    {
                        ConcludeDocGeneration(_docDefinition.Description, _docDefinition.Description, ApplicationController.Instance.HumanPlayer.NameHuman);
                    }
                }
                else
                {
                    ConcludeDocGeneration(_docDefinition.Description, _docDefinition.Description, ApplicationController.Instance.HumanPlayer.NameHuman);
                }
            }
            if (nameEvent.Equals(EventAICommandGenerateDocPromptConfirmation))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {                    
#if !SHORTCUT_LLM_SERVER
                    WorkDayData.Instance.AddTimeSession();
#endif
                    SystemEventController.Instance.DelaySystemEvent(EventAICommandGenerateDocRequestGeneration, 0.2f, (string)parameters[2]);
                }
                else
                {                    
                    SystemEventController.Instance.DispatchSystemEvent(EventAICommandGenerateDocGenerationCompleted, false, _docDefinition);
                    _isCompleted = true;
                }
            }
            if (nameEvent.Equals(EventAICommandGenerateDocRetryPrompt))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
                    string title = LanguageController.Instance.GetText("text.request.generation.doc.for.task");
                    ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandGenerateDocPromptConfirmation);
                    UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewInitValues, _promptBuilder);
                    UIEventController.Instance.DelayUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRefreshInput, 0.1f);
                }
                else
                {
                    SystemEventController.Instance.DispatchSystemEvent(EventAICommandGenerateDocGenerationCompleted, false, _docDefinition);
                    _isCompleted = true;
                }
            }
        }

        public void Run()
        {
        }
    }
}