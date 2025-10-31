using InGameCodeEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.UserManagement;
#if USE_VUPLEX
using Vuplex.WebView;
#endif
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.WorkDayData;

namespace yourvrexperience.WorkDay
{
	public class ScreenAnalysisHumanView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenAnalysisHumanView";

		public const string EventScreenAnalysisHumanViewReloadBrowser = "EventScreenAnalysisHumanViewReloadBrowser";
		public const string EventScreenAnalysisHumanViewReloadCodeEditor = "EventScreenAnalysisHumanViewReloadCodeEditor";
		
		public const string SubEventScreenAnalysisHumanViewConfirmSendAnalysis = "SubEventScreenAnalysisHumanViewConfirmSendAnalysis";

        public const string CookieAnalysis = "CookieAnalysis";
        public const string CookieHistory = "CookieHistory";

        public enum ContributionType { Meeting = 0, Document = 1, Task = 2 }

		public class ContributionHuman
        {
			public ContributionType Type;
			public DateTime Time;
			public int ProjectID;
			public string UID;
			public int TaskUID;
			public string Name;

			public ContributionHuman(ContributionType type, DateTime time, int projectID, string uid, int taskUID, string name)
            {
				Type = type;
				Time = time;
				ProjectID = projectID;
				UID = uid;
				TaskUID = taskUID;
				Name = name;
			}
		}

		[SerializeField] private TextMeshProUGUI titleScreen;

		[SerializeField] private CodeEditor inputCodeEditor;

		[SerializeField] private SlotManagerView SlotManagerContributions;
		[SerializeField] private GameObject PrefabContribution;

		[SerializeField] private Button buttonSource;
		[SerializeField] private Button buttonView;
		[SerializeField] private Button buttonSummary;
		[SerializeField] private Button buttonAI;
		[SerializeField] private Button buttonCancel;
		[SerializeField] private Button buttonPreviousAnalysis;

		[SerializeField] private Button buttonDownloadImage;
		[SerializeField] private Image contentImage;
#if USE_VUPLEX
		[SerializeField] private CanvasWebViewPrefab webBrowser;
#endif

		private TabsData _tabSelected = TabsData.HTML;
		private string _data;
		private string _summary;
		private bool _isImage;
		private string _history;
		private string _responseAnalysis;
		private string _responseHistory;

		private Vector2 _originalImageSize;

		private List<ContributionHuman> _contributions;

		private bool IsImage
        {
			set
            {
				_isImage = value;
				contentImage.gameObject.SetActive(_isImage);
				buttonDownloadImage.gameObject.SetActive(_isImage);
				buttonView.gameObject.SetActive(!_isImage);
				inputCodeEditor.gameObject.SetActive(!_isImage);
			}
		}

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			buttonSource.onClick.AddListener(OnViewSource);
			buttonView.onClick.AddListener(OnViewBrowser);
			buttonSummary.onClick.AddListener(OnViewSummary);
			buttonDownloadImage.onClick.AddListener(OnDownloadImage);
			buttonAI.onClick.AddListener(OnAskAI);
			buttonCancel.onClick.AddListener(OnButtonCancel);
            buttonPreviousAnalysis.onClick.AddListener(OnPreviousAnalysis);

			titleScreen.text = LanguageController.Instance.GetText("screen.perform.analysis.candidate.title");

			buttonSource.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.contribution");
            buttonView.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.view");
            buttonSummary.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.summary");
            buttonDownloadImage.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.download");
            buttonPreviousAnalysis.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.analysis");
            
            _data = "";
			_summary="";
			IsImage = false;
			OnViewSource();

			_responseAnalysis = PlayerPrefs.GetString(CookieAnalysis, "");
			_responseHistory = PlayerPrefs.GetString(CookieHistory, "");            
            buttonPreviousAnalysis.gameObject.SetActive(_responseAnalysis.Length > 0);

            buttonAI.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.perform.analysis.candidate");

