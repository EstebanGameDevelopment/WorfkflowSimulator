using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class ScreenSystemCostsView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenSystemCostsView";

		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private Button checkCosts;
		[SerializeField] private Button checkStorage;
		[SerializeField] private Button buttonCancel;

		public override string NameScreen
		{
			get { return ScreenName; }
		}


		public override void Initialize(params object[] parameters)
		{			
			base.Initialize(parameters);

			buttonCancel.onClick.AddListener(OnCancel);

			checkCosts.onClick.AddListener(OnCheckCosts);
			checkCosts.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.system.costs.money");

			checkStorage.onClick.AddListener(OnCheckStorage);
			checkStorage.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.system.costs.storage");

			title.text = LanguageController.Instance.GetText("screen.system.costs.title");
		}

		public override void Destroy()
		{
			base.Destroy();
		}

		private void OnCancel()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnCheckCosts()
		{
			GameObject screen = ScreenController.Instance.CreateScreen(ScreenAICostHistoryView.ScreenName, false, false);
			UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, screen, _canvas.sortingOrder + 1);
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnCheckStorage()
		{
			GameObject screen = ScreenController.Instance.CreateScreen(ScreenAIStorageUsedView.ScreenName, false, false);
			UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, screen, _canvas.sortingOrder + 1);
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}
	}
}