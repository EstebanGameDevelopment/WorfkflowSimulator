using yourvrexperience.Utils;
using yourvrexperience.Networking;
using static yourvrexperience.WorkDay.ApplicationController;

namespace yourvrexperience.WorkDay
{
	public class MenuStateConnecting : IBasicState
	{
		public void Initialize()
		{
			SystemEventController.Instance.Event += OnSystemEvent;
			NetworkController.Instance.NetworkEvent += OnNetworkEvent;

			ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoading, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("text.connecting"));

			NetworkController.Instance.Initialize();
			NetworkController.Instance.Connect();
		}

		public void Destroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			if (NetworkController.Instance != null) NetworkController.Instance.NetworkEvent -= OnNetworkEvent;
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{

		}

		private void OnNetworkEvent(string nameEvent, int originNetworkID, int targetNetworkID, object[] parameters)
		{
			if (nameEvent.Equals(NetworkController.EventNetworkControllerListRoomsConfirmedUpdated))
			{
				if (!ApplicationController.Instance.HasStartedSession)
				{
					ApplicationController.Instance.HasStartedSession = true;
					if (!NetworkController.Instance.ExistNameRoom(ApplicationController.Instance.RoomName))
                    {
#if ENABLE_MIRROR
						ApplicationController.Instance.NumberClients = -1;
						NetworkController.Instance.JoinRoom(ApplicationController.Instance.RoomName);
#else
						ApplicationController.Instance.NumberClients = 10;
						NetworkController.Instance.CreateRoom(ApplicationController.Instance.RoomName, ApplicationController.Instance.NumberClients);
#endif
					}
					else
					{
						NetworkController.Instance.JoinRoom(ApplicationController.Instance.RoomName);
					}
				}
			}
			if (nameEvent.Equals(NetworkController.EventNetworkControllerConfirmationConnectionWithRoom))
			{
				if (ApplicationController.Instance.State == StatesGame.Connecting)
				{
					ApplicationController.Instance.ChangeGameState(StatesGame.Loading);
				}
				NetworkController.Instance.DelayNetworkEvent(EventMainControllerGameReadyToStart, 0.2f, -1, -1);
			}
		}

		public void Run()
		{
		}
	}
}