using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using yourvrexperience.ai;
using yourvrexperience.UserManagement;
using yourvrexperience.Utils;
using static yourvrexperience.Utils.AESEncryption;
using static yourvrexperience.WorkDay.AskBaseaCreateSessionChatGPTHTTP;

namespace yourvrexperience.WorkDay
{
    [CreateAssetMenu(menuName = "Game/WorkDayData")]
    public class WorkDayData : ScriptableObject
    {
        public const int PORT_SESSION_SERVER = 5000;
        public const int TOTAL_TIME_SCREEN_SESSION = 600;
        public const int TOTAL_RETRIES_AI_FAILED = 3;

        public const float SIZE_CELL = 0.5f;

        public const string RESERVED_NAME_NONE = "NONE";
        public const string RESERVED_NAME_NOBODY = "NOBODY";
        public const string RESERVED_NAME_ALL = "ALL";
        public const string RESERVED_NAME_SOCIAL = "SOCIAL-PROGRAMMED";
        public const string RESERVED_NAME_CASUAL = "SOCIAL-CASUAL";
        public const string RESERVED_NAME_INTERRUPTIONS = "SOCIAL-INTERRUPTIONS";
        
        public bool IsReservedWord(string name) 
        { 
            return name.Equals(LanguageController.Instance.GetText("screen.calendar." + RESERVED_NAME_NOBODY)) 
                || name.Equals(LanguageController.Instance.GetText("screen.calendar." + RESERVED_NAME_NONE)) 
                || name.Equals(LanguageController.Instance.GetText("screen.calendar." + RESERVED_NAME_ALL)) 
                || name.Equals(LanguageController.Instance.GetText("screen.calendar." + WorkDayData.RESERVED_NAME_SOCIAL)) 
                || name.Equals(LanguageController.Instance.GetText("screen.calendar." + WorkDayData.RESERVED_NAME_CASUAL)) 
                || name.Equals(LanguageController.Instance.GetText("screen.calendar." + WorkDayData.RESERVED_NAME_INTERRUPTIONS));  
        }

        public enum FileDocType { Text = 0, Image }
        public enum TabsData { HTML = 0, CSS, BROWSER, SUMMARY }
        public enum LLMProvider { OpenAI = 0, Mistral, Google, Grok, OpenRouterGPTMini, OpenRouterMistralNemo, OpenRouterGeminiFlash, OpenRouterGrokMini, OpenRouterAnthropicHaiku, Ollama, OpenAIUltra, MistralUltra, GoogleUltra, GrokUltra, OpenRouterGPTUltra, OpenRouterMistralLarge, OpenRouterGeminiPro, OpenRouterGrokPro, OpenRouterAnthropicPro, DeepSeek, Anthropic, None }

        public const string SERVER_PYTHON_CORS_SCREEN_MANAGER = "https://www.workflowsimulator.com";
        public const string WEBSERVER_LOCAL_WORKDAYEDITOR = "http://localhost:8080/workflowsimulator/";

#if UNITY_EDITOR
        public const string WEBSERVER_REMOTE_WORKDAYEDITOR = "http://localhost:8080/workflowsimulator/";
        public const string WEBSERVER_ASSETBUNDLE_WORKDAYEDITOR = "http://localhost:8080/workflowsimulator/";
#else
        public const string WEBSERVER_REMOTE_WORKDAYEDITOR = "https://www.workflowsimulator.com/workflowsimulator/php/";
        public const string WEBSERVER_ASSETBUNDLE_WORKDAYEDITOR = "https://www.workflowsimulator.com/workflowsimulator/assets/";
#endif

        public const string EventCommUpdateProjectIndexHTTP = "yourvrexperience.WorkDay.UpdateProjectIndexHTTP";
        public const string EventCommUpdateProjectDataHTTP = "yourvrexperience.WorkDay.UpdateProjectDataHTTP";
        public const string EventCommDownloadProjectDataHTTP = "yourvrexperience.WorkDay.DownloadProjectDataHTTP";
        public const string EventCommConsultUserProjectsHTTP = "yourvrexperience.WorkDay.ConsultUserProjectsHTTP";
        public const string EventCommDeleteProjectDataHTTP = "yourvrexperience.WorkDay.DeleteProjectDataHTTP";
        public const string EventCommDownloadReferenceDataHTTP = "yourvrexperience.WorkDay.DownloadReferenceDataHTTP";
        public const string EventCommUpdateAnalysisDataHTTP = "yourvrexperience.WorkDay.UpdateAnalysisDataHTTP";
        
        public const string EventCommUploadImageDataHTTP = "yourvrexperience.WorkDay.UploadImageDataHTTP";
        public const string EventCommConsultUserImagesHTTP = "yourvrexperience.WorkDay.ConsultUserImagesHTTP";
        public const string EventCommDownloadImageData = "yourvrexperience.WorkDay.DownloadImageDataHTTP";
        public const string EventCommDeleteImageDataHTTP = "yourvrexperience.WorkDay.DeleteImageDataHTTP";
        public const string EventCommDeleteImageByProjectHTTP = "yourvrexperience.WorkDay.DeleteImageByProjectHTTP";
        public const string EventCommDeleteUserAccountDataHTTP = "yourvrexperience.WorkDay.DeleteUserAccountDataHTTP";

        public const string EventCommDownloadUserSlotsHTTP = "yourvrexperience.WorkDay.DownloadSlotsDataHTTP";
        public const string EventCommUpdatePurchaseSlotHTTP = "yourvrexperience.WorkDay.UpdatePurchaseSlotHTTP";
        public const string EventCommUpdateProjectSlotHTTP = "yourvrexperience.WorkDay.UpdateProjectSlotHTTP";

        public const string EventCommConsultStorageUsedHTTP = "yourvrexperience.WorkDay.ConsultStorageUsedHTTP";

