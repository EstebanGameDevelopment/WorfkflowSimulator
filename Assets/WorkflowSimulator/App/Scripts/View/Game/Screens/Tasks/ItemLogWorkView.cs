using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class ItemLogWorkView : MonoBehaviour, ISlotView
    {
        public const string EventItemLogWorkViewSelected = "EventItemLogWorkViewSelected";
        
        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private TimeWorkingDataDisplay _logWork;

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
            _logWork = (TimeWorkingDataDisplay)((ItemMultiObjectEntry)parameters[0]).Objects[2];

            transform.Find("Text").GetComponent<TextMeshProUGUI>().text = _logWork.Owner;
            transform.Find("Icon").GetComponent<IconColorView>().Refresh();

            transform.Find("Start").GetComponent<TextMeshProUGUI>().text = _logWork.StartTime.ToShortDateString() + " " + _logWork.StartTime.ToShortTimeString();
            transform.Find("End").GetComponent<TextMeshProUGUI>().text = _logWork.EndTime.ToShortDateString() + " " + _logWork.EndTime.ToShortTimeString();

            _background = transform.GetComponent<Image>();
            transform.GetComponent<Button>().onClick.AddListener(ButtonPressed);

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
                _logWork = null;
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
            UIEventController.Instance.DispatchUIEvent(EventItemLogWorkViewSelected, _parent, this.gameObject, (Selected ? _index : -1), _logWork);
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventItemLogWorkViewSelected))
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