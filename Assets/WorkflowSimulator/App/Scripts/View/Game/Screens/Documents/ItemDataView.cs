using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class ItemDataView : MonoBehaviour, ISlotView
    {
        public const string EventItemDataViewSelected = "EventItemDataViewSelected";
        public const string EventItemDataViewUnSelecteAll = "EventItemDataViewUnSelecteAll";
        public const string EventItemDataViewUnSelectContainer = "EventItemDataViewUnSelectContainer";
        public const string EventItemDataViewDelete = "EventItemDataViewDelete";
        public const string EventItemDataViewRefresh = "EventItemDataViewRefresh";
        public const string EventItemDataViewForceSelection = "EventItemDataViewForceSelection";
        public const string EventItemDataViewForceNameSelection = "EventItemDataViewForceNameSelection";
        
        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private DocumentData _document;
        private TextMeshProUGUI _textName;
        private IconColorView _icon;

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
            _document = (DocumentData)((ItemMultiObjectEntry)parameters[0]).Objects[2];

            _textName = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            _icon = transform.Find("Icon").GetComponent<IconColorView>();
            UpdateColorIconOwner();

            _textName.text = _document.Name;
            Button buttonDelete = transform.Find("Delete").GetComponent<Button>();

            buttonDelete.onClick.AddListener(OnDeleteBoard);
            
            if (ApplicationController.Instance.IsPlayMode)
            {
                if (_document.IsGlobal && !_document.Owner.Equals(ApplicationController.Instance.HumanPlayer.NameHuman))
                {
                    buttonDelete.interactable = false;
                }                
            }

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

        private void UpdateColorIconOwner()
        {
            Color colorGroup = WorkDayData.Instance.CurrentProject.GetColorForMember(_document.Owner);
            _icon.ApplyInfo(WorkDayData.Instance.CurrentProject.GetGroupLetter(_document.Owner), colorGroup);
            _icon.Locked = true;
        }

        public void ButtonPressed()
        {
            ItemSelected();
        }

        public void ItemSelected(bool dispatchEvent = true)
        {
            Selected = !Selected;
            UIEventController.Instance.DispatchUIEvent(EventItemDataViewSelected, _parent, this.gameObject, (Selected ? _index : -1), _document);
        }

        private void OnDeleteBoard()
        {            
            UIEventController.Instance.DispatchUIEvent(EventItemDataViewDelete, _parent, this.gameObject, _document);
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventItemDataViewForceSelection))
            {                
                if (_document.Id == (int)parameters[0])
                {
                    ButtonPressed();
                }
            }
            if (nameEvent.Equals(EventItemDataViewForceNameSelection))
            {
                if (_document.Name == (string)parameters[0])
                {
                    ButtonPressed();
                }
            }
            if (nameEvent.Equals(EventItemDataViewSelected))
            {
                if ((GameObject)parameters[0] == _parent)
                {
                    if ((GameObject)parameters[1] != this.gameObject)
                    {
                        Selected = false;
                    }
                }
            }
            if (nameEvent.Equals(EventItemDataViewUnSelecteAll))
            {                
                Selected = false;
            }
            if (nameEvent.Equals(EventItemDataViewUnSelectContainer))
            {
                if ((GameObject)parameters[0] == _parent)
                {
                    Selected = false;
                }
            }
            if (nameEvent.Equals(EventItemDataViewRefresh))
            {
                if (_document == (DocumentData)parameters[0])
                {
                    _textName.text = _document.Name;
                    UpdateColorIconOwner();
                }
            }            
        }
    }
}