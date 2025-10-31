using System;
using System.Collections.Generic;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    [System.Serializable]
    public class TaskItemData : IEquatable<TaskItemData>
    {
        public enum TaskStates { TODO = 0, DOING, DONE, VERIFIED }

        public int UID;
        public string Name;
        public int State;
        public int Progress;
        public string Description;
        public DocumentData[] Data;
        public int EstimatedTime;
        public string[] Members;
        public string Summary;
        public int Linked;
        public int Depth;
        public int Feature;
        public bool IsUserCreated;

        public TaskItemData(int uid, string name, string description, DocumentData[] data, int estimatedTime, int state, int linked, int feature, params string[] members)
        {
            UID = uid;
            Name = name;
            State = state;
            Progress = 0;
            Description = description;
            Data = data;
            EstimatedTime = estimatedTime;
            Linked = linked;
            Feature = feature;
            if (members != null)
            {
                Members = new string[members.Length];
                for (int i = 0; i < members.Length; i++)
                {
                    Members[i] = members[i];
                }
            }
            Depth = -1;
            IsUserCreated = false;
        }

        public bool Equals(TaskItemData other)
        {
            return UID == other.UID;
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

        public bool HasHumanPlayer()
        {
            List<string> realMembers = GetHumanMembers();
            if (ApplicationController.Instance.HumanPlayer != null)
            {
                return realMembers.Contains(ApplicationController.Instance.HumanPlayer.NameHuman);
            }
            else
            {
                return false;
            }            
        }
        public List<string> GetHumanMembers()
        {
            List<string> members = new List<string>();
            if (Members != null)
            {
                for (int i = 0; i < Members.Length; i++)
                {
                    string member = Members[i];
                    GroupInfoData groupHumans = WorkDayData.Instance.CurrentProject.GetGroupByName(member);
                    if (groupHumans != null)
                    {
                        foreach (string memberOfGroup in groupHumans.Members)
                        {
                            if (!members.Contains(memberOfGroup))
                            {
                                members.Add(memberOfGroup);
                            }
                        }
                    }
                    else
                    {
                        if (!members.Contains(member))
                        {
                            members.Add(member);
                        }
                    }
                }
            }
            return members;
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

        public string PackHumanMembers()
        {
            string output = "";
            List<string> members = GetHumanMembers();
            if (members.Count > 0)
            {
                for (int i = 0; i < members.Count; i++)
                {
                    if (output.Length > 0) output += ",";
                    output += members[i];
                }
            }
            return output;
        }

        public void ClearData()
        {
            SetData((new List<DocumentData>()).ToArray());
        }

        public void SetData(DocumentData[] data)
        {
            Data = data;
        }

        public List<DocumentData> GetData()
        {
            List<DocumentData> data = new List<DocumentData>();
            if (Data != null)
            {
                for (int i = 0; i < Data.Length; i++)
                {
                    data.Add(Data[i]);
                }
            }
            return data;
        }

        public DocumentData GetDataByID(int id)
        {
            if (Data != null)
            {
                for (int i = 0; i < Data.Length; i++)
                {
                    if (Data[i].Id == id)
                    {
                        return Data[i];
                    }
                }
            }
            return null;
        }

        public bool IsImageLinked(int idTargetImage)
        {
            if (Data != null)
            {
                for (int i = 0; i < Data.Length; i++)
                {
                    int idImage = Data[i].GetImageID();
                    if ((idImage != -1) && (idImage == idTargetImage))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsMemberOfTask(string memberToCheck)
        {
            if (Members != null)
            {
                for (int i = 0; i < Members.Length; i++)
                {
                    string member = Members[i];
                    GroupInfoData group = WorkDayData.Instance.CurrentProject.GetGroupByName(member);
                    if (group != null)
                    {
                        if (group.IsMember(memberToCheck))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (member.Equals(memberToCheck))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void AddNewMember(string memberToAdd)
        {
            if (!IsMemberOfTask(memberToAdd))
            {
                List<string> members = GetMembers();
                members.Add(memberToAdd);
                SetMembers(members);
            }
        }

        public bool IsTaskToDo()
        {
            return ((TaskStates)State == TaskStates.TODO);
        }

        public bool IsTaskDoing()
        {
            return ((TaskStates)State == TaskStates.DOING);
        }

        public bool IsTaskCompleted()
        {
            return (((TaskStates)State == TaskStates.DONE) || ((TaskStates)State == TaskStates.VERIFIED));
        }

        public bool IsTaskVerified()
        {
            return ((TaskStates)State == TaskStates.VERIFIED);
        }

        public void Reset()
        {
            State = (int)TaskStates.TODO;
            Progress = 0;
            Depth = -1;
            ClearData();
        }

        public void ResetDepth()
        {
            Depth = -1;
        }
    }

    [System.Serializable]
	public class BoardData : IEquatable<BoardData>
	{
        public string BoardName;
        public string Description;
        public int ProjectId;
		public TaskItemData[] Tasks;

        public BoardData(string boardName, string description, int projectId)
        {
            BoardName = boardName;
            Description = description;
            ProjectId = projectId;
        }

        public void SetTasks(TaskItemData[] tasks)
        {
            Tasks = tasks;
        }

        public List<TaskItemData> GetTasks(int stateTask = -1)
        {
            List<TaskItemData> tasks = new List<TaskItemData>();
            if (Tasks != null)
            {
                for (int i = 0; i < Tasks.Length; i++)
                {
                    if (stateTask == -1)
                    {
                        tasks.Add(Tasks[i]);
                    }
                    else
                    {
                        if (Tasks[i].State == stateTask)
                        {
                            tasks.Add(Tasks[i]);
                        }
                    }
                }
            }
            return tasks;
        }

        public TaskItemData GetTaskByUID(int taskUID)
        {
            if (Tasks != null)
            {
                for (int i = 0; i < Tasks.Length; i++)
                {
                    if (Tasks[i].UID == taskUID)
                    {
                        return Tasks[i];
                    }
                }
            }
            return null;
        }

        public TaskItemData GetTaskByName(string taskName)
        {
            if (Tasks != null)
            {
                for (int i = 0; i < Tasks.Length; i++)
                {
                    if (Tasks[i].Name == taskName)
                    {
                        return Tasks[i];
                    }
                }
            }
            return null;
        }
        

        public bool ExistTask(string taskName)
        {
            if (Tasks != null)
            {
                for (int i = 0; i < Tasks.Length; i++)
                {
                    if (Tasks[i].Name.Equals(taskName))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool DeleteTask(TaskItemData itemData)
        {
            bool output = false;
            int uidTaskToDelete = itemData.UID;
            List<TaskItemData> currentTasks = GetTasks();
            if (currentTasks != null)
            {
                for (int i = 0; i < currentTasks.Count; i++)
                {
                    if (currentTasks[i] == itemData)
                    {
                        output = true;
                        currentTasks.RemoveAt(i);
                        SetTasks(currentTasks.ToArray());
                    }
                }
            }
            

            if (output)
            {
                // UNLINK TASKS
                List<TaskItemData> updatedTasks = GetTasks();
                if (updatedTasks != null)
                {
                    for (int i = 0; i < updatedTasks.Count; i++)
                    {
                        if (updatedTasks[i].Linked == uidTaskToDelete)
                        {
                            updatedTasks[i].Linked = -1;
                        }
                    }
                }

                string imagesToDelete = "";
                if (itemData.Data != null)
                {
                    for (int i = 0; i < itemData.Data.Length; i++)
                    {
                        DocumentData doc = itemData.Data[i];
                        if (!doc.IsGlobal)
                        {
                            int idImage = doc.GetImageID();
                            if (idImage != -1)
                            {
                                if (!WorkDayData.Instance.CurrentProject.IsImageInGlobalDocuments(idImage))
                                {
                                    if (imagesToDelete.Length > 0) imagesToDelete += ";";
                                    imagesToDelete += idImage;
                                }
                            }
                        }
                    }
                }
                if (imagesToDelete.Length > 0)
                {
                    WorkDayData.Instance.DeleteImage(imagesToDelete);
                }

                SystemEventController.Instance.DispatchSystemEvent(TasksController.EventTasksControllerDeletedTaskConfirmation, WorkDayData.Instance.CurrentProject.ProjectInfoSelected, itemData.UID);
            }

            return output;
        }

        public void DeleteAllImagesForTasks()
        {
            List<TaskItemData> currentTasks = GetTasks();
            if (currentTasks != null)
            {
                string imagesToDelete = "";
                for (int i = 0; i < currentTasks.Count; i++)
                {
                    TaskItemData itemData = currentTasks[i];
                    if (itemData.Data != null)
                    {
                        for (int j = 0; j < itemData.Data.Length; j++)
                        {
                            DocumentData doc = itemData.Data[j];
                            if (!doc.IsGlobal)
                            {
                                int idImage = doc.GetImageID();
                                if (idImage != -1)
                                {
                                    if (!WorkDayData.Instance.CurrentProject.IsImageInGlobalDocuments(idImage))
                                    {
                                        if (imagesToDelete.Length > 0) imagesToDelete += ";";
                                        imagesToDelete += idImage;
                                    }
                                }
                            }
                        }
                    }
                }
                if (imagesToDelete.Length > 0)
                {
                    WorkDayData.Instance.DeleteImage(imagesToDelete);
                }
            }
        }

        public bool Equals(BoardData other)
        {
            return BoardName == other.BoardName;
        }

        public void Reset()
        {
            if (Tasks != null)
            {
                for (int i = 0; i < Tasks.Length; i++)
                {
                    Tasks[i].Reset();
                }
            }
        }

        public void ResetDepth()
        {
            if (Tasks != null)
            {
                for (int i = 0; i < Tasks.Length; i++)
                {
                    Tasks[i].ResetDepth();
                }
            }
        }

        public int CheckDepthForTaskName(int taskLinked)
        {
            if (Tasks != null)
            {
                for (int i = 0; i < Tasks.Length; i++)
                {
                    if (Tasks[i].UID.Equals(taskLinked))
                    {
                        if (Tasks[i].Linked == -1)
                        {
                            return 1;
                        }
                        else
                        {
                            return 1 + CheckDepthForTaskName(Tasks[i].Linked);
                        }
                    }
                }
            }
            return 0;
        }

        public void CalculateDepth()
        {
            if (Tasks != null)
            {
                for (int i = 0; i < Tasks.Length; i++)
                {
                    if (Tasks[i].Depth == -1)
                    {
                        if (Tasks[i].Linked == -1)
                        {
                            Tasks[i].Depth = 0;
                        }
                        else
                        {
                            Tasks[i].Depth = CheckDepthForTaskName(Tasks[i].Linked);
                        }             
                    }
                }
            }
        }
    }
}