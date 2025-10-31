using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class ScreenAIServersView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenAIServersView";

		public const string SubEventScreenAIServersViewExitConfirmation = "SubEventScreenAIServersViewExitConfirmation";

		public const string Coockie_ValidatedKey_OpenAI = "Coockie_ValidatedKey_OpenAI";
		public const string Coockie_ValidatedKey_Mistral = "Coockie_ValidatedKey_Mistral";
		public const string Coockie_ValidatedKey_DeepSeek = "Coockie_ValidatedKey_DeepSeek";
		public const string Coockie_ValidatedKey_Gemini = "Coockie_ValidatedKey_Gemini";
		public const string Coockie_ValidatedKey_OpenRouter = "Coockie_ValidatedKey_OpenRouter";
		public const string Coockie_ValidatedKey_Stability = "Coockie_ValidatedKey_Stability";
		public const string Coockie_ValidatedKey_Grok = "Coockie_ValidatedKey_Grok";

		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private GameObject contentScroll;

		[SerializeField] private TextMeshProUGUI titleLLMProviders;

		[SerializeField] private TextMeshProUGUI titleOpenAI;
		[SerializeField] private TextMeshProUGUI titleMistral;
		[SerializeField] private TextMeshProUGUI titleDeepSeek;
		[SerializeField] private TextMeshProUGUI titleGemini;
		[SerializeField] private TextMeshProUGUI titleOpenRouter;
		[SerializeField] private TextMeshProUGUI titleStability;
		[SerializeField] private TextMeshProUGUI titleGrok;

		[SerializeField] private TextMeshProUGUI titleServerSession;
		[SerializeField] private TextMeshProUGUI titleServerImage;

		[SerializeField] private CustomInput inputOpenAI;
		[SerializeField] private CustomInput inputMistral;
		[SerializeField] private CustomInput inputDeepSeek;
		[SerializeField] private CustomInput inputGemini;
		[SerializeField] private CustomInput inputOpenRouter;
		[SerializeField] private CustomInput inputStability;
		[SerializeField] private CustomInput inputGrok;

		[SerializeField] private CustomInput inputServerSession;
		[SerializeField] private CustomInput inputServerImage;

		[SerializeField] private Button buttonBack;
		[SerializeField] private Button buttonSave;

		[SerializeField] private GameObject checkValidOpenAI;
		[SerializeField] private GameObject checkValidMistral;
		[SerializeField] private GameObject checkValidDeepSeek;
		[SerializeField] private GameObject checkValidGemini;
		[SerializeField] private GameObject checkValidOpenRouter;
		[SerializeField] private GameObject checkValidStability;
		[SerializeField] private GameObject checkValidGrok;

		private int _providerVerification = 0;

		private bool _modifiedOpenAI = false;
		private bool _modifiedMistral = false;
		private bool _modifiedDeepSeek = false;
		private bool _modifiedGemini = false;
		private bool _modifiedOpenRouter = false;
		private bool _modifiedStability = false;
		private bool _modifiedGrok = false;

		private string _nameProviderToCheck = "";
		private string _cookieProviderChecked = "";

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			titleScreen.text = LanguageController.Instance.GetText("screen.ai.server.title");

			buttonBack.onClick.AddListener(OnButtonBack);
			buttonSave.onClick.AddListener(OnButtonSave);

			titleLLMProviders.text = LanguageController.Instance.GetText("screen.ai.server.title.llm.providers");

			titleOpenAI.text = LanguageController.Instance.GetText("screen.ai.server.api.key.openai");
			titleMistral.text = LanguageController.Instance.GetText("screen.ai.server.api.key.mistral");
			titleDeepSeek.text = LanguageController.Instance.GetText("screen.ai.server.api.key.deepseek");
			titleGemini.text = LanguageController.Instance.GetText("screen.ai.server.api.key.gemini");
			titleOpenRouter.text = LanguageController.Instance.GetText("screen.ai.server.api.key.openrouter");
			titleStability.text = LanguageController.Instance.GetText("screen.ai.server.api.key.stability");
			titleGrok.text = LanguageController.Instance.GetText("screen.ai.server.api.key.grok");

			titleServerSession.text = LanguageController.Instance.GetText("screen.ai.server.api.key.local.llm");
			titleServerImage.text = LanguageController.Instance.GetText("screen.ai.server.api.key.local.image");

			inputOpenAI.text = WorkDayData.Instance.apiKeyOpenAI;
			inputMistral.text = WorkDayData.Instance.apiKeyMistral;
			inputDeepSeek.text = WorkDayData.Instance.apiKeyDeepSeek;
			inputGemini.text = WorkDayData.Instance.apiKeyGemini;
			inputOpenRouter.text = WorkDayData.Instance.apiKeyOpenRouter;
			inputStability.text = WorkDayData.Instance.apiKeyStability;
			inputGrok.text = WorkDayData.Instance.apiKeyGrok;

			inputServerSession.text = WorkDayData.Instance.serverScreenSession;
			inputServerImage.text = WorkDayData.Instance.serverImageSession;

			inputOpenAI.onValueChanged.AddListener(OnAPIKeyOpenAIChanged);
			inputMistral.onValueChanged.AddListener(OnAPIKeyMistralChanged);
			inputDeepSeek.onValueChanged.AddListener(OnAPIKeyDeepSeekChanged);
			inputGemini.onValueChanged.AddListener(OnAPIKeyGeminiChanged);
			inputOpenRouter.onValueChanged.AddListener(OnAPIKeyOpenRouterChanged);
			inputStability.onValueChanged.AddListener(OnAPIKeyStabilityChanged);
			inputGrok.onValueChanged.AddListener(OnAPIKeyGrokChanged);

			inputServerSession.onValueChanged.AddListener(OnServerSessionChanged);
			inputServerImage.onValueChanged.AddListener(OnServerImageChanged);

			UIEventController.Instance.Event += OnUIEvent;
			SystemEventController.Instance.Event += OnSystemEvent;

			LoadValidationKeys();

			SoundsController.Instance.PlaySoundFX(GameSounds.FxSelection, false, GameSounds.VolumeFXSelection);
		}

        private void LoadValidationKeys()
		{
			bool isValidOpenAI = (PlayerPrefs.GetInt(Coockie_ValidatedKey_OpenAI, 0) == 1);
			bool isValidMistral = (PlayerPrefs.GetInt(Coockie_ValidatedKey_Mistral, 0) == 1);
			bool isValidDeepSeek = (PlayerPrefs.GetInt(Coockie_ValidatedKey_DeepSeek, 0) == 1);
			bool isValidGemini = (PlayerPrefs.GetInt(Coockie_ValidatedKey_Gemini, 0) == 1);
			bool isValidGrok = (PlayerPrefs.GetInt(Coockie_ValidatedKey_Grok, 0) == 1);
			bool isValidOpenRouter = (PlayerPrefs.GetInt(Coockie_ValidatedKey_OpenRouter, 0) == 1);
			bool isValidStability = (PlayerPrefs.GetInt(Coockie_ValidatedKey_Stability, 0) == 1);

			checkValidOpenAI.transform.Find("On").gameObject.SetActive(isValidOpenAI);
			checkValidOpenAI.transform.Find("Off").gameObject.SetActive(!isValidOpenAI);

			checkValidMistral.transform.Find("On").gameObject.SetActive(isValidMistral);
			checkValidMistral.transform.Find("Off").gameObject.SetActive(!isValidMistral);

			checkValidDeepSeek.transform.Find("On").gameObject.SetActive(isValidDeepSeek);
			checkValidDeepSeek.transform.Find("Off").gameObject.SetActive(!isValidDeepSeek);

			checkValidGemini.transform.Find("On").gameObject.SetActive(isValidGemini);
			checkValidGemini.transform.Find("Off").gameObject.SetActive(!isValidGemini);

            checkValidGrok.transform.Find("On").gameObject.SetActive(isValidGrok);
            checkValidGrok.transform.Find("Off").gameObject.SetActive(!isValidGrok);

            checkValidOpenRouter.transform.Find("On").gameObject.SetActive(isValidOpenRouter);
			checkValidOpenRouter.transform.Find("Off").gameObject.SetActive(!isValidOpenRouter);

			checkValidStability.transform.Find("On").gameObject.SetActive(isValidStability);
			checkValidStability.transform.Find("Off").gameObject.SetActive(!isValidStability);
		}

		private void OnButtonSave()
		{
#if UNITY_EDITOR
            string titleValidated = LanguageController.Instance.GetText("text.info");
            string descriptionValidated = LanguageController.Instance.GetText("screen.ai.server.verified.api.keys");
            ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, this.gameObject, titleValidated, descriptionValidated);
            WorkDayData.Instance.SaveServerAddressData();
            SoundsController.Instance.PlaySoundFX(GameSounds.FxConfirmation, false, GameSounds.VolumeFXConfirmation);
