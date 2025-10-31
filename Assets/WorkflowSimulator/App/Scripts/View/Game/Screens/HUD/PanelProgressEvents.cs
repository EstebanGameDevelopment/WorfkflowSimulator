using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.CurrentDocumentInProgress;
using static yourvrexperience.WorkDay.MeetingController;

namespace yourvrexperience.WorkDay
{
	public class PanelProgressEvents : MonoBehaviour
	{
		public const string EventPanelProgressEventsExpanded = "EventPanelProgressEventsExpanded";

        [SerializeField] private CustomButton buttonListTODO;
		[SerializeField] private CustomButton buttonListInProgress;
		[SerializeField] private CustomButton buttonListDone;
		[SerializeField] private CustomButton buttonListMeetings;
		[SerializeField] private CustomButton buttonCollapse;

		[SerializeField] private GameObject panelFeedback;
		[SerializeField] private TextMeshProUGUI textFeedback;
		[SerializeField] private TextMeshProUGUI textTitle;

		[SerializeField] private GameObject panelListEventsProgress;
		[SerializeField] private SlotManagerView SlotManagerEventsProgress;
		[SerializeField] private GameObject ProgressItemViewPrefab;
		[SerializeField] private GameObject MeetingProgressItemViewPrefab;
		[SerializeField] private GameObject ProgressItemViewReversedPrefab;

		[SerializeField] private CustomButton buttonPanelInProgress;
		[SerializeField] private CustomButton buttonPanelDone;
		[SerializeField] private CustomButton buttonPanelTODO;
		[SerializeField] private CustomButton buttonPanelMeetings;

		[SerializeField] private SlotManagerView SlotManagerPopUpEvents;

		private bool _hasBeenDestroyed = false;
		private float _timeOutToRefresh = 0;
		private StateCurrentDoc _currentState = StateCurrentDoc.NONE;
		private bool _enabledRefresh = true;

		public void Initialize()
		{
			buttonListTODO.PointerEnterButton += OnEnterListTODO;
			buttonListTODO.PointerExitButton += OnExitButtonClear;

			buttonListInProgress.PointerEnterButton += OnEnterListInProgress;
			buttonListInProgress.PointerExitButton += OnExitButtonClear;
			
			buttonListDone.PointerEnterButton += OnEnterListDone;
			buttonListDone.PointerExitButton += OnExitButtonClear;

			buttonListMeetings.PointerEnterButton += OnEnterListMeetings;
			buttonListMeetings.PointerExitButton += OnExitButtonClear;

			buttonListTODO.onClick.AddListener(OnButtonListTODO);
			buttonListInProgress.onClick.AddListener(OnButtonListInProgress);
			buttonListDone.onClick.AddListener(OnButtonListDone);
			buttonListMeetings.onClick.AddListener(OnButtonListMeetings);

			buttonCollapse.onClick.AddListener(OnButtonCollapse);

			buttonPanelTODO.onClick.AddListener(OnButtonListTODO);
			buttonPanelInProgress.onClick.AddListener(OnButtonListInProgress);
			buttonPanelDone.onClick.AddListener(OnButtonListDone);
			buttonPanelMeetings.onClick.AddListener(OnButtonListMeetings);

			SlotManagerPopUpEvents.ClearCurrentGameObject(true);
			SlotManagerPopUpEvents.Initialize(0, new List<ItemMultiObjectEntry>(), ProgressItemViewReversedPrefab);

			SystemEventController.Instance.Event += OnSystemEvent;
			UIEventController.Instance.Event += OnUIEvent;

			OnExitButtonClear(null);
			DisablePanels();
			EnableButtons(true);

			_currentState = StateCurrentDoc.NONE;
		}

        void OnDestroy()
		{
			Destroy();
		}

		public void Destroy()
        {
			if (!_hasBeenDestroyed)
            {
				_hasBeenDestroyed = true;
				if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
				if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
			}
		}

		private void OnButtonCollapse()
		{
			DisablePanels();
			EnableButtons(true);
			_currentState = StateCurrentDoc.NONE;
		}

