using yourvrexperience.Utils;
using System.Collections.Generic;
using static yourvrexperience.WorkDay.EditionSubStateAreas;

namespace yourvrexperience.WorkDay
{
	public class CommandGoToBathroom : CommandGoToBase, IGameCommand
	{
		public const string EventCommandGoToBathroomCompleted = "EventCommandGoToBathroomCompleted";

		public const int TimeToBathroom = 10;

		private bool _isRunning;
		private WorldItemData _memberData;

		public string Name
		{
			get { return "GoToBathroom"; }
		}


		public MeetingData Meeting
		{
			get { return null; }
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
				SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerLeaveMeeting, runningMeeting, _member);
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

		private bool TriggerBathroom()
        {
			AreaData areaBathroom = WorkDayData.Instance.CurrentProject.GetFreeChairInRoom(AreaMode.Bathroom);
			if (areaBathroom != null)
			{
				_isRunning = true;			

				CommandGoToAreaChair cmdGoToChairMeetingRoom = new CommandGoToAreaChair();
				cmdGoToChairMeetingRoom.Initialize(_member, areaBathroom.Name);
				cmdGoToChairMeetingRoom.Prioritary = false;
				SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdGoToChairMeetingRoom);

				CommandGoToOwnChair cmdGoToYourChair = new CommandGoToOwnChair();
				cmdGoToYourChair.Initialize(_member, (float)(TimeToBathroom * 60));
				cmdGoToYourChair.Prioritary = false;
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

			if (!TriggerBathroom())
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
					SystemEventController.Instance.DispatchSystemEvent(EventCommandGoToBathroomCompleted, _member);
				}
			}
		}
	}
}