#else
			RequestProviderVerification();
#endif
        }

        private void OnServerSessionChanged(string value)
		{
			WorkDayData.Instance.serverScreenSession = value;
		}

		private void OnServerImageChanged(string value)
		{
			WorkDayData.Instance.serverImageSession = value;
		}

		private void OnAPIKeyOpenAIChanged(string value)
		{
			_modifiedOpenAI = true;
			WorkDayData.Instance.apiKeyOpenAI = value;
			if (WorkDayData.Instance.apiKeyOpenAI.Length == 0) _modifiedOpenAI = false;
		}

		private void OnAPIKeyMistralChanged(string value)
		{
			_modifiedMistral = true;
			WorkDayData.Instance.apiKeyMistral = value;
			if (WorkDayData.Instance.apiKeyMistral.Length == 0) _modifiedMistral = false;
		}

		private void OnAPIKeyDeepSeekChanged(string value)
		{
			_modifiedDeepSeek = true;
			WorkDayData.Instance.apiKeyDeepSeek = value;
			if (WorkDayData.Instance.apiKeyDeepSeek.Length == 0) _modifiedDeepSeek = false;
		}

		private void OnAPIKeyGeminiChanged(string value)
		{
			_modifiedGemini = true;
			WorkDayData.Instance.apiKeyGemini = value;
			if (WorkDayData.Instance.apiKeyGemini.Length == 0) _modifiedGemini = false;
		}

		private void OnAPIKeyOpenRouterChanged(string value)
		{
			_modifiedOpenRouter = true;
			WorkDayData.Instance.apiKeyOpenRouter = value;
			if (WorkDayData.Instance.apiKeyOpenRouter.Length == 0) _modifiedOpenRouter = false;
		}
		private void OnAPIKeyStabilityChanged(string value)
		{
			_modifiedStability = true;
			WorkDayData.Instance.apiKeyStability = value;
			if (WorkDayData.Instance.apiKeyStability.Length == 0) _modifiedStability = false;
		}

		private void OnAPIKeyGrokChanged(string value)
		{
			_modifiedGrok = true;
			WorkDayData.Instance.apiKeyGrok = value;
			if (WorkDayData.Instance.apiKeyGrok.Length == 0) _modifiedGrok = false;
		}

		private bool HasBeenModified()
		{
			return _modifiedOpenAI || _modifiedMistral || _modifiedDeepSeek || _modifiedGemini || _modifiedOpenRouter || _modifiedStability || _modifiedGrok;
		}

		private void RequestProviderVerification()
		{
			_nameProviderToCheck = "";
			_providerVerification = -1;
			if (_modifiedOpenAI)
			{
				_modifiedOpenAI = false;
				_providerVerification = 0;
				_nameProviderToCheck = "OpenAI";
				_cookieProviderChecked = Coockie_ValidatedKey_OpenAI;
				WorkDayData.Instance.ValidateAPIKey(_providerVerification, WorkDayData.Instance.apiKeyOpenAI);
			}
			else
			if (_modifiedMistral)
			{
				_modifiedMistral = false;
				_providerVerification = 1;
				_nameProviderToCheck = "Mistral";
				_cookieProviderChecked = Coockie_ValidatedKey_Mistral;
				WorkDayData.Instance.ValidateAPIKey(_providerVerification, WorkDayData.Instance.apiKeyMistral);
			}
			else
			if (_modifiedDeepSeek)
			{
				_modifiedDeepSeek = false;
				_providerVerification = 2;
				_nameProviderToCheck = "DeepSeek";
				_cookieProviderChecked = Coockie_ValidatedKey_DeepSeek;
				WorkDayData.Instance.ValidateAPIKey(_providerVerification, WorkDayData.Instance.apiKeyDeepSeek);
			}
			else
			if (_modifiedGemini)
			{
				_modifiedGemini = false;
				_providerVerification = 3;
				_nameProviderToCheck = "Google";
				_cookieProviderChecked = Coockie_ValidatedKey_Gemini;
				WorkDayData.Instance.ValidateAPIKey(_providerVerification, WorkDayData.Instance.apiKeyGemini);
			}
			else
			if (_modifiedOpenRouter)
			{
				_modifiedOpenRouter = false;
				_providerVerification = 4;
				_nameProviderToCheck = "OpenRouter";
				_cookieProviderChecked = Coockie_ValidatedKey_OpenRouter;
				WorkDayData.Instance.ValidateAPIKey(_providerVerification, WorkDayData.Instance.apiKeyOpenRouter);
			}
            else
            if (_modifiedStability)
            {
				_modifiedStability = false;
				_providerVerification = 5;
				_nameProviderToCheck = "Stability";
				_cookieProviderChecked = Coockie_ValidatedKey_Stability;
				WorkDayData.Instance.ValidateAPIKey(_providerVerification, WorkDayData.Instance.apiKeyStability);
			}
            else
            if (_modifiedGrok)
            {
				_modifiedGrok = false;
				_providerVerification = 6;
				_nameProviderToCheck = "Grok";
				_cookieProviderChecked = Coockie_ValidatedKey_Grok;
				WorkDayData.Instance.ValidateAPIKey(_providerVerification, WorkDayData.Instance.apiKeyGrok);
			}

			UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
			if (_providerVerification != -1)
			{
				string titleWait = LanguageController.Instance.GetText("text.info");
				string descriptionWait = LanguageController.Instance.GetText("screen.ai.server.verifying.api.keys", _nameProviderToCheck);
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoading, this.gameObject, titleWait, descriptionWait);
			}
			else
			{
				string titleValidated = LanguageController.Instance.GetText("text.info");
				string descriptionValidated = LanguageController.Instance.GetText("screen.ai.server.verified.api.keys");
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, this.gameObject, titleValidated, descriptionValidated);
				WorkDayData.Instance.SaveServerAddressData();
				SoundsController.Instance.PlaySoundFX(GameSounds.FxConfirmation, false, GameSounds.VolumeFXConfirmation);
			}
		}

		public override void Destroy()
		{
			base.Destroy();

			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
		}

		private void OnButtonBack()
		{
#if UNITY_EDITOR
            UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
#else
			if (HasBeenModified())
			{
				string title = LanguageController.Instance.GetText("text.warning");
				string descriptionWarningChange = LanguageController.Instance.GetText("screen.ai.server.exit.without.verifying");
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, title, descriptionWarningChange, SubEventScreenAIServersViewExitConfirmation);
			}
			else
			{
				UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
			}
