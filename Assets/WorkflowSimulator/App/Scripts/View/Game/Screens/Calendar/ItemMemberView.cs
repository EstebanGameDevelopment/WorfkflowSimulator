using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class ItemMemberView : MonoBehaviour, ISlotView
    {
        public const string EventItemMemberViewSelected = "EventItemMemberViewSelected";
        public const string EventItemMemberViewDelete = "EventItemMemberViewDelete";
        public const string EventItemMemberViewDisableDelete = "EventItemMemberViewDisableDelete";
        
        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private string _nameHuman;

        private Button _deleteMember;

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
            _nameHuman = (string)((ItemMultiObjectEntry)parameters[0]).Objects[2];
            bool enableDelete = true;
            if (((ItemMultiObjectEntry)parameters[0]).Objects.Count > 3)
            {
                enableDelete = (bool)((ItemMultiObjectEntry)parameters[0]).Objects[3];
            }

            transform.Find("Text").GetComponent<TextMeshProUGUI>().text = _nameHuman;
            _deleteMember = transform.Find("Delete").GetComponent<Button>();
            _deleteMember.onClick.AddListener(OnDeleteMember);
            transform.Find("Icon").GetComponent<IconColorView>().Refresh();

            if (!enableDelete)
            {
                _deleteMember.interactable = false;
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
                _deleteMember = null;
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
            if (parameters[0] is bool)
            {
                _deleteMember.interactable = (bool)parameters[0];
            }
        }

        public void ButtonPressed()
        {
            ItemSelected();
        }

        public void ItemSelected(bool dispatchEvent = true)
        {
            Selected = !Selected;
            UIEventController.Instance.DispatchUIEvent(EventItemMemberViewSelected, _parent, this.gameObject, (Selected ? _index : -1), _nameHuman);
        }

        private void OnDeleteMember()
        {
            UIEventController.Instance.DispatchUIEvent(EventItemMemberViewDelete, _parent, this.gameObject, _nameHuman);
        }

        public void EnableInteraction(bool interaction)
        {
            _deleteMember.interactable = interaction;
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventItemMemberViewSelected))
            {
                if ((GameObject)parameters[0] == _parent)
                {
                    if ((GameObject)parameters[1] != this.gameObject)
                    {
                        Selected = false;
                    }
                }
            }
            if (nameEvent.Equals(EventItemMemberViewDisableDelete))
            {                
                _deleteMember.interactable = false;
            }
        }
    }
}