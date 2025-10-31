using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.RunStateRun;

namespace yourvrexperience.WorkDay
{
	public class ScreenPanelEditionView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenPanelEditionView";
		
		public const string EventScreenPanelEditionActivation = "EventScreenPanelEditionActivation";

		public const string EventScreenPanelEditionUpdateTitle = "EventScreenPanelEditionUpdateTitle";
		public const string EventScreenPanelEditionResetToIdle = "EventScreenPanelEditionResetToIdle";
		public const string EventScreenPanelEditionViewSelectTab = "EventScreenPanelEditionViewSelectTab";
		public const string EventScreenPanelEditionViewActivateCancellation = "EventScreenPanelEditionViewActivateCancellation";

		public const string EventScreenPanelEditionViewDelayedBake = "EventScreenPanelEditionViewDelayedBake";
		public const string EventScreenPanelEditionViewDelayedDestroyLoadingBake = "EventScreenPanelEditionViewDelayedDestroyLoadingBake";

		public const string BtnEventEditionAction = "BtnEventEditionAction";

		public enum TabsEdition { Resize = 0, Decoration, Avatars }

		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private Button buttonBack;
		[SerializeField] private Button buttonSave;
		[SerializeField] private Button buttonRotateLeft;
		[SerializeField] private Button buttonRotateRight;

		[SerializeField] private Button[] buttonTabs;
		[SerializeField] private GameObject[] contentTabs;

		[SerializeField] private Image iconClose;
		[SerializeField] private Image iconCancel;

		private bool _isPlacementMode;

		public override string NameScreen
		{
			get { return ScreenName; }
		}

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			titleScreen.text = LanguageController.Instance.GetText("screen.edition.panel.title");

			buttonBack.onClick.AddListener(OnBackButton);
			buttonSave.onClick.AddListener(OnSaveButton);
			buttonRotateLeft.onClick.AddListener(OnRotateLeftButton);
			buttonRotateRight.onClick.AddListener(OnRotateRigthButton);

			UIEventController.Instance.Event += OnUIEvent;
			SystemEventController.Instance.Event += OnSystemEvent;
			

            SystemEventController.Instance.DispatchSystemEvent(EventScreenPanelEditionActivation, true);

			if (ApplicationController.Instance.LevelView.IsReadyToPlay() == LevelView.CodeLevelReady.NoArea)
            {
				string title = LanguageController.Instance.GetText("text.info");
				string description = LanguageController.Instance.GetText("screen.panel.edition.requirements.for.level");
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, title, description);
				UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetAlignment, TextAlignmentOptions.TopLeft);
			}

			SetCancelIcon(false);
		}

        public override void Destroy()
		{
			base.Destroy();
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			
			SystemEventController.Instance.DispatchSystemEvent(EventScreenPanelEditionActivation, false);
			UIEventController.Instance.DelayUIEvent(TimeHUD.EventTimeHUDUpdateEnableInteraction, 0.1f, true);

			string title = LanguageController.Instance.GetText("text.warning");
			string description = "";
			switch (ApplicationController.Instance.LevelView.IsReadyToPlay())
            {
				case LevelView.CodeLevelReady.NoArea:
					description = LanguageController.Instance.GetText("screen.panel.edition.no.area.in.level");
					break;

				case LevelView.CodeLevelReady.NoHumans:
					description = LanguageController.Instance.GetText("screen.panel.edition.no.humans.in.level");
					break;

				case LevelView.CodeLevelReady.NoChairs:
					description = LanguageController.Instance.GetText("screen.panel.edition.no.chairs.in.level");
					break;

				case LevelView.CodeLevelReady.NoHumansWithChairs:
					description = LanguageController.Instance.GetText("screen.panel.edition.no.humans.with.chairs.in.level");
					break;

				case LevelView.CodeLevelReady.NoExit:
					description = LanguageController.Instance.GetText("screen.panel.edition.no.exit.in.level");
					break;
            }
			if (description.Length > 0)
            {
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, title, description);
            }
		}

		private void SetCancelIcon(bool activated)
        {
			_isPlacementMode = activated;
			iconClose.gameObject.SetActive(!_isPlacementMode);
			iconCancel.gameObject.SetActive(_isPlacementMode);
		}

		private void OnBackButton()
		{
			if (_isPlacementMode)
            {
				UIEventController.Instance.DispatchUIEvent(ItemImageCatalog.EventItemImageCatalogUnSelectAll);
			}
			else
            {
				UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
				SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunChangeState, SubStateRun.Idle, null, null);
			}
		}

		private TabsEdition GetSelectedTab(Button button)
        {
			for (int i = 0; i < buttonTabs.Length; i++)
            {
				if (buttonTabs[i] == button)
                {
					return (TabsEdition)i;
                }
			}
			return TabsEdition.Resize;
		}

		private void EnableAllButtons()
        {
			for (int i = 0; i < buttonTabs.Length; i++)
            {
				buttonTabs[i].interactable = true;
			}
		}

		private void DisableAllTabs()
        {
			for (int i = 0; i < contentTabs.Length; i++)
			{
				contentTabs[i].GetComponent<ITabEdition>().Deactivate();
			}
		}

		private void OnRotateRigthButton()
		{
			SystemEventController.Instance.DispatchSystemEvent(EditionSubStateIdle.EventSubStateIdleRotateCamera, true);
		}

		private void OnRotateLeftButton()
		{
			SystemEventController.Instance.DispatchSystemEvent(EditionSubStateIdle.EventSubStateIdleRotateCamera, false);
		}

		private void OnSaveButton()
		{
			UIEventController.Instance.DispatchUIEvent(TimeHUD.EventTimeHUDSaveProject, true);
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{			
			if (nameEvent.Equals(EventScreenPanelEditionResetToIdle))
            {
				EnableAllButtons();
				DisableAllTabs();
			}			
			if (nameEvent.Equals(EventScreenPanelEditionViewSelectTab))
            {
				TabsEdition tabSelected = (TabsEdition)parameters[0];
				EnableAllButtons();
				DisableAllTabs();

				buttonTabs[(int)tabSelected].interactable = false;
				contentTabs[(int)tabSelected].GetComponent<ITabEdition>().Activate();
			}
			if (nameEvent.Equals(EventScreenPanelEditionUpdateTitle))
            {
				titleScreen.text = (string)parameters[0];
			}
			if (nameEvent.Equals(EventScreenPanelEditionViewActivateCancellation))
            {
				SetCancelIcon((bool)parameters[0]);
			}
			if (nameEvent.Equals(ItemImageCatalog.EventItemImageCatalogUnSelectAll))
            {
				SetCancelIcon(false);
			}
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventScreenPanelEditionViewDelayedBake))
            {
				SystemEventController.Instance.DelaySystemEvent(EventScreenPanelEditionViewDelayedDestroyLoadingBake, 2f);
				ApplicationController.Instance.LevelView.BakeNavMesh();
			}
			if (nameEvent.Equals(EventScreenPanelEditionViewDelayedDestroyLoadingBake))
            {
				UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
            }
			if (nameEvent.Equals(ButtonSystemEvent.EventButtonSystemEventClicked))
			{
				if (((string)parameters[0]).Equals(BtnEventEditionAction))
				{
					TabsEdition tabSelected = GetSelectedTab((Button)parameters[1]);

					EnableAllButtons();
					DisableAllTabs();

					buttonTabs[(int)tabSelected].interactable = false;
					contentTabs[(int)tabSelected].GetComponent<ITabEdition>().Activate();
				}
			}
		}
	}
}