#endif
        }

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(SubEventScreenAIServersViewExitConfirmation))
			{
				if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
				{
					UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
				}
			}
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(AskBaseaValidateKeyChatGPTHTTP.EventAskBaseaValidateKeyChatGPTHTTPCompleted))
			{
				if ((bool)parameters[0])
				{
					PlayerPrefs.SetInt(_cookieProviderChecked, 1);
					LoadValidationKeys();
					RequestProviderVerification();
				}
				else
				{
					switch (_providerVerification)
					{
						case 0:
							_modifiedOpenAI = true;
							WorkDayData.Instance.apiKeyOpenAI = "";
							break;
						case 1:
							_modifiedMistral = true;
							WorkDayData.Instance.apiKeyMistral = "";
							break;
						case 2:
							_modifiedDeepSeek = true;
							WorkDayData.Instance.apiKeyDeepSeek = "";
							break;
						case 3:
							_modifiedGemini = true;
							WorkDayData.Instance.apiKeyGemini = "";
							break;
						case 4:
							_modifiedOpenRouter = true;
							WorkDayData.Instance.apiKeyOpenRouter = "";
							break;
						case 5:
							_modifiedStability = true;
							WorkDayData.Instance.apiKeyStability = "";
							break;
						case 6:
							_modifiedGrok = true;
							WorkDayData.Instance.apiKeyGrok = "";
							break;
					}
					UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
					PlayerPrefs.SetInt(_cookieProviderChecked, 0);
					LoadValidationKeys();
					string titleError = LanguageController.Instance.GetText("text.error");
					string descriptionError = LanguageController.Instance.GetText("screen.ai.server.verified.error.keys", _nameProviderToCheck);
					ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, this.gameObject, titleError, descriptionError);
					SoundsController.Instance.PlaySoundFX(GameSounds.FxSelection, false, 0.5f);
				}
			}
		}
	}
}