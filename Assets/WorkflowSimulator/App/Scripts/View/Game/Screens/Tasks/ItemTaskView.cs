using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class ItemTaskView : MonoBehaviour, ISlotView
    {
        public const string EventItemTaskViewSelected = "EventItemTaskViewSelected";
        public const string EventItemTaskViewHUDSelected = "EventItemTaskViewHUDSelected";
        public const string EventItemTaskViewEdit = "EventItemTaskViewEdit";
        public const string EventItemTaskViewDelete = "EventItemTaskViewDelete";
        public const string EventItemTaskViewRefresh = "EventItemTaskViewRefresh";
        public const string EventItemTaskViewAllRefresher = "EventItemTaskViewAllRefresher";

        public GameObject PrefabPoint;

        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private TaskItemData _task;
        private TextMeshProUGUI _textArea;
        private SlotManagerView _slotPoints;

        private TextMeshProUGUI _referenceCalculator;

        private Image _iconWorking;
        private bool _isWorking;
        private List<string> _humansWorkingInTask;

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
        private bool IsWorking
        {
            get { return _isWorking; }
            set
            {
                _isWorking = value;
                if (_isWorking)
                {
                    _iconWorking.color = Color.red;
                }
                else
                {
                    _iconWorking.color = Color.white;
                }
            }
        }

        public void Initialize(params object[] parameters)
        {
            _parent = (GameObject)((ItemMultiObjectEntry)parameters[0]).Objects[0];
            _index = (int)((ItemMultiObjectEntry)parameters[0]).Objects[1];
            _task = (TaskItemData)((ItemMultiObjectEntry)parameters[0]).Objects[2];
            _referenceCalculator = null;
            if (((ItemMultiObjectEntry)parameters[0]).Objects.Count > 3)
            {
                _referenceCalculator = (TextMeshProUGUI)((ItemMultiObjectEntry)parameters[0]).Objects[3];
            }

            _background = transform.Find("BG").GetComponent<Image>();
            transform.Find("Background").GetComponent<Button>().onClick.AddListener(OnSelectedItemTask);            

            _textArea = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            float totalHoursDone = WorkDayData.Instance.CurrentProject.GetTotalLoggedTimeForTask(-1, _task.UID);
            string timeProgressData = "<b>"+ LanguageController.Instance.GetText("text.estimated") +"(" + _task.EstimatedTime + "H) - "+ LanguageController.Instance.GetText("word.done") + "(" + Utilities.CeilDecimal(totalHoursDone,1) + "h)</b>";
            
            string textItem = "";
            if (_referenceCalculator == null)
            {
                ProjectInfoData projectInfo = WorkDayData.Instance.CurrentProject.GetProjectByTaskItemUID(_task.UID);
                textItem = Utilities.ShortenText(_task.Name, 20) + "\n<b>" + timeProgressData + "\n" + Utilities.ShortenText(projectInfo.Name, 20) + "</b>\n" + _task.Description;
                _defaultColor = projectInfo.GetColor();
                Selected = false;
            }
            else
            {
                if (_task.Linked != -1)
                {
                    var (taskItemLinked, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_task.Linked);
                    if (taskItemLinked  != null)
                    {
                        string dependencyData = "<b>"+ LanguageController.Instance.GetText("text.dependency") + "(" + Utilities.ShortenText(taskItemLinked.Name, 20) + ")</b>";
                        textItem = _task.Name + "\n" + timeProgressData + "\n" + dependencyData + "\n" + _task.Description;
                    }
                }
                else
                {
                    textItem = _task.Name + "\n" + timeProgressData + "\n" + _task.Description;
                }
            }
            _textArea.text = textItem;

            if (_referenceCalculator != null)
            {
                string finalText = textItem.Substring(0, ((textItem.Length > 250) ? 250 : textItem.Length));
                if (textItem.Length > 250)
                {
                    finalText += "...";
                }
                _textArea.ForceMeshUpdate();
                _referenceCalculator.fontSize = _textArea.fontSize;
                _referenceCalculator.gameObject.SetActive(true);
                _referenceCalculator.text = finalText;
                _referenceCalculator.ForceMeshUpdate();
                Vector2 preferredValues = _referenceCalculator.GetPreferredValues(finalText, this.gameObject.GetComponent<LayoutElement>().preferredWidth, 0);
                _referenceCalculator.gameObject.SetActive(false);
                this.gameObject.GetComponent<LayoutElement>().preferredHeight = preferredValues.y + 90;
            }

            transform.Find("Edit").GetComponent<Button>().onClick.AddListener(OnEditTask);
            Button buttonDelete = null; 
            if (_referenceCalculator != null)
            {
                buttonDelete = transform.Find("Delete").GetComponent<Button>();
                buttonDelete.onClick.AddListener(OnDeleteTask);
            }
            CustomButton btnWorking = transform.Find("Working").GetComponent<CustomButton>();
            btnWorking.PointerEnterButton += OnBtnWorkingEnter;
            btnWorking.PointerExitButton += OnBtnWorkingExit;
            btnWorking.onClick.AddListener(OnStartProgressTask);
            _iconWorking = btnWorking.transform.Find("Icon").GetComponent<Image>();

            _slotPoints = transform.Find("ScrollListPoints").GetComponent<SlotManagerView>();
            RenderPointsMembers();

            btnWorking.gameObject.SetActive(true);
            if (ApplicationController.Instance.SelectedHuman != null)
            {
                btnWorking.interactable = true;
            }
            else
            {
                btnWorking.interactable = false;
            }
            IsWorking = false;

            UIEventController.Instance.Event += OnUIEvent;
            SystemEventController.Instance.Event += OnSystemEvent;

            if (ApplicationController.Instance.SelectedHuman != null)
            {
                SystemEventController.Instance.DispatchSystemEvent(TasksController.EventTasksControllerRequestTask, _task, ApplicationController.Instance.SelectedHuman.NameHuman);
            }
            else
            {
                SystemEventController.Instance.DispatchSystemEvent(TasksController.EventTasksControllerRequestTask, _task);
            }

            if (ApplicationController.Instance.IsPlayMode && !_task.IsUserCreated)
            {
                if (buttonDelete != null)
                {
                    buttonDelete.interactable = false;
                }

                if ((ApplicationController.Instance.SelectedHuman != null) 
                    && (ApplicationController.Instance.HumanPlayer.NameHuman.Equals(ApplicationController.Instance.SelectedHuman.NameHuman)))
                {
                    btnWorking.interactable = true;
                }
                else
                {
                    btnWorking.interactable = false;
                }
            }
            if (ApplicationController.Instance.IsPlayMode)
            {
                if (_task.HasHumanPlayer())
                {
                    if (ApplicationController.Instance.HumanPlayer == ApplicationController.Instance.SelectedHuman)
                    {
                        btnWorking.interactable = true;
                    }
                    else
                    {
                        btnWorking.interactable = false;
                    }
                }
            }
            if (!ApplicationController.Instance.TimeHUD.IsPlayingTime)
            {
                btnWorking.interactable = false;
            }
        }

        private void OnSelectedItemTask()
        {
            ItemSelected();
        }

        private void OnStartProgressTask()
        {
            SystemEventController.Instance.DispatchSystemEvent(TasksController.EventTasksControllerStartTask, _task);
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
                _task = null;
                _humansWorkingInTask = null;
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
        }

        public void ButtonPressed()
        {
            ItemSelected();
        }

        public void ItemSelected(bool dispatchEvent = true)
        {
            Selected = !Selected;
            if (Selected)
            {
                UIEventController.Instance.DispatchUIEvent(ScreenTaskManagerView.EventScreenTaskManagerViewShowTaskInformation, _task);
            }
            UIEventController.Instance.DispatchUIEvent(EventItemTaskViewSelected, _parent, this.gameObject, (Selected ? _index : -1), _task);
            if (_referenceCalculator == null)
            {
                UIEventController.Instance.DispatchUIEvent(EventItemTaskViewHUDSelected, _parent, this.gameObject, (Selected ? _index : -1), _task);
            }                
        }

        private void OnEditTask()
        {
            if (_referenceCalculator == null)
            {
                var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_task.UID);
                ProjectInfoData projectMeeting = WorkDayData.Instance.CurrentProject.GetProjectByTaskItemUID(_task.UID);
                SystemEventController.Instance.DispatchSystemEvent(ScreenProjectsView.EventScreenProjectsViewLoadProject, projectMeeting, false);
                ScreenController.Instance.CreateScreen(ScreenTaskManagerView.ScreenName, true, true, boardName);
            }
            UIEventController.Instance.DispatchUIEvent(EventItemTaskViewEdit, _parent, this.gameObject, _task);
        }

        private void OnDeleteTask()
        {
            UIEventController.Instance.DispatchUIEvent(EventItemTaskViewDelete, _parent, this.gameObject, _task);
        }

        private void OnBtnWorkingExit(CustomButton value)
        {
            UIEventController.Instance.DispatchUIEvent(ScreenTaskManagerView.EventScreenTaskManagerViewRefreshMembersWorking, -1);
        }

        private void OnBtnWorkingEnter(CustomButton value)
        {
            UIEventController.Instance.DispatchUIEvent(ScreenTaskManagerView.EventScreenTaskManagerViewRefreshMembersWorking, _task.UID);
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventItemTaskViewSelected))
            {
                if ((GameObject)parameters[0] == _parent)
                {
                    if ((GameObject)parameters[1] != this.gameObject)
                    {
                        Selected = false;
                    }
                }
            }
            if (nameEvent.Equals(EventItemTaskViewRefresh))
            {
                if (_task == (TaskItemData)parameters[0])
                {
                    RenderPointsMembers();
                }
            }
            if (nameEvent.Equals(EventItemTaskViewAllRefresher))
            {
                RenderPointsMembers();
            }
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(TasksController.EventTasksControllerResponseTask))
            {
                int taskUID = (int)parameters[0];
                bool isInProgress = (bool)parameters[1];
                if (_task.UID == taskUID)
                {
                    IsWorking = isInProgress;
                    if (IsWorking)
                    {
                        _humansWorkingInTask = (List<string>)parameters[2];
                    }
                }
            }
        }

        private void Update()
        {
            if (IsWorking)
            {
                _iconWorking.transform.localEulerAngles += new Vector3(0, 0, 90 * Time.deltaTime);
            }
        }
    }
}