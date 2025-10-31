using UnityEngine;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.EditionSubStateAreas;

namespace yourvrexperience.WorkDay
{
	public class CommandGoToOwnChair : CommandGoToBase, IGameCommand
	{
		public const string EventCommandGoToOwnChairStarted = "EventCommandGoToOwnChairStarted";
		public const string EventCommandGoToOwnChairCompleted = "EventCommandGoToOwnChairCompleted";

		private HumanView _human = null;
		private GameObject _targetChairGO = null;
		private bool _isRunning = false;

		public string Name
		{
			get { return "GoToOwnChair"; }
		}

		public MeetingData Meeting
		{
			get { return null; }
		}

		public override void Initialize(params object[] parameters)
		{
			base.Initialize();

			_member = (string)parameters[0];
			_targetChairGO = null;
			_timeToStart = 0;
			_timeAcum = 0;
			_timeSecond = 0;
			if (parameters.Length > 1)
            {
				_timeToStart = (float)parameters[1];
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
					if (_human.CurrentChair != null)
					{
						var (itemGO, itemData) = ApplicationController.Instance.LevelView.GetItemByOwner(_human.NameHuman);
						if (itemGO != null)
						{
							if (itemGO.GetComponent<ChairView>() == null)
                            {
								_isCompleted = false;
							}
							else
                            {
								if (itemGO.GetComponent<ChairView>() == _human.CurrentChair)
								{
									_isCompleted = true;
								}
							}
						}
					}
				}
			}

			if (!_isCompleted)
            {
				_human.ItemData.IsAvailable = false;
				var (itemGO, itemData) = ApplicationController.Instance.LevelView.GetItemByOwner(_human.NameHuman);
				if (itemGO != null)
				{
					_targetChairGO = itemGO;
				}
				else
				{
					var (exitGO, exitData) = ApplicationController.Instance.LevelView.GetAnyChairByByTypeArea(AreaMode.Exit);
					if (exitGO != null)
                    {
						_targetChairGO = exitGO;
					}
					else
                    {
						_isCompleted = true;
                    }					
				}
			}
			else
            {
				SystemEventController.Instance.DelaySystemEvent(CommandGoToAreaChair.EventCommandGoToAreaChairDestinationReached, 1, _human.NameHuman);
				SystemEventController.Instance.DelaySystemEvent(EventCommandGoToOwnChairCompleted, 1, _human.NameHuman);
			}
			SystemEventController.Instance.DispatchSystemEvent(LevelView.EventLevelViewLinesRequestDestroy);

			SystemEventController.Instance.Event += OnSystemEvent;
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
			if (_human.gameObject == human)
            {
				_isCompleted = true;
				_human.ItemData.IsAvailable = true;
				SystemEventController.Instance.DelaySystemEvent(CommandGoToAreaChair.EventCommandGoToAreaChairDestinationReached, 1, _human.NameHuman);
				SystemEventController.Instance.DelaySystemEvent(EventCommandGoToOwnChairCompleted, 1, _human.NameHuman);
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
				SystemEventController.Instance.DispatchSystemEvent(EventCommandGoToOwnChairStarted, _human.NameHuman);
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