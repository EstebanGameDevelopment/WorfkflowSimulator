using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
#if ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR || ENABLE_NREAL
using yourvrexperience.VR;
#endif

namespace yourvrexperience.WorkDay
{
    public class ItemRoleEmployee : MonoBehaviour, ISlotView
    {        
        public const string EventItemRoleEmployeeSelected = "EventItemRoleEmployeeSelected";

        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private IconColorView _iconColorGroup;

        private WorldItemData _employee;

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
                    _background.color = Color.grey;
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
            _employee = (WorldItemData)((ItemMultiObjectEntry)parameters[0]).Objects[2];

            transform.Find("Name").GetComponent<TextMeshProUGUI>().text = _employee.Name;
            GroupInfoData groupEmployee = WorkDayData.Instance.CurrentProject.GetGroupOfMember(_employee.Name);
            string nameSelectedGroup = LanguageController.Instance.GetText("text.no.group");
            if (groupEmployee != null)
            {
                nameSelectedGroup = groupEmployee.Name;
            }
            transform.Find("Group").GetComponent<TextMeshProUGUI>().text = nameSelectedGroup;
            if (_employee.IsLead)
            {
                transform.Find("Category").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.lead");
            }
            else
            if (_employee.IsSenior)
            {
                transform.Find("Category").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.senior");
            }
            else
            {
                transform.Find("Category").GetComponent<TextMeshProUGUI>().text = "";
            }

            _background = transform.GetComponent<Image>();
            transform.GetComponent<Button>().onClick.AddListener(ButtonPressed);

            _iconColorGroup = transform.Find("Icon").GetComponent<IconColorView>();
            _iconColorGroup.Locked = true;
            if (groupEmployee != null)
            {
                _iconColorGroup.ApplyInfo(groupEmployee.Name, groupEmployee.GetColor());
            }
            else
            {
                transform.Find("Icon").gameObject.SetActive(false);
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
                _employee = null;
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
            UIEventController.Instance.DispatchUIEvent(EventItemRoleEmployeeSelected, _parent, this.gameObject, (Selected ? _index : -1), _employee);
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventItemRoleEmployeeSelected))
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