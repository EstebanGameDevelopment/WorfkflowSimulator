using UnityEngine;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.EditionSubStateAreas;

namespace yourvrexperience.WorkDay
{
	public class CommandGoToAreaChair : CommandGoToBase, IGameCommand
	{
		public const string EventCommandGoToAreaChairDestinationReached = "EventCommandGoToAreaChairDestinationReached";

		private HumanView _human = null;
		private bool _isRunning = false;
		private GameObject _targetChairGO = null;
		private string _areaNameRoom;

		public MeetingData Meeting
		{
			get { return null; }
		}

		public string Name
		{
			get { return "GoToAreaChair"; }
		}

		public override void Initialize(params object[] parameters)
		{
			base.Initialize();

			_member = (string)parameters[0];			
			_areaNameRoom = (string)parameters[1];
			_timeToStart = 0;
			if (parameters.Length > 2)
            {
				_timeToStart = (float)parameters[2];
			}
			
			var (humanGO, humanData) = ApplicationController.Instance.LevelView.GetItemByName(_member);
			if (humanGO == null)
			{
				_isCompleted = true;
			}
			else
			{
				_human = humanGO.GetComponent<HumanView>();
				if (_human == null)
				{
					_isCompleted = true;
				}
				else
                {
					var (ownedChairGO, ownedChairData) = ApplicationController.Instance.LevelView.GetItemByOwner(_human.NameHuman);
					if (ownedChairGO == null)
                    {
						if (!_human.gameObject.activeSelf)
						{
							var (exitGO, exitData) = ApplicationController.Instance.LevelView.GetAnyChairByByTypeArea(AreaMode.Exit);
							if (exitGO != null)
                            {
								_human.gameObject.SetActive(true);
								_human.gameObject.transform.position = exitGO.transform.position;
							}
						}
						else
						{
							_isCompleted = true;
						}
					}
				}
			}
			
			if (!_isCompleted)
            {
				var (chairGO, chairData) = ApplicationController.Instance.LevelView.GetFreeChairByRoomRoomArea(_areaNameRoom);
				if (chairGO != null)
				{
					_targetChairGO = chairGO;
					_human.ItemData.IsAvailable = false;
					_human.SetChair(_targetChairGO);
				}
				else
				{
					_isCompleted = true;
				}
			}

			SystemEventController.Instance.Event += OnSystemEvent;
			SystemEventController.Instance.DispatchSystemEvent(LevelView.EventLevelViewLinesRequestDestroy);
		}

        public void Destroy()
		{
			if (_human != null)
            {
				_human.StopMovement();
				_human.ItemData.IsAvailable = true;
				if (_isRunning)
                {
					_human.DestinationReachedEvent -= OnDestinationReached;
				}				
			}
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			_human = null;
			_targetChairGO = null;			
		}

		public bool IsBlocking()
		{
			return false;
		}

		private void OnDestinationReached(GameObject human)
		{
			if (_human != null)
            {
				if (_human.gameObject == human)
				{
					_human.ItemData.IsAvailable = true;
					SystemEventController.Instance.DelaySystemEvent(EventCommandGoToAreaChairDestinationReached, 0.2f, _human.NameHuman);
					_isCompleted = true;
				}
			}
		}

		public bool IsCompleted()
		{
			return _isCompleted;
		}

		public override void RunAction()
		{
			if (!_isRunning)
            {
				_isRunning = true;
				_human.DestinationReachedEvent += OnDestinationReached;
				_human.GoToChair(_targetChairGO);
			}
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(TimeHUD.EventTimeHUDShortcutAction))
			{
				if ((_human != null) && (_targetChairGO != null))
                {
					_human.Teleport(_targetChairGO.transform.position);
                }
			}
		}

		public override void Run()
		{
			base.Run();
		}
    }
}