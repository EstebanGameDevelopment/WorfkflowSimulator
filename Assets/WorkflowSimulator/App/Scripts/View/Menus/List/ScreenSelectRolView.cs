using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
#if ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR || ENABLE_NREAL
using yourvrexperience.VR;
#endif

namespace yourvrexperience.WorkDay
{
	public class ScreenSelectRolView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenSelectRolView";

		public const string EventScreenSelectRolViewSelectedProfile = "EventScreenSelectRolViewSelectedProfile";

		[SerializeField] private GameObject EmployeeNamePrefab;
		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private TextMeshProUGUI descriptionScreen;
		[SerializeField] private SlotManagerView SlotManagerEmployees;
		[SerializeField] private Button buttonSelect;
		[SerializeField] private Button buttonClose;

		private WorldItemData _selectedRole;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);
			
			buttonClose.onClick.AddListener(OnButtonBack);
			buttonSelect.onClick.AddListener(OnButtonSelectRole);

			titleScreen.text = LanguageController.Instance.GetText("screen.select.rol.title");
			descriptionScreen.text = LanguageController.Instance.GetText("screen.select.rol.please.select.profile");

			buttonSelect.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.select.rol.confirmation");
			buttonSelect.interactable = false;

			UIEventController.Instance.Event += OnUIEvent;

			List<WorldItemData> humans = WorkDayData.Instance.CurrentProject.GetHumans();
			LoadEmployees(humans);
		}

		public override void Destroy()
		{
			base.Destroy();

			_selectedRole = null;

			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
		}

		private void OnButtonBack()
		{			
			UIEventController.Instance.DispatchUIEvent(EventScreenSelectRolViewSelectedProfile, false);
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnButtonSelectRole()
		{
			if (_selectedRole != null)
            {
				UIEventController.Instance.DispatchUIEvent(EventScreenSelectRolViewSelectedProfile, true, _selectedRole);
				UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
			}
		}

		private void LoadEmployees(List<WorldItemData> employees)
        {
			SlotManagerEmployees.ClearCurrentGameObject(true);
			List<ItemMultiObjectEntry> itemsEmployees = new List<ItemMultiObjectEntry>();
			if (employees != null)
            {
				for (int i = 0; i < employees.Count; i++)
				{
					itemsEmployees.Add(new ItemMultiObjectEntry(this.gameObject, i, employees[i]));
				}
				SlotManagerEmployees.Initialize(itemsEmployees.Count, itemsEmployees, EmployeeNamePrefab);
			}
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ItemRoleEmployee.EventItemRoleEmployeeSelected))
			{
				if ((GameObject)parameters[0] == this.gameObject)
				{
					if ((int)parameters[2] == -1)
					{
						descriptionScreen.text = "";
						_selectedRole = null;
						buttonSelect.interactable = false;
					}
					else
					{
						
						_selectedRole = (WorldItemData)parameters[3];
						descriptionScreen.text = "<b>" + LanguageController.Instance.GetText("text.name") + "(" + _selectedRole.Name + "):</b>\n" + _selectedRole.Data;
						GroupInfoData groupData = WorkDayData.Instance.CurrentProject.GetGroupOfMember(_selectedRole.Name);
						if (groupData != null)
                        {
							descriptionScreen.text += "\n\n<b>" + LanguageController.Instance.GetText("text.group") + "(" + groupData.Name + "):</b>\n" + groupData.Description;
						}
						buttonSelect.interactable = true;
					}
				}
			}
		}
	}
}