using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class ItemMeetingView : MonoBehaviour, ISlotView
    {
        public const string EventItemMeetingViewSelected = "EventItemMeetingViewSelected";
        public const string EventItemMeetingViewDelete = "EventItemMeetingViewDelete";
        public const string EventItemMeetingViewEdit = "EventItemMeetingViewEdit";
        
        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private MeetingData _meeting;
        private TextMeshProUGUI _textArea;

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
                    _background.color = Color.white;
                }
            }
        }

        public void Initialize(params object[] parameters)
        {
            _parent = (GameObject)((ItemMultiObjectEntry)parameters[0]).Objects[0];
            _index = (int)((ItemMultiObjectEntry)parameters[0]).Objects[1];
            _meeting = (MeetingData)((ItemMultiObjectEntry)parameters[0]).Objects[2];

            bool interactionEnabled = true;
            if (((ItemMultiObjectEntry)parameters[0]).Objects.Count > 3)
            {
                interactionEnabled = (bool)((ItemMultiObjectEntry)parameters[0]).Objects[3];
            }

            _textArea = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            _textArea.text = _meeting.Name;

            transform.Find("Edit").GetComponent<Button>().onClick.AddListener(OnEditMeeting);
            transform.Find("Delete").GetComponent<Button>().onClick.AddListener(OnDeleteMeeting);

            if (!interactionEnabled)
            {
                transform.Find("Edit").GetComponent<Button>().interactable = false;
                transform.Find("Delete").GetComponent<Button>().interactable = false;
            }
            if (ApplicationController.Instance.IsPlayMode)
            {
                transform.Find("Delete").GetComponent<Button>().interactable = false;
            }

            UIEventController.Instance.Event += OnUIEvent;
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
            UIEventController.Instance.DispatchUIEvent(EventItemMeetingViewSelected, _parent, this.gameObject, (Selected ? _index : -1), _meeting);
        }

        private void OnEditMeeting()
        {
            UIEventController.Instance.DispatchUIEvent(EventItemMeetingViewEdit, _parent, this.gameObject, _meeting);
        }

        private void OnDeleteMeeting()
        {
            UIEventController.Instance.DispatchUIEvent(EventItemMeetingViewDelete, _parent, this.gameObject, _meeting);
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventItemMeetingViewSelected))
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