			List<ContributionHuman> contributions = new List<ContributionHuman>();
			List<MeetingData> meetings = WorkDayData.Instance.CurrentProject.GetMeetings();
			foreach (MeetingData meeting in meetings)
            {
				if (meeting.Completed)
                {
					if (meeting.HasPlayer(true))
                    {
						contributions.Add(new ContributionHuman(ContributionType.Meeting, meeting.GetTimeStart(), meeting.ProjectId, meeting.GetUID(), -1, meeting.Name));
					}
                }
            }

			List<DocumentData> documents = WorkDayData.Instance.CurrentProject.GetDocuments();
			foreach (DocumentData document in documents)
            {
				if (document.Owner.ToLower().Equals(ApplicationController.Instance.HumanPlayer.NameHuman.ToLower()))
                {
					contributions.Add(new ContributionHuman(ContributionType.Document, document.GetCreationTime(), document.ProjectId, document.Id.ToString(), -1, document.Name));
				}
            }

			List<BoardData> boards = WorkDayData.Instance.CurrentProject.GetAllBoards();
			foreach (BoardData board in boards)
			{
				List<TaskItemData> tasks = WorkDayData.Instance.CurrentProject.GetAllTasks(board);
				foreach (TaskItemData task in tasks)
                {
					List<DocumentData> docs = task.GetData();
					foreach(DocumentData doc in docs)
                    {
						if (doc.Owner.ToLower().Equals(ApplicationController.Instance.HumanPlayer.NameHuman.ToLower()))
                        {
							contributions.Add(new ContributionHuman(ContributionType.Task, doc.GetCreationTime(), board.ProjectId, doc.Id.ToString(), task.UID, doc.Name));
						}							
					}					
				}
			}
			_contributions = contributions.OrderBy(m => m.Time).ToList();
			LoadContributions();

			_originalImageSize = contentImage.GetComponent<RectTransform>().sizeDelta;