		private void EnableButtons(bool value)
        {
			buttonListTODO.gameObject.SetActive(value);
			buttonListInProgress.gameObject.SetActive(value);
			buttonListDone.gameObject.SetActive(value);

			buttonCollapse.gameObject.SetActive(!value);
		}

		private void DisablePanels()
        {
			panelListEventsProgress.gameObject.SetActive(false);
			SlotManagerEventsProgress.ClearCurrentGameObject(true);
		}

		private void OnExitButtonClear(CustomButton value)
		{
			panelFeedback.gameObject.SetActive(false);
			textFeedback.text = "";
		}

		private void OnEnterListTODO(CustomButton value)
		{
			panelFeedback.gameObject.SetActive(true);
			textFeedback.text = LanguageController.Instance.GetText("progress.panel.documents.todo");
		}

		private void OnEnterListDone(CustomButton value)
		{
			panelFeedback.gameObject.SetActive(true);
			textFeedback.text = LanguageController.Instance.GetText("progress.panel.documents.done");
        }

		private void OnEnterListInProgress(CustomButton value)
		{
			panelFeedback.gameObject.SetActive(true);
			textFeedback.text = LanguageController.Instance.GetText("progress.panel.documents.in.progress");
        }

		private void OnEnterListMeetings(CustomButton obj)
		{
			panelFeedback.gameObject.SetActive(true);
			textFeedback.text = LanguageController.Instance.GetText("progress.panel.meetings.in.progress");
        }

		private void OnButtonListDone()
		{
			OnExitButtonClear(null);
			DisablePanels();
			EnableButtons(false);

			LoadProgressDocsData(StateCurrentDoc.DONE);
			textTitle.text = LanguageController.Instance.GetText("progress.panel.documents.done");
        }

		private void OnButtonListInProgress()
		{
			OnExitButtonClear(null);
			DisablePanels();
			EnableButtons(false);

			LoadProgressDocsData(StateCurrentDoc.DOING);
			textTitle.text = LanguageController.Instance.GetText("progress.panel.documents.in.progress");
        }

		private void EnableInteraction(bool enabled)
		{
			buttonListTODO.interactable = enabled;
            buttonListInProgress.interactable = enabled;
            buttonListDone.interactable = enabled;
            buttonListMeetings.interactable = enabled;
            buttonCollapse.interactable = enabled;

            buttonPanelInProgress.interactable = enabled;
            buttonPanelDone.interactable = enabled;
            buttonPanelTODO.interactable = enabled;
            buttonPanelMeetings.interactable = enabled;
        }

        private void EnableVisibility(bool enabled)
        {
            buttonListTODO.gameObject.SetActive(enabled);
            buttonListInProgress.gameObject.SetActive(enabled);
            buttonListDone.gameObject.SetActive(enabled);
            buttonListMeetings.gameObject.SetActive(enabled);
        }

        private void OnButtonListTODO()
		{
			OnExitButtonClear(null);
			DisablePanels();
			EnableButtons(false);

			LoadProgressDocsData(StateCurrentDoc.TODO);
			textTitle.text = LanguageController.Instance.GetText("progress.panel.documents.todo");
        }

		private void OnButtonListMeetings()
		{
			OnExitButtonClear(null);
			DisablePanels();
			EnableButtons(false);

			LoadProgressDocsData(StateCurrentDoc.MEETING);
			textTitle.text = LanguageController.Instance.GetText("progress.panel.meetings.in.progress");
        }

		private void LoadProgressDocsData(StateCurrentDoc state)
        {
			UIEventController.Instance.DispatchUIEvent(EventPanelProgressEventsExpanded);
			_enabledRefresh = true;
			_currentState = state;
			if (state == StateCurrentDoc.MEETING)
            {
				FillListMeetings(MeetingController.Instance.MeetingsInProgress);
			}
			else
            {
				List<CurrentDocumentInProgress> allDocs = AICommandsController.Instance.GetAllDocuments(state);
				List<CurrentDocumentInProgress> finalDocs = new List<CurrentDocumentInProgress>();
				foreach (CurrentDocumentInProgress doc in allDocs)
				{
					bool shouldAdd = true;
					foreach (CurrentDocumentInProgress docCheck in finalDocs)
					{
						if (doc.GetDocUniqueID().Equals(docCheck.GetDocUniqueID()))
						{
							shouldAdd = false;
						}
					}
					if (shouldAdd)
					{
						finalDocs.Add(doc);
					}
				}
				List<CurrentDocumentInProgress> orderedDocs = null;
				switch (state)
				{
					case StateCurrentDoc.TODO:
						orderedDocs = finalDocs.OrderByDescending(d => d.GetCreatedTime()).ToList();
						break;

					case StateCurrentDoc.DOING:
						orderedDocs = finalDocs.OrderByDescending(d => d.GetStartTime()).ToList();
						break;

					case StateCurrentDoc.DONE:
						orderedDocs = finalDocs.OrderByDescending(d => d.GetDoneTime()).ToList();
						break;
				}

				if (orderedDocs != null)
				{
					FillListDocs(orderedDocs);
				}
			}
		}

		private void FillListDocs(List<CurrentDocumentInProgress> documents)
        {
			panelListEventsProgress.SetActive(true);
			SlotManagerPopUpEvents.ClearCurrentGameObject(true);
			SlotManagerEventsProgress.ClearCurrentGameObject(true);
			SlotManagerEventsProgress.Initialize(0, new List<ItemMultiObjectEntry>(), ProgressItemViewPrefab);
			
			foreach (CurrentDocumentInProgress document in documents)
			{
				var (taskData, board) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(document.TaskID);                
				if (taskData != null)
				{
                    SlotManagerEventsProgress.AddItem(new ItemMultiObjectEntry(this.gameObject, SlotManagerEventsProgress.Data.Count, document));
                }                
			}
		}

		private void FillListMeetings(List<MeetingInProgress> meetings)
		{
			panelListEventsProgress.SetActive(true);
			SlotManagerPopUpEvents.ClearCurrentGameObject(true);
			SlotManagerEventsProgress.ClearCurrentGameObject(true);
			SlotManagerEventsProgress.Initialize(0, new List<ItemMultiObjectEntry>(), MeetingProgressItemViewPrefab);

			foreach (MeetingInProgress meeting in meetings)
			{
				SlotManagerEventsProgress.AddItem(new ItemMultiObjectEntry(this.gameObject, SlotManagerEventsProgress.Data.Count, meeting));
			}
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ItemMeetingProgressView.EventItemMeetingProgressViewSelected))
            {
				if (this.gameObject == (GameObject)parameters[0])
				{
					if ((int)parameters[2] == -1)
					{
						UIEventController.Instance.DispatchUIEvent(ScreenHistoryMeetingView.EventScreenHistoryMeetingViewDestroy);
					}
					else
					{
						UIEventController.Instance.DispatchUIEvent(ScreenHistoryMeetingView.EventScreenHistoryMeetingViewDestroy);
						ScreenController.Instance.CreateScreen(ScreenHistoryMeetingView.ScreenHUDName, false, false, (MeetingData)parameters[3]);
					}
				}				
            }
			if (nameEvent.Equals(ItemTaskProgressEventView.EventItemTaskProgressEventViewSelected))
            {
				if (this.gameObject == (GameObject)parameters[0])
                {
					if ((int)parameters[2] == -1)
                    {
						SystemEventController.Instance.DispatchSystemEvent(LevelView.EventLevelViewLinesRequestDestroy);
					}
					else
                    {
						_enabledRefresh = false;
						UIEventController.Instance.DispatchUIEvent(ItemTaskView.EventItemTaskViewHUDSelected, null, null, 1, (TaskItemData)parameters[4]);
					}
				}
            }
			if (nameEvent.Equals(ItemTaskProgressEventView.EventItemTaskProgressEventViewDelete))
			{
				if (this.gameObject == (GameObject)parameters[0])
				{
					SlotManagerPopUpEvents.RemoveItem((ItemMultiObjectEntry)parameters[1]);
                }
			}
			if (nameEvent.Equals(ScreenInfoItemView.EventScreenInfoItemViewReportExpandedInfo))
			{
				if ((bool)parameters[0])
				{
                    if (_currentState != StateCurrentDoc.NONE)
                    {
                        OnButtonCollapse();
                    }
                }
            }
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ScreenPanelEditionView.EventScreenPanelEditionActivation))
			{
				if ((bool)parameters[0])
				{
					if (_currentState != StateCurrentDoc.NONE)
					{
                        OnButtonCollapse();
                    }
                    SlotManagerPopUpEvents.ClearCurrentGameObject(true);
                    EnableVisibility(false);
                }
				else
				{
                    EnableVisibility(true);
                }
            }
            if (nameEvent.Equals(MeetingController.EventMeetingControllerMeetingStopped) 
				|| nameEvent.Equals(MeetingController.EventMeetingControllerMeetingStarted))
            {
				if (_currentState == StateCurrentDoc.MEETING)
                {
					OnButtonListMeetings();
				}
			}
			if (nameEvent.Equals(ScreenDialogView.EventScreenDialogViewStarted))
            {
				OnButtonCollapse();
			}
			if (nameEvent.Equals(CurrentDocumentInProgress.EventCurrentDocumentInProgressStateStarted))
			{
				CurrentDocumentInProgress currentWorking = (CurrentDocumentInProgress)parameters[0];
				float timeToDisappear = 5 + SlotManagerPopUpEvents.Data.Count;
				bool shouldAdd = true;
				foreach(ItemMultiObjectEntry item in SlotManagerPopUpEvents.Data)
                {
					CurrentDocumentInProgress entry = (CurrentDocumentInProgress)item.Objects[2];
					if (entry.Equals(currentWorking))
                    {
						shouldAdd = false;
					}
                }

				if (shouldAdd)
                {
					SlotManagerPopUpEvents.AddItem(new ItemMultiObjectEntry(this.gameObject, SlotManagerPopUpEvents.Data.Count, currentWorking, true, timeToDisappear));
				}				
			}
			if (nameEvent.Equals(CurrentDocumentInProgress.EventCurrentDocumentInProgressStateStopped))
			{
				CurrentDocumentInProgress currentWorking = (CurrentDocumentInProgress)parameters[0];
				float timeToDisappear = 5 + SlotManagerPopUpEvents.Data.Count;
				SlotManagerPopUpEvents.AddItem(new ItemMultiObjectEntry(this.gameObject, SlotManagerPopUpEvents.Data.Count, currentWorking, false, timeToDisappear));
			}
			if (nameEvent.Equals(AICommandsController.EventAICommandsControllerEvaluationDocsCompleted))
            {
				_enabledRefresh = true;
				_timeAcum = 10000;
				_timeOutToRefresh = 10000;
				switch (_currentState)
				{
					case StateCurrentDoc.TODO:
						OnButtonListTODO();
						break;

					case StateCurrentDoc.DOING:
						OnButtonListInProgress();
						break;

					case StateCurrentDoc.DONE:
						OnButtonListDone();
						break;
				}
			}
			if (nameEvent.Equals(ClockController.EventClockControllerTimeSpeedUp))
            {
				if ((bool)parameters[0])
                {
					_enabledRefresh = true;
					_timeAcum = 10000;
					_timeOutToRefresh = 10000;
				}
			}
		}

		private float _timeAcum = 0;

        private void Update()
        {
			if (panelListEventsProgress.activeSelf && _enabledRefresh)
            {
				_timeAcum += Time.deltaTime;
				if (_timeAcum > 1)
                {
					_timeAcum = 0;
					_timeOutToRefresh += (float)ApplicationController.Instance.TimeHUD.IncrementTime.TotalSeconds;
					if (_timeOutToRefresh > 60 * 5)
					{
						_timeOutToRefresh = 0;
						SlotManagerEventsProgress.ApplyGenericAction();
					}
				}
			}			
        }
    }
}