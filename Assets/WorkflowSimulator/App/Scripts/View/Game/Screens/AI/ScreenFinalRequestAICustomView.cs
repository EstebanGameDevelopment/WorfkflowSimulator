using InGameCodeEditor;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.MeetingController;

namespace yourvrexperience.WorkDay
{
	public class ScreenFinalRequestAICustomView : ScreenFinalRequestAIView, IScreenView
	{
		public const string ScreenCustomName = "ScreenFinalRequestAICustomView";

		public const string EventScreenFinalRequestAICustomViewInitValues = "EventScreenFinalRequestAICustomViewInitValues";
		public const string EventScreenFinalRequestAICustomViewRefreshInput = "EventScreenFinalRequestAICustomViewRefreshInput";

		public const string EventScreenFinalRequestAICustomViewAddedTag = "EventScreenFinalRequestAICustomViewAddedTag";
		public const string EventScreenFinalRequestAICustomViewRemovedTag = "EventScreenFinalRequestAICustomViewRemovedTag";

		[SerializeField] private CustomToggle[] tooglesData;
		[SerializeField] private CodeEditor InputCodeEditor;
		[SerializeField] private TextMeshProUGUI ProjectFeedback;

		private IPromptBuilder _promptBuilder = null;
		private List<string> _allTags = null;
		private List<string> _cleanedTags = null;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			ProjectFeedback.text = "";

			foreach (Toggle toggle in tooglesData)
            {
				toggle.gameObject.SetActive(false);
			}
		}

		public override void Destroy()
		{
			base.Destroy();

			_promptBuilder = null;
			_allTags = null;
			_cleanedTags = null;
		}

		private void OnXMLTagChanged(CustomToggle toggle, string xmlTag)
		{
			if (toggle.isOn)
            {
				_promptBuilder.AddTag(xmlTag);
				if (_promptBuilder.GetMeetingUID() != null)
				{
					SystemEventController.Instance.DispatchSystemEvent(EventScreenFinalRequestAICustomViewAddedTag, _promptBuilder.GetMeetingUID(), xmlTag);
				}
			}
			else
            {
				_promptBuilder.RemoveTag(xmlTag);
				if (_promptBuilder.GetMeetingUID() != null)
				{
					SystemEventController.Instance.DispatchSystemEvent(EventScreenFinalRequestAICustomViewRemovedTag, _promptBuilder.GetMeetingUID(), xmlTag);
				}
			}
			UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, _promptBuilder.BuildPrompt());
		}

		protected override void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventScreenInformationRequestAllScreensDestroyed))
			{
				return;
			}

			base.OnUIEvent(nameEvent, parameters);

			if (nameEvent.Equals(EventScreenFinalRequestAICustomViewRefreshInput))
			{
				if (_inputValue != null)
				{
					InputCodeEditor.Refresh(true);
				}
			}
			if (nameEvent.Equals(EventScreenInformationSetInputText))
			{
				if (_inputValue != null)
				{
					InputCodeEditor.Refresh(true);
				}
			}
			if (nameEvent.Equals(EventScreenFinalRequestAICustomViewInitValues))
			{
				if (_promptBuilder == null)
				{
					_promptBuilder = (IPromptBuilder)parameters[0];

					ProjectFeedback.text = _promptBuilder.GetProjectFeedback();
					_content.GetComponent<Image>().color = _promptBuilder.GetPromptColor();
					
					ApplicationController.Instance.LastProjectFeedback = _promptBuilder.GetProjectFeedback();
					ApplicationController.Instance.LastProjectColor = _promptBuilder.GetPromptColor();

					_allTags = _promptBuilder.GetEnabledTags();
					_cleanedTags = new List<string>();
					List<string> existingTags = null;
					if (_promptBuilder.GetMeetingUID() != null)
					{
						MeetingInProgress meetingIn = MeetingController.Instance.GetMeetingInProgressByUID(_promptBuilder.GetMeetingUID());
						if (meetingIn != null)
                        {
							existingTags = meetingIn.Tags;
						}
					}
					// THE PROBABILITY OF BEING ZERO MEANS THAT IT SHOULD HAVE LOADED A PREVIOUS STATE
					if (existingTags != null)
                    {
						if (existingTags.Count == 0)
						{
							for (int i = 0; i < _allTags.Count; i++)
							{
								string currTag = _allTags[i];
								existingTags.Add(currTag);
							}
						}
					}
					for (int i = 0; i < _allTags.Count; i++)
					{
						string currTag = _allTags[i];
						string cleanedTag = currTag;
						cleanedTag = cleanedTag.Replace("<", "");
						cleanedTag = cleanedTag.Replace(">", "");
						_cleanedTags.Add(cleanedTag);

						tooglesData[i].gameObject.SetActive(true);
						tooglesData[i].Name = currTag;
						tooglesData[i].GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText(currTag);
						tooglesData[i].isOn = true;
						if (existingTags != null)
                        {
							if (!existingTags.Contains(currTag))
                            {
								tooglesData[i].isOn = false;
							}
						}
						tooglesData[i].PointerClickedButton += OnXMLTagChanged;
					}

					if (existingTags != null)
                    {
						for (int i = 0; i < _allTags.Count; i++)
						{
							string currTag = _allTags[i];
							if (!existingTags.Contains(currTag))
							{
								_promptBuilder.RemoveTag(currTag);
							}
						}
					}

					UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, _promptBuilder.BuildPrompt());
				}
			}
		}
    }
}