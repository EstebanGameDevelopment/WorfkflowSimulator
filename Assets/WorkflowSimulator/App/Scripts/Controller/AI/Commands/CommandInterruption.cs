using UnityEngine;
using yourvrexperience.Utils;
using System;
using System.Collections.Generic;

namespace yourvrexperience.WorkDay
{
	public class CommandInterruption : CommandGoToBase, IGameCommand
	{
		public const string EventCommandInterruptionCompleted = "EventCommandInterruptionCompleted";

		public const int TimeToInterrupt = 5;

		private bool _isRunning;
		private HumanView _humanView;
		private WorldItemData _memberData;
		private HumanView _targetToBother;
		private string _nameMeetingInterruption = null;

		private float _timeoutToCancel = 0;

		public string Name
		{
			get { return "Interruption"; }
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

			var (humanGO, humanData) = ApplicationController.Instance.LevelView.GetItemByName(_member);

			MeetingData runningMeeting = null;
			if ((humanGO == null) || (humanData == null))
			{
				_isCompleted = true;
			}
			else
            {
				_humanView = humanGO.GetComponent<HumanView>();
				_memberData = humanData;

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
			if (_targetToBother != null)
			{
				if (_targetToBother.ItemData != null)
                {
					_targetToBother.ItemData.IsAvailable = true;
				}				
			}
			if (_isRunning)
            {
				if (_humanView != null)
                {
					_humanView.DestinationReachedEvent -= OnDestinationReached;
				}				
			}

			_humanView = null;
			_memberData = null;
			_targetToBother = null;
		}

        public bool IsCompleted()
		{
			return _isCompleted;
		}

		private bool TriggerInterruption()
        {
			_isRunning = true;

			string playerName = "";
			if (ApplicationController.Instance.HumanPlayer != null)
            {
				playerName = ApplicationController.Instance.HumanPlayer.NameHuman;
			}

			List<GameObject> targetsToBother = new List<GameObject>();
			List<WorldItemData> humans = ApplicationController.Instance.LevelView.GetHumans();
			foreach (WorldItemData humanData in humans)
            {
				if (humanData.IsAvailable)
                {
					if (!_memberData.Name.Equals(humanData.Name))
                    {
						if (MeetingController.Instance.GetMeetingOfHuman(humanData.Name) == null)
                        {
							if (!CommandsController.Instance.IsActorsInWay(humanData.Name))
                            {
								if ((playerName.Length == 0) || ((playerName.Length > 0) && (!humanData.Name.Equals(playerName))))
								{
									var (chairGO, chairData) = ApplicationController.Instance.LevelView.GetItemByOwner(humanData.Name);
									if (chairGO != null)
									{
										var (humanGO, humanDT) = ApplicationController.Instance.LevelView.GetItemByName(humanData.Name);
										if (humanGO != null)
										{
											targetsToBother.Add(humanGO);
										}
									}
								}
							}
						}
					}
				}
			}

			if (targetsToBother.Count == 0)
            {
				_isCompleted = true;
				_isRunning = false;
			}
			else
            {
				int indexHuman = UnityEngine.Random.Range(0, targetsToBother.Count);
				_targetToBother = targetsToBother[indexHuman].GetComponent<HumanView>();

				if (_targetToBother == null)
                {
					_isCompleted = true;
					_isRunning = false;
				}
				else
                {
					SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);

					_humanView.GoToHuman(_targetToBother.gameObject);
					_humanView.DestinationReachedEvent += OnDestinationReached;
					_memberData.IsAvailable = false;
				}
			}

			return true;
		}

        private void OnDestinationReached(GameObject human)
        {
			List<string> assistantsToInterruption = new List<string>();
			if (ApplicationController.Instance.HumanPlayer != null)
            {
				if (MeetingController.Instance.GetMeetingOfHuman(ApplicationController.Instance.HumanPlayer.NameHuman) == null)
				{
					assistantsToInterruption.Add(ApplicationController.Instance.HumanPlayer.NameHuman);
				}
			}
			assistantsToInterruption.Add(_member);
			if ((_targetToBother == null) || (_humanView == null))
            {
				_isCompleted = true;
				GoBackToYourOwnChair();
			}
			else
            {
				assistantsToInterruption.Add(_targetToBother.NameHuman);

				_humanView.DestinationReachedEvent -= OnDestinationReached;

				DateTime dateTimeStart = WorkDayData.Instance.CurrentProject.GetCurrentTime();
				DateTime dateTimeEnd = dateTimeStart.AddSeconds(TimeToInterrupt * 60);
				_nameMeetingInterruption = LanguageController.Instance.GetText("word.interruption") + " " + dateTimeStart.ToShortTimeString();
				SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerAddMeeting, null, -1, _nameMeetingInterruption, LanguageController.Instance.GetText("text.description.interruption"), new List<DocumentData>(), dateTimeStart, dateTimeEnd, -1, assistantsToInterruption.ToArray(), false, false, false, true, false);
			}
		}

		public override void RunAction()
		{
			if (_isRunning) return;

			TriggerInterruption();
		}

		public override void Run()
		{
			base.Run();

			if (_isRunning)
            {
				if (_nameMeetingInterruption == null)
                {
					_timeoutToCancel += Time.deltaTime;
					if (_timeoutToCancel > 10)
					{
						_isCompleted = true;
					}
				}
			}			
		}

		private void GoBackToYourOwnChair()
        {
			CommandGoToOwnChair cmdGoToYourChair = new CommandGoToOwnChair();
			cmdGoToYourChair.Initialize(_member, 1f);
			cmdGoToYourChair.Prioritary = false;
			SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdGoToYourChair);
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(MeetingController.EventMeetingControllerMeetingStopped))
            {
				MeetingData meeting = (MeetingData)parameters[0];
				if (meeting != null)
                {
					if (_nameMeetingInterruption != null)
                    {
						if (meeting.Name.Equals(_nameMeetingInterruption))
						{
							GoBackToYourOwnChair();
						}
					}
				}
			}
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
					SystemEventController.Instance.DispatchSystemEvent(EventCommandInterruptionCompleted, _member);
				}
			}
		}
	}
}