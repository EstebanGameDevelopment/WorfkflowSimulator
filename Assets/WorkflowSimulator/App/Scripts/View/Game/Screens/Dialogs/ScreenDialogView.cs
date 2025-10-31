using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.ItemReferenceView;

namespace yourvrexperience.WorkDay
{
	public class ScreenDialogView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenDialogView";
		
		public const string EventScreenDialogViewAddAIInteraction = "EventScreenDialogViewAddAIInteraction";
		public const string EventScreenDialogViewStarted = "EventScreenDialogViewStarted";
		public const string EventScreenDialogViewClosed = "EventScreenDialogViewClosed";
		public const string EventScreenDialogViewDelayForceNext = "EventScreenDialogViewDelayForceNext";
		public const string EventScreenDialogViewDisableBecauseMeetingFinished = "EventScreenDialogViewDisableBecauseMeetingFinished";
		public const string EventScreenDialogViewForceLeaveSocialMeetings = "EventScreenDialogViewForceLeaveSocialMeetings";
		public const string EventScreenDialogViewForceNextReplyInterruptor = "EventScreenDialogViewForceNextReplyInterruptor";
		public const string EventScreenDialogViewForceEnableNextButtons = "EventScreenDialogViewForceEnableNextButtons";

		private string EventScreenDialogViewConcludedMeeting = "EventScreenDialogViewConcludedMeeting";

		public const string SubEventScreenDialogViewEditedData = "SubEventScreenDialogViewEditedData";
		
		[SerializeField] private GameObject backgroundDialog;
		[SerializeField] private GameObject backgroundMeeting;
		[SerializeField] private TextMeshProUGUI nameLeft;
		[SerializeField] private TextMeshProUGUI nameRight;
		[SerializeField] private TextMeshProUGUI textDialog;
		[SerializeField] private TextMeshProUGUI textFeedback;
		[SerializeField] private ScrollRect textScrollRect;
		[SerializeField] private RawImage avatarLeft;
		[SerializeField] private RawImage avatarRight;
		[SerializeField] private TMP_InputField inputText;
		[SerializeField] private CustomButton buttonNext;
		[SerializeField] private CustomButton buttonClose;
		[SerializeField] private CustomButton buttonHistory;
		[SerializeField] private CustomButton buttonData;
		[SerializeField] private CustomButton buttonMeeting;
		[SerializeField] private CustomButton buttonSave;
		[SerializeField] private CustomButton buttonExpand;
		[SerializeField] private CustomButton buttonCollapse;
		[SerializeField] private GameObject panelListAssistants;
		[SerializeField] private SlotManagerView SlotManagerAssistants;
		[SerializeField] private GameObject PrefabAssistant;

		[SerializeField] private Camera cameraPrefabLeft;
		[SerializeField] private Camera cameraPrefabRight;

		[SerializeField] private TextMeshProUGUI textNameMeeting;
		
		[SerializeField] private IconColorView iconMemberLeft;
		[SerializeField] private IconColorView iconMemberRight;

		[SerializeField] private TextMeshProUGUI textTypeReference;
		[SerializeField] private GameObject panelListReferences;
		[SerializeField] private CustomButton buttonCloseReferences;
		[SerializeField] private CustomButton buttonConfirmReference;
		[SerializeField] private SlotManagerView SlotManagerReferences;
		[SerializeField] private GameObject PrefabReference;

		[SerializeField] private TextMeshProUGUI textExtraInfo;
		[SerializeField] private GameObject panelExtraInfo;
		[SerializeField] private CustomButton buttonExpandExtraInfo;

		public override string NameScreen
		{
			get { return ScreenName; }
		}

		private int _turnName = -1;
		private MeetingData _meeting;
		private TaskItemData _task;
		private HumanView _human;
		private GroupInfoData _groupHuman;
		private string _data;

		private GameObject _actorRight;
		private GameObject _actorLeft;

		private Camera _cameraLeft;
		private Camera _cameraRight;

		private string _imageToConfirm = "";
		private string _nameActorLeft = "";
		private string _nameActorRight = "";

		private ReferenceTypes _typeReferenceSelected = ReferenceTypes.None;
		private string _idReferenceSelected = "";
		private string _textReferenceSelected = "";