        public const string EventCommAskBaseaCreateSessionChatGPTHTTP = "yourvrexperience.WorkDay.AskBaseaCreateSessionChatGPTHTTP";
        public const string EventCommAskBaseaAddTimeSessionChatGPTHTTP = "yourvrexperience.WorkDay.AskBaseaAddTimeSessionChatGPTHTTP";
        public const string EventCommAskBaseaDestroySessionChatGPTHTTP = "yourvrexperience.WorkDay.AskBaseaDestroySessionChatGPTHTTP";
        public const string EventCommAskBaseaValidateKeyChatGPTHTTP = "yourvrexperience.WorkDay.AskBaseaValidateKeyChatGPTHTTP";

        public const string EventCommGenericQuestionGPTAskQuestionHTTP = "yourvrexperience.WorkDay.AskGenericQuestionHTTP";
        public const string EventCommWorkDayAIDocSummaryTextHTTP = "yourvrexperience.WorkDay.AskWorkDayAIDocSummaryTextHTTP";
        public const string EventCommWorkDayAIDocSummaryImageHTTP = "yourvrexperience.WorkDay.AskWorkDayAIDocSummaryImageHTTP";
        public const string EventCommWWorkDayAIReplyTextMeetingHTTP = "yourvrexperience.WorkDay.AskWorkDayAIReplyTextMeetingHTTP";
        public const string EventCommWWorkDayAIReplyMeetingSummaryHTTP = "yourvrexperience.WorkDay.AskWorkDayAIMeetingSummaryHTTP";
        public const string EventCommWorkDayAITasksDocumentsHTTP = "yourvrexperience.WorkDay.AskWorkDayAITasksDocumentsHTTP";
        public const string EventCommWorkDayAIGenerateDocTextHTTP = "yourvrexperience.WorkDay.AskWorkDayAIGenerateDocTextHTTP";
        public const string EventCommWWorkDayAIMakeGlobalsHTTP = "yourvrexperience.WorkDay.AskWorkDayAIMakeGlobalsHTTP";
        public const string EventCommWWorkDayAIFeaturesDescriptionHTTP = "yourvrexperience.WorkDay.AskWorkDayAIFeaturesDescriptionHTTP";
        public const string EventCommWWorkDayAICreateTasksHTTP = "yourvrexperience.WorkDay.AskWorkDayAICreateTasksHTTP";
        public const string EventCommWWorkDayAISprintBoardDefinitionHTTP = "yourvrexperience.WorkDay.AskWorkDayAISprintBoardDefinitionHTTP";
        public const string EventCommWWorkDayAIProjectDefinitionHTTP = "yourvrexperience.WorkDay.AskWorkDayAIProjectDefinitionHTTP";
        public const string EventCommWWorkDayAIMeetingsDefinitionHTTP = "yourvrexperience.WorkDay.AskWorkDayAIMeetingDefinitionHTTP";
        public const string EventCommWWorkDayAITeamCompanyHTTP = "yourvrexperience.WorkDay.AskWorkDayAITeamCompanyHTTP";
        
        public const string EventWorkDayDataUpdatedLLMProvider = "EventWorkDayDataUpdatedLLMProvider";
        
        public const string LayerFloor = "Floor";
        public const string LayerCell = "Cell";
        public const string LayerArea = "Area";
        public const string LayerItem = "Item";
        public const string LayerChair = "Chair";
        public const string LayerHuman = "Human";

        public const string CoockieLLMModelBase = "CoockieLLMModel";

        public const string Coockie_APIKey_OpenAI = "Coockie_APIKey_OpenAI";
        public const string Coockie_APIKey_Mistral = "Coockie_APIKey_Mistral";
        public const string Coockie_APIKey_DeepSeek = "Coockie_APIKey_DeepSeek";
        public const string Coockie_APIKey_Gemini = "Coockie_APIKey_Gemini";
        public const string Coockie_APIKey_Grok = "Coockie_APIKey_Grok";
        public const string Coockie_APIKey_OpenRouter = "Coockie_APIKey_OpenRouter";
        public const string Coockie_APIKey_Stability = "Coockie_APIKey_Stability";

        public const string Coockie_Server_Session = "Coockie_Server_Session";
        public const string Coockie_Server_Image = "Coockie_Server_Image";

        public const string EncryptionAESKey = "RdtdsQLouCqMQWxi";
        public const string EncryptionAESIV = "HyJdk634fjJKiJ23dslcdkmc";

        public const string EncryptionPlayerPrefsAESKey = "ZRcuWqLcaZeYWVzy";
        
        public const string PLAYERPREFS_LOCAL_ENCRYPTION = "sEcreT-fCky-wOrK-DaY";

        private static WorkDayData _instance;
        public static WorkDayData Instance
        {
            get { return _instance; }
        }

        [Tooltip("URL Base to download narrations")]
        [SerializeField] private string urlBase;
        [Tooltip("Session Screen Server")]
        [SerializeField] public string serverScreenSession;
        [Tooltip("Image Server")]
        [SerializeField] public string serverImageSession;

        [Tooltip("OpenAI API Key")]
        [SerializeField] public string apiKeyOpenAI;
        [Tooltip("Mistral API Key")]
        [SerializeField] public string apiKeyMistral;
        [Tooltip("DeepSeek API Key")]
        [SerializeField] public string apiKeyDeepSeek;
        [Tooltip("Gemini API Key")]
        [SerializeField] public string apiKeyGemini;
        [Tooltip("Gemini API Key")]
        [SerializeField] public string apiKeyGrok;
        [Tooltip("OpenRouter API Key")]
        [SerializeField] public string apiKeyOpenRouter;
        [Tooltip("Stability API Key")]
        [SerializeField] public string apiKeyStability;

