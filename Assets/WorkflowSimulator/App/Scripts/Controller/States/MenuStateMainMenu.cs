using UnityEngine;
using yourvrexperience.UserManagement;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class MenuStateMainMenu : IBasicState
	{
		public const float BoxGunShiftFromCamera = -1;

		public const string EventGameStateMenuPositionReady = "EventGameStateMenuPositionReady";
		public const string EventGameStateMenuQuitGame = "EventGameStateMenuQuitGame";

		public const string SubEventExitAppConfirmation = "SubEventExitAppConfirmation";

		private GameObject _source;

		private bool _isEditRequested = true;

		public void Initialize()
		{
			UIEventController.Instance.Event += OnUIEvent;
			SystemEventController.Instance.Event += OnSystemEvent;

			if (ApplicationController.Instance.PreviousState == ApplicationController.StatesGame.None)
			{
				ApplicationController.Instance.FadeOutCamera();
			}

			ButtonOpenLink.IsEnabled = true;

			ScreenController.Instance.CreateScreen(ScreenMainMenuView.ScreenName, true, false);
		}

		public void Destroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ScreenMainMenuView.EventScreenMainMenuViewPlayGame))
			{
				if ((UsersController.Instance.CurrentUser != null) 
					&& (UsersController.Instance.CurrentUser.Email.Length > 0) 
					&& (UsersController.Instance.CurrentUser.Password.Length > 0))
				{
					if (UsersController.Instance.CurrentUser.Validated)
					{
                        if (WorkDayData.Instance.CheckLoadedAnyAPIKey())
                        {
                            ApplicationController.Instance.IsPlayMode = true;
                            ApplicationController.Instance.ChangeGameState(ApplicationController.StatesGame.EditOptions);
                        }
                        else
                        {
                            ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.ai.server.api.at.least.one.key"));
                        }
					}
					else
					{
						ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.main.menu.email.not.validated"));
					}
				}
				else
                {
					ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("message.warning"), LanguageController.Instance.GetText("screen.main.menu.login.before.entering"));
				}
			}
			if (nameEvent.Equals(ScreenMainMenuView.EventScreenMainMenuViewEditGame))
			{
				if ((UsersController.Instance.CurrentUser != null)
					&& (UsersController.Instance.CurrentUser.Email.Length > 0)
					&& (UsersController.Instance.CurrentUser.Password.Length > 0))
				{
					if (UsersController.Instance.CurrentUser.Validated)
					{
						if (UsersController.Instance.CurrentUser.GetLevel() > 0)
						{
							if (WorkDayData.Instance.CheckLoadedAnyAPIKey())
							{
                                ApplicationController.Instance.IsPlayMode = false;
                                ApplicationController.Instance.ChangeGameState(ApplicationController.StatesGame.EditOptions);
                            }
                            else
							{
                                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.ai.server.api.at.least.one.key"));
                            }
						}
						else
						{
							ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("message.warning"), LanguageController.Instance.GetText("screen.main.menu.no.permission.access"));
						}
					}
					else
					{
						ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("message.warning"), LanguageController.Instance.GetText("screen.main.menu.email.not.validated"));
					}
				}
				else
				{
					ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("message.warning"), LanguageController.Instance.GetText("screen.main.menu.login.before.entering"));
				}
			}
			if (nameEvent.Equals(ScreenMainMenuView.EventScreenMainMenuViewSettings))
			{
				ApplicationController.Instance.ChangeGameState(ApplicationController.StatesGame.Settings);
			}
			if (nameEvent.Equals(ScreenMainMenuView.EventScreenMainMenuViewExitGame))
			{
				_source = (GameObject)parameters[0];
				string titleWarning = LanguageController.Instance.GetText("text.warning");
				string textAskToExit = LanguageController.Instance.GetText("screen.main.do.you.want.to.exit");
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, _source, titleWarning, textAskToExit, SubEventExitAppConfirmation);
			}
			if (nameEvent.Equals(SubEventExitAppConfirmation))
			{
				ScreenInformationResponses userResponse = (ScreenInformationResponses)parameters[1];
				if (userResponse == ScreenInformationResponses.Confirm)
				{
					ScreenController.Instance.DestroyScreens();
					string titleInfo = LanguageController.Instance.GetText("text.info");
					string textNowExiting = LanguageController.Instance.GetText("screen.main.now.exiting");
					ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoading, _source, titleInfo, textNowExiting);
					SystemEventController.Instance.DelaySystemEvent(EventGameStateMenuQuitGame, 2);
				}
			}
        }

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
		}

		public void Run()
		{
		}
	}
}