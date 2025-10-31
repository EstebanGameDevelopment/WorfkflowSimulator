using yourvrexperience.Utils;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Maything.UI.CalendarSchedulerUI;
using static yourvrexperience.WorkDay.HumanView;

namespace yourvrexperience.WorkDay
{
	public class MeetingController : MonoBehaviour
	{
		public const string EventMeetingControllerAddMeeting = "EventMeetingControllerAddMeeting";
		public const string EventMeetingControllerDeleteMeeting = "EventMeetingControllerDeleteMeeting";
		public const string EventMeetingControllerDeleteAllMeetings = "EventMeetingControllerDeleteAllMeetings";
		public const string EventMeetingControllerUpdatedMeeting = "EventMeetingControllerUpdatedMeeting";

		public const string EventMeetingControllerRunAction = "EventMeetingControllerRunAction";
		public const string EventMeetingControllerStopMeeting = "EventMeetingControllerStopMeeting";
		public const string EventMeetingControllerMeetingStopped = "EventMeetingControllerMeetingStopped";
		public const string EventMeetingControllerLeaveMeeting = "EventMeetingControllerLeaveMeeting";
		public const string EventMeetingControllerPoliteRequestToEndRunningMeetings = "EventMeetingControllerPoliteRequestToEndRunningMeetings";
		public const string EventMeetingControllerForceEndRunningSocialMeetings = "EventMeetingControllerForceEndRunningSocialMeetings";

		public const string EventMeetingControllerRequestStartMeeting = "EventMeetingControllerRequestStartMeeting";
		public const string EventMeetingControllerResponseStartMeeting = "EventMeetingControllerResponseStartMeeting";
		public const string EventMeetingControllerUIRequestToStartMeeting = "EventMeetingControllerUIRequestToStartMeeting";
		public const string EventMeetingControllerMeetingStarted = "EventMeetingControllerMeetingStarted";
		public const string EventMeetingControllerJoinMeeting = "EventMeetingControllerJoinMeeting";
		
		public const string EventMeetingControllerMeetingSummarized = "EventMeetingControllerMeetingSummarized";
		public const string EventMeetingControllerCreateScreenMeeting = "EventMeetingControllerCreateScreenMeeting";
		public const string EventMeetingControllerMeetingsLoaded = "EventMeetingControllerMeetingsLoaded";
		public const string EventMeetingControllerMeetingsRefreshData = "EventMeetingControllerMeetingsRefreshData";

		public const string SubEventMeetingControllerConfirmationAssistance = "SubEventMeetingControllerConfirmationAssistance";
		public const string SubEventMeetingControllerConfirmationDelete = "SubEventMeetingControllerConfirmationDelete";
		public const string SubEventMeetingControllerConfirmationAllDelete = "SubEventMeetingControllerConfirmationAllDelete";
		public const string SubEventMeetingControllerConfirmationStartInterruption = "SubEventMeetingControllerConfirmationStartInterruption";

		private static MeetingController _instance;

		public static MeetingController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(MeetingController)) as MeetingController;
				}
				return _instance;
			}
		}

		public class MeetingInProgress
        {
			public MeetingData Meeting;
			public bool RequestToEndMeeting;
			public List<string> Assisting;
			public List<string> Tags;

			public MeetingInProgress(MeetingData meeting)
            {
				Meeting = meeting;
				Assisting = new List<string>();
				Tags = new List<string>();
				RequestToEndMeeting = false;

				if ((Meeting.ExtraData != null) && (Meeting.ExtraData.Length > 0))
                {
					RebuildAssistants();
				}
            }

			public MeetingInProgress(MeetingData meeting, string information)
			{
				Meeting = meeting;
				Assisting = new List<string>();

				Meeting.ExtraData = information;
				RebuildAssistants();
			}

			public void Destroy()
            {
				Meeting = null;
            }

			private void RebuildAssistants()
            {
				string[] assisting = Meeting.ExtraData.Split(",");
				foreach (string assistant in assisting)
				{
					Assisting.Add(assistant);
				}
			}

			public bool AddAssistant(string member)
			{
				if (!Assisting.Contains(member))
                {
					Assisting.Add(member);
					UpdateAssistants();
					return true;
				}
				return false;
			}

			public bool RemoveAssistant(string member)
            {
				int removed = 0;
				while (Assisting.Remove(member))
                {
					removed++;
				}
                if (removed > 0)
                {
					UpdateAssistants();
					return true;
				}
				return false;
            }

			public bool ContainsAssistant(string member)
			{
				return Assisting.Contains(member);
			}

			public void UpdateAssistants()
            {
				string packet = "";
				foreach (string assistant in Assisting)
				{
					if (packet.Length > 0) packet += ",";
					packet += assistant;
				}
				Meeting.ExtraData = packet;
			}

			public string GetExtraData()
            {
				return Meeting.ExtraData;
			}
		}

		// PRIVATE MEMBERS
		private List<MeetingInProgress> _meetingsInProgress = new List<MeetingInProgress>();

		private string _nameToInterrupt = "";

		public List<MeetingInProgress> MeetingsInProgress
        {
			get { return _meetingsInProgress; }
        }

		public void Initialize()
		{
			SystemEventController.Instance.Event += OnSystemEvent;
			UIEventController.Instance.Event += OnUIEvent;
		}

        void OnDestroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
		}

		private void ClearInProgressMeetings()
        {
			foreach (MeetingInProgress meetingInProgress in _meetingsInProgress)
			{
				if (meetingInProgress != null)
				{
					meetingInProgress.Destroy();
				}
			}
			_meetingsInProgress.Clear();
		}

		private MeetingData GetMeetingOfPlayer()
        {
			foreach (MeetingInProgress meetingInProgress in _meetingsInProgress)
			{
				if (meetingInProgress.Meeting.HasPlayer(true))
				{
					return meetingInProgress.Meeting;
				}
			}
			return null;
		}

		public MeetingData GetMeetingOfHuman(string nameHuman)
		{
			foreach (MeetingInProgress meetingInProgress in _meetingsInProgress)
			{
				if (meetingInProgress.Meeting.IsAssistingMember(nameHuman))
				{
					return meetingInProgress.Meeting;
				}
			}
			return null;
		}

		private bool ContainsMeeting(MeetingData meeting)
        {
			foreach (MeetingInProgress meetingInProgress in _meetingsInProgress)
			{
				if (meetingInProgress.Meeting == meeting)
				{
					return true;
				}
			}
			return false;
		}

		private MeetingInProgress GetMeetingInProgress(MeetingData meeting)
		{
			foreach (MeetingInProgress meetingInProgress in _meetingsInProgress)
			{
				if (meetingInProgress.Meeting == meeting)
				{
					return meetingInProgress;
				}
			}
			return null;
		}

		private bool CheckMembersInRunningMeeting(MeetingData targetMeeting)
        {
			if (!ContainsMeeting(targetMeeting))
            {
				List<string> membersMeeting = targetMeeting.GetHumanMembers();
				foreach (string memberInMeeting in membersMeeting)
				{
					foreach (MeetingInProgress meetingInProgress in _meetingsInProgress)
                    {
						if (meetingInProgress.Meeting.IsMemberInMeeting(memberInMeeting))
                        {
							return true;
                        }
					}
				}
			}
			return false;
		}

		private bool RemoveMemberFromMeeting(MeetingData meeting, string nameMember)
        {			
			MeetingInProgress meetingInProgress = GetMeetingInProgress(meeting);
			if (meetingInProgress != null)
            {
				if (meetingInProgress.RemoveAssistant(nameMember))
                {
					return true;
                }
			}
			return false;
		}

		private void RequestMeetingsToSpeedUp()
        {
			foreach (MeetingInProgress meeting in _meetingsInProgress)
			{
				if (meeting != null)
				{
					if (!meeting.Meeting.HasPlayer(true))
                    {
						meeting.RequestToEndMeeting = true;
					}
				}
			}
		}

		private void RunMeeting(MeetingData meetingToRun)
		{
			MeetingInProgress meetingInProgress = GetMeetingInProgress(meetingToRun);
			if ((meetingInProgress.Assisting.Count <= 1) || (meetingToRun.Iterations > 5 * meetingToRun.TotalIterations))
			{
				SystemEventController.Instance.DispatchSystemEvent(EventMeetingControllerStopMeeting, meetingInProgress.Meeting);
			}
			else
            {
				if (ApplicationController.Instance.TimeHUD.SpeedUpToggleOn)
                {
					RequestMeetingsToSpeedUp();
				}
				CommandAIMeetingResponse cmdNextToTalk = new CommandAIMeetingResponse();
				cmdNextToTalk.Initialize(meetingToRun, meetingInProgress.RequestToEndMeeting);
				SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdNextToTalk);
			}
		}

		public MeetingInProgress GetMeetingInProgressByUID(string meetingUID)
        {
			foreach (MeetingInProgress meeting in _meetingsInProgress)
			{
				if (meeting != null)
				{
					if (meeting.Meeting != null)
					{
						if (meeting.Meeting.GetUID().Equals(meetingUID))
						{
							return meeting;
						}
					}
				}
			}
			return null;
		}

		private void FixDependencies(List<DocumentMeetingJSON> docs)
        {
			if (docs == null) return;
			if (docs.Count == 0) return;

			bool forceNullDependency = true;
			foreach (DocumentMeetingJSON docIni in docs)
			{
				if ((docIni.dependency == null) || (docIni.dependency.Length == 0))
				{
					forceNullDependency = false;
				}
			}

			if (forceNullDependency)
			{
				docs[0].dependency = "";
			}


			foreach (DocumentMeetingJSON docMeet in docs)
			{
				if ((docMeet.dependency != null) && (docMeet.dependency.Length > 0))
				{
					bool hasBeenFound = false;
					foreach (DocumentMeetingJSON docCheck in docs)
                    {
						if (docMeet != docCheck)
                        {
							if (StringSimilarity.CalculateSimilarityPercentage(docCheck.name.ToLower(), docMeet.dependency.ToLower()) > 85)
                            {
								docMeet.dependency = docCheck.name;
								hasBeenFound = true;
								break;
							}
						}

					}
					if (!hasBeenFound)
                    {
						docMeet.dependency = "";
					}
				}
			}
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(HumanView.EventHumanViewReachedDestination))
			{
				HumanView human = (HumanView)parameters[0];
				TargetDestination targetReached = (TargetDestination)parameters[2];
				if (human.ItemData.IsPlayer)
                {
					if (targetReached == TargetDestination.Human)
                    {
						GameObject targetToInterrupt = (GameObject)parameters[3];
						if (targetToInterrupt.GetComponent<HumanView>() != null)
                        {
							_nameToInterrupt = targetToInterrupt.GetComponent<HumanView>().NameHuman;
							ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("text.do.you.want.to.interrupt.this.other.employee", _nameToInterrupt), SubEventMeetingControllerConfirmationStartInterruption);
						}
                    }
                }
			}
			if (nameEvent.Equals(EventMeetingControllerDeleteAllMeetings))
			{
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("text.do.you.want.to.delete.all.meetings"), SubEventMeetingControllerConfirmationAllDelete);
			}
			if (nameEvent.Equals(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewAddedTag))
			{
				string meetingUID = (string)parameters[0];
				string addedTag = (string)parameters[1];
				MeetingInProgress meeting = GetMeetingInProgressByUID(meetingUID);
				if (meeting != null)
				{
					if (meeting.Tags == null)
                    {
						meeting.Tags = new List<string>();
					}
					if (!meeting.Tags.Contains(addedTag))
                    {
						meeting.Tags.Add(addedTag);
					}
				}
			}
			if (nameEvent.Equals(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRemovedTag))
			{
				string meetingUID = (string)parameters[0];
				string removedTag = (string)parameters[1];
				MeetingInProgress meeting = GetMeetingInProgressByUID(meetingUID);
				if (meeting != null)
				{
					meeting.Tags.Remove(removedTag);
				}
			}
			if (nameEvent.Equals(EventMeetingControllerForceEndRunningSocialMeetings))
            {
				foreach (MeetingInProgress meeting in _meetingsInProgress)
				{
					if (meeting != null)
					{
						if (meeting.Meeting != null)
                        {
							if (meeting.Meeting.IsSocialMeeting())
							{
								SystemEventController.Instance.DelaySystemEvent(MeetingController.EventMeetingControllerStopMeeting, 0.5f, meeting.Meeting);
							}
						}
					}
				}
			}
			if (nameEvent.Equals(ClockController.EventClockControllerTimeSpeedUp))
            {
				if ((bool)parameters[0])
                {
					RequestMeetingsToSpeedUp();
				}
			}
			if (nameEvent.Equals(EventMeetingControllerPoliteRequestToEndRunningMeetings))
            {
				if (parameters.Length > 0)
                {
					string member = (string)parameters[0];
					bool requestStopSocial = (bool)parameters[1];
					MeetingData meetingException = null;
					if (parameters.Length > 2)
                    {
						meetingException = (MeetingData)parameters[2];
					}						
					foreach (MeetingInProgress meeting in _meetingsInProgress)
					{
						if (meeting != null)
						{
							if (meeting.Meeting != meetingException)
                            {
								if (requestStopSocial)
								{
									if (meeting.Meeting.IsSocialMeeting())
									{
										if (meeting.ContainsAssistant(member))
										{
											meeting.RequestToEndMeeting = true;
										}
									}
								}
								else
								{
									if (!meeting.Meeting.IsSocialMeeting())
									{
										if (meeting.ContainsAssistant(member))
										{
											meeting.RequestToEndMeeting = true;
										}
									}
								}
							}
						}
					}
				}
				else
                {
					foreach (MeetingInProgress meeting in _meetingsInProgress)
					{
						if (meeting != null)
						{
							meeting.RequestToEndMeeting = true;
						}
					}
				}
			}
			if (nameEvent.Equals(EventMeetingControllerAddMeeting))
			{
				MeetingData meeting = null;
				if (parameters[0] == null)
				{
					int taskId = (int)parameters[1];
					string nameMeeting = (string)parameters[2];
					string descriptionMeeting = (string)parameters[3];
					DocumentData[] dataMeeting = ((List<DocumentData>)parameters[4]).ToArray();
					DateTime timeStartMeeting = (DateTime)parameters[5];
					DateTime timeEndMeeting = (DateTime)parameters[6];
					int projectID = (int)parameters[7];
					bool canClose = (bool)parameters[9];
					bool canLeave = (bool)parameters[10];
					bool findRoom = (bool)parameters[11];
					bool startDialogForPlayer = (bool)parameters[12];
					bool initiatedByPlayer = (bool)parameters[13];

					if (taskId != -1)
					{
						var (task, boardname) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(taskId);
						if (task != null)
						{
							task.SetData(dataMeeting);
						}
						meeting = new MeetingData(nameMeeting, projectID, taskId, descriptionMeeting, null, timeStartMeeting, timeEndMeeting, canClose, canLeave, findRoom, startDialogForPlayer, initiatedByPlayer, (string[])parameters[8]);
					}
					else
					{
						meeting = new MeetingData(nameMeeting, projectID, taskId, descriptionMeeting, dataMeeting, timeStartMeeting, timeEndMeeting, canClose, canLeave, findRoom, startDialogForPlayer, initiatedByPlayer, (string[])parameters[8]);
					}
					meeting.IsUserCreated = ApplicationController.Instance.IsPlayMode;

					List<MeetingData> meetings = WorkDayData.Instance.CurrentProject.GetMeetings();
					meetings.Add(meeting);
					WorkDayData.Instance.CurrentProject.SetMeetings(meetings);

					UIEventController.Instance.DispatchUIEvent(ScreenCalendarView.EventScreenCalendarViewCreateMeeting, meeting);
				}
				else
				{
					meeting = (MeetingData)parameters[0];
					string initialMeetingUID = meeting.GetUID();
					meeting.TaskId = (int)parameters[1];
					meeting.Name = (string)parameters[2];
					meeting.Description = (string)parameters[3];
					if (meeting.TaskId != -1)
					{
						var (task, boardname) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(meeting.TaskId);
						if (task != null)
						{
							task.SetData(((List<DocumentData>)parameters[4]).ToArray());
						}
					}
					else
					{
						meeting.Data = ((List<DocumentData>)parameters[4]).ToArray();
					}
					meeting.SetTimeStart((DateTime)parameters[5]);
					meeting.SetTimeEnd((DateTime)parameters[6]);
					meeting.SetMembers(((string[])parameters[8]).ToList<string>());
					meeting.CanClose = (bool)parameters[9];
					meeting.CanLeave = (bool)parameters[10];
					meeting.FindRoom = (bool)parameters[11];
					meeting.StartDialogScreenForPlayer = (bool)parameters[12];
					meeting.InitiatedByPlayer = (bool)parameters[13];

					UIEventController.Instance.DispatchUIEvent(ScreenCalendarView.EventScreenCalendarViewUpdateMeeting, initialMeetingUID, meeting);
				}
			}
			if (nameEvent.Equals(EventMeetingControllerDeleteMeeting))
			{
				string meetingUID = (string)parameters[0];
				MeetingData meeting = WorkDayData.Instance.CurrentProject.GetMeetingByUID(meetingUID);
				if (meeting != null)
                {
					if (meeting.InProgress)
                    {
						ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("text.cannot.delete.meeting.in.progress"));
					}
					else
                    {
						ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmationInput, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("text.do.you.want.to.delete.to.this.meeting", meeting.Name) + " : " + meeting.Name, SubEventMeetingControllerConfirmationDelete);
						UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, meetingUID);
					}
				}
			}
			if (nameEvent.Equals(EventMeetingControllerJoinMeeting))
			{
				UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
				MeetingData meeting = (MeetingData)parameters[0];
				int taskUID = (int)parameters[1];
				DateTime startingTime = (DateTime)parameters[2];
				string nameJoiner = (string)parameters[3];

				MeetingInProgress meetingInProgress = GetMeetingInProgress(meeting);
				meetingInProgress.AddAssistant(nameJoiner);

				TasksController.Instance.StartProgressForHuman(nameJoiner, taskUID, startingTime, meeting.IsSocialMeeting());

				if (ApplicationController.Instance.HumanPlayer.NameHuman.Equals(nameJoiner))
				{
					ScreenController.Instance.DestroyScreens();
					ScreenController.Instance.CreateScreen(ScreenDialogView.ScreenName, false, true, meeting);
					UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, ScreenDialogView.ScreenName, 10);
				}

				if (meeting.FindRoom && (meeting.RoomName != null) && (meeting.RoomName.Length > 0))
				{
					int numberFree = ApplicationController.Instance.LevelView.CountFreeChairForArea(meeting.RoomName);
					if (numberFree > 0)
					{
						CommandGoToAreaChair cmdGoToChairMeetingRoom = new CommandGoToAreaChair();
						cmdGoToChairMeetingRoom.Initialize(nameJoiner, meeting.RoomName);
						SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdGoToChairMeetingRoom);						
					}
				}
				else
                {
					if (meeting.IsSocialMeeting())
                    {
						List<string> socialAssistants = meeting.GetAssistingMembers(false);
						foreach (string socialAssisting in socialAssistants)
                        {
							var (humanGO, humanData) = ApplicationController.Instance.LevelView.GetItemByName(socialAssisting);
							string nameAreaSocial = null;
							if (humanGO.GetComponent<HumanView>().CurrentChair != null)
                            {
								if (humanGO.GetComponent<HumanView>().CurrentChair.Area != null)
                                {
									nameAreaSocial = humanGO.GetComponent<HumanView>().CurrentChair.Area.Name;
								}
							}
							if (nameAreaSocial != null)
							{
								CommandGoToAreaChair cmdGoToChairSocialMeeting = new CommandGoToAreaChair();
								cmdGoToChairSocialMeeting.Initialize(nameJoiner, nameAreaSocial);
								cmdGoToChairSocialMeeting.Prioritary = false;
								SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdGoToChairSocialMeeting);
							}
						}
					}
                }

				SystemEventController.Instance.DispatchSystemEvent(EventMeetingControllerMeetingStarted, meeting);
			}
			if (nameEvent.Equals(EventMeetingControllerLeaveMeeting))
            {
				MeetingData meeting = (MeetingData)parameters[0];
				string nameMember = (string)parameters[1];
				if (RemoveMemberFromMeeting(meeting, nameMember))
                {
					if (ApplicationController.Instance.HumanPlayer != null)
                    {
						if (ApplicationController.Instance.HumanPlayer.NameHuman.Equals(nameMember))
                        {
							CommandGoToOwnChair cmdGoToYourChair = new CommandGoToOwnChair();
							cmdGoToYourChair.Initialize(nameMember);
							SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdGoToYourChair);
							SystemEventController.Instance.DelaySystemEvent(TimeHUD.EventTimeHUDCancelSelectionObject, 0.1f);
						}
					}

					MeetingInProgress meetingInProgress = GetMeetingInProgress(meeting);
					if (meetingInProgress.Assisting.Count == 0)
                    {
						SystemEventController.Instance.DispatchSystemEvent(EventMeetingControllerStopMeeting, meeting);
                    }
					else
                    {
						SystemEventController.Instance.DispatchSystemEvent(EventMeetingControllerRunAction, meeting);
					}					
				}
			}
			if (nameEvent.Equals(EventMeetingControllerRequestStartMeeting))
            {
				MeetingData meeting = (MeetingData)parameters[0];
				int taskUID = (int)parameters[1];
				DateTime startingTime = (DateTime)parameters[2];
				string expectHuman = "";
				bool includeHumanInMeeting = true;
				if (parameters.Length > 3)
                {
					expectHuman = (string)parameters[3];
					includeHumanInMeeting = false;
				}
				string membersAssistingToMeeting = meeting.GetMembersPacket(expectHuman, includeHumanInMeeting);
				string[] namesHumans = membersAssistingToMeeting.Split(",");
				if (namesHumans.Length <= 1)
                {
					meeting.InProgress = false;
					meeting.Completed = true;
					SystemEventController.Instance.DispatchSystemEvent(EventMeetingControllerStopMeeting, meeting);
				}
				else
                {
					if (meeting.HasPlayer(false) && includeHumanInMeeting)
                    {
						ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("text.now.loading.meeting"));
					}

					// ++AI++ ASK AI WHAT MEMBERS ARE GOING TO ASSIST TO THE MEETING (LIKE INCOMPETENT MANAGERS MISSING IT)
					if (meeting.IsSocialMeeting())
                    {						
						membersAssistingToMeeting = meeting.GetMembersNotInAMeetingPacket(expectHuman, includeHumanInMeeting);
						namesHumans = membersAssistingToMeeting.Split(",");
					}
					SystemEventController.Instance.DelaySystemEvent(EventMeetingControllerResponseStartMeeting, 1, meeting, taskUID, startingTime, membersAssistingToMeeting);
				}
			}
			if (nameEvent.Equals(EventMeetingControllerResponseStartMeeting))
			{
				MeetingData meeting = (MeetingData)parameters[0];
				if (meeting.HasPlayer(true))
                {
					UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
				}
				
				int taskUID = (int)parameters[1];
				DateTime startingTime = (DateTime)parameters[2];
				string packetNamesHumans = ((string)parameters[3]);
				string[] namesHumans = packetNamesHumans.Split(",");
				if (namesHumans.Length <= 1)
                {
					meeting.InProgress = false;
					meeting.Completed = true;
					SystemEventController.Instance.DispatchSystemEvent(EventMeetingControllerStopMeeting, meeting);
				}
				else
                {
					bool allowStartMeeting = true;
					if (!ContainsMeeting(meeting) && allowStartMeeting)
					{
						meeting.InProgress = true;
						meeting.Completed = false;
						if (meeting.TaskId != -1)
                        {
							meeting.ShouldCreateDocuments = 100;
						}

						MeetingInProgress newMeeting = new MeetingInProgress(meeting, packetNamesHumans);
						_meetingsInProgress.Add(newMeeting);

						// ++AI++ ITERATIONS IS TO SIMULATE THE MEETING CONVERSATIONS
						if (meeting.IsSocialMeeting())
                        {
							if (meeting.IsInterruptionMeeting())
                            {
								meeting.SetIterations(meeting.GetTotalMinutes(), 5);
							}
							else
                            {
								meeting.SetIterations((2 * meeting.GetTotalMinutes())/3, 5);
							}
						}
						else
                        {
							meeting.SetIterations((meeting.GetTotalMinutes() / 2), 20);
						}						

						if (taskUID != -1)
                        {
							foreach (string nameHuman in namesHumans)
							{
								TasksController.Instance.StartProgressForHuman(nameHuman, taskUID, startingTime, meeting.IsSocialMeeting());
							}
						}

						AreaData meetingRoom = null;
						// OPTIONAL MOVE TO A MEETING ROOM
						if (meeting.FindRoom)
						{
							meetingRoom = CommandsController.Instance.FindRoomForMeeting(meeting, namesHumans);
						}

						if (ApplicationController.Instance.HumanPlayer == null)
						{
							SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
							SystemEventController.Instance.DispatchSystemEvent(EventMeetingControllerRunAction, meeting);
						}
						else
						{
							if (meeting.HasPlayer(true) && meeting.StartDialogScreenForPlayer)
							{
								ScreenController.Instance.DestroyScreens();
								SystemEventController.Instance.DispatchSystemEvent(HumanView.EventHumanViewForceSelection, ApplicationController.Instance.HumanPlayer.NameHuman);
								ScreenController.Instance.CreateScreen(ScreenDialogView.ScreenName, false, true, meeting);
								UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, ScreenDialogView.ScreenName, 10);

								if (meetingRoom != null)
								{
									List<string> membersToAssist = namesHumans.ToList<string>();
									SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerActorsInTheWay, meeting, membersToAssist);
								}
								else
                                {
									// IF THERE IS NO ROOM AVAILABLE ENABLE THE DIALOG BUTTONS AND INPUT TEXT
									SystemEventController.Instance.DelaySystemEvent(EditionSubStateBase.EventSubStateBaseEnableMovement, 0.2f, true);
								}
							}
							else
							{
								SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
								SystemEventController.Instance.DispatchSystemEvent(EventMeetingControllerRunAction, meeting);
							}
						}
						SystemEventController.Instance.DispatchSystemEvent(EventMeetingControllerMeetingStarted, meeting);
					}
				}
			}
			if (nameEvent.Equals(EventMeetingControllerMeetingStarted))
            {
				MeetingData meetingStarted = (MeetingData)parameters[0];
				List<string> assistingMembers = meetingStarted.GetAssistingMembers(false);

				foreach (MeetingInProgress meetingInProgress in _meetingsInProgress)
				{
					if (meetingInProgress.Meeting != meetingStarted)
                    {
						foreach (string assisting in assistingMembers)
                        {
							if (meetingInProgress.ContainsAssistant(assisting))
                            {
								if (meetingInProgress.RemoveAssistant(assisting))
                                {
#if UNITY_EDITOR
                                    Debug.Log("MEMBER[" + assisting + "] FOUND IN AN EXISTING MEETING[" + meetingInProgress.Meeting.Name + "] HAS BEEN REMOVED!!!");
#endif
								}
							}
						}
					}
				}
			}
			if (nameEvent.Equals(EventMeetingControllerRunAction))
            {
				MeetingData meeting = (MeetingData)parameters[0];				
				if (ContainsMeeting(meeting))
                {
					if (meeting.InProgress)
                    {
						RunMeeting(meeting);
					}
					else
                    {						
						_meetingsInProgress.Remove(GetMeetingInProgress(meeting));
                    }
				}
			}
			if (nameEvent.Equals(EventMeetingControllerStopMeeting))
            {
				MeetingData meeting = (MeetingData)parameters[0];
				if (ContainsMeeting(meeting))
				{
					meeting.InProgress = false;
					meeting.Completed = true;

					if (meeting.FindRoom && (meeting.RoomName != null) && (meeting.RoomName.Length > 0))
                    {
						List<string> membersAssisted = meeting.GetAssistingMembers(false);
						SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerActorsInTheWay, meeting, membersAssisted, true);
					}

					// ++AI++ REQUEST TO SUMMARIZE THE MEETING
					AICommandsController.Instance.AddNewAICommand(new AICommandSummarizeMeeting(), meeting.HasPlayer(true), meeting, EventMeetingControllerMeetingSummarized);
				}
			}
			if (nameEvent.Equals(EventMeetingControllerMeetingSummarized))
            {
				MeetingData meeting = (MeetingData)parameters[0];
				if (meeting.HasPlayer(true))
				{
					UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
				}
				if (_meetingsInProgress.Remove(GetMeetingInProgress(meeting)))
                {
					if (parameters[1] != null)
                    {
						MeetingSummaryJSON meetingSummary = (MeetingSummaryJSON)parameters[1];
						meeting.Summary = meetingSummary.summary;

						if (meetingSummary.documents != null)
                        {
							FixDependencies(meetingSummary.documents);

							foreach (DocumentMeetingJSON docMeet in meetingSummary.documents)
							{
								string[] persons = docMeet.persons.Split(",");
								int uidProgress = WorkDayData.Instance.CurrentProject.AddDocProgress(new CurrentDocumentInProgress(WorkDayData.Instance.CurrentProject.GetCurrentProgressNextID(), meeting.ProjectId, meeting.GetUID(), meeting.TaskId, docMeet.name, docMeet.persons, docMeet.dependency, docMeet.type, docMeet.time, docMeet.data, WorkDayData.Instance.CurrentProject.GetCurrentTime()));
								foreach (string personDirty in persons)
								{
									string person = personDirty.Trim();
									WorldItemData humanData = WorkDayData.Instance.CurrentProject.GetItemByName(person);
									if ((humanData != null) && (meeting.TaskId != -1))
									{
										TaskProgressData currTaskInProgress = humanData.GetTaskProgressByID(meeting.TaskId);
										if (currTaskInProgress != null)
                                        {
											currTaskInProgress.AddCurrentDocProgressUID(uidProgress);
										}										
									}
								}
							}
						}
						SystemEventController.Instance.DispatchSystemEvent(AICommandsController.EventAICommandsControllerEvaluateDocsToWork);
					}
					List<string> members = meeting.GetAssistingMembers(true);
					if (!meeting.IsSocialMeeting())
					{
						foreach (string member in members)
						{
							WorldItemData humanData = WorkDayData.Instance.CurrentProject.GetItemByName(member);
							if (humanData != null)
							{
								if (humanData.IsHuman)
								{
									humanData.IsAvailable = true;
								}
							}
						}
					}
					foreach (string member in members)
					{
						if (!CommandsController.Instance.CheckExistingCommandGoToChair(member))
						{
							var (humanGO, humanData) = ApplicationController.Instance.LevelView.GetItemByName(member);
							var (chairGO, chairData) = ApplicationController.Instance.LevelView.GetItemByOwner(member);
							if (chairGO == null)
							{
								CommandGoToOwnChair cmdGoOwnChair = new CommandGoToOwnChair();
								cmdGoOwnChair.Initialize(member);
								SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdGoOwnChair);
							}
							else
							{
								if (humanGO.GetComponent<HumanView>().CurrentChair != chairGO.GetComponent<ChairView>())
								{
									CommandGoToOwnChair cmdGoOwnChair = new CommandGoToOwnChair();
									cmdGoOwnChair.Initialize(member);
									SystemEventController.Instance.DispatchSystemEvent(CommandsController.EventCommandsControllerAddCommand, cmdGoOwnChair);
								}
							}
						}
					}
					SystemEventController.Instance.DispatchSystemEvent(EventMeetingControllerMeetingStopped, meeting);

					if (meeting.HasPlayer(true))
					{
						SystemEventController.Instance.DelaySystemEvent(TimeHUD.EventTimeHUDCancelSelectionObject, 0.1f);
					}
#if UNITY_EDITOR
                    Debug.Log("----------REMOVED MEETING FROM MEETING CONTROLLER!!!!!!");
#endif
				}
            }			
			if (nameEvent.Equals(RunStateLoading.EventRunStateLoadingCompleted))
            {
				List<MeetingData> meetings = WorkDayData.Instance.CurrentProject.GetMeetings();
				foreach (MeetingData meeting in meetings)
                {
					if (meeting.InProgress)
                    {
						if (!ContainsMeeting(meeting))
                        {
							MeetingInProgress meetingInProgress = new MeetingInProgress(meeting);
							_meetingsInProgress.Add(meetingInProgress);
							ApplicationController.Instance.LevelView.LinkHumanInMeetingWithChairs(meetingInProgress.Assisting);
#if UNITY_EDITOR
                            Debug.Log("MEETING IN PROGRESS=" + meeting.Name + " EXTRA DATA="+ meeting.ExtraData);
#endif
                            if (meeting.HasPlayer(false))
                            {
								if (meetingInProgress.Assisting.Contains(ApplicationController.Instance.HumanPlayer.NameHuman))
                                {
									ScreenController.Instance.DestroyScreens();
									SystemEventController.Instance.DispatchSystemEvent(HumanView.EventHumanViewForceSelection, ApplicationController.Instance.HumanPlayer.NameHuman);
									ScreenController.Instance.CreateScreen(ScreenDialogView.ScreenName, false, true, meeting);
									UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, ScreenDialogView.ScreenName, 10);
								}
								else
                                {
									SystemEventController.Instance.DispatchSystemEvent(EventMeetingControllerRunAction, meeting);
								}
							}
							else
                            {
								SystemEventController.Instance.DispatchSystemEvent(EventMeetingControllerRunAction, meeting);
                            }
						}
					}
				}
				SystemEventController.Instance.DispatchSystemEvent(EventMeetingControllerMeetingsLoaded);
				SystemEventController.Instance.DelaySystemEvent(ScreenDialogView.EventScreenDialogViewForceEnableNextButtons, 0.2f);
            }
			if (nameEvent.Equals(ApplicationController.EventMainControllerReleaseGameResources))
			{
				_meetingsInProgress.Clear();
			}
			if (nameEvent.Equals(SystemEventController.EventSystemEventControllerReleaseAllResources))
			{
				_instance = null;
				_meetingsInProgress.Clear();
				GameObject.Destroy(this.gameObject);
			}
			if (nameEvent.Equals(SystemEventController.EventSystemEventControllerDontDestroyOnLoad))
			{
				if (Instance)
				{
					DontDestroyOnLoad(Instance.gameObject);
				}
			}
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventMeetingControllerUIRequestToStartMeeting))
            {
				MeetingData meeting = (MeetingData)parameters[0];
				int taskId = (int)parameters[1];
				int canvasOrder = -1;
				if (parameters.Length > 2)
                {
					canvasOrder = (int)parameters[2];
				}
				if (meeting.Completed)
				{
					ScreenController.Instance.CreateScreen(ScreenHistoryMeetingView.ScreenName, false, false, meeting);
					if (canvasOrder != -1)
                    {
						UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, ScreenHistoryMeetingView.ScreenName, canvasOrder);
					}					
				}
				else
				{
					if (meeting.InProgress)
					{
						if (meeting.IsSocialMeeting())
						{
							if (ApplicationController.Instance.HumanPlayer != null)
                            {
								if (!meeting.IsMemberInMeeting(ApplicationController.Instance.HumanPlayer.NameHuman))
								{
									if (meeting.AddMember(ApplicationController.Instance.HumanPlayer.NameHuman))
									{
										SystemEventController.Instance.DispatchSystemEvent(EventMeetingControllerMeetingsRefreshData, meeting);
									}
								}
							}
						}

						if (meeting.HasPlayer(false))
						{
							// JOIN AS MEMBER
							SystemEventController.Instance.DispatchSystemEvent(HumanView.EventHumanViewForceSelection, ApplicationController.Instance.HumanPlayer.NameHuman);
							SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerJoinMeeting, meeting, taskId, WorkDayData.Instance.CurrentProject.GetCurrentTime(), ApplicationController.Instance.HumanPlayer.NameHuman);
						}
						else
						{
							// JOIN AS A OBSERVER
							ScreenController.Instance.DestroyScreens();
							ScreenController.Instance.CreateScreen(ScreenDialogView.ScreenName, false, true, meeting);
							UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, ScreenDialogView.ScreenName, 10);
						}
					}
					else
					{
						SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerRequestStartMeeting, meeting, taskId, WorkDayData.Instance.CurrentProject.GetCurrentTime());
					}
				}
			}
			if (nameEvent.Equals(ScreenCalendarView.EventScreenCalendarViewSetNewDate))
			{
				bool isMeeting = (bool)parameters[0];
				DateTime selectedTime = (DateTime)parameters[1];
				if (!isMeeting)
				{
					DateTime newCurrentTime = new DateTime(selectedTime.Year, selectedTime.Month, selectedTime.Day, WorkDayData.Instance.CurrentProject.StartingHour, 0, 0);
					WorkDayData.Instance.CurrentProject.SetCurrentTime(newCurrentTime);
					WorkDayData.Instance.CurrentProject.ResetAllMeetings();
					WorkDayData.Instance.CurrentProject.ResetAllBoards();
					WorkDayData.Instance.CurrentProject.CalculateTasksDepth();
					WorkDayData.Instance.CurrentProject.Reset();
					ApplicationController.Instance.LevelView.ResetLevel();
					ClearInProgressMeetings();
					SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdateCurrentTime);
				}
				else
				{
					MeetingData meeting = WorkDayData.Instance.CurrentProject.GetMeetingByUID((string)parameters[2]);
					if (meeting != null)
					{
						DateTime startTime = meeting.GetTimeStart();
						meeting.SetTimeStart(new DateTime(selectedTime.Year, selectedTime.Month, selectedTime.Day, startTime.Hour, startTime.Minute, startTime.Second));

						DateTime endTime = meeting.GetTimeEnd();
						meeting.SetTimeEnd(new DateTime(selectedTime.Year, selectedTime.Month, selectedTime.Day, endTime.Hour, endTime.Minute, endTime.Second));

						SystemEventController.Instance.DispatchSystemEvent(EventMeetingControllerUpdatedMeeting, meeting);
					}
				}
			}
			if (nameEvent.Equals(EventMeetingControllerCreateScreenMeeting))
			{
				DateTime cellDateTime = (DateTime)parameters[0];
				if (!WorkDayData.Instance.CurrentProject.IsFreeDay(cellDateTime))
				{
					ScreenController.Instance.CreateScreen(ScreenMeetingView.ScreenName, false, false, -1, cellDateTime);
				}
				else
				{
					ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.meeting.no.meetings.in.free.time"));
				}
			}
			if (nameEvent.Equals(CalendarSchedulerItem.EventCalendarSchedulerItemEdit))
			{
				string meetingUID = (string)parameters[0];
				MeetingData meeting = WorkDayData.Instance.CurrentProject.GetMeetingByUID(meetingUID);
				ScreenController.Instance.CreateScreen(ScreenMeetingView.ScreenName, false, false, meeting.TaskId, meeting);
			}
			if (nameEvent.Equals(SubEventMeetingControllerConfirmationAssistance))
			{
				ScreenInformationResponses userResponse = (ScreenInformationResponses)parameters[1];
				MeetingData meetingToStart = WorkDayData.Instance.CurrentProject.GetMeetingByUID((string)parameters[2]);
				DateTime currentTime = WorkDayData.Instance.CurrentProject.GetCurrentTime();
				if (userResponse == ScreenInformationResponses.Confirm)
				{
					SystemEventController.Instance.DispatchSystemEvent(HumanView.EventHumanViewForceSelection, ApplicationController.Instance.HumanPlayer.NameHuman);
					MeetingData meetingForPlayer = GetMeetingOfPlayer();
					if (meetingForPlayer == null)
                    {
						SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerRequestStartMeeting, meetingToStart, meetingToStart.TaskId, currentTime);
					}
					else
                    {
						GameObject screenLeaveGO = ScreenController.Instance.CreateScreen(ScreenLeaveMeetingView.ScreenName, false, false, meetingForPlayer, true);
						UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, screenLeaveGO, 10);
                    }
				}
				else
				{
					SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerRequestStartMeeting, meetingToStart, meetingToStart.TaskId, currentTime, ApplicationController.Instance.HumanPlayer.NameHuman);
				}
			}
			if (nameEvent.EndsWith(SubEventMeetingControllerConfirmationDelete))
            {
				ScreenInformationResponses userResponse = (ScreenInformationResponses)parameters[1];
				string meetingUID = (string)parameters[2];
				if (userResponse == ScreenInformationResponses.Confirm)
				{
					if (WorkDayData.Instance.CurrentProject.RemoveMeetingByUID(meetingUID))
					{
						UIEventController.Instance.DispatchUIEvent(ScreenCalendarView.EventScreenCalendarViewRemoveMeeting, meetingUID);
					}
				}
			}
			if (nameEvent.Equals(SubEventMeetingControllerConfirmationAllDelete))
            {
				ScreenInformationResponses userResponse = (ScreenInformationResponses)parameters[1];
				if (userResponse == ScreenInformationResponses.Confirm)
				{
					WorkDayData.Instance.CurrentProject.RemoveMeetingsByProject(WorkDayData.Instance.CurrentProject.ProjectInfoSelected);
					WorkDayData.Instance.CurrentProject.RemoveMeetingsByProject(-1);
					UIEventController.Instance.DispatchUIEvent(ScreenCalendarView.EventScreenCalendarViewRefreshMeetings);
				}
			}
			if (nameEvent.Equals(SubEventMeetingControllerConfirmationStartInterruption))
			{
				if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
				{
					List<string> assistants = new List<string>() { ApplicationController.Instance.HumanPlayer.NameHuman, _nameToInterrupt };

					// CREATE INTERRUPTION MEETING
					DateTime dateTimeStart = WorkDayData.Instance.CurrentProject.GetCurrentTime();
					DateTime dateTimeEnd = dateTimeStart.AddSeconds(20 * 60);
					string nameMeetingInterruption = LanguageController.Instance.GetText("word.interruption") + " " + dateTimeStart.ToShortTimeString();
					SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerAddMeeting, null, -1, nameMeetingInterruption, LanguageController.Instance.GetText("text.description.interruption"), new List<DocumentData>(), dateTimeStart, dateTimeEnd, -1, assistants.ToArray(), false, false, false, true, true);
				}
			}
		}


		private float _timeAcum = 0;

		private void Update()
        {
			_timeAcum += Time.deltaTime;
			if (_timeAcum > 1)
            {
				_timeAcum = 0;
				if (ApplicationController.Instance.TimeHUD != null)
                {
					if (ApplicationController.Instance.TimeHUD.IsPlayingTime)
					{
						DateTime currentTime = WorkDayData.Instance.CurrentProject.GetCurrentTime();
						List<MeetingData> meetings = WorkDayData.Instance.CurrentProject.GetMeetings();
						foreach (MeetingData meeting in meetings)
						{
							if (!meeting.Completed && !meeting.InProgress)
							{
								DateTime startTime = meeting.GetTimeStart();
								DateTime endTime = meeting.GetTimeEnd();
								if ((startTime < currentTime) && (currentTime < endTime))
								{
									if (!meeting.Requested)
									{
										meeting.Requested = true;
										if (meeting.IsSocialMeeting())
										{
											if (!meeting.StartDialogScreenForPlayer)
											{
												if (ApplicationController.Instance.HumanPlayer != null)
												{
													SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerRequestStartMeeting, meeting, meeting.TaskId, currentTime, ApplicationController.Instance.HumanPlayer.NameHuman);
												}
												else
												{
													SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerRequestStartMeeting, meeting, meeting.TaskId, currentTime);
												}
											}
											else
											{
												SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerRequestStartMeeting, meeting, meeting.TaskId, currentTime);
											}
										}
										else
										{
											if (meeting.HasPlayer(false))
											{
												SystemEventController.Instance.DispatchSystemEvent(ScreenDialogView.EventScreenDialogViewForceLeaveSocialMeetings);
												SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDResetTimeIncrement);
												ProjectInfoData projectInfo = WorkDayData.Instance.CurrentProject.GetProject(meeting.ProjectId);
												if (projectInfo != null)
												{
													ApplicationController.Instance.LastProjectColor = projectInfo.GetColor();
													ApplicationController.Instance.LastProjectFeedback = projectInfo.Name;
												}
                                                ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmationInput, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("text.do.you.want.to.assist.to.this.meeting", meeting.Name) + "\n\n" + meeting.Name, SubEventMeetingControllerConfirmationAssistance);
                                                UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, meeting.GetUID());
											}
											else
											{
												SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerRequestStartMeeting, meeting, meeting.TaskId, currentTime);
											}
										}
										return;
									}
								}
							}
						}
					}
				}
			}			
		}
    }
}