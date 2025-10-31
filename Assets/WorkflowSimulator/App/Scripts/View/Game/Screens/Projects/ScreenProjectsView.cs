using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class ScreenProjectsView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenProjectsView";
		
		public const string EventScreenProjectsViewLoadProject = "EventScreenProjectsViewLoadProject";
		public const string EventScreenProjectsViewRefreshProjects = "EventScreenProjectsViewRefreshProjects";
		public const string EventScreenProjectsViewGeneration = "EventScreenProjectsViewGeneration";

		public const string SubEventScreenProjectsViewDeleteConfirmation = "SubEventScreenProjectsViewDeleteConfirmation";

		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private TextMeshProUGUI titleName;
		[SerializeField] private TextMeshProUGUI titleDescription;
		[SerializeField] private TextMeshProUGUI feedback;

		[SerializeField] private CustomInput inputNameProject;
		[SerializeField] private CustomInput inputDescriptionProject;

		[SerializeField] private Button buttonAdd;
		[SerializeField] private Button buttonNew;
		[SerializeField] private Button buttonLoadProject;
		[SerializeField] private Button buttonGenerateProject;
		[SerializeField] private Button buttonClose;

		[SerializeField] private SlotManagerView SlotManagerProjects;
		[SerializeField] private GameObject PrefabProject;

		[SerializeField] private IconColorProjectView iconColorProject;
		[SerializeField] private Button btnColorProject;

		[SerializeField] private TextMeshProUGUI titleTimeStart;
		[SerializeField] private TextMeshProUGUI titleTimeLunch;
		[SerializeField] private TextMeshProUGUI titleTimeEnd;
		[SerializeField] private TextMeshProUGUI titleWeekend;

		[SerializeField] private TimePicker pickTimeStart;
		[SerializeField] private TimePicker pickTimeLunch;
		[SerializeField] private TimePicker pickTimeEnd;
		[SerializeField] private CustomDropdown DropDownWeekend;

		[SerializeField] private CustomToggle toggleEnableBreaks;
		[SerializeField] private CustomToggle toggleEnableInterruptions;

		[SerializeField] private CustomButton btnStartDay;
		[SerializeField] private CustomButton btnLunchTime;
		[SerializeField] private CustomButton btnEndDay;

		private ProjectInfoData _currentProject;
		private List<ProjectInfoData> _projects;
		private string _initialProjectName;
		private ProjectInfoData _projectToDelete;

		private Color _colorSelectedProject;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			buttonClose.onClick.AddListener(OnClose);

			buttonAdd.onClick.AddListener(OnAddProject);
			buttonNew.onClick.AddListener(OnNewProject);
			buttonLoadProject.onClick.AddListener(OnLoadProject);
			buttonGenerateProject.onClick.AddListener(OnGenerateProject);
			btnColorProject.onClick.AddListener(OnColorProject);
			
			inputNameProject.text = "";
			inputDescriptionProject.text = "";
			buttonAdd.interactable = false;

			inputNameProject.onValueChanged.AddListener(OnNameProjectChanged);
			inputDescriptionProject.onValueChanged.AddListener(OnDescriptionProjectChanged);

			UIEventController.Instance.Event += OnUIEvent;
			
			LoadProjectsInfo(true);
			
			pickTimeStart.ChangeCurrentSelectedHour(WorkDayData.Instance.CurrentProject.StartingHour);
			pickTimeStart.ChangeCurrentSelectedMinute(0);
			pickTimeStart.IsAM = (WorkDayData.Instance.CurrentProject.StartingHour <= 12);
			pickTimeStart.UpdateTimeDisplayed();
			pickTimeStart.SelectHour(48 - ((pickTimeStart.CurrentSelectedHour % 12) * 30));
			pickTimeStart.SelectMinute(48 - (((float)pickTimeStart.CurrentSelectedMinute / 5) * 30f));
			pickTimeStart.OnTimeSelected.AddListener(OnStartTimeSelect);

			pickTimeLunch.ChangeCurrentSelectedHour(WorkDayData.Instance.CurrentProject.LunchHour);
			pickTimeLunch.ChangeCurrentSelectedMinute(0);
			pickTimeLunch.IsAM = (WorkDayData.Instance.CurrentProject.LunchHour <= 12);
			pickTimeLunch.UpdateTimeDisplayed();
			pickTimeLunch.SelectHour(48 - ((pickTimeLunch.CurrentSelectedHour % 12) * 30));
			pickTimeLunch.SelectMinute(48 - (((float)pickTimeLunch.CurrentSelectedMinute / 5) * 30f));
			pickTimeLunch.OnTimeSelected.AddListener(OnLunchTimeSelect);

			pickTimeEnd.ChangeCurrentSelectedHour(WorkDayData.Instance.CurrentProject.EndingHour);
			pickTimeEnd.ChangeCurrentSelectedMinute(0);
			pickTimeEnd.IsAM = (WorkDayData.Instance.CurrentProject.EndingHour <= 12);
			pickTimeEnd.UpdateTimeDisplayed();
			pickTimeEnd.SelectHour(48 - ((pickTimeEnd.CurrentSelectedHour % 12) * 30));
			pickTimeEnd.SelectMinute(48 - (((float)pickTimeEnd.CurrentSelectedMinute / 5) * 30f));
			pickTimeEnd.OnTimeSelected.AddListener(OnEndTimeSelect);

			DropDownWeekend.ClearOptions();
			int selectionWeekend = 0;
			DropDownWeekend.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("day.week." + DayOfWeek.Monday.ToString().ToLower())));
			if (DayOfWeek.Monday == WorkDayData.Instance.CurrentProject.EndingDayOfWeek) selectionWeekend = 0;

			DropDownWeekend.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("day.week." + DayOfWeek.Tuesday.ToString().ToLower())));
			if (DayOfWeek.Tuesday == WorkDayData.Instance.CurrentProject.EndingDayOfWeek) selectionWeekend = 1;

			DropDownWeekend.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("day.week." + DayOfWeek.Wednesday.ToString().ToLower())));
			if (DayOfWeek.Wednesday == WorkDayData.Instance.CurrentProject.EndingDayOfWeek) selectionWeekend = 2;

			DropDownWeekend.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("day.week." + DayOfWeek.Thursday.ToString().ToLower())));
			if (DayOfWeek.Thursday == WorkDayData.Instance.CurrentProject.EndingDayOfWeek) selectionWeekend = 3;

			DropDownWeekend.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("day.week." + DayOfWeek.Friday.ToString().ToLower())));
			if (DayOfWeek.Friday == WorkDayData.Instance.CurrentProject.EndingDayOfWeek) selectionWeekend = 4;

			DropDownWeekend.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("day.week." + DayOfWeek.Saturday.ToString().ToLower())));
			if (DayOfWeek.Saturday == WorkDayData.Instance.CurrentProject.EndingDayOfWeek) selectionWeekend = 5;

			DropDownWeekend.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("day.week." + DayOfWeek.Sunday.ToString().ToLower())));
			if (DayOfWeek.Sunday == WorkDayData.Instance.CurrentProject.EndingDayOfWeek) selectionWeekend = 6;

			DropDownWeekend.value = selectionWeekend;
			DropDownWeekend.onValueChanged.AddListener(OnWeekendDayChanged);

			DropDownWeekend.PointerEnterButton += OnWeekendEnter;
			DropDownWeekend.PointerExitButton += OnWeekendExit;

			toggleEnableBreaks.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.create.new.company.toggle.enable.breaks");
			toggleEnableBreaks.isOn = WorkDayData.Instance.CurrentProject.EnableBreaks;
			toggleEnableBreaks.onValueChanged.AddListener(OnEnableBreaks);

			toggleEnableInterruptions.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.create.new.company.toggle.enable.interruptions");
			toggleEnableInterruptions.isOn = WorkDayData.Instance.CurrentProject.EnableInterruptions;
			toggleEnableInterruptions.onValueChanged.AddListener(OnEnableInterruptions);

			toggleEnableBreaks.PointerEnterButton += OnToogleEnableBreaksEnter;
			toggleEnableBreaks.PointerExitButton += OnFeedbackReset;

			toggleEnableInterruptions.PointerEnterButton += OnToogleEnableInterruptionsEnter;
			toggleEnableInterruptions.PointerExitButton += OnFeedbackReset;

			btnStartDay.PointerEnterButton += OnEnterStartDay;
			btnStartDay.PointerExitButton += OnExitFeedback;

			btnLunchTime.PointerEnterButton += OnEnterLunchTime;
			btnLunchTime.PointerExitButton += OnExitFeedback;

			btnEndDay.PointerEnterButton += OnEnterEndDay;
			btnEndDay.PointerExitButton += OnExitFeedback;

			SystemEventController.Instance.DispatchSystemEvent(PlayerView.EventPlayerAppEnableMovement, false);

            titleScreen.text = LanguageController.Instance.GetText("screen.project.title");
            titleName.text = LanguageController.Instance.GetText("screen.project.name");
            titleDescription.text = LanguageController.Instance.GetText("screen.project.description");

            buttonLoadProject.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.project.load.project");
            buttonGenerateProject.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.project.generate.project");

            titleTimeStart.text = LanguageController.Instance.GetText("screen.project.start.hour");
            titleTimeLunch.text = LanguageController.Instance.GetText("screen.project.lunch.hour");
            titleTimeEnd.text = LanguageController.Instance.GetText("screen.project.end.hour");
            titleWeekend.text = LanguageController.Instance.GetText("screen.project.day.weekend");

            if (ApplicationController.Instance.IsPlayMode)
			{
				buttonGenerateProject.interactable = false;

				inputNameProject.interactable = false;
				inputDescriptionProject.interactable = false;

				buttonAdd.interactable = false;
				buttonNew.interactable = false;
				btnColorProject.interactable = false;

				DropDownWeekend.interactable = false;

				toggleEnableBreaks.interactable = false;
				toggleEnableInterruptions.interactable = false;

				btnStartDay.interactable = false;
				btnLunchTime.interactable = false;
				btnEndDay.interactable = false;
			}
		}


        public override void Destroy()
		{
			base.Destroy();

			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
			_currentProject = null;
			if (_projects != null)
			{
				_projects.Clear();
			}
			_projects = null;
			_projectToDelete = null;

			SystemEventController.Instance.DispatchSystemEvent(PlayerView.EventPlayerAppEnableMovement, true);
		}

		private void OnWeekendExit(CustomDropdown value)
		{
			feedback.text = "";
		}

		private void OnWeekendEnter(CustomDropdown value)
		{
			feedback.text = LanguageController.Instance.GetText("screen.create.new.company.description.weekend.starts");
		}

		private void OnEnterEndDay(CustomButton value)
		{
			feedback.text = LanguageController.Instance.GetText("screen.create.new.company.description.day.end");
		}

		private void OnEnterLunchTime(CustomButton value)
		{
			feedback.text = LanguageController.Instance.GetText("screen.create.new.company.description.lunch.time");
		}

		private void OnEnterStartDay(CustomButton value)
		{
			feedback.text = LanguageController.Instance.GetText("screen.create.new.company.description.start.day");
		}

		private void OnExitFeedback(CustomButton value)
		{
			feedback.text = "";
		}

		private void OnToogleEnableBreaksEnter(CustomToggle value)
		{
			feedback.text = LanguageController.Instance.GetText("screen.create.new.company.description.enable.breaks");
		}

		private void OnToogleEnableInterruptionsEnter(CustomToggle value)
		{
			feedback.text = LanguageController.Instance.GetText("screen.create.new.company.description.enable.interruptions");
		}

		private void OnFeedbackReset(CustomToggle value)
		{
			feedback.text = "";
		}

		private void OnEnableInterruptions(bool value)
		{
			WorkDayData.Instance.CurrentProject.EnableInterruptions = value;
		}

		private void OnEnableBreaks(bool value)
		{
			WorkDayData.Instance.CurrentProject.EnableBreaks = value;
		}

		private void OnGenerateProject()
		{
			AICommandsController.Instance.AddNewAICommand(new AICommandGenerateProject(), true, EventScreenProjectsViewGeneration);
		}

		private void OnStartTimeSelect()
		{
			if (pickTimeEnd.CurrentSelectedHour - pickTimeStart.CurrentSelectedHour < 2)
            {
				pickTimeStart.IsAM = true;
				pickTimeStart.ChangeCurrentSelectedHour(WorkDayData.Instance.CurrentProject.StartingHour);
				pickTimeStart.ChangeCurrentSelectedMinute(0);
				pickTimeStart.IsAM = (WorkDayData.Instance.CurrentProject.StartingHour <= 12);
				pickTimeStart.SelectHour(48 - ((pickTimeStart.CurrentSelectedHour % 12) * 30));
				pickTimeStart.SelectMinute(48 - (((float)pickTimeStart.CurrentSelectedMinute / 5) * 30f));
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.projects.there.should.be.a.difference.of.2.hours"));
            }			
			else
            {
				WorkDayData.Instance.CurrentProject.StartingHour = pickTimeStart.CurrentSelectedHour;
			}			
			pickTimeStart.UpdateTimeDisplayed();
		}

		private void OnLunchTimeSelect()
		{
			if ((pickTimeEnd.CurrentSelectedHour - pickTimeLunch.CurrentSelectedHour < 2) 
				|| (pickTimeLunch.CurrentSelectedHour - pickTimeStart.CurrentSelectedHour  < 2))
			{
				pickTimeLunch.IsAM = true;
				pickTimeLunch.ChangeCurrentSelectedHour(WorkDayData.Instance.CurrentProject.LunchHour);
				pickTimeLunch.ChangeCurrentSelectedMinute(0);
				pickTimeLunch.IsAM = (WorkDayData.Instance.CurrentProject.LunchHour <= 12);
				pickTimeLunch.SelectHour(48 - ((pickTimeLunch.CurrentSelectedHour % 12) * 30));
				pickTimeLunch.SelectMinute(48 - (((float)pickTimeLunch.CurrentSelectedMinute / 5) * 30f));
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.projects.lunch.time.should.be.in.the.right.hours"));
			}
			else
			{
				WorkDayData.Instance.CurrentProject.LunchHour = pickTimeLunch.CurrentSelectedHour;
			}
			pickTimeLunch.UpdateTimeDisplayed();
		}


		private void OnEndTimeSelect()
		{
			if (pickTimeEnd.CurrentSelectedHour - pickTimeStart.CurrentSelectedHour < 2)
			{
				pickTimeEnd.IsAM = true;
				pickTimeEnd.ChangeCurrentSelectedHour(WorkDayData.Instance.CurrentProject.EndingHour);
				pickTimeEnd.ChangeCurrentSelectedMinute(0);
				pickTimeEnd.IsAM = (WorkDayData.Instance.CurrentProject.EndingHour <= 12);
				pickTimeEnd.SelectHour(48 - ((pickTimeEnd.CurrentSelectedHour % 12) * 30));
				pickTimeEnd.SelectMinute(48 - (((float)pickTimeEnd.CurrentSelectedMinute / 5) * 30f));
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.projects.there.should.be.a.difference.of.2.hours"));
			}
			else
			{
				WorkDayData.Instance.CurrentProject.EndingHour = pickTimeEnd.CurrentSelectedHour;
			}			
			pickTimeEnd.UpdateTimeDisplayed();
		}

		private void OnWeekendDayChanged(int value)
		{
			switch (value)
            {
				case 0:
					WorkDayData.Instance.CurrentProject.EndingDayOfWeek = DayOfWeek.Monday;
					break;
				case 1:
					WorkDayData.Instance.CurrentProject.EndingDayOfWeek = DayOfWeek.Tuesday;
					break;
				case 2:
					WorkDayData.Instance.CurrentProject.EndingDayOfWeek = DayOfWeek.Wednesday;
					break;
				case 3:
					WorkDayData.Instance.CurrentProject.EndingDayOfWeek = DayOfWeek.Thursday;
					break;
				case 4:
					WorkDayData.Instance.CurrentProject.EndingDayOfWeek = DayOfWeek.Friday;
					break;
				case 5:
					WorkDayData.Instance.CurrentProject.EndingDayOfWeek = DayOfWeek.Saturday;
					break;
				case 6:
					WorkDayData.Instance.CurrentProject.EndingDayOfWeek = DayOfWeek.Sunday;
					break;
			}
		}

		private void OnColorProject()
		{
			ScreenController.Instance.CreateScreen(ScreenColorPickerView.ScreenName, false, false);
		}

		private void OnNameProjectChanged(string value)
		{
			ProjectInfoData tmpProject = new ProjectInfoData(-1, value, "");
			if (_currentProject == null)
			{
				buttonAdd.interactable = !_projects.Contains(tmpProject);
			}
			else
			{
				if (!_projects.Contains(tmpProject))
                {
					_currentProject.Name = value;
					UIEventController.Instance.DispatchUIEvent(ItemProjectInfoView.EventItemProjectInfoViewRefreshName, _currentProject);
				}
			}
			UpdateAddState();
		}

		private void OnDescriptionProjectChanged(string value)
		{
			if (_currentProject != null)
			{
				_currentProject.Description = value;
			}
			UpdateAddState();
		}

		private void LoadProjectsInfo(bool selectItem)
		{
			_projects = WorkDayData.Instance.CurrentProject.GetProjects();
			SlotManagerProjects.ClearCurrentGameObject(true);
			SlotManagerProjects.Initialize(0, new List<ItemMultiObjectEntry>(), PrefabProject);

			for (int i = 0; i < _projects.Count; i++)
			{
				SlotManagerProjects.AddItem(new ItemMultiObjectEntry(SlotManagerProjects.gameObject, SlotManagerProjects.Data.Count, _projects[i]));
			}
			// slotManager.SetVerticalScroll(0);

			if (selectItem)
            {
				if (_projects.Count > 0)
                {
					UIEventController.Instance.DispatchUIEvent(ItemProjectInfoView.EventItemProjectInfoViewForceSelection, WorkDayData.Instance.CurrentProject.ProjectInfoSelected);
				}				
            }
		}

		private void OnAddProject()
		{
			if (_currentProject == null)
            {
				if ((inputNameProject.text.Length > 0) && (inputDescriptionProject.text.Length > 0))
                {					
					string nameProject = inputNameProject.text;					
					string descriptionProject = inputDescriptionProject.text;
					ProjectInfoData newProject = new ProjectInfoData(-1, nameProject, descriptionProject);

					if (!_projects.Contains(newProject))
                    {
						SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunAddProject, nameProject, descriptionProject);
						OnNewProject();
					}
				}
			}
		}

        private void UpdateAddState()
        {
			if (_currentProject != null)
			{
				buttonAdd.interactable = false;
			}
			else
			{
				if ((inputNameProject.text.Length > 0) && (inputDescriptionProject.text.Length > 0))
				{
					string nameProject = inputNameProject.text;
					string descriptionProject = inputDescriptionProject.text;
					ProjectInfoData newProject = new ProjectInfoData(-1, nameProject, descriptionProject);
					buttonAdd.interactable = !_projects.Contains(newProject);
				}
			}
		}

        private void OnNewProject()
		{
			_currentProject = null;
			inputNameProject.text = "";
			inputDescriptionProject.text = "";			
			buttonLoadProject.interactable = false;
			buttonAdd.interactable = false;
			iconColorProject.ApplyColor(Color.white);
			UIEventController.Instance.DispatchUIEvent(ItemProjectInfoView.EventItemProjectInfoViewUnselectAll);
		}

		private void OnLoadProject()
		{			
			SystemEventController.Instance.DispatchSystemEvent(EventScreenProjectsViewLoadProject, _currentProject, true);
			OnClose();
		}

		private void OnClose()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ScreenColorPickerView.EventScreenColorPickerViewColorSelected))
            {
				Color colorProject = (Color)parameters[0];
				_colorSelectedProject = colorProject;
				iconColorProject.ApplyColor(_colorSelectedProject);
				if (_currentProject != null)
                {
					_currentProject.SetColor(_colorSelectedProject);
					UIEventController.Instance.DispatchUIEvent(ItemProjectInfoView.EventItemProjectInfoViewRefreshName, _currentProject);
				}
            }
			if (nameEvent.Equals(EventScreenProjectsViewRefreshProjects))
            {
				LoadProjectsInfo(false);
				OnNewProject();
				UpdateAddState();
			}
			if (nameEvent.Equals(ItemProjectInfoView.EventItemProjectInfoViewDelete))
			{
				_projectToDelete = (ProjectInfoData)parameters[2];
				string titleWarning = LanguageController.Instance.GetText("text.warning");
				string textAskToExit = LanguageController.Instance.GetText("screen.main.do.you.want.to.delete.project");
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, titleWarning, textAskToExit, SubEventScreenProjectsViewDeleteConfirmation);
			}
			if (nameEvent.Equals(SubEventScreenProjectsViewDeleteConfirmation))
			{
				ScreenInformationResponses userResponse = (ScreenInformationResponses)parameters[1];
				if (userResponse == ScreenInformationResponses.Confirm)
				{
					SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunDeleteProject, _projectToDelete);
				}
			}
			if (nameEvent.Equals(ItemProjectInfoView.EventItemProjectInfoViewSelected))
			{
				if ((int)parameters[2] == -1)
				{
					OnNewProject();					
				}
				else
				{
					_currentProject = (ProjectInfoData)parameters[3];
					_colorSelectedProject = _currentProject.GetColor();
					iconColorProject.ApplyInfo(_currentProject.Name, _colorSelectedProject);
					inputNameProject.text = _currentProject.Name;
					_initialProjectName = _currentProject.Name;
					inputDescriptionProject.text = _currentProject.Description;
					buttonLoadProject.interactable = true;
				}
			}
		}
	}
}