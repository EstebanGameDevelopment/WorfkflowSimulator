using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.ScreenListEventsHUDView;

namespace yourvrexperience.WorkDay
{
	public class ScreenGroupsView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenGroupsView";
		
		public const string EventScreenGroupsViewRefresh = "EventScreenGroupsViewRefresh";

		public const string SubEventScreenGroupsViewDeleteConfirmation = "SubEventScreenGroupsViewDeleteConfirmation";

		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private TextMeshProUGUI titleName;
		[SerializeField] private TextMeshProUGUI titleFeedback;

		[SerializeField] private CustomInput inputNameGroup;

		[SerializeField] private CustomButton colorButton;
		[SerializeField] private CustomButton buttonAdd;
		[SerializeField] private CustomButton buttonNew;
		[SerializeField] private CustomButton buttonPeople;
		[SerializeField] private Button buttonClose;

		[SerializeField] private SlotManagerView SlotManagerGroups;
		[SerializeField] private GameObject PrefabGroup;
		[SerializeField] private IconColorView IconColorGroup;

		private GroupInfoData _currentGroup;
		private List<GroupInfoData> _groups;
		private string _initialGroupName;
		private GroupInfoData _groupToDelete;
		private Color _colorSelected = Color.black;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			buttonClose.onClick.AddListener(OnClose);

			buttonAdd.onClick.AddListener(OnAddGroup);
			buttonNew.onClick.AddListener(OnNewGroup);
			buttonPeople.onClick.AddListener(OnShowEmployees);
			colorButton.onClick.AddListener(OnColorGroup);

			colorButton.PointerEnterButton += OnColorButtonEnter;
			colorButton.PointerExitButton += OnFeedbackReset;

			buttonAdd.PointerEnterButton += OnAddGroupEnter;
			buttonAdd.PointerExitButton += OnFeedbackReset;

			buttonNew.PointerEnterButton += OnNewClearEnter;
			buttonNew.PointerExitButton += OnFeedbackReset;

			buttonPeople.PointerEnterButton += OnShowPeopleEnter;
			buttonPeople.PointerExitButton += OnFeedbackReset;

			titleFeedback.text = "";

			inputNameGroup.text = "";
			buttonAdd.interactable = false;

			inputNameGroup.onValueChanged.AddListener(OnNameGroupChanged);

			UIEventController.Instance.Event += OnUIEvent;
			SystemEventController.Instance.Event += OnSystemEvent;
			
			LoadGroupsInfo(true);

			if (ApplicationController.Instance.IsPlayMode)
            {
				inputNameGroup.interactable = false;
				colorButton.interactable = false;
				buttonAdd.interactable = false;
				buttonNew.interactable = false;
			}

