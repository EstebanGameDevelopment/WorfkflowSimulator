using UnityEngine;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.ApplicationController;
using System.Collections.Generic;
using InGameCodeEditor;

namespace yourvrexperience.WorkDay
{
    public class AICommandGenerateProject : IAICommand
    {
        private const string EventAICommandProjectDefinitionRequest = "EventAICommandProjectDefinitionRequest";
        private const string EventAICommandProjectDefinitionResponse = "EventAICommandProjectDefinitionResponse";
        private const string EventAICommandProjectDefinitionPromptConfirmation = "EventAICommandProjectDefinitionPromptConfirmation";
        private const string EventAICommandProjectDefinitionPromptRetry = "EventAICommandProjectDefinitionPromptRetry";

        private bool _isCompleted = false;
        private string _outputEvent;
        private PromptBuilder _promptBuilder;
        private bool _confirmation;
        private ProjectDefinitionJSON _projectDefinition;
        
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

            BuildPromptProjectDefinition();
            if (_confirmation)
            {
                string title = LanguageController.Instance.GetText("ai.title.generate.project.description");
                SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
                ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandProjectDefinitionPromptConfirmation);
                UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewInitValues, _promptBuilder);
                UIEventController.Instance.DelayUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRefreshInput, 0.1f);
                SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, false);
            }
            else
            {
                AskProjectLLM(_promptBuilder.BuildPrompt());
            }
        }

        public void Destroy()
        {
            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
            if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

            if (_projectDefinition != null)
            {
                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformationImage, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("ai.generate.new.project.description.completed"), "", "", "", ApplicationController.Instance.GetContentImage(ImagesIndex.TaskCompleted));
                SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunAddProject, _projectDefinition.name, _projectDefinition.description);
            }
            _projectDefinition = null;
            _promptBuilder = null;
        }

        private void AskProjectLLM(string prompt)
        {
            if (_confirmation)
            {
                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("ai.processing.now.please.wait"));
            }
            WorkDayData.Instance.AskWorkDayAIProjectDefinition(prompt, true, EventAICommandProjectDefinitionResponse);
        }

        private void BuildPromptProjectDefinition()
        {
            string question = PromptController.Instance.GetText("ai.command.generation.project.definition.naming",
                                                                "<" + PromptController.Instance.GetText("xml.tag.employees") + ">",
                                                                "<" + PromptController.Instance.GetText("xml.tag.company") + ">");

            switch (PromptController.Instance.CodeLanguage)
            {
                case PromptController.CodeLanguageEnglish:
                    question += JSONDataEnglish.documentTextProjectDefinition;
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
            _promptBuilder.SetPromptColor(Color.white);
            _promptBuilder.SetProjectFeedback("");

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

            // COMPANY "xml.tag.company"
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.company") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.company") + ">",
                                                "</" + PromptController.Instance.GetText("xml.tag.company") + ">"),
                                                WorkDayData.Instance.CurrentProject.Company);
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventAICommandProjectDefinitionRequest))
            {
                AskProjectLLM((string)parameters[0]);
            }
            if (nameEvent.Equals(EventAICommandProjectDefinitionResponse))
            {
                if (_confirmation)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                }
                if ((bool)parameters[0])
                {
                    string projectDefinitionResponse = (string)parameters[1];
#if UNITY_EDITOR
                    Debug.Log("PROJECT RESPONSE RECEIVED=" + projectDefinitionResponse);
#endif

                    if (!JSONDataFormatValidator.ValidateJsonItem<ProjectDefinitionJSON>(projectDefinitionResponse))
                    {
                        string title = LanguageController.Instance.GetText("ai.title.error.prompt");
                        ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLongInput, null, title, "", EventAICommandProjectDefinitionPromptRetry);
                        UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, projectDefinitionResponse);
                        if (GameObject.FindAnyObjectByType<CodeEditor>() != null)
                        {
                            GameObject.FindAnyObjectByType<CodeEditor>().Refresh();
                        }
                        return;
                    }

                    _projectDefinition = JsonUtility.FromJson<ProjectDefinitionJSON>(projectDefinitionResponse);
                    _isCompleted = true;
                }
                else
                {
                    string title = LanguageController.Instance.GetText("text.error");
                    string description = LanguageController.Instance.GetText("ai.title.error.prompt");
                    ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, title, description, EventAICommandProjectDefinitionPromptRetry);
                    _isCompleted = false;
                }
            }
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventAICommandProjectDefinitionPromptConfirmation))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
#if !SHORTCUT_LLM_SERVER
                    WorkDayData.Instance.AddTimeSession();
#endif
                    SystemEventController.Instance.DelaySystemEvent(EventAICommandProjectDefinitionRequest, 0.2f, (string)parameters[2]);
                }
                else
                {
                    _isCompleted = true;
                }
            }
            if (nameEvent.Equals(EventAICommandProjectDefinitionPromptRetry))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
                    string title = LanguageController.Instance.GetText("ai.title.generate.project.description");
                    ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandProjectDefinitionPromptConfirmation);
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