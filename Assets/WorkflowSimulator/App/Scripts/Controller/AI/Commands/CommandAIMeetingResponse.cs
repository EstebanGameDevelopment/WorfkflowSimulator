using yourvrexperience.Utils;
using System;
using System.Collections.Generic;

namespace yourvrexperience.WorkDay
{
	public class CommandAIMeetingResponse : CommandGoToBase, IGameCommand
	{
		public const string EventCommandAIMeetingResponse_DEFINITION = "EventCommandAIMeetingResponse";
		public const string EventCommandAIMeetingCompletedAIProcessing_DEFINITION = "EventCommandAIMeetingCompletedAIProcessing";

		public string EventCommandAIMeetingResponse = "";
		public string EventCommandAIMeetingCompletedAIProcessing = "";

		private MeetingData _meeting;
		private bool _requestedToConclude = false;

		public MeetingData Meeting
        {
			get { return _meeting; }
        }

        public string Name
        {
			get { return "AIMeetingResponse"; }
        }

        public override void Initialize(params object[] parameters)
		{
			base.Initialize();

			_meeting = (MeetingData)parameters[0];
			_prioritary = !_meeting.IsSocialMeeting();
			_requestedToConclude = false;
			if (parameters.Length > 1)
            {
				_requestedToConclude = (bool)parameters[1];
			}

			EventCommandAIMeetingResponse = EventCommandAIMeetingResponse_DEFINITION + _meeting.GetUID();
			EventCommandAIMeetingCompletedAIProcessing = EventCommandAIMeetingCompletedAIProcessing_DEFINITION + _meeting.GetUID();

			SystemEventController.Instance.Event += OnSystemEvent;

			if (!_isCompleted)
            {
				_meeting.IsProcessingAI = true;
				_timeAcum = 0;
				_timeToStart = _meeting.DelayIterations;
				if (_meeting.Interactions == null)
                {
					if (ApplicationController.Instance.HumanPlayer == null)
                    {
						_timeToStart = 1;
					}
					else
                    {
						if (_meeting.IsInterruptionMeeting())
						{
							if (_meeting.InitiatedByPlayer)
							{
								InteractionData aiInteraction = new InteractionData(true, ApplicationController.Instance.HumanPlayer.NameHuman, LanguageController.Instance.GetText("text.hello.friend"), "", "", WorkDayData.Instance.CurrentProject.GetCurrentTime());
								List<InteractionData> interactionsInterruptionByPlayer = _meeting.GetInteractions();
								interactionsInterruptionByPlayer.Add(aiInteraction);
								_meeting.SetInteractions(interactionsInterruptionByPlayer);
								SystemEventController.Instance.DelaySystemEvent(ScreenDialogView.EventScreenDialogViewForceEnableNextButtons, 0.2f);
							}
							else
							{
								_timeToStart = 1;
							}
						}
						else
						{
							_timeToStart = 1;
						}
					}
				}
				else
                {
					if (_meeting.Interactions.Length == 0)
					{
						_timeToStart = 1;
					}
				}
				if (_meeting.HasPlayer(true) && !_meeting.IsInterruptionMeeting())
				{
					_timeToStart = 1;
				}
			}
			else
            {
				throw new Exception("CommandTalkNext::THE MEETING IS COMPLETED");
			}
		}

        public void Destroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			_meeting = null;
		}

		public bool IsBlocking()
		{
			return true;
		}

		public bool IsCompleted()
		{
			return _isCompleted;
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ScreenDialogView.EventScreenDialogViewForceNextReplyInterruptor))
            {
				if (_meeting.HasPlayer(true))
				{
					if (_meeting.IsInterruptionMeeting())
					{
						_timeAcum = _timeToStart;
					}
				}
			}
			if (nameEvent.Equals(ScreenDialogView.EventScreenDialogViewAddAIInteraction))
            {
				if (_meeting == (MeetingData)parameters[0])
                {
					_isCompleted = true;
					bool runAILogic = true;
					if (_meeting.HasPlayer(true))
					{
						if (!_meeting.IsInterruptionMeeting())
						{
							runAILogic = false;
						}
					}
					if (_requestedToConclude)
					{
						if (_meeting.TotalIterations - _meeting.Iterations > 3)
						{
							_meeting.Iterations = _meeting.TotalIterations - 3;
							_meeting.DelayIterations = 1f;
						}
					}
					_meeting.Iterations++;
					if (runAILogic)
					{
						if (_meeting.Iterations > _meeting.TotalIterations + 10)
						{
							SystemEventController.Instance.DelaySystemEvent(MeetingController.EventMeetingControllerStopMeeting, 0.1f, _meeting);
						}
						else
						{
							SystemEventController.Instance.DelaySystemEvent(MeetingController.EventMeetingControllerRunAction, 0.1f, _meeting);
						}
					}
					SystemEventController.Instance.DispatchSystemEvent(EventCommandAIMeetingCompletedAIProcessing, _meeting);
				}
			}
		}

		public override void RunAction()
		{
			CommandTalkInMeeting cmdTalk = new CommandTalkInMeeting();
			cmdTalk.Initialize(_meeting);
			SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdTalk);
		}

		public override void Run()
		{
			base.Run();
		}
    }
}