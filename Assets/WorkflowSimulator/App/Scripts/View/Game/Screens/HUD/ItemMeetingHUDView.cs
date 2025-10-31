using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.ScreenCalendarView;

namespace yourvrexperience.WorkDay
{
    public class ItemMeetingHUDView : MonoBehaviour, ISlotView
    {
        public const string EventItemMeetingHUDViewSelected = "EventItemMeetingHUDViewSelected";
        public const string EventItemMeetingHUDViewDelayedTrigger = "EventItemMeetingHUDViewDelayedTrigger";
        public const string SubEventItemMeetingHUDViewStartMeeting = "SubEventItemMeetingHUDViewStartMeeting";

        public GameObject PrefabPoint;

        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private MeetingData _meeting;
        private SlotManagerView _slotPoints;

        private CustomButton _btnWorking;
        private Image _iconStart;
        private Image _iconWorking;
        private bool _isWorking;

        private TextMeshProUGUI _textArea;

        private Color _defaultColor = Color.white;

        public int Index
        {
            get { return _index; }
        }
        public ItemMultiObjectEntry Data
        {
            get { return _data; }
        }
        public virtual bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                if (_selected)
                {
                    _background.color = Color.magenta;
                }
                else
                {
                    _background.color = _defaultColor;
                }
            }
        }
        private bool IsWorking
        {
            get { return _isWorking; }
            set
            {
                _isWorking = value;
                if (_isWorking)
                {
                    _iconWorking.color = Color.red;
                    _iconStart.gameObject.SetActive(false);
                    _iconWorking.gameObject.SetActive(true);
                }
                else
                {
                    _iconWorking.color = Color.white;
                    _iconStart.gameObject.SetActive(true);
                    _iconWorking.gameObject.SetActive(false);
                }
            }
        }

        public void Initialize(params object[] parameters)
        {
            _parent = (GameObject)((ItemMultiObjectEntry)parameters[0]).Objects[0];
            _index = (int)((ItemMultiObjectEntry)parameters[0]).Objects[1];
            _meeting = (MeetingData)((ItemMultiObjectEntry)parameters[0]).Objects[2];
            
            _background = transform.Find("BG").GetComponent<Image>();
            transform.Find("Background").GetComponent<Button>().onClick.AddListener(OnSelectedItemTask);
            
            _textArea = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            ProjectInfoData projectInfo = WorkDayData.Instance.CurrentProject.GetProject(_meeting.ProjectId);
            string textItem = "";
            if (projectInfo != null)
            {
                if (WorkDayData.Instance.CurrentProject.GetCurrentTime().DayOfYear != _meeting.GetTimeStart().DayOfYear)
                {
                    textItem = Utilities.ShortenText(_meeting.Name, 20) + "\n<b>" + _meeting.GetTimeStart().ToShortDateString() + " " + _meeting.GetTimeStart().ToShortTimeString() + "\n" + Utilities.ShortenText(projectInfo.Name, 20) + "</b>\n" + _meeting.Description;
                }
                else
                {
                    textItem = Utilities.ShortenText(_meeting.Name, 20) + "\n<b>" + _meeting.GetTimeStart().ToShortTimeString() + "\n" + Utilities.ShortenText(projectInfo.Name, 20) + "</b>\n" + _meeting.Description;
                }
            }
            else
            {
                if (WorkDayData.Instance.CurrentProject.GetCurrentTime().DayOfYear != _meeting.GetTimeStart().DayOfYear)
                {
                    textItem = Utilities.ShortenText(_meeting.Name, 20) + "\n<b>" + _meeting.GetTimeStart().ToShortDateString() + " " + _meeting.GetTimeStart().ToShortTimeString() + "\n" + "SOCIAL" + "</b>\n" + _meeting.Description;
                }
                else
                {
                    textItem = Utilities.ShortenText(_meeting.Name, 20) + "\n<b>" + _meeting.GetTimeStart().ToShortTimeString() + "\n" + "SOCIAL" + "</b>\n" + _meeting.Description;
                }
            }
            
            if (projectInfo != null)
            {
                _defaultColor = projectInfo.GetColor();
            }
            Selected = false;
            _textArea.text = textItem;

            transform.Find("Edit").GetComponent<Button>().onClick.AddListener(OnEditMeeting);
            _btnWorking = transform.Find("Working").GetComponent<CustomButton>();
            _btnWorking.onClick.AddListener(OnStartMeeting);
            _iconStart = _btnWorking.transform.Find("IconStart").GetComponent<Image>();
            _iconWorking = _btnWorking.transform.Find("IconWorking").GetComponent<Image>();
            IsWorking = _meeting.InProgress;

            _slotPoints = transform.Find("ScrollListPoints").GetComponent<SlotManagerView>();
            RenderPointsMembers();

            _btnWorking.gameObject.SetActive(true);
            UpdateStartState();
            IsWorking = false;

            UIEventController.Instance.Event += OnUIEvent;
            SystemEventController.Instance.Event += OnSystemEvent;

            if (ApplicationController.Instance.IsPlayMode)
            {
                _btnWorking.interactable = _meeting.HasPlayer(true);
            }
            if (!ApplicationController.Instance.TimeHUD.IsPlayingTime)
            {
                _btnWorking.interactable = false;
            }
        }

        private void OnSelectedItemTask()
        {
            ItemSelected();
        }

        private void UpdateStartState()
        {
            _btnWorking.interactable = false;
            if (ApplicationController.Instance.SelectedHuman != null)
            {
                if ((ApplicationController.Instance.HumanPlayer != null)
                    && (ApplicationController.Instance.HumanPlayer == ApplicationController.Instance.SelectedHuman))
                {
                    if (_meeting != null)
                    {
                        if (_meeting.IsMemberInMeeting(ApplicationController.Instance.SelectedHuman.NameHuman))
                        {
                            _btnWorking.interactable = true;
                        }
                        else
                        {
                            if (_meeting.InProgress)
                            {
                                _btnWorking.interactable = true;
                            }
                        }
                    }
                }
            }
        }

        private void OnStartMeeting()
        {
            if (ApplicationController.Instance.IsPlayMode)
            {
                if (ApplicationController.Instance.SelectedHuman == null)
                {
                    if (ApplicationController.Instance.HumanPlayer != null)
                    {
                        SystemEventController.Instance.DelaySystemEvent(EventItemMeetingHUDViewDelayedTrigger, 0.2f, _meeting);
                        SystemEventController.Instance.DispatchSystemEvent(HumanView.EventHumanViewForceSelection, ApplicationController.Instance.HumanPlayer.NameHuman);                        
                        return;
                    }
                }
            }

            if (ApplicationController.Instance.SelectedHuman != null)
            {
                if ((ApplicationController.Instance.HumanPlayer != null) 
                    && (ApplicationController.Instance.HumanPlayer == ApplicationController.Instance.SelectedHuman))                    
                {
                    string description = "";
                    if (_meeting.InProgress)
                    {
                        description = LanguageController.Instance.GetText("text.do.you.want.to.join.this.meeting") + " " + _meeting.Name;
                    }
                    else
                    {
                        if (!_meeting.IsMemberInMeeting(ApplicationController.Instance.SelectedHuman.NameHuman))
                        {
                            ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, this.gameObject, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("text.you.cannot.start.a.meeting.if.you.are.not.a.member"));
                            return;
                        }
                        description = LanguageController.Instance.GetText("text.do.you.want.to.start.this.meeting") + " " + _meeting.Name;
                    }
                    ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, this.gameObject, LanguageController.Instance.GetText("text.warning"), description, SubEventItemMeetingHUDViewStartMeeting);
                }
            }
            else
            {
                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, this.gameObject, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("text.you.cannot.start.a.meeting.without.selecting.an.employee"));
            }
        }

        private void RenderPointsMembers()
        {
            List<string> members = _meeting.GetMembers();
            List<Color> colors = new List<Color>();
            foreach (string member in members)
            {
                Color colorMember = WorkDayData.Instance.CurrentProject.GetColorForMember(member);
                if (!colors.Contains(colorMember) && (colorMember != Color.gray))
                {
                    colors.Add(colorMember);
                }
            }

            _slotPoints.ClearCurrentGameObject(true);
            List<ItemMultiObjectEntry> itemsPointsColor = new List<ItemMultiObjectEntry>();
            for (int i = 0; i < colors.Count; i++)
            {
                itemsPointsColor.Add(new ItemMultiObjectEntry(this.gameObject, i, colors[i]));
            }
            _slotPoints.Initialize(itemsPointsColor.Count, itemsPointsColor, PrefabPoint);
        }

        void OnDestroy()
        {
            Destroy();
        }

        public bool Destroy()
        {
            if (_parent != null)
            {
                _parent = null;
                _meeting = null;
                if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
                if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ApplyGenericAction(params object[] parameters)
        {
        }

        public void ButtonPressed()
        {
            ItemSelected();
        }

        public void ItemSelected(bool dispatchEvent = true)
        {
            Selected = !Selected;
            UIEventController.Instance.DispatchUIEvent(EventItemMeetingHUDViewSelected, _parent, this.gameObject, (Selected ? _index : -1), _meeting);
        }

        private void OnEditMeeting()
        {
            GameObject calendarGO = ScreenController.Instance.CreateScreen(ScreenCalendarView.ScreenName, false, false, CalendarOption.NORMAL, false);
            GameObject meetingGO = ScreenController.Instance.CreateScreen(ScreenMeetingView.ScreenName, false, false, _meeting.TaskId, _meeting);
            UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, meetingGO, calendarGO.GetComponent<Canvas>().sortingOrder + 1);
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(SubEventItemMeetingHUDViewStartMeeting))
            {
                if (this.gameObject == (GameObject)parameters[0])
                {
                    ScreenInformationResponses userResponse = (ScreenInformationResponses)parameters[1];
                    if (userResponse == ScreenInformationResponses.Confirm)
                    {
                        SystemEventController.Instance.DispatchSystemEvent(ScreenListEventsHUDView.EventScreenListEventsHUDViewDestroy);
                        UIEventController.Instance.DispatchUIEvent(MeetingController.EventMeetingControllerUIRequestToStartMeeting, _meeting, _meeting.TaskId);
                    }
                }
            }
            if (nameEvent.Equals(EventItemMeetingHUDViewSelected))
            {
                if ((GameObject)parameters[0] == _parent)
                {
                    if ((GameObject)parameters[1] != this.gameObject)
                    {
                        Selected = false;
                    }
                }
            }
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(ApplicationController.EventMainControllerSelectedHuman))
            {
                UpdateStartState();
            }
            if (nameEvent.Equals(EventItemMeetingHUDViewDelayedTrigger))
            {
                if (_meeting == (MeetingData)parameters[0])
                {
                    OnStartMeeting();
                }
            }
        }

        private void Update()
        {
            if (IsWorking)
            {
                _iconWorking.transform.localEulerAngles += new Vector3(0, 0, 90 * Time.deltaTime);
            }
        }
    }
}