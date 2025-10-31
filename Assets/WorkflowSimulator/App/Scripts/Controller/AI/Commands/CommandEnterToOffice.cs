using UnityEngine;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.EditionSubStateAreas;

namespace yourvrexperience.WorkDay
{
	public class CommandEnterToOffice : CommandGoToBase, IGameCommand
	{
		public const string EventCommandEnterToOfficeReachedChair = "EventCommandEnterToOfficeReachedChair";

		private HumanView _human = null;
		private bool _isRunning = false;

		public MeetingData Meeting
		{
			get { return null; }
		}

		public string Name
		{
			get { return "EnterToOffice"; }
		}

		public override void Initialize(params object[] parameters)
		{
			base.Initialize();
			_member = (string)parameters[0];
			int rangeMinutes = (int)parameters[1];
			_timeToStart = UnityEngine.Random.Range(1, rangeMinutes) * 60;

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
					var (chairGO, chairData) = ApplicationController.Instance.LevelView.GetItemByOwner(_member);
					if (chairGO == null)
                    {
						_isCompleted = true;
					}
				}
			}

			SystemEventController.Instance.Event += OnSystemEvent;

			if (!_isCompleted)
            {
				_human.ItemData.IsAvailable = false;
				_human.gameObject.SetActive(false);
			}

			SystemEventController.Instance.DispatchSystemEvent(LevelView.EventLevelViewLinesRequestDestroy);
		}

        public void Destroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;

			if (_human != null)
            {
				_human.StopMovement();
				_human.ItemData.IsAvailable = true;
				if (_isRunning)
                {
					_human.DestinationReachedEvent -= OnDestinationReached;
				}				
			}
			_human = null;
		}

		public bool IsBlocking()
        {
			return false;
        }

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ChairView.EventChairViewReportInAreaData))
			{
				if (parameters[0] is CommandEnterToOffice)
                {
					if (this == (CommandEnterToOffice)parameters[0])
					{
						if (!_isRunning)
						{
							_isRunning = true;
							ChairView chairExit = (ChairView)parameters[1];
							_human.gameObject.SetActive(true);
							_human.SetInitialChair(chairExit.gameObject);

							var (chairGO, chairData) = ApplicationController.Instance.LevelView.GetItemByOwner(_human.NameHuman);
							_human.DestinationReachedEvent += OnDestinationReached;
							_human.GoToChair(chairGO);
						}
					}
				}
			}
		}

		private void OnDestinationReached(GameObject human)
		{
			if (_human.gameObject == human)
            {
				_human.ItemData.IsAvailable = true;
				SystemEventController.Instance.DelaySystemEvent(EventCommandEnterToOfficeReachedChair, 0.2f, _human.NameHuman);
				_isCompleted = true;
			}
		}

		public bool IsCompleted()
		{
			return _isCompleted;
		}

		public override void RunAction()
		{
			SystemEventController.Instance.DispatchSystemEvent(ChairView.EventChairViewRequestInAreaData, this, AreaMode.Exit);
		}

		public override void Run()
		{
			base.Run();
		}
    }
}