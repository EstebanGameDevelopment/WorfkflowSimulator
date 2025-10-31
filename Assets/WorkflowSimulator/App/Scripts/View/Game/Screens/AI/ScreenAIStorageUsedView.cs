using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class ScreenAIStorageUsedView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenAIStorageUsedView";

		[SerializeField] private TextMeshProUGUI Title;
		[SerializeField] private TextMeshProUGUI StorageData;
		[SerializeField] private TextMeshProUGUI StorageImages;
		[SerializeField] private TextMeshProUGUI StorageTotal;

		[SerializeField] private TextMeshProUGUI LoadingMessage;

		[SerializeField] private Button CloseButton;
		[SerializeField] private SlotManagerView SlotManagerStorage;
		[SerializeField] private GameObject StoragePrefab;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			CloseButton.onClick.AddListener(OnClose);

			Title.gameObject.SetActive(false);
			StorageTotal.gameObject.SetActive(false);
			StorageData.gameObject.SetActive(false);
			StorageImages.gameObject.SetActive(false);
			SlotManagerStorage.gameObject.SetActive(false);

			LoadingMessage.gameObject.SetActive(true);
			LoadingMessage.text = LanguageController.Instance.GetText("screen.loading");

			SystemEventController.Instance.Event += OnSystemEvent;

			WorkDayData.Instance.GetStorageUsed(WorkDayData.Instance.CurrentIndexProject, WorkDayData.Instance.CurrentProject.GetLevel());
		}

		public override void Destroy()
		{
			base.Destroy();

			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
		}

		private void OnClose()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ConsultStorageUsedHTTP.EventConsultStorageUsedHTTPCompleted))
			{
				if ((bool)parameters[0])
				{
					LoadStorageUsed((StorageUsed)parameters[1]);
				}
			}
		}

		private void LoadStorageUsed(StorageUsed storage)
		{
			LoadingMessage.gameObject.SetActive(false);

			Title.text = LanguageController.Instance.GetText("screen.storage.used.title");

			Title.gameObject.SetActive(true);
			StorageData.gameObject.SetActive(true);
			StorageTotal.gameObject.SetActive(true);
			StorageTotal.text = LanguageController.Instance.GetText("screen.storage.used.storage.total") + " " + Math.Round((storage.PercentageTotal * 100), 2) + "%";
			StorageData.text = "*" + LanguageController.Instance.GetText("screen.storage.used.storage.data") + " " + Math.Round((storage.PercentageData * 100), 2) + "%";
			StorageData.color = Color.red;

			if (WorkDayData.Instance.CurrentProject.GetLevel() >= 1)
			{
				StorageImages.text = "*" + LanguageController.Instance.GetText("screen.storage.used.storage.images") + " " + Math.Round((storage.PercentageImages * 100), 2) + "%";
				StorageImages.color = Color.blue;
				StorageImages.gameObject.SetActive(true);
			}

			SlotManagerStorage.gameObject.SetActive(true);
			SlotManagerStorage.ClearCurrentGameObject(true);
			List<ItemMultiObjectEntry> itemsStorage = new List<ItemMultiObjectEntry>();
			itemsStorage.Add(new ItemMultiObjectEntry(this.gameObject, 0, storage));
			SlotManagerStorage.Initialize(itemsStorage.Count, itemsStorage, StoragePrefab);
		}
	}
}