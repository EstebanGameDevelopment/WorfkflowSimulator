using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class ItemAsistantView : MonoBehaviour, ISlotView
    {
        public const string EventItemAsistantViewSelected = "EventItemAsistantViewSelected";
        
        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private string _nameHuman;

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

            transform.Find("Text").GetComponent<TextMeshProUGUI>().text = _nameHuman;
            GroupInfoData groupData = WorkDayData.Instance.CurrentProject.GetGroupOfMember(_nameHuman);
            if (groupData == null)
            {
                transform.Find("Group").GetComponent<TextMeshProUGUI>().text = "";
            }
            else
            {
                transform.Find("Group").GetComponent<TextMeshProUGUI>().text = groupData.Name;
            }
            transform.Find("Icon").GetComponent<IconColorView>().Refresh();

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
            UIEventController.Instance.DispatchUIEvent(EventItemAsistantViewSelected, _parent, this.gameObject, (Selected ? _index : -1), _nameHuman);
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventItemAsistantViewSelected))
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