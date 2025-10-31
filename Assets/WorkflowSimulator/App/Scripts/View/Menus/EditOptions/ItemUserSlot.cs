using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
#if ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR || ENABLE_NREAL
using yourvrexperience.VR;
#endif

namespace yourvrexperience.WorkDay
{
    public class ItemUserSlot : MonoBehaviour, ISlotView
    {
        public const string EventItemUserSlotSelected = "EventItemUserSlotSelected";
        public const string EventItemUserSlotToUpgrade = "EventItemUserSlotToUpgrade";

        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;

        private ProjectSlot _slotUser;
        private List<ProjectEntryIndex> _projectsList;
        private bool _isFreeSlot = false;

        private Image _icon;

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
        public bool IsFreeSlot
        {
            get { return _isFreeSlot; }
        }

        public void Initialize(params object[] parameters)
        {
            _parent = (GameObject)((ItemMultiObjectEntry)parameters[0]).Objects[0];
            _index = (int)((ItemMultiObjectEntry)parameters[0]).Objects[1];
            _slotUser = (ProjectSlot)((ItemMultiObjectEntry)parameters[0]).Objects[2];
            _projectsList = (List<ProjectEntryIndex>)((ItemMultiObjectEntry)parameters[0]).Objects[3];
            Sprite[] icons = (Sprite[])((ItemMultiObjectEntry)parameters[0]).Objects[4];

            bool upgradable = true;
            TextMeshProUGUI levelText = transform.Find("Level").GetComponent<TextMeshProUGUI>();
            if (_slotUser.Level == 1)
            {
                levelText.text = LanguageController.Instance.GetText("item.slot.user.level.basic");
            }
            if (_slotUser.Level == 2)
            {
                levelText.text = LanguageController.Instance.GetText("item.slot.user.level.images");
            }
            if (yourvrexperience.Utils.Utilities.GetCurrentTimestamp() - _slotUser.Timeout > 0)
            {
                upgradable = true;
            }
            if (_slotUser.Timeout == -1)
            {
                upgradable = false;
            }

            if (_slotUser.Timeout == -1)
            {
                transform.Find("Timeout").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.GetText("word.forever");
            }
            else
            {
                transform.Find("Timeout").GetComponent<TextMeshProUGUI>().text = yourvrexperience.Utils.Utilities.ConvertTimestampToDate(_slotUser.Timeout);
            }

            _icon = transform.Find("Icon").GetComponent<Image>();
            _icon.overrideSprite = icons[_slotUser.Level - 1];

            ProjectEntryIndex foundStory = _projectsList.Find(s => s.Id == _slotUser.Project);
            TextMeshProUGUI projectText = transform.Find("Story").GetComponent<TextMeshProUGUI>();
            if (foundStory != null)
            {
                _isFreeSlot = false;
                projectText.text = foundStory.Title;
                projectText.color = Color.red;
            }
            else
            {
                _isFreeSlot = true;
                projectText.text = LanguageController.Instance.GetText("item.slot.user.free.slot");
                projectText.color = Color.green;
            }

            _background = transform.GetComponent<Image>();
            transform.GetComponent<Button>().onClick.AddListener(ButtonPressed);

            if (upgradable)
            {
                transform.Find("ButtonUpgrade").GetComponent<Button>().onClick.AddListener(OnUpgradeSlot);
            }
            else
            {
                transform.Find("ButtonUpgrade").gameObject.SetActive(false);
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
                _slotUser = null;
                _projectsList = null;
                if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnUpgradeSlot()
        {
            UIEventController.Instance.DispatchUIEvent(EventItemUserSlotToUpgrade, _slotUser);
        }

        public void ButtonPressed()
        {
            ItemSelected();
        }

        public void ItemSelected(bool dispatchEvent = true)
        {
            Selected = !Selected;
            UIEventController.Instance.DispatchUIEvent(EventItemUserSlotSelected, _parent, this.gameObject, (Selected ? _index : -1), _slotUser);
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventItemUserSlotSelected))
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

        public void ApplyGenericAction(params object[] parameters)
        {
        }
    }
}