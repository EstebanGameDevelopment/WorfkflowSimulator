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
	public class ScreenSlotsManagementView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenSlotsManagementView";

		public const string EventScreenSlotsManagementViewSelectedSlot = "EventScreenSlotsManagementViewSelectedSlot";

		[SerializeField] private GameObject UserSlotPrefab;
		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private TextMeshProUGUI descriptionScreen;
		[SerializeField] private SlotManagerView SlotManager;
		[SerializeField] private Button buttonSelect;
		[SerializeField] private Button buttonAdd;
		[SerializeField] private Button buttonClose;
		[SerializeField] private Sprite[] iconsPackage;

		private bool _isSelectionCreation = false;
		private List<ProjectEntryIndex> _projectsList;
		private ProjectSlot _selectedStorySlot = null;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);
			_isSelectionCreation = (bool)parameters[0];

			buttonClose.onClick.AddListener(OnButtonBack);
			buttonAdd.onClick.AddListener(OnButtonAdd);
			buttonSelect.onClick.AddListener(OnButtonSelect);

			buttonSelect.gameObject.SetActive(false);
			buttonAdd.gameObject.SetActive(false);

			titleScreen.text = LanguageController.Instance.GetText("screen.user.slots.title");

			if (_isSelectionCreation)
			{
				buttonSelect.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.user.slots.select.slot");
				descriptionScreen.text = LanguageController.Instance.GetText("screen.user.slots.assign.story.description");
				buttonSelect.gameObject.SetActive(true);
				buttonSelect.interactable = false;
			}
			else
			{
				buttonAdd.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.user.slots.add.slot");
				descriptionScreen.text = LanguageController.Instance.GetText("screen.user.slots.list");
				buttonAdd.gameObject.SetActive(true);
			}

			UIEventController.Instance.Event += OnUIEvent;
			SystemEventController.Instance.Event += OnSystemEvent;

			SystemEventController.Instance.DispatchSystemEvent(MenuStateEditOptions.EventGameStateEditProjectRequestProjectList);
		}

		public override void Destroy()
		{
			base.Destroy();

			_projectsList = null;
			_selectedStorySlot = null;
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
		}

		private void LoadUserSlotsList()
		{
			SlotManager.ClearCurrentGameObject(true);
			List<ItemMultiObjectEntry> itemsUserSlots = new List<ItemMultiObjectEntry>();
			for (int i = 0; i < WorkDayData.Instance.UserSlots.Count; i++)
			{
				itemsUserSlots.Add(new ItemMultiObjectEntry(this.gameObject, i, WorkDayData.Instance.UserSlots[i], _projectsList, iconsPackage));
			}
			SlotManager.Initialize(itemsUserSlots.Count, itemsUserSlots, UserSlotPrefab);
		}

		private void OnButtonBack()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnButtonSelect()
		{
			if (_selectedStorySlot != null)
			{				
				UIEventController.Instance.DispatchUIEvent(EventScreenSlotsManagementViewSelectedSlot, _selectedStorySlot);
				OnButtonBack();
			}
		}

		private void OnButtonAdd()
		{
			Content.gameObject.SetActive(false);
			ScreenController.Instance.CreateScreen(ScreenPurchaseOptionsView.ScreenName, false, false, -1, 0, yourvrexperience.Utils.Utilities.GetCurrentTimestamp());
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(MenuStateEditOptions.EventGameStateEditProjectResponseProjectsList))
			{
				Content.gameObject.SetActive(true);
				_projectsList = (List<ProjectEntryIndex>)parameters[0];
				LoadUserSlotsList();
			}
			if (nameEvent.Equals(CheckoutController.EventCheckoutControllerDownloadedSlotsConfirmation))
			{
				LoadUserSlotsList();
			}
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ItemUserSlot.EventItemUserSlotSelected))
			{
				if (_isSelectionCreation)
				{
					if ((int)parameters[2] == -1)
					{
						buttonSelect.interactable = false;
						_selectedStorySlot = null;
					}
					else
					{
						_selectedStorySlot = (ProjectSlot)parameters[3];
						if (_selectedStorySlot.Project == -1)
						{
							buttonSelect.interactable = true;
						}
						else
						{
							buttonSelect.interactable = false;
							_selectedStorySlot = null;
							ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.error"), LanguageController.Instance.GetText("screen.user.slots.not.free.slot"));
						}
					}
				}
			}
			if (nameEvent.Equals(ItemUserSlot.EventItemUserSlotToUpgrade))
			{
				Content.gameObject.SetActive(false);
				ProjectSlot upgradeStorySlot = (ProjectSlot)parameters[0];
				ScreenController.Instance.CreateScreen(ScreenPurchaseOptionsView.ScreenName, false, false, upgradeStorySlot.Id, upgradeStorySlot.Level, upgradeStorySlot.Timeout);
			}
		}
	}
}