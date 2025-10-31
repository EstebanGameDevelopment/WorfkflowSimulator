using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.RunStateRun;
using static yourvrexperience.WorkDay.ScreenCalendarView;

namespace yourvrexperience.WorkDay
{
	public class ScreenInfoItemView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenInfoItemView";
		
		public const string EventScreenInfoItemViewItemSelected = "EventScreenInfoItemViewItemSelected";
		public const string EventScreenInfoItemViewDestroy = "EventScreenInfoItemViewDestroy";
		public const string EventScreenInfoItemViewOnlyDestroyView = "EventScreenInfoItemViewOnlyDestroyView";
		public const string EventScreenInfoItemViewReportDestroyed = "EventScreenInfoItemViewReportDestroyed";
		public const string EventScreenInfoItemViewRequestExpandedInfo = "EventScreenInfoItemViewRequestExpandedInfo";
		public const string EventScreenInfoItemViewReportExpandedInfo = "EventScreenInfoItemViewReportExpandedInfo";
		public const string EventScreenInfoItemViewHumanPlayerAssigned = "EventScreenInfoItemViewHumanPlayerAssigned";

		public const string SubEventScreenInfoItemViewConfirmDeleteHuman = "SubEventScreenInfoItemViewConfirmDeleteHuman";

		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private TMP_InputField inputName;
		[SerializeField] private TMP_Dropdown dropOwner;
		[SerializeField] private TextMeshProUGUI nameArea;
		[SerializeField] private Image iconGroup;
		[SerializeField] private TMP_Dropdown groupMember;
		[SerializeField] private TextMeshProUGUI feedback;
		[SerializeField] private Button buttonClose;
		[SerializeField] private Button buttonMove;
		[SerializeField] private Button buttonRotate;
		[SerializeField] private Button buttonDelete;
		[SerializeField] private Button buttonExpand;
		
		[SerializeField] private GameObject foregroundBase;
		[SerializeField] private GameObject foregroundExpanded;
		[SerializeField] private Button buttonCollapse;
		[SerializeField] private TMP_InputField inputDescription;
		[SerializeField] private CustomToggle toggleHumanControlled;
		[SerializeField] private CustomToggle toggleIsClient;
		[SerializeField] private CustomToggle toggleIsLead;
		[SerializeField] private CustomToggle toggleIsSenior;
		[SerializeField] private CustomToggle toggleIsFastPaced;

		[SerializeField] private TextMeshProUGUI titleCheckProgress;
		[SerializeField] private Button buttonCheckProgress;

		public override string NameScreen
		{
			get { return ScreenName; }
		}

		private GameObject _itemGO;
		private WorldItemData _itemData;
		private AreaData _areaData;
		private AssetDefinitionItem _itemDefinition;
		private bool _isExpanded = false;

