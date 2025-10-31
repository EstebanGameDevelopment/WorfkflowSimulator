using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class ItemReferenceView : MonoBehaviour, ISlotView
    {
        public const string EventItemReferenceViewSelected = "EventItemReferenceViewSelected";

        public enum ReferenceTypes { None = 0, Person = 1, Document = 2, Task = 3 }

        private TextMeshProUGUI _description;
        private TextMeshProUGUI _group;
        private IconColorView _iconColor;

        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private ReferenceTypes _type;
        private string _id;
        private Button _buttonBackground;
        private string _extraInformation;

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
            _type = (ReferenceTypes)((ItemMultiObjectEntry)parameters[0]).Objects[2];
            _id = (string)((ItemMultiObjectEntry)parameters[0]).Objects[3];

            _background = transform.Find("Background").GetComponent<Image>();
            _buttonBackground = transform.Find("Button").GetComponent<Button>();
            _buttonBackground.onClick.AddListener(OnSelectedReference);

            _description = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            _group = transform.Find("Group").GetComponent<TextMeshProUGUI>();
            _group.text = "";
            _iconColor = transform.Find("Icon").GetComponent<IconColorView>();
            _iconColor.Locked = true;
            _extraInformation = "";
            switch (_type)
            {
                case ReferenceTypes.Person:
                    WorldItemData humanData = WorkDayData.Instance.CurrentProject.GetItemByName(_id);
                    if (humanData != null)
                    {
                        _extraInformation = humanData.Data;
                    }                    
                    GroupInfoData groupInfo = WorkDayData.Instance.CurrentProject.GetGroupByName(_id);
                    if (groupInfo != null)
                    {
                        _description.text = groupInfo.Name;
                        _iconColor.ApplyColor(groupInfo.GetColor());                        
                    }
                    else
                    {
                        GroupInfoData groupOfMember = WorkDayData.Instance.CurrentProject.GetGroupOfMember(_id);
                        if (groupOfMember != null)
                        {
                            _description.text = _id;
                            _group.text = groupOfMember.Name;
                            _iconColor.ApplyColor(groupOfMember.GetColor());
                        }
                        else
                        {
                            _description.text = _id;
                        }
                    }
                    break;

                case ReferenceTypes.Document:
                    int documentID = int.Parse(_id);
                    DocumentData document = WorkDayData.Instance.CurrentProject.GetDocumentInSystemByID(documentID);
                    _description.text = document.Name;
                    _extraInformation = document.Description;
                    if ((document.Owner != null) && (document.Owner.Length > 0))
                    {
                        GroupInfoData groupOwner = WorkDayData.Instance.CurrentProject.GetGroupByName(document.Owner);
                        if (groupOwner != null)
                        {
                            _description.text = document.Name;
                            _group.text = groupOwner.Name;
                            _iconColor.ApplyColor(groupOwner.GetColor());
                        }
                        else
                        {
                            GroupInfoData groupOfMemberOwner = WorkDayData.Instance.CurrentProject.GetGroupOfMember(document.Owner);
                            if (groupOfMemberOwner != null)
                            {
                                _description.text = document.Name;
                                _group.text = document.Owner;
                                _iconColor.ApplyColor(groupOfMemberOwner.GetColor());
                            }
                            else
                            {
                                _description.text = document.Name;
                            }
                        }
                    }
                    break;

                case ReferenceTypes.Task:
                    int taskID = int.Parse(_id);
                    var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(taskID);
                    BoardData boardData = WorkDayData.Instance.CurrentProject.GetBoardFor(boardName);
                    if (boardData != null)
                    {
                        ProjectInfoData projectInfoData = WorkDayData.Instance.CurrentProject.GetProject(boardData.ProjectId);
                        _description.text = taskItemData.Name;
                        _group.text = boardName;
                        _iconColor.ApplyColor(projectInfoData.GetColor());
                        _extraInformation = taskItemData.Description;
                    }
                    break;
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

        private void OnSelectedReference()
        {
            ItemSelected();
        }

        public void ItemSelected(bool dispatchEvent = true)
        {
            Selected = !Selected;
            UIEventController.Instance.DispatchUIEvent(EventItemReferenceViewSelected, _parent, this.gameObject, (Selected ? _index : -1), _type, _id, _description.text, _extraInformation);
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventItemReferenceViewSelected))
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