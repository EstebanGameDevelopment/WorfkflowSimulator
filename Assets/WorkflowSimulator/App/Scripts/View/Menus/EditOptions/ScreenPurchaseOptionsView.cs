using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
#if ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR || ENABLE_NREAL
using yourvrexperience.VR;
#endif

namespace yourvrexperience.WorkDay
{
	public class ScreenPurchaseOptionsView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenPurchaseOptionsView";

		public const string EventScreenPurchaseOptionsViewBasic = "EventScreenPurchaseOptionsViewBasic";
		public const string EventScreenPurchaseOptionsViewImages = "EventScreenPurchaseOptionsViewImages";

		public const string EventScreenPurchaseOptionsViewCancelPurchase = "EventScreenPurchaseOptionsViewCancelPurchase";
		public const string EventScreenPurchaseOptionsViewCompletedPurchase = "EventScreenPurchaseOptionsViewCompletedPurchase";

		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private Button buttonBasic;
		[SerializeField] private Button buttonImages;
		[SerializeField] private Button buttonClose;

		[SerializeField] private TextMeshProUGUI feedback;

		private int _slotID = -1;
		private int _level = 0;
		private long _timeout = 0;

		private bool _isPurchasing = false;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);
			_slotID = (int)parameters[0];
			_level = (int)parameters[1];
			_timeout = (long)parameters[2];

			buttonClose.onClick.AddListener(OnButtonBack);
			buttonBasic.onClick.AddListener(OnPurchaseBasic);
			buttonImages.onClick.AddListener(OnPurchaseImages);
			feedback.gameObject.SetActive(false);
			feedback.text = "";

			bool shouldShowEverything = false;
			if (yourvrexperience.Utils.Utilities.GetCurrentTimestamp() - _timeout > 0)
			{
				shouldShowEverything = true;
			}

			titleScreen.text = LanguageController.Instance.GetText("screen.purchase.options.title");
			if ((_slotID != -1) && (_level >= 1) && !shouldShowEverything)
			{
				buttonBasic.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.purchase.options.package.acquired.standard");
				buttonBasic.GetComponentInChildren<TextMeshProUGUI>().text += "\n" + LanguageController.Instance.GetText("screen.purchase.options.package.purchased");
				buttonBasic.interactable = false;
			}
			else
			{
				buttonBasic.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.purchase.options.package.standard");
			}
			if ((_slotID != -1) && (_level >= 2) && !shouldShowEverything)
			{
				buttonImages.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.purchase.options.package.acquired.images");
				buttonImages.GetComponentInChildren<TextMeshProUGUI>().text += "\n" + LanguageController.Instance.GetText("screen.purchase.options.package.purchased");
				buttonImages.interactable = false;
			}
			else
			{
				buttonImages.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.purchase.options.package.images");
			}

			UIEventController.Instance.Event += OnUIEvent;
		}

		public override void Destroy()
		{
			base.Destroy();

			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
		}

		private void HideAll(string message, bool showClose)
		{
			buttonBasic.gameObject.SetActive(false);
			buttonImages.gameObject.SetActive(false);
			buttonClose.gameObject.SetActive(showClose);
			feedback.gameObject.SetActive(true);
			feedback.text = message;
		}

		private void OnButtonBack()
		{
			if (_isPurchasing)
			{
				SystemEventController.Instance.DispatchSystemEvent(EventScreenPurchaseOptionsViewCancelPurchase);
			}
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnPurchaseBasic()
		{
			SystemEventController.Instance.DispatchSystemEvent(EventScreenPurchaseOptionsViewBasic, _slotID);
			_isPurchasing = true;
			HideAll(LanguageController.Instance.GetText("message.complete.transaction.in.other.window"), true);
		}

		private void OnPurchaseImages()
		{
#if DISABLE_PURCHASES
			ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("message.request.for.testers"));
#else
			SystemEventController.Instance.DispatchSystemEvent(EventScreenPurchaseOptionsViewImages, _slotID);
			_isPurchasing = true;
			HideAll(LanguageController.Instance.GetText("message.complete.transaction.in.other.window"), true);
#endif
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventScreenPurchaseOptionsViewCompletedPurchase))
			{
				_isPurchasing = false;
				if ((bool)parameters[0])
				{
					OnButtonBack();
				}
				else
				{
					HideAll(LanguageController.Instance.GetText("message.completed.transaction.cancelled"), true);
				}
			}
		}
	}
}