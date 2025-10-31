using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
#if ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR || ENABLE_NREAL
using yourvrexperience.VR;
#endif

namespace yourvrexperience.WorkDay
{
	public class ScreenMainOptionsView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenMainOptionsView";

		public const string EventScreenMainOptionsViewCreate = "EventScreenMainOptionsViewCreate";
		public const string EventScreenMainOptionsViewEdit = "EventScreenMainOptionsViewEdit";
		public const string EventScreenMainOptionsViewBack = "EventScreenMainOptionsViewBack";

		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private TextMeshProUGUI descriptionScreen;

		[SerializeField] private CustomButton buttonCreate;
		[SerializeField] private CustomButton buttonEdit;
		
		[SerializeField] private CustomButton buttonSlots;

		[SerializeField] private TextMeshProUGUI titleCreate;
		[SerializeField] private TextMeshProUGUI titleEdit;

		[SerializeField] private Button buttonBack;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			buttonBack.onClick.AddListener(OnButtonBack);
			buttonCreate.onClick.AddListener(OnButtonCreateStory);
			buttonEdit.onClick.AddListener(OnButtonEditStory);

			titleScreen.text = LanguageController.Instance.GetText("screen.edition.options.title");
			descriptionScreen.text = LanguageController.Instance.GetText("screen.edition.options.description");

			titleCreate.text = LanguageController.Instance.GetText("screen.edition.options.button.create");
			titleEdit.text = LanguageController.Instance.GetText("screen.edition.options.button.edit");

			buttonSlots.onClick.AddListener(OnButtonSlots);

			UIEventController.Instance.Event += OnUIEvent;
		}

        public override void Destroy()
		{
			base.Destroy();

			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
		}

		private void OnButtonSlots()
		{
			ScreenController.Instance.CreateScreen(ScreenSlotsManagementView.ScreenName, false, false, false);
		}

		private void OnButtonBack()
		{
			UIEventController.Instance.DispatchUIEvent(EventScreenMainOptionsViewBack);
		}

		private void OnButtonCreateStory()
		{
			UIEventController.Instance.DispatchUIEvent(EventScreenMainOptionsViewCreate);
		}

		private void OnButtonEditStory()
		{
			UIEventController.Instance.DispatchUIEvent(EventScreenMainOptionsViewEdit);
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
		}
	}
}