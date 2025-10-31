using UnityEngine;
using yourvrexperience.Utils;
using System;
using System.Collections.Generic;

namespace yourvrexperience.WorkDay
{
	public class CommandTalkInMeeting : IGameCommand
	{
		public const string EventCommandTalkInMeetingResponse_DEFINITION = "EventCommandTalkInMeetingResponse";
		public const string EventCommandTalkInMeetingDocsSumarized_DEFINITION = "EventCommandTalkInMeetingDocsSumarized";

		public string EventCommandTalkInMeetingResponse = "";
		public string EventCommandTalkInMeetingDocsSumarized = "";

		private bool _isCompleted = false;
		private HumanView _human;
		private MeetingData _meeting;
		private List<InteractionData> _interactions;
		private bool _isRunning = false;
		private bool _shouldFinish = false;
		private bool _isAI = false;
		private string _message = "";
		private string _data = "";
		private bool _isHumanTalking;
		private string _member;
		private bool _prioritary;

		private string _chatText;
		private string _dataInteraction;
		private string _summary = "";
		private DocumentData _docData;

		public string Name
		{
			get { return "TalkInMeeting"; }
		}

		public MeetingData Meeting
		{
			get { return _meeting; }
		}
        public bool Prioritary
        {
			get { return _prioritary; }
        }
		public string Member
		{
			get { return _member; }
		}
		public bool RequestDestruction
		{
			get { return false; }
			set { }
		}

		public void Initialize(params object[] parameters)
		{
			if (parameters.Length == 1)
            {
				_isAI = true;
				_member = null;
				_meeting = (MeetingData)parameters[0];
				_prioritary = !_meeting.IsSocialMeeting();
				_message = "";
			}
			else
            {
				_isAI = false;
				_member = (string)parameters[0];
				_meeting = (MeetingData)parameters[1];
				_prioritary = !_meeting.IsSocialMeeting();
				if (parameters.Length > 2)
				{
					_message = (string)parameters[2];
				}
				if (parameters.Length > 3)
				{
					_data = (string)parameters[3];
				}
				if (parameters.Length > 4)
				{
					_summary = (string)parameters[4];
				}
			}
			EventCommandTalkInMeetingResponse = EventCommandTalkInMeetingResponse_DEFINITION + _meeting.GetUID();
			EventCommandTalkInMeetingDocsSumarized = EventCommandTalkInMeetingDocsSumarized_DEFINITION + _meeting.GetUID();

			_interactions = _meeting.GetInteractions();

			_isHumanTalking = false;
			if (_member != null)
            {
				if (_meeting.HasPlayer(true))
				{
					_isHumanTalking = _member.Equals(ApplicationController.Instance.HumanPlayer.NameHuman);
				}
			}

			if (!_isAI)
            {
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
			}

			SystemEventController.Instance.Event += OnSystemEvent;

			if (!_isCompleted)
            {
				_isRunning = true;
				if (!_isAI)
                {
					SystemEventController.Instance.DelaySystemEvent(EventCommandTalkInMeetingResponse, 0.1f, _meeting, false, _member, _message, _data, _summary);
				}
				else
                {
					bool enableConfirmationPrompt = false;
					if (ApplicationController.Instance.HumanPlayer != null)
					{
						if (_meeting.IsAssistingMember(ApplicationController.Instance.HumanPlayer.NameHuman))
						{
							enableConfirmationPrompt = true;
						}
					}

					AICommandsController.Instance.AddNewAICommand(new AICommandReplyText(), enableConfirmationPrompt, _meeting, EventCommandTalkInMeetingResponse);
				}
			}
			else
            {
				throw new Exception("CommandTalkHuman::THE MEETING HAS BEEN COMPLETED");
            }
		}

        public void Destroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;

			_human = null;
			_meeting = null;
			_docData = null;
			_interactions = null;
		}

		public bool IsBlocking()
		{
			return false;
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventCommandTalkInMeetingResponse))
			{
				if (_isRunning)
                {
					if (_meeting == (MeetingData)parameters[0])
					{
						if (parameters.Length <= 1)
                        {
							_chatText = "";
							CompleteProcess();
							return;
                        }

						_isRunning = false;
						_shouldFinish = (bool)parameters[1];
						_member = (string)parameters[2];
						_chatText = (string)parameters[3];
						_dataInteraction = (string)parameters[4];
						_summary = (string)parameters[5];

						if (_shouldFinish)
                        {
							_meeting.Iterations = 1000000000;
						}

						if (_isAI)
                        {
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
						}

						if ((_human != null) && (_dataInteraction != null) && (_dataInteraction.Length > 0))
                        {
							int idImage = ScreenMultiInputDataView.GetImageFromText(_dataInteraction);
							if (idImage != -1)
                            {
								_docData = new DocumentData(-1, WorkDayData.Instance.CurrentProject.ProjectInfoSelected, "", "", "", new HTMLData() { html = idImage.ToString() }, false, true, "", true, -1, _meeting.TaskId);
							}
							else
                            {
								HTMLData htmlData = new HTMLData();
								htmlData.SetHTML(_dataInteraction);
								_docData = new DocumentData(-1, WorkDayData.Instance.CurrentProject.ProjectInfoSelected, "", "", "", htmlData, false, false, "", true, -1, _meeting.TaskId);
							}
							List<DocumentData> tmpListDocuments = new List<DocumentData>();
							tmpListDocuments.Add(_docData);
							AICommandsController.Instance.AddNewAICommand(new AICommandSummarizeDocs(), !_isAI, tmpListDocuments, EventCommandTalkInMeetingDocsSumarized);
						}
						else
                        {
							CompleteProcess();
						}
					}
				}
			}
			if (nameEvent.Equals(EventCommandTalkInMeetingDocsSumarized))
			{
				if ((bool)parameters[0])
				{
					if (_docData != null)
					{
						_summary = _docData.Summary;
						Debug.Log("SUMMARY FOR AI RECEIVED=" + _summary);
					}
				}
				_docData = null;
				CompleteProcess();
			}
		}

		private void CompleteProcess()
        {
			if (_chatText.Length > 0)
            {
				InteractionData aiInteraction = new InteractionData(!_isHumanTalking, _member, _chatText, _dataInteraction, _summary, WorkDayData.Instance.CurrentProject.GetCurrentTime());
				_interactions.Add(aiInteraction);
				_meeting.SetInteractions(_interactions);
				SystemEventController.Instance.DispatchSystemEvent(ScreenDialogView.EventScreenDialogViewAddAIInteraction, _meeting, aiInteraction);
			}
			else
            {
				SystemEventController.Instance.DispatchSystemEvent(ScreenDialogView.EventScreenDialogViewAddAIInteraction, _meeting);
			}
			_isCompleted = true;
		}

		public bool IsCompleted()
		{
			return _isCompleted;
		}

		public void RunAction()
		{
		}

		public void Run()
		{
		}
    }
}