        [Tooltip("Player movement speed")]
        [SerializeField] private float playersDesktopSpeed = 10;
        [Tooltip("Sensitivity of the rotation of the camera in desktop mode")]
        [SerializeField] private float sensitivityCamera = 7;

        private ProjectData _currentProject;
        private int _currentIndexProject;
        private string _currentTitleProject;

        private string _nameWorkDayProject;

        private LLMProvider _llmProvider;
        private string _ivString;

        private string _serverState;
        private int _portNumber;

        private List<ProjectSlot> _userSlots;

        public string CoockieLLMModel
        {
            get { return CoockieLLMModelBase + "_" + UsersController.Instance.CurrentUser.Id; }
        }

        public ProjectData CurrentProject
        {
            get { return _currentProject; }
        }
        public string CurrentTitleProject
        {
            get { return _currentTitleProject; }
        }
        public int CurrentIndexProject
        {
            get { return _currentIndexProject; }
        }
        public string URLBase
        {
            get { return urlBase; }
        }
        public LLMProvider LlmProvider
        {
            get { return _llmProvider; }
        }

        public string NameWorkDayProject
        {
            get { return _nameWorkDayProject; }
            set { _nameWorkDayProject = value; }
        }
        public float PlayersDesktopSpeed
        {
            get { return playersDesktopSpeed; }
        }
        public float SensitivityCamera
        {
            get { return sensitivityCamera; }
        }
        public int PortNumber
        {
            get { return _portNumber; }
            set { _portNumber = value; }
        }
        
        public string ServerState
        {
            get { return _serverState; }
            set { _serverState = value; }
        }
        public List<ProjectSlot> UserSlots
        {
            get { return _userSlots; }
            set { _userSlots = value; }
        }
        
        public void Initialize()
        {
            _instance = this;
            _ivString = AESEncryption.GenerateIVFromPassword(PLAYERPREFS_LOCAL_ENCRYPTION);
#if ENABLE_REMOTE_CORS_SERVER
            urlBase = WEBSERVER_REMOTE_WORKDAYEDITOR;
#else
            urlBase = WEBSERVER_REMOTE_WORKDAYEDITOR;
#endif
            CommsHTTPConstants.Instance.URL_BASE_PHP = urlBase;
            LoadServerAddressData();
        }

        public ProjectSlot GetSlotForProject(int idProject)
        {
            return _userSlots.FirstOrDefault(s => s.Project == idProject);
        }

        private string EncryptData(string data)
        {
            if (data.Length > 0)
            {
                AESEncryptedText encryptionData = AESEncryption.Encrypt(Encoding.UTF8.GetBytes(data), EncryptionPlayerPrefsAESKey, Convert.FromBase64String(_ivString));
                return Convert.ToBase64String(encryptionData.EncryptedData);
            }
            else
            {
                return "";
            }
        }

