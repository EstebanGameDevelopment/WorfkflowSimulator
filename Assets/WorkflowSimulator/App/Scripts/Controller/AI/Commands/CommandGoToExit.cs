using UnityEngine;
using yourvrexperience.Utils;
using System.Collections.Generic;
using static yourvrexperience.WorkDay.EditionSubStateAreas;

namespace yourvrexperience.WorkDay
{
	public class CommandGoToExit : CommandGoToBase, IGameCommand
	{
		private HumanView _human = null;
		private bool _isRunning = false;
		private MeetingData _runningMeeting;
		private string _eventCompleted;

		public string Name
		{
			get { return "GoToExit"; }
		}

		public MeetingData Meeting
		{
			get { return null; }
		}

		public override void Initialize(params object[] parameters)
		{
			_member = (string)parameters[0];
			int rangeMinutes = (int)parameters[1];
			_timeToStart = UnityEngine.Random.Range(1, rangeMinutes) * 60;
			_eventCompleted = "";
			if (parameters.Length > 2)
            {
				_eventCompleted = (string)parameters[2];
			}

			List<MeetingData> orderedMeetings = WorkDayData.Instance.CurrentProject.GetMeetingsForHuman(WorkDayData.Instance.CurrentProject.GetCurrentTime(), _member);
			if (orderedMeetings.Count > 0)
            {
				_runningMeeting = orderedMeetings[0];
				if (_runningMeeting != null)
                {
					if (!_runningMeeting.InProgress)
					{
						_runningMeeting = null;
					}
				}
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
			}

			SystemEventController.Instance.Event += OnSystemEvent;

			SystemEventController.Instance.DispatchSystemEvent(LevelView.EventLevelViewLinesRequestDestroy);

			if ((_human != null) && !_isCompleted)
            {
				_human.ItemData.IsAvailable = false;
			}
		}

		public bool IsBlocking()
		{
			return false;
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
			_runningMeeting = null;
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ChairView.EventChairViewReportInAreaData))
			{
				if (parameters[0] is CommandGoToExit)
				{
					if (this == (CommandGoToExit)parameters[0])
					{
						if (!_isRunning)
						{
							_isRunning = true;
							ChairView chair = (ChairView)parameters[1];
							_human.DestinationReachedEvent += OnDestinationReached;
							_human.GoToChair(chair.gameObject);
						}
					}
				}
			}
		}

		private void OnDestinationReached(GameObject human)
		{
			if (_human.gameObject == human)
            {
				_isCompleted = true;
				_human.ItemData.IsAvailable = true;
				_human.gameObject.SetActive(false);
				SystemEventController.Instance.DispatchSystemEvent(_eventCompleted);
			}
		}

		public bool IsCompleted()
		{
			return _isCompleted;
		}

		public override void RunAction()
		{
			if (_runningMeeting == null)
            {
				SystemEventController.Instance.DispatchSystemEvent(ChairView.EventChairViewRequestInAreaData, this, AreaMode.Exit);
			}
			else
            {
				if (_runningMeeting.Completed)
                {
					SystemEventController.Instance.DispatchSystemEvent(ChairView.EventChairViewRequestInAreaData, this, AreaMode.Exit);
				}
				else
                {
					_timeAcum = 0;
					_timeToStart = 10;
					_hasStartedAction = false;
				}
			}
		}

		public override void Run()
		{
			base.Run();
		}
    }
}