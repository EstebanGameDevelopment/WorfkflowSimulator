using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class MenuStatePlayOptions : IBasicState
	{
		public void Initialize()
		{
			SystemEventController.Instance.Event += OnSystemEvent;

			ScreenController.Instance.CreateScreen(ScreenSplashView.ScreenName, true, false);
		}

		public void Destroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
		}

		public void Run()
		{
		}
	}
}