			SystemEventController.Instance.Event += OnSystemEvent;
			UIEventController.Instance.Event += OnUIEvent;
		}

        public override void Destroy()
		{
			base.Destroy();

			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
		}

		private void LoadContributions()
        {
			SlotManagerContributions.ClearCurrentGameObject(true);
			SlotManagerContributions.Initialize(0, new List<ItemMultiObjectEntry>(), PrefabContribution);

			_history = "";
			if (_contributions != null)
			{
				for (int i = 0; i < _contributions.Count; i++)
				{
					SlotManagerContributions.AddItem(new ItemMultiObjectEntry(SlotManagerContributions.gameObject, SlotManagerContributions.Data.Count, _contributions[i]));
				}

				// FIRST IN HISTORY ARE THE TASKS
                string entry;
                for (int i = 0; i < _contributions.Count; i++)
                {
                    entry = "\n<" + PromptController.Instance.GetText("xml.tag.contribution");
					if (_contributions[i].Type == ContributionType.Task)
					{
                        entry += " " + PromptController.Instance.GetText("xml.tag.type") + "=\"" + PromptController.Instance.GetText("xml.tag.task") + "\"";
                        entry += " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + _contributions[i].Name + "\"";
                        entry += " " + PromptController.Instance.GetText("xml.tag.creator") + "=\"" + ApplicationController.Instance.HumanPlayer.NameHuman + "\"";
                        entry += ">\n";
                        var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(_contributions[i].TaskUID);
                        if (taskItemData != null)
                        {
                            List<DocumentData> docs = taskItemData.GetData();
                            foreach (DocumentData doc in docs)
                            {
                                if (doc.Id.ToString() == _contributions[i].UID)
                                {
									// DO NOT REMOVE!!! IT'S NECESSARY TO CHOOSE THE REAL WORK OF THE CANDIDATE, NOT ONLY THE SUMMARY
                                    /*
                                    if ((doc.Summary != null) && (doc.Summary.Length > 20))
                                    {
                                        entry += doc.Summary;
                                    }
                                    else
                                    {
                                        entry += doc.Data.GetHTML();
                                    }
                                    */
                                    entry += doc.Data.GetHTML();
                                }
                            }
                        }
                        entry += "\n</" + PromptController.Instance.GetText("xml.tag.contribution") + ">";
                        _history += entry + "\n";
                    }
                }

				for (int i = 0; i < _contributions.Count; i++)
				{
					entry = "\n<" + PromptController.Instance.GetText("xml.tag.contribution");
					// NEXT IN HISTORY ARE THE MEETINGS AND DOCUMENTS
					switch (_contributions[i].Type)
					{
						case ContributionType.Meeting:
							entry += " " + PromptController.Instance.GetText("xml.tag.type") + "=\"" + PromptController.Instance.GetText("xml.tag.meeting") + "\"";
							entry += " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + _contributions[i].Name + "\"";
							entry += ">\n";
							MeetingData meeting = WorkDayData.Instance.CurrentProject.GetMeetingByUID(_contributions[i].UID);
							entry += meeting.PackXMLInteractions(false);
							entry += "\n</" + PromptController.Instance.GetText("xml.tag.contribution") + ">";
                            _history += entry + "\n";
                            break;

						case ContributionType.Document:
							entry += " " + PromptController.Instance.GetText("xml.tag.type") + "=\"" + PromptController.Instance.GetText("xml.tag.document") + "\"";
							entry += " " + PromptController.Instance.GetText("xml.tag.name") + "=\"" + _contributions[i].Name + "\"";
							entry += " " + PromptController.Instance.GetText("xml.tag.creator") + "=\"" + ApplicationController.Instance.HumanPlayer.NameHuman + "\"";
							entry += ">\n";
							DocumentData docGlobal = WorkDayData.Instance.CurrentProject.GetDocumentByID(int.Parse(_contributions[i].UID));
                            
							// DO NOT REMOVE!!! IT'S NECESSARY TO CHOOSE THE REAL WORK OF THE CANDIDATE, NOT ONLY THE SUMMARY
                            /*
							if ((docGlobal.Summary != null) && (docGlobal.Summary.Length > 20))
							{
								entry += docGlobal.Summary;
							}
							else
							{
								entry += docGlobal.Data.GetHTML();
							}
							*/
                            entry += "\n</" + PromptController.Instance.GetText("xml.tag.contribution") + ">";
                            _history += entry + "\n";
                            break;

						case ContributionType.Task:
							break;
					}					
				}
            }

            SlotManagerContributions.SetVerticalScroll(0);
		}

		private void OnButtonCancel()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnAskAI()
		{
			AICommandsController.Instance.AddNewAICommand(new AICommandAnalysisHuman(), true, _history);
		}

        private void UpdateTabVisibility()
        {
			switch (_tabSelected)
			{
				case TabsData.HTML:
					inputCodeEditor.gameObject.SetActive(true);
#if USE_VUPLEX
					webBrowser.gameObject.SetActive(false);
#endif
					buttonSource.interactable = false;
					buttonView.interactable = true;
					buttonSummary.interactable = true;
					break;
				case TabsData.CSS:
					inputCodeEditor.gameObject.SetActive(true);
#if USE_VUPLEX
					webBrowser.gameObject.SetActive(false);
#endif
					buttonSource.interactable = true;
					buttonView.interactable = true;
					buttonSummary.interactable = true;
					break;
				case TabsData.BROWSER:
					inputCodeEditor.gameObject.SetActive(false);
#if USE_VUPLEX
					webBrowser.gameObject.SetActive(true);
#endif
					buttonSource.interactable = true;
					buttonView.interactable = false;
					buttonSummary.interactable = true;
					break;
				case TabsData.SUMMARY:
					inputCodeEditor.gameObject.SetActive(true);
#if USE_VUPLEX
					webBrowser.gameObject.SetActive(false);
#endif
					buttonSource.interactable = true;
					buttonView.interactable = true;
					buttonSummary.interactable = false;
					break;
			}
			inputCodeEditor.Refresh(true);
		}

        private void OnDownloadImage()
        {
			int idImage;
			if (int.TryParse(_data, out idImage))
			{
				if (idImage != -1)
                {
					Application.OpenURL(DownloadImageDataHTTP.GetURLToDownload(idImage));
				}					
			}
        }

		private void OnViewSource()
		{
			if (!_isImage)
            {
				contentImage.gameObject.SetActive(false);
				_tabSelected = TabsData.HTML;
				inputCodeEditor.Text = _data;
				UpdateTabVisibility();
			}
			else
            {
				_tabSelected = TabsData.HTML;				
				UpdateTabVisibility();
				IsImage = true;
			}
		}

		private void OnViewBrowser()
		{
			if (!_isImage)
			{
				contentImage.gameObject.SetActive(false);
				_tabSelected = TabsData.BROWSER;
				UpdateTabVisibility();
				SystemEventController.Instance.DelaySystemEvent(EventScreenAnalysisHumanViewReloadBrowser, 0.1f);
			}
		}

		private void OnViewSummary()
		{
			if (_isImage)
            {
				if (_tabSelected != TabsData.SUMMARY)
                {
					_tabSelected = TabsData.SUMMARY;
					inputCodeEditor.Text = _summary;
					inputCodeEditor.Refresh();
					contentImage.gameObject.SetActive(false);
				}
				else
                {
					_tabSelected = TabsData.HTML;
					contentImage.gameObject.SetActive(true);
				}
			}
			else
            {
				_tabSelected = TabsData.SUMMARY;
				inputCodeEditor.Text = _summary;
				inputCodeEditor.Refresh();
			}
			UpdateTabVisibility();
		}

        private void OnPreviousAnalysis()
        {
			DisplayResultsAnalysis(_responseAnalysis);
        }

		private void DisplayResultsAnalysis(string responseAnalysis)
		{
            // DISPLAY TEXT RESULT
            string textSend = LanguageController.Instance.GetText("text.send.analysis");
            string textCancel = LanguageController.Instance.GetText("text.cancel");
			GameObject analysisScreen = ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLongInput, null, LanguageController.Instance.GetText("text.analysis") + " : " + ApplicationController.Instance.HumanPlayer.NameHuman, "", SubEventScreenAnalysisHumanViewConfirmSendAnalysis, textSend, textCancel);
            UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, responseAnalysis);
			GameObject screenLoading = ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("text.loading"));
            UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, screenLoading, analysisScreen.GetComponent<Canvas>().sortingOrder + 1);
            UIEventController.Instance.DelayUIEvent(EventScreenAnalysisHumanViewReloadCodeEditor, 1f, screenLoading);
            GameObject.FindAnyObjectByType<CodeEditor>().Refresh();
        }


