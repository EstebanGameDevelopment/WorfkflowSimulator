using System;
using System.Collections.Generic;
using UnityEngine;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public struct XMLTag : IEquatable<XMLTag>
    {
        public string Tag;
        public string StartingTag;
        public string EndingTag;

        public XMLTag(string tag, string startingTag, string endingTag)
        {
            Tag = tag;
            StartingTag = startingTag;
            EndingTag = endingTag;
        }

        public bool Equals(XMLTag other)
        {
            return Tag == other.Tag;
        }
    }

    public class PromptBuilder : IPromptBuilder
    {
        private string _meetingUID = null;
        private string _projectFeedback = "";
        private string _question;
        private Color _promptColor = Color.white;
        private Dictionary<XMLTag, string> _content;
        private List<string> EnabledTags;

        public PromptBuilder(string question)
        {
            _question = question;
            _content = new Dictionary<XMLTag, string>();
            EnabledTags = new List<string>();
        }

        public PromptBuilder(string question, Dictionary<XMLTag, string> content)
        {
            _question = question;
            _content = content;
            EnabledTags = new List<string>();
            foreach (KeyValuePair<XMLTag, string> item in content)
            {
                EnabledTags.Add(item.Key.Tag);
                AddTag(item.Key.Tag);
            }
        }

        public void SetMeetingUID(string meetingUID)
        {
            _meetingUID = meetingUID;
        }

        public string GetMeetingUID()
        {
            return _meetingUID;
        }

        public string GetProjectFeedback()
        {
            return _projectFeedback;
        }

        public Color GetPromptColor()
        {
            return _promptColor;
        }
        public void SetPromptColor(Color promptColor)
        {
            _promptColor = promptColor;
        }

        public void SetProjectFeedback(string projectFeedback)
        {
            _projectFeedback = projectFeedback;
        }

        public List<string> GetEnabledTags()
        {
            return EnabledTags;
        }

        public List<string> GetAllTags()
        {
            List<string> allTags = new List<string>();
            foreach (KeyValuePair<XMLTag, string> item in _content)
            {
                allTags.Add(item.Key.Tag);
            }
            return allTags;
        }

        public void AddContent(XMLTag tag, string content)
        {
            string contentExisting = "";
            if (!_content.TryGetValue(tag, out contentExisting))
            {
                if (content == null)
                {
                    _content.Add(tag, "");
                }
                else
                {
                    _content.Add(tag, content);
                }                
                if ((content !=null) && (content.Length > 0))
                {
                    AddTag(tag.Tag);
                }
            }            
        }

        public void AddTag(string tag)
        {
            if (!EnabledTags.Contains(tag))
            {
                EnabledTags.Add(tag);
            }
        }

        public void RemoveTag(string tag)
        {
            EnabledTags.Remove(tag);
        }

        public string BuildPrompt()
        {
            string prompt = _question;
            prompt += "\n\n";
            foreach (KeyValuePair<XMLTag, string> item in _content)
            {
                if (EnabledTags.Contains(item.Key.Tag))
                {
                    prompt += item.Key.StartingTag + item.Value + item.Key.EndingTag;
                    prompt += "\n\n";
                }
                else
                {
                    prompt = Utilities.RemoveLinesContainingText(prompt, item.Key.Tag);
                }
            }
            return prompt;
        }
    }

}