		private bool _meetingConcluded = false;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			_meeting = (MeetingData)parameters[0];
			EventScreenDialogViewConcludedMeeting = CommandAIMeetingResponse.EventCommandAIMeetingCompletedAIProcessing_DEFINITION + _meeting.GetUID();
			if (_meeting.TaskId != -1)
            {
				var (task, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_meeting.TaskId);
				_task = task;
			}
			_human = null;
			if (parameters.Length > 1)
            {
				if (parameters[1] != null)
                {
					_human = (HumanView)parameters[1];
				}
			}

			if (_meeting.ProjectId != -1)
            {
				ProjectInfoData projectInfo = WorkDayData.Instance.CurrentProject.GetProject(_meeting.ProjectId);
				backgroundDialog.GetComponent<Image>().color = projectInfo.GetColor();
				backgroundMeeting.GetComponent<Image>().color = projectInfo.GetColor();
				ApplicationController.Instance.LastProjectColor = projectInfo.GetColor();
			}
			else
            {
				ApplicationController.Instance.LastProjectColor = Color.white;
			}			

			if (_human == null)
            {
				List<string> members = _meeting.GetMembers();
				foreach (string member in members)
                {
					GroupInfoData group = WorkDayData.Instance.CurrentProject.GetGroupByName(member);
					if (group == null)
                    {
						var (itemGO, itemData) = ApplicationController.Instance.LevelView.GetItemByName(member);
						if (itemData.IsPlayer)
                        {
							_human = itemGO.GetComponent<HumanView>();
                        }
					}
					else
                    {
						foreach (string memberGroup in group.Members)
                        {
							var (itemGO, itemData) = ApplicationController.Instance.LevelView.GetItemByName(memberGroup);
							if (itemData.IsPlayer)
							{
								_human = itemGO.GetComponent<HumanView>();
							}
						}
					}
                }
            }

			avatarLeft.gameObject.SetActive(false);
			avatarRight.gameObject.SetActive(false);

			nameLeft.text = "";
			nameRight.text = "";
			textDialog.text = "";
			inputText.text = "";
			textFeedback.text = "";

			textNameMeeting.text = _meeting.GetTimeStart().ToShortTimeString() + " - " + _meeting.GetTimeEnd().ToShortTimeString() + "\n" + _meeting.Name;

			if (_human == null)
            {
				inputText.gameObject.SetActive(false);
				buttonNext.interactable = false;
				buttonData.interactable = false;
			}

			buttonClose.PointerEnterButton += OnCloseDialogEnter;
			buttonNext.PointerEnterButton += OnNextDialogEnter;
			buttonHistory.PointerEnterButton += OnHistoryEnter;
			buttonData.PointerEnterButton += OnDataEnter;
			buttonMeeting.PointerEnterButton += OnMeetingInfoEnter;
			buttonSave.PointerEnterButton += OnSaveEnter;
			buttonExpand.PointerEnterButton += OnExpandAssistantsEnter;

			buttonClose.PointerExitButton += OnResetFeedback;
			buttonNext.PointerExitButton += OnResetFeedback;
			buttonHistory.PointerExitButton += OnResetFeedback;
			buttonData.PointerExitButton += OnResetFeedback;
			buttonMeeting.PointerExitButton += OnResetFeedback;
			buttonSave.PointerExitButton += OnResetFeedback;

			buttonClose.onClick.AddListener(OnCloseDialog);
			buttonNext.onClick.AddListener(OnNextDialogAction);
			buttonHistory.onClick.AddListener(OnHistory);
			buttonData.onClick.AddListener(OnData);
			buttonMeeting.onClick.AddListener(OnMeetingInfo);
			buttonSave.onClick.AddListener(OnSaveProject);
			buttonExpand.onClick.AddListener(OnExpandAssistants);
			buttonCollapse.onClick.AddListener(OnCollapseAssistants);

			UIEventController.Instance.Event += OnUIEvent;
			SystemEventController.Instance.Event += OnSystemEvent;

            if (_human != null)
            {
				_groupHuman = WorkDayData.Instance.CurrentProject.GetGroupOfMember(_human.NameHuman);
			}

