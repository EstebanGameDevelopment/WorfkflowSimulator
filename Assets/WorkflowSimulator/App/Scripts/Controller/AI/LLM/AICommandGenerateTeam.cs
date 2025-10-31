using UnityEngine;
using yourvrexperience.Utils;
using System.Collections.Generic;
using InGameCodeEditor;

namespace yourvrexperience.WorkDay
{
    public class AICommandGenerateTeam : IAICommand
    {
        private const string EventAICommandGenerateTeamRequest = "EventAICommandGenerateTeamRequest";
        private const string EventAICommandGenerateTeamResponse = "EventAICommandGenerateTeamResponse";
        private const string EventAICommandGenerateTeamPromptConfirmation = "EventAICommandGenerateTeamPromptConfirmation";
        private const string EventAICommandGenerateTeamPromptRetry = "EventAICommandGenerateTeamPromptRetry";
        private const string EventAICommandGenerateTeamCompleted = "EventAICommandGenerateTeamCompleted";

        private bool _isCompleted = false;
        private string _outputEvent;
        private string _companyData;
        private bool _isFastPaced;
        private PromptBuilder _promptBuilder;
        private bool _confirmation;
        private TeamCompanyListJSON _teamDefinition;
        
        public string Name
        {
            get
            {
                return "GenerateTeam";
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
            _companyData = (string)parameters[1];
            _isFastPaced = (bool)parameters[2];

            BuildPromptProjectDefinition();
            string title = LanguageController.Instance.GetText("ai.title.generate.team.company");
            SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
            ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandGenerateTeamPromptConfirmation);
            UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewInitValues, _promptBuilder);
            UIEventController.Instance.DelayUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRefreshInput, 0.1f);
            SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, false);
        }

        public void Destroy()
        {
            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
            if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

            if (_teamDefinition != null)
            {
                if (_outputEvent.Length > 0)
                {
                    SystemEventController.Instance.DispatchSystemEvent(_outputEvent, true, _teamDefinition);
                }
                else
                {
                    SystemEventController.Instance.DispatchSystemEvent(EventAICommandGenerateTeamCompleted, true, _teamDefinition);
                }
            }
            else
            {
                if (_outputEvent.Length > 0)
                {
                    SystemEventController.Instance.DispatchSystemEvent(_outputEvent, false);
                }
                else
                {
                    SystemEventController.Instance.DispatchSystemEvent(EventAICommandGenerateTeamCompleted, false);
                }
            }

            _teamDefinition = null;
            _promptBuilder = null;
        }

        private void AskTeamCompanyLLM(string prompt)
        {
            ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("ai.processing.now.please.wait"));
            WorkDayData.Instance.AskWorkDayAITeamCompany(prompt, true, EventAICommandGenerateTeamResponse);
        }

        private void BuildPromptProjectDefinition()
        {
            string question = "";
            if (_isFastPaced)
            {
                question = PromptController.Instance.GetText("ai.command.generation.team.fast.paced.company",
                                                                "<" + PromptController.Instance.GetText("xml.tag.company") + ">");
            }
            else
            {
                question = PromptController.Instance.GetText("ai.command.generation.team.company",
                                                                "<" + PromptController.Instance.GetText("xml.tag.company") + ">");
            }

            switch (PromptController.Instance.CodeLanguage)
            {
                case PromptController.CodeLanguageEnglish:
                    question += JSONDataEnglish.teamCompanyJsonString;
                    break;
                case PromptController.CodeLanguageSpanish:
                    question += JSONDataSpanish.teamCompanyJsonString;
                    break;
                case PromptController.CodeLanguageGerman:
                    question += JSONDataGerman.teamCompanyJsonString;
                    break;
                case PromptController.CodeLanguageFrench:
                    question += JSONDataFrench.teamCompanyJsonString;
                    break;
                case PromptController.CodeLanguageItalian:
                    question += JSONDataItalian.teamCompanyJsonString;
                    break;
                case PromptController.CodeLanguageRussian:
                    question += JSONDataRussian.teamCompanyJsonString;
                    break;
                case PromptController.CodeLanguageCatalan:
                    question += JSONDataCatalan.teamCompanyJsonString;
                    break;
            }

            _promptBuilder = new PromptBuilder(question);
            _promptBuilder.SetPromptColor(Color.white);
            _promptBuilder.SetProjectFeedback("");

            // COMPANY "xml.tag.company"
            _promptBuilder.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.company") + ">",
                                                "<" + PromptController.Instance.GetText("xml.tag.company") + ">",
                                                "</" + PromptController.Instance.GetText("xml.tag.company") + ">"),
                                                _companyData);
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventAICommandGenerateTeamRequest))
            {
                AskTeamCompanyLLM((string)parameters[0]);
            }
            if (nameEvent.Equals(EventAICommandGenerateTeamResponse))
            {
                UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                if ((bool)parameters[0])
                {
                    string teamResponse = (string)parameters[1];
#if UNITY_EDITOR
                    Debug.Log("TEAM RESPONSE RECEIVED=" + teamResponse);
#endif

                    if (!JSONDataFormatValidator.ValidateJsonItem<TeamCompanyListJSON>(teamResponse))
                    {
                        string title = LanguageController.Instance.GetText("ai.title.error.prompt");
                        ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLongInput, null, title, "", EventAICommandGenerateTeamPromptRetry);
                        UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, teamResponse);
                        if (GameObject.FindAnyObjectByType<CodeEditor>() != null)
                        {
                            GameObject.FindAnyObjectByType<CodeEditor>().Refresh();
                        }
                        return;
                    }

                    _teamDefinition = JsonUtility.FromJson<TeamCompanyListJSON>(teamResponse);

                    Dictionary<string, List<EmployeeCompanyJSON>> teamFixing = new Dictionary<string, List<EmployeeCompanyJSON>>();
                    foreach (GroupCompanyJSON group in _teamDefinition.groups)
                    {
                        List<EmployeeCompanyJSON> existingGroup;
                        if (!teamFixing.TryGetValue(group.name, out existingGroup))
                        {
                            existingGroup = new List<EmployeeCompanyJSON>();
                            teamFixing.Add(group.name, existingGroup);
                        }
                        foreach (EmployeeCompanyJSON employee in _teamDefinition.employees)
                        {
                            if (StringSimilarity.CalculateSimilarityPercentage(employee.group.ToLower(), group.name.ToLower()) > 85)
                            {
                                employee.group = group.name;
                                existingGroup.Add(employee);
                            }
                        }
                    }

                    string categoryLead = PromptController.Instance.GetText("xml.tag.category.lead");
                    string categorySenior = PromptController.Instance.GetText("xml.tag.category.senior");
                    string categoryNormal = PromptController.Instance.GetText("xml.tag.category.normal");

                    foreach (KeyValuePair<string, List<EmployeeCompanyJSON>> team in teamFixing)
                    {
                        int totalLeads = 0;
                        EmployeeCompanyJSON employeeReference = null;
                        foreach (EmployeeCompanyJSON employee in team.Value)
                        {
                            if (StringSimilarity.CalculateSimilarityPercentage(employee.category.ToLower(), categoryLead.ToLower()) > 85)
                            {
                                employee.category = categoryLead;
                                totalLeads++;
                            }
                            employeeReference = employee;
                        }
                        if ((totalLeads == 0) && (employeeReference != null))
                        {
                            employeeReference.category = categoryLead;
                        }
                    }

                    _isCompleted = true;
                }
                else
                {
                    string title = LanguageController.Instance.GetText("text.error");
                    string description = LanguageController.Instance.GetText("ai.title.error.prompt");
                    ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, title, description, EventAICommandGenerateTeamPromptRetry);
                    _isCompleted = false;
                }
            }
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventAICommandGenerateTeamPromptConfirmation))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
#if !SHORTCUT_LLM_SERVER
                    WorkDayData.Instance.AddTimeSession();
#endif
                    SystemEventController.Instance.DelaySystemEvent(EventAICommandGenerateTeamRequest, 0.2f, (string)parameters[2]);
                }
                else
                {
                    _isCompleted = true;
                }
            }
            if (nameEvent.Equals(EventAICommandGenerateTeamPromptRetry))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
                    string title = LanguageController.Instance.GetText("ai.title.generate.team.company");
                    ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, null, title, "", EventAICommandGenerateTeamPromptConfirmation);
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