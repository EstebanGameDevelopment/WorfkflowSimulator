using UnityEngine;
using yourvrexperience.Utils;
using System.Collections.Generic;
using static yourvrexperience.WorkDay.EditionSubStateAreas;

namespace yourvrexperience.WorkDay
{
	public class CommandLunchTime : CommandGoToBase, IGameCommand
	{
		public const string EventCommandLunchTimeCompleted = "EventCommandLunchTimeCompleted";

		public const int TimeToLunch = 30;

		private bool _isRunning;
		private WorldItemData _memberData;
		private MeetingData _runningMeeting;
		private float _deadlineTimeLunch;
		private float _timeLocalSecond;

		public string Name
		{
			get { return "LunchTime"; }
		}

		public MeetingData Meeting
		{
			get { return null; }
		}

		public override void Initialize(params object[] parameters)
		{
			_member = (string)parameters[0];
			int rangeMinutes = (int)parameters[1];
			int totalLunchTime = (int)parameters[2];
			_deadlineTimeLunch = (totalLunchTime - TimeToLunch) * 60;
			_timeToStart = UnityEngine.Random.Range(1, rangeMinutes) * 60;
			_timeAcum = 0;
			_timeLocalSecond = 0;
			_isRunning = false;
			_prioritary = true;

			_memberData = WorkDayData.Instance.CurrentProject.GetItemByName(_member);

			if (_memberData == null)
            {
				_isCompleted = true;
			}
			else
            {
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
			}

			SystemEventController.Instance.Event += OnSystemEvent;

			if (!_isCompleted && (_memberData != null))
            {
				_memberData.IsAvailable = false;
			}
		}

		public bool IsBlocking()
		{
			return false;
		}

		public void Destroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;

			if (_memberData != null)
            {
				_memberData.IsAvailable = true;
			}
			_memberData = null;
			_runningMeeting = null;

			SystemEventController.Instance.DispatchSystemEvent(EventCommandLunchTimeCompleted, _member);
		}

        public bool IsCompleted()
		{
			return _isCompleted;
		}

		private bool TriggerLunch()
        {
			AreaData areaKitchen = WorkDayData.Instance.CurrentProject.GetFreeChairInRoom(AreaMode.Kitchen);
			if (areaKitchen != null)
			{
				_isRunning = true;

				CommandGoToAreaChair cmdGoToChairMeetingRoom = new CommandGoToAreaChair();
				cmdGoToChairMeetingRoom.Initialize(_member, areaKitchen.Name);
				SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdGoToChairMeetingRoom);

				CommandGoToOwnChair cmdGoToYourChair = new CommandGoToOwnChair();
				cmdGoToYourChair.Initialize(_member, (float)(TimeToLunch * 60));
				SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdGoToYourChair);

				_memberData.IsAvailable = false;

				return true;
			}
			else
            {
				return false;
            }
		}

		public override void RunAction()
		{
			if (_isRunning) return;

			bool shouldRepeat = true;
			if (_runningMeeting == null)
			{
				shouldRepeat = !TriggerLunch();
			}
			else
			{
				if (_runningMeeting.Completed)
				{
					shouldRepeat = !TriggerLunch();
				}
				else
				{
					shouldRepeat = true;
				}
			}

			if (shouldRepeat)
            {
				_timeAcum = 0;
				_timeToStart = 10;
				_hasStartedAction = false;
				SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerPoliteRequestToEndRunningMeetings, _member, false, _runningMeeting);
			}
		}

		public override void Run()
		{
			base.Run();

			if (!_isRunning && !_isCompleted)
            {
				_timeLocalSecond += Time.deltaTime;
				if (_timeLocalSecond >= 1)
				{
					_timeLocalSecond -= 1;
					_deadlineTimeLunch -= (float)ApplicationController.Instance.TimeHUD.IncrementTime.TotalSeconds;
					if (_deadlineTimeLunch < 0)
					{
						_isRunning = true;
						_memberData.IsAvailable = false;

						if (_runningMeeting != null)
						{
							MeetingData runningMeeting = _runningMeeting;
							_runningMeeting = null;
							SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerLeaveMeeting, runningMeeting, _member);
						}

						CommandGoToOwnChair cmdGoToYourChair = new CommandGoToOwnChair();
						cmdGoToYourChair.Initialize(_member, (float)(TimeToLunch * 60));
						SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdGoToYourChair);
					}
				}
			}			
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(CommandGoToOwnChair.EventCommandGoToOwnChairStarted))
            {
				string nameMember = (string)parameters[0];
				if (_member.Equals(nameMember))
                {
					_memberData.IsAvailable = true;
				}
			}
			if (nameEvent.Equals(CommandGoToOwnChair.EventCommandGoToOwnChairCompleted))
			{
				string nameMember = (string)parameters[0];
				if (_member.Equals(nameMember))
				{
					_memberData.IsAvailable = true;
					_isCompleted = true;
					_isRunning = false;
					SystemEventController.Instance.DispatchSystemEvent(EventCommandLunchTimeCompleted, _member);
				}
			}
		}
	}
}