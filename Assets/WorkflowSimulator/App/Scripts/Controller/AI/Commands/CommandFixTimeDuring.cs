using yourvrexperience.Utils;
using System;

namespace yourvrexperience.WorkDay
{
	public class CommandFixTimeDuring : CommandGoToBase, IGameCommand
	{
		private string _eventCompleted;

		public MeetingData Meeting
		{
			get { return null; }
		}

		public string Name
		{
			get { return "FixTimeDuring"; }
		}

		public override void Initialize(params object[] parameters)
		{
			base.Initialize();

			int timeSpeed = (int)parameters[0];
			_timeToStart = (int)parameters[1];
			_isCompleted = false;
			_eventCompleted = "";
			if (parameters.Length > 2)
            {
				_eventCompleted = (string)parameters[2];
			}

			ApplicationController.Instance.TimeHUD.IncrementTime = new TimeSpan(0, 0, timeSpeed);

			SystemEventController.Instance.DispatchSystemEvent(LevelView.EventLevelViewLinesRequestDestroy);
		}

        public void Destroy()
		{
		}

		public bool IsBlocking()
		{
			return false;
		}

		public bool IsCompleted()
		{
			return _isCompleted;
		}

		public override void RunAction()
		{
			SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
			if (_eventCompleted.Length > 0)
            {
				SystemEventController.Instance.DispatchSystemEvent(_eventCompleted);
            }
			_isCompleted = true;
		}

		public override void Run()
		{
			base.Run();
		}
    }
}