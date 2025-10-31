using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class ScreenGroupMembersView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenGroupMembersView";

		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private TextMeshProUGUI titleDescription;
		[SerializeField] private CustomInput inputDescriptionGroup;

		[SerializeField] private Button buttonAssign;
		[SerializeField] private Button buttonUnAssign;
		
		[SerializeField] private SlotManagerView SlotManagerHumans;
		[SerializeField] private SlotManagerView SlotManagerMembers;
		[SerializeField] private GameObject PrefabHuman;

		[SerializeField] private Button buttonClose;

		private GroupInfoData _selectedGroup;

		private List<string> _humanNames;
		private List<string> _membersNames;

		private string _humanNameToAssign = "";
		private string _humanNameToUnAssign = "";
		
		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			_selectedGroup = (GroupInfoData)parameters[0];

			buttonClose.onClick.AddListener(OnClose);

			buttonAssign.onClick.AddListener(OnAssignMember);
			buttonUnAssign.onClick.AddListener(OnUnAssignMember);
			buttonAssign.interactable = false;
			buttonUnAssign.interactable = false;

			LoadHumanNames();
			LoadGroupMembers();

			inputDescriptionGroup.text = _selectedGroup.Description;
			inputDescriptionGroup.onValueChanged.AddListener(OnDescriptionGroupChanged);

			titleScreen.text = LanguageController.Instance.GetText("text.groups");
            titleDescription.text = LanguageController.Instance.GetText("text.description.groups");

            if (ApplicationController.Instance.IsPlayMode)
			{
				inputDescriptionGroup.interactable = false;
			}

			UIEventController.Instance.Event += OnUIEvent;
			SystemEventController.Instance.Event += OnSystemEvent;
		}

		public override void Destroy()
		{
			base.Destroy();
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;

			_selectedGroup = null;
		}

		private void OnDescriptionGroupChanged(string value)
		{
			if (_selectedGroup != null)
			{
				_selectedGroup.Description = value;
			}
		}

		private void LoadHumanNames()
        {
			_humanNames = new List<string>();
			List<WorldItemData> humans = ApplicationController.Instance.LevelView.GetHumans();
			foreach (WorldItemData human in humans)
            {
				_humanNames.Add(human.Name);
			}
			LoadHumans(SlotManagerHumans, _humanNames);			
		}

		private void LoadGroupMembers()
		{
			_membersNames = _selectedGroup.GetMembers();
			LoadHumans(SlotManagerMembers, _membersNames);
		}

		private void OnClose()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void LoadHumans(SlotManagerView slotManager, List<string> humans)
		{
			slotManager.ClearCurrentGameObject(true);
			slotManager.Initialize(0, new List<ItemMultiObjectEntry>(), PrefabHuman);

			for (int i = 0; i < humans.Count; i++)
			{
				slotManager.AddItem(new ItemMultiObjectEntry(slotManager.gameObject, slotManager.Data.Count, humans[i]));
			}
		}

		private void OnAssignMember()
		{
			if ((_humanNameToAssign.Length > 0) && (_selectedGroup != null))
			{
				string humanNameToAssign = _humanNameToAssign;
				_humanNameToAssign = "";
				UIEventController.Instance.DispatchUIEvent(RunStateRun.EventRunStateRunAssignHumanToGroup, humanNameToAssign, _selectedGroup);
			}
		}

		private void OnUnAssignMember()
        {
			if ((_humanNameToUnAssign.Length > 0) && (_selectedGroup != null))
			{
				string humanNameToUnAssign = _humanNameToUnAssign;
				_humanNameToUnAssign = "";
				UIEventController.Instance.DispatchUIEvent(RunStateRun.EventRunStateRunUnAssignHumanToGroup, humanNameToUnAssign, _selectedGroup);
			}
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(HumanView.EventHumanViewGroupUpdated))
			{
				LoadGroupMembers();
				SlotManagerHumans.ApplyGenericAction();
			}
		}


		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ItemHumanView.EventItemHumanViewSelected))
            {
				if ((GameObject)parameters[0] == SlotManagerHumans.gameObject)
                {
					if ((int)parameters[2] == -1)
                    {
						_humanNameToAssign = "";
						buttonAssign.interactable = false;
					}
					else
                    {
						_humanNameToAssign = (string)parameters[3];
						buttonAssign.interactable = true;
						buttonUnAssign.interactable = false;
						if (ApplicationController.Instance.IsPlayMode)
                        {
							buttonAssign.interactable = false;
							buttonUnAssign.interactable = false;
						}
						UIEventController.Instance.DispatchUIEvent(ItemHumanView.EventItemHumanViewForceUnSelect, SlotManagerMembers.gameObject);
					}
                }
				if ((GameObject)parameters[0] == SlotManagerMembers.gameObject)
				{
					if ((int)parameters[2] == -1)
					{
						_humanNameToUnAssign = "";
						buttonUnAssign.interactable = false;
					}
					else
					{
						_humanNameToUnAssign = (string)parameters[3];
						buttonUnAssign.interactable = true;
						buttonAssign.interactable = false;
						if (ApplicationController.Instance.IsPlayMode)
						{
							buttonAssign.interactable = false;
							buttonUnAssign.interactable = false;
						}
						UIEventController.Instance.DispatchUIEvent(ItemHumanView.EventItemHumanViewForceUnSelect, SlotManagerHumans.gameObject);
					}
				}
			}
		}
	}
}