#if USE_VUPLEX
        private async void ReloadBrowser()
        {
			await webBrowser.WaitUntilInitialized();
			if (_data != null)
			{
				webBrowser.WebView.LoadHtml(_data);
			}
		}
#endif
		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventScreenAnalysisHumanViewReloadBrowser))
            {
#if USE_VUPLEX
				ReloadBrowser();
#endif
            }
			if (nameEvent.Equals(ImageDatabaseController.EventImageDatabaseControllerAvailableImage))
            {
				if ((bool)parameters[1])
                {
					int idImage = (int)parameters[0];					
					ImageUtils.LoadBytesSpriteResize(_originalImageSize, contentImage, ImageDatabaseController.Instance.GetImageDataByID(idImage));
					contentImage.gameObject.SetActive(true);
				}
            }
            if (nameEvent.Equals(UpdateAnalysisDataHTTP.EventUpdateAnalysisDataHTTPCompleted))
            {
				UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                if ((bool)parameters[0])
                {
                    string title = LanguageController.Instance.GetText("text.info");
                    string description = LanguageController.Instance.GetText("text.employee.analysis.success.sent");
                    ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, title, description);
                }
                else
                {
                    string title = LanguageController.Instance.GetText("text.error");
                    string description = LanguageController.Instance.GetText("text.employee.analysis.fail.sent");
                    ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, title, description);
                }
            }
			if (nameEvent.Equals(AICommandAnalysisHuman.EventAICommandAnalysisHumanResults))
			{
				string responseAnalysis = (string)parameters[0];
				if (responseAnalysis.Length > 0)
				{
					_responseAnalysis = responseAnalysis;
					_responseHistory = (string)parameters[1];
                    PlayerPrefs.SetString(CookieAnalysis, _responseAnalysis);
                    PlayerPrefs.SetString(CookieHistory, _responseHistory);
                    buttonPreviousAnalysis.gameObject.SetActive(_responseAnalysis.Length > 0);

					DisplayResultsAnalysis(_responseAnalysis);
                }
            }
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventScreenAnalysisHumanViewReloadCodeEditor))
			{
				GameObject screenLoading = (GameObject)parameters[0];
				UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationDestroy, screenLoading);
                GameObject.FindAnyObjectByType<CodeEditor>().Refresh();
            }
            if (nameEvent.Equals(SubEventScreenAnalysisHumanViewConfirmSendAnalysis))
            {
                if ((ScreenInformationResponses)parameters[1] == ScreenInformationResponses.Confirm)
                {
                    ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, null, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("text.now.sending.employee.analysis"));
					string dataToSend = _responseAnalysis + "\n\n-------------------------------------------------\n-------------------------------------------------\n\n" + _responseHistory;
                    WorkDayData.Instance.UpdateAnalysisData(UsersController.Instance.CurrentUser.Email + " (" + ApplicationController.Instance.HumanPlayer.NameHuman + "): " + ApplicationController.Instance.HumanPlayer.ItemData.Data, dataToSend);
                }
            }
            if (nameEvent.Equals(ItemHumanContributionView.EventItemHumanContributionViewSelected))
            {
				if ((GameObject)parameters[0] == SlotManagerContributions.gameObject)
				{
					if ((int)parameters[2] == -1)
                    {
						_data = "";
						_summary = "";
						IsImage = false;
						OnViewSource();
					}
					else
                    {
						ContributionHuman contribution = (ContributionHuman)parameters[3];
						switch (contribution.Type)
						{
							case ContributionType.Meeting:
								MeetingData meeting = WorkDayData.Instance.CurrentProject.GetMeetingByUID(contribution.UID);
								_data = meeting.PackXMLInteractions(true, ApplicationController.Instance.HumanPlayer.NameHuman);
								_summary = meeting.Summary;
								IsImage = false;
								OnViewSource();
								break;

							case ContributionType.Document:
								DocumentData docGlobal = WorkDayData.Instance.CurrentProject.GetDocumentByID(int.Parse(contribution.UID));
								_data = docGlobal.Data.GetHTML();
								_summary = docGlobal.Summary;
								IsImage = false;
								OnViewSource();
								IsImage = docGlobal.IsImage;
								if (_isImage)
                                {
									SystemEventController.Instance.DispatchSystemEvent(ImageDatabaseController.EventImageDatabaseControllerDownloadImage, int.Parse(_data), true);
								}
								break;

							case ContributionType.Task:
								var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(contribution.TaskUID);
								if (taskItemData != null)
								{
									List<DocumentData> docs = taskItemData.GetData();
									foreach (DocumentData doc in docs)
									{
										if (doc.Id.ToString() == contribution.UID)
										{
											_data = doc.Data.GetHTML();
											_summary = doc.Summary;
											IsImage = false;
											OnViewSource();
											IsImage = doc.IsImage;
											if (_isImage)
                                            {
												SystemEventController.Instance.DispatchSystemEvent(ImageDatabaseController.EventImageDatabaseControllerDownloadImage, int.Parse(_data), true);
											}
										}
									}
								}
								break;
						}
					}
				}
			}
		}
	}
}