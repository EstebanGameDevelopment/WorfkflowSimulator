using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class MenuStateSettings : IBasicState
	{
		public void Initialize()
		{
			SystemEventController.Instance.Event += OnSystemEvent;
			UIEventController.Instance.Event += OnUIEvent;

			ScreenController.Instance.CreateScreen(ScreenSettingsView.ScreenName, true, false);
		}

        public void Destroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{

		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ScreenSettingsView.EventScreenSettingsViewBack))
			{
				ApplicationController.Instance.ChangeGameState(ApplicationController.StatesGame.MainMenu);
			}
		}

		public void Run()
		{
		}
	}
}