using UnityEngine;
using yourvrexperience.Utils;
using System;
using yourvrexperience.UserManagement;
using static yourvrexperience.WorkDay.MenuStateRun;
using System.Collections.Generic;
using System.Linq;

namespace yourvrexperience.WorkDay
{
	public class RunStateLoading : IBasicState
	{
		public const string EventRunStateLoadingAreasLoad = "EventRunStateLoadingAreasLoad";
		public const string EventRunStateLoadingBake = "EventRunStateLoadingBake";
		public const string EventRunStateLoadingCompleted = "EventRunStateLoadingCompleted";

		class AIGroup
        {
			public string Name;
			public string Description;
			public List<AIEmployee> Members;
        }

		class AIEmployee : IEquatable<AIEmployee>
        {
			public string Name;
			public string Category;
			public string Sex;
			public string Description;

			public AIEmployee(string name, string category, string sex, string skills, string personality)
            {
				Name = name;
				Category = category;
				Sex = sex;
				Description = LanguageController.Instance.GetText("word.skills") + " : " + skills;
				Description += "\n\n";
				Description += LanguageController.Instance.GetText("word.personality") + " : " + personality;
			}

			public bool Equals(AIEmployee other)
            {
				return Name.Equals(other.Name);
            }
        }

		public void Initialize()
		{
			SystemEventController.Instance.Event += OnSystemEvent;

			ApplicationController.Instance.LevelView.CreateCells(WorkDayData.Instance.CurrentProject.Cells);
			ApplicationController.Instance.LevelView.CreateItems(WorkDayData.Instance.CurrentProject.Items);
			
			// INITIALIZE WORLD WITH TEAM INFORMATION
			if (ApplicationController.Instance.TeamCompany != null)
            {
				string categoryLead = PromptController.Instance.GetText("xml.tag.category.lead");
				string categorySenior = PromptController.Instance.GetText("xml.tag.category.senior");
				string categoryNormal = PromptController.Instance.GetText("xml.tag.category.normal");

				string sexWoman = PromptController.Instance.GetText("xml.tag.woman");
				string sexMan = PromptController.Instance.GetText("xml.tag.man");

				List<AIGroup> aiGroups = new List<AIGroup>();
				foreach (GroupCompanyJSON group in ApplicationController.Instance.TeamCompany.groups)
                {
					aiGroups.Add(new AIGroup() { Name = group.name, Description = group.description, Members = new List<AIEmployee>() });
				}
				
				// FIX GROUP ASSIGNATION FOR EACH EMPLOYEE
				foreach (EmployeeCompanyJSON employee in ApplicationController.Instance.TeamCompany.employees)
				{
					foreach (AIGroup aiGroup in aiGroups)
                    {
						if (StringSimilarity.CalculateSimilarityPercentage(aiGroup.Name.ToLower(), employee.group.ToLower()) > 85)
                        {
							AIEmployee aiEmployee = new AIEmployee(employee.name, employee.category, employee.sex, employee.skills, employee.personality);
							if (!aiGroup.Members.Contains(aiEmployee))
                            {
								aiGroup.Members.Add(aiEmployee);
							}
						}
					}
				}

				List<AIGroup> aiGroupsOrdered = aiGroups.OrderByDescending(d => d.Members.Count).ToList();
				List<GroupInfoData> groups = WorkDayData.Instance.CurrentProject.GetGroups();
				List<GroupInfoData> groupsOrdered = groups.OrderByDescending(d => d.Members.Length).ToList();

				// RENAME GROUPS
				for (int i = 0; i < aiGroupsOrdered.Count; i++)
                {
					if (i < aiGroupsOrdered.Count)
                    {
						groupsOrdered[i].Name = aiGroupsOrdered[i].Name;
						groupsOrdered[i].Description = aiGroupsOrdered[i].Description;
					}
				}
				List<GroupInfoData> groupsRemoved = new List<GroupInfoData>();
				while (groupsOrdered.Count > aiGroupsOrdered.Count)
                {
					groupsRemoved.Add(groupsOrdered[groupsOrdered.Count - 1]);
					groupsOrdered.RemoveAt(groupsOrdered.Count - 1);
				}
				WorkDayData.Instance.CurrentProject.SetGroups(groupsOrdered.ToArray());
				
				// RENAME THE HUMANS
				groups = WorkDayData.Instance.CurrentProject.GetGroups();
				List<(WorldItemData, GameObject)> humansToUpdate = new List<(WorldItemData, GameObject)>();
				List<string> humansToRemove = new List<string>();
				for (int i = 0; i < groups.Count; i++)
                {
					GroupInfoData targetGroup = groups[i];
					AIGroup newGroup = aiGroupsOrdered[i];
					List<string> targetMembers = targetGroup.GetMembers();

					for (int j = 0; j < targetMembers.Count; j++)
                    {
						string targetMember = targetMembers[j];
						if (j < newGroup.Members.Count)
                        {							
							AIEmployee newMember = newGroup.Members[j];
							targetMembers[j] = newMember.Name;

							var (humanGO, humanData) = ApplicationController.Instance.LevelView.GetItemByName(targetMember);
							if (humanGO != null)
                            {
								WorkDayData.Instance.CurrentProject.ReplaceHumanInSystem(targetMember, newMember.Name);
								bool isAClient = false;
                                if (StringSimilarity.CalculateSimilarityPercentage(LanguageController.Instance.GetText("text.clients").ToLower(), newGroup.Name.ToLower()) > 85)
								{
                                    isAClient = true;
                                }
                                if (StringSimilarity.CalculateSimilarityPercentage(LanguageController.Instance.GetText("text.customers").ToLower(), newGroup.Name.ToLower()) > 85)
                                {
                                    isAClient = true;
                                }
								if (!isAClient)
								{
                                    ApplicationController.Instance.LevelView.ReplaceOwner(targetMember, newMember.Name);
                                }
								else
								{
                                    ApplicationController.Instance.LevelView.ReplaceOwner(targetMember, LanguageController.Instance.GetText("screen.calendar.NOBODY"));
                                }
								humanGO.GetComponent<HumanView>().NameHuman = newMember.Name;
								humanGO.name = newMember.Name;
								humanData.Name = newMember.Name;
								humanData.Data = newMember.Description;
								humanData.IsClient = isAClient;

                                bool isManFinalSex = false;
								if (StringSimilarity.CalculateSimilarityPercentage(newMember.Sex.ToLower(), sexWoman.ToLower()) > 90)
                                {
									isManFinalSex = false;
								}
								else
                                {
									isManFinalSex = true;
								}
								if (humanData.IsMan != isManFinalSex)
                                {
									humansToUpdate.Add((humanData, humanGO));
								}

								if (StringSimilarity.CalculateSimilarityPercentage(newMember.Category.ToLower(), categoryLead.ToLower()) > 85)
                                {
									humanData.IsLead = true;
									humanData.IsSenior = false;
								}
								else
								if (StringSimilarity.CalculateSimilarityPercentage(newMember.Category.ToLower(), categorySenior.ToLower()) > 85)
								{
									humanData.IsLead = false;
									humanData.IsSenior = true;
								}
								else
								{
									humanData.IsLead = false;
									humanData.IsSenior = false;
								}
							}
						}
						else
                        {
							humansToRemove.Add(targetMember);
							targetMembers.RemoveAt(j);
							j--;
						}
					}
					targetGroup.SetMembers(targetMembers);
				}

				foreach (GroupInfoData groupRemoved in groupsRemoved)
                {
					List<string> removedMembers = groupRemoved.GetMembers();
					humansToRemove.AddRange(removedMembers);
				}

				// DELETE HUMANS NOT USED				
				foreach(string humanToRemove in humansToRemove)
                {
					var (humanGO, humanData) = ApplicationController.Instance.LevelView.GetItemByName(humanToRemove);
					if (humanGO != null)
                    {
						ApplicationController.Instance.LevelView.DeleteItem(humanGO, true);
					}
				}
				SystemEventController.Instance.DispatchSystemEvent(HumanView.EventHumanViewGroupUpdated);

				// REPLACE THE SEX OF THE WORKERS
				foreach ((WorldItemData, GameObject) humanToReplace in humansToUpdate)
                {
					ApplicationController.Instance.LevelView.ReplaceHumanGender(humanToReplace.Item1, humanToReplace.Item2);
                }
				humansToUpdate.Clear();

				// SET INITIAL PROJECT
				string projectName = ApplicationController.Instance.TeamCompany.projectname;
				string projectDescription = ApplicationController.Instance.TeamCompany.projectdescription;
				List<ProjectInfoData> projects = WorkDayData.Instance.CurrentProject.GetProjects();
				projects[0].Name = projectName;
				projects[0].Description = projectDescription;
				projects[0].SetColor(Color.white);
			}

			WorkDayData.Instance.ConsultImages((int)UsersController.Instance.CurrentUser.Id);
		}

		public void Destroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ConsultUserImagesHTTP.EventConsultUserImagesHTTPCompleted))
            {
				if ((bool)parameters[0])
                {
					List<int> imagesForUser = (List<int>)parameters[1];
					string imagesToDelete = "";
					foreach (int idImageUser in imagesForUser)
                    {
						if (!WorkDayData.Instance.CurrentProject.ExistImageInDocuments(idImageUser))
                        {
							if (imagesToDelete.Length > 0) imagesToDelete += ";";
							imagesToDelete += idImageUser;
						}
					}
					if (imagesToDelete.Length == 0)
                    {
						SystemEventController.Instance.DispatchSystemEvent(EventRunStateLoadingAreasLoad, 0.2f);
					}
					else
                    {
						WorkDayData.Instance.DeleteImage(imagesToDelete);
					}
				}
				else
                {
					SystemEventController.Instance.DispatchSystemEvent(EventRunStateLoadingAreasLoad, 0.2f);
				}								
			}
			if (nameEvent.Equals(DeleteImageDataHTTP.EventDeleteImageDataHTTPCompleted))
            {
				SystemEventController.Instance.DispatchSystemEvent(EventRunStateLoadingAreasLoad, 0.2f);
			}
			if (nameEvent.Equals(EventRunStateLoadingAreasLoad))
            {
				ApplicationController.Instance.LevelView.CreateAreas(WorkDayData.Instance.CurrentProject.Areas);
				SystemEventController.Instance.DispatchSystemEvent(EventRunStateLoadingBake, 0.4f);
			}
			if (nameEvent.Equals(EventRunStateLoadingBake))
            {
				ApplicationController.Instance.LevelView.CalculatePathfindingInformation();
				SystemEventController.Instance.DelaySystemEvent(ChairView.EventChairViewAssignAreaData, 0.1f);
				UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyAllScreens);
				SystemEventController.Instance.DispatchSystemEvent(MenuStateRun.EventRunStateChangeState, StatesRun.Run);				
				SystemEventController.Instance.DelaySystemEvent(EventRunStateLoadingCompleted, 0.1f);
			}
		}

		public void Run()
		{
		}
	}
}