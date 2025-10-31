using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.ai;
using yourvrexperience.UserManagement;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class ScreenChatAIView : BaseScreenView, IScreenView
	{
		public const bool EnableLocalStorageHistory = true;

		public const string EventScreenChatAIViewResponseAI = "EventScreenChatAIViewResponseAI";
		public const string EventScreenChatAIViewScrollToResponse = "EventScreenChatAIViewScrollToResponse";
		public const string EventScreenChatAIViewRequestHistory = "EventScreenChatAIViewRequestHistory";

		public const string SubEventScreenChatAIViewConfirmDeleteConversation = "SubEventScreenChatAIViewConfirmDeleteConversation";
		public const string SubEventScreenChatAIViewUpdatedInstructions = "SubEventScreenChatAIViewUpdatedInstructions";

		public const string ScreenName = "ScreenChatAIView";

		public const string CoockieChat = "CoockieChat_";
		public const string CoockieInitialInstructionChat = "CoockieInitialInstructionChat_";

		[SerializeField] private TextMeshProUGUI titleChatAI;
		[SerializeField] private CustomInput inputArea;
		[SerializeField] private Button askAIButton;
		[SerializeField] private Button checkInstructionsButton;
		[SerializeField] private Button newConversationButton;
		[SerializeField] private Button buttonCancel;

		[SerializeField] private GameObject infoLoadingPopUp;
		[SerializeField] private TextMeshProUGUI textLoadingPopUp;
		[SerializeField] private TimeCounterDisplay timeCounterDisplay;

		[SerializeField] private GameObject ChatViewPrefab;
		[SerializeField] private SlotManagerView SlotManagerChat;

		[SerializeField] private TextMeshProUGUI textSizeCalculator;

		private string _conversationID;
		private List<ChatMessage> _chatMessages = new List<ChatMessage>();
		private string _instructions = "";
		private string _selectedText = "";

		private PromptBuilder _promptChatAI = null;

		public override string NameScreen
		{
			get { return ScreenName; }
		}

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			_conversationID = UsersController.Instance.CurrentUser.Id + "_" + WorkDayData.Instance.CurrentProject.ProjectInfoSelected + "_" + LanguageController.Instance.CodeLanguage;

			textSizeCalculator.gameObject.SetActive(false);

			SlotManagerChat.ClearCurrentGameObject(true);
			SlotManagerChat.Initialize(0, new List<ItemMultiObjectEntry>(), ChatViewPrefab);

			SystemEventController.Instance.Event += OnSystemEvent;
			UIEventController.Instance.Event += OnUIEvent;

			askAIButton.onClick.AddListener(OnAskAI);
			askAIButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.chat.ai.ask.question");

			checkInstructionsButton.onClick.AddListener(OnCheckInstructions);
			checkInstructionsButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.chat.ai.check.instructions");

			newConversationButton.onClick.AddListener(OnCreateNewConversation);
			newConversationButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.chat.ai.create.new.conversation");

			titleChatAI.text = LanguageController.Instance.GetText("screen.chat.ai.title");
			inputArea.text = _selectedText;

			buttonCancel.onClick.AddListener(OnCancel);

			if (!EnableLocalStorageHistory)
			{
				infoLoadingPopUp.SetActive(true);
				textLoadingPopUp.text = LanguageController.Instance.GetText("screen.loading");
				timeCounterDisplay.ResetText();

				_instructions = "";
				SystemEventController.Instance.DelaySystemEvent(EventScreenChatAIViewRequestHistory, 0.5f);
			}
			else
			{
				string chatHistory = PlayerPrefs.GetString(CoockieChat + _conversationID, "");
				_instructions = PlayerPrefs.GetString(CoockieInitialInstructionChat + _conversationID, "");

				if (chatHistory.Length > 0)
				{
					ChatMessage[] messagesChat = JsonConvert.DeserializeObject<ListChatMessages>(chatHistory).Messages;
					if ((messagesChat != null) && (messagesChat.Length > 0))
					{
						LoadChatMessages(messagesChat.ToList());
					}
				}
			}

			if (_instructions.Length == 0)
			{
				InitInstructions();
			}

			SystemEventController.Instance.DispatchSystemEvent(EditionSubStateIdle.EventSubStateIdleEnableWorldSelection, false);
			SystemEventController.Instance.DispatchSystemEvent(EditionSubStateBase.EventSubStateBaseEnableMovement, false);
		}

        public override void Destroy()
		{
			base.Destroy();
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

			if (EnableLocalStorageHistory)
			{
				ListChatMessages listChatMessage = new ListChatMessages();
				listChatMessage.Messages = _chatMessages.ToArray();
				string chatHistoryToSave = JsonConvert.SerializeObject(listChatMessage, Formatting.Indented);
				PlayerPrefs.SetString(CoockieChat + _conversationID, chatHistoryToSave);
			}

			SystemEventController.Instance.DispatchSystemEvent(EditionSubStateIdle.EventSubStateIdleEnableWorldSelection, true);
			SystemEventController.Instance.DispatchSystemEvent(EditionSubStateBase.EventSubStateBaseEnableMovement, true);
		}

		private void DeleteChatInfo()
        {
			if (!EnableLocalStorageHistory)
			{
				GameAIData.Instance.AskDeleteHistory(_conversationID);
			}
			else
			{
				PlayerPrefs.SetString(CoockieChat + _conversationID, "");
				PlayerPrefs.SetString(CoockieInitialInstructionChat + _conversationID, "");
			}
		}

		private void OnCancel()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private string GetChatHistory()
		{
			if ((_chatMessages.Count > 0) && (EnableLocalStorageHistory))
			{
				string question = PromptController.Instance.GetText("screen.chat.ai.asking.ai.new.question.with.previous.history"
															, "<" + PromptController.Instance.GetText("xml.tag.new.question") + ">"
															, "<" + PromptController.Instance.GetText("xml.tag.conversation.history") + ">"
															, "<" + PromptController.Instance.GetText("xml.tag.information") + ">"
															);
				question += "\n\n";
				question += "<" + PromptController.Instance.GetText("xml.tag.conversation.history") + ">";
				question += "\n\n";
				for (int i = 0; i < _chatMessages.Count; i += 2)
				{
					question += "<" + PromptController.Instance.GetText("xml.tag.question") + ">";
					if (_chatMessages[i].Mode != 1) Debug.LogError("ScreenChatAIView::Error in creating the history. No human question.");
					question += _chatMessages[i].Text;
					question += "</" + PromptController.Instance.GetText("xml.tag.question") + ">";
					question += "\n\n";
					question += "<" + PromptController.Instance.GetText("xml.tag.response") + ">";
					if (_chatMessages[i + 1].Mode != 0) Debug.LogError("ScreenChatAIView::Error in creating the history. No machine response.");
					question += _chatMessages[i + 1].Text;
					question += "</" + PromptController.Instance.GetText("xml.tag.response") + ">";
					question += "\n\n";
				}
				question += "</" + PromptController.Instance.GetText("xml.tag.conversation.history") + ">";
				question += "\n\n";

				return question;
			}
			else
			{
				return "";
			}
		}

		private void InitInstructions()
		{
			string question = PromptController.Instance.GetText("screen.chat.ai.asking.ai.generic",
												"<" + PromptController.Instance.GetText("xml.tag.question") + ">",
												"<" + PromptController.Instance.GetText("xml.tag.employees") + ">",
												"<" + PromptController.Instance.GetText("xml.tag.meetings") + ">",
												"<" + PromptController.Instance.GetText("xml.tag.documents") + ">",
												"<" + PromptController.Instance.GetText("xml.tag.sprint") + ">",
												"<" + PromptController.Instance.GetText("xml.tag.project") + ">");

			question += "\n\n";
			_promptChatAI = new PromptBuilder(question);
			question = _promptChatAI.BuildPrompt();

			ProjectInfoData projectInfo = WorkDayData.Instance.CurrentProject.GetProject(WorkDayData.Instance.CurrentProject.ProjectInfoSelected);

			// EMPLOYEES "xml.tag.employees"
			string employeesCompany = "\n";
			List<WorldItemData> humans = ApplicationController.Instance.LevelView.GetHumans();
			foreach (WorldItemData human in humans)
			{
				GroupInfoData groupOfAssistant = WorkDayData.Instance.CurrentProject.GetGroupOfMember(human.Name);
				if (groupOfAssistant != null)
				{
					employeesCompany += "<" + PromptController.Instance.GetText("xml.tag.employee")
								+ " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + human.Name + "\""
								+ " " + PromptController.Instance.GetText("xml.tag.group") + "=\"" + groupOfAssistant.Name + "\""
								+ " " + PromptController.Instance.GetText("xml.tag.description") + "=\"" + PromptController.Instance.ReplaceConflictiveCharacters(human.Data) + "\""
								+ "/>";
					employeesCompany += "\n";
				}
				else
				{
					employeesCompany += "<" + PromptController.Instance.GetText("xml.tag.employee")
								+ " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + human.Name + "\""
								+ " " + PromptController.Instance.GetText("xml.tag.description") + "=\"" + PromptController.Instance.ReplaceConflictiveCharacters(human.Data) + "\""
								+ "/>";
					employeesCompany += "\n";
				}
			}
			_promptChatAI.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.employees") + ">",
												"<" + PromptController.Instance.GetText("xml.tag.employees") + ">",
												"</" + PromptController.Instance.GetText("xml.tag.employees") + ">"),
												employeesCompany);

			// MEETINGS "xml.tag.meetings"
			string meetingsSprint = "\n";
			List<MeetingData> meetings = WorkDayData.Instance.CurrentProject.GetMeetings();
			foreach (MeetingData meeting in meetings)
            {
				if (meeting.ProjectId == projectInfo.Id)
                {
					meetingsSprint += "\n";
					string stateMeeting = PromptController.Instance.GetText("xml.value.todo");
					if (meeting.Completed)
                    {
						stateMeeting = PromptController.Instance.GetText("xml.value.done");
					}
					else
                    {
						if (meeting.InProgress)
                        {
							stateMeeting = PromptController.Instance.GetText("xml.value.doing");
						}
                    }
					meetingsSprint += "<" + PromptController.Instance.GetText("xml.tag.meeting")
								+ " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + meeting.Name + "\""
								+ " " + PromptController.Instance.GetText("xml.tag.description") + "=\"" + meeting.Description + "\""
								+ " " + PromptController.Instance.GetText("xml.tag.participants") + "=\"" + meeting.GetMembersPacket("", false, true) + "\""
								+ " " + PromptController.Instance.GetText("xml.tag.state") + "=\"" + stateMeeting + "\""
								+ (((meeting.Summary != null)&& (meeting.Summary.Length > 0))?" " + PromptController.Instance.GetText("xml.tag.summary") + "=\"" + PromptController.Instance.ReplaceConflictiveCharacters(meeting.Summary) + "\"":"")
								+ "/>";
					meetingsSprint += "\n";
				}
			}
			_promptChatAI.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.meetings") + ">",
												"<" + PromptController.Instance.GetText("xml.tag.meetings") + ">",
												"</" + PromptController.Instance.GetText("xml.tag.meetings") + ">"),
												meetingsSprint + "\n");

			// DOCUMENTS "xml.tag.documents"
			List<DocumentData> globalDocs = WorkDayData.Instance.CurrentProject.GetDocuments();
			string documentsContent = "\n\n";
			foreach (DocumentData document in globalDocs)
			{
				if (WorkDayData.Instance.CurrentProject.ProjectInfoSelected == document.ProjectId)
				{
					string infoDocument = "";
					if ((document.Summary != null) && (document.Summary.Length > 1))
                    {
						infoDocument = "\" " + PromptController.Instance.GetText("xml.tag.summary") + "=\"" + PromptController.Instance.ReplaceConflictiveCharacters(document.Summary);
					}
					else
                    {
						infoDocument = "\" " + PromptController.Instance.GetText("xml.tag.description") + "=\"" + PromptController.Instance.ReplaceConflictiveCharacters(document.Description);
					}

					documentsContent += "<" + PromptController.Instance.GetText("xml.tag.doc")
								+ " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + document.Name
								+ infoDocument
								+ "\"/>";
					documentsContent += "\n";
				}
			}
			_promptChatAI.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.documents") + ">",
												"<" + PromptController.Instance.GetText("xml.tag.documents") + ">",
												"</" + PromptController.Instance.GetText("xml.tag.documents") + ">"),
												documentsContent + "\n");


			// SPRINT (FEATURES) "xml.tag.sprint"
			_promptChatAI.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.sprint") + ">",
												"<" + PromptController.Instance.GetText("xml.tag.sprint") + ">",
												"</" + PromptController.Instance.GetText("xml.tag.sprint") + ">"),
												WorkDayData.Instance.CurrentProject.PackBoardsXML(projectInfo.Id, true) + "\n\n");

			// PROJECT "xml.tag.project"			
			_promptChatAI.AddContent(new XMLTag("<" + PromptController.Instance.GetText("xml.tag.project") + ">",
												"<" + PromptController.Instance.GetText("xml.tag.project") + " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + projectInfo.Name + "\">",
												"</" + PromptController.Instance.GetText("xml.tag.project") + ">"),
												PromptController.Instance.ReplaceConflictiveCharacters(projectInfo.Description) + "\n");

			_instructions = _promptChatAI.BuildPrompt();
		}

		private void OnAskAI()
		{
			if (SlotManagerChat.Data.Count >= 12)
			{
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("screen.chat.limit.conversation"));
				return;
			}

			infoLoadingPopUp.SetActive(true);
			timeCounterDisplay.Activate();
			textLoadingPopUp.text = LanguageController.Instance.GetText("screen.chat.ai.asking.ai.wait");

			bool enableChain = false;  // WE WILL DISABLE CHAIN STORAGE BECAUSE THE SIZE OF SQL ALCHEMY DATABASE COULD GROW EXPONENTIALLY
			string question = "";
			if ((_chatMessages.Count == 0) || EnableLocalStorageHistory)
			{
				question += _instructions;
			}
			if (EnableLocalStorageHistory)
			{
				string withChat = GetChatHistory();
				if (withChat.Length > 1)
				{
					question = withChat + "<" + PromptController.Instance.GetText("xml.tag.information") + ">" + question + "</" + PromptController.Instance.GetText("xml.tag.information") + ">";
				}
			}
			else
			{
				if (!enableChain) question = _instructions;
			}
			string finalResult = "";
			if ((_chatMessages.Count == 0) || (!EnableLocalStorageHistory))
			{
				question += "<" + PromptController.Instance.GetText("xml.tag.question") + ">" + inputArea.text + "</" + PromptController.Instance.GetText("xml.tag.question") + ">";
				if (EnableLocalStorageHistory)
				{
					string[] paragraphs = _instructions.Split(new string[] { "\n\n" }, StringSplitOptions.None);
					finalResult = _instructions;
					if (paragraphs.Length > 1)
					{
						finalResult = string.Join("\n\n", paragraphs, 1, paragraphs.Length - 1);
					}
					PlayerPrefs.SetString(CoockieInitialInstructionChat + _conversationID, finalResult);
				}
			}
			else
			{
				question += "<" + PromptController.Instance.GetText("xml.tag.new.question") + ">" + inputArea.text + "</" + PromptController.Instance.GetText("xml.tag.new.question") + ">";
			}
			RegisterText(1, inputArea.text);
			inputArea.text = "";
			GameAIData.Instance.AskGenericQuestionAI(_conversationID, question, enableChain, EventScreenChatAIViewResponseAI);
			if (finalResult.Length > 1)
			{
				_instructions = finalResult;
			}
		}

		private void OnCheckInstructions()
		{
			string finalInstructions = _instructions;
			if (_chatMessages.Count > 0)
			{
				ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAIView.ScreenName, this.gameObject, LanguageController.Instance.GetText("screen.chat.ai.prompt.instructions"), "", SubEventScreenChatAIViewUpdatedInstructions);
				finalInstructions = "<" + PromptController.Instance.GetText("xml.tag.information") + ">" + _instructions + "</" + PromptController.Instance.GetText("xml.tag.information") + ">";
				UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, GetChatHistory() + finalInstructions);
			}
			else
			{
				ScreenInformationView.CreateScreenInformation(ScreenFinalRequestAICustomView.ScreenCustomName, this.gameObject, LanguageController.Instance.GetText("screen.chat.ai.prompt.instructions"), "", SubEventScreenChatAIViewUpdatedInstructions);
				UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewInitValues, _promptChatAI);
				UIEventController.Instance.DelayUIEvent(ScreenFinalRequestAICustomView.EventScreenFinalRequestAICustomViewRefreshInput, 0.1f);
			}
			
			if (EnableLocalStorageHistory && (_chatMessages != null) && (_chatMessages.Count > 0))
			{
				UIEventController.Instance.DispatchUIEvent(ScreenFinalRequestAIView.EventScreenFinalRequestAIViewDisableInput);
			}
		}

		private void OnCreateNewConversation()
		{
			string titleWarning = LanguageController.Instance.GetText("text.warning");
			string titleDeleteDescription = LanguageController.Instance.GetText("screen.chat.ai.confirmation.create.new.conversation");
			ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, this.gameObject, titleWarning, titleDeleteDescription, SubEventScreenChatAIViewConfirmDeleteConversation);
		}

		private void RegisterMessage(ChatMessage message)
		{
			ChatMessage newMessage = new ChatMessage();
			newMessage.Mode = message.Mode;
			if (newMessage.Mode == 1)
			{
				newMessage.Text = GetQuestionText(message.Text);
			}
			else
			{
				newMessage.Text = message.Text;
			}
			_chatMessages.Add(newMessage);
			SlotManagerChat.AddItem(new ItemMultiObjectEntry(this.gameObject, SlotManagerChat.Data.Count, newMessage, textSizeCalculator));
			UIEventController.Instance.DelayUIEvent(EventScreenChatAIViewScrollToResponse, 0.1f);
		}

		private void RegisterText(int mode, string text)
		{
			ChatMessage newMessage = new ChatMessage();
			newMessage.Mode = mode;
			newMessage.Text = text;
			_chatMessages.Add(newMessage);
			SlotManagerChat.AddItem(new ItemMultiObjectEntry(this.gameObject, SlotManagerChat.Data.Count, newMessage, textSizeCalculator));
			UIEventController.Instance.DelayUIEvent(EventScreenChatAIViewScrollToResponse, 0.1f);
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(SubEventScreenChatAIViewConfirmDeleteConversation))
			{
				if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
				{
					InitInstructions();

					DeleteChatInfo();

					if (EnableLocalStorageHistory)
					{
						_chatMessages.Clear();
						SlotManagerChat.ClearCurrentGameObject(true);
						SlotManagerChat.ClearData();
					}
					else
					{
						infoLoadingPopUp.SetActive(true);
						textLoadingPopUp.text = LanguageController.Instance.GetText("screen.chat.ai.delete.history.wait");
					}
				}
			}
			if (nameEvent.Equals(SubEventScreenChatAIViewUpdatedInstructions))
			{
				if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
				{
					if (EnableLocalStorageHistory)
					{
						if (_chatMessages.Count == 0)
						{
							_instructions = (string)parameters[2];
						}
					}
					else
					{
						_instructions = (string)parameters[2];
					}
				}
			}
			if (nameEvent.Equals(EventScreenChatAIViewScrollToResponse))
			{
				SlotManagerChat.SetVerticalScroll(0);
			}
		}

		private string GetQuestionText(string xmlString)
		{
			string startTag = "<" + PromptController.Instance.GetText("xml.tag.question") + ">";
			string endTag = "</" + PromptController.Instance.GetText("xml.tag.question") + ">";
			int st = xmlString.LastIndexOf(startTag);
			int start = st + startTag.Length;
			int end = xmlString.LastIndexOf(endTag);
			if ((start < end) && (st != -1))
			{
				return xmlString.Substring(start, end - start);
			}
			else
			{
				return xmlString;
			}
		}

		private void LoadChatMessages(List<ChatMessage> data)
		{
			SlotManagerChat.ClearCurrentGameObject(true);
			List<ChatMessage> messagesHistory = data;
			for (int i = 0; i < messagesHistory.Count; i++)
			{
				ChatMessage message = messagesHistory[i];
				RegisterMessage(message);
			}
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventScreenChatAIViewRequestHistory))
			{
				GameAIData.Instance.AskChatHistory(_conversationID);
			}
			if (nameEvent.Equals(AskChatHistoryGPTHTTP.EventGenericAskChatHistoryGPTHTTPCompleted))
			{
				infoLoadingPopUp.SetActive(false);
				timeCounterDisplay.Deactivate();
				if ((bool)parameters[0])
				{
					if (parameters.Length > 1)
					{
						LoadChatMessages((List<ChatMessage>)parameters[1]);
					}
				}
			}
			if (nameEvent.Equals(EventScreenChatAIViewResponseAI))
			{
				infoLoadingPopUp.SetActive(false);
				timeCounterDisplay.Deactivate();
				if ((bool)parameters[0])
				{
					string response = (string)parameters[1];
					RegisterText(0, response);
				}
				else
				{
					RegisterText(0, LanguageController.Instance.GetText("screen.chat.ai.error.response.from.ai"));
				}
			}
			if (nameEvent.Equals(AskDeleteHistoryGPTHTTP.EventGenericAskDeleteHistoryGPTHTTPCompleted))
			{
				infoLoadingPopUp.SetActive(false);
				timeCounterDisplay.Deactivate();

				_chatMessages.Clear();
				SlotManagerChat.ClearCurrentGameObject(true);
				SlotManagerChat.ClearData();
			}
		}
	}
}