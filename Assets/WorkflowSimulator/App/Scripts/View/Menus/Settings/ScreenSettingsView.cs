using Assets.SimpleSignIn.Google.Scripts;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.UserManagement;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class ScreenSettingsView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenSettingsView";

		public const string GooglePassword = "CTdh8003Tfnvks97Ff";

		public const string EVENT_REGISTER_PLATFORM_MESSAGE_DELAYED_REPORT_NEW_USER = "EVENT_REGISTER_PLATFORM_MESSAGE_DELAYED_REPORT_NEW_USER";

		public const string EventRegisterPlatformFinallyLogin = "EventRegisterPlatformFinallyLogin";

		public const string EventScreenSettingsViewBack = "EventScreenSettingsViewBack";

		public const string SubEventRemoveConfirmationResponse = "SubEventRemoveConfirmationResponse";
		public const string SubEventLogoutConfirmationResponse = "SubEventLogoutConfirmationResponse";
		public const string SubEventShowOptionsAfterRemoveAccount = "SubEventShowOptionsAfterRemoveAccount";
		public const string SubEventRemoveInfoGoogleError = "SubEventRemoveInfoGoogleError";
		public const string SubEventRegisterForPlatformConfirmation = "SubEventRegisterForPlatformConfirmation";

		private enum ConfigSettings { Loading, Options, LoggedIn }

		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private GameObject contentLoading;
		[SerializeField] private GameObject contentOptions;
		[SerializeField] private GameObject contentLoggedIn;
		[SerializeField] private Button buttonCloud;
		[SerializeField] private Button buttonBack;

		[SerializeField] private TextMeshProUGUI loadingTitle;

		[SerializeField] private Button buttonRegisterMail;
		[SerializeField] private Button buttonRegisterGoogle;

		[SerializeField] private TextMeshProUGUI emailValue;
		[SerializeField] private Button buttonLogout;
		[SerializeField] private Button buttonRemove;

		private ConfigSettings _config;
		private int _iterationsRequest = 0;
		private string _emailToCheck = "";
		private string _passwordToCheck = "";

		private bool _reportSoundLoggedIn = false;

		private GoogleAuth _googleAuth;

		private string _platformToDelete;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			titleScreen.text = LanguageController.Instance.GetText("screen.settings.title");

			buttonRegisterMail.onClick.AddListener(OnRegisterMail);
			buttonRegisterMail.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.settings.register.email.option");
			buttonRegisterGoogle.onClick.AddListener(OnRegisterGoogle);
			buttonRegisterGoogle.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.settings.register.google.option");

			buttonRegisterGoogle.gameObject.SetActive(true);

			_googleAuth = new GoogleAuth();
			_googleAuth.TryResume(OnSignInGoogle, OnGetTokenResponseGoogle);

			loadingTitle.text = LanguageController.Instance.GetText("screen.settings.loading.info");

			buttonLogout.onClick.AddListener(OnLogout);
			buttonLogout.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.settings.loggedin.logout");
			buttonRemove.onClick.AddListener(OnRemoveAccount);
			buttonRemove.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.settings.loggedin.remove.account");

			buttonBack.onClick.AddListener(OnButtonBack);
			buttonCloud.onClick.AddListener(OnButtonCloud);

			SystemEventController.Instance.Event += OnSystemEvent;
			UIEventController.Instance.Event += OnUIEvent;

			ChangeConfiguration(ConfigSettings.Loading);

			if (!LogIn())
			{
				ChangeConfiguration(ConfigSettings.Options);
			}
			else
            {
				ChangeConfiguration(ConfigSettings.LoggedIn);
			}

			if ((UsersController.Instance.CurrentUser == null) || (UsersController.Instance.CurrentUser.Id == -1))
			{
				_reportSoundLoggedIn = true;
			}
		}

		public override void Destroy()
		{
			base.Destroy();

			_googleAuth = null;

			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
		}

		private void OnButtonCloud()
		{
			ScreenController.Instance.CreateScreen(ScreenAIServersView.ScreenName, false, true);
		}

		private bool LogIn()
		{
			if (UsersController.Instance.CurrentUser == null)
			{
				return false;
			}
			else
			{
				if ((UsersController.Instance.CurrentUser.Email.Length > 0) && (UsersController.Instance.CurrentUser.Password.Length > 0))
				{
					UserModel.LoginWithStoredLogin();
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		private void ChangeConfiguration(ConfigSettings newConfig)
		{
			_config = newConfig;

			switch (_config)
			{
				case ConfigSettings.Loading:
					contentLoading.SetActive(true);
					contentOptions.SetActive(false);
					contentLoggedIn.SetActive(false);
					break;

				case ConfigSettings.Options:
					contentLoading.SetActive(false);
					contentOptions.SetActive(true);
					contentLoggedIn.SetActive(false);
					break;

				case ConfigSettings.LoggedIn:
					contentLoading.SetActive(false);
					contentOptions.SetActive(false);
					contentLoggedIn.SetActive(true);

					emailValue.text = UsersController.Instance.CurrentUser.Email;
					break;
			}
		}

		private void OnRegisterGoogle()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
			ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoading, this.gameObject, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"));

			_googleAuth.SignIn(OnSignInGoogle, caching: true);
		}

		public void SignOutGoogle()
		{
			_googleAuth.SignOut(revokeAccessToken: true);
		}

		private void OnSignInGoogle(bool success, string error, UserInfo userInfo)
		{
#if UNITY_EDITOR
            Debug.Log("OnSignInGoogle::success=" + success);
#endif
			if (success)
			{
				_emailToCheck = userInfo.email;
				_passwordToCheck = SHAEncryption.GenerateShortHash(_emailToCheck, GooglePassword, 8);
#if UNITY_EDITOR
                Debug.Log("OnSignInGoogle::email[" + _emailToCheck + "]::PWD[" + _passwordToCheck + "]");
#endif
				UIEventController.Instance.DelayUIEvent(UsersController.EVENT_USER_SOCIAL_LOGIN_REQUEST, 0.1f, _emailToCheck, _passwordToCheck, LoginPlatforms.Email, UserModel.ACCOUNT_DATA_GOOGLE);
			}
			else
			{
				UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
				string titleInfoComplete = LanguageController.Instance.GetText("message.error");
				string descriptionInfoComplete = LanguageController.Instance.GetText("screen.register.login.with.platform.has.failed");
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, titleInfoComplete, descriptionInfoComplete);
			}
		}

		private void OnGetTokenResponseGoogle(bool success, string error, TokenResponse tokenResponse)
		{
			if (success)
			{
			}
		}

		private void OnRegisterMail()
		{
			ScreenController.Instance.CreateScreen(ScreenLoginUserView.ScreenName, false, true);
		}

		private void OnRemoveAccount()
		{
			string titleRemoveConfirmation = LanguageController.Instance.GetText("message.warning");
			string descriptionRemoveConfirmation = LanguageController.Instance.GetText("screen.settings.loggedin.question.confirmation.delete.account");
			ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, this.gameObject, titleRemoveConfirmation, descriptionRemoveConfirmation, SubEventRemoveConfirmationResponse);
		}

		private void OnLogout()
		{
			string titleRemoveConfirmation = LanguageController.Instance.GetText("message.warning");
			string descriptionRemoveConfirmation = LanguageController.Instance.GetText("screen.settings.loggedin.question.confirmation.logout.account");
			ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, this.gameObject, titleRemoveConfirmation, descriptionRemoveConfirmation, SubEventLogoutConfirmationResponse);
		}

		private void OnButtonBack()
		{			
			UIEventController.Instance.DispatchUIEvent(EventScreenSettingsViewBack);
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(SubEventShowOptionsAfterRemoveAccount))
			{
				ChangeConfiguration(ConfigSettings.Options);
			}
			if (nameEvent.Equals(SubEventRemoveConfirmationResponse))
			{
				if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
				{
					_platformToDelete = UsersController.Instance.CurrentUser.PlatformData;

					UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
					string titleRemStory = LanguageController.Instance.GetText("message.info");
					string descriptionRemovingStories = LanguageController.Instance.GetText("screen.creating.now.removing.account.in.progress");
					ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoading, this.gameObject, titleRemStory, descriptionRemovingStories);

					WorkDayData.Instance.DeleteUserAccount();
				}
			}
			if (nameEvent.Equals(SubEventLogoutConfirmationResponse))
			{
				if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
				{
					UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
					SystemEventController.Instance.DispatchSystemEvent(UsersController.EVENT_USER_RESET_LOCAL_DATA);
					ChangeConfiguration(ConfigSettings.Options);
					SoundsController.Instance.PlaySoundFX(GameSounds.FxConfirmation, false, GameSounds.VolumeFXConfirmation);
					_reportSoundLoggedIn = true;
				}
			}
			if (nameEvent.Equals(SubEventRegisterForPlatformConfirmation))
			{
				UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoading, this.gameObject, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"));
				SystemEventController.Instance.DelaySystemEvent(EventRegisterPlatformFinallyLogin, 2);
			}
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(DeleteUserAccountDataHTTP.EventDeleteUserAccountDataHTTPCompleted))
			{
				if ((bool)parameters[0])
				{
					string titleWait = LanguageController.Instance.GetText("screen.wait.register.title");
					string descriptionWait = LanguageController.Instance.GetText("screen.settings.loggedin.removing.account");
					ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoading, this.gameObject, titleWait, descriptionWait);
					UIEventController.Instance.DelayUIEvent(UsersController.EVENT_USER_REMOVE_SINGLE_RECORD, 0.2f, UsersController.Instance.CurrentUser.Id);
				}
				else
				{
					UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
					string titleWait = LanguageController.Instance.GetText("message.error");
					string descriptionFailedRemove = LanguageController.Instance.GetText("screen.settings.loggedin.removed.account.failed");
					ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, this.gameObject, titleWait, descriptionFailedRemove);
				}
			}
			if (nameEvent.Equals(UsersController.EVENT_USER_CONFIRMATION_REMOVED_RECORD))
			{
				if ((bool)parameters[0])
				{
					SystemEventController.Instance.DispatchSystemEvent(UsersController.EVENT_USER_RESET_LOCAL_DATA);
					if (_platformToDelete.Equals(UserModel.ACCOUNT_DATA_GOOGLE))
					{
						_platformToDelete = "";
						try
						{
							SignOutGoogle();
						}
						catch (Exception err) { }
					}
					UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
					string titleWait = LanguageController.Instance.GetText("message.info");
					string descriptionWait = LanguageController.Instance.GetText("screen.settings.loggedin.removed.success");
					ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, this.gameObject, titleWait, descriptionWait, SubEventShowOptionsAfterRemoveAccount);
				}
				else
				{
					UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
					string titleWait = LanguageController.Instance.GetText("message.error");
					string descriptionFailedRemove = LanguageController.Instance.GetText("screen.settings.loggedin.removed.account.failed");
					ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, this.gameObject, titleWait, descriptionFailedRemove);
				}
				SoundsController.Instance.PlaySoundFX(GameSounds.FxConfirmation, false, GameSounds.VolumeFXConfirmation);
			}
			if (nameEvent.Equals(UsersController.EVENT_USER_REGISTER_CONFIRMATION))
			{
				bool success = (bool)parameters[0];
				if (success)
				{
					UIEventController.Instance.DelayUIEvent(UsersController.EVENT_USER_LOGIN_REQUEST, 0.1f, _emailToCheck, _passwordToCheck, LoginPlatforms.Email);
				}
				else
				{
					UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
					string titleInfoError = LanguageController.Instance.GetText("message.error");
					string descriptionInfoError = LanguageController.Instance.GetText("screen.register.wrong.register");
					ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, this.gameObject, titleInfoError, descriptionInfoError);
				}
			}
			if (nameEvent.Equals(EVENT_REGISTER_PLATFORM_MESSAGE_DELAYED_REPORT_NEW_USER))
			{
				UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
				string titleInfoComplete = LanguageController.Instance.GetText("message.info");
				string descriptionInfoComplete = LanguageController.Instance.GetText("screen.register.check.email");
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, titleInfoComplete, descriptionInfoComplete, SubEventRegisterForPlatformConfirmation);
			}
			if (nameEvent.Equals(EventRegisterPlatformFinallyLogin))
			{
				UIEventController.Instance.DelayUIEvent(UsersController.EVENT_USER_LOGIN_REQUEST, 0.1f, UsersController.Instance.CurrentRegisterEmail, UsersController.Instance.CurrentRegisterPassword, UsersController.Instance.CurrentRegisterPlatform, UsersController.Instance.CurrentAccessToken);
			}
			if (nameEvent.Equals(UsersController.EVENT_USER_LOGIN_FORMATTED))
			{
				if ((bool)parameters[0])
				{
					UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
					ChangeConfiguration(ConfigSettings.LoggedIn);
				}
				else
				{
					_iterationsRequest++;
					if (_iterationsRequest < 0)
					{
						if ((UsersController.Instance.CurrentRegisterEmail != null) && (UsersController.Instance.CurrentRegisterEmail.Length > 0))
						{
							UIEventController.Instance.DelayUIEvent(UsersController.EVENT_USER_LOGIN_REQUEST, 3f, UsersController.Instance.CurrentRegisterEmail, UsersController.Instance.CurrentRegisterPassword, UsersController.Instance.CurrentRegisterPlatform, UsersController.Instance.CurrentAccessToken);
						}
						else
						{
							UIEventController.Instance.DelayUIEvent(UsersController.EVENT_USER_LOGIN_REQUEST, 3f, UsersController.Instance.CurrentUser.Email, UsersController.Instance.CurrentUser.Password, UsersController.Instance.CurrentRegisterPlatform, UsersController.Instance.CurrentAccessToken);
						}
					}
					else
					{
						UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
						string titleInfoComplete = LanguageController.Instance.GetText("message.error");
						string descriptionInfoComplete = LanguageController.Instance.GetText("screen.register.login.with.platform.has.failed");
						ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, titleInfoComplete, descriptionInfoComplete);
						ChangeConfiguration(ConfigSettings.Options);
					}
				}
			}
		}
	}
}