using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.ScreenAnalysisHumanView;

namespace yourvrexperience.WorkDay
{
    public class ItemHumanContributionView : MonoBehaviour, ISlotView
    {
        public const string EventItemHumanContributionViewSelected = "EventItemHumanContributionViewSelected";
        
        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private Image _iconMeeting;
        private Image _iconDoc;
        private Image _iconTask;
        private bool _selected = false;
        private ContributionHuman _contribution;
        private Color _defaultColor;

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
            _contribution = (ContributionHuman)((ItemMultiObjectEntry)parameters[0]).Objects[2];

            _iconMeeting = transform.Find("Icon/Meeting").GetComponent<Image>();
            _iconDoc = transform.Find("Icon/Doc").GetComponent<Image>();
            _iconTask = transform.Find("Icon/Task").GetComponent<Image>();
            ProjectInfoData project = WorkDayData.Instance.CurrentProject.GetProject(_contribution.ProjectID);

            _iconMeeting.gameObject.SetActive(false);
            _iconDoc.gameObject.SetActive(false);
            _iconTask.gameObject.SetActive(false);

            string textLabel = "";
            switch (_contribution.Type)
            {
                case ContributionType.Meeting:
                    MeetingData meeting = WorkDayData.Instance.CurrentProject.GetMeetingByUID(_contribution.UID);
                    textLabel = Utilities.ShortenText(meeting.Name, 30) + "\n<b>" + LanguageController.Instance.GetText("word.meeting") + "</b>\n";
                    if (project != null)
                    {
                        textLabel += Utilities.ShortenText(project.Name, 30);
                    }                    
                    _iconMeeting.gameObject.SetActive(true);                    
                    break;

                case ContributionType.Document:
                    DocumentData docGlobal = WorkDayData.Instance.CurrentProject.GetDocumentByID(int.Parse(_contribution.UID));
                    textLabel = Utilities.ShortenText(docGlobal.Name, 30) + "\n<b>" + LanguageController.Instance.GetText("word.document") + "</b>\n";
                    if (project != null)
                    {
                        textLabel += Utilities.ShortenText(project.Name, 30);
                    }
                    _iconDoc.gameObject.SetActive(true);
                    break;

                case ContributionType.Task:
                    var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_contribution.TaskUID);
                    if (taskItemData != null)
                    {
                        List<DocumentData> docs = taskItemData.GetData();
                        foreach (DocumentData doc in docs)
                        {
                            if (doc.Id.ToString() == _contribution.UID)
                            {
                                textLabel = Utilities.ShortenText(doc.Name, 30) + "\n<b>" + LanguageController.Instance.GetText("word.task") + "</b>\n";
                                if (project != null)
                                {
                                    textLabel += Utilities.ShortenText(project.Name, 30);
                                }
                            }
                        }
                    }
                    _iconTask.gameObject.SetActive(true);
                    break;
            }
            transform.Find("Text").GetComponent<TextMeshProUGUI>().text = textLabel;

            _background = transform.GetComponent<Image>();
            transform.GetComponent<Button>().onClick.AddListener(ButtonPressed);
            
            if (project != null)
            {
                _defaultColor = project.GetColor();
            }            
            _background.color = _defaultColor;

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
            UIEventController.Instance.DispatchUIEvent(EventItemHumanContributionViewSelected, _parent, this.gameObject, (Selected ? _index : -1), _contribution);
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {            
            if (nameEvent.Equals(EventItemHumanContributionViewSelected))
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