using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using UnityEngine;

namespace yourvrexperience.Utils
{
    [CreateAssetMenu(menuName = "Game/LanguageController")]
    public class LanguageController : ScriptableObject
    {
        public const string EventLanguageControllerChangedCodeLanguage = "EventLanguageControllerChangedCodeLanguage";

        public const string CodeLanguageEnglish = "en";
        public const string CodeLanguageSpanish = "es";
        public const string CodeLanguageCatalan = "ca";
        public const string CodeLanguageGerman = "de";
        public const string CodeLanguageFrench = "fr";
        public const string CodeLanguageItalian = "it";
        public const string CodeLanguageRussian = "ru";

        public enum TranslationTypes { English = 0, Spanish, German, French, Italian, Russian, Catalan }

        private static LanguageController _instance;
        public static LanguageController Instance
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

            Debug.Log("Language: " + CodeLanguage);
            Debug.Log("Culture Name: " + cultureName);
        }

		private void Destroy()
		{
			if (Instance)
			{
				LanguageController reference = _instance;
				_instance = null;
			}
		}

        public string GetCodeLanguageByType(TranslationTypes translation)
        {
            switch (translation)
            {
                case TranslationTypes.English:
                    return CodeLanguageEnglish;
                case TranslationTypes.Spanish:
                    return CodeLanguageSpanish;
                case TranslationTypes.German:
                    return CodeLanguageGerman;
                case TranslationTypes.French:
                    return CodeLanguageFrench;
                case TranslationTypes.Italian:
                    return CodeLanguageItalian;
                case TranslationTypes.Russian:
                    return CodeLanguageRussian;
                case TranslationTypes.Catalan:
                    return CodeLanguageCatalan;
            }
            return CodeLanguageEnglish;
        }

        public TranslationTypes GetTypeLanguageByCode(string code)
        {
            switch (code)
            {
                case CodeLanguageEnglish:
                    return TranslationTypes.English;
                case CodeLanguageSpanish:
                    return TranslationTypes.Spanish;
                case CodeLanguageGerman:
                    return TranslationTypes.German;
                case CodeLanguageFrench:
                    return TranslationTypes.French;
                case CodeLanguageItalian:
                    return TranslationTypes.Italian;
                case CodeLanguageRussian:
                    return TranslationTypes.Russian;
                case CodeLanguageCatalan:
                    return TranslationTypes.Catalan;
            }
            return TranslationTypes.English;
        }

        public string GetNameLanguageByCode(string code)
        {
            switch (code)
            {
                case CodeLanguageEnglish:
                    return LanguageController.Instance.GetText("language.name.english");
                case CodeLanguageSpanish:
                    return LanguageController.Instance.GetText("language.name.spanish");
                case CodeLanguageGerman:
                    return LanguageController.Instance.GetText("language.name.german");
                case CodeLanguageFrench:
                    return LanguageController.Instance.GetText("language.name.french");
                case CodeLanguageItalian:
                    return LanguageController.Instance.GetText("language.name.italian");
                case CodeLanguageRussian:
                    return LanguageController.Instance.GetText("language.name.russian");
                case CodeLanguageCatalan:
                    return LanguageController.Instance.GetText("language.name.catalan");
            }
            return LanguageController.Instance.GetText("language.name.english");
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

        public void ChangeLanguage(string newCodeLanguage)
        {
            bool reportToSystemLanguageChange = false;
            if (CodeLanguage != newCodeLanguage)
            {
                reportToSystemLanguageChange = true;
            }
            CodeLanguage = newCodeLanguage;
            if (reportToSystemLanguageChange)
            {
                SystemEventController.Instance.DispatchSystemEvent(EventLanguageControllerChangedCodeLanguage, CodeLanguage);
            }
        }

		private void OnSystemEvent(string nameEvent, object[] parameters)
        {
			if (nameEvent.Equals(SystemEventController.EventSystemEventControllerReleaseAllResources))
            {
                Destroy();
            }		
        }
    }
}