			if (_meeting.IsProcessingAI)
            {
				inputText.interactable = false;
				inputText.text = LanguageController.Instance.GetText("screen.dialog.wait.for.ai.to.process");
			}
			
			LoadAssistants();
			panelListAssistants.SetActive(false);

			iconMemberLeft.Locked = true;
			iconMemberRight.Locked = true;

			iconMemberLeft.gameObject.SetActive(false);
			iconMemberRight.gameObject.SetActive(false);

			if (_meeting.HasPlayer(true))
			{
				buttonData.interactable = true;
			}
			else
			{
				buttonData.interactable = false;
			}

			if (_meeting.IsSocialMeeting() || _meeting.IsInterruptionMeeting())
			{
				buttonSave.interactable = false;
			}

			if (_meeting.IsInterruptionMeeting())
			{
				buttonData.interactable = false;
				UIEventController.Instance.DelayUIEvent(EventScreenDialogViewDelayForceNext, 0.2f);
			}

			panelListReferences.SetActive(false);
			buttonCloseReferences.onClick.AddListener(OnCloseReferencePanel);
			buttonConfirmReference.onClick.AddListener(OnConfirmReferenceSelected);

			buttonCloseReferences.PointerEnterButton += OnCloseReferenceEnter;
			buttonCloseReferences.PointerExitButton += OnResetFeedback;

			buttonConfirmReference.PointerEnterButton += OnConfirmReferenceEnter;
			buttonConfirmReference.PointerExitButton += OnResetFeedback;

			inputText.onValueChanged.AddListener(OnInputTextChanged);
			
			panelExtraInfo.SetActive(false);
			textExtraInfo.text = "";
			buttonExpandExtraInfo.onClick.AddListener(OnExpandReferenceSelection);
			buttonExpandExtraInfo.PointerEnterButton += OnExpandReferenceSelectionEnter;
			buttonExpandExtraInfo.PointerExitButton += OnResetFeedback;

