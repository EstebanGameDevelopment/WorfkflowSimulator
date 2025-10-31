using UnityEngine;
using yourvrexperience.Utils;
using System.Collections.Generic;
using static yourvrexperience.WorkDay.EditionSubStateAreas;

namespace yourvrexperience.WorkDay
{
	public class CommandGoToBreak : CommandGoToBase, IGameCommand
	{
		public const string EventCommandGoToBreakCompleted = "EventCommandGoToBreakCompleted";

		public const int TimeToBreak = 10;

		private bool _isRunning;
		private WorldItemData _memberData;

		public MeetingData Meeting
		{
			get { return null; }
		}

		public string Name
		{
			get { return "GoToBreak"; }
		}

		public override void Initialize(params object[] parameters)
		{
			base.Initialize();
			_prioritary = false;

			_member = (string)parameters[0];
			_timeAcum = 0;
			_timeToStart = 0;
			_isRunning = false;

			_memberData = WorkDayData.Instance.CurrentProject.GetItemByName(_member);

			MeetingData runningMeeting = null;
			if (_memberData == null)
            {
				_isCompleted = true;
			}
			else
            {
				List<MeetingData> orderedMeetings = WorkDayData.Instance.CurrentProject.GetMeetingsForHuman(WorkDayData.Instance.CurrentProject.GetCurrentTime(), _member);
				if (orderedMeetings.Count > 0)
				{
					runningMeeting = orderedMeetings[0];
					if (runningMeeting != null)
					{
						if (!runningMeeting.InProgress)
						{
							runningMeeting = null;
						}
					}
				}
			}

			SystemEventController.Instance.Event += OnSystemEvent;

			if (runningMeeting != null)
			{
				_isCompleted = true;
			}

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
		}

        public bool IsCompleted()
		{
			return _isCompleted;
		}

		private bool TriggerBreak()
        {
			AreaData areaKitchen = WorkDayData.Instance.CurrentProject.GetFreeChairInRoom(AreaMode.Kitchen);
			if (areaKitchen != null)
			{
				_isRunning = true;

				CommandGoToAreaChair cmdGoToChairKitchen = new CommandGoToAreaChair();
				cmdGoToChairKitchen.Initialize(_member, areaKitchen.Name);
				cmdGoToChairKitchen.Prioritary = false;
				SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdGoToChairKitchen);

				_memberData.IsAvailable = false;
				_memberData.HasRested = true;

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

			if (!TriggerBreak())
            {
				_timeAcum = 0;
				_timeToStart = 10;
				_hasStartedAction = false;
			}
		}

		public override void Run()
		{
			base.Run();
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
					SystemEventController.Instance.DispatchSystemEvent(EventCommandGoToBreakCompleted, _member);
				}
			}
		}
	}
}