using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class ItemChatView : MonoBehaviour, ISlotView
    {
        public const string EventItemChatViewSelected = "EventItemChatViewSelected";
        public const string EventItemChatViewDelete = "EventItemChatViewDelete";

        public const string SubEventItemChatViewDataMeeting = "SubEventItemChatViewDataMeeting";

        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private MeetingData _meeting;
        private InteractionData _chat;
        private TextMeshProUGUI _textArea;
        private IconColorView _iconColor;

        private TextMeshProUGUI _referenceCalculator;

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
            _chat = (InteractionData)((ItemMultiObjectEntry)parameters[0]).Objects[2];
            _meeting = (MeetingData)((ItemMultiObjectEntry)parameters[0]).Objects[3];
            _referenceCalculator = (TextMeshProUGUI)((ItemMultiObjectEntry)parameters[0]).Objects[4];

            Transform editBtn = transform.Find("EditText");
            Transform deleteBtn = transform.Find("Delete");
            Button editEntry = null;
            if (editBtn != null)
            {
                editEntry = editBtn.GetComponent<Button>();
                editEntry.onClick.AddListener(OnEditText);
            }
            if (deleteBtn != null)
            {
                Button deleteButton = deleteBtn.GetComponent<Button>();
                deleteButton.onClick.AddListener(OnDelete);
            }
            _textArea = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            _iconColor = transform.Find("Icon").GetComponent<IconColorView>();
            _iconColor.Locked = true;
            Color memberColor = WorkDayData.Instance.CurrentProject.GetColorForMember(_chat.NameActor);
            GroupInfoData memberGroup = WorkDayData.Instance.CurrentProject.GetGroupOfMember(_chat.NameActor);
            string finalGroup = LanguageController.Instance.GetText("text.no.group");
            if (memberGroup != null)
            {
                finalGroup = memberGroup.Name;
            }

            if (editEntry != null)
            {
                if (_meeting.IsSocialMeeting())
                {
                    editEntry.gameObject.SetActive(false);
                }
            }

            string finalText = "";
            if (_chat.IsAI)
            {                
                finalText = "<color=black>" + _chat.NameActor + " ("+ finalGroup + ") " + _chat.GetDate() + "\n\n" + _chat.Text + "</color>";
            }
            else
            {
                finalText = "<color=orange>" + _chat.NameActor + " (" + finalGroup + ") " + _chat.GetDate() + "\n\n" + _chat.Text + "</color>";
            }
            _iconColor.ApplyColor(memberColor);
            _textArea.ForceMeshUpdate();
            _referenceCalculator.gameObject.SetActive(true);
            _referenceCalculator.text = finalText;
            _referenceCalculator.ForceMeshUpdate();
            Vector2 preferredValues = _referenceCalculator.GetPreferredValues(finalText, this.gameObject.GetComponent<LayoutElement>().preferredWidth, 0);
            _referenceCalculator.gameObject.SetActive(false);
            this.gameObject.GetComponent<LayoutElement>().preferredHeight = preferredValues.y + 110;
            _background = transform.GetComponent<Image>();
            _textArea.text = finalText;

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
                _chat = null;
                _meeting = null;
                _iconColor = null;
                if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ButtonPressed()
        {
            ItemSelected();
        }

        public void ItemSelected(bool dispatchEvent = true)
        {
            Selected = !Selected;
            UIEventController.Instance.DispatchUIEvent(EventItemChatViewSelected, _parent, this.gameObject, (Selected ? _index : -1), _chat);
        }

        private void OnEditText()
        {
            string chatAIDocumentsTitleName = "";
            if (WorkDayData.Instance.CurrentProject.ProjectInfoSelected != -1)
            {
                ProjectInfoData projectInfo = WorkDayData.Instance.CurrentProject.GetProject(WorkDayData.Instance.CurrentProject.ProjectInfoSelected);
                if (projectInfo != null)
                {
                    chatAIDocumentsTitleName = LanguageController.Instance.GetText("text.ai.chat.documents.for") + _meeting.Name + " (" + projectInfo.Name + ")";
                }
            }
            ScreenInformationView.CreateScreenInformation(ScreenDocumentsDataView.ScreenName, null, chatAIDocumentsTitleName, "", SubEventItemChatViewDataMeeting + _index);
            List<DocumentData> docs = new List<DocumentData>();
            if (_meeting.TaskId != -1)
            {
                var (task, boardname) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_meeting.TaskId);
                if (task != null)
                {
                    docs = task.GetData();
                }
            }
            else
            {
                docs = _meeting.GetData();
            }                
            UIEventController.Instance.DispatchUIEvent(ScreenDocumentsDataView.EventScreenDocumentsDataViewInitialization, _meeting.ProjectId, false, docs, WorkDayData.Instance.CurrentProject.GetDocuments());
            UIEventController.Instance.DispatchUIEvent(ScreenDocumentsDataView.EventScreenDocumentsDataViewSetUpDataDocument, _meeting.Name, _chat.Text, _chat.Data, _chat.Summary);
        }

        private void OnDelete()
        {            
            UIEventController.Instance.DispatchUIEvent(EventItemChatViewDelete, _parent, this.gameObject, _chat);            
        }

        public void ApplyGenericAction(params object[] parameters)
        {
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(SubEventItemChatViewDataMeeting + _index))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
                    List<DocumentData> docs = (List<DocumentData>)parameters[2];
                    if (_meeting.TaskId != -1)
                    {
                        var (task, boardname) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_meeting.TaskId);
                        if (task != null)
                        {
                            SystemEventController.Instance.DispatchSystemEvent(DocumentController.EventDocumentControllerUpdateTaskDocs, task, docs.ToArray());
                        }
                    }
                    else
                    {
                        SystemEventController.Instance.DispatchSystemEvent(DocumentController.EventDocumentControllerUpdateMeetingDocs, _meeting, docs.ToArray());
                    }
                }
            }
            if (nameEvent.Equals(EventItemChatViewSelected))
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