			titleScreen.text = LanguageController.Instance.GetText("text.groups");
            titleName.text = LanguageController.Instance.GetText("text.name");
        }

        public override void Destroy()
		{
			base.Destroy();

			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			_currentGroup = null;
			if (_groups != null)
			{
				_groups.Clear();
			}
			_groups = null;
			_groupToDelete = null;
		}

		private void OnClose()
		{
			SystemEventController.Instance.DispatchSystemEvent(LevelView.EventLevelViewLinesRequestDestroy);
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnColorButtonEnter(CustomButton value)
		{
			titleFeedback.text = LanguageController.Instance.GetText("screen.group.set.color.group");
		}

		private void OnNewClearEnter(CustomButton value)
		{
			titleFeedback.text = LanguageController.Instance.GetText("screen.group.clear.data");
		}

		private void OnAddGroupEnter(CustomButton value)
		{
			titleFeedback.text = LanguageController.Instance.GetText("screen.group.save.group");
		}

		private void OnShowPeopleEnter(CustomButton value)
		{
			titleFeedback.text = LanguageController.Instance.GetText("screen.group.show.employees");
        }

		private void OnFeedbackReset(CustomButton value)
		{
			titleFeedback.text = "";
		}

		private void OnColorGroup()
		{
			ScreenController.Instance.CreateScreen(ScreenColorPickerView.ScreenName, false, false);
		}

		private void OnNameGroupChanged(string value)
		{
			GroupInfoData tmpGroup = new GroupInfoData(value, "", _colorSelected);
			if (_currentGroup == null)
			{
				buttonAdd.interactable = !_groups.Contains(tmpGroup);
			}
			else
			{				
				if (!_groups.Contains(tmpGroup)
					&& !ApplicationController.Instance.LevelView.CheckNameBelongToHuman(value)
					&& !WorkDayData.Instance.IsReservedWord(value))
                {
					_currentGroup.Name = value;
					IconColorGroup.ApplyInfo(_currentGroup.Name, _colorSelected);
					UIEventController.Instance.DispatchUIEvent(ItemGroupInfoView.EventItemGroupInfoViewUpdateName, _currentGroup);
				}
			}
			UpdateAddState();
		}

		private void LoadGroupsInfo(bool selectItem)
		{
			_groups = WorkDayData.Instance.CurrentProject.GetGroups();
			SlotManagerGroups.ClearCurrentGameObject(true);
			SlotManagerGroups.Initialize(0, new List<ItemMultiObjectEntry>(), PrefabGroup);

			for (int i = 0; i < _groups.Count; i++)
			{
				SlotManagerGroups.AddItem(new ItemMultiObjectEntry(SlotManagerGroups.gameObject, SlotManagerGroups.Data.Count, _groups[i]));
			}

			if (selectItem)
            {
				if (_groups.Count > 0)
                {
					UIEventController.Instance.DispatchUIEvent(ItemProjectInfoView.EventItemProjectInfoViewForceSelection, WorkDayData.Instance.CurrentProject.ProjectInfoSelected);
				}				
            }
		}

		private void OnShowEmployees()
		{
			GameObject screenGO = ScreenController.Instance.CreateScreen(ScreenListEventsHUDView.ScreenName, false, true, TypeLateralInfo.PERSONS);
			UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, screenGO, _canvas.sortingOrder + 1);
		}

		private void OnAddGroup()
		{
			if (_currentGroup == null)
            {
				if (inputNameGroup.text.Length > 0)
                {					
					string nameGroup = inputNameGroup.text;					
					string descriptionGroup = "";
					GroupInfoData newGroup = new GroupInfoData(nameGroup, descriptionGroup, _colorSelected);

					if (!_groups.Contains(newGroup) 
						&& !ApplicationController.Instance.LevelView.CheckNameBelongToHuman(nameGroup) 
						&& !WorkDayData.Instance.IsReservedWord(nameGroup))
                    {
						SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunAddGroup, nameGroup, descriptionGroup, _colorSelected);
						OnNewGroup();
					}
				}
			}
		}

        private void UpdateAddState()
        {
			if (_currentGroup != null)
			{
				buttonAdd.interactable = false;
			}
			else
			{
				if (inputNameGroup.text.Length > 0)
				{
					string nameGroup = inputNameGroup.text;
					string descriptionGroup = "";
					GroupInfoData newGroup = new GroupInfoData(nameGroup, descriptionGroup, _colorSelected);
					buttonAdd.interactable = !_groups.Contains(newGroup) && !ApplicationController.Instance.LevelView.CheckNameBelongToHuman(nameGroup);
				}
			}
		}

        private void OnNewGroup()
		{
			_currentGroup = null;
			inputNameGroup.text = "";
			buttonAdd.interactable = false;
			_colorSelected = Color.white;
			IconColorGroup.ApplyInfo(" ", _colorSelected);
			UIEventController.Instance.DispatchUIEvent(ItemGroupInfoView.EventItemGroupInfoViewUnselectAll);
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ScreenListEventsHUDView.EventScreenListEventsHUDViewDestroy))
			{
				UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
			}
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ScreenCalendarView.EventScreenCalendarViewOpened))
			{
				OnClose();
            }
			if (nameEvent.Equals(ScreenColorPickerView.EventScreenColorPickerViewColorSelected))
            {
				_colorSelected = (Color)parameters[0];
				IconColorGroup.ApplyInfo(inputNameGroup.text, _colorSelected);
				if (_currentGroup != null)
				{
					_currentGroup.SetColor(_colorSelected);
					IconColorGroup.Refresh();
					UIEventController.Instance.DispatchUIEvent(ItemGroupInfoView.EventItemGroupInfoViewRefreshColor, _currentGroup);
				}
			}
			if (nameEvent.Equals(EventScreenGroupsViewRefresh))
            {
				LoadGroupsInfo(false);
				OnNewGroup();
				UpdateAddState();
			}
			if (nameEvent.Equals(ItemGroupInfoView.EventItemGroupInfoViewEdit))
            {
				GroupInfoData groupToEdit = (GroupInfoData)parameters[2];
				ScreenController.Instance.CreateScreen(ScreenGroupMembersView.ScreenName, false, true, groupToEdit);
			}
			if (nameEvent.Equals(ItemGroupInfoView.EventItemGroupInfoViewDelete))
			{
				_groupToDelete = (GroupInfoData)parameters[2];
				string titleWarning = LanguageController.Instance.GetText("text.warning");
				string textAskToExit = LanguageController.Instance.GetText("screen.main.do.you.want.to.delete.group");
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, titleWarning, textAskToExit, SubEventScreenGroupsViewDeleteConfirmation);
			}
			if (nameEvent.Equals(SubEventScreenGroupsViewDeleteConfirmation))
			{
				ScreenInformationResponses userResponse = (ScreenInformationResponses)parameters[1];
				if (userResponse == ScreenInformationResponses.Confirm)
				{
					SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunDeleteGroup, _groupToDelete);
				}
			}
			if (nameEvent.Equals(ItemGroupInfoView.EventItemGroupInfoViewSelected))
			{
				if ((int)parameters[2] == -1)
				{
					OnNewGroup();
				}
				else
				{
					_currentGroup = (GroupInfoData)parameters[3];
					_colorSelected = _currentGroup.GetColor();
					IconColorGroup.ApplyInfo(_currentGroup.Name, _colorSelected);
					inputNameGroup.text = _currentGroup.Name;
					_initialGroupName = _currentGroup.Name;
				}
			}
		}
	}
}