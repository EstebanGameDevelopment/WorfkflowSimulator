using UnityEngine;
using yourvrexperience.Utils;
using yourvrexperience.Networking;
using yourvrexperience.ai;

namespace yourvrexperience.WorkDay
{
	public class MenuStateLoad : IBasicState
	{
		private bool _processCompleted = false;

		public void Initialize()
		{
			SystemEventController.Instance.Event += OnSystemEvent;
			NetworkController.Instance.NetworkEvent += OnNetworkEvent;

			if (!ApplicationController.Instance.IsMultiplayer)
            {
				SystemEventController.Instance.DelaySystemEvent(ApplicationController.EventMainControllerGameReadyToStart, 0.2f);
			}
		}

		public void Destroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			if (NetworkController.Instance != null) NetworkController.Instance.NetworkEvent -= OnNetworkEvent;
		}

		private void CreateGameElements()
		{
			if (!_processCompleted)
			{
				_processCompleted = true;
				SystemEventController.Instance.DispatchSystemEvent(ApplicationController.EventMainControllerReleaseGameResources, false);
				ApplicationController.Instance.CreateGameElementsView();
			}
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ApplicationController.EventMainControllerGameReadyToStart))
			{
				CreateGameElements();
			}
			if (nameEvent.Equals(ApplicationController.EventMainControllerAllPlayerViewReadyToStartGame))
            {
				ApplicationController.Instance.SetUpAISession();
			}
			if (nameEvent.Equals(InitProviderLLMHTTP.EventInitProviderLLMHTTPCompleted))
            {
				if (!ApplicationController.Instance.IsMultiplayer)
				{
					ApplicationController.Instance.ChangeGameState(ApplicationController.StatesGame.Run);
				}
			}
		}

		private void OnNetworkEvent(string nameEvent, int originNetworkID, int targetNetworkID, object[] parameters)
		{
			if (nameEvent.Equals(ApplicationController.EventMainControllerGameReadyToStart))
			{
				CreateGameElements();
			}
			if (nameEvent.Equals(ApplicationController.EventMainControllerAllPlayerViewReadyToStartGame))
			{
				ApplicationController.Instance.SetUpAISession();
			}
		}

		public void Run()
		{
		}
	}
}