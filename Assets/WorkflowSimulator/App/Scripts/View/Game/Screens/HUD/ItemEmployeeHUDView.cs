using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class ItemEmployeeHUDView : MonoBehaviour, ISlotView
    {        
        public const string EventItemEmployeeHUDViewSelected = "EventItemEmployeeHUDViewSelected";
        
        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private WorldItemData _humanData;
        private IconColorView _iconColor;

        private TextMeshProUGUI _textName;
        private TextMeshProUGUI _textGroup;

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
            _humanData = (WorldItemData)((ItemMultiObjectEntry)parameters[0]).Objects[2];
            
            _background = transform.Find("BG").GetComponent<Image>();
            transform.Find("Background").GetComponent<Button>().onClick.AddListener(OnSelectedItemHuman);
            
            _textName = transform.Find("Name").GetComponent<TextMeshProUGUI>();
            _textGroup = transform.Find("Group").GetComponent<TextMeshProUGUI>();
            _textName.text = "<b>" + _humanData.Name + "</b>";
            GroupInfoData group = WorkDayData.Instance.CurrentProject.GetGroupOfMember(_humanData.Name);
            _textGroup.text = "";
            if (group != null)
            {
                _textGroup.text = group.Name;
            }
            _iconColor = transform.Find("Icon").GetComponent<IconColorView>();
            _iconColor.Refresh();

            UIEventController.Instance.Event += OnUIEvent;
        }

        private void OnSelectedItemHuman()
        {
            ItemSelected();
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
                _humanData = null;
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
            UIEventController.Instance.DispatchUIEvent(EventItemEmployeeHUDViewSelected, _parent, this.gameObject, (Selected ? _index : -1), _humanData);
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventItemEmployeeHUDViewSelected))
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