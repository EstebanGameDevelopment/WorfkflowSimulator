using System.Collections.Generic;
using UnityEngine;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.ApplicationController;
using static yourvrexperience.WorkDay.EditionSubStateAreas;

namespace yourvrexperience.WorkDay
{
	public class RunStateRun : IBasicState
	{
        public const string EventRunStateRunChangeState = "EventRunStateRunChangeState";        
        
        public const string EventRunStateRunAddProject = "EventRunStateRunAddProject";        
        public const string EventRunStateRunAddGroup = "EventRunStateRunAddGroup";
        public const string EventRunStateRunRemoveMemberFromGroups = "EventRunStateRunRemoveMemberFromGroups";

        public const string EventRunStateRunDeleteProject = "EventRunStateRunDeleteProject";        
        public const string EventRunStateRunSelectedProject = "EventRunStateRunSelectedProject";

        public const string EventRunStateRunDeleteGroup = "EventRunStateRunDeleteGroup";
        public const string EventRunStateRunDeleteHuman = "EventRunStateRunDeleteHuman";

        public const string EventRunStateRunAssignHumanToGroup = "EventRunStateRunAssignHumanToGroup";
        public const string EventRunStateRunUnAssignHumanToGroup = "EventRunStateRunUnAssignHumanToGroup";

        public const string SubEventExitGameConfirmation = "SubEventExitGameConfirmation";
        public const string SubEventInformationPopupSavedDisplayed = "SubEventInformationPopupSavedDisplayed";

        public enum SubStateRun { None = 0, Idle, Resize, Areas, Decoration, Avatar, Move, Play }

		private SubStateRun _state;
        private IBasicState _runState;

        public void Initialize()
		{
			SystemEventController.Instance.Event += OnSystemEvent;
            UIEventController.Instance.Event += OnUIEvent;

            UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyAllScreens);

            if (ApplicationController.Instance.IsFreeMode)
            {
                ChangeSubRunState(SubStateRun.Idle, null, Vector3.zero);
                ApplicationController.Instance.PlayerView.transform.position = WorkDayData.Instance.CurrentProject.CameraPosition;
                ApplicationController.Instance.PlayerView.transform.eulerAngles = WorkDayData.Instance.CurrentProject.CameraRotation;
                ApplicationController.Instance.PlayerView.CurrentRotation = WorkDayData.Instance.CurrentProject.ConfigurationCamera;
            }
            else
            {
                ChangeSubRunState(SubStateRun.Play, null, Vector3.zero);
            }

            ApplicationController.Instance.InstantiateHUD();
            ApplicationController.Instance.TimeHUD.Initialize();

            UIEventController.Instance.DelayUIEvent(TimeHUD.EventTimeHUDUpdateEnableInteraction, 0.1f, true);
            if (ApplicationController.Instance.TeamCompany != null)
            {
                ApplicationController.Instance.TeamCompany = null;
                UIEventController.Instance.DispatchUIEvent(TimeHUD.EventTimeHUDSaveProject);
            }
        }

        public void Destroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
            if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
        }

        private void ChangeSubRunState(SubStateRun newSubState, object data, object data2)
        {
            if (_state == newSubState)
            {
                return;
            }
            if (_runState != null)
            {
                _runState.Destroy();
            }
            _runState = null;
            _state = newSubState;

            switch (_state)
            {
                case SubStateRun.None:
                    break;

                case SubStateRun.Idle:
                    _runState = new EditionSubStateIdle();
                    break;

                case SubStateRun.Resize:
                    _runState = new EditionSubStateResize();
                    break;

                case SubStateRun.Areas:
                    _runState = new EditionSubStateAreas((AreaMode)data);
                    break;

                case SubStateRun.Decoration:
                    _runState = new EditionSubStateDecoration((AssetDefinitionItem)data);
                    break;

                case SubStateRun.Avatar:
                    _runState = new EditionSubStateAvatar((AssetDefinitionItem)data);
                    break;

                case SubStateRun.Move:
                    _runState = new EditionSubStateMove((AssetDefinitionItem)data, (WorldItemData)data2);
                    break;

                case SubStateRun.Play:
                    _runState = new EditionSubStatePlay();
                    break;
            }
            _runState.Initialize();
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
		{
            if (nameEvent.Equals(ScreenProjectsView.EventScreenProjectsViewLoadProject))
            {
                ProjectInfoData selectedProject = (ProjectInfoData)parameters[0];
                bool showConfirmation = (bool)parameters[1];
                WorkDayData.Instance.CurrentProject.ProjectInfoSelected = selectedProject.Id;
                SystemEventController.Instance.DispatchSystemEvent(EventRunStateRunSelectedProject);
                if (showConfirmation)
                {
                    ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("run.state.project.successfully.loaded"));
                }
            }
            if (nameEvent.Equals(EventRunStateRunAddProject))
            {
                string nameProject = (string)parameters[0];
                string descriptionProject = (string)parameters[1];

                List<ProjectInfoData> projects = WorkDayData.Instance.CurrentProject.GetProjects();
                ProjectInfoData newProject = new ProjectInfoData(WorkDayData.Instance.CurrentProject.GetProjectNextID(), nameProject, descriptionProject);
                if (!projects.Contains(newProject))
                {
                    projects.Add(newProject);
                    WorkDayData.Instance.CurrentProject.SetProjects(projects.ToArray());
                    UIEventController.Instance.DispatchUIEvent(ScreenProjectsView.EventScreenProjectsViewRefreshProjects);
                }
            }
            if (nameEvent.Equals(EventRunStateRunAddGroup))
            {
                string nameGroup = (string)parameters[0];
                string descriptionGroup = (string)parameters[1];
                Color colorGroup = (Color)parameters[2];

                List<GroupInfoData> groups = WorkDayData.Instance.CurrentProject.GetGroups();
                GroupInfoData newGroup = new GroupInfoData(nameGroup, descriptionGroup, colorGroup);
                if (!groups.Contains(newGroup))
                {
                    groups.Add(newGroup);
                    WorkDayData.Instance.CurrentProject.SetGroups(groups.ToArray());
                    UIEventController.Instance.DispatchUIEvent(ScreenGroupsView.EventScreenGroupsViewRefresh);
                }
            }
            if (nameEvent.Equals(EventRunStateRunRemoveMemberFromGroups))
            {                
                string nameMember = (string)parameters[0];
                List<GroupInfoData> groupsProject = WorkDayData.Instance.CurrentProject.GetGroups();
                foreach (GroupInfoData group in groupsProject)
                {
                    group.RemoveMember(nameMember);
                }
            }
            if (nameEvent.Equals(EventRunStateRunDeleteProject))
            {
                ProjectInfoData projectToDelete = (ProjectInfoData)parameters[0];
                if (WorkDayData.Instance.CurrentProject.RemoveProjectInfo(projectToDelete))
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenProjectsView.EventScreenProjectsViewRefreshProjects);
                }
            }
            if (nameEvent.Equals(EventRunStateRunDeleteGroup))
            {
                GroupInfoData groupToDelete = (GroupInfoData)parameters[0];
                if (WorkDayData.Instance.CurrentProject.RemoveGroupInfo(groupToDelete))
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenGroupsView.EventScreenGroupsViewRefresh);
                }
            }
            if (nameEvent.Equals(EventRunStateRunDeleteHuman))
            {
                string nameHuman = (string)parameters[0];
                WorkDayData.Instance.CurrentProject.DeleteHumanFromSystem(nameHuman);
            }
            if (nameEvent.Equals(EventRunStateRunChangeState))
            {
                ChangeSubRunState((SubStateRun)parameters[0], parameters[1], parameters[2]);
            }
            if (nameEvent.Equals(UpdateProjectDataHTTP.EventUpdateProjectDataHTTPCompleted))
            {
                string titleConfirmation = LanguageController.Instance.GetText("text.info");
                string textConfirmation = LanguageController.Instance.GetText("screen.run.state.confirmed.saving");
                UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformationImage, null, titleConfirmation, textConfirmation, "", "", "", ApplicationController.Instance.GetContentImage(ImagesIndex.Uploaded));
                SystemEventController.Instance.DispatchSystemEvent(SubEventInformationPopupSavedDisplayed);
            }     
            if (nameEvent.Equals(AskBaseaAddTimeSessionChatGPTHTTP.EventAskBaseaAddTimeSessionChatGPTHTTPCompleted))
            {
                _timeoutToAddSessionTime = 0;
            }
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(TimeHUD.EventTimeHUDExitGame))
            {
                string titleWarning = LanguageController.Instance.GetText("text.warning");
                string textAskToExit = LanguageController.Instance.GetText("screen.run.state.want.to.exit");
                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, titleWarning, textAskToExit, SubEventExitGameConfirmation);
            }
            if (nameEvent.Equals(SubEventExitGameConfirmation))
            {
                ScreenInformationResponses userResponse = (ScreenInformationResponses)parameters[1];
                if (userResponse == ScreenInformationResponses.Confirm)
                {
                    ApplicationController.Instance.ChangeGameState(ApplicationController.StatesGame.ReleaseMemory);
                }
            }
            if (nameEvent.Equals(TimeHUD.EventTimeHUDSaveProject))
            {
                string titleSave = LanguageController.Instance.GetText("text.info");
                string textSaving = LanguageController.Instance.GetText("screen.run.state.now.saving");
                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, null, titleSave, textSaving, "", "", "", ApplicationController.Instance.GetContentImage(ImagesIndex.Uploading));
                ApplicationController.Instance.LevelView.SaveData((parameters.Length > 0));
                WorkDayData.Instance.UpdateCurrentProjectData();
            }
            if (nameEvent.Equals(EventRunStateRunAssignHumanToGroup))
            {
                string nameHuman = (string)parameters[0];
                GroupInfoData groupTarget = (GroupInfoData)parameters[1];
                if (groupTarget != null)
                {
                    List<string> members = groupTarget.GetMembers();
                    if (!members.Contains(nameHuman))
                    {
                        SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunRemoveMemberFromGroups, nameHuman);
                        members.Add(nameHuman);
                        groupTarget.SetMembers(members);
                        SystemEventController.Instance.DispatchSystemEvent(HumanView.EventHumanViewGroupUpdated);
                    }
                }
            }
            if (nameEvent.Equals(EventRunStateRunUnAssignHumanToGroup))
            {
                string nameHuman = (string)parameters[0];
                GroupInfoData groupTarget = (GroupInfoData)parameters[1];
                List<string> members = groupTarget.GetMembers();
                if (groupTarget != null)
                {
                    if (members.Remove(nameHuman))
                    {
                        groupTarget.SetMembers(members);
                        SystemEventController.Instance.DispatchSystemEvent(HumanView.EventHumanViewGroupUpdated);
                    }
                }
            }
        }

        private float _timeoutToAddSessionTime = 0;

        public void Run()
		{
            ApplicationController.Instance.TimeHUD.Run();
            if (_runState != null)
            {
#if !SHORTCUT_LLM_SERVER
                _timeoutToAddSessionTime += Time.deltaTime;
                if (_timeoutToAddSessionTime > WorkDayData.TOTAL_TIME_SCREEN_SESSION / 2)
                {
                    _timeoutToAddSessionTime = 0;                    
                    WorkDayData.Instance.AddTimeSession();
                }
#endif

                _runState.Run();
            }
		}
	}
}