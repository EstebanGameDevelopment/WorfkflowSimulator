using yourvrexperience.Utils;
using yourvrexperience.Networking;

namespace yourvrexperience.WorkDay
{
	public class MenuStateReleaseMemory : IBasicState
	{
		public const string EventGameStateReleaseMemoryStart = "EventGameStateReleaseMemoryStart";
		public const string EventGameStateReleaseMemoryEnd = "EventGameStateReleaseMemoryEnd";

		public void Initialize()
		{
			SystemEventController.Instance.Event += OnSystemEvent;
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyAllScreens);

            string titleList = LanguageController.Instance.GetText("text.info");
            string descriptionList = LanguageController.Instance.GetText("text.releasing.resources");
            ScreenController.Instance.CreateScreen(ScreenWaitProgressView.ScreenName, false, false, titleList, descriptionList);
			NetworkController.Instance.Disconnect();

			SystemEventController.Instance.DelaySystemEvent(EventGameStateReleaseMemoryStart, 0.2f);
		}

		public void Destroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventGameStateReleaseMemoryStart))
            {
				SystemEventController.Instance.DispatchSystemEvent(ApplicationController.EventMainControllerReleaseGameResources, true);
				SystemEventController.Instance.DispatchSystemEvent(PlayerView.EventPlayerViewReleaseGameResources);
				SystemEventController.Instance.DelaySystemEvent(EventGameStateReleaseMemoryEnd, 0.4f);
				WorkDayData.Instance.DestroySession();
			}
			if (nameEvent.Equals(EventGameStateReleaseMemoryEnd))
            {
				ApplicationController.Instance.ChangeGameState(ApplicationController.StatesGame.MainMenu);
			}
		}

		public void Run()
		{
		}
	}
}