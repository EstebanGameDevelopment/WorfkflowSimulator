using yourvrexperience.Utils;
using UnityEngine;
using System;
using System.Collections;
using System.Globalization;
using System.Xml;

namespace yourvrexperience.WorkDay
{
    [CreateAssetMenu(menuName = "Game/PromptController")]
    public class PromptController : ScriptableObject
    {
        public const string CodeLanguageEnglish = "en";
        public const string CodeLanguageSpanish = "es";
        public const string CodeLanguageCatalan = "ca";
        public const string CodeLanguageGerman = "de";
        public const string CodeLanguageFrench = "fr";
        public const string CodeLanguageItalian = "it";
        public const string CodeLanguageRussian = "ru";

        private static PromptController _instance;
        public static PromptController Instance
        {
            get
            {
                return _instance;
            }
        }

        public TextAsset GameTexts;
        public string CodeLanguage = "en";
        public string[] SupportedLanguages;

        private Hashtable m_texts = new Hashtable();

        public void Initialize()
        {
            _instance = this;
            DetectOSLanguage();
            SystemEventController.Instance.Event += OnSystemEvent;
        }

        void OnDestroy()
        {
            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
        }

        private void DetectOSLanguage()
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            CodeLanguage = currentCulture.TwoLetterISOLanguageName;
            string cultureName = currentCulture.Name;

            if (!CodeLanguage.Equals(CodeLanguageEnglish) && !CodeLanguage.Equals(CodeLanguageSpanish))
            {
                CodeLanguage = CodeLanguageEnglish;
            }
#if UNITY_EDITOR
            Debug.Log("Language: " + CodeLanguage);
            Debug.Log("Culture Name: " + cultureName);
#endif
        }

        private void Destroy()
        {
            if (Instance)
            {
                _instance = null;
            }
        }

        public void SetGameTexts(TextAsset gameTexts)
        {
            GameTexts = gameTexts;
            m_texts.Clear();
            LoadGameTexts();
        }

        private void LoadGameTexts()
        {
            if (m_texts.Count != 0) return;
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(GameTexts.text);
            XmlNodeList textsList = xmlDoc.GetElementsByTagName("text");
            foreach (XmlNode textEntry in textsList)
            {
                XmlNodeList textNodes = textEntry.ChildNodes;
                string idText = textEntry.Attributes["id"].Value;
                m_texts.Add(idText, new TextEntry(idText, textNodes));
            }
        }

        public string GetText(string id)
        {
            LoadGameTexts();
            if (m_texts[id] != null)
            {
                return ((TextEntry)m_texts[id]).GetText(CodeLanguage);
            }
            else
            {
                return id;
            }
        }

        public string ReplaceConflictiveCharacters(string text)
        {
            if (text != null)
            {
                text = text.Replace("\"", "'");
                return text;
            }
            else
            {
                return text;
            }
        }

        public string GetTextForLanguage(string id, string code)
        {
            LoadGameTexts();
            if (m_texts[id] != null)
            {
                return ((TextEntry)m_texts[id]).GetText(code);
            }
            else
            {
                return id;
            }
        }

        public string GetText(string _id, params object[] _list)
        {
            LoadGameTexts();
            if (m_texts[_id] != null)
            {
                string buffer = ((TextEntry)m_texts[_id]).GetText(CodeLanguage);
                string result = "";
                for (int i = 0; i < _list.Length; i++)
                {
                    string valueThing = (_list[i]).ToString();
                    int indexTag = buffer.IndexOf("~");
                    if (indexTag != -1)
                    {
                        result += buffer.Substring(0, indexTag) + valueThing;
                        buffer = buffer.Substring(indexTag + 1, buffer.Length - (indexTag + 1));
                    }
                }
                result += buffer;
                return result;
            }
            else
            {
                return _id;
            }
        }

        public string GetNameLunch(DateTime day)
        {
            return LanguageController.Instance.GetText("text.lunch.time.name") + " " + day.DayOfWeek;
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(SystemEventController.EventSystemEventControllerReleaseAllResources))
            {
                Destroy();
            }
            if (nameEvent.Equals(LanguageController.EventLanguageControllerChangedCodeLanguage))
            {
                CodeLanguage = (string)parameters[0];
            }
        }
    }

}