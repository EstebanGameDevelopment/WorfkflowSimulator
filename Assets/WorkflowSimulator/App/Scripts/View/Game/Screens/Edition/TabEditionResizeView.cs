using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class TabEditionResizeView : TabEditionBaseView, ITabEdition
    {
        public const string TabNameView = "TabEditionResizeView";

        public const string EventTabEditionResizeViewActivation = "EventTabEditionResizeViewActivation";
        public const string EventTabEditionResizeViewWork = "EventTabEditionResizeViewWork";
        public const string EventTabEditionResizeViewMeeting = "EventTabEditionResizeViewMeeting";
        public const string EventTabEditionResizeViewKitchen = "EventTabEditionResizeViewKitchen";
        public const string EventTabEditionResizeViewBathroom = "EventTabEditionResizeViewBathroom";
        public const string EventTabEditionResizeViewExit = "EventTabEditionResizeViewExit";

        public const string EventTabEditionResizeViewSelectArea = "EventTabEditionResizeViewSelectArea";
        public const string EventTabEditionResizeViewAddArea = "EventTabEditionResizeViewAddArea";
        public const string EventTabEditionResizeViewRemoveArea = "EventTabEditionResizeViewRemoveArea";
        
        public const string SubEventResizeWorldConfirmation = "SubEventResizeWorldConfirmation";
        
        [SerializeField] private ToggleGroup toggleGroup;
        [SerializeField] private Toggle toggleResize;
        [SerializeField] private Toggle toggleWork;
        [SerializeField] private Toggle toggleMeeting;
        [SerializeField] private Toggle toggleKitchen;
        [SerializeField] private Toggle toggleBathroom;
        [SerializeField] private Toggle toggleExit;

        [SerializeField] private Button buttonSelectArea;
        [SerializeField] private Button buttonAddArea;
        [SerializeField] private Button buttonRemoveArea;

        private bool _listenChange = false;

        public override void Activate()
        {
            base.Activate();

            if (!_initialized)
            {
                toggleResize.isOn = false;
                toggleResize.onValueChanged.AddListener(OnButtonTabResize);
                toggleWork.onValueChanged.AddListener(OnButtonWorkArea);
                toggleMeeting.onValueChanged.AddListener(OnButtonMeetingArea);
                toggleKitchen.onValueChanged.AddListener(OnButtonKitchenArea);
                toggleBathroom.onValueChanged.AddListener(OnButtonBathroomArea);
                toggleExit.onValueChanged.AddListener(OnButtonExitArea);

                buttonSelectArea.onClick.AddListener(OnSelectArea);
                buttonAddArea.onClick.AddListener(OnAddArea);
                buttonRemoveArea.onClick.AddListener(OnRemoveArea);

                _initialized = true;
            }

            UIEventController.Instance.Event += OnUIEvent;

            toggleResize.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.edition.resize.layout");
            toggleWork.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.edition.work.areas");
            toggleMeeting.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.edition.meeting.areas");
            toggleKitchen.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.edition.kitchen.areas");
            toggleBathroom.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.edition.bathroom.areas");
            toggleExit.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.edition.exit.areas");

            buttonSelectArea.gameObject.SetActive(false);
            buttonAddArea.gameObject.SetActive(false);
            buttonRemoveArea.gameObject.SetActive(false);

            _listenChange = true;
        }

        public override string TabName()
        {
            return TabNameView;
        }

        public override void Deactivate()
        {
            base.Deactivate();

            if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

            _listenChange = false;
            TogglesTabIsOff();
        }

        void OnDestroy()
        {
            Deactivate();
        }

        private void TogglesTabIsOff()
        {
            toggleResize.isOn = false;
            toggleWork.isOn = false;
            toggleMeeting.isOn = false;
            toggleKitchen.isOn = false;
            toggleBathroom.isOn = false;
            toggleExit.isOn = false;
        }

        private void ClearAllToggles()
        {
            _listenChange = false;
            TogglesTabIsOff();
            _listenChange = true;
        }

        private void OnButtonTabResize(bool value)
        {
            if (_listenChange)
            {
                if (value)
                {
                    string titleWarning = LanguageController.Instance.GetText("text.warning");
                    string textAskToResize = LanguageController.Instance.GetText("screen.tab.edition.resize.confirmation");
                    ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, titleWarning, textAskToResize, SubEventResizeWorldConfirmation);
                }
                else
                {
                    UIEventController.Instance.DispatchUIEvent(EventTabEditionResizeViewActivation, value);
                }
            }            
        }

        private void OnButtonWorkArea(bool value)
        {
            if (_listenChange)
            {
                UIEventController.Instance.DispatchUIEvent(EventTabEditionResizeViewWork, value);                
            }
        }

        private void OnButtonMeetingArea(bool value)
        {
            if (_listenChange)
            {
                UIEventController.Instance.DispatchUIEvent(EventTabEditionResizeViewMeeting, value);
            }
        }

        private void OnButtonKitchenArea(bool value)
        {
            if (_listenChange)
            {
                UIEventController.Instance.DispatchUIEvent(EventTabEditionResizeViewKitchen, value);
            }
        }

        private void OnButtonBathroomArea(bool value)
        {
            if (_listenChange)
            {
                UIEventController.Instance.DispatchUIEvent(EventTabEditionResizeViewBathroom, value);
            }
        }

        private void OnButtonExitArea(bool value)
        {
            if (_listenChange)
            {
                UIEventController.Instance.DispatchUIEvent(EventTabEditionResizeViewExit, value);
            }
        }

        private void OnSelectArea()
        {
            UIEventController.Instance.DispatchUIEvent(EventTabEditionResizeViewSelectArea);
        }

        private void OnAddArea()
        {
            UIEventController.Instance.DispatchUIEvent(EventTabEditionResizeViewAddArea);
        }

        private void OnRemoveArea()
        {
            UIEventController.Instance.DispatchUIEvent(EventTabEditionResizeViewRemoveArea);
        }

        protected override void OnSystemEvent(string nameEvent, object[] parameters)
        {
            base.OnSystemEvent(nameEvent, parameters);

            if (nameEvent.Equals(ScreenInfoItemView.EventScreenInfoItemViewItemSelected))
            {
                ClearAllToggles();
            }
            if (nameEvent.Equals(EditionSubStateDecoration.EventSubStateDecorationStarted) || nameEvent.Equals(EditionSubStateAvatar.EventSubStateAvatarStarted))
            {
                ClearAllToggles();
            }
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(SubEventResizeWorldConfirmation))
            {
                ScreenInformationResponses userResponse = (ScreenInformationResponses)parameters[1];
                if (userResponse == ScreenInformationResponses.Confirm)
                {
                    _listenChange = false;
                    toggleResize.isOn = true;
                    UIEventController.Instance.DispatchUIEvent(ItemImageCatalog.EventItemImageCatalogUnSelectAll);
                    UIEventController.Instance.DispatchUIEvent(EventTabEditionResizeViewActivation, true);
                    _listenChange = true;
                }
                else
                {
                    ClearAllToggles();
                }
            }
            if (nameEvent.Equals(EventTabEditionResizeViewSelectArea))
            {
                buttonAddArea.interactable = true;
                buttonRemoveArea.interactable = true;
                buttonSelectArea.interactable = false;

                SystemEventController.Instance.DispatchSystemEvent(ScreenInfoItemView.EventScreenInfoItemViewDestroy);
            }
            if (nameEvent.Equals(EventTabEditionResizeViewAddArea))
            {
                buttonAddArea.interactable = false;
                buttonRemoveArea.interactable = true;
                buttonSelectArea.interactable = true;

                SystemEventController.Instance.DispatchSystemEvent(ScreenInfoItemView.EventScreenInfoItemViewDestroy);
            }
            if (nameEvent.Equals(EventTabEditionResizeViewRemoveArea))
            {
                buttonAddArea.interactable = true;
                buttonRemoveArea.interactable = false;
                buttonSelectArea.interactable = true;

                SystemEventController.Instance.DispatchSystemEvent(ScreenInfoItemView.EventScreenInfoItemViewDestroy);
            }
            if (nameEvent.Equals(EditionSubStateAreas.EventSubStateAreasActivated))
            {
                buttonAddArea.gameObject.SetActive((bool)parameters[0]);
                buttonRemoveArea.gameObject.SetActive((bool)parameters[0]);
                buttonSelectArea.gameObject.SetActive((bool)parameters[0]);
            }
        }

        public void Run()
        {
        }
    }
}