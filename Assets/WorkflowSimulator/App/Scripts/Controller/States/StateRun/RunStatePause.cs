using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class RunStatePause : IBasicState
	{
		public void Initialize()
		{
			SystemEventController.Instance.Event += OnSystemEvent;

			ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoading, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("text.pause"));
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