        private string DecryptData(string data)
        {
            if (data.Length > 0)
            {
                try
                {
                    byte[] dataDecrypted = AESEncryption.Decrypt(Convert.FromBase64String(data), _ivString, EncryptionPlayerPrefsAESKey);
                    return Encoding.UTF8.GetString(dataDecrypted);
                }
                catch (Exception err)
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        public void LoadServerAddressData()
        {
            apiKeyOpenAI = DecryptData(PlayerPrefs.GetString(Coockie_APIKey_OpenAI, apiKeyOpenAI));
            apiKeyMistral = DecryptData(PlayerPrefs.GetString(Coockie_APIKey_Mistral, apiKeyMistral));
            apiKeyDeepSeek = DecryptData(PlayerPrefs.GetString(Coockie_APIKey_DeepSeek, apiKeyDeepSeek));
            apiKeyGrok = DecryptData(PlayerPrefs.GetString(Coockie_APIKey_Grok, apiKeyGrok));
            apiKeyGemini = DecryptData(PlayerPrefs.GetString(Coockie_APIKey_Gemini, apiKeyGemini));
            apiKeyOpenRouter = DecryptData(PlayerPrefs.GetString(Coockie_APIKey_OpenRouter, apiKeyOpenRouter));
            apiKeyStability = DecryptData(PlayerPrefs.GetString(Coockie_APIKey_Stability, apiKeyStability));

            serverScreenSession = DecryptData(PlayerPrefs.GetString(Coockie_Server_Session, serverScreenSession));
            serverImageSession = DecryptData(PlayerPrefs.GetString(Coockie_Server_Image, serverImageSession));
        }

        public void SaveServerAddressData()
        {
            PlayerPrefs.SetString(Coockie_APIKey_OpenAI, EncryptData(apiKeyOpenAI));
            PlayerPrefs.SetString(Coockie_APIKey_Mistral, EncryptData(apiKeyMistral));
            PlayerPrefs.SetString(Coockie_APIKey_DeepSeek, EncryptData(apiKeyDeepSeek));
            PlayerPrefs.SetString(Coockie_APIKey_Grok, EncryptData(apiKeyGrok));
            PlayerPrefs.SetString(Coockie_APIKey_Gemini, EncryptData(apiKeyGemini));
            PlayerPrefs.SetString(Coockie_APIKey_OpenRouter, EncryptData(apiKeyOpenRouter));
            PlayerPrefs.SetString(Coockie_APIKey_Stability, EncryptData(apiKeyStability));

            PlayerPrefs.SetString(Coockie_Server_Session, EncryptData(serverScreenSession));
            PlayerPrefs.SetString(Coockie_Server_Image, EncryptData(serverImageSession));
        }

        public bool CheckLoadedAnyAPIKey()
        {
            return (apiKeyOpenAI.Length > 2) || (apiKeyMistral.Length > 2) || (apiKeyDeepSeek.Length > 2) || (apiKeyGemini.Length > 2) || (apiKeyOpenRouter.Length > 2) || (apiKeyGrok.Length > 2);
        }

        public void ClearCache()
        {
        }

        public void LoadCurrentProject(string data)
        {
            ClearCache();
            _currentProject = DeserializeProject(data);
        }

        public DateTime GetApplicationNowDate()
        {
            return DateTime.Now;
        }

        public int GetWeekOfMonth(DateTime date)
        {
            DateTime firstDayOfMonth = new DateTime(date.Year, date.Month, 1);

            CultureInfo culture = CultureInfo.CurrentCulture;
            Calendar calendar = culture.Calendar;
            CalendarWeekRule weekRule = culture.DateTimeFormat.CalendarWeekRule;
            DayOfWeek firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;

            int weekOfYear = calendar.GetWeekOfYear(date, weekRule, firstDayOfWeek);
            int firstWeek = calendar.GetWeekOfYear(firstDayOfMonth, weekRule, firstDayOfWeek);

            return weekOfYear - firstWeek + 1;
        }

        public string SerializeProject(ProjectData project)
        {
            return JsonConvert.SerializeObject(project, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        public ProjectData DeserializeProject(string data)
        {
            if ((data == null) || (data.Length == 0))
            {
                ProjectData projectData = new ProjectData(LanguageController.CodeLanguageEnglish, "", "", false, false, 9, 13, 18, DayOfWeek.Saturday);
                return projectData;
            }
            else
            {
                return JsonConvert.DeserializeObject<ProjectData>(data);
            }
        }

        public void InitializeDropdown(TMP_Dropdown dropDownLLM)
        {
            dropDownLLM.ClearOptions();

            // CHEAP
            if (WorkDayData.Instance.apiKeyOpenAI.Length > 2) dropDownLLM.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenAI.ToString())));
            if (WorkDayData.Instance.apiKeyMistral.Length > 2) dropDownLLM.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("llm.provider." + LLMProvider.Mistral.ToString())));
            if (WorkDayData.Instance.apiKeyGemini.Length > 2) dropDownLLM.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("llm.provider." + LLMProvider.Google.ToString())));
            if (WorkDayData.Instance.apiKeyGrok.Length > 2) dropDownLLM.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("llm.provider." + LLMProvider.Grok.ToString())));
            if (WorkDayData.Instance.apiKeyOpenRouter.Length > 2) dropDownLLM.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterGPTMini.ToString())));
            if (WorkDayData.Instance.apiKeyOpenRouter.Length > 2) dropDownLLM.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterMistralNemo.ToString())));
            if (WorkDayData.Instance.apiKeyOpenRouter.Length > 2) dropDownLLM.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterGeminiFlash.ToString())));
            if (WorkDayData.Instance.apiKeyOpenRouter.Length > 2) dropDownLLM.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterGrokMini.ToString())));

            // EXPENSIVE
            if (WorkDayData.Instance.apiKeyOpenAI.Length > 2) dropDownLLM.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenAIUltra.ToString())));
            if (WorkDayData.Instance.apiKeyMistral.Length > 2) dropDownLLM.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("llm.provider." + LLMProvider.MistralUltra.ToString())));
            if (WorkDayData.Instance.apiKeyGemini.Length > 2) dropDownLLM.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("llm.provider." + LLMProvider.GoogleUltra.ToString())));
            if (WorkDayData.Instance.apiKeyGrok.Length > 2) dropDownLLM.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("llm.provider." + LLMProvider.GrokUltra.ToString())));
            if (WorkDayData.Instance.apiKeyOpenRouter.Length > 2) dropDownLLM.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterGPTUltra.ToString())));
            if (WorkDayData.Instance.apiKeyOpenRouter.Length > 2) dropDownLLM.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterMistralLarge.ToString())));
            if (WorkDayData.Instance.apiKeyOpenRouter.Length > 2) dropDownLLM.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterGeminiPro.ToString())));
            if (WorkDayData.Instance.apiKeyOpenRouter.Length > 2) dropDownLLM.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterGrokPro.ToString())));

            // DEEPSEEK
            if (WorkDayData.Instance.apiKeyDeepSeek.Length > 2) dropDownLLM.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("llm.provider." + LLMProvider.DeepSeek.ToString())));

            // OLLAMA
            dropDownLLM.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("llm.provider." + LLMProvider.Ollama.ToString())));

            string nameProvider = GetLLMProviderName(WorkDayData.Instance.LlmProvider);
            dropDownLLM.value = dropDownLLM.options.FindIndex(option => option.text == nameProvider);
        }

        public LLMProvider GetLLMProviderIndex(string newProvider)
        {
            if (newProvider.Equals(LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenAI.ToString()))) return LLMProvider.OpenAI;
            if (newProvider.Equals(LanguageController.Instance.GetText("llm.provider." + LLMProvider.Mistral.ToString()))) return LLMProvider.Mistral;
            if (newProvider.Equals(LanguageController.Instance.GetText("llm.provider." + LLMProvider.Google.ToString()))) return LLMProvider.Google;
            if (newProvider.Equals(LanguageController.Instance.GetText("llm.provider." + LLMProvider.Grok.ToString()))) return LLMProvider.Grok;
            if (newProvider.Equals(LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterGPTMini.ToString()))) return LLMProvider.OpenRouterGPTMini;
            if (newProvider.Equals(LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterMistralNemo.ToString()))) return LLMProvider.OpenRouterMistralNemo;
            if (newProvider.Equals(LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterGeminiFlash.ToString()))) return LLMProvider.OpenRouterGeminiFlash;
            if (newProvider.Equals(LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterGrokMini.ToString()))) return LLMProvider.OpenRouterGrokMini;

            if (newProvider.Equals(LanguageController.Instance.GetText("llm.provider." + LLMProvider.Ollama.ToString()))) return LLMProvider.Ollama;

            if (newProvider.Equals(LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenAIUltra.ToString()))) return LLMProvider.OpenAIUltra;
            if (newProvider.Equals(LanguageController.Instance.GetText("llm.provider." + LLMProvider.MistralUltra.ToString()))) return LLMProvider.MistralUltra;
            if (newProvider.Equals(LanguageController.Instance.GetText("llm.provider." + LLMProvider.GoogleUltra.ToString()))) return LLMProvider.GoogleUltra;
            if (newProvider.Equals(LanguageController.Instance.GetText("llm.provider." + LLMProvider.GrokUltra.ToString()))) return LLMProvider.GrokUltra;
            if (newProvider.Equals(LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterGPTUltra.ToString()))) return LLMProvider.OpenRouterGPTUltra;
            if (newProvider.Equals(LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterMistralLarge.ToString()))) return LLMProvider.OpenRouterMistralLarge;
            if (newProvider.Equals(LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterGeminiPro.ToString()))) return LLMProvider.OpenRouterGeminiPro;
            if (newProvider.Equals(LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterGrokPro.ToString()))) return LLMProvider.OpenRouterGrokPro;

            if (newProvider.Equals(LanguageController.Instance.GetText("llm.provider." + LLMProvider.DeepSeek.ToString()))) return LLMProvider.DeepSeek;
            return LLMProvider.None;
        }

        public string GetLLMProviderName(LLMProvider newProvider)
        {
            switch (newProvider)
            {
                case LLMProvider.OpenAI: return LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenAI.ToString());
                case LLMProvider.Mistral: return LanguageController.Instance.GetText("llm.provider." + LLMProvider.Mistral.ToString());
                case LLMProvider.Google: return LanguageController.Instance.GetText("llm.provider." + LLMProvider.Google.ToString());
                case LLMProvider.Grok: return LanguageController.Instance.GetText("llm.provider." + LLMProvider.Grok.ToString());
                case LLMProvider.OpenRouterGPTMini: return LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterGPTMini.ToString());
                case LLMProvider.OpenRouterMistralNemo: return LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterMistralNemo.ToString());
                case LLMProvider.OpenRouterGeminiFlash: return LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterGeminiFlash.ToString());
                case LLMProvider.OpenRouterGrokMini: return LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterGrokMini.ToString());

                case LLMProvider.Ollama: return LanguageController.Instance.GetText("llm.provider." + LLMProvider.Ollama.ToString());

                case LLMProvider.OpenAIUltra: return LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenAIUltra.ToString());
                case LLMProvider.MistralUltra: return LanguageController.Instance.GetText("llm.provider." + LLMProvider.MistralUltra.ToString());
                case LLMProvider.GoogleUltra: return LanguageController.Instance.GetText("llm.provider." + LLMProvider.GoogleUltra.ToString());
                case LLMProvider.GrokUltra: return LanguageController.Instance.GetText("llm.provider." + LLMProvider.GrokUltra.ToString());
                case LLMProvider.OpenRouterGPTUltra: return LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterGPTUltra.ToString());
                case LLMProvider.OpenRouterMistralLarge: return LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterMistralLarge.ToString());
                case LLMProvider.OpenRouterGeminiPro: return LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterGeminiPro.ToString());
                case LLMProvider.OpenRouterGrokPro: return LanguageController.Instance.GetText("llm.provider." + LLMProvider.OpenRouterGrokPro.ToString());

                case LLMProvider.DeepSeek: return LanguageController.Instance.GetText("llm.provider." + LLMProvider.DeepSeek.ToString());
            }
            return "";
        }

        public void SetLLMProvider(LLMProvider newProvider)
        {
            PlayerPrefs.SetInt(CoockieLLMModel, (int)newProvider);
            _llmProvider = (LLMProvider)PlayerPrefs.GetInt(CoockieLLMModel, (int)LLMProvider.None);
            InitLLMProvider();
            SystemEventController.Instance.DispatchSystemEvent(EventWorkDayDataUpdatedLLMProvider);
        }

        public void InitLLMProvider()
        {
            AIProvidersLLM aiProviderLLM = AIProvidersLLM.CHAT_GPT;
            string model = "";
            float costInput = 0;
            float costOutput = 0;
            switch (_llmProvider)
            {
                case LLMProvider.None:
                    if ((apiKeyOpenAI != null) && (apiKeyOpenAI.Length > 1))
                    {
                        _llmProvider = LLMProvider.OpenAI;
                        aiProviderLLM = AIProvidersLLM.CHAT_GPT;
                        model = "gpt-4o-mini";
                        costInput = 0.0000003f;
                        costOutput = 0.0000003f;
                    }
                    else
                    if ((apiKeyMistral != null) && (apiKeyMistral.Length > 1))
                    {
                        _llmProvider = LLMProvider.Mistral;
                        aiProviderLLM = AIProvidersLLM.MISTRAL;
                        model = "open-mistral-nemo-2407";
                        costInput = 0.0000003f;
                        costOutput = 0.0000003f;
                    }
                    else
                    if ((apiKeyDeepSeek != null) && (apiKeyDeepSeek.Length > 1))
                    {
                        _llmProvider = LLMProvider.DeepSeek;
                        aiProviderLLM = AIProvidersLLM.DEEPSEEK;
                        model = "deepseek-chat";
                        costInput = 0.0000003f;
                        costOutput = 0.0000003f;
                    }
                    else
                    if ((apiKeyGemini != null) && (apiKeyGemini.Length > 1))
                    {
                        _llmProvider = LLMProvider.Google;
                        aiProviderLLM = AIProvidersLLM.GOOGLE;
                        model = "gemini-1.5-flash";
                        costInput = 0.00000035f;
                        costOutput = 0.0000007f;
                    }
                    else
                    if ((apiKeyGrok != null) && (apiKeyGrok.Length > 1))
                    {
                        _llmProvider = LLMProvider.Grok;
                        aiProviderLLM = AIProvidersLLM.GROK;
                        model = "grok-3-mini";
                        costInput = 0.00000035f;
                        costOutput = 0.0000007f;
                    }
                    else
                    if ((apiKeyOpenRouter != null) && (apiKeyOpenRouter.Length > 1))
                    {
                        _llmProvider = LLMProvider.OpenRouterGPTMini;
                        aiProviderLLM = AIProvidersLLM.OPENROUTER;
                        model = "openai/gpt-4o-mini";
                        costInput = 0.00000035f;
                        costOutput = 0.0000007f;
                    }
                    break;

                case LLMProvider.OpenAI:
                    aiProviderLLM = AIProvidersLLM.CHAT_GPT;
                    model = "gpt-4o-mini";
                    costInput = 0.0000003f;
                    costOutput = 0.0000003f;
                    break;
                case LLMProvider.Mistral:
                    aiProviderLLM = AIProvidersLLM.MISTRAL;
                    model = "open-mistral-nemo-2407";
                    costInput = 0.0000003f;
                    costOutput = 0.0000003f;
                    break;
                case LLMProvider.DeepSeek:
                    aiProviderLLM = AIProvidersLLM.DEEPSEEK;
                    model = "deepseek-chat";
                    costInput = 0.0000003f;
                    costOutput = 0.0000003f;
                    break;
                case LLMProvider.Google:
                    aiProviderLLM = AIProvidersLLM.GOOGLE;
                    model = "gemini-2.5-flash";
                    costInput = 0.00000035f;
                    costOutput = 0.0000007f;
                    break;
                case LLMProvider.Grok:
                    aiProviderLLM = AIProvidersLLM.GROK;
                    model = "grok-3-mini";
                    costInput = 0.00000035f;
                    costOutput = 0.0000007f;
                    break;                    
                case LLMProvider.OpenRouterGPTMini:
                    aiProviderLLM = AIProvidersLLM.OPENROUTER;
                    model = "openai/gpt-4o-mini";
                    costInput = 0.00000035f;
                    costOutput = 0.0000007f;
                    break;
                case LLMProvider.OpenRouterMistralNemo:
                    aiProviderLLM = AIProvidersLLM.OPENROUTER;
                    model = "mistralai/mistral-nemo";
                    costInput = 0.00000035f;
                    costOutput = 0.0000007f;
                    break;
                case LLMProvider.OpenRouterGeminiFlash:
                    aiProviderLLM = AIProvidersLLM.OPENROUTER;
                    model = "google/gemini-2.0-flash-001";
                    costInput = 0.00000035f;
                    costOutput = 0.0000007f;
                    break;
                case LLMProvider.OpenRouterGrokMini:
                    aiProviderLLM = AIProvidersLLM.OPENROUTER;
                    model = "x-ai/grok-3-mini";
                    costInput = 0.00000035f;
                    costOutput = 0.0000007f;
                    break;
                case LLMProvider.Ollama:
                    aiProviderLLM = AIProvidersLLM.LOCAL;
                    costInput = 0;
                    costOutput = 0;
                    break;
                case LLMProvider.OpenAIUltra:
                    aiProviderLLM = AIProvidersLLM.CHAT_GPT;
                    model = "gpt-4o";
                    costInput = 0.000005f;
                    costOutput = 0.000015f;
                    break;
                case LLMProvider.MistralUltra:
                    aiProviderLLM = AIProvidersLLM.MISTRAL;
                    model = "mistral-large-latest";
                    costInput = 0.000003f;
                    costOutput = 0.000009f;
                    break;
                case LLMProvider.GoogleUltra:
                    aiProviderLLM = AIProvidersLLM.GOOGLE;
                    model = "gemini-2.5-pro";
                    costInput = 0.000003f;
                    costOutput = 0.000007f;
                    break;
                case LLMProvider.GrokUltra:
                    aiProviderLLM = AIProvidersLLM.GROK;
                    model = "grok-4-0709";
                    costInput = 0.00000035f;
                    costOutput = 0.0000007f;
                    break;
                case LLMProvider.OpenRouterGPTUltra:
                    aiProviderLLM = AIProvidersLLM.OPENROUTER;
                    model = "openai/chatgpt-4o-latest";
                    costInput = 0.000003f;
                    costOutput = 0.000007f;
                    break;
                case LLMProvider.OpenRouterMistralLarge:
                    aiProviderLLM = AIProvidersLLM.OPENROUTER;
                    model = "mistralai/mistral-large";
                    costInput = 0.000003f;
                    costOutput = 0.000007f;
                    break;
                case LLMProvider.OpenRouterGeminiPro:
                    aiProviderLLM = AIProvidersLLM.OPENROUTER;
                    model = "google/gemini-pro-1.5";
                    costInput = 0.000003f;
                    costOutput = 0.000007f;
                    break;
                case LLMProvider.OpenRouterGrokPro:
                    aiProviderLLM = AIProvidersLLM.OPENROUTER;
                    model = "x-ai/grok-3";
                    costInput = 0.00000035f;
                    costOutput = 0.0000007f;
                    break;
            }

            GameAIData.Instance.InitLLMProvider(aiProviderLLM, model, costInput, costOutput);
        }

        public void ValidateAPIKey(int provider, string apiKey, string customEvent = "")
        {
            List<ItemMultiTextEntry> headers = new List<ItemMultiTextEntry>();
            headers.Add(new ItemMultiTextEntry("Content-Type", "application/json"));
            CommController.Instance.RequestHeader(EventCommAskBaseaValidateKeyChatGPTHTTP, headers, false, provider, apiKey, customEvent);
        }


        public void UpdateProjectIndex(int id, int user, int dataid, string title, string description, int category1, int category2, int category3)
        {
            CommController.Instance.Request(EventCommUpdateProjectIndexHTTP, false, id, user, dataid, title, description, category1, category2, category3);
        }

        public void UpdateProjectData(int id, string data)
        {
            CommController.Instance.Request(EventCommUpdateProjectDataHTTP, false, id, data);
        }

        public void UpdateAnalysisData(string candidate, string analysis)
        {
            CommController.Instance.Request(EventCommUpdateAnalysisDataHTTP, false, candidate, analysis);
        }

        public void ConsultUserProjects(int user)
        {
            CommController.Instance.Request(EventCommConsultUserProjectsHTTP, false, user);
        }

        public void ConsultImages(int user)
        {
            CommController.Instance.Request(EventCommConsultUserImagesHTTP, false, user);
        }

        public void UpdateCurrentProjectData()
        {
            string data = SerializeProject(_currentProject);
            UpdateProjectData(_currentIndexProject, data);
        }

        public void DownloadStoryData(int id, string title)
        {
            _currentIndexProject = id;
            _currentTitleProject = title;

            if (UsersController.Instance.CurrentUser.Profile.Data.Length > 0)
            {
                if (_currentIndexProject != int.Parse(UsersController.Instance.CurrentUser.Profile.Data))
                {
                    UsersController.Instance.CurrentUser.Profile.Data = _currentIndexProject.ToString();
                    UsersController.Instance.CurrentUser.Profile.Data2 = "";
                    UsersController.Instance.CurrentUser.Profile.Data3 = "";
                    UsersController.Instance.CurrentUser.Profile.Data4 = "";
                }
            }

            CommController.Instance.Request(EventCommDownloadProjectDataHTTP, true, id);
        }

        public void DeleteProject(int projectID)
        {
            CommController.Instance.Request(EventCommDeleteProjectDataHTTP, false, projectID);
        }

        public void DownloadReferenceData(int size)
        {
            CommController.Instance.Request(EventCommDownloadReferenceDataHTTP, true, size);
        }
        

        public void UploadImageData(int idImage, int project, string name, byte[] data)
        {
            CommController.Instance.Request(EventCommUploadImageDataHTTP, false, idImage, project, name, data);
        }

        public void DownloadImageData(int id, bool shouldReport)
        {
            CommController.Instance.Request(EventCommDownloadImageData, true, id, shouldReport);
        }

        public void DeleteImage(string data)
        {
            CommController.Instance.Request(EventCommDeleteImageDataHTTP, false, data);
        }

        public void DeleteImageByProject(int projectID)
        {
            CommController.Instance.Request(EventCommDeleteImageByProjectHTTP, false, projectID);
        }

        public void DeleteUserAccount()
        {
            CommController.Instance.Request(EventCommDeleteUserAccountDataHTTP, false);
        }

        public void GetStorageUsed(int projectID, int level)
        {
            if ((CurrentProject != null ) && (CurrentProject.CalculateStorage() > 0))
            {
                SystemEventController.Instance.DelaySystemEvent(ConsultStorageUsedHTTP.EventConsultStorageUsedHTTPCompleted, 1, true, CurrentProject.Storage);
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log("RETRIEVING NEW STORAGE USED::projectID["+ projectID + "]!!!");
#endif
                CommController.Instance.Request(EventCommConsultStorageUsedHTTP, false, projectID, level);
            }
        }

        public void CreateServerSession(string language, string customEvent = "")
        {
            List<ItemMultiTextEntry> headers = new List<ItemMultiTextEntry>();
            headers.Add(new ItemMultiTextEntry("Content-Type", "application/json"));
            CommController.Instance.RequestHeader(EventCommAskBaseaCreateSessionChatGPTHTTP, headers, false, language, customEvent);
        }

        public void AddTimeSession(string customEvent = "")
        {
            List<ItemMultiTextEntry> headers = new List<ItemMultiTextEntry>();
            headers.Add(new ItemMultiTextEntry("Content-Type", "application/json"));
            CommController.Instance.RequestPriorityHeader(EventCommAskBaseaAddTimeSessionChatGPTHTTP, headers, false, customEvent);
        }

        public void DestroySession(string customEvent = "")
        {
            List<ItemMultiTextEntry> headers = new List<ItemMultiTextEntry>();
            headers.Add(new ItemMultiTextEntry("Content-Type", "application/json"));
            CommController.Instance.RequestHeader(EventCommAskBaseaDestroySessionChatGPTHTTP, headers, false, customEvent);
        }

        public void AskGenericQuestionAI(string question, bool askCost, string customEvent = "")
        {
            List<ItemMultiTextEntry> headers = new List<ItemMultiTextEntry>();
            headers.Add(new ItemMultiTextEntry("Content-Type", "application/json"));
            CommController.Instance.RequestHeader(EventCommGenericQuestionGPTAskQuestionHTTP, headers, false, question, askCost, customEvent);
        }

        public void AskWorkDayAIDocSummaryText(string question, bool askCost, string customEvent = "")
        {
            List<ItemMultiTextEntry> headers = new List<ItemMultiTextEntry>();
            headers.Add(new ItemMultiTextEntry("Content-Type", "application/json"));
            CommController.Instance.RequestHeader(EventCommWorkDayAIDocSummaryTextHTTP, headers, false, question, askCost, customEvent);
        }

        public void AskWorkDayAIDocSummaryImage(string question, string imageBase64, bool askCost, string customEvent = "")
        {
            List<ItemMultiTextEntry> headers = new List<ItemMultiTextEntry>();
            headers.Add(new ItemMultiTextEntry("Content-Type", "application/json"));
            CommController.Instance.RequestHeader(EventCommWorkDayAIDocSummaryImageHTTP, headers, false, question, imageBase64, askCost, customEvent);
        }

        public void AskWorkDayAIReplyTextMeeting(string question, bool askCost, string customEvent = "")
        {
            List<ItemMultiTextEntry> headers = new List<ItemMultiTextEntry>();
            headers.Add(new ItemMultiTextEntry("Content-Type", "application/json"));
            CommController.Instance.RequestHeader(EventCommWWorkDayAIReplyTextMeetingHTTP, headers, false, question, askCost, customEvent);
        }

        public void AskWorkDayAIMeetingSummary(string question, bool askCost, string customEvent = "")
        {
            List<ItemMultiTextEntry> headers = new List<ItemMultiTextEntry>();
            headers.Add(new ItemMultiTextEntry("Content-Type", "application/json"));
            CommController.Instance.RequestHeader(EventCommWWorkDayAIReplyMeetingSummaryHTTP, headers, false, question, askCost, customEvent);
        }

        public void AskWorkDayAITasksDocuments(string question, bool askCost, string customEvent = "")
        {
            List<ItemMultiTextEntry> headers = new List<ItemMultiTextEntry>();
            headers.Add(new ItemMultiTextEntry("Content-Type", "application/json"));
            CommController.Instance.RequestHeader(EventCommWorkDayAITasksDocumentsHTTP, headers, false, question, askCost, customEvent);
        }

        public void AskWorkDayAIGenerateDocText(string question, bool askCost, string customEvent = "")
        {
            List<ItemMultiTextEntry> headers = new List<ItemMultiTextEntry>();
            headers.Add(new ItemMultiTextEntry("Content-Type", "application/json"));
            CommController.Instance.RequestHeader(EventCommWorkDayAIGenerateDocTextHTTP, headers, false, question, askCost, customEvent);
        }

        public void AskWorkDayAIMakeGlobals(string question, bool askCost, string customEvent = "")
        {
            List<ItemMultiTextEntry> headers = new List<ItemMultiTextEntry>();
            headers.Add(new ItemMultiTextEntry("Content-Type", "application/json"));
            CommController.Instance.RequestHeader(EventCommWWorkDayAIMakeGlobalsHTTP, headers, false, question, askCost, customEvent);
        }
        
        public void AskWorkDayAIFeaturesDescription(string question, bool askCost, string customEvent = "")
        {
            List<ItemMultiTextEntry> headers = new List<ItemMultiTextEntry>();
            headers.Add(new ItemMultiTextEntry("Content-Type", "application/json"));
            CommController.Instance.RequestHeader(EventCommWWorkDayAIFeaturesDescriptionHTTP, headers, false, question, askCost, customEvent);
        }

        public void AskWorkDayAICreateTasks(string question, bool askCost, string customEvent = "")
        {
            List<ItemMultiTextEntry> headers = new List<ItemMultiTextEntry>();
            headers.Add(new ItemMultiTextEntry("Content-Type", "application/json"));
            CommController.Instance.RequestHeader(EventCommWWorkDayAICreateTasksHTTP, headers, false, question, askCost, customEvent);
        }

        public void AskWorkDayAISprintBoardDefinition(string question, bool askCost, string customEvent = "")
        {
            List<ItemMultiTextEntry> headers = new List<ItemMultiTextEntry>();
            headers.Add(new ItemMultiTextEntry("Content-Type", "application/json"));
            CommController.Instance.RequestHeader(EventCommWWorkDayAISprintBoardDefinitionHTTP, headers, false, question, askCost, customEvent);
        }

        public void AskWorkDayAIProjectDefinition(string question, bool askCost, string customEvent = "")
        {
            List<ItemMultiTextEntry> headers = new List<ItemMultiTextEntry>();
            headers.Add(new ItemMultiTextEntry("Content-Type", "application/json"));
            CommController.Instance.RequestHeader(EventCommWWorkDayAIProjectDefinitionHTTP, headers, false, question, askCost, customEvent);
        }

        public void AskWorkDayAIMeetingsDefinition(string question, bool askCost, string customEvent = "")
        {
            List<ItemMultiTextEntry> headers = new List<ItemMultiTextEntry>();
            headers.Add(new ItemMultiTextEntry("Content-Type", "application/json"));
            CommController.Instance.RequestHeader(EventCommWWorkDayAIMeetingsDefinitionHTTP, headers, false, question, askCost, customEvent);
        }

        public void AskWorkDayAITeamCompany(string question, bool askCost, string customEvent = "")
        {
            List<ItemMultiTextEntry> headers = new List<ItemMultiTextEntry>();
            headers.Add(new ItemMultiTextEntry("Content-Type", "application/json"));
            CommController.Instance.RequestHeader(EventCommWWorkDayAITeamCompanyHTTP, headers, false, question, askCost, customEvent);
        }

        public void DownloadUserSlots(int user)
        {
            CommController.Instance.Request(EventCommDownloadUserSlotsHTTP, false, user);
        }

        public void PurchaseUserSlot(int slot, int level, long timeout, string receiptData)
        {
            CommController.Instance.Request(EventCommUpdatePurchaseSlotHTTP, false, slot, level, timeout, receiptData);
        }

        public void UpdateProjectSlot(int slot, int story)
        {
            CommController.Instance.Request(EventCommUpdateProjectSlotHTTP, false, slot, story);
        }

        private string _serverSessionFinal = "";

        public string GetServerScreenSession()
        {
            if (WorkDayData.Instance.serverScreenSession.Length > 0)
            {
                return WorkDayData.Instance.serverScreenSession;
            }
            else
            {
                return WorkDayData.SERVER_PYTHON_CORS_SCREEN_MANAGER;
            }
        }
    }
}