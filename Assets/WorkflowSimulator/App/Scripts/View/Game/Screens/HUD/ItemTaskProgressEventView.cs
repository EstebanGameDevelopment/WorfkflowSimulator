using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class ItemTaskProgressEventView : MonoBehaviour, ISlotView
    {
        public const string EventItemTaskProgressEventViewSelected = "EventItemTaskProgressEventViewSelected";
        public const string EventItemTaskProgressEventViewDelete = "EventItemTaskProgressEventViewDelete";

        public GameObject PrefabPoint;

        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private CurrentDocumentInProgress _currDoc;
        private TaskItemData _task;
        private SlotManagerView _slotPoints;
        private string _projectName;

        private Button _btnEdit;

        private TextMeshProUGUI _textArea;

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
        public TaskItemData Task
        {
            get { return _task; }
        }

        private bool _isPopUpEvent = false;
        private float _consumeEvent = 0;
        private ItemMultiObjectEntry _initialData;

        public void Initialize(params object[] parameters)
        {
            _initialData = (ItemMultiObjectEntry)parameters[0];
            _parent = (GameObject)_initialData.Objects[0];
            _index = (int)_initialData.Objects[1];
            _currDoc = (CurrentDocumentInProgress)_initialData.Objects[2];
            var (taskData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_currDoc.TaskID);
            _projectName = "";
            if (boardName != null)
            {
                BoardData board = WorkDayData.Instance.CurrentProject.GetBoardFor(boardName);
                if (board != null)
                {
                    ProjectInfoData project = WorkDayData.Instance.CurrentProject.GetProject(board.ProjectId);
                    if (project != null)
                    {
                        _defaultColor = project.GetColor();
                        _projectName = project.Name;
                    }                    
                }
            }

            _task = taskData;
            _background = transform.Find("BG").GetComponent<Image>();
            _textArea = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            transform.Find("Background").GetComponent<Button>().onClick.AddListener(OnSelectedItemTask);

            SetUpData();
            Selected = false;

            _btnEdit = transform.Find("Edit").GetComponent<Button>();
            _btnEdit.onClick.AddListener(OnEditTask);

            _slotPoints = transform.Find("ScrollListPoints").GetComponent<SlotManagerView>();
            RenderPointsMembers();

            UIEventController.Instance.Event += OnUIEvent;
            SystemEventController.Instance.Event += OnSystemEvent;

            Selected = false;
        }

        private void OnSelectedItemTask()
        {
            ItemSelected();
        }

        private void SetUpData()
        {
            string typePopUp = "";
            if (_initialData.Objects.Count > 3)
            {
                _isPopUpEvent = true;
                if ((bool)(_initialData.Objects[3]))
                {
                    typePopUp = LanguageController.Instance.GetText("text.work.started") + ": ";
                }
                else
                {
                    typePopUp = LanguageController.Instance.GetText("text.work.stopped") + ": ";
                }

                _consumeEvent = (float)_initialData.Objects[4];
            }

            string textItem = "";
            string textTaskName = "";
            if (_task != null)
            {
                textTaskName = Utilities.ShortenText(_task.Name, 40);
            }
            if (_isPopUpEvent)
            {
                textItem = typePopUp + "<b>(" + _currDoc.Time + "h) " + Utilities.ShortenText(_currDoc.Name, 35) + "</b>\n" + textTaskName + "\n";
            }
            else
            {
                if (_currDoc.IsDone())
                {
                    textItem = "<b>(" + _currDoc.Time + "h) " + Utilities.ShortenText(_currDoc.Name, 30) + "</b>\n" + textTaskName + "\n";
                }
                else
                {
                    textItem = "<b>(" + Utilities.CeilDecimal(_currDoc.TimeDone / 60, 1) + "/" + _currDoc.Time + "h) " + Utilities.ShortenText(_currDoc.Name, 25) + "</b>\n" + textTaskName + "\n";
                }
            }

            string[] bufPersons = _currDoc.Persons.Split(",");
            string finalTokenPersons = "";
            foreach (string person in bufPersons)
            {
                string finalPerson = person.Trim();                
                if (!_currDoc.Working)
                {
                    if (finalTokenPersons.Length > 0) finalTokenPersons += ",";
                    finalTokenPersons += finalPerson;
                }
                else
                {
                    WorldItemData finalHumanData = WorkDayData.Instance.CurrentProject.GetItemByName(finalPerson);
                    if (finalHumanData != null)
                    {
                        TaskProgressData progressData = finalHumanData.GetActiveTask();
                        if (progressData != null)
                        {
                            if (progressData.TaskUID == _currDoc.TaskID)
                            {
                                if (finalTokenPersons.Length > 0) finalTokenPersons += ",";
                                finalTokenPersons += finalPerson;
                            }
                        }
                    }
                }
            }
            
            if ((_projectName != null) && (_projectName.Length > 0))
            {
                textItem += "<b>" + Utilities.ShortenText(finalTokenPersons, 35) + "</b>" + "\n" + _currDoc.Type.ToUpper();
            }
            else
            {
                textItem += "<b>" + Utilities.ShortenText(finalTokenPersons, 35) + "</b>";
            }
            _textArea.text = textItem.Trim();
        }

        private void RenderPointsMembers()
        {
            List<string> members = _task.GetMembers();
            List<Color> colors = new List<Color>();
            foreach (string member in members)
            {
                Color colorMember = WorkDayData.Instance.CurrentProject.GetColorForMember(member);
                if (!colors.Contains(colorMember) && (colorMember != Color.gray))
                {
                    colors.Add(colorMember);
                }
            }

            _slotPoints.ClearCurrentGameObject(true);
            List<ItemMultiObjectEntry> itemsPointsColor = new List<ItemMultiObjectEntry>();
            for (int i = 0; i < colors.Count; i++)
            {
                itemsPointsColor.Add(new ItemMultiObjectEntry(this.gameObject, i, colors[i]));
            }
            _slotPoints.Initialize(itemsPointsColor.Count, itemsPointsColor, PrefabPoint);
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
                _currDoc = null;
                _task = null;
                _initialData = null;
                if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
                if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ApplyGenericAction(params object[] parameters)
        {
            SetUpData();
        }

        public void ButtonPressed()
        {
            ItemSelected();
        }

        public void ItemSelected(bool dispatchEvent = true)
        {
            Selected = !Selected;
            UIEventController.Instance.DispatchUIEvent(EventItemTaskProgressEventViewSelected, _parent, this.gameObject, (Selected ? _index : -1), _currDoc, _task);
        }

        private void OnEditTask()
        {
            SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDCancelSelectionObject);
            SystemEventController.Instance.DispatchSystemEvent(LevelView.EventLevelViewLinesRequestDestroy);

            var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_task.UID);
            ProjectInfoData projectMeeting = WorkDayData.Instance.CurrentProject.GetProjectByTaskItemUID(_task.UID);
            SystemEventController.Instance.DispatchSystemEvent(ScreenProjectsView.EventScreenProjectsViewLoadProject, projectMeeting, false);
            ScreenController.Instance.CreateScreen(ScreenTaskManagerView.ScreenName, true, true, boardName);
            UIEventController.Instance.DispatchUIEvent(ItemTaskView.EventItemTaskViewEdit, null, null, _task);

            if (_currDoc.IsDone())
            {
                UIEventController.Instance.DispatchUIEvent(ScreenTaskView.EventScreenTaskViewForceShowData, _currDoc.Name);
            }
            else
            {
                UIEventController.Instance.DispatchUIEvent(ScreenTaskView.EventScreenTaskViewForceShowDocsTODO);
            }            
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventItemTaskProgressEventViewSelected))
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

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(ApplicationController.EventMainControllerSelectedHuman))
            {
                
            }
        }

        private void Update()
        {
            if (_consumeEvent > 0)
            {
                _consumeEvent -= Time.deltaTime;
                _initialData.Objects[4] = _consumeEvent;
                if (_consumeEvent <= 0)
                {
                    _consumeEvent = 0;
                    UIEventController.Instance.DispatchUIEvent(EventItemTaskProgressEventViewDelete, _parent, _initialData);
                }
            }
        }
    }
}