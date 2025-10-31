using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class ItemGroupInfoView : MonoBehaviour, ISlotView
    {        
        public const string EventItemGroupInfoViewSelected = "EventItemGroupInfoViewSelected";
        public const string EventItemGroupInfoViewUnselectAll = "EventItemGroupInfoViewUnselectAll";
        public const string EventItemGroupInfoViewForceSelection = "EventItemGroupInfoViewForceSelection";
        public const string EventItemGroupInfoViewDelete = "EventItemGroupInfoViewDelete";
        public const string EventItemGroupInfoViewEdit = "EventItemGroupInfoViewEdit";
        public const string EventItemGroupInfoViewUpdateName = "EventItemGroupInfoViewUpdateName";
        public const string EventItemGroupInfoViewRefreshColor = "EventItemGroupInfoViewRefreshColor";
        
        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private GroupInfoData _groupInfoData;
        private IconColorView _icon;

        private TextMeshProUGUI _nameGroup;

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
            _groupInfoData = (GroupInfoData)((ItemMultiObjectEntry)parameters[0]).Objects[2];

            _icon = transform.Find("Icon").GetComponent<IconColorView>();
            _nameGroup = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            _nameGroup.text = _groupInfoData.Name;
            _icon.Refresh();
            Button buttonEdit = transform.Find("Edit").GetComponent<Button>();
            Button buttonDelete = transform.Find("Delete").GetComponent<Button>();

            buttonEdit.onClick.AddListener(OnEditGroup);
            buttonDelete.onClick.AddListener(OnDeleteGroup);

            if (ApplicationController.Instance.IsPlayMode)
            {
                buttonDelete.gameObject.SetActive(false);
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

        public void ButtonPressed()
        {
            ItemSelected();
        }

        public void ItemSelected(bool dispatchEvent = true)
        {
            Selected = !Selected;
            UIEventController.Instance.DispatchUIEvent(EventItemGroupInfoViewSelected, _parent, this.gameObject, (Selected ? _index : -1), _groupInfoData);
        }
        
        private void OnEditGroup()
        {
            UIEventController.Instance.DispatchUIEvent(EventItemGroupInfoViewEdit, _parent, this.gameObject, _groupInfoData);
        }

        private void OnDeleteGroup()
        {            
            UIEventController.Instance.DispatchUIEvent(EventItemGroupInfoViewDelete, _parent, this.gameObject, _groupInfoData);
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventItemGroupInfoViewSelected))
            {
                if ((GameObject)parameters[0] == _parent)
                {
                    if ((GameObject)parameters[1] != this.gameObject)
                    {
                        Selected = false;
                    }
                }
            }
            if (nameEvent.Equals(EventItemGroupInfoViewUnselectAll))
            {
                Selected = false;
            }
            if (nameEvent.Equals(EventItemGroupInfoViewForceSelection))
            {                
                if (_groupInfoData.Name == (string)parameters[0])
                {
                    ButtonPressed();
                }
            }
            if (nameEvent.Equals(EventItemGroupInfoViewUpdateName))
            {
                if (_groupInfoData == (GroupInfoData)parameters[0])
                {
                    _nameGroup.text = _groupInfoData.Name;
                }
            }
            if (nameEvent.Equals(EventItemGroupInfoViewRefreshColor))
            {
                if (_groupInfoData == (GroupInfoData)parameters[0])
                {
                    _icon.Refresh();
                }
            }
        }
    }
}