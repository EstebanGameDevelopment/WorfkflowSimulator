using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
#if ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR || ENABLE_NREAL
using yourvrexperience.VR;
#endif

namespace yourvrexperience.WorkDay
{
    public class ItemProjectEntryIndex : MonoBehaviour, ISlotView
    {
        public const string EventItemProjectEntryIndexSelected = "EventItemProjectEntryIndexSelected";

        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private Image _icon;

        private ProjectEntryIndex _projectEntryIndex;

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
            _projectEntryIndex = (ProjectEntryIndex)((ItemMultiObjectEntry)parameters[0]).Objects[2];
            Sprite[] iconsPackages = (Sprite[])((ItemMultiObjectEntry)parameters[0]).Objects[3];

            transform.Find("Name").GetComponent<TextMeshProUGUI>().text = _projectEntryIndex.Title;

            _background = transform.GetComponent<Image>();
            transform.GetComponent<Button>().onClick.AddListener(ButtonPressed);

            _icon = transform.Find("Icon").GetComponent<Image>();
            ProjectSlot selectedSlot = WorkDayData.Instance.GetSlotForProject(_projectEntryIndex.Id);
            if (selectedSlot != null)
            {
                _icon.overrideSprite = iconsPackages[selectedSlot.Level - 1];
                if (selectedSlot.Timeout == -1)
                {
                    transform.Find("Timeout").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.GetText("word.forever");
                }
                else
                {
                    transform.Find("Timeout").GetComponent<TextMeshProUGUI>().text = yourvrexperience.Utils.Utilities.ConvertTimestampToDate(selectedSlot.Timeout);
                }
            }
            else
            {
                _icon.gameObject.SetActive(false);
                transform.Find("Timeout").gameObject.SetActive(false);
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
                _projectEntryIndex = null;
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
            UIEventController.Instance.DispatchUIEvent(EventItemProjectEntryIndexSelected, _parent, this.gameObject, (Selected ? _index : -1), _projectEntryIndex);
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventItemProjectEntryIndexSelected))
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