        public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);
			_itemGO = (GameObject)parameters[0];

			ToggleDisplayExtendedInfo(false);
			buttonExpand.gameObject.SetActive(false);

			if (parameters[1] is WorldItemData)
            {
				_itemData = (WorldItemData)parameters[1];
				_itemDefinition = AssetsCatalogData.Instance.GetAssetById(_itemData.CatalogId);
				titleScreen.text = _itemDefinition.Name;

				buttonClose.onClick.AddListener(OnCloseButton);
				
				buttonMove.onClick.AddListener(OnMoveButton);
				buttonRotate.onClick.AddListener(OnRotateButton);
				buttonDelete.onClick.AddListener(OnDeleteButton);

				titleCheckProgress.gameObject.SetActive(false);
				buttonCheckProgress.gameObject.SetActive(false);
                titleCheckProgress.text = LanguageController.Instance.GetText("screen.calendar.option.show.progress");

                buttonCheckProgress.onClick.AddListener(OnCheckProgress);

				if (_itemData.IsHuman)
				{
					dropOwner.gameObject.SetActive(false);
					nameArea.gameObject.SetActive(false);
					groupMember.gameObject.SetActive(true);
					iconGroup.gameObject.SetActive(true);

					buttonExpand.gameObject.SetActive(true);
					buttonExpand.onClick.AddListener(OnExpandPanel);

					inputDescription.text = _itemData.Data;
					buttonCollapse.onClick.AddListener(OnCollapsePanel);
					inputDescription.onValueChanged.AddListener(OnDescriptionHumanChanged);

					toggleHumanControlled.isOn = _itemData.IsPlayer;
					toggleIsClient.isOn = _itemData.IsClient;
					toggleIsLead.isOn = _itemData.IsLead;
					toggleIsSenior.isOn = _itemData.IsSenior;
					toggleIsFastPaced.isOn = _itemData.IsAsshole;

					toggleHumanControlled.onValueChanged.AddListener(OnControlHuman);
					toggleIsClient.onValueChanged.AddListener(OnIsClient);
					toggleIsLead.onValueChanged.AddListener(OnIsLead);
					toggleIsSenior.onValueChanged.AddListener(OnIsSenior);
					toggleIsFastPaced.onValueChanged.AddListener(OnIsAsshole);

					toggleHumanControlled.PointerEnterButton += OnHumanControlledEnter;
					toggleIsClient.PointerEnterButton += OnIsClientEnter;
					toggleIsLead.PointerEnterButton += OnIsLeadEnter;
					toggleIsSenior.PointerEnterButton += OnIsSeniorEnter;
					toggleIsFastPaced.PointerEnterButton += OnIsAssholeEnter;

					toggleHumanControlled.PointerExitButton += OnResetFeedback;
					toggleIsClient.PointerExitButton += OnResetFeedback;
					toggleIsLead.PointerExitButton += OnResetFeedback;
					toggleIsSenior.PointerExitButton += OnResetFeedback;
					toggleIsFastPaced.PointerExitButton += OnResetFeedback;

					toggleHumanControlled.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.info.title.is.human.player");
					toggleIsClient.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.info.title.is.client");
					toggleIsLead.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.info.title.is.lead");
					toggleIsSenior.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.info.title.is.senior");
					toggleIsFastPaced.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.info.title.is.fastpaced");

					groupMember.options.Clear();
					groupMember.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("text.no.group.assigned")));
					int idGroupFound = -1;
					List<GroupInfoData> groups = WorkDayData.Instance.CurrentProject.GetGroups();
					GroupInfoData groupOfHuman = WorkDayData.Instance.CurrentProject.GetGroupOfMember(_itemData.Name);
					if (groups.Count == 0)
                    {
						groupMember.value = 0;
						groupMember.interactable = false;
					}
					else
                    {						
						int counter = 0;
						for (int i = 0; i < groups.Count; i++)
						{
							GroupInfoData group = groups[i];
							if (group != null)
                            {
								groupMember.options.Add(new TMP_Dropdown.OptionData(group.Name));
								if (groupOfHuman != null)
								{
									if (groupOfHuman.Name.Equals(group.Name))
									{
										idGroupFound = counter;
									}
								}
								counter++;
							}
						}
					}
					if (idGroupFound != -1)
                    {
						groupMember.value = idGroupFound + 1;
						iconGroup.color = WorkDayData.Instance.CurrentProject.GetColorForMember(_itemData.Name);
					}
					else
                    {
						groupMember.value = 0;
						iconGroup.color = Color.gray;
					}
					groupMember.onValueChanged.AddListener(OnGroupChanged);

					inputName.text = _itemData.Name;
					inputName.onEndEdit.AddListener(OnNameChanged);
				}
				else
				if (_itemData.IsChair)
				{
					dropOwner.gameObject.SetActive(true);
					inputName.gameObject.SetActive(false);
					groupMember.gameObject.SetActive(false);
					iconGroup.gameObject.SetActive(false);

					List<WorldItemData> humans = ApplicationController.Instance.LevelView.GetHumans();
					dropOwner.options.Clear();
					dropOwner.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("screen.calendar." + WorkDayData.RESERVED_NAME_NOBODY)));
					int idFound = 0;
					for (int i = 0; i < humans.Count; i++)
					{
						WorldItemData human = humans[i];
						if (_itemData.Owner != null)
						{
							if (human.Name.ToLower().Equals(_itemData.Owner.ToLower()))
							{
								idFound = i + 1;
							}
						}

						dropOwner.options.Add(new TMP_Dropdown.OptionData(human.Name));
					}

					dropOwner.value = idFound;
					dropOwner.onValueChanged.AddListener(OnOwnerAssigned);

					nameArea.gameObject.SetActive(true);
					if (_itemGO.GetComponent<ChairView>() == null)
                    {
						nameArea.text = LanguageController.Instance.GetText("screen.info.no.chair.assigned");
					}
					else
                    {
						if (_itemGO.GetComponent<ChairView>().Area == null)
                        {
                            nameArea.text = LanguageController.Instance.GetText("screen.info.no.area.assigned");
                        }
						else
                        {
							if (_itemGO.GetComponent<ChairView>().Area.Name == null)
                            {
                                nameArea.text = LanguageController.Instance.GetText("screen.info.no.area.assigned");
                            }
							else
                            {
								nameArea.text = _itemGO.GetComponent<ChairView>().Area.Name;
							}
						}
					}
				}
				else
				{
					nameArea.gameObject.SetActive(false);
					dropOwner.gameObject.SetActive(false);
					inputName.gameObject.SetActive(false);
					groupMember.gameObject.SetActive(false);
					iconGroup.gameObject.SetActive(false);
				}

				UIEventController.Instance.Event += OnUIEvent;
				SystemEventController.Instance.Event += OnSystemEvent;

				SystemEventController.Instance.DispatchSystemEvent(EventScreenInfoItemViewItemSelected);
			}
			if (parameters[1] is AreaData)
			{
				_areaData = (AreaData)parameters[1];

				titleScreen.text = LanguageController.Instance.GetText("text.area");

				buttonClose.onClick.AddListener(OnCloseButton);

				dropOwner.gameObject.SetActive(false);
				buttonMove.gameObject.SetActive(false);
				buttonRotate.gameObject.SetActive(false);
				buttonDelete.gameObject.SetActive(false);
				groupMember.gameObject.SetActive(false);
				iconGroup.gameObject.SetActive(false);

				inputName.text = _areaData.Name;
				inputName.onEndEdit.AddListener(OnNameChanged);

				UIEventController.Instance.Event += OnUIEvent;
				SystemEventController.Instance.Event += OnSystemEvent;
			}
			ApplicationController.Instance.IsInfoScreenDisplayed = true;

			RefreshEditionVisibility(ApplicationController.Instance.LevelView.EditionMode);

			if (ApplicationController.Instance.IsPlayMode)
            {
				inputName.interactable = false;
				groupMember.interactable = false;
				inputDescription.interactable = false;
				toggleHumanControlled.interactable = false;
				toggleIsClient.interactable = false;
				toggleIsLead.interactable = false;
				toggleIsSenior.interactable = false;
				toggleIsFastPaced.interactable = false;
			}
		}

        public override void Destroy()
		{
			base.Destroy();
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;

			_itemGO = null;
			_itemData = null;
			_itemDefinition = null;
			ApplicationController.Instance.IsInfoScreenDisplayed = false;
		}

		private void OnGroupChanged(int value)
		{
			if (value == 0)
            {
				if (_itemData.IsHuman)
				{
					GroupInfoData groupMember = WorkDayData.Instance.CurrentProject.GetGroupOfMember(_itemData.Name);
					UIEventController.Instance.DispatchUIEvent(RunStateRun.EventRunStateRunUnAssignHumanToGroup, _itemData.Name, groupMember);
					iconGroup.color = Color.white;
				}
			}
			else
            {
				string groupName = ((TMP_Dropdown.OptionData)groupMember.options[value]).text;
				GroupInfoData groupSelected = WorkDayData.Instance.CurrentProject.GetGroupByName(groupName);
				if (groupSelected != null)
				{
					if (_itemData.IsHuman)
					{
						UIEventController.Instance.DispatchUIEvent(RunStateRun.EventRunStateRunAssignHumanToGroup, _itemData.Name, groupSelected);
						iconGroup.color = groupSelected.GetColor();
					}
				}
			}
		}

		private void OnResetFeedback(CustomToggle value)
		{
			feedback.text = "";
		}

		private void OnIsAssholeEnter(CustomToggle value)
		{
			feedback.text = LanguageController.Instance.GetText("screen.info.feedback.is.fastpaced");
		}

		private void OnIsSeniorEnter(CustomToggle value)
		{
			feedback.text = LanguageController.Instance.GetText("screen.info.feedback.is.senior");
		}

		private void OnIsLeadEnter(CustomToggle value)
		{
			feedback.text = LanguageController.Instance.GetText("screen.info.feedback.is.lead");
		}

		private void OnIsClientEnter(CustomToggle value)
		{
			feedback.text = LanguageController.Instance.GetText("screen.info.feedback.is.client");
		}

		private void OnHumanControlledEnter(CustomToggle value)
		{
			feedback.text = LanguageController.Instance.GetText("screen.info.feedback.is.human.player");
		}

		private void RefreshEditionVisibility(bool visibility)
        {
			buttonMove.gameObject.SetActive(visibility);
			buttonRotate.gameObject.SetActive(visibility);
			buttonDelete.gameObject.SetActive(visibility);
		}

		private void OnCheckProgress()
		{
			ScreenController.Instance.CreateScreen(ScreenCalendarView.ScreenName, false, false, CalendarOption.NORMAL, false, true);
		}

		private void ToggleDisplayExtendedInfo(bool enabled)
        {
			_isExpanded = enabled;

			buttonClose.gameObject.SetActive(!enabled);
			buttonMove.gameObject.SetActive(!enabled);
			buttonRotate.gameObject.SetActive(!enabled);
			buttonDelete.gameObject.SetActive(!enabled);
			buttonExpand.gameObject.SetActive(!enabled);

			foregroundBase.gameObject.SetActive(!enabled);
			foregroundExpanded.gameObject.SetActive(enabled);
			buttonCollapse.gameObject.SetActive(enabled);
			inputDescription.gameObject.SetActive(enabled);
			toggleHumanControlled.gameObject.SetActive(enabled);
			toggleIsClient.gameObject.SetActive(enabled);
			toggleIsLead.gameObject.SetActive(enabled);
			toggleIsSenior.gameObject.SetActive(enabled);
			toggleIsFastPaced.gameObject.SetActive(enabled);

			RefreshEditionVisibility(ApplicationController.Instance.LevelView.EditionMode);

			if (enabled)
            {
				if (_itemData.IsHuman)
                {
					titleCheckProgress.gameObject.SetActive(true);
					buttonCheckProgress.gameObject.SetActive(true);
				}
				else
                {
					titleCheckProgress.gameObject.SetActive(false);
					buttonCheckProgress.gameObject.SetActive(false);
				}
			}
			else
            {
				titleCheckProgress.gameObject.SetActive(false);
				buttonCheckProgress.gameObject.SetActive(false);
			}
		}

		private void OnControlHuman(bool value)
		{
			WorkDayData.Instance.CurrentProject.SetHumanControlled(_itemData, value);
			ApplicationController.Instance.HumanPlayer = null;
			SystemEventController.Instance.DispatchSystemEvent(EventScreenInfoItemViewHumanPlayerAssigned);
			if (value)
            {
				if (toggleIsClient.isOn)
                {
					toggleIsClient.isOn = false;
				}				
			}
		}

		private void OnIsClient(bool value)
        {
			if (value)
            {
				if (toggleHumanControlled.isOn)
                {
					toggleHumanControlled.isOn = false;
				}				
			}
			_itemData.IsClient = value;
		}

		private void OnIsLead(bool value)
		{
			_itemData.IsLead = value;
		}

		private void OnIsSenior(bool value)
        {
			_itemData.IsSenior = value;
		}

		private void OnIsAsshole(bool value)
		{
			_itemData.IsAsshole = value;
		}

		private void OnDescriptionHumanChanged(string value)
		{
			_itemData.Data = value;
		}

		private void OnCollapsePanel()
		{
			UIEventController.Instance.DispatchUIEvent(EventScreenInfoItemViewReportExpandedInfo, false);
			ToggleDisplayExtendedInfo(false);
		}

		private void OnExpandPanel()
		{
			UIEventController.Instance.DispatchUIEvent(EventScreenInfoItemViewReportExpandedInfo, true);
			ToggleDisplayExtendedInfo(true);
		}

		private void OnCloseButton()
		{			
			SystemEventController.Instance.DispatchSystemEvent(EventScreenInfoItemViewReportDestroyed);
			SystemEventController.Instance.DispatchSystemEvent(EditionSubStateIdle.EventSubStateIdleCancelCurrentSelection);
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnDeleteButton()
		{
			if (_itemData != null)
            {
				if (_itemData.IsHuman)
                {
					ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.info.item.confirm.delete.human"), SubEventScreenInfoItemViewConfirmDeleteHuman);
					return;
                }
			}
			ApplicationController.Instance.LevelView.DeleteItem(_itemGO, true);
			OnCloseButton();
		}

		private void OnRotateButton()
		{
			ApplicationController.Instance.LevelView.RotateItem(_itemGO);
		}

		private void OnMoveButton()
		{
			WorldItemData cloned = _itemData.Clone();
			AssetDefinitionItem itemDefinition = _itemDefinition;
			ApplicationController.Instance.LevelView.DeleteItem(_itemGO, false);
			OnCloseButton();
			SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDSelectionObject);
			SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunChangeState, SubStateRun.Move, itemDefinition, cloned);
		}

		private void OnOwnerAssigned(int value)
		{
			_itemData.Owner = ((TMP_Dropdown.OptionData)dropOwner.options[value]).text;
		}

		private void OnNameChanged(string value)
		{
			if (_itemData != null)
            {
				var (itemGO, itemActor) = ApplicationController.Instance.LevelView.GetItemByName(value);
				GroupInfoData groupNamed = WorkDayData.Instance.CurrentProject.GetGroupByName(value);
				if ((itemActor == null) && (groupNamed == null))
				{
					if (WorkDayData.Instance.IsReservedWord(value))
					{
						inputName.text = _itemData.Name;
					}
					else
					{
						string previousName = _itemData.Name;
						_itemData.Name = value;
						WorkDayData.Instance.CurrentProject.ReplaceHumanInSystem(previousName, _itemData.Name);
						ApplicationController.Instance.LevelView.ReplaceOwner(previousName, _itemData.Name);
						_itemGO.GetComponent<HumanView>().NameHuman = _itemData.Name;
					}
				}
				else
				{
					inputName.text = _itemData.Name;
				}
			}
			else
            {
				if (_areaData != null)
                {
					AreaData areaFound = ApplicationController.Instance.LevelView.GetAreaByName(value);
					if (areaFound == null)
                    {
						_areaData.Name = value;
						SystemEventController.Instance.DispatchSystemEvent(ChairView.EventChairViewAssignAreaData);
					}
					else
                    {
						inputName.text = _areaData.Name;
					}
				}
            }
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(SubEventScreenInfoItemViewConfirmDeleteHuman))
			{
				ScreenInformationResponses userResponse = (ScreenInformationResponses)parameters[1];
				if (userResponse == ScreenInformationResponses.Confirm)
				{
					ApplicationController.Instance.LevelView.DeleteItem(_itemGO, true);
					OnCloseButton();
				}
			}
			if (nameEvent.Equals(TabEditionBaseView.EventTabEditionBaseViewActivation))
			{
				OnCloseButton();
			}
			if (nameEvent.Equals(EventScreenInfoItemViewRequestExpandedInfo))
            {
				OnExpandPanel();
			}
			if (nameEvent.Equals(PanelProgressEvents.EventPanelProgressEventsExpanded))
			{
                if (_isExpanded)
				{
					OnCollapsePanel();
				}
            }
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ScreenPanelEditionView.EventScreenPanelEditionActivation))
			{
				RefreshEditionVisibility((bool)parameters[0]);
			}
			if (nameEvent.Equals(ScreenDialogView.EventScreenDialogViewStarted))
			{
				OnCloseButton();
			}
			if (nameEvent.Equals(EventScreenInfoItemViewOnlyDestroyView))
            {
				UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
			}
			if (nameEvent.Equals(EventScreenInfoItemViewDestroy))
            {				
				OnCloseButton();
			}
		}
	}
}