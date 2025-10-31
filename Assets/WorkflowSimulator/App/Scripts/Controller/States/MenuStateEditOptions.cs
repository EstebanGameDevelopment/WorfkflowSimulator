using yourvrexperience.Utils;
using System;
using yourvrexperience.UserManagement;
using System.Collections.Generic;

namespace yourvrexperience.WorkDay
{
	public class MenuStateEditOptions : IBasicState
	{
		public const string EventGameStateEditProjectRequestProjectList = "EventGameStateEditProjectRequestProjectList";
		public const string EventGameStateEditProjectResponseProjectsList = "EventGameStateEditProjectResponseProjectsList";

		public enum GameEditStoryStates { MainOption = 0, NameStory, ListStories, Processing, Null }

		private GameEditStoryStates _state = GameEditStoryStates.Null;

		private int _idProjectIndex;
		private int _idProjectData;
		private ProjectSlot _slotSelected;

		private string _companyName;
		private string _companyDescription;
		private string _projectData;
		private bool _enableBreaks;
		private bool _enableInterruptions;

		private bool _isProjectNew = false;
		private bool _reloadProjectAfterDelete = false;
		private bool _initialLoadSlots = false;
		private List<ProjectEntryIndex> _projectsList;

		private int _startingHour;
		private int _lunchHour;
		private int _endHour;
		private DayOfWeek _weekend;

		private string _bookData;

		public void Initialize()
		{
			SystemEventController.Instance.Event += OnSystemEvent;
			UIEventController.Instance.Event += OnUIEvent;

			ScreenController.Instance.CreateScreen(ScreenMainOptionsView.ScreenName, true, false);

			string titleList = LanguageController.Instance.GetText("text.info");
			string descriptionList = LanguageController.Instance.GetText("screen.gamestateeditoptions.now.loading.projects");
			ScreenController.Instance.CreateScreen(ScreenWaitProgressView.ScreenName, false, false, titleList, descriptionList);
			WorkDayData.Instance.ConsultUserProjects((int)UsersController.Instance.CurrentUser.Id);
			_state = GameEditStoryStates.MainOption;
		}

		public void Destroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
		}

		private void CompleteSuccessLoadProjects()
		{
            UIEventController.Instance.DispatchUIEvent(ScreenWaitProgressView.EventScreenWaitProgressViewDestroy);
            if (ApplicationController.Instance.IsPlayMode)
            {
                _isProjectNew = false;
                _state = GameEditStoryStates.ListStories;
                _slotSelected = null;
                ScreenController.Instance.CreateScreen(ScreenListProjectsView.ScreenName, true, false, _projectsList);
            }
        }

        private void CompleteLoadGame()
        {			
			if (_isProjectNew)
			{
				WorkDayData.Instance.CurrentProject.Language = LanguageController.Instance.CodeLanguage;
				if (_slotSelected != null)
				{
					WorkDayData.Instance.CurrentProject.SetLevel(_slotSelected.Level);
					_slotSelected.Project = WorkDayData.Instance.CurrentIndexProject;
					WorkDayData.Instance.UpdateProjectSlot(_slotSelected.Id, _slotSelected.Project);
				}
				else
				{
					LoadGame();
				}
			}
			else
			{
				LanguageController.Instance.ChangeLanguage(WorkDayData.Instance.CurrentProject.Language);
				LoadGame();
			}
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
        {
			if (nameEvent.Equals(ScreenMainOptionsView.EventScreenMainOptionsViewCreate))
            {
				_isProjectNew = true;
				ScreenController.Instance.CreateScreen(ScreenSlotsManagementView.ScreenName, false, false, true);
			}
			if (nameEvent.Equals(ScreenSlotsManagementView.EventScreenSlotsManagementViewSelectedSlot))
            {
				_slotSelected = (ProjectSlot)parameters[0];
				_state = GameEditStoryStates.NameStory;
				ScreenController.Instance.CreateScreen(ScreenNameWorkDayView.ScreenName, true, false);
			}
			if (nameEvent.Equals(ScreenMainOptionsView.EventScreenMainOptionsViewEdit))
			{
				_isProjectNew = false;
                _state = GameEditStoryStates.ListStories;
                _slotSelected = null;
                ScreenController.Instance.CreateScreen(ScreenListProjectsView.ScreenName, true, false, _projectsList);
            }
			if (nameEvent.Equals(ScreenMainOptionsView.EventScreenMainOptionsViewBack))
			{
				ApplicationController.Instance.ChangeGameState(ApplicationController.StatesGame.MainMenu);
			}
			if (nameEvent.Equals(ScreenNameWorkDayView.EventScreenNameWorkDayViewCreate))
			{
				if (_state == GameEditStoryStates.NameStory)
				{
					bool aiGeneration = (bool)parameters[0];
					_companyName = (string)parameters[1];
					_companyDescription = (string)parameters[2];
					_enableBreaks = (bool)parameters[3];
					_enableInterruptions = (bool)parameters[4];
					_startingHour = (int)parameters[5];
					_lunchHour = (int)parameters[6];
					_endHour = (int)parameters[7];
					_weekend = (DayOfWeek)parameters[8];
					if (aiGeneration)
					{
						ProjectData newProject = new ProjectData(_companyName, "", _companyDescription, _enableBreaks, _enableInterruptions, _startingHour, _lunchHour, _endHour, _weekend);
						newProject.Language = LanguageController.Instance.CodeLanguage;
						_projectData = WorkDayData.Instance.SerializeProject(newProject);
						WorkDayData.Instance.NameWorkDayProject = _companyName;
						string titleCreate = LanguageController.Instance.GetText("text.info");
						string descriptionCreate = LanguageController.Instance.GetText("screen.gamestateeditoptions.now.creating.project");
						ScreenController.Instance.CreateScreen(ScreenWaitProgressView.ScreenName, false, false, titleCreate, descriptionCreate);
						WorkDayData.Instance.UpdateProjectIndex(-1, (int)UsersController.Instance.CurrentUser.Id, _slotSelected.Id, _companyName, _companyDescription, 0, 1, 2);
					}
					else
					{
						ApplicationController.Instance.TeamCompany = (TeamCompanyListJSON)parameters[9];
						string titleCreate = LanguageController.Instance.GetText("text.info");
						string descriptionCreate = LanguageController.Instance.GetText("screen.gamestateeditoptions.now.creating.project");
						ScreenController.Instance.CreateScreen(ScreenWaitProgressView.ScreenName, false, false, titleCreate, descriptionCreate);
						WorkDayData.Instance.DownloadReferenceData(ApplicationController.Instance.TeamCompany.employees.Count);
					}
				}
			}
			if (nameEvent.Equals(ScreenListProjectsView.EventScreenListProjectsViewClose))
            {
				if (ApplicationController.Instance.IsPlayMode)
				{
					ApplicationController.Instance.ChangeGameState(ApplicationController.StatesGame.MainMenu);
				}
				else
                {
					ScreenController.Instance.CreateScreen(ScreenMainOptionsView.ScreenName, true, false);
					_state = GameEditStoryStates.MainOption;
				}
			}
			if (nameEvent.Equals(ScreenNameWorkDayView.EventScreenNameWorkDayViewBack))
            {
				if (ApplicationController.Instance.IsPlayMode)
                {
					ApplicationController.Instance.ChangeGameState(ApplicationController.StatesGame.MainMenu);
				}
				else
                {
					ScreenController.Instance.CreateScreen(ScreenMainOptionsView.ScreenName, true, false);
					_state = GameEditStoryStates.MainOption;
				}
			}
			if (nameEvent.Equals(ScreenListProjectsView.EventScreenListProjectsViewSelectProject))
			{
				if (_state == GameEditStoryStates.ListStories)
                {
					ProjectEntryIndex selectedStory = (ProjectEntryIndex)parameters[0];
					string titleCreate = LanguageController.Instance.GetText("text.info");
					string descriptionCreate = LanguageController.Instance.GetText("screen.gamestateeditoptions.now.loading.selected.project");
					ScreenController.Instance.CreateScreen(ScreenWaitProgressView.ScreenName, false, false, titleCreate, descriptionCreate);
					WorkDayData.Instance.DownloadStoryData(selectedStory.Id, selectedStory.Title);
				}
			}
			if (nameEvent.Equals(ScreenSelectRolView.EventScreenSelectRolViewSelectedProfile))
            {
				if ((bool)parameters[0])
                {
                    WorldItemData humanSelected = (WorldItemData)parameters[1];
					WorkDayData.Instance.CurrentProject.SetHumanControlled(humanSelected, true);
					string titleCreate = LanguageController.Instance.GetText("text.info");
					string descriptionCreate = LanguageController.Instance.GetText("screen.gamestateeditoptions.now.loading.selected.project");
					ScreenController.Instance.CreateScreen(ScreenWaitProgressView.ScreenName, false, false, titleCreate, descriptionCreate);
					CompleteLoadGame();
                }
				else
                {
					SystemEventController.Instance.DispatchSystemEvent(ScreenWaitProgressView.EventScreenWaitProgressViewDestroy);
				}
            }
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(CheckoutController.EventCheckoutControllerDownloadedSlotsConfirmation))
			{
				UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
				WorkDayData.Instance.ConsultUserProjects((int)UsersController.Instance.CurrentUser.Id);
			}
			if (nameEvent.Equals(EventGameStateEditProjectRequestProjectList))
			{
				SystemEventController.Instance.DispatchSystemEvent(EventGameStateEditProjectResponseProjectsList, _projectsList);
			}
			if (nameEvent.Equals(DownloadReferenceDataHTTP.EventDownloadReferenceDataHTTPCompleted))
			{
				if (_state == GameEditStoryStates.NameStory)
                {
					if ((bool)parameters[0])
					{
						string jsonReference = (string)parameters[1];
						ProjectData newProject = WorkDayData.Instance.DeserializeProject(jsonReference);
						newProject.Language = LanguageController.Instance.CodeLanguage;
						newProject.ProjectName = "";
						newProject.Company = _companyDescription;
						newProject.EnableBreaks = _enableBreaks;
						newProject.EnableInterruptions = _enableInterruptions;
						newProject.StartingHour = _startingHour;
						newProject.LunchHour = _lunchHour;
						newProject.EndingHour = _endHour;
						newProject.EndingDayOfWeek = _weekend;
						newProject.InitCurrentTime();
						_projectData = WorkDayData.Instance.SerializeProject(newProject);
						WorkDayData.Instance.NameWorkDayProject = _companyName;
						WorkDayData.Instance.UpdateProjectIndex(-1, (int)UsersController.Instance.CurrentUser.Id, -1, _companyName, _companyDescription, 0, 1, 2);
					}
					else
					{
						UIEventController.Instance.DelayUIEvent(ScreenWaitProgressView.EventScreenWaitProgressViewSetText, 0.2f, LanguageController.Instance.GetText("screen.creating.new.office.title"), LanguageController.Instance.GetText("screen.creating.new.office.error"));
					}
				}
			}
			if (nameEvent.Equals(UpdateProjectIndexHTTP.EventUpdateProjectIndexHTTPCompleted))
			{
				if (_state == GameEditStoryStates.NameStory)
				{
					if ((bool)parameters[0])
					{
						// NEW STORY
						_idProjectIndex = (int)parameters[1];
						_idProjectData = (int)parameters[2];
						WorkDayData.Instance.UpdateProjectData(_idProjectData, _projectData);
					}
					else
					{
						UIEventController.Instance.DelayUIEvent(ScreenWaitProgressView.EventScreenWaitProgressViewSetText, 0.2f, LanguageController.Instance.GetText("screen.creating.new.office.title"), LanguageController.Instance.GetText("screen.creating.new.office.error"));
					}
				}
			}
			if (nameEvent.Equals(UpdateProjectDataHTTP.EventUpdateProjectDataHTTPCompleted))
			{
				if (_state == GameEditStoryStates.NameStory)
				{
					if ((bool)parameters[0])
					{
						UIEventController.Instance.DelayUIEvent(ScreenWaitProgressView.EventScreenWaitProgressViewSetText, 0.2f, LanguageController.Instance.GetText("screen.creating.new.office.title"), LanguageController.Instance.GetText("screen.creating.new.office.completed"));
						WorkDayData.Instance.DownloadStoryData(_idProjectData, _companyName);
					}
					else
					{
						UIEventController.Instance.DelayUIEvent(ScreenWaitProgressView.EventScreenWaitProgressViewSetText, 0.2f, LanguageController.Instance.GetText("screen.creating.new.office.title"), LanguageController.Instance.GetText("screen.creating.new.office.error"));
					}
				}
			}
			if (nameEvent.Equals(DownloadProjectDataHTTP.EventDownloadProjectDataHTTPCompleted))
			{
				if ((bool)parameters[0])
				{
					_bookData = (string)parameters[1];
					WorkDayData.Instance.LoadCurrentProject(_bookData);
					if (!ApplicationController.Instance.IsPlayMode)
                    {
						CompleteLoadGame();
					}
					else
                    {
						UIEventController.Instance.DispatchUIEvent(ScreenWaitProgressView.EventScreenWaitProgressViewDestroy);
						ScreenController.Instance.CreateScreen(ScreenSelectRolView.ScreenName, false, false);
					}					
				}
				else
				{
					UIEventController.Instance.DelayUIEvent(ScreenWaitProgressView.EventScreenWaitProgressViewSetText, 0.2f, LanguageController.Instance.GetText("screen.creating.new.office.title"), LanguageController.Instance.GetText("screen.creating.new.office.error"));
				}
			}
			if (nameEvent.Equals(ConsultUserProjectsHTTP.EventConsultUserProjectsHTTPCompleted))
			{
				if ((bool)parameters[0])
				{
					_projectsList = (List<ProjectEntryIndex>)parameters[1];
					UIEventController.Instance.DispatchUIEvent(ScreenListProjectsView.EventScreenListProjectsViewLoadProjects, _projectsList);
				}
                if (WorkDayData.Instance.UserSlots == null)
				{
					_initialLoadSlots = true;
                    WorkDayData.Instance.DownloadUserSlots((int)UsersController.Instance.CurrentUser.Id);
                }
				else
				{
                    CompleteSuccessLoadProjects();
                }					
			}
			if (nameEvent.Equals(UpdateProjectSlotHTTP.EventUpdateProjectSlotHTTPCompleted))
			{
				if ((bool)parameters[0])
				{
					LoadGame();
				}
				else
				{
					UIEventController.Instance.DelayUIEvent(ScreenWaitProgressView.EventScreenWaitProgressViewSetText, 0.2f, LanguageController.Instance.GetText("text.error"), LanguageController.Instance.GetText("screen.creating.slot.update.story.error"));
					SystemEventController.Instance.DelaySystemEvent(ScreenWaitProgressView.EventScreenWaitProgressViewDestroy, 3);
				}
			}
			if (nameEvent.Equals(ScreenListProjectsView.EventScreenListProjectsViewDeleteProject))
			{
				if (_state == GameEditStoryStates.ListStories)
                {
					string titleCreate = LanguageController.Instance.GetText("text.info");
					string descriptionCreate = LanguageController.Instance.GetText("screen.gamestateeditoptions.now.deleting.project");
					ScreenController.Instance.CreateScreen(ScreenWaitProgressView.ScreenName, false, false, titleCreate, descriptionCreate);
					WorkDayData.Instance.DeleteProject((int)parameters[0]);
				}
			}
			if (nameEvent.Equals(DeleteProjectDataHTTP.EventDeleteProjectDataHTTPCompleted))
            {
				if (_state == GameEditStoryStates.ListStories)
				{
					_reloadProjectAfterDelete = true;
					WorkDayData.Instance.DownloadUserSlots((int)UsersController.Instance.CurrentUser.Id);
				}
			}
			if (nameEvent.Equals(DownloadSlotsDataHTTP.EventDownloadSlotsDataHTTPCompleted))
            {
				if (_reloadProjectAfterDelete)
                {
					_reloadProjectAfterDelete = false;
					WorkDayData.Instance.ConsultUserProjects((int)UsersController.Instance.CurrentUser.Id);
					UIEventController.Instance.DelayUIEvent(ScreenWaitProgressView.EventScreenWaitProgressViewDestroy, 1);
				}
                if (_initialLoadSlots)
                {
                    _initialLoadSlots = false;
                    CompleteSuccessLoadProjects();
                }
            }
        }

		private void LoadGame()
		{
			if (ApplicationController.Instance.IsMultiplayer)
			{
				ApplicationController.Instance.ChangeGameState(ApplicationController.StatesGame.Connecting);
			}
			else
			{
				ApplicationController.Instance.NumberClients = 1;
				ApplicationController.Instance.ChangeGameState(ApplicationController.StatesGame.Loading);
			}
		}

		public void Run()
		{
		}
	}
}