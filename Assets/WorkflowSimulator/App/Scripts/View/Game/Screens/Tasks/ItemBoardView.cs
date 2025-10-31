using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class ItemBoardView : MonoBehaviour, ISlotView
    {
        public const string EventItemBoardViewSelected = "EventItemBoardViewSelected";
        public const string EventItemBoardViewEdit = "EventItemBoardViewEdit";
        public const string EventItemBoardViewDelete = "EventItemBoardViewDelete";
        public const string EventItemBoardViewUnSelectByParent = "EventItemBoardViewUnSelectByParent";
        
        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private string _nameBoard;

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
            _nameBoard = (string)((ItemMultiObjectEntry)parameters[0]).Objects[2];
            bool isDelete = (bool)((ItemMultiObjectEntry)parameters[0]).Objects[3];

            transform.Find("Text").GetComponent<TextMeshProUGUI>().text = _nameBoard;
            Button buttonDelete = transform.Find("Delete").GetComponent<Button>();
            Button buttonEdit = transform.Find("Edit").GetComponent<Button>();
            Button buttonEditNoAssigned = transform.Find("EditNoAssigned").GetComponent<Button>();

            buttonDelete.onClick.AddListener(OnDeleteBoard);
            buttonEdit.onClick.AddListener(OnEditBoard);
            buttonEditNoAssigned.onClick.AddListener(OnEditBoard);
            if (isDelete)
            {
                buttonDelete.gameObject.SetActive(true);
                buttonEditNoAssigned.gameObject.SetActive(true);
                buttonEdit.gameObject.SetActive(false);
            }
            else
            {
                buttonEditNoAssigned.gameObject.SetActive(false);
                buttonDelete.gameObject.SetActive(false);
                buttonEdit.gameObject.SetActive(true);
            }

            _background = transform.GetComponent<Image>();
            transform.GetComponent<Button>().onClick.AddListener(ButtonPressed);

            UIEventController.Instance.Event += OnUIEvent;

            if (ApplicationController.Instance.IsPlayMode)
            {
                buttonDelete.interactable = false;
            }
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
            UIEventController.Instance.DispatchUIEvent(EventItemBoardViewSelected, _parent, this.gameObject, (Selected ? _index : -1), _nameBoard);
        }

        private void OnEditBoard()
        {
            UIEventController.Instance.DispatchUIEvent(EventItemBoardViewEdit, _parent, this.gameObject, _nameBoard);
        }

        private void OnDeleteBoard()
        {            
            UIEventController.Instance.DispatchUIEvent(EventItemBoardViewDelete, _parent, this.gameObject, _nameBoard);
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventItemBoardViewSelected))
            {
                if ((GameObject)parameters[0] == _parent)
                {
                    if ((GameObject)parameters[1] != this.gameObject)
                    {
                        Selected = false;
                    }
                }
            }
            if (nameEvent.Equals(EventItemBoardViewUnSelectByParent))
            {
                if ((GameObject)parameters[0] == _parent)
                {
                    Selected = false;
                }
            }
        }
    }
}