			SystemEventController.Instance.DispatchSystemEvent(EventScreenDialogViewStarted, _meeting);
			UIEventController.Instance.DispatchUIEvent(TimeHUD.EventTimeHUDUpdateEnableInteraction, false);
			SystemEventController.Instance.DispatchSystemEvent(EditionSubStateIdle.EventSubStateIdleEnableWorldSelection, false);
			ApplicationController.Instance.TimeHUD.LockedInteraction = true;
		}

        public override void Destroy()
		{
			base.Destroy();

			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;

			_meeting = null;
			_human = null;

			if (_actorRight != null)
            {
				GameObject.Destroy(_actorRight);
            }
			if (_actorLeft != null)
			{
				GameObject.Destroy(_actorLeft);
			}
			_actorLeft = null;
			_actorRight = null;

			if (_cameraLeft != null)
            {
				GameObject.Destroy(_cameraLeft.gameObject);
			}
			if (_cameraRight != null)
			{
				GameObject.Destroy(_cameraRight.gameObject);
			}
			_cameraLeft = null;
			_cameraRight = null;

			ApplicationController.Instance.TimeHUD.LockedInteraction = false;
			SystemEventController.Instance.DispatchSystemEvent(PlayerView.EventPlayerAppEnableMovement, true);
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerReactivateAllScreens);
			UIEventController.Instance.DispatchUIEvent(TimeHUD.EventTimeHUDUpdateEnableInteraction, true);
			SystemEventController.Instance.DispatchSystemEvent(EditionSubStateIdle.EventSubStateIdleEnableWorldSelection, true);
			SystemEventController.Instance.DispatchSystemEvent(EventScreenDialogViewClosed);
		}

		private void OnExpandReferenceSelectionEnter(CustomButton value)
		{
			textFeedback.text = LanguageController.Instance.GetText("text.expand.reference.selection");
		}

		private void OnExpandReferenceSelection()
		{
			if (_typeReferenceSelected != ReferenceTypes.None)
			{
				switch (_typeReferenceSelected)
				{
					case ReferenceTypes.Document:
						OnMeetingInfo();
						UIEventController.Instance.DispatchUIEvent(ScreenMeetingView.EventScreenMeetingViewForceDataButton);
						UIEventController.Instance.DispatchUIEvent(ItemDataView.EventItemDataViewForceSelection, int.Parse(_idReferenceSelected));
						break;

					case ReferenceTypes.Task:
						int taskID = int.Parse(_idReferenceSelected);
						var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(taskID);
						GameObject taskView = ScreenController.Instance.CreateScreen(ScreenTaskView.ScreenName, false, false, taskItemData, boardName, true);
						UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, taskView, _canvas.sortingOrder + 1);
						break;
				}
			}
		}

		private void OnConfirmReferenceEnter(CustomButton value)
		{
			textFeedback.text = LanguageController.Instance.GetText("text.confirm.reference.choice");
		}

		private void OnCloseReferenceEnter(CustomButton value)
		{
			textFeedback.text = LanguageController.Instance.GetText("text.cancel.reference");
		}

		private void OnInputTextChanged(string value)
		{
			if (value.Length > 0)
            {
				string charSelection = value.Substring(value.Length - 1, 1);

				bool foundChar = false;
				if (charSelection.Equals("@"))
                {
					foundChar = true;
					LoadReferenceList(ReferenceTypes.Person);
				}
				if (charSelection.Equals("#"))
				{
					foundChar = true;
					LoadReferenceList(ReferenceTypes.Document);
				}
				if (charSelection.Equals("~"))
				{
					foundChar = true;
					LoadReferenceList(ReferenceTypes.Task);
				}

				if (foundChar)
                {
					inputText.onValueChanged.RemoveListener(OnInputTextChanged);
					inputText.text = value.Substring(0, value.Length - 1);
					inputText.onValueChanged.AddListener(OnInputTextChanged);
				}
			}
		}

		private void LoadReferenceList(ReferenceTypes type)
        {
			panelListReferences.SetActive(true);
			SlotManagerReferences.ClearCurrentGameObject(true);
			List<ItemMultiObjectEntry> itemsReferences = new List<ItemMultiObjectEntry>();

			switch (type)
            {
				case ReferenceTypes.Person:
					textTypeReference.text = LanguageController.Instance.GetText("text.persons");
					List<string> assistants = _meeting.GetAssistingMembers(false);
					for (int i = 0; i < assistants.Count; i++)
					{
						itemsReferences.Add(new ItemMultiObjectEntry(this.gameObject, i, ReferenceTypes.Person, assistants[i]));
					}
					break;

				case ReferenceTypes.Document:
					textTypeReference.text = LanguageController.Instance.GetText("text.documents");
					List<string> docsIDs = new List<string>();
					if (_meeting.TaskId != -1)
                    {
						var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_meeting.TaskId);
						List<DocumentData> docsTask = taskItemData.GetData();
						foreach (DocumentData doc in docsTask)
                        {
							if (!docsIDs.Contains(doc.Id.ToString()))
                            {
								docsIDs.Add(doc.Id.ToString());
							}
						}
					}
					else
                    {
						List<DocumentData> docsMeeting = _meeting.GetData();
						foreach (DocumentData doc in docsMeeting)
						{
							if (!docsIDs.Contains(doc.Id.ToString()))
							{
								docsIDs.Add(doc.Id.ToString());
							}
						}
					}
					List<DocumentData> docsGlobal = WorkDayData.Instance.CurrentProject.GetDocuments();
					foreach (DocumentData doc in docsGlobal)
					{
						if (!docsIDs.Contains(doc.Id.ToString()))
						{
							docsIDs.Add(doc.Id.ToString());
						}
					}
					for (int i = 0; i < docsIDs.Count; i++)
					{
						itemsReferences.Add(new ItemMultiObjectEntry(this.gameObject, i, ReferenceTypes.Document, docsIDs[i]));
					}
					break;

				case ReferenceTypes.Task:
					textTypeReference.text = LanguageController.Instance.GetText("text.tasks");
					List<BoardData> boards = WorkDayData.Instance.CurrentProject.GetAllBoards();
					foreach(BoardData board in boards)
                    {
						if (board.ProjectId == _meeting.ProjectId)
                        {
							List<TaskItemData> tasks = board.GetTasks();
							foreach (TaskItemData task in tasks)
                            {
								itemsReferences.Add(new ItemMultiObjectEntry(this.gameObject, itemsReferences.Count, ReferenceTypes.Task, task.UID.ToString()));
							}
						}
					}
					break;
            }

			SlotManagerReferences.Initialize(itemsReferences.Count, itemsReferences, PrefabReference);
		}

		private void OnCloseReferencePanel()
		{
			panelListReferences.SetActive(false);
			panelExtraInfo.SetActive(false);
			_typeReferenceSelected = ReferenceTypes.None;			
		}

		private void OnConfirmReferenceSelected()
		{
			panelListReferences.SetActive(false);
			panelExtraInfo.SetActive(false);

			if (_typeReferenceSelected != ReferenceTypes.None)
            {
				switch (_typeReferenceSelected)
                {
					case ReferenceTypes.Person:
						inputText.text += "<color=red>"+ _textReferenceSelected + "</color> ";						
						break;

					case ReferenceTypes.Document:
						inputText.text += "<color=blue>" + _textReferenceSelected + "</color> ";
						break;

					case ReferenceTypes.Task:
						inputText.text += "<color=green>" + _textReferenceSelected + "</color> ";
						break;
                }
				_typeReferenceSelected = ReferenceTypes.None;
				inputText.caretPosition = inputText.text.Length;
			}
		}

		private void OnExpandAssistantsEnter(CustomButton obj)
		{
			textFeedback.text = LanguageController.Instance.GetText("screen.dialog.see.asisstants.meeting");
		}

		private void OnSaveEnter(CustomButton value)
		{
			textFeedback.text = LanguageController.Instance.GetText("screen.dialog.save.project");
		}

		private void OnResetFeedback(CustomButton value)
		{
			textFeedback.text = "";
		}

		private void OnMeetingInfoEnter(CustomButton value)
		{
			textFeedback.text = LanguageController.Instance.GetText("screen.dialog.information.about.current.meeting");
		}

		private void OnDataEnter(CustomButton value)
		{
			textFeedback.text = LanguageController.Instance.GetText("screen.dialog.add.additional.data.like.images");
		}

		private void OnHistoryEnter(CustomButton value)
		{
			textFeedback.text = LanguageController.Instance.GetText("screen.dialog.check.chat.history");
		}

		private void OnNextDialogEnter(CustomButton value)
		{
			textFeedback.text = LanguageController.Instance.GetText("screen.dialog.send.message.or.request.ai.response");
		}

		private void OnCloseDialogEnter(CustomButton value)
		{
			textFeedback.text = LanguageController.Instance.GetText("screen.dialog.close.dialog.and.end.meeting");
		}

		private void OnCollapseAssistants()
		{
			buttonExpand.gameObject.SetActive(true);
			buttonHistory.gameObject.SetActive(true);
			buttonSave.gameObject.SetActive(true);
			buttonMeeting.gameObject.SetActive(true);

			panelListAssistants.SetActive(false);
		}

		private void OnExpandAssistants()
		{
			buttonExpand.gameObject.SetActive(false);
			buttonHistory.gameObject.SetActive(false);
			buttonSave.gameObject.SetActive(false);
			buttonMeeting.gameObject.SetActive(false);

			panelListAssistants.SetActive(true);
		}

		private void LoadAssistants()
		{
			SlotManagerAssistants.ClearCurrentGameObject(true);
			List<ItemMultiObjectEntry> itemsUserStories = new List<ItemMultiObjectEntry>();
			string[] assistants = _meeting.ExtraData.Split(",");
			for (int i = 0; i < assistants.Length; i++)
			{
				if (assistants.Length > 0)
                {
					itemsUserStories.Add(new ItemMultiObjectEntry(this.gameObject, i, assistants[i]));
				}				
			}
			SlotManagerAssistants.Initialize(itemsUserStories.Count, itemsUserStories, PrefabAssistant);
		}

		private void OnMeetingInfo()
		{
			GameObject screenMeeting = ScreenController.Instance.CreateScreen(ScreenMeetingView.ScreenName, false, false, _meeting.TaskId, _meeting);
			UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, screenMeeting, _canvas.sortingOrder + 1);
			UIEventController.Instance.DispatchUIEvent(ScreenMeetingView.EventScreenMeetingViewDisableBecauseRunningMeeting, screenMeeting);
		}

		private void OnCloseDialog()
		{
			if (_imageToConfirm.Length > 0)
            {
				string imageToConfirm = _imageToConfirm;
				_imageToConfirm = "";
				WorkDayData.Instance.DeleteImage(imageToConfirm);
            }
			if (_meeting.HasPlayer(true))
			{
				GameObject targetScreenGO = ScreenController.Instance.CreateScreen(ScreenLeaveMeetingView.ScreenName, false, false, _meeting);
				UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, targetScreenGO, _canvas.sortingOrder + 1);
			}
			else
            {
				UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
			}
		}

		private void OnSaveProject()
		{
			UIEventController.Instance.DispatchUIEvent(TimeHUD.EventTimeHUDSaveProject);
		}

		private void OnHistory()
		{
			GameObject historyScreen = ScreenController.Instance.CreateScreen(ScreenHistoryMeetingView.ScreenName, false, true, _meeting);
			UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, historyScreen, _canvas.sortingOrder + 1);
		}

		private void OnData()
		{
			ScreenInformationView.CreateScreenInformation(ScreenMultiInputDataView.ScreenName, this.gameObject, LanguageController.Instance.GetText("text.info"), "", SubEventScreenDialogViewEditedData);
			UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, _data);
		}

		private void SetInteraction(InteractionData interaction)
        {
			bool rebuildAvatarGO = true;
			if (_nameActorLeft.Equals(interaction.NameActor))
            {
				_turnName = 0;
				rebuildAvatarGO = false;
			}			
			else
			if (_nameActorRight.Equals(interaction.NameActor))
			{
				_turnName = 1;
				rebuildAvatarGO = false;
			}
			else
            {
				_turnName = (_turnName + 1) % 2;
			}

			switch (_turnName)
            {
				case 0:
					GroupInfoData groupLeft = WorkDayData.Instance.CurrentProject.GetGroupOfMember(interaction.NameActor);
					if (groupLeft != null)
                    {
						nameLeft.text = interaction.NameActor + " (" + groupLeft.Name + ")";
						iconMemberLeft.gameObject.SetActive(true);
						iconMemberLeft.ApplyInfo(groupLeft.Name, groupLeft.GetColor());
					}
					else
                    {
						nameLeft.text = interaction.NameActor;
					}
					_nameActorLeft = interaction.NameActor;
					break;

				case 1:
					GroupInfoData groupRight = WorkDayData.Instance.CurrentProject.GetGroupOfMember(interaction.NameActor);
					if (groupRight != null)
					{
						nameRight.text = interaction.NameActor + " (" + groupRight.Name + ")";
						iconMemberRight.gameObject.SetActive(true);
						iconMemberRight.ApplyInfo(groupRight.Name, groupRight.GetColor());						
					}
					else
					{
						nameRight.text = interaction.NameActor;
					}
					_nameActorRight = interaction.NameActor;
					break;
            }
			textDialog.text = interaction.Text;
			float totalHeight = ScrollText.ResizeTextMeshPro(textDialog, 10);
			Vector2 startPosition = Vector2.zero - new Vector2(0, (totalHeight / 2) + 10);
			textScrollRect.content.anchoredPosition = startPosition;

			var (humanGO, humanData) = ApplicationController.Instance.LevelView.GetItemByName(interaction.NameActor);
			if (humanData != null)
            {
				AssetDefinitionItem assetDefinition = AssetsCatalogData.Instance.GetAssetById(humanData.CatalogId);

				float scaleAvatars = 0.5f;
				if (assetDefinition != null)
                {
					switch (_turnName)
					{
						case 0:
							avatarLeft.gameObject.SetActive(true);
							if (_cameraLeft == null)
							{
								_cameraLeft = Instantiate(cameraPrefabLeft);
								_cameraLeft.transform.parent = this.transform;
								_cameraLeft.transform.position = new Vector3(1000, 1000, 1000);
							}
							if (rebuildAvatarGO)
							{
								if (_actorLeft != null)
								{
									GameObject.Destroy(_actorLeft);
									_actorLeft = null;
								}
								_actorLeft = AssetBundleController.Instance.CreateGameObject(assetDefinition.AssetName);
								_actorLeft.transform.parent = this.transform;
								_actorLeft.transform.position = _cameraLeft.transform.position + _cameraLeft.transform.forward + Vector3.left * 0.2f;
								_actorLeft.transform.LookAt(_cameraLeft.transform);
								_actorLeft.transform.localScale = new Vector3(scaleAvatars, scaleAvatars, scaleAvatars);
							}
							break;

						case 1:
							avatarRight.gameObject.SetActive(true);
							if (_cameraRight == null)
							{
								_cameraRight = Instantiate(cameraPrefabRight);
								_cameraRight.transform.parent = this.transform;
								_cameraRight.transform.position = new Vector3(2000, 2000, 2000);
							}
							if (rebuildAvatarGO)
							{
								if (_actorRight != null)
								{
									GameObject.Destroy(_actorRight);
									_actorRight = null;
								}
								_actorRight = AssetBundleController.Instance.CreateGameObject(assetDefinition.AssetName);
								_actorRight.transform.parent = this.transform;
								_actorRight.transform.position = _cameraRight.transform.position + _cameraRight.transform.forward - Vector3.left * 0.1f;
								_actorRight.transform.LookAt(_cameraRight.transform);
								_actorRight.transform.localScale = new Vector3(scaleAvatars, scaleAvatars, scaleAvatars);
							}
							break;
					}
				}
			}
		}

		private void OnNextDialogAction()
		{
			if (_human == null)
            {
				if (!_meetingConcluded)
				{
					buttonNext.interactable = false;
					buttonData.interactable = false;
				}
			}
			else
            {
				if (_meeting.IsInterruptionMeeting())
				{
					if (inputText.text.Length > 0)
					{
						List<InteractionData> interactions = _meeting.GetInteractions();
						InteractionData aiInteraction = new InteractionData(false, _human.NameHuman, inputText.text, _data, "", WorkDayData.Instance.CurrentProject.GetCurrentTime());
						interactions.Add(aiInteraction);
						_meeting.SetInteractions(interactions);
						SystemEventController.Instance.DispatchSystemEvent(ScreenDialogView.EventScreenDialogViewAddAIInteraction, _meeting, aiInteraction);
						
						inputText.text = "";
						_data = "";
						_imageToConfirm = "";
					}
					else
                    {
						SystemEventController.Instance.DispatchSystemEvent(EventScreenDialogViewForceNextReplyInterruptor);
                    }
				}
				else
				{
					if (inputText.text.Length > 0)
					{
						CommandTalkInMeeting cmdHumanTalk = new CommandTalkInMeeting();
						cmdHumanTalk.Initialize(_human.NameHuman, _meeting, inputText.text, _data);
						SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdHumanTalk);
						inputText.text = "";
						_data = "";
						_imageToConfirm = "";
					}
					else
					{
						SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerRunAction, _meeting);
						if (!_meetingConcluded)
						{
							buttonNext.interactable = false;
							buttonData.interactable = false;
							inputText.interactable = false;
						}
					}
				}
			}
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ItemReferenceView.EventItemReferenceViewSelected))
            {
				if ((int)parameters[2] != -1)
                {
					_typeReferenceSelected = (ReferenceTypes)parameters[3];
					_idReferenceSelected = (string)parameters[4];
					_textReferenceSelected = (string)parameters[5];
					string descriptionReferenceSelected = (string)parameters[6];

					switch (_typeReferenceSelected)
                    {
						case ReferenceTypes.Person:
							panelExtraInfo.SetActive(true);
							textExtraInfo.text = descriptionReferenceSelected;
							buttonExpandExtraInfo.gameObject.SetActive(false);
							break;

						case ReferenceTypes.Document:
							panelExtraInfo.SetActive(true);
							buttonExpandExtraInfo.gameObject.SetActive(true);
							textExtraInfo.text = descriptionReferenceSelected;
							break;

						case ReferenceTypes.Task:
							panelExtraInfo.SetActive(true);
							buttonExpandExtraInfo.gameObject.SetActive(true);
							textExtraInfo.text = descriptionReferenceSelected;
							break;
                    }
				}
				else
                {
					panelExtraInfo.SetActive(false);
					_typeReferenceSelected = ReferenceTypes.None;
					_idReferenceSelected = "";
					_textReferenceSelected = "";
				}
			}
			if (nameEvent.Equals(EventScreenDialogViewDelayForceNext))
            {
				SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerRunAction, _meeting);
				if (!_meetingConcluded)
				{
					buttonNext.interactable = false;
					buttonData.interactable = false;
					inputText.interactable = false;
				}
			}
			if (nameEvent.Equals(SubEventScreenDialogViewEditedData))
            {
				if ((GameObject)parameters[0] == this.gameObject)
                {
					if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
					{
						_data = (string)parameters[2];
					}
				}
			}
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventScreenDialogViewForceEnableNextButtons))
            {				
				buttonNext.interactable = true;
				buttonData.interactable = true;
				inputText.interactable = true;
				if (_meeting.IsInterruptionMeeting())
				{
					buttonData.interactable = false;
				}
				inputText.text = "";
			}
			if (nameEvent.Equals(EditionSubStateBase.EventSubStateBaseEnableMovement))
            {
				if ((bool)parameters[0])
                {
					buttonNext.interactable = true;
					buttonData.interactable = true;
					inputText.interactable = true;
					if (_meeting.IsInterruptionMeeting())
					{
						buttonData.interactable = false;
					}
					inputText.text = "";
				}
			}
			if (nameEvent.Equals(EventScreenDialogViewForceLeaveSocialMeetings))
            {				
				if (_meeting.IsSocialMeeting())
                {
					SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerLeaveMeeting, _meeting, ApplicationController.Instance.SelectedHuman.NameHuman);
				}
			}
			if (nameEvent.Equals(EventScreenDialogViewDisableBecauseMeetingFinished))
            {
				if (_meeting == (MeetingData)parameters[0])
                {
					_meetingConcluded = true;
					_meeting.CanClose = true;
					_meeting.CanLeave = false;

					inputText.interactable = false;
					buttonNext.interactable = false;
					buttonData.interactable = true;
					buttonSave.interactable = false;
					buttonClose.interactable = true;
				}
			}
			if (nameEvent.Equals(EventScreenDialogViewConcludedMeeting))
            {
				if (_meeting == (MeetingData)parameters[0])
                {
					if (!_meetingConcluded)
                    {
						inputText.interactable = true;
						if (!_meeting.IsInterruptionMeeting())
						{
							inputText.text = "";
						}
					}
				}
			}
			if (nameEvent.Equals(MeetingController.EventMeetingControllerStopMeeting))
            {
				if (_meeting == (MeetingData)parameters[0])
                {
					UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
				}				
			}
			if (nameEvent.Equals(MeetingController.EventMeetingControllerLeaveMeeting))
			{
				if (_meeting == (MeetingData)parameters[0])
				{
					UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
				}
			}
			if (nameEvent.Equals(EventScreenDialogViewAddAIInteraction))
            {
				if (_meeting == (MeetingData)parameters[0])
                {
					UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationDestroyAllEvenIgnored);
					if (parameters.Length <= 1)
                    {
						if (!_meetingConcluded)
                        {
							buttonNext.interactable = true;
							buttonData.interactable = true;
							inputText.interactable = true;
							if (_meeting.IsInterruptionMeeting())
							{
								buttonData.interactable = false;
							}
						}
					}
					else
                    {
						InteractionData interaction = (InteractionData)parameters[1];
						SetInteraction(interaction);
						if (_meeting.HasPlayer(true))
						{
							if (!_meetingConcluded)
							{
								buttonNext.interactable = true;
								buttonData.interactable = true;
								inputText.interactable = true;
								if (_meeting.IsInterruptionMeeting())
								{
									buttonData.interactable = false;
								}
							}
						}
						else
						{
							if (!_meetingConcluded)
							{
								buttonNext.interactable = false;
								buttonData.interactable = false;
								inputText.interactable = false;
							}
						}
					}
				}
			}
			if (nameEvent.Equals(UploadImageDataHTTP.EventUploadImageDataHTTPCompleted))
			{
				if ((bool)parameters[0])
				{
					_imageToConfirm = ((int)parameters[1]).ToString();
				}
			}
		}
	}
}