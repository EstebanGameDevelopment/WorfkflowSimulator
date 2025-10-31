using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.UserManagement;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class ScreenMainMenuView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenMainMenuView";

		public const string EventScreenMainMenuViewPlayGame = "EventScreenMainMenuViewPlayGame";
		public const string EventScreenMainMenuViewEditGame = "EventScreenMainMenuViewEditGame";
		public const string EventScreenMainMenuViewSettings = "EventScreenMainMenuViewSettings";
		public const string EventScreenMainMenuViewExitGame = "EventScreenMainMenuViewExitGame";
		public const string EventScreenMainMenuViewChangeVideoTutorial = "EventScreenMainMenuViewChangeVideoTutorial";

		[SerializeField] private TextMeshProUGUI versionApp;
		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private TextMeshProUGUI descriptionScreen;
		[SerializeField] private CustomButton buttonPlayStory;
		[SerializeField] private CustomButton buttonEditStory;
		[SerializeField] private CustomButton buttonInfoSecurity;
		[SerializeField] private CustomButton buttonRefresh;
		[SerializeField] private TextMeshProUGUI contextFeedback;
		[SerializeField] private Button buttonExit;
		[SerializeField] private Button buttonSettings;
		[SerializeField] private TextAsset agreementFile;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			buttonPlayStory.onClick.AddListener(OnButtonPlayStory);
			buttonEditStory.onClick.AddListener(OnButtonEditStory);
			buttonInfoSecurity.onClick.AddListener(OnButtonInfoAgreement);
			buttonRefresh.onClick.AddListener(OnButtonRefresh);
			buttonExit.onClick.AddListener(OnButtonExit);
			buttonSettings.onClick.AddListener(OnButtonSettings);

			contextFeedback.text = "";

			UpdateLocalTexts();

			buttonPlayStory.PointerEnterButton += OnPlayStoryEnter;
			buttonEditStory.PointerEnterButton += OnEditStoryEnter;
			buttonInfoSecurity.PointerEnterButton += OnInformationEnter;
			buttonRefresh.PointerEnterButton += OnRefreshEnter;

			buttonPlayStory.PointerExitButton += OnClearContextHelp;
			buttonEditStory.PointerExitButton += OnClearContextHelp;
			buttonInfoSecurity.PointerExitButton += OnClearContextHelp;
			buttonRefresh.PointerExitButton += OnClearContextHelp;

			SystemEventController.Instance.Event += OnSystemEvent;

			versionApp.text = LanguageController.Instance.GetText("text.version") + " " + Application.version;
			
			if (UsersController.Instance.CurrentUser != null)
            {
				buttonRefresh.interactable = ((UsersController.Instance.CurrentUser.Email.Length > 0) && (UsersController.Instance.CurrentUser.Password.Length > 0));
			}
			else
            {
				buttonRefresh.interactable = false;
			}

#if !UNITY_EDITOR && UNITY_WEBGL
			buttonExit.gameObject.SetActive(false);
#endif
		}


        public override void Destroy()
		{
			base.Destroy();

			SystemEventController.Instance.Event -= OnSystemEvent;
		}

		private void UpdateLocalTexts()
		{
			titleScreen.text = LanguageController.Instance.GetText("screen.main.menu.title");

			if ((UsersController.Instance.CurrentUser == null) || (UsersController.Instance.CurrentUser.Id == -1))
			{
				descriptionScreen.text = LanguageController.Instance.GetText("screen.main.menu.description.register.first");
			}
			else
			{
				descriptionScreen.text = LanguageController.Instance.GetText("screen.main.menu.description");
			}
		}

		private void OnInformationEnter(CustomButton value)
		{
			contextFeedback.text = LanguageController.Instance.GetText("screen.main.menu.context.information.no.responsible");
		}

		private void OnPlayStoryEnter(CustomButton obj)
		{
			contextFeedback.text = LanguageController.Instance.GetText("screen.main.menu.context.help.play.workflow");
		}

		private void OnEditStoryEnter(CustomButton obj)
		{
			contextFeedback.text = LanguageController.Instance.GetText("screen.main.menu.context.help.edit.workflow");
		}

		private void OnRefreshEnter(CustomButton value)
		{
			contextFeedback.text = LanguageController.Instance.GetText("screen.main.menu.context.refresh.access");
		}

		private void OnClearContextHelp(CustomButton obj)
		{
			contextFeedback.text = "";
		}

		private void OnButtonRefresh()
		{
			if ((UsersController.Instance.CurrentUser.Email.Length > 0) && (UsersController.Instance.CurrentUser.Password.Length > 0))
			{
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("screen.main.menu.checking.mail.validation"));
				UserModel.LoginWithStoredLogin();
			}
		}

		private void OnButtonInfoAgreement()
		{
			ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenMediumInput, null, LanguageController.Instance.GetText("screen.main.menu.context.information.no.responsible"), "");
			UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, agreementFile.text);
		}

		private void OnButtonExit()
		{
			UIEventController.Instance.DispatchUIEvent(EventScreenMainMenuViewExitGame, this.gameObject);
		}

		private void OnButtonPlayStory()
		{
			UIEventController.Instance.DispatchUIEvent(EventScreenMainMenuViewPlayGame, this.gameObject);
		}

		private void OnButtonEditStory()
		{
			UIEventController.Instance.DispatchUIEvent(EventScreenMainMenuViewEditGame, this.gameObject);
		}

		private void OnButtonSettings()
		{
			UIEventController.Instance.DispatchUIEvent(EventScreenMainMenuViewSettings);
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(LanguageController.EventLanguageControllerChangedCodeLanguage))
			{
				UpdateLocalTexts();
			}
			if (nameEvent.Equals(UsersController.EVENT_USER_LOGIN_FORMATTED))
			{
				UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
			}
		}
	}
}