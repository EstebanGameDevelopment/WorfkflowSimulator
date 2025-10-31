using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.ScreenListEventsHUDView;

namespace yourvrexperience.WorkDay
{
	public class ScreenDocsTODOView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenDocsTODOView";
		
		public const string EventScreenDocsTODOViewClosed = "EventScreenDocsTODOViewClosed";
		
		public const string SubEventScreenDocsTODOViewConfirmRegeneration = "SubEventScreenDocsTODOViewConfirmRegeneration";

		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private TextMeshProUGUI titleName;
		[SerializeField] private TextMeshProUGUI titleType;
		[SerializeField] private TextMeshProUGUI titlePeople;
		[SerializeField] private TextMeshProUGUI titleStatus;
		[SerializeField] private TextMeshProUGUI titleTime;
		[SerializeField] private TextMeshProUGUI titleDependency;
		[SerializeField] private TextMeshProUGUI titleProject;
		[SerializeField] private TMP_InputField valueName;
		[SerializeField] private TMP_InputField valueType;
		[SerializeField] private TMP_InputField valuePeople;
		[SerializeField] private TextMeshProUGUI valueStatus;
		[SerializeField] private TMP_InputField valueTime;
		[SerializeField] private TextMeshProUGUI titleTimeHours;
		[SerializeField] private TMP_Dropdown valueDependency;
		[SerializeField] private TextMeshProUGUI valueProject;
		[SerializeField] private TMP_InputField inputDescription;		
		[SerializeField] private SlotManagerView SlotManagerDocsTODO;
		[SerializeField] private GameObject PrefabDocTODO;
		[SerializeField] private Button CloseButton;
		[SerializeField] private Button AddPeopleButton;
		[SerializeField] private Button AIGenerateDocsButton;

		private List<CurrentDocumentInProgress> _docsToDo;
		private CurrentDocumentInProgress _selectedDocument;
		private List<CurrentDocumentInProgress> _orderedDocs;
		private bool _enableChangeDoc = false;
		private TaskItemData _task;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			_task = (TaskItemData)parameters[0];

			var (taskData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_task.UID);
			BoardData boardData = WorkDayData.Instance.CurrentProject.GetBoardFor(boardName);
			ProjectInfoData project = WorkDayData.Instance.CurrentProject.GetProject(boardData.ProjectId);
			_content.GetComponent<Image>().color = project.GetColor();

			LoadDocsTODO();

			UIEventController.Instance.Event += OnUIEvent;
			SystemEventController.Instance.Event += OnSystemEvent;

			CloseButton.onClick.AddListener(OnCloseButton);
			AddPeopleButton.onClick.AddListener(OnAddPeopleButton);

			titleName.gameObject.SetActive(false);
			titleType.gameObject.SetActive(false);
			valueName.gameObject.SetActive(false);
			valueType.gameObject.SetActive(false);
			titlePeople.gameObject.SetActive(false);
			valuePeople.gameObject.SetActive(false);
			titleDependency.gameObject.SetActive(false);
			valueDependency.gameObject.SetActive(false);
			titleStatus.gameObject.SetActive(false);
			valueStatus.gameObject.SetActive(false);
			titleTime.gameObject.SetActive(false);
			valueTime.gameObject.SetActive(false);
			titleTimeHours.gameObject.SetActive(false);
			titleProject.gameObject.SetActive(false);
			valueProject.gameObject.SetActive(false);
			inputDescription.gameObject.SetActive(false);
			AddPeopleButton.gameObject.SetActive(false);

			valueName.onValueChanged.AddListener(OnNameDocumentChanged);
			valueType.onValueChanged.AddListener(OnTypeDocumentChanged);
			valuePeople.onValueChanged.AddListener(OnPeopleDocumentChanged);
			valueTime.onValueChanged.AddListener(OnTimeDocumentChanged);
			inputDescription.onValueChanged.AddListener(OnDescriptionDocumentChanged);
			valueDependency.onValueChanged.AddListener(OnDependencyChanged);

            titleScreen.text = LanguageController.Instance.GetText("screen.docs.todo.title");
            titleName.text = LanguageController.Instance.GetText("text.name");
            titleType.text = LanguageController.Instance.GetText("text.type");
            titlePeople.text = LanguageController.Instance.GetText("word.assigned");
            titleStatus.text = LanguageController.Instance.GetText("text.status");
            titleTime.text = LanguageController.Instance.GetText("text.time");
            titleDependency.text = LanguageController.Instance.GetText("text.dependency");
            titleProject.text = LanguageController.Instance.GetText("text.project");

            AIGenerateDocsButton.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.calculate.current.docs.for.task");
			AIGenerateDocsButton.onClick.AddListener(CreateDocsTODO);

			bool isInteractableGeneration = true;			
			List<MeetingData> meetingsForTask = WorkDayData.Instance.CurrentProject.GetMeetingsByTaskUID(_task.UID);
            if (meetingsForTask != null)
			{
				foreach(MeetingData meeting in meetingsForTask)
				{
                    if (meeting.InProgress)
					{
                        isInteractableGeneration = false;
                    }
                }
			}
			AIGenerateDocsButton.interactable = isInteractableGeneration;

        }

        public override void Destroy()
		{
			base.Destroy();
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			_docsToDo = null;
			_orderedDocs = null;
			_selectedDocument = null;
			_task = null;

			UIEventController.Instance.DispatchUIEvent(EventScreenDocsTODOViewClosed);
		}

		private void CreateDocsTODO()
        {
			if (_docsToDo.Count > 0)
            {
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("text.do.you.want.to.delete.all.progress.docs.for.task"), SubEventScreenDocsTODOViewConfirmRegeneration);				
			}
			else
            {
				List<MeetingData> meetingsLinkedWithTask = WorkDayData.Instance.CurrentProject.GetMeetingsByTaskUID(_task.UID);
				if ((meetingsLinkedWithTask == null) || (meetingsLinkedWithTask.Count == 0))
				{
					AICommandsController.Instance.CalculateDocsTODO(_task);
				}
				else
				{
					string descriptionLinkedMeeting = LanguageController.Instance.GetText("text.cannot.start.task.because.linked.meeting", meetingsLinkedWithTask[0].Name);
					ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), descriptionLinkedMeeting);
				}
			}
		}

		private void OnCloseButton()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnAddPeopleButton()
		{
			GameObject screenGO = ScreenController.Instance.CreateScreen(ScreenListEventsHUDView.ScreenPeopleName, false, false, TypeLateralInfo.PERSONS, true);
			UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, screenGO, _canvas.sortingOrder + 1);
		}

		private void LoadDocsTODO()
        {
			if (_docsToDo != null)
            {
				_docsToDo.Clear();
			}
			if (_orderedDocs != null)
			{
				_orderedDocs.Clear();
			}
			_docsToDo = AICommandsController.Instance.ExistsDocumentsToDoForTask(_task.UID);
			List<CurrentDocumentInProgress> docsToBeDone = new List<CurrentDocumentInProgress>();
			for (int i = 0; i < _docsToDo.Count; i++)
			{
				CurrentDocumentInProgress doc = _docsToDo[i];
                AddUniqueDoc(docsToBeDone, doc);
			}
			_orderedDocs = docsToBeDone.OrderBy(d => d.Depth).ToList();

            SlotManagerDocsTODO.ClearCurrentGameObject(true);
			SlotManagerDocsTODO.Initialize(0, new List<ItemMultiObjectEntry>(), PrefabDocTODO);

			for (int i = 0; i < _orderedDocs.Count; i++)
			{
				SlotManagerDocsTODO.AddItem(new ItemMultiObjectEntry(SlotManagerDocsTODO.gameObject, SlotManagerDocsTODO.Data.Count, _orderedDocs[i]));
			}
		}

		private void ApplyUpdate()
        {
			WorkDayData.Instance.CurrentProject.AdjustEstimationTask(_task.UID);
			SlotManagerDocsTODO.ApplyGenericAction();
		}

		public void AddUniqueDoc(List<CurrentDocumentInProgress> docs, CurrentDocumentInProgress docToAdd)
        {
			bool shouldAdd = true;
			foreach(CurrentDocumentInProgress doc in docs)
            {
				if (doc.GetDocUniqueID().Equals(docToAdd.GetDocUniqueID()))
                {
					shouldAdd = false;
				}
            }
			if (shouldAdd)
            {
				docs.Add(docToAdd);
			}
        }

		private void UpdateDescriptionUniqueDoc(CurrentDocumentInProgress docTarget, string description)
		{
			int totalUpdated = 0;
			foreach (CurrentDocumentInProgress doc in _docsToDo)
			{
				if (doc.GetDocUniqueID().Equals(docTarget.GetDocUniqueID()))
				{
					doc.Description = description;
					totalUpdated++;
				}
			}
		}

		private void UpdateNameUniqueDoc(CurrentDocumentInProgress docTarget, string oldName, string name)
		{
			int totalUpdated = 0;
			foreach (CurrentDocumentInProgress doc in _docsToDo)
			{
				if (doc.GetDocUniqueID().Equals(docTarget.GetDocUniqueID()))
				{
					doc.Name = name;
					totalUpdated++;
				}
				if (doc.Dependency.Equals(oldName))
				{
					doc.Dependency = name;
					totalUpdated++;
				}
			}
		}

		private void UpdateTypeUniqueDoc(CurrentDocumentInProgress docTarget, string newType)
		{
			int totalUpdated = 0;
			foreach (CurrentDocumentInProgress doc in _docsToDo)
			{
				if (doc.GetDocUniqueID().Equals(docTarget.GetDocUniqueID()))
				{
					doc.Type = newType;
					totalUpdated++;
				}
			}
		}

		private void UpdateTimeUniqueDoc(CurrentDocumentInProgress docTarget, int newTime)
		{
			int totalUpdated = 0;
			foreach (CurrentDocumentInProgress doc in _docsToDo)
			{
				if (doc.GetDocUniqueID().Equals(docTarget.GetDocUniqueID()))
				{
					doc.Time = newTime;
					totalUpdated++;
				}
			}
		}

		private void UpdateDependencyUniqueDoc(CurrentDocumentInProgress docTarget, string dependency)
		{
			int totalUpdated = 0;
			foreach (CurrentDocumentInProgress doc in _docsToDo)
			{
				if (doc.GetDocUniqueID().Equals(docTarget.GetDocUniqueID()))
				{
					doc.Dependency = dependency;
					totalUpdated++;
				}
			}
		}

		private void UpdatePeopleUniqueDoc(CurrentDocumentInProgress docTarget, string persons)
		{
			int totalUpdated = 0;
			foreach (CurrentDocumentInProgress doc in _docsToDo)
			{
				if (doc.GetDocUniqueID().Equals(docTarget.GetDocUniqueID()))
				{
					doc.Persons = persons;
					totalUpdated++;
				}
			}
		}

		private void OnDescriptionDocumentChanged(string value)
		{
			if (!_enableChangeDoc) return;

			if (_selectedDocument != null)
            {
				_selectedDocument.Description = value;
				UpdateDescriptionUniqueDoc(_selectedDocument, value);				
			}
		}

		private void OnTimeDocumentChanged(string value)
		{
			if (!_enableChangeDoc) return;

			if (_selectedDocument != null)
			{
				_selectedDocument.Time = int.Parse(value);
				UpdateTimeUniqueDoc(_selectedDocument, int.Parse(value));
				ApplyUpdate();
			}
		}

		private void OnPeopleDocumentChanged(string value)
		{
			if (!_enableChangeDoc) return;

			if (_selectedDocument != null)
			{
				_selectedDocument.Persons = value;
				UpdatePeopleUniqueDoc(_selectedDocument, value);
				ApplyUpdate();
			}
		}

		private void OnNameDocumentChanged(string value)
		{
			if (!_enableChangeDoc) return;

			if (_selectedDocument != null)
			{
				string oldName = _selectedDocument.Name;
				_selectedDocument.Name = value;
				UpdateNameUniqueDoc(_selectedDocument, oldName, value);
				ApplyUpdate();
			}
		}

		private void OnTypeDocumentChanged(string value)
		{
			if (!_enableChangeDoc) return;

			if (_selectedDocument != null)
			{
				_selectedDocument.Type = value;
				UpdateTypeUniqueDoc(_selectedDocument, value);
				ApplyUpdate();
			}
		}
		

		private void OnDependencyChanged(int value)
		{
			if (!_enableChangeDoc) return;

			if (value == 0)
            {
				_selectedDocument.Dependency = "";
			}
			else
            {
				int finalSelection = value - 1;
				_selectedDocument.Dependency = _orderedDocs[finalSelection].Name;
				UpdateDependencyUniqueDoc(_selectedDocument, _selectedDocument.Dependency);
				ApplyUpdate();
			}
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(AICommandsController.EventAICommandsControllerEvaluateDocsToWork))
			{
                LoadDocsTODO();
				UIEventController.Instance.DispatchUIEvent(ScreenTaskManagerView.EventScreenTaskManagerViewRefreshTasks);
			}
			if (nameEvent.Equals(AICommandsController.EventAICommandsControllerReportEvaluationDone))
			{
                LoadDocsTODO();
                UIEventController.Instance.DispatchUIEvent(ScreenTaskManagerView.EventScreenTaskManagerViewRefreshTasks);
            }
        }

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(SubEventScreenDocsTODOViewConfirmRegeneration))
            {
				if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
				{
					SystemEventController.Instance.DispatchSystemEvent(AICommandsController.EventAICommandsControllerDocumentsTaskDelete, _task.UID);
				}
			}
			if (nameEvent.Equals(ScreenListEventsHUDView.EventScreenListEventsHUDViewSelectedEmployee))
            {
				valuePeople.text = valuePeople.text + "," + (string)parameters[0];
			}
			if (nameEvent.Equals(ItemDocTODOView.EventItemDocTODOViewDelete))
			{
				if ((GameObject)parameters[0] == SlotManagerDocsTODO.gameObject)
				{
					SystemEventController.Instance.DispatchSystemEvent(AICommandsController.EventAICommandsControllerDocumentTODODelete, (CurrentDocumentInProgress)parameters[2]);
					LoadDocsTODO();
				}
			}
			if (nameEvent.Equals(ItemDocTODOView.EventItemDocTODOViewSelected))
            {
				if ((GameObject)parameters[0] == SlotManagerDocsTODO.gameObject)
                {
					if ((int)parameters[2] == -1)
                    {
						titleName.gameObject.SetActive(false);
						titleType.gameObject.SetActive(false);
						valueName.gameObject.SetActive(false);
						valueType.gameObject.SetActive(false);
						titlePeople.gameObject.SetActive(false);
						valuePeople.gameObject.SetActive(false);
						titleDependency.gameObject.SetActive(false);
						valueDependency.gameObject.SetActive(false);
						inputDescription.gameObject.SetActive(false);
						titleStatus.gameObject.SetActive(false);
						valueStatus.gameObject.SetActive(false);
						titleTime.gameObject.SetActive(false);
						valueTime.gameObject.SetActive(false);
						titleTimeHours.gameObject.SetActive(false);
						titleProject.gameObject.SetActive(false);
						valueProject.gameObject.SetActive(false);
						AddPeopleButton.gameObject.SetActive(false);
						_enableChangeDoc = false;
						_selectedDocument = null;
					}
					else
                    {
						titleName.gameObject.SetActive(true);
						titleType.gameObject.SetActive(true);
						valueName.gameObject.SetActive(true);
						valueType.gameObject.SetActive(true);
						titlePeople.gameObject.SetActive(true);
						valuePeople.gameObject.SetActive(true);
						titleDependency.gameObject.SetActive(true);
						valueDependency.gameObject.SetActive(true);
						inputDescription.gameObject.SetActive(true);
						titleStatus.gameObject.SetActive(true);
						valueStatus.gameObject.SetActive(true);
						titleTimeHours.gameObject.SetActive(true);
						titleTime.gameObject.SetActive(true);
						valueTime.gameObject.SetActive(true);
						titleProject.gameObject.SetActive(true);
						valueProject.gameObject.SetActive(true);
						AddPeopleButton.gameObject.SetActive(true);

						_enableChangeDoc = false;
						_selectedDocument = (CurrentDocumentInProgress)parameters[3];
						valueName.text = _selectedDocument.Name;
						valueType.text = _selectedDocument.Type;
						valuePeople.text = _selectedDocument.Persons;

						valueDependency.ClearOptions();

						int counter = 0;
						int indexSelected = 0;
						valueDependency.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("text.no.dependencies")));
						foreach (CurrentDocumentInProgress doc in _orderedDocs)
                        {
							valueDependency.options.Add(new TMP_Dropdown.OptionData(doc.Name));
							if (_selectedDocument.Dependency != null)
                            {
								if (_selectedDocument.Dependency.Length > 0)
                                {
									if (_selectedDocument.Dependency.ToLower().Equals(doc.Name.ToLower()))
                                    {
										indexSelected = counter + 1;
									}
								}
							}
							counter++;
						}
						valueDependency.value = indexSelected;
						valueDependency.RefreshShownValue();

						inputDescription.text = _selectedDocument.Description;
						if (_selectedDocument.IsDone())
                        {
							valueStatus.text = LanguageController.Instance.GetText("word.done");
						}
						else
                        {
							if (_selectedDocument.Working)
                            {
								valueStatus.text = LanguageController.Instance.GetText("word.working");
							}
							else
                            {
								valueStatus.text = LanguageController.Instance.GetText("word.todo");
							}
						}
						valueTime.text = _selectedDocument.Time.ToString();
						titleTimeHours.text = LanguageController.Instance.GetText("word.hours");
						valueProject.text = WorkDayData.Instance.CurrentProject.GetProject(_selectedDocument.ProjectID).Name;
						inputDescription.interactable = !_selectedDocument.IsDone();
						_enableChangeDoc = true;
					}
				}
			}
		}
    }
}