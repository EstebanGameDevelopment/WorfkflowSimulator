using System;
using System.Collections.Generic;
using UnityEngine;

namespace yourvrexperience.WorkDay
{
    [System.Serializable]
    public class InteractionData
    {
        public bool IsAI;
        public string NameActor;        
        public string Text;
        public string Data;
        public string Summary;
        public string Date;

        public InteractionData(bool isAI, string nameActor, string text, string data, string summary, DateTime date)
        {
            IsAI = isAI;
            NameActor = nameActor;
            Text = text;
            Data = data;
            Summary = summary;
            SetDate(date);
        }

        public DateTime GetDate()
        {
            if (Date.Length > 0)
            {
                return DateTime.Parse(Date);
            }
            else
            {
                return DateTime.Now;
            }
        }
        public void SetDate(DateTime time)
        {
            Date = time.ToString("o"); // "o" = ISO 8601 format
        }

        public bool CheckImage(int imageID)
        {
            int idImage = ScreenMultiInputDataView.GetImageFromText(Data);
            if (idImage != -1)
            {
                return (imageID == idImage);
            }
            else
            {
                return false;
            }
        }
    }

    [System.Serializable]
    public class MeetingData
    {
        public string Name;
        public int ProjectId;
        public int TaskId;
        public string Description;
        public string TimeStart; // "o" = ISO 8601 format
        public string TimeEnd; // "o" = ISO 8601 format
        public string[] Members;
        public InteractionData[] Interactions;
        public DocumentData[] Data;
        public string Summary;
        public bool InProgress;
        public int Iterations;        
        public float DelayIterations;        
        public int TotalIterations;
        public bool Completed;
        public string ExtraData;
        public bool IsProcessingAI;
        public bool Requested;
        public string RoomName;
        public bool CanClose;
        public bool CanLeave;
        public bool FindRoom;
        public bool StartDialogScreenForPlayer;
        public int ShouldCreateDocuments;
        public bool InitiatedByPlayer;
        public bool IsUserCreated;

        public MeetingData(string name, int projectId, int taskId, string description, DocumentData[] data, DateTime timeStart, DateTime timeEnd, bool canClose, bool canLeave, bool findRoom, bool startDialogForPlayer, bool initiatedByPlayer, params string[] members)
        {
            Name = name;
            ProjectId = projectId;
            TaskId = taskId;
            Description = description;
            Data = data;
            TimeStart = timeStart.ToString("o"); // "o" = ISO 8601 format
            TimeEnd = timeEnd.ToString("o"); // "o" = ISO 8601 format
            if (members != null)
            {
                Members = new string[members.Length];
                for (int i = 0; i < members.Length; i++)
                {
                    Members[i] = members[i];
                }
            }
            InProgress = false;
            Completed = false;
            IsProcessingAI = false;
            Requested = false;
            CanClose = canClose;
            CanLeave = canLeave;
            FindRoom = findRoom;
            StartDialogScreenForPlayer = startDialogForPlayer;
            ShouldCreateDocuments = 0;
            InitiatedByPlayer = initiatedByPlayer;
            IsUserCreated = false;
        }

        public string GetUID()
        {
            return Name + TimeStart;
        }

        public DateTime GetTimeStart()
        {
            return DateTime.Parse(TimeStart);
        }
        public DateTime GetTimeEnd()
        {
            return DateTime.Parse(TimeEnd);
        }
        public void SetTimeStart(DateTime time)
        {
            TimeStart = time.ToString("o"); // "o" = ISO 8601 format
        }
        public void SetTimeEnd(DateTime time)
        {
            TimeEnd = time.ToString("o"); // "o" = ISO 8601 format
        }
        public int GetTotalHours()
        {
            return (int)((GetTimeEnd() - GetTimeStart()).TotalHours);
        }
        public int GetTotalMinutes()
        {
            return (int)((GetTimeEnd() - GetTimeStart()).TotalMinutes);
        }
        public int GetTotalSeconds()
        {
            return (int)((GetTimeEnd() - GetTimeStart()).TotalSeconds);
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

        public bool AddMember(string member)
        {
            List<string> members = GetMembers();
            if (!members.Contains(member))
            {
                members.Add(member);
                SetMembers(members);
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<string> GetAssistingMembers(bool includePlayer)
        {
            if ((ExtraData == null) || (ExtraData.Length == 0))
            {
                return GetMembers();
            }

            string[] assistingMembers = ExtraData.Split(",");
            List<string> assisting = new List<string>();
            foreach (string assistant in assistingMembers)
            {
                if ((ApplicationController.Instance.HumanPlayer != null) && !includePlayer)
                {
                    if (!assistant.Equals(ApplicationController.Instance.HumanPlayer.NameHuman))
                    {
                        assisting.Add(assistant);
                    }
                }
                else
                {
                    assisting.Add(assistant);
                }                
            }
            return assisting;
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

        public string GetMembersPacket(string humanName = "", bool includeHuman = true, bool includeGroup = false)
        {
            string totalHumans = "";
            if (includeHuman)
            {
                totalHumans += humanName;
            }            
            if (Members != null)
            {
                foreach (string member in Members)
                {
                    if (!member.Equals(humanName) || (humanName.Length == 0))
                    {
                        GroupInfoData groupHumans = WorkDayData.Instance.CurrentProject.GetGroupByName(member);
                        if (groupHumans != null)
                        {
                            foreach (string memberOfGroup in groupHumans.Members)
                            {
                                if (!memberOfGroup.Equals(humanName) || (humanName.Length == 0))
                                {
                                    if (totalHumans.Length > 0) totalHumans += ",";
                                    totalHumans += memberOfGroup;
                                    if (includeGroup)
                                    {
                                        totalHumans += "(" + member + ")";
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (totalHumans.Length > 0) totalHumans += ",";
                            totalHumans += member;
                            GroupInfoData groupOfSingleMember = WorkDayData.Instance.CurrentProject.GetGroupOfMember(member);
                            if (includeGroup)
                            {
                                if (groupOfSingleMember != null)
                                {
                                    totalHumans += "(" + groupOfSingleMember.Name + ")";
                                }
                            }
                        }
                    }
                }
            }
            return totalHumans;
        }

        public string GetMembersNotInAMeetingPacket(string humanName = "", bool includeHuman = true, bool includeGroup = false)
        {
            string totalHumans = "";
            if (includeHuman)
            {
                totalHumans += humanName;
            }
            if (Members != null)
            {
                foreach (string member in Members)
                {
                    if (!member.Equals(humanName) || (humanName.Length == 0))
                    {
                        GroupInfoData groupHumans = WorkDayData.Instance.CurrentProject.GetGroupByName(member);
                        if (groupHumans != null)
                        {
                            foreach (string memberOfGroup in groupHumans.Members)
                            {
                                if (!memberOfGroup.Equals(humanName) || (humanName.Length == 0))
                                {                                    
                                    MeetingData meeting = MeetingController.Instance.GetMeetingOfHuman(memberOfGroup);
                                    if (meeting == null)
                                    {
                                        if (totalHumans.Length > 0) totalHumans += ",";
                                        totalHumans += memberOfGroup;
                                        if (includeGroup)
                                        {
                                            totalHumans += "("+ member + ")";
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            MeetingData meeting = MeetingController.Instance.GetMeetingOfHuman(member);
                            if (meeting == null)
                            {
                                if (totalHumans.Length > 0) totalHumans += ",";
                                totalHumans += member;
                                GroupInfoData groupOfSingleMember = WorkDayData.Instance.CurrentProject.GetGroupOfMember(member);
                                if (includeGroup)
                                {
                                    if (groupOfSingleMember != null)
                                    {
                                        totalHumans += "(" + groupOfSingleMember.Name + ")";
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return totalHumans;
        }

        public void SetInteractions(List<InteractionData> interactions)
        {
            Interactions = interactions.ToArray();
        }

        public List<InteractionData> GetInteractions()
        {
            List<InteractionData> interactions = new List<InteractionData>();
            if (Interactions != null)
            {
                for (int i = 0; i < Interactions.Length; i++)
                {
                    interactions.Add(Interactions[i]);
                }
            }
            return interactions;
        }

        public string PackXMLInteractions(bool colorize, string actor = "")
        {
            string output = "";
            List<InteractionData> interactions = new List<InteractionData>();
            output += "<" + PromptController.Instance.GetText("xml.tag.meeting");
            output += " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + Name + "\"";
            output += ">\n";
            if (Interactions != null)
            {
                for (int i = 0; i < Interactions.Length; i++)
                {
                    output += "<" + PromptController.Instance.GetText("xml.tag.reply");
                    if (colorize && (Interactions[i].NameActor.Equals(actor)))
                    {
                        output += " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + Interactions[i].NameActor.ToUpper() + "\"";
                    }
                    else
                    {
                        output += " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + Interactions[i].NameActor + "\"";
                    }
                    output += ">\n";
                    output += Interactions[i].Text;
                    output += "</" + PromptController.Instance.GetText("xml.tag.reply") + ">\n\n";
                }
            }
            output += "</" + PromptController.Instance.GetText("xml.tag.meeting") + ">\n\n";
            return output;
        }

        public void DebugLog()
        {
            string membersList = "";
            if (Members != null)
            {
                foreach (string member in Members)
                {
                    membersList += member + ",";
                }
            }
            Debug.Log("MEETING[" + Name + "]::[" + GetTimeStart().ToShortDateString() + "][" + GetTimeStart().DayOfWeek.ToString() + "][" + GetTimeStart().Hour + "]::MEMBERS[" + membersList + "]");
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

        public bool IsMemberInMeeting(string member)
        {
            if (Members != null)
            {
                for (int i = 0; i < Members.Length; i++)
                {
                    string nameMember = Members[i];
                    GroupInfoData groupMember = WorkDayData.Instance.CurrentProject.GetGroupByName(Members[i]);
                    if (groupMember == null)
                    {
                        if (nameMember.Equals(member))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (groupMember.IsMember(member))
                        {
                            return true;
                        }
                        else
                        {
                            if (groupMember.Name.Equals(member))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public bool IsThereAnyMemberOfGroup(GroupInfoData group)
        {
            for (int i = 0; i < Members.Length; i++)
            {
                string nameMember = Members[i];
                if (group.IsMember(nameMember) || group.Name.Equals(nameMember))
                {
                    return true;
                }
            }
            return false;
        }

        public string GetImagesLinked()
        {
            string imagesToDelete = "";
            if (Data != null)
            {
                for (int i = 0; i < Data.Length; i++)
                {
                    DocumentData doc = Data[i];
                    if (!doc.IsGlobal)
                    {
                        int idImage = doc.GetImageID();
                        if (idImage != -1)
                        {
                            if (imagesToDelete.Length > 0) imagesToDelete += ";";
                            imagesToDelete += idImage;
                        }
                    }
                }
            }
            if (Interactions != null)
            {
                for (int i = 0; i < Interactions.Length; i++)
                {
                    string interactionData = Interactions[i].Data;
                    if ((interactionData != null) && (interactionData.Length > 0))
                    {
                        int idImage = ScreenMultiInputDataView.GetImageFromText(interactionData);
                        if (idImage != -1)
                        {
                            if (!WorkDayData.Instance.CurrentProject.IsImageInSystemData(idImage))
                            {
                                if (imagesToDelete.Length > 0) imagesToDelete += ";";
                                imagesToDelete += idImage;
                            }
                        }
                    }
                }
            }
            return imagesToDelete;
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

        public bool HasPlayer(bool considerAssisting)
        {
            if (ApplicationController.Instance.HumanPlayer == null) return false;

            bool isInvited = false;
            if (Members != null)
            {
                for (int i = 0; i < Members.Length; i++)
                {
                    string nameMember = Members[i];
                    GroupInfoData groupMember = WorkDayData.Instance.CurrentProject.GetGroupByName(Members[i]);
                    if (groupMember == null)
                    {
                        WorldItemData member = WorkDayData.Instance.CurrentProject.GetItemByName(nameMember);
                        if (member.IsHuman && member.IsPlayer)
                        {
                            isInvited = true;
                        }
                    }
                    else
                    {
                        if (groupMember.HasPlayer())
                        {
                            isInvited = true;
                        }
                    }
                }
            }
            if (considerAssisting)
            {
                if (isInvited)
                {
                    if (ApplicationController.Instance.HumanPlayer != null)
                    {
                        if ((ExtraData != null) && (ExtraData.Length > 0))
                        {
                            isInvited = ExtraData.IndexOf(ApplicationController.Instance.HumanPlayer.NameHuman) != -1;
                        }
                    }
                }
            }
            return isInvited;
        }

        public void SetIterations(int iterations, int delay)
        {
            Iterations = 0;
            TotalIterations = iterations;
            if (iterations > 0)
            {
                DelayIterations = GetTotalSeconds() / (TotalIterations * delay);                
            }
            else
            {
                DelayIterations = 0;
            }
            if (DelayIterations < 1) DelayIterations = 1;
        }
        public void Reset()
        {
            SetInteractions(new List<InteractionData>());
            SetIterations(0, 10);
            InProgress = false;
            Completed = false;
            Summary = "";
            RoomName = "";
            Requested = false;
            StartDialogScreenForPlayer = true;
            CanLeave = true;
            InitiatedByPlayer = false;
        }

        public bool IsAssistingMember(string member)
        {
            if ((ExtraData == null) || (ExtraData.Length == 0))
            {
                return false;
            }
            else
            {
                return ExtraData.IndexOf(member) != -1;
            }            
        }

        public bool IsSocialMeeting()
        {
            return ((TaskId == -1) && (ProjectId == -1));
        }

        public bool IsInterruptionMeeting()
        {
            return IsSocialMeeting() && !CanClose && !CanLeave;
        }
    }
}