using yourvrexperience.Utils;
using yourvrexperience.Networking;

namespace yourvrexperience.WorkDay
{
	public class MenuStateRun : IBasicState
	{
        public const string EventRunStateChangeState = "EventRunStateChangeState";
        public const string EventRunStateRequestState = "EventRunStateRequestState";
        public const string EventRunStateResponseState = "EventRunStateResponseState";

        public enum StatesRun { None = 0, Loading, Run, Pause, Exit }

		private StatesRun _state;
        private StatesRun _previousState;
        private IBasicState _runState;        
        private bool _changeStateRequested = false;

        public void Initialize()
		{
            SystemEventController.Instance.Event += OnSystemEvent;			
            NetworkController.Instance.NetworkEvent += OnNetworkEvent;

            if (!ApplicationController.Instance.IsMultiplayer)
            {
                ChangeRunState(StatesRun.Loading);
            }
            else
            {
                if (NetworkController.Instance.IsServer)
                {
                    ChangeRunState(StatesRun.Loading);
                }
                else
                {
                    NetworkController.Instance.DispatchNetworkEvent(EventRunStateRequestState, NetworkController.Instance.UniqueNetworkID, -1, NetworkController.Instance.UniqueNetworkID, (int)StatesRun.Loading);
                }
            }
        }

        public void Destroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			if (NetworkController.Instance != null) NetworkController.Instance.NetworkEvent -= OnNetworkEvent;
        }

        public void ChangeRunState(StatesRun newGameState)
        {
            if (!ApplicationController.Instance.IsMultiplayer)
            {
                ChangeLocalRunState(newGameState);
            }
            else
            {
                switch (newGameState)
                {
                    case StatesRun.None:
                    case StatesRun.Loading:
                        ChangeLocalRunState(newGameState);
                        break;

                    default:
                        ChangeRemoteGameState((int)newGameState);
                        break;
                }
            }
        }

        private void ChangeRemoteGameState(int newState)
        {
            if (!_changeStateRequested)
            {
                _changeStateRequested = true;
                NetworkController.Instance.DispatchNetworkEvent(EventRunStateChangeState, NetworkController.Instance.UniqueNetworkID, -1, newState);
            }
        }

        private void ChangeLocalRunState(StatesRun newGameState)
        {
            if (_state == newGameState)
            {
                return;
            }
            if (_runState != null)
            {
                _runState.Destroy();
            }
            _runState = null;
            _previousState = _state;
            _state = newGameState;

            switch (_state)
            {
                case StatesRun.None:
                    break;

                case StatesRun.Loading:
                    _runState = new RunStateLoading();
                    break;

                case StatesRun.Run:
                    _runState = new RunStateRun();
                    break;

                case StatesRun.Pause:
                    _runState = new RunStatePause();
                    break;

                case StatesRun.Exit:
                    _runState = new RunStatePause();
                    break;
            }
            if (_runState != null)
            {
                _runState.Initialize();
            }
        }

        private void OnNetworkEvent(string nameEvent, int originNetworkID, int targetNetworkID, object[] parameters)
        {
            if (nameEvent.Equals(EventRunStateChangeState))
            {
                int newState = (int)parameters[0];
                _changeStateRequested = false;
                ChangeLocalRunState((StatesRun)newState);
            }
            if (nameEvent.Equals(EventRunStateRequestState))
            {
                if (NetworkController.Instance.IsServer)
                {
                    int netIDOrigin = (int)parameters[0];
                    int newState = (int)parameters[1];
                    if (netIDOrigin != NetworkController.Instance.UniqueNetworkID)
                    {
                        NetworkController.Instance.DelayNetworkEvent(EventRunStateResponseState, 0.01f, -1, -1, netIDOrigin, newState);
                    }
                }
            }
            if (nameEvent.Equals(EventRunStateResponseState))
            {
                if (!NetworkController.Instance.IsServer)
                {
                    int netIDOrigin = (int)parameters[0];
                    int newState = (int)parameters[1];
                    if (netIDOrigin == NetworkController.Instance.UniqueNetworkID)
                    {
                        _changeStateRequested = false;
                        ChangeLocalRunState((StatesRun)newState);
                    }
                }
            }
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
		{
            if (nameEvent.Equals(EventRunStateChangeState))
            {                
                ChangeRunState((StatesRun)parameters[0]);
            }
		}

		public void Run()
		{
            if (_runState != null)
            {
                _runState.Run();
            }
        }
    }
}