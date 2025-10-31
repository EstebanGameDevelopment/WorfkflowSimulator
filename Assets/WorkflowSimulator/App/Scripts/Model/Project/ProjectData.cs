using Maything.UI.CalendarSchedulerUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.EditionSubStateAreas;
using static yourvrexperience.WorkDay.TaskItemData;

namespace yourvrexperience.WorkDay
{
    [System.Serializable]
    public class GroupInfoData : IEquatable<GroupInfoData>
    {
        public string Name;
        public string Description;
        public string ColorHex;
        public string[] Members;

        public GroupInfoData(string name, string description, Color color, params string[] members)
        {
            Name = name;
            Description = description;
            ColorHex = ColorUtility.ToHtmlStringRGB(color);
            if (members != null)
            {
                SetMembers(members.ToList<string>());
            }
        }

        public bool Equals(GroupInfoData other)
        {
            return Name.Equals(other.Name);
        }

        public override string ToString()
        {
            List<string> members = GetMembers();
            string output = "";
            foreach (string member in members) output += member + ",";

            return "Group Info[" + Name + "][" + Description + "][" + output + "]";
        }

        public Color GetColor()
        {
            Color color;
            string finalColor = "#" + ColorHex;
            if (ColorUtility.TryParseHtmlString(finalColor, out color))
                return color;
            return Color.black;
        }

        public void SetColor(Color color)
        {
            ColorHex = ColorUtility.ToHtmlStringRGBA(color);
        }

        public void SetMembers(List<string> members)
        {
            Members = members.ToArray();
        }

        public List<string> GetMembers()
        {
            List<string> members = new List<string>();
            if (Members != null)
            {
                for (int i = 0; i < Members.Length; i++)
                {
                    members.Add(Members[i]);
                }
            }
            return members;
        }

        public bool IsMember(string member)
        {
            if (Members != null)
            {
                for (int i = 0; i < Members.Length; i++)
                {
                    if (Members[i].Equals(member))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HasPlayer()
        {
            if (Members != null)
            {
                for (int i = 0; i < Members.Length; i++)
                {
                    string member = Members[i];
                    WorldItemData memberData = WorkDayData.Instance.CurrentProject.GetItemByName(member);
                    if (memberData != null)
                    {
                        if (memberData.IsHuman && memberData.IsPlayer)
                        {
                            return true;
                        }                        
                    }
                }
            }
            return false;
        }

        public bool RemoveMember(string member)
        {
            List<string> members = GetMembers();
            if (members.Remove(member))
            {
                SetMembers(members);
                return true;
            }
            else
            {
                return false;
            }
        }
        public string PackMembers()
        {
            string output = "";
            if (Members != null)
            {
                for (int i = 0; i < Members.Length; i++)
                {
                    if (output.Length > 0) output += ",";
                    output += Members[i];
                }
            }
            return output;
        }
    }


    [System.Serializable]
    public class ProjectInfoData : IEquatable<ProjectInfoData>
    {
        public int Id;
        public string Name;
        public string Description;
        public string ColorHex;

        public ProjectInfoData(int id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        public bool Equals(ProjectInfoData other)
        {
            return Name.Equals(other.Name) || Id == other.Id;
        }

        public override string ToString()
        {
            return "Project Info[" + Id + "][" + Name + "][" + Description + "]";
        }

        public Color GetColor()
        {
            Color color;
            string finalColor = "#" + ColorHex;
            if (ColorUtility.TryParseHtmlString(finalColor, out color))
                return color;
            return Color.white;
        }

        public void SetColor(Color color)
        {
            ColorHex = ColorUtility.ToHtmlStringRGBA(color);
        }
    }

    [System.Serializable]
    public struct HTMLData
    {
        public string html;
        public int language;

        public void SetHTML(string rawHTML)
        {
            html = rawHTML.Replace("\\", "\\\\")
                        .Replace("\"", "\\\"")
                        .Replace("\n", "\\n")
                        .Replace("\r", "");
        }
        public string GetHTML()
        {
            try
            {
                string rawCode = Regex.Unescape(html);
                return rawCode;
            } catch (Exception err)
            {
                return html;
            }
        }
    }

    [System.Serializable]
    public class DocumentData : IEquatable<DocumentData>
    {
        public int Id;
        public int ProjectId;
        public string Name;
        public string Description;
        public string Owner;
        public HTMLData Data;
        public bool IsGlobal;
        public bool IsImage;
        public bool IsChanged;
        public string Summary;
        public bool IsAI;
        public int FeatureID;
        public int TaskID;
        public string CreationTime;
        public bool IsUserCreated;

        public DocumentData(int id, int projectId, string name, string description, string owner, HTMLData data, bool isGlobal, bool isImage, string summary, bool isChanged, int featureID, int taskID)
        {
            Id = id;
            ProjectId = projectId;
            Name = name;
            Description = description;
            Owner = owner;
            Data = data;
            IsGlobal = isGlobal;
            IsImage = isImage;
            Summary = summary;
            IsChanged = isChanged;
            IsAI = false;
            FeatureID = featureID;
            TaskID = taskID;
            SetCreationTime(WorkDayData.Instance.CurrentProject.GetCurrentTime());
            IsUserCreated = ApplicationController.Instance.IsPlayMode;
        }

        public DateTime GetCreationTime()
        {
            return DateTime.Parse(CreationTime);
        }
        public void SetCreationTime(DateTime time)
        {
            CreationTime = time.ToString("o"); // "o" = ISO 8601 format
        }

        public bool Equals(DocumentData other)
        {
            return (Name.Equals(other.Name) || Id == other.Id) && (ProjectId == other.ProjectId);
        }

        public DocumentData Clone()
        {
            return new DocumentData(Id, ProjectId, Name, Description, Owner, Data, IsGlobal, IsImage, Summary, IsChanged, FeatureID, TaskID);
        }

        public void Copy(DocumentData document)
        {
            Name = document.Name;
            ProjectId = document.ProjectId;
            Description = document.Description;
            Data = document.Data;
            Owner = document.Owner;
            IsGlobal = document.IsGlobal;
            IsImage = document.IsImage;
            Summary = document.Summary;
            IsChanged = document.IsChanged;
            FeatureID = document.FeatureID;
            TaskID = document.TaskID;
            CreationTime = document.CreationTime;
            IsUserCreated = document.IsUserCreated;            
        }

        public int GetImageID()
        {
            if (Data.GetHTML().Length > 0)
            {
                int idImage;
                if (int.TryParse(Data.GetHTML(), out idImage))
                {
                    return idImage;
                }
            }
            return -1;
        }

        public override string ToString()
        {
            return Name + ";Project=" + ProjectId + ";IsGlobal="+IsGlobal;
        }
    }

    [System.Serializable]
    public class ProjectData
    {
        public string ProjectName = "";
        public string Language = "en";
        public string Company = "";
        public int Level = 0;

        public Vector3[] Cells;
        public WorldItemData[] Items;
        public AreaData[] Areas;
        public BoardData[] Boards;
        public DocumentData[] Documents;
        public ProjectInfoData[] Projects;
        public GroupInfoData[] Groups;
        public MeetingData[] Meetings;
        public CurrentDocumentInProgress[] CurrentDocProgress;
        public int DocumentNextID;        
        public int CurrentProgressNextID;        
        public int ProjectInfoSelected;
        public int ProjectNextID;
        public string CurrentDateTime;
        public int TaskNextID;
        public int StartingHour;
        public int LunchHour;
        public int EndingHour;
        public DayOfWeek EndingDayOfWeek;
        public bool IsSundayFirst;
        public bool EnableBreaks;
        public bool EnableInterruptions;

        public Vector3 CameraPosition;
        public Vector3 CameraRotation;
        public int ConfigurationCamera;

        public bool StartDayTrigger;
        public bool LunchDayTrigger;
        public bool EndDayTrigger;

        public CostAIOperation[] Cost;
        public StorageUsed Storage;

        public ProjectData(string language, string project, string company, bool enableBreaks, bool enableInterruptions, int startHour, int lunchHour, int endHour, DayOfWeek weekend)
        {
            Language = language;
            ProjectName = project;
            Company = company;
            DocumentNextID = 0;
            StartingHour = 9;
            LunchHour = 13;
            EndingHour = 18;
            EndingDayOfWeek = DayOfWeek.Saturday;
            CameraPosition = new Vector3(0, 8.45f, -6.507f);
            CameraRotation = new Vector3(55, 0, 0);
            EnableBreaks = enableBreaks;
            EnableInterruptions = enableInterruptions;

            StartingHour = startHour;
            LunchHour = lunchHour;
            EndingHour = endHour;
            EndingDayOfWeek = weekend;

            InitCurrentTime();
        }

        public void SetLevel(int level)
        {
            Level = level;
        }

        public int GetLevel()
        {
            return Level;
        }

        public void SetCurrentTime(DateTime time)
        {
            CurrentDateTime = time.ToString("o");
        }

        public bool HasDayStarted()
        {
            DateTime currTime = GetCurrentTime();
            DateTime startDay = new DateTime(currTime.Year, currTime.Month, currTime.Day, StartingHour, 0, 0);
            return currTime != startDay;
        }

        public void InitCurrentTime()
        {
            DateTime nextMonday = ApplicationController.Instance.GetNextMonday(DateTime.Now);
            DateTime currTime = new DateTime(nextMonday.Year, nextMonday.Month, nextMonday.Day, StartingHour, 0, 0);
            SetCurrentTime(currTime);
        }

        public DateTime GetCurrentTime()
        {
            if ((CurrentDateTime == null) || (CurrentDateTime.Length == 0))
            {
                SetCurrentTime(DateTime.Now);
            }
            return DateTime.Parse(CurrentDateTime);
        }

        public bool IsFreeDay(DateTime day)
        {
            if (EndingDayOfWeek == DayOfWeek.Monday)
            {
                return false;
            }
            else
            {
                int finalDay = (int)day.DayOfWeek;
                int currentFinalDay = (int)EndingDayOfWeek;
                if (day.DayOfWeek == DayOfWeek.Sunday) finalDay = 7;
                if (EndingDayOfWeek == DayOfWeek.Sunday) currentFinalDay = 7;
                return ((currentFinalDay - finalDay) <= 0);
            }
        }

        public Color GetColorForMember(string member)
        {
            GroupInfoData group = GetGroupByName(member);
            if (group != null)
            {
                return group.GetColor();
            }
            else
            {
                group = GetGroupOfMember(member);
                if (group != null)
                {
                    return group.GetColor();
                }
                else
                {
                    return Color.white;
                }
            }
        }

        public string GetGroupLetter(string member)
        {
            GroupInfoData group = GetGroupByName(member);
            if (group != null)
            {
                return group.Name.Substring(0,1);
            }
            else
            {
                group = GetGroupOfMember(member);
                if (group != null)
                {
                    return group.Name.Substring(0, 1);
                }
                else
                {
                    return "";
                }
            }
        }

        private void DeleteProjectMeetings(int projectId)
        {
            List<MeetingData> meetings = GetMeetings();
            int counter = 0;
            for (int j = 0; j < meetings.Count; j++)
            {
                if (meetings[j].ProjectId == projectId)
                {
                    meetings.RemoveAt(j);
                    j--;
                    counter++;
                }
            }
            if (counter > 0)
            {
#if UNITY_EDITOR
                Debug.Log("TOTAL MEETINGS REMOVED["+ counter + "] WITH REMAINING OF ["+ meetings.Count + "]");
#endif
            }
            SetMeetings(meetings);
        }

        public int GetCurrentProgressNextID()
        {
            CurrentProgressNextID++;
            return CurrentProgressNextID;
        }

        public int GetDocumentNextID()
        {
            DocumentNextID++;
            return DocumentNextID;
        }

        public int GetProjectNextID()
        {
            int output = ProjectNextID;
            ProjectNextID++;
            return output;
        }

        public int GetTaskNextID()
        {
            TaskNextID++;
            return TaskNextID;
        }

        public void SetCells(GameObject[] cellsGO)
        {
            List<Vector3> cellPositions = new List<Vector3>();
            foreach(GameObject go in cellsGO)
            {
                cellPositions.Add(go.transform.position);
            }
            Cells = cellPositions.ToArray();
        }

        public void SetItems(WorldItemData[] itemsWorld)
        {
            Items = itemsWorld;
        }

        public List<WorldItemData> GetHumans(bool excludeClients = true)
        {
            if (Items != null)
            {
                List<WorldItemData> humans = new List<WorldItemData>();
                foreach (WorldItemData item in Items)
                {
                    if (item.IsHuman)
                    {
                        if (excludeClients)
                        {
                            if (!item.IsClient)
                            {
                                humans.Add(item);
                            }
                        }
                        else
                        {
                            humans.Add(item);
                        }                        
                    }
                }
                return humans;
            }
            return null;
        }

        public void SetAreas(AreaData[] areasWorld)
        {
            Areas = areasWorld;
        }

        public AreaData GetFreeMeetingRoomFor(AreaMode typeArea, int numberAssistants)
        {
            if (Areas != null)
            {
                foreach (AreaData area in Areas)
                {
                    if ((AreaMode)area.Type == typeArea)
                    {
                        if (ApplicationController.Instance.LevelView.CheckFreeRoom(area.Name))
                        {
                            int totalChairsFree = ApplicationController.Instance.LevelView.CountFreeChairForArea(area.Name);
                            if (totalChairsFree >= numberAssistants)
                            {
                                return area;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public AreaData GetFreeChairInRoom(AreaMode typeArea)
        {
            if (Areas != null)
            {
                foreach (AreaData area in Areas)
                {
                    if ((AreaMode)area.Type == typeArea)
                    {
                        int totalChairsFree = ApplicationController.Instance.LevelView.CountFreeChairForArea(area.Name);
                        if (totalChairsFree > 0)
                        {
                            return area;
                        }
                    }
                }
            }
            return null;
        }

        public void ClearBoards()
        {
            SetBoards((new List<BoardData>()).ToArray());
        }

        public void SetBoards(BoardData[] boards)
        {
            Boards = boards;
        }

        public List<BoardData> GetAllBoards()
        {
            List<BoardData> boards = new List<BoardData>();
            if (Boards != null)
            {
                for (int i = 0; i < Boards.Length; i++)
                {
                    boards.Add(Boards[i]);
                }
            }
            return boards;
        }

        public string PackBoardsXML(int projectID, bool includeTask = false)
        {
            List<BoardData> boards = new List<BoardData>();
            string packet = "\n\n";
            if (Boards != null)
            {
                for (int i = 0; i < Boards.Length; i++)
                {
                    if (Boards[i].ProjectId == projectID)
                    {
                        BoardData board = Boards[i];
                        if (!includeTask)
                        {
                            packet += "<" + PromptController.Instance.GetText("xml.tag.board") 
                                    + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + board.BoardName + "\""
                                    + " " + PromptController.Instance.GetText("xml.tag.description") + "=\"" + PromptController.Instance.ReplaceConflictiveCharacters(board.Description) + "\"/>";
                            packet += "\n\n";
                        }
                        else
                        {
                            packet += "<" + PromptController.Instance.GetText("xml.tag.board") 
                                    + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + board.BoardName + "\""
                                    + " " + PromptController.Instance.GetText("xml.tag.description") + "=\"" + PromptController.Instance.ReplaceConflictiveCharacters(board.Description) + "\">";
                            packet += "\n";
                            List<TaskItemData> tasks = WorkDayData.Instance.CurrentProject.GetAllTasks(board);
                            foreach (TaskItemData task in tasks)
                            {
                                string stateTask = PromptController.Instance.GetText("xml.value.todo");
                                if (task.IsTaskCompleted() || task.IsTaskVerified())
                                {
                                    stateTask = PromptController.Instance.GetText("xml.value.done");
                                }
                                else
                                {
                                    if (task.IsTaskDoing())
                                    {
                                        stateTask = PromptController.Instance.GetText("xml.value.doing");
                                    }
                                }
                                List<string> membersTask = task.GetHumanMembers();
                                string packMembersTask = "";
                                foreach (string member in membersTask)
                                {
                                    WorldItemData humanData = WorkDayData.Instance.CurrentProject.GetItemByName(member);
                                    if (packMembersTask.Length > 0) packMembersTask += ",";
                                    packMembersTask += humanData.Name;
                                    GroupInfoData groupInfoDataMember = WorkDayData.Instance.CurrentProject.GetGroupOfMember(member);
                                    if (groupInfoDataMember != null)
                                    {
                                        packMembersTask += "(" + groupInfoDataMember.Name  + ")";
                                    }
                                    int hoursDone = humanData.GetTotalHoursProgressForTask(projectID, task.UID);
                                    if (hoursDone > 0)
                                    {
                                        packMembersTask += "(" + hoursDone + " " + PromptController.Instance.GetText("xml.value.hours.done") + ")";
                                    }
                                    TimeWorkingDataDisplay taskProgress = humanData.GetCurrentTaskProgress(projectID);
                                    if ((taskProgress != null) && (taskProgress.TaskUID == task.UID))
                                    {
                                        packMembersTask += "(" + PromptController.Instance.GetText("xml.value.now.working") + ")";
                                    }
                                }
                                packet += "<" + PromptController.Instance.GetText("xml.tag.task") 
                                        + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + task.Name + "\""
                                        + " " + PromptController.Instance.GetText("xml.tag.description") + "=\"" + PromptController.Instance.ReplaceConflictiveCharacters(task.Description) + "\""
                                        + " " + PromptController.Instance.GetText("xml.tag.state") + "=\"" + stateTask + "\""
                                        + " " + PromptController.Instance.GetText("xml.tag.employees.assigned") + "=\"" + packMembersTask + "\""
                                        + "/>";
                                packet += "\n\n";
                            }
                            packet += "</" + PromptController.Instance.GetText("xml.tag.board") + ">";
                        }
                    }
                }
            }
            return packet;
        }

        public void SetProjects(ProjectInfoData[] projects)
        {
            Projects = projects;
        }

        public List<ProjectInfoData> GetProjects()
        {
            List<ProjectInfoData> projects = new List<ProjectInfoData>();
            if (Projects != null)
            {
                for (int i = 0; i < Projects.Length; i++)
                {
                    projects.Add(Projects[i]);
                }
            }
            if (projects.Count == 0)
            {
                projects.Add(new ProjectInfoData(0, LanguageController.Instance.GetText("text.hud.main.title.project"), LanguageController.Instance.GetText("text.hud.main.description.project")));
                SetProjects(projects.ToArray());
                ProjectInfoSelected = 0;
            }
            return projects;
        }

        public ProjectInfoData GetProject(int id)
        {
            List<ProjectInfoData> projects = GetProjects();
            if (Projects != null)
            {                
                for (int i = 0; i < projects.Count; i++)
                {
                    if (projects[i].Id == id)
                    {
                        return projects[i];
                    }
                }
            }
            return null;
        }

        public ProjectInfoData GetProject(string nameProject)
        {
            if (Projects != null)
            {
                for (int i = 0; i < Projects.Length; i++)
                {
                    if (Projects[i].Name.Equals(nameProject))
                    {
                        return Projects[i];
                    }
                }
            }
            return null;
        }


        public bool RemoveProjectInfo(ProjectInfoData project)
        {
            List<ProjectInfoData> projects = GetProjects();
            if (projects.Remove(project))
            {
                List<string> boardsRemoved = DeleteBoardByProject(project.Id);
                foreach (string boardName in boardsRemoved)
                {
                    DeleteBoardForHumans(boardName);
                }
                DeleteDocuments(project.Id);
                DeleteProjectMeetings(project.Id);
                DeleteAllLoggedWorkLinkedToProject(project.Id);
                SetProjects(projects.ToArray());
                return true;
            }
            return false;
        }


        public void SetGroups(GroupInfoData[] groups)
        {
            Groups = groups;
        }

        public List<GroupInfoData> GetGroups()
        {
            List<GroupInfoData> groups = new List<GroupInfoData>();
            if (Groups != null)
            {
                for (int i = 0; i < Groups.Length; i++)
                {
                    groups.Add(Groups[i]);
                }
            }
            return groups;
        }

        public GroupInfoData GetGroupByName(string name)
        {
            if ((name == null) || (name.Length == 0)) return null;

            if (Groups != null)
            {
                for (int i = 0; i < Groups.Length; i++)
                {
                    if (Groups[i].Name.Equals(name))
                    {
                        return Groups[i];
                    }
                }
            }
            return null;
        }

        public GroupInfoData GetGroupOfMember(string member)
        {
            if ((member == null) || (member.Length == 0)) return null;

            if (Groups != null)
            {
                for (int i = 0; i < Groups.Length; i++)
                {
                    if (Groups[i].IsMember(member))
                    {
                        return Groups[i];
                    }
                }
            }
            return null;
        }

        public bool RemoveGroupInfo(GroupInfoData group)
        {
            List<GroupInfoData> groups = GetGroups();
            if (groups.Remove(group))
            {
                DeleteMemberInMeetings(group.Name);
                DeleteMemberInTasks(group.Name);
                SetGroups(groups.ToArray());
                return true;
            }
            return false;
        }

        public void ReplaceHumanInSystem(string previousName, string newName)
        {
            ReplaceMemberInMeetings(previousName, newName);
            ReplaceMemberInTasks(previousName, newName);
            ReplaceMemberInGroups(previousName, newName);
            ReplaceMemberInDocuments(previousName, newName);
        }

        public void DeleteHumanFromSystem(string nameHuman)
        {
            DeleteMemberInMeetings(nameHuman);
            DeleteMemberInTasks(nameHuman);
            DeleteMemberInGroups(nameHuman);
            DeleteMemberInDocuments(nameHuman);
        }

        public BoardData GetBoardFor(string board, bool shouldCreate = true)
        {
            List<BoardData> boards = GetAllBoards();
            for (int i = 0; i < boards.Count; i++)
            {
                if (boards[i].BoardName.Equals(board))
                {
                    return boards[i];
                }
            }
            if (shouldCreate)
            {
                BoardData newBoard = new BoardData(board, "", ProjectInfoSelected);
                boards.Add(newBoard);
                SetBoards(boards.ToArray());
                return newBoard;
            }
            else
            {
                return null;
            }
        }

        public bool DeleteBoard(string board)
        {
            List<BoardData> boards = GetAllBoards();
            for (int i = 0; i < boards.Count; i++)
            {
                if (boards[i].BoardName.Equals(board))
                {
                    boards.RemoveAt(i);
                    SetBoards(boards.ToArray());
                    return true;
                }
            }
            return false;
        }

        public List<string> DeleteBoardByProject(int projectId)
        {
            List<string> namesBoardsRemoved = new List<string>();
            List<BoardData> boards = GetAllBoards();
            int counter = 0;
            for (int i = 0; i < boards.Count; i++)
            {
                if (boards[i].ProjectId == projectId)
                {
                    namesBoardsRemoved.Add(boards[i].BoardName);
                    boards.RemoveAt(i);
                    counter++;
                    i--;
                }
            }
#if UNITY_EDITOR
            Debug.Log("BOARDS REMOVED[" + counter + "] REMAINING BOARDS TOTAL[" + boards.Count + "]");
#endif
            SetBoards(boards.ToArray());
            return namesBoardsRemoved;
        }

        public void DeleteBoardForHumans(string boardName)
        {
            foreach (WorldItemData item in Items)
            {
                if (item.IsHuman)
                {
                    List<string> boards = item.GetBoards();
                    if (boards.Remove(boardName))
                    {
                        item.SetBoards(boards);
#if UNITY_EDITOR
                        Debug.Log("BOARD [" + boardName + "] REMOVED FOR HUMAN[" + item.Name + "]");
#endif
                    }
                }
            }
        }

        public bool RemoveBoardByName(string boardName)
        {
            BoardData boardToDelete = GetBoardFor(boardName);
            if (boardToDelete != null)
            {
                List<BoardData> boards = GetAllBoards();
                if (boards.Remove(boardToDelete))
                {
                    DeleteBoardForHumans(boardName);
                    DeleteAllLoggedWorkLinkedToBoard(boardName);
                    DeleteMeetingsLinkedToBoard(boardName);
                    SetBoards(boards.ToArray());
                    boardToDelete.DeleteAllImagesForTasks();
                    return true;
                }
            }
            return false;
        }

        public void DeleteAllLoggedWorkLinkedToBoard(string boardName)
        {
            BoardData boardToDelete = GetBoardFor(boardName);
            if (boardToDelete != null)
            {
                if (boardToDelete.Tasks != null)
                {
                    foreach (TaskItemData task in boardToDelete.Tasks)
                    {
                        foreach (WorldItemData item in Items)
                        {
                            if (item.IsHuman)
                            {
                                item.DeleteWorkingLogs(boardToDelete.ProjectId, task.UID);
                            }
                        }
                    }
                }
            }
        }

        public void DeleteAllLoggedWorkLinkedToProject(int projectID)
        {
            List<BoardData> allBoards = GetAllBoards();
            foreach (BoardData board in allBoards)
            {
                if (board != null)
                {
                    if (board.Tasks != null)
                    {
                        foreach (TaskItemData task in board.Tasks)
                        {
                            foreach (WorldItemData item in Items)
                            {
                                if (item.IsHuman)
                                {
                                    item.DeleteWorkingLogs(projectID, task.UID);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void DeleteMemberInDocuments(string nameToDelete)
        {
            if (Documents != null)
            {
                for (int i = 0; i < Documents.Length; i++)
                {
                    if (Documents[i].Owner != null)
                    {
                        if (Documents[i].Owner.Equals(nameToDelete))
                        {
                            Documents[i].Owner = LanguageController.Instance.GetText("screen.calendar." + WorkDayData.RESERVED_NAME_NOBODY);
                        }
                    }
                }
            }
        }

        public void DeleteMemberInMeetings(string nameToDelete)
        {
            List<MeetingData> meetings = GetMeetings();
            foreach (MeetingData meeting in meetings)
            {
                List<string> membersGroup = meeting.GetMembers();
                if (membersGroup.Remove(nameToDelete))
                {
                    meeting.SetMembers(membersGroup);
                }
                List<DocumentData> docs = meeting.GetData();
                for (int i = 0; i < docs.Count; i++)
                {
                    if (docs[i].Owner != null)
                    {
                        if (docs[i].Owner.Equals(nameToDelete))
                        {
                            docs[i].Owner = LanguageController.Instance.GetText("screen.calendar." + WorkDayData.RESERVED_NAME_NOBODY);
                        }
                    }
                }
            }
        }

        public void DeleteMemberInTasks(string nameToDelete)
        {
            List<BoardData> boards = GetAllBoards();
            foreach (BoardData board in boards)
            {
                List<TaskItemData> tasksBoard = board.GetTasks();
                foreach (TaskItemData task in tasksBoard)
                {
                    List<string> membersTask = task.GetMembers();
                    if (membersTask.Remove(nameToDelete))
                    {
                        task.SetMembers(membersTask);
                    }

                    List<DocumentData> docs = task.GetData();
                    for (int i = 0; i < docs.Count; i++)
                    {
                        if (docs[i].Owner != null)
                        {
                            if (docs[i].Owner.Equals(nameToDelete))
                            {
                                docs[i].Owner = LanguageController.Instance.GetText("screen.calendar." + WorkDayData.RESERVED_NAME_NOBODY);
                            }
                        }
                    }
                }
            }
        }

        public void DeleteMemberInGroups(string nameToDelete)
        {
            List<GroupInfoData> groups = GetGroups();
            foreach (GroupInfoData group in groups)
            {
                if (group.IsMember(nameToDelete))
                {
                    List<string> members = group.GetMembers();
                    if (members.Remove(nameToDelete))
                    {
                        group.SetMembers(members);
                    }
                }
            }
        }

        public void ReplaceMemberInMeetings(string oldName, string newName)
        {
            List<MeetingData> meetings = GetMeetings();
            foreach (MeetingData meeting in meetings)
            {
                List<string> membersGroup = meeting.GetMembers();
                if (membersGroup.Remove(oldName))
                {
                    membersGroup.Add(newName);
                    meeting.SetMembers(membersGroup);
                }

                List<DocumentData> docs = meeting.GetData();
                for (int i = 0; i < docs.Count; i++)
                {
                    if (docs[i].Owner != null)
                    {
                        if (docs[i].Owner.Equals(oldName))
                        {
                            docs[i].Owner = newName;
                        }
                    }
                }
            }
        }

        public void ReplaceMemberInDocuments(string oldName, string newName)
        {
            if (Documents != null)
            {
                for (int i = 0; i < Documents.Length; i++)
                {
                    if (Documents[i].Owner != null)
                    {
                        if (Documents[i].Owner.Equals(oldName))
                        {
                            Documents[i].Owner = newName;
                        }
                    }
                }
            }
        }

        public void ReplaceMemberInTasks(string oldName, string newName)
        {
            List<BoardData> boards = GetAllBoards();
            foreach (BoardData board in boards)
            {
                List<TaskItemData> tasksBoard = board.GetTasks();
                foreach (TaskItemData task in tasksBoard)
                {
                    List<string> membersTask = task.GetMembers();
                    if (membersTask.Remove(oldName))
                    {
                        membersTask.Add(newName);
                        task.SetMembers(membersTask);
                    }
                    List<DocumentData> docs = task.GetData();
                    for (int i = 0; i < docs.Count; i++)
                    {
                        if (docs[i].Owner != null)
                        {
                            if (docs[i].Owner.Equals(oldName))
                            {
                                docs[i].Owner = newName;
                            }
                        }
                    }
                }
            }
        }

        public void ReplaceMemberInGroups(string oldName, string newName)
        {
            List<GroupInfoData> groups = GetGroups();
            foreach (GroupInfoData group in groups)
            {
                if (group.IsMember(oldName))
                {
                    List<string> members = group.GetMembers();
                    if (members.Remove(oldName))
                    {
                        members.Add(newName);
                        group.SetMembers(members);
                    }
                }
            }
        }

        public void ClearDocuments()
        {
            SetDocuments((new List<DocumentData>()).ToArray());
        }

        public void SetDocuments(DocumentData[] documents)
        {
            Documents = documents;
        }

        public List<DocumentData> GetDocuments()
        {
            List<DocumentData> documents = new List<DocumentData>();
            if (Documents != null)
            {
                for (int i = 0; i < Documents.Length; i++)
                {
                    documents.Add(Documents[i]);
                }
            }
            return documents;
        }

        private void DeleteDocuments(int projectID)
        {
            List<DocumentData> documents = GetDocuments();
            int counter = 0;
            for (int i = 0; i < documents.Count; i++)
            {
                if (documents[i].ProjectId == projectID)
                {
                    documents.RemoveAt(i);
                    i--;
                    counter++;
                }
            }
#if UNITY_EDITOR
            Debug.Log("DOCUMENTS REMOVED[" + counter + "] REMAINING DOCUMENTS TOTAL[" + documents.Count + "]");
#endif
            SetDocuments(documents.ToArray());
        }

        public DocumentData GetDocumentByID(int documentID)
        {
            if (Documents != null)
            {
                for (int i = 0; i < Documents.Length; i++)
                {
                    if (Documents[i].Id == documentID)
                    {
                        return Documents[i];
                    }
                }
            }
            return null;
        }

        public DocumentData GetDocumentByFeatureID(int featureID)
        {
            if (Documents != null)
            {
                for (int i = 0; i < Documents.Length; i++)
                {
                    if (Documents[i].FeatureID == featureID)
                    {
                        return Documents[i];
                    }
                }
            }
            return null;
        }

        public DocumentData GetDocumentByName(string documentName)
        {
            if (Documents != null)
            {
                for (int i = 0; i < Documents.Length; i++)
                {
                    DocumentData doc = Documents[i];
                    if (doc.Name == documentName)
                    {
                        return doc;
                    }
                }
            }
            return null;
        }
        public DocumentData GetDocumentInSystemByID(int documentID)
        {
            DocumentData doc = GetDocumentByID(documentID);
            if (doc != null)
            {
                return doc;
            }
            else
            {
                List<BoardData> boards = GetAllBoards();
                foreach(BoardData board in boards)
                {
                    List<TaskItemData> tasks = board.GetTasks();
                    foreach(TaskItemData task in tasks)
                    {
                        DocumentData docTask = task.GetDataByID(documentID);
                        if (docTask != null)
                        {
                            return docTask;
                        }
                    }
                }
                List<MeetingData> meetings = GetMeetings();
                foreach (MeetingData meeting in meetings)
                {
                    DocumentData docMeeting = meeting.GetDataByID(documentID);
                    if (docMeeting != null)
                    {
                        return docMeeting;
                    }
                }
            }
            return null;
        }

        public void UpdateGlobalDocuments()
        {
            if (Documents != null)
            {
                for (int i = 0; i < Documents.Length; i++)
                {
                    DocumentData document = Documents[i];

                    // UPDATE TASK BOARDS
                    List<BoardData> boardsData = GetAllBoards();
                    foreach (BoardData board in boardsData)
                    {
                        List<TaskItemData> tasks = board.GetTasks();
                        foreach (TaskItemData task in tasks)
                        {
                            List<DocumentData> documentsTask = task.GetData();
                            foreach (DocumentData documentTask in documentsTask)
                            {
                                if (documentTask.Equals(document))
                                {
                                    documentTask.Copy(document);
                                }
                            }
                            // DELETE NON-EXISTANT GLOBALS
                            bool somethingDelete = false;
                            for (int k = 0; k < documentsTask.Count; k++)
                            {
                                DocumentData documentTask = documentsTask[k];
                                bool existsGDoc = false;
                                if (documentTask.IsGlobal)
                                {                                    
                                    foreach (DocumentData gdoc in Documents)
                                    {
                                        if (gdoc.Equals(documentTask))
                                        {
                                            existsGDoc = true;
                                        }
                                    }
                                    if (!existsGDoc)
                                    {
                                        somethingDelete = true;
                                        documentsTask.RemoveAt(k);
                                        k--;
                                    }
                                }
                            }
                            if (somethingDelete)
                            {
                                task.SetData(documentsTask.ToArray());
                            }
                        }
                    }

                    // UPDATE CALENDAR MEETINGS
                    List<MeetingData> meetings = GetMeetings();
                    foreach (MeetingData meeting in meetings)
                    {
                        // UPDATE DOCUMENTS
                        List<DocumentData> documentsTask = meeting.GetData();
                        foreach (DocumentData documentTask in documentsTask)
                        {
                            if (documentTask.Equals(document))
                            {
                                documentTask.Copy(document);
                            }
                        }
                        // DELETE NON-EXISTANT GLOBALS
                        bool somethingDelete = false;
                        for (int k = 0; k < documentsTask.Count; k++)
                        {
                            DocumentData documentTask = documentsTask[k];
                            bool existsGDoc = false;
                            if (documentTask.IsGlobal)
                            {
                                foreach (DocumentData gdoc in Documents)
                                {
                                    if (gdoc.Equals(documentTask))
                                    {
                                        existsGDoc = true;
                                    }
                                }
                                if (!existsGDoc)
                                {
                                    somethingDelete = true;
                                    documentsTask.RemoveAt(k);
                                    k--;
                                }
                            }
                        }
                        if (somethingDelete)
                        {
                            meeting.SetData(documentsTask.ToArray());
                        }
                    }                    
                }
            }
        }

        public void ClearMeetings()
        {
            SetMeetings(new List<MeetingData>());
        }

        public void SetMeetings(List<MeetingData> meetings)
        {
            Meetings = meetings.ToArray();
        }

        public List<MeetingData> GetMeetings(bool excludeSocial = false)
        {
            List<MeetingData> meetings = new List<MeetingData>();
            if (Meetings != null)
            {
                for (int i = 0; i < Meetings.Length; i++)
                {
                    if (!excludeSocial)
                    {
                        meetings.Add(Meetings[i]);
                    }
                    else
                    {
                        if (!Meetings[i].IsSocialMeeting())
                        {
                            meetings.Add(Meetings[i]);
                        }
                    }                    
                }
            }
            return meetings;
        }

        public string PackMeetings(string tagxml, bool done, bool task, bool social, DateTime limitDate)
        {
            string output = "";
            if (Meetings != null)
            {
                DateTime currTime = WorkDayData.Instance.CurrentProject.GetCurrentTime();
                for (int i = 0; i < Meetings.Length; i++)
                {
                    MeetingData meeting = Meetings[i];

                    bool shouldAdd = false;                    
                    if (!done)
                    {
                        if (social)
                        {
                            if (meeting.IsSocialMeeting())
                            {
                                shouldAdd = true;
                            }
                        }
                        else
                        {
                            if (!meeting.IsSocialMeeting())
                            {
                                shouldAdd = true;
                            }
                        }
                    }
                    else
                    {
                        if (meeting.Completed)
                        {
                            if (social)
                            {
                                if (meeting.IsSocialMeeting())
                                {
                                    shouldAdd = true;
                                }
                            }
                            else
                            {
                                if (!meeting.IsSocialMeeting())
                                {
                                    shouldAdd = true;
                                }
                            }
                        }
                    }
                    
                    if (meeting.GetTimeStart() > limitDate)
                    {
                        shouldAdd = false;
                    }

                    if (!task)
                    {
                        if (meeting.TaskId != -1)
                        {
                            shouldAdd = false;
                        }
                    }

                    if (shouldAdd)
                    {
                        output += "<" + tagxml + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + meeting.Name + "\" " + PromptController.Instance.GetText("xml.tag.date") + " =\"" + currTime.ToLongDateString() + "\">";
                        if (meeting.Completed)
                        {
                            output += meeting.Summary;
                        }
                        else
                        {
                            output += meeting.Description;
                        }
                        output += "<" + tagxml + "/>\n";
                    }
                }
            }
            return output;
        }

        public string PackTasks(string tagxml, int projectId, TaskStates state, int exceptionTask = -1)
        {
            string output = "";
            if (Boards != null)
            {
                foreach (BoardData board in Boards)
                {
                    if (board.ProjectId == projectId)
                    {
                        List<TaskItemData> tasks = board.GetTasks();
                        foreach (TaskItemData task in tasks)
                        {
                            if (task.UID != exceptionTask)
                            {
                                bool shouldAdd = false;
                                switch (state)
                                {
                                    case TaskStates.TODO:
                                        shouldAdd = task.IsTaskToDo();
                                        break;

                                    case TaskStates.DOING:
                                        shouldAdd = task.IsTaskDoing();
                                        break;

                                    case TaskStates.DONE:
                                        shouldAdd = task.IsTaskCompleted();
                                        break;
                                }

                                if (shouldAdd)
                                {
                                    if (task.IsTaskCompleted())
                                    {
                                        output += "<" + tagxml + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + task.Name + "\">";
                                        output += task.Summary;
                                        output += "</" + tagxml + ">";
                                    }
                                    else
                                    {
                                        output += "<" + tagxml + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + task.Name + "\">";
                                        output += task.Description;
                                        output += "</" + tagxml + ">";
                                    }
                                    output += "\n";
                                }
                            }
                        }
                    }
                }
            }
            return output;
        }

        public MeetingData GetMeeting(string name, DateTime date)
        {
            if (Meetings != null)
            {
                for (int i = 0; i < Meetings.Length; i++)
                {
                    if (Meetings[i].Name.Equals(name))
                    {
                        if (date.DayOfYear == Meetings[i].GetTimeStart().DayOfYear)
                        {
                            return Meetings[i];
                        }                        
                    }
                }
            }
            return null;
        }

        public bool RemoveMeetingByUID(string uid)
        {
            bool output = false;
            if (Meetings != null)
            {
                MeetingData meetingTarget = null;
                List<MeetingData> meetings = GetMeetings();
                for (int i = 0; i < meetings.Count; i++)
                {
                    if (meetings[i].GetUID().Equals(uid))
                    {
                        output = true;
                        meetingTarget = meetings[i];
                        meetings.RemoveAt(i);
                        SetMeetings(meetings);
                        break;
                    }
                }
                if (output)
                {
                    if (meetingTarget != null)
                    {
                        string imagesToDelete = meetingTarget.GetImagesLinked();
                        if ((imagesToDelete != null) && (imagesToDelete.Length > 0))
                        {
                            WorkDayData.Instance.DeleteImage(imagesToDelete);
                        }
                    }
                }
            }
            return output;
        }

        public void RemoveMeetingsByProject(int projectID)
        {
            string imagesLinkedToMeetings = "";
            List<MeetingData> meetings = GetMeetings();
            if (meetings != null)
            {
                for (int i = 0; i < meetings.Count; i++)
                {
                    if (meetings[i].ProjectId == projectID)
                    {
                        meetings.RemoveAt(i);
                        i--;
                    }
                }
            }
            if ((imagesLinkedToMeetings != null) && (imagesLinkedToMeetings.Length > 0))
            {
                WorkDayData.Instance.DeleteImage(imagesLinkedToMeetings);
            }
            SetMeetings(meetings);
        }

        public MeetingData GetMeetingByUID(string uid)
        {
            if (Meetings != null)
            {
                for (int i = 0; i < Meetings.Length; i++)
                {
                    if (Meetings[i].GetUID().Equals(uid))
                    {
                        return Meetings[i];
                    }
                }
            }
            return null;
        }

        public List<MeetingData> GetMeetings(string name)
        {
            List<MeetingData> meetingsWithName = new List<MeetingData>();
            if (Meetings != null)
            {
                foreach (MeetingData meeting in Meetings)
                {
                    if (meeting.Name.Equals(name))
                    {
                        meetingsWithName.Add(meeting);
                    }
                }
            }
            return meetingsWithName;
        }

        public List<MeetingData> GetMeetings(DateTime date)
        {
            List<MeetingData> meetingsForDate = new List<MeetingData>();
            if (Meetings != null)
            {
                foreach (MeetingData meeting in Meetings)
                {
                    DateTime timeMeeting = meeting.GetTimeStart();
                    if ((timeMeeting.Year == date.Year) && (timeMeeting.Month == date.Month) && (timeMeeting.Day == date.Day))
                    {
                        meetingsForDate.Add(meeting);
                    }
                }
            }
            return meetingsForDate;
        }

        public List<MeetingData> GetMeetingsByTaskUID(int taskUID)
        {
            List<MeetingData> meetingsForTask = new List<MeetingData>();
            if (Meetings != null)
            {
                foreach (MeetingData meeting in Meetings)
                {
                    if (meeting.TaskId == taskUID)
                    {
                        meetingsForTask.Add(meeting);
                    }
                }
            }
            return meetingsForTask;
        }        

        public MeetingData GetMeetings(DateTime date, string name)
        {
            List<MeetingData> meetingsForDate = GetMeetings(date);
            if (Meetings != null)
            {
                foreach (MeetingData meeting in Meetings)
                {
                    if (meeting.Name.Equals(name))
                    {
                        return meeting;
                    }
                }
            }
            return null;
        }

        public (TaskItemData, string) GetTaskItemDataByUID(int taskUID)
        {
            List<BoardData> boards = GetAllBoards();
            foreach (BoardData board in boards)
            {
                TaskItemData task = board.GetTaskByUID(taskUID);
                if (task != null)
                {
                    return (task, board.BoardName);
                }
            }
            return (null, null);
        }

        public (TaskItemData, string) GetTaskItemDataName(string taskName)
        {
            List<BoardData> boards = GetAllBoards();
            foreach (BoardData board in boards)
            {
                TaskItemData task = board.GetTaskByName(taskName);
                if (task != null)
                {
                    return (task, board.BoardName);
                }
            }
            return (null, null);
        }

        public List<TaskItemData> GetAllTasks(BoardData targetBoard, int exceptTaskID = -1)
        {
            List<TaskItemData> tasks = new List<TaskItemData>();
            List<BoardData> boards = GetAllBoards();
            foreach (BoardData board in boards)
            {
                if ((board == targetBoard) || (targetBoard == null))
                {
                    List<TaskItemData> tasksBoard = board.GetTasks();
                    foreach (TaskItemData taskInBoard in tasksBoard)
                    {
                        if (exceptTaskID != -1)
                        {
                            if (exceptTaskID != taskInBoard.UID) tasks.Add(taskInBoard);
                        }
                        else
                        {
                            tasks.Add(taskInBoard);
                        }
                    }
                }
            }
            return tasks;
        }

        public List<(TaskItemData, BoardData)> GetAllTasksAssignedTo(string human)
        {
            List<(TaskItemData, BoardData)> humanTasks = new List<(TaskItemData, BoardData)>();
            List<BoardData> boards = GetAllBoards();
            foreach (BoardData board in boards)
            {
                List<TaskItemData> tasksBoard = board.GetTasks();
                foreach (TaskItemData taskInBoard in tasksBoard)
                {
                    if (taskInBoard.IsMemberOfTask(human))
                    {
                        if (!taskInBoard.IsTaskCompleted())
                        {
                            humanTasks.Add((taskInBoard, board));
                        }
                    }
                }
            }
            return humanTasks;
        }


        public ProjectInfoData GetProjectByTaskItemUID(int taskUID)
        {
            List<BoardData> boards = GetAllBoards();
            foreach (BoardData board in boards)
            {
                TaskItemData task = board.GetTaskByUID(taskUID);
                if (task != null)
                {
                    return GetProject(board.ProjectId);
                }
            }
            return null;
        }

        public List<MeetingData> GetMeetingsForHuman(DateTime currentTime, params string[] humans)
        {
            List<MeetingData> meetings = new List<MeetingData>();
            if (Meetings != null)
            {
                for (int i = 0; i < Meetings.Length; i++)
                {
                    MeetingData meeting = Meetings[i];
                    if (!meeting.Completed && (currentTime < meeting.GetTimeEnd()))
                    {
                        List<string> members = meeting.Members.ToList<string>();
                        if (humans == null)
                        {
                            if (!meetings.Contains(meeting)) meetings.Add(meeting);
                        }
                        else
                        {
                            if (humans.Length == 0)
                            {
                                if (!meetings.Contains(meeting)) meetings.Add(meeting);
                            }
                            else
                            {
                                foreach (string human in humans)
                                {
                                    if (meeting.IsMemberInMeeting(human))
                                    {
                                        if (!meetings.Contains(meeting)) meetings.Add(meeting);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            List<MeetingData> sortedMeetings = meetings.OrderBy(meeting => meeting.GetTimeStart()).ToList();
            return sortedMeetings;
        }

        public void SetHumanControlled(WorldItemData human, bool isPlayer)
        {
            if (Items != null)
            {
                foreach (WorldItemData item in Items)
                {
                    if (item.IsHuman)
                    {
                        item.IsPlayer = false;
                    }
                    if (item == human)
                    {
                        item.IsPlayer = isPlayer;
                    }
                }
            }
        }

        public WorldItemData GetHumanControlled()
        {
            if (Items != null)
            {
                foreach (WorldItemData item in Items)
                {
                    if (item.IsHuman && item.IsPlayer)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public void DeleteMeetingsLinkedToTaskID(int taskUID)
        {
            string imagesLinkedToMeetings = "";
            List<MeetingData> meetings = GetMeetings();
            if (meetings != null)
            {
                for (int i = 0; i < meetings.Count; i++)
                {
                    if (meetings[i].TaskId == taskUID)
                    {
                        string imagesInteractions = meetings[i].GetImagesLinked();
                        if ((imagesInteractions != null) && (imagesInteractions.Length > 0))
                        {
                            if (imagesLinkedToMeetings.Length > 0) imagesLinkedToMeetings += ";";
                            imagesLinkedToMeetings += imagesInteractions;
                        }
                        meetings.RemoveAt(i);
                        i--;
                    }
                }
            }
            if ((imagesLinkedToMeetings != null) && (imagesLinkedToMeetings.Length > 0))
            {
                WorkDayData.Instance.DeleteImage(imagesLinkedToMeetings);
            }
            SetMeetings(meetings);
        }

        public void DeleteMeetingsLinkedToBoard(string boardDeleted)
        {
            string imagesLinkedToMeetings = "";
            List<MeetingData> meetings = GetMeetings();
            if (meetings != null)
            {
                for (int i = 0; i < meetings.Count; i++)
                {
                    if (meetings[i].TaskId != -1)
                    {
                        var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(meetings[i].TaskId);
                        if (boardName.Length > 0)
                        {
                            if (boardDeleted.Equals(boardName))
                            {
                                string imagesInteractions = meetings[i].GetImagesLinked();
                                if ((imagesInteractions != null) && (imagesInteractions.Length > 0))
                                {
                                    if (imagesLinkedToMeetings.Length > 0) imagesLinkedToMeetings += ";";
                                    imagesLinkedToMeetings += imagesInteractions;
                                }
                                meetings.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }
            }
            if ((imagesLinkedToMeetings != null) && (imagesLinkedToMeetings.Length > 0))
            {
                WorkDayData.Instance.DeleteImage(imagesLinkedToMeetings);
            }
            SetMeetings(meetings);
        }

        public bool IsImageInGlobalDocuments(int idTargetImage)
        {
            if (Documents != null)
            {
                for (int i = 0; i < Documents.Length; i++)
                {
                    int idImage = Documents[i].GetImageID();
                    if ((idImage != -1) && (idImage == idTargetImage))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsImageInSystemData(int idTargetImage)
        {
            if (IsImageInGlobalDocuments(idTargetImage))
            {
                return true;
            }
            List<BoardData> boards = GetAllBoards();
            foreach (BoardData board in boards)
            {
                List<TaskItemData> tasks = board.GetTasks();
                foreach (TaskItemData task in tasks)
                {
                    if (task.IsImageLinked(idTargetImage))
                    {
                        return true;
                    }
                }
            }
            List<MeetingData> meetings = GetMeetings();
            foreach (MeetingData meeting in meetings)
            {
                if (meeting.IsImageLinked(idTargetImage))
                {
                    return true;
                }
            }
            return false;
        }

        public float GetTotalLoggedTimeForTask(int projectID, int taskUID)
        {
            float totalTimeForTask = 0;
            foreach (WorldItemData human in Items)
            {
                if (human.IsHuman)
                {
                    totalTimeForTask += human.GetTotalDecimalHoursProgressForTask(projectID, taskUID);
                }
            }
            return totalTimeForTask;
        }

        public List<TimeWorkingDataDisplay> GetAllLogsWorkForTask(int projectID, int taskUID)
        {             
            List<TimeWorkingDataDisplay> logsWork = new List<TimeWorkingDataDisplay>();
            foreach (WorldItemData human in Items)
            {
                if (human.IsHuman)
                {
                    List<TimeWorkingDataDisplay> logsForHuman = human.GetAllLogsWorkForTask(projectID, taskUID, human.Name);
                    if (logsForHuman.Count > 0)
                    {
                        logsWork.AddRange(logsForHuman);
                    }                    
                }
            }
            return logsWork;
        }

        public void DeleteTaskWorkLogs(int projectID, int taskUID)
        {
            foreach (WorldItemData human in Items)
            {
                if (human.IsHuman)
                {
                    human.DeleteWorkingLogs(projectID, taskUID);
                }
            }
        }

        public List<string> GetHumansWorkingInTask(int taskUID)
        {
            List<string> humanWorkingInTask = new List<string>();
            foreach (WorldItemData human in Items)
            {
                if (human.IsHuman)
                {
                    TaskProgressData taskProgress = human.GetActiveTask();
                    if (taskProgress != null)
                    {
                        if (taskProgress.TaskUID == taskUID)
                        {
                            if (!humanWorkingInTask.Contains(human.Name))
                            {
                                humanWorkingInTask.Add(human.Name);
                            }
                        }
                    }
                }
            }
            return humanWorkingInTask;
        }

        public List<string> GetHumansAssignedToTask(int taskUID)
        {
            List<TaskItemData> allTasks = WorkDayData.Instance.CurrentProject.GetAllTasks(null);
            List<string> humanAssignedToTask = new List<string>();
            foreach (TaskItemData task in allTasks)
            {
                if (task.UID == taskUID)
                {
                    return task.GetMembers();
                }
                
            }
            return null;
        }

        public void StartProgressTask(string nameHuman, int taskUID, DateTime startWorking)
        {
            var (taskData, boardName) = GetTaskItemDataByUID(taskUID);
            WorldItemData itemData = GetItemByName(nameHuman);
            if ((taskData != null) && (itemData != null))
            {
                if (itemData.IsHuman)
                {
                    taskData.State = (int)TaskStates.DOING;
                    taskData.AddNewMember(nameHuman);
                    BoardData boardOfTaskToStart = GetBoardFor(boardName);
                    if (boardOfTaskToStart != null)
                    {
                        TaskProgressData taskWorking = new TaskProgressData(boardOfTaskToStart.ProjectId, taskUID, startWorking, nameHuman);
                        List<TaskProgressData> loggedWork = itemData.GetLoggedWork();
                        if (!loggedWork.Contains(taskWorking))
                        {
                            loggedWork.Add(taskWorking);
                            itemData.SetLoggedWork(loggedWork);
                        }
                        else
                        {
                            taskWorking = loggedWork.Find(task => task.TaskUID == taskUID);
                            taskWorking.SetStartTime(startWorking);
                            taskWorking.Working = true;
                        }
                    }
                    UIEventController.Instance.DispatchUIEvent(CalendarSchedulerItem.EventCalendarSchedulerItemRefresh);
                    UIEventController.Instance.DispatchUIEvent(ItemTaskView.EventItemTaskViewAllRefresher);
                    SystemEventController.Instance.DispatchSystemEvent(TasksController.EventTasksControllerStartedTask, itemData, taskData);
                }
            }
        }

        public WorldItemData GetItemByName(string nameItem)
        {
            string finalName = nameItem.ToLower();
            if (Items != null)
            {
                foreach (WorldItemData item in Items)
                {
                    if (item != null)
                    {
                        if (item.Name.ToLower().Equals(finalName))
                        {
                            return item;
                        }
                    }
                }
            }
            return null;
        }

        public string GetClosestName(string nameItem, int percentage, bool isHuman)
        {
            string finalName = nameItem.ToLower();
            if (Items != null)
            {
                foreach (WorldItemData item in Items)
                {
                    if (item != null)
                    {
                        bool check = true;
                        if (isHuman)
                        {
                            check = item.IsHuman;
                        }

                        if (check)
                        {
                            if (item.Name.ToLower().Equals(finalName))
                            {
                                return item.Name;
                            }
                        }
                    }
                }
                foreach (WorldItemData item in Items)
                {
                    if (item != null)
                    {
                        bool check = true;
                        if (isHuman)
                        {
                            check = item.IsHuman;
                        }

                        if (check)
                        {
                            if (StringSimilarity.CalculateSimilarityPercentage(item.Name.ToLower(), finalName) >= percentage)
                            {
                                return item.Name;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public string GetClosestName(string nameItem, bool isHuman)
        {
            int percentage = 0;
            string finalName = nameItem.ToLower();
            string outputName = "";
            if (Items != null)
            {
                foreach (WorldItemData item in Items)
                {
                    if (item != null)
                    {
                        bool check = true;
                        if (isHuman)
                        {
                            check = item.IsHuman;
                        }
                        
                        if (check)
                        {
                            if (item.Name.ToLower().Equals(finalName))
                            {
                                percentage = 1000;
                                outputName = item.Name;
                            }
                            else
                            {
                                int currPercentage = (int)StringSimilarity.CalculateSimilarityPercentage(item.Name.ToLower(), finalName);
                                if (currPercentage >= percentage)
                                {
                                    percentage = currPercentage;
                                    outputName = item.Name;
                                }
                            }
                        }
                    }
                }
            }
            return outputName;
        }

        public void ResetAllMeetings()
        {
            List<MeetingData> meetings = GetMeetings();
            for (int i = 0; i < meetings.Count; i++)
            {
                MeetingData meeting = meetings[i];
                bool shouldReset = true;
                if (meeting.IsSocialMeeting())
                {
                    if (meeting.GetTotalMinutes() < 30)
                    {
                        shouldReset = false;
                        meetings.RemoveAt(i);
                        i--;
                    }
                }
                if (shouldReset)
                {
                    meeting.Reset();
                }
            }
            SetMeetings(meetings);
        }

        public void ResetAllBoards()
        {
            List<BoardData> boards = GetAllBoards();
            for (int i = 0; i < boards.Count; i++)
            {
                BoardData board = boards[i];
                if (board != null)
                {
                    board.Reset();
                }
            }
        }

        public void CalculateTasksDepth()
        {
            List<BoardData> boards = GetAllBoards();
            for (int i = 0; i < boards.Count; i++)
            {
                BoardData board = boards[i];
                if (board != null)
                {
                    board.CalculateDepth();
                }
            }
        }

        public void ClearCost()
        {
            List<CostAIOperation> costData = new List<CostAIOperation>();
            Cost = costData.ToArray();
        }

        public void AddNewCost(float cost, string operation, string llmProvider, int inputTokens, int outputTokens)
        {
            List<CostAIOperation> costData;
            if (Cost == null)
            {
                costData = new List<CostAIOperation>();
            }
            else
            {
                costData = Cost.ToList<CostAIOperation>();
            }
            costData.Add(new CostAIOperation(operation, llmProvider, cost, inputTokens, outputTokens));
            Cost = costData.ToArray();
        }

        public float GetTotalCost()
        {
            float total = 0;
            if (Cost != null)
            {
                foreach (CostAIOperation singleOperation in Cost)
                {
                    if (singleOperation != null)
                    {
                        total += singleOperation.Cost;
                    }
                }
            }
            return total;
        }

        public int CalculateStorage()
        {
            if (Storage == null) Storage = new StorageUsed();
            return Storage.Data + Storage.Images;
        }
        
        public bool ExistImageInDocuments(int image)
        {
            if (Documents != null)
            {
                foreach (DocumentData doc in Documents)
                {
                    if (doc.IsImage)
                    {
                        int idImage = int.Parse(doc.Data.GetHTML());
                        if (image == idImage)
                        {
                            return true;
                        }
                    }
                }
            }

            if (Boards != null)
            {
                foreach (BoardData board in Boards)
                {
                    if (board.Tasks != null)
                    {
                        foreach (TaskItemData task in board.Tasks)
                        {
                            if (task.Data != null)
                            {
                                foreach (DocumentData doc in task.Data)
                                {
                                    if (doc.IsImage)
                                    {
                                        int idImage = int.Parse(doc.Data.GetHTML());
                                        if (image == idImage)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (Meetings != null)
            {
                foreach (MeetingData meeting in Meetings)
                {
                    if (meeting.Interactions != null)
                    {
                        foreach (InteractionData interaction in meeting.Interactions)
                        {
                            if (interaction != null)
                            {
                                if (interaction.CheckImage(image))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    if (meeting.Data != null)
                    {
                        foreach (DocumentData doc in meeting.Data)
                        {
                            if (doc.IsImage)
                            {
                                int idImage = int.Parse(doc.Data.GetHTML());
                                if (image == idImage)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        public void ClearCurrentProgress()
        {
            SetCurrentDocProgress(new List<CurrentDocumentInProgress>());
        }

        public List<CurrentDocumentInProgress> GetCurrentDocProgress()
        {
            if (CurrentDocProgress != null)
            {
                return CurrentDocProgress.ToList<CurrentDocumentInProgress>();
            }
            else
            {
                return new List<CurrentDocumentInProgress>();
            }
        }

        public void SetCurrentDocProgress(List<CurrentDocumentInProgress> currDocProgress)
        {
            CurrentDocProgress = currDocProgress.ToArray();
        }

        public CurrentDocumentInProgress GetCurrentDocProgress(int uid)
        {
            if (CurrentDocProgress != null)
            {
                foreach (CurrentDocumentInProgress docProgress in CurrentDocProgress)
                {
                    if (docProgress.UID == uid)
                    {
                        return docProgress;
                    }
                }
            }
            return null;
        }

        public bool IsCurrentDocProgressCompleted(int uid)
        {
            if (CurrentDocProgress != null)
            {
                foreach (CurrentDocumentInProgress docProgress in CurrentDocProgress)
                {
                    if (docProgress.UID == uid)
                    {
                        return docProgress.IsDone();
                    }
                }
            }
            return false;
        }

        public bool IsCurrentDocProgressCompleted(string nameProgress)
        {
            if (CurrentDocProgress != null)
            {
                foreach (CurrentDocumentInProgress docProgress in CurrentDocProgress)
                {
                    if (docProgress.Name.ToLower().Equals(nameProgress.ToLower()))
                    {
                        return docProgress.IsDone();
                    }
                }
            }
            return false;
        }

        public bool RemoveDocProgress(CurrentDocumentInProgress docToDelete)
        {
            List<CurrentDocumentInProgress> progressDocs = GetCurrentDocProgress();
            for (int i = 0; i < progressDocs.Count; i ++)
            {
                CurrentDocumentInProgress doc = progressDocs[i];
                if (doc != null)
                {
                    if (doc.Equals(docToDelete))
                    {
                        int taskToDelete = docToDelete.TaskID;
                        progressDocs.RemoveAt(i);
                        SetCurrentDocProgress(progressDocs);
                        AdjustEstimationTask(taskToDelete);
                        return true;
                    }
                }
            }
            return false;
        }

        public bool RemoveDocProgressTask(int taskID)
        {
            List<CurrentDocumentInProgress> progressDocs = GetCurrentDocProgress();
            for (int i = 0; i < progressDocs.Count; i++)
            {
                CurrentDocumentInProgress doc = progressDocs[i];
                if (doc != null)
                {
                    if (doc.TaskID == taskID)
                    {
                        progressDocs.RemoveAt(i);
                        SetCurrentDocProgress(progressDocs);
                        return true;
                    }
                }
            }
            return false;
        }

        public int AddDocProgress(CurrentDocumentInProgress docToAdd)
        {
            bool shouldAdd = true;
            List<CurrentDocumentInProgress> progressDocs = GetCurrentDocProgress();
            for (int i = 0; i < progressDocs.Count; i++)
            {
                CurrentDocumentInProgress doc = progressDocs[i];
                if (doc != null)
                {
                    if (doc.Equals(docToAdd))
                    {
                        shouldAdd = false;
                    }
                }
            }
            if (shouldAdd)
            {
                progressDocs.Add(docToAdd);
                SetCurrentDocProgress(progressDocs);
                AdjustEstimationTask(docToAdd.TaskID);
            }
            return docToAdd.UID;
        }

        public void AdjustEstimationTask(int taskID)
        {
            List<CurrentDocumentInProgress> progressDocs = GetCurrentDocProgress();
            int newEstimate = 0;
            foreach (CurrentDocumentInProgress currProgress in progressDocs)
            {
                if (currProgress.TaskID == taskID)
                {
                    newEstimate += currProgress.Time;
                }
            }
            var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(taskID);
            if (taskItemData != null)
            {
                taskItemData.EstimatedTime = newEstimate;
            }
        }

        public void Reset()
        {
            CurrentProgressNextID = 0;
            ClearCurrentProgress();
        }
    }
}