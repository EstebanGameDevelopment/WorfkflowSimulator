using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.MeetingController;
using static yourvrexperience.WorkDay.ScreenCalendarView;

namespace yourvrexperience.WorkDay
{
    public class ItemMeetingProgressView : MonoBehaviour, ISlotView
    {
        public const string EventItemMeetingProgressViewSelected = "EventItemMeetingProgressViewSelected";

        public GameObject PrefabPoint;

        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private MeetingData _meeting;
        private SlotManagerView _slotPoints;
        private string _projectName;

        private Button _btnEdit;

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
        public MeetingData Meeting
        {
            get { return _meeting; }
        }

        private ItemMultiObjectEntry _initialData;

        public void Initialize(params object[] parameters)
        {
            _initialData = (ItemMultiObjectEntry)parameters[0];
            _parent = (GameObject)_initialData.Objects[0];
            _index = (int)_initialData.Objects[1];
            MeetingInProgress meetingProgress = (MeetingInProgress)_initialData.Objects[2];
            _meeting = meetingProgress.Meeting;
            
            _projectName = "";
            ProjectInfoData project = WorkDayData.Instance.CurrentProject.GetProject(_meeting.ProjectId);
            if (project != null)
            {
                _defaultColor = project.GetColor();
                _projectName = project.Name;
            }                    

            _background = transform.Find("BG").GetComponent<Image>();
            _textArea = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            transform.Find("Background").GetComponent<Button>().onClick.AddListener(OnSelectedItemMeeting);

            SetUpData();
            Selected = false;

            _btnEdit = transform.Find("Edit").GetComponent<Button>();
            _btnEdit.onClick.AddListener(OnEditMeeting);

            _slotPoints = transform.Find("ScrollListPoints").GetComponent<SlotManagerView>();
            RenderPointsMembers();

            UIEventController.Instance.Event += OnUIEvent;

            Selected = false;
        }

        private void OnSelectedItemMeeting()
        {
            ItemSelected();
        }

        private void SetUpData()
        {
            string textItem = "<b>" + Utilities.ShortenText(_meeting.Name, 40) + "</b>" + "\n" + _meeting.GetMembersPacket();

            if ((_projectName != null) && (_projectName.Length > 0))
            {
                _textArea.text = textItem + "\n" + _projectName;
            }
            else
            {
                _textArea.text = textItem;
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
                _initialData = null;
                if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ApplyGenericAction(params object[] parameters)
        {
            SetUpData();
        }

        public void ButtonPressed()
        {
            ItemSelected();
        }

        public void ItemSelected(bool dispatchEvent = true)
        {
            Selected = !Selected;
            UIEventController.Instance.DispatchUIEvent(EventItemMeetingProgressViewSelected, _parent, this.gameObject, (Selected ? _index : -1), _meeting);
        }

        private void OnEditMeeting()
        {
            SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDCancelSelectionObject);
            SystemEventController.Instance.DispatchSystemEvent(LevelView.EventLevelViewLinesRequestDestroy);

            GameObject calendarGO = ScreenController.Instance.CreateScreen(ScreenCalendarView.ScreenName, false, false, CalendarOption.NORMAL, false);
            GameObject meetingGO = ScreenController.Instance.CreateScreen(ScreenMeetingView.ScreenName, false, false, _meeting.TaskId, _meeting);
            UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, meetingGO, calendarGO.GetComponent<Canvas>().sortingOrder + 1);
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventItemMeetingProgressViewSelected))
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
    }
}