using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class MenuStateSplash : IBasicState
	{
		public const string EventGameStateSplashCompleted = "EventGameStateSplashCompleted";

		public void Initialize()
		{
			SystemEventController.Instance.Event += OnSystemEvent;

			SystemEventController.Instance.DelaySystemEvent(EventGameStateSplashCompleted, 1);
			CameraFader.Instance.FadeOut();

			ScreenController.Instance.CreateScreen(ScreenSplashView.ScreenName, true, false);
		}

		public void Destroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventGameStateSplashCompleted))
			{
				if (DeviceDectector.IsRunningInMobileDevice())
				{
					UIEventController.Instance.DispatchUIEvent(ScreenSplashView.EventScreenSplashViewSetDescription, LanguageController.Instance.GetText("screen.splash.device.not.authorized"));
				}
				else
				{
					ApplicationController.Instance.ChangeGameState(ApplicationController.StatesGame.Download);
				}
			}
		}

		public void Run()
		{
		}
	}
}