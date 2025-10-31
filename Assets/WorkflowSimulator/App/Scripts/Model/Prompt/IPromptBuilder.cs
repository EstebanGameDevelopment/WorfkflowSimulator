using System.Collections.Generic;
using UnityEngine;

namespace yourvrexperience.WorkDay
{
    public interface IPromptBuilder
    {
        void AddContent(XMLTag tag, string content);
        void RemoveTag(string startingTag);
        void AddTag(string startingTag);
        List<string> GetEnabledTags();
        List<string> GetAllTags();

        string BuildPrompt();
        void SetMeetingUID(string meetingUID);
        string GetMeetingUID();
        string GetProjectFeedback();
        void SetProjectFeedback(string projectFeedback);
        Color GetPromptColor();
        void SetPromptColor(Color promptColor);
    }
}