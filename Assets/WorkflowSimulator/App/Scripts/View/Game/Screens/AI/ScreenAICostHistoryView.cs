using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class ScreenAICostHistoryView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenAICostHistoryView";
		
		[SerializeField] private TextMeshProUGUI titleTotalCost;
		[SerializeField] private TextMeshProUGUI valueTotalCost;

		[SerializeField] private Button CloseButton;
		[SerializeField] private Button ClearButton;
		[SerializeField] private SlotManagerView SlotManagerImages;
		[SerializeField] private GameObject CostPrefab;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			CloseButton.onClick.AddListener(OnClose);
			ClearButton.onClick.AddListener(OnClearHistory);

			titleTotalCost.text = LanguageController.Instance.GetText("text.total.cost");
            valueTotalCost.text = "$" + WorkDayData.Instance.CurrentProject.GetTotalCost();
			LoadCostHistory();

			UIEventController.Instance.Event += OnUIEvent;
		}

		public override void Destroy()
		{
			base.Destroy();

			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
		}

		private void OnClose()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnClearHistory()
		{
			WorkDayData.Instance.CurrentProject.ClearCost();
			LoadCostHistory();
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
		}

		private void LoadCostHistory()
		{
			SlotManagerImages.ClearCurrentGameObject(true);
			List<ItemMultiObjectEntry> itemsImages = new List<ItemMultiObjectEntry>();
			if (WorkDayData.Instance.CurrentProject.GetTotalCost() > 0)
			{
				int startIndex = 0;
				if (WorkDayData.Instance.CurrentProject.Cost.Length > 200)
                {
					startIndex = WorkDayData.Instance.CurrentProject.Cost.Length - 200;
				}
				int totalLength = WorkDayData.Instance.CurrentProject.Cost.Length;
				if (totalLength > 200)
                {
					totalLength = 200;
				}
				for (int i = startIndex; i < WorkDayData.Instance.CurrentProject.Cost.Length; i++)
				{
					CostAIOperation costItem = WorkDayData.Instance.CurrentProject.Cost[i];
					itemsImages.Add(new ItemMultiObjectEntry(this.gameObject, i, costItem));
				}
				SlotManagerImages.Initialize(itemsImages.Count, itemsImages, CostPrefab);
				Vector2 sizeOriginal = SlotManagerImages.Content.GetComponent<RectTransform>().sizeDelta;
				SlotManagerImages.Content.GetComponent<RectTransform>().sizeDelta = new Vector2(totalLength * ItemAICostView.WITH, ItemAICostView.HEIGHT);
				SlotManagerImages.SetHorizontalScroll(1);
			}
		}
	}
}