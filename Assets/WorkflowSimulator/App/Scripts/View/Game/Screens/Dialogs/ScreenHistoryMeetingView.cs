using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class ScreenHistoryMeetingView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenHistoryMeetingView";
		public const string ScreenHUDName = "ScreenHUDHistoryMeetingView";

		public const string EventScreenHistoryMeetingViewDestroy = "EventScreenHistoryMeetingViewDestroy";

		[SerializeField] private TextMeshProUGUI titleChatAI;		
		[SerializeField] private Button buttonCancel;		

		[SerializeField] private GameObject ChatViewPrefab;
		[SerializeField] private SlotManagerView SlotManagerChat;

		[SerializeField] private TextMeshProUGUI textSizeCalculator;

		[SerializeField] private TextMeshProUGUI titleReopenMeeting;
		[SerializeField] private Button buttonReopenMeeting;

		private MeetingData _meeting;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			_meeting = (MeetingData)parameters[0];

			if (_meeting.ProjectId != -1)
			{
				ProjectInfoData projectInfo = WorkDayData.Instance.CurrentProject.GetProject(_meeting.ProjectId);
				_content.GetComponent<Image>().color = projectInfo.GetColor();
			}

			if (titleChatAI != null)
            {
				titleChatAI.text = LanguageController.Instance.GetText("text.history");
			}

			if (buttonReopenMeeting != null)
            {
				if (!_meeting.Completed)
				{
					buttonReopenMeeting.gameObject.SetActive(false);
				}
				else
				{
					if (ApplicationController.Instance.SelectedHuman == null)
					{
						buttonReopenMeeting.interactable = false;
					}
					else
					{
						if (ApplicationController.Instance.SelectedHuman == ApplicationController.Instance.HumanPlayer)
						{
							buttonReopenMeeting.interactable = _meeting.HasPlayer(false);
						}
						else
						{
							buttonReopenMeeting.interactable = false;
						}
					}
					buttonReopenMeeting.onClick.AddListener(OnReopenMeeting);
                }
				buttonReopenMeeting.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.reopen.meeting");

            }

			buttonCancel.onClick.AddListener(OnCancel);

			LoadMeetingHistory();

			SystemEventController.Instance.Event += OnSystemEvent;
			UIEventController.Instance.Event += OnUIEvent;
		}

        public override void Destroy()
		{
			base.Destroy();

			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
		}

        private void OnCancel()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void LoadMeetingHistory()
        {
			SlotManagerChat.ClearCurrentGameObject(true);
			SlotManagerChat.Initialize(0, new List<ItemMultiObjectEntry>(), ChatViewPrefab);

			List<InteractionData> interactions = _meeting.GetInteractions();
			if (interactions != null)
			{
				for (int i = 0; i < interactions.Count; i++)
				{
					SlotManagerChat.AddItem(new ItemMultiObjectEntry(this.gameObject, SlotManagerChat.Data.Count, interactions[i], _meeting, textSizeCalculator));
				}
			}

			SlotManagerChat.SetVerticalScroll(0);
		}

		public override void ActivateContent(bool value)
		{
			base.ActivateContent(value);

			LoadMeetingHistory();
		}

		private void OnReopenMeeting()
		{
			DateTime currTime = WorkDayData.Instance.CurrentProject.GetCurrentTime();			
			SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerRequestStartMeeting, _meeting, _meeting.TaskId, currTime);
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventScreenHistoryMeetingViewDestroy))
            {
				OnCancel();
			}
			if (nameEvent.Equals(ItemChatView.EventItemChatViewDelete))
			{
				if (this.gameObject == (GameObject)parameters[0])
                {
					InteractionData interactionToDelete = (InteractionData)parameters[2];
					if (interactionToDelete != null)
                    {
						List<InteractionData> interactions = _meeting.GetInteractions();
						if (interactions.Remove(interactionToDelete))
                        {
							_meeting.SetInteractions(interactions);
							LoadMeetingHistory();
						}
					}
                }
			}
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(MeetingController.EventMeetingControllerStopMeeting))
			{
				MeetingData meeting = (MeetingData)parameters[0];
				if ((meeting != null) && (meeting == _meeting))
                {
					_meeting = null;
					OnCancel();
				}
			}
			if (nameEvent.Equals(MeetingController.EventMeetingControllerMeetingStopped))
			{
				MeetingData meeting = (MeetingData)parameters[0];
				if ((meeting != null) && (meeting == _meeting))
				{
					_meeting = null;
					OnCancel();
				}
			}			
			if (nameEvent.Equals(ScreenDialogView.EventScreenDialogViewAddAIInteraction))
			{
				MeetingData meeting = (MeetingData)parameters[0];
				if ((meeting != null) && (meeting == _meeting))
				{
					if (titleChatAI != null)
					{
						UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationDestroyAllEvenIgnored);
					}
					LoadMeetingHistory();
				}
			}
		}
	}
}