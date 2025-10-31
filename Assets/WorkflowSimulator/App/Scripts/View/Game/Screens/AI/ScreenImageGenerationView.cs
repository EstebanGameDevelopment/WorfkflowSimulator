using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class ScreenImageGenerationView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenImageGenerationView";
		
		public const string EventScreenImageGenerationViewImageCompleted = "EventScreenImageGenerationViewImageCompleted";
		public const string EventScreenImageGenerationViewImageCancelled = "EventScreenImageGenerationViewImageCancelled";
		public const string EventScreenImageGenerationViewImageSetColor = "EventScreenImageGenerationViewImageSetColor";

		public enum AIImageProvider { Dalle2 = 0, Dalle3, StabilityAI, Mistral, Gemini, Grok, None }

		public const string ProviderRemoteDalle2 = "Dall.e 2 (OpenAI)";
		public const string ProviderRemoteDalle3 = "Dall.e 3 (OpenAI)";
		public const string ProviderRemoteMistral = "Pixtral (Mistral)";
		public const string ProviderRemoteGemini = "Gemini (Google)";
		public const string ProviderRemoteGrok = "Grok (X)";
		public const string ProviderRemoteStability = "Stability SD3 Large";

		[SerializeField] private TextMeshProUGUI titleImageGen;
		[SerializeField] private TextMeshProUGUI descriptionImageGen;
		[SerializeField] private TextMeshProUGUI titleProvider;
		[SerializeField] private TextMeshProUGUI titleResolution;
		[SerializeField] private TextMeshProUGUI titleQuality;
		[SerializeField] private CustomInput inputDescription;

		[SerializeField] private Button buttonConfirm;
		[SerializeField] private Button buttonCancel;
		[SerializeField] private Button buttonClose;

		[SerializeField] private TMP_Dropdown dropDownProvider;
		[SerializeField] private TMP_Dropdown dropDownResolution;
		[SerializeField] private Slider InputCycles;

		public const string Coockie_Image_Resolution = "Coockie_Image_Resolution";

		public static int GetResolutionImage()
		{
			return PlayerPrefs.GetInt(Coockie_Image_Resolution, -1);
		}

		public static void SetResolutionImage(int resolution)
		{
			PlayerPrefs.SetInt(Coockie_Image_Resolution, resolution);
		}

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			string descriptionParams = "";
			if (parameters.Length > 0)
            {
				descriptionParams = (string)parameters[0];
				if (parameters.Length > 1)
                {
					int projectID = (int)parameters[1];
					ProjectInfoData project = WorkDayData.Instance.CurrentProject.GetProject(projectID);
					if (project != null)
                    {
						_content.GetComponent<Image>().color = project.GetColor();
					}					
				}
			}

			titleImageGen.text = LanguageController.Instance.GetText("screen.imagen.gen.title");
			descriptionImageGen.text = LanguageController.Instance.GetText("screen.imagen.gen.description");
			titleProvider.text = LanguageController.Instance.GetText("screen.imagen.gen.provider");
			titleResolution.text = LanguageController.Instance.GetText("screen.imagen.gen.resolution");
			inputDescription.text = descriptionParams;

			buttonConfirm.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.confirm");
			buttonCancel.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.cancel");

			buttonConfirm.onClick.AddListener(OnRequestImage);
			buttonCancel.onClick.AddListener(OnCancel);
			buttonClose.onClick.AddListener(OnCancel);

			dropDownProvider.options.Clear();
			if ((WorkDayData.Instance.apiKeyOpenAI.Length > 2) || (WorkDayData.Instance.apiKeyOpenRouter.Length > 2))
			{
				dropDownProvider.options.Add(new TMP_Dropdown.OptionData(ProviderRemoteDalle2));
				dropDownProvider.options.Add(new TMP_Dropdown.OptionData(ProviderRemoteDalle3));
			}
			if ((WorkDayData.Instance.apiKeyMistral.Length > 2) || (WorkDayData.Instance.apiKeyOpenRouter.Length > 2))
			{
				dropDownProvider.options.Add(new TMP_Dropdown.OptionData(ProviderRemoteMistral));
			}
			if ((WorkDayData.Instance.apiKeyGemini.Length > 2) || (WorkDayData.Instance.apiKeyOpenRouter.Length > 2))
			{
				dropDownProvider.options.Add(new TMP_Dropdown.OptionData(ProviderRemoteGemini));
			}
			if ((WorkDayData.Instance.apiKeyGrok.Length > 2) || (WorkDayData.Instance.apiKeyGrok.Length > 2))
			{
				dropDownProvider.options.Add(new TMP_Dropdown.OptionData(ProviderRemoteGrok));
			}
			if (WorkDayData.Instance.apiKeyStability.Length > 2)
			{
				dropDownProvider.options.Add(new TMP_Dropdown.OptionData(ProviderRemoteStability));
			}
			dropDownProvider.onValueChanged.AddListener(OnProviderChanged);
			OnProviderChanged(0);

			dropDownResolution.onValueChanged.AddListener(OnResolutionChanged);

			InputCycles.value = 0.5f;

            UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAIView.EventScreenFinalRequestAIViewOpened);            
        }

        public override void Destroy()
		{
			base.Destroy();

            UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAIView.EventScreenFinalRequestAIViewClosed);
        }

		private void OnCancel()
		{
			SystemEventController.Instance.DispatchSystemEvent(EventScreenImageGenerationViewImageCancelled);
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private AIImageProvider GetImageProviderIndex(string newProvider)
		{
			if (newProvider.Equals(ProviderRemoteDalle2)) return AIImageProvider.Dalle2;
			if (newProvider.Equals(ProviderRemoteDalle3)) return AIImageProvider.Dalle3;
			if (newProvider.Equals(ProviderRemoteStability)) return AIImageProvider.StabilityAI;
			if (newProvider.Equals(ProviderRemoteMistral)) return AIImageProvider.Mistral;
			if (newProvider.Equals(ProviderRemoteGemini)) return AIImageProvider.Gemini;
			if (newProvider.Equals(ProviderRemoteGrok)) return AIImageProvider.Grok;
			return AIImageProvider.None;
		}

		private void OnProviderChanged(int valueSelected)
		{
			AIImageProvider value = GetImageProviderIndex(dropDownProvider.options[valueSelected].text);

			dropDownResolution.ClearOptions();
			int finalId = 0;
			switch (value)
			{
				case AIImageProvider.Dalle2:
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("256x256"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("512x512"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("1024x1024"));
					dropDownResolution.value = 0;
					finalId = ((GetResolutionImage() == -1) ? 1 : GetResolutionImage());
					dropDownResolution.value = ((finalId >= 3) ? 1 : finalId);
					break;

				case AIImageProvider.Dalle3:
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("1024x1024"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("1792x1024"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("1024x1792"));
					dropDownResolution.value = 0;
					finalId = ((GetResolutionImage() == -1) ? 1 : GetResolutionImage());
					dropDownResolution.value = ((finalId >= 3) ? 1 : finalId);
					break;

				case AIImageProvider.StabilityAI:
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("256x256"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("512x512"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("720x405"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("720x540"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("1024x576"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("1024x768"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("1024x1024"));
					dropDownResolution.value = 0;
					finalId = ((GetResolutionImage() == -1) ? 4 : GetResolutionImage());
					dropDownResolution.value = ((finalId >= 7) ? 4 : finalId);
					break;

				case AIImageProvider.Mistral:
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("256x256"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("512x512"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("720x405"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("720x540"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("1024x576"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("1024x768"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("1024x1024"));
					dropDownResolution.value = 0;
					finalId = ((GetResolutionImage() == -1) ? 4 : GetResolutionImage());
					dropDownResolution.value = ((finalId >= 7) ? 4 : finalId);
					break;

				case AIImageProvider.Gemini:
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("256x256"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("512x512"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("720x405"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("720x540"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("1024x576"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("1024x768"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("1024x1024"));
					dropDownResolution.value = 0;
					finalId = ((GetResolutionImage() == -1) ? 4 : GetResolutionImage());
					dropDownResolution.value = ((finalId >= 7) ? 4 : finalId);
					break;

				case AIImageProvider.Grok:
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("256x256"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("512x512"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("720x405"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("720x540"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("1024x576"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("1024x768"));
					dropDownResolution.options.Add(new TMP_Dropdown.OptionData("1024x1024"));
					dropDownResolution.value = 0;
					finalId = ((GetResolutionImage() == -1) ? 4 : GetResolutionImage());
					dropDownResolution.value = ((finalId >= 7) ? 4 : finalId);
					break;
			}
			dropDownResolution.RefreshShownValue();
		}

		private void OnResolutionChanged(int value)
		{
			SetResolutionImage(value);
		}

		private void OnRequestImage()
		{
			string descriptionImage = inputDescription.text;
			if (descriptionImage.Length < 10)
            {
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.error"), LanguageController.Instance.GetText("screen.imagen.gen.text.to.short.to.generate"));
				return;
            }

			string sizeData = dropDownResolution.options[dropDownResolution.value].text;
			string[] tokensSize = sizeData.Split('x');
			int widthImage = int.Parse(tokensSize[0]);
			int heightImage = int.Parse(tokensSize[1]);

			AIImageProvider valueProvider = GetImageProviderIndex(dropDownProvider.options[dropDownProvider.value].text);
			switch (valueProvider)
            {
				case AIImageProvider.Mistral:
				case AIImageProvider.Gemini:
					descriptionImage += "\n\n" + LanguageController.Instance.GetText("screen.imagen.gen.provide.image.with.resolution", sizeData);
					break;
            }
			GameObject screenLoading = ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("screen.image.now.creating.image.please.wait"));
			UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, screenLoading, _canvas.sortingOrder + 1);
			SystemEventController.Instance.DispatchSystemEvent(ImageDatabaseController.EventImageDatabaseControllerGenerateImage, (int)valueProvider, InputCycles.value, widthImage, heightImage, descriptionImage, EventScreenImageGenerationViewImageCompleted);
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}
	}
}