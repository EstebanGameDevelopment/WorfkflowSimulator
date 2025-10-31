using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class ItemDocTODOView : MonoBehaviour, ISlotView
    {
        public const string EventItemDocTODOViewSelected = "EventItemDocTODOViewSelected";
        public const string EventItemDocTODOViewDelete = "EventItemDocTODOViewDelete";
        
        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private CurrentDocumentInProgress _doc;
        private GameObject _iconContent;
        private Image _iconWorking;
        private Image _iconDone;
        private Button _btnContent;
        private Button _btnDelete;
        private Color _defaultColor = Color.white;

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
                    _background.color = _defaultColor;
                }
            }
        }

        public void Initialize(params object[] parameters)
        {
            _parent = (GameObject)((ItemMultiObjectEntry)parameters[0]).Objects[0];
            _index = (int)((ItemMultiObjectEntry)parameters[0]).Objects[1];
            _doc = (CurrentDocumentInProgress)((ItemMultiObjectEntry)parameters[0]).Objects[2];
            ProjectInfoData project = WorkDayData.Instance.CurrentProject.GetProject(_doc.ProjectID);

            transform.Find("Name").GetComponent<TextMeshProUGUI>().text = _doc.Name;
            transform.Find("Type").GetComponent<TextMeshProUGUI>().text = _doc.Type.ToUpper();
            transform.Find("Assigned").GetComponent<TextMeshProUGUI>().text = _doc.Persons;
            transform.Find("Dependency").GetComponent<TextMeshProUGUI>().text = _doc.Dependency;
            transform.Find("Project").GetComponent<TextMeshProUGUI>().text = project.Name;
            transform.Find("Time").GetComponent<TextMeshProUGUI>().text = _doc.Time + "h";
            _defaultColor = project.GetColor();

            _iconContent = transform.Find("State").gameObject;
            _btnContent = _iconContent.GetComponent<Button>();
            _btnContent.onClick.AddListener(OnButtonContent);
            _iconWorking = transform.Find("State/Working").GetComponent<Image>();
            _iconDone = transform.Find("State/Done").GetComponent<Image>();
            _iconContent.SetActive(false);

            _btnDelete = transform.Find("Delete").GetComponent<Button>();
            _btnDelete.onClick.AddListener(OnDeleteTODO);

            bool isDocumentInProgress = AICommandsController.Instance.IsDocumentsWorkingForTask(_doc.TaskID, _doc.GetDocUniqueID());

            if (isDocumentInProgress || _doc.IsDone())
            {
                _iconContent.SetActive(true);
                _iconWorking.gameObject.SetActive(false);
                _iconDone.gameObject.SetActive(false);

                if (isDocumentInProgress && !_doc.IsDone())
                {
                    _iconWorking.gameObject.SetActive(true);
                }
                if (_doc.IsDone())
                {
                    _iconDone.gameObject.SetActive(true);
                    _btnDelete.interactable = false;
                }
            }

            _background = transform.GetComponent<Image>();
            transform.GetComponent<Button>().onClick.AddListener(ButtonPressed);

            UIEventController.Instance.Event += OnUIEvent;

            Selected = false;
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
                _doc = null;
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
            transform.Find("Name").GetComponent<TextMeshProUGUI>().text = _doc.Name;
            transform.Find("Assigned").GetComponent<TextMeshProUGUI>().text = _doc.Persons;
            transform.Find("Dependency").GetComponent<TextMeshProUGUI>().text = _doc.Dependency;
            transform.Find("Time").GetComponent<TextMeshProUGUI>().text = _doc.Time + "h";
        }

        private void OnButtonContent()
        {
            if (_doc.IsDone())
            {
                UIEventController.Instance.DispatchUIEvent(ScreenTaskView.EventScreenTaskViewForceShowData, _doc.Name);
            }
            else
            {
                UIEventController.Instance.DispatchUIEvent(AICommandsController.EventAICommandsControllerForceCompleteCurrentTaskProgress, _doc);
            }
        }

        public void ButtonPressed()
        {
            ItemSelected();
        }

        public void ItemSelected(bool dispatchEvent = true)
        {
            Selected = !Selected;
            UIEventController.Instance.DispatchUIEvent(EventItemDocTODOViewSelected, _parent, this.gameObject, (Selected ? _index : -1), _doc);
        }

        private void OnDeleteTODO()
        {
            UIEventController.Instance.DispatchUIEvent(EventItemDocTODOViewDelete, _parent, _index, _doc);
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventItemDocTODOViewSelected))
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

        void Update()
        {
            if (_iconWorking.gameObject.activeSelf)
            {
                _iconWorking.transform.localEulerAngles += new Vector3(0, 0, 90 * Time.deltaTime);
            }
        }
    }
}