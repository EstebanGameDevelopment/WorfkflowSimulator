#if UNITY_WEBGL && !UNITY_EDITOR
using Assets.SimpleFileBrowserForWebGL;
#endif
using InGameCodeEditor;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if USE_VUPLEX
using Vuplex.WebView;
#endif
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.WorkDayData;

namespace yourvrexperience.WorkDay
{
	public class ScreenDocumentsDataView : ScreenInformationView, IScreenView
	{
		public const string ScreenName = "ScreenDocumentsDataView";
		
		public const string EventScreenDocumentsDataViewInitialization = "EventScreenDocumentsDataViewInitialization";
		public const string EventScreenDocumentsDataViewUpdateGlobalData = "EventScreenDocumentsDataViewUpdateGlobalData";
		public const string EventScreenDocumentsDataViewSetUpDataDocument = "EventScreenDocumentsDataViewSetUpDataDocument";
		public const string EventScreenDocumentsDataViewUploadFile = "EventScreenDocumentsDataViewUploadFile";
		public const string EventScreenDocumentsDataViewReloadBrowser = "EventScreenDocumentsDataViewReloadBrowser";
		public const string EventScreenDocumentsDataViewFixAuthor = "EventScreenDocumentsDataViewFixAuthor";
		public const string EventScreenDocumentsDataViewForceAutomaticUpload = "EventScreenDocumentsDataViewForceAutomaticUpload";

		private enum LanguageThemes { NONE, C_SHARP, JAVASCRIPT, JSON, LUA, PYTHON }
		
		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private TextMeshProUGUI titleName;
		[SerializeField] private TextMeshProUGUI titleDescription;

		[SerializeField] private CustomInput inputNameData;
		[SerializeField] private CustomInput inputDescriptionData;
		[SerializeField] private CodeEditor inputCodeEditor;

		[SerializeField] private Button buttonAddData;
		[SerializeField] private Button buttonCleanData;
		
		[SerializeField] private Button buttonAddProject;
		[SerializeField] private Button buttonAddLocal;
		[SerializeField] private Button buttonRefresh;

		[SerializeField] private SlotManagerView SlotManagerCurrentData;
		[SerializeField] private SlotManagerView SlotManagerProjectData;
		[SerializeField] private GameObject PrefabData;

		[SerializeField] private Button buttonHTML;
		[SerializeField] private Button buttonBrowser;
		[SerializeField] private Button buttonSummary;
		[SerializeField] private Button buttonImageGen;

#if USE_VUPLEX
		[SerializeField] private CanvasWebViewPrefab webBrowser;
#endif

		[SerializeField] private TMP_Dropdown DropDownMembers;
		[SerializeField] private IconColorView colorDocument;

		[SerializeField] private TMP_Dropdown DropDownFileType;
		[SerializeField] private Button buttonUploadImage;
		[SerializeField] private Button buttonDownloadImage;
		[SerializeField] private Image contentImage;

		private int _projectIdSelected;
		private List<DocumentData> _dataLocal;
		private List<DocumentData> _dataGlobal;
		private DocumentData _currentData;
		private bool _isGlobal = false;
		private bool _isImage = false;
		private bool _isSelectionLocal = false;
		private bool _isGlobalSelection = false;

		private TabsData _tabSelected = TabsData.HTML;
		private HTMLData _dataDoc;
		private string _newSummary = "";
		private string _owner = "";

		private byte[] _imageBytes;
		private Vector2 _originalImageSize;
		private List<string> _imagesCreated = new List<string>();
		private bool _forceAutomaticUploadGeneratedImage = false;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			buttonAddData.onClick.AddListener(OnAddData);
			buttonCleanData.onClick.AddListener(OnCleanData);
			buttonAddData.interactable = false;

			buttonAddProject.interactable = false;
			buttonAddProject.onClick.AddListener(OnAddToGlobal);

			buttonAddLocal.interactable = false;
			buttonAddLocal.onClick.AddListener(OnAddToLocal);

			buttonRefresh.interactable = false;
			buttonRefresh.onClick.AddListener(OnRefreshGlobal);

			inputNameData.onValueChanged.AddListener(OnNameDocumentChanged);
			inputDescriptionData.onValueChanged.AddListener(OnDescriptionDocumentChanged);
			_inputValue.onValueChanged.AddListener(OnDataDocumentChanged);

			buttonHTML.onClick.AddListener(OnEditorHTML);
			buttonBrowser.onClick.AddListener(OnViewBrowser);
			buttonSummary.onClick.AddListener(OnViewSummary);
			
			_dataDoc.SetHTML("");
			OnEditorHTML();
			
			List<WorldItemData> humans = ApplicationController.Instance.LevelView.GetHumans();
			if ((humans == null) || (humans.Count == 0))
			{
				DropDownMembers.gameObject.SetActive(false);
			}
			else
			{
				DropDownMembers.ClearOptions();
				DropDownMembers.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("screen.calendar." + WorkDayData.RESERVED_NAME_NOBODY)));
				List<GroupInfoData> groups = WorkDayData.Instance.CurrentProject.GetGroups();
				foreach (GroupInfoData group in groups)
				{
					DropDownMembers.options.Add(new TMP_Dropdown.OptionData(group.Name));
				}
				foreach (WorldItemData human in humans)
				{
					DropDownMembers.options.Add(new TMP_Dropdown.OptionData(human.Name));
				}
				DropDownMembers.value = 0;
				_owner = LanguageController.Instance.GetText("screen.calendar." + WorkDayData.RESERVED_NAME_NOBODY);
				DropDownMembers.onValueChanged.AddListener(OnSelectedHuman);
			}

			DropDownFileType.ClearOptions();
			DropDownFileType.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("format.text")));
			DropDownFileType.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("format.image")));
			DropDownFileType.value = 0;
			DropDownFileType.onValueChanged.AddListener(OnFileTypeChanged);
			if (!ApplicationController.Instance.IsImageAuthorized())
			{
				DropDownFileType.interactable = false;
			}

			buttonUploadImage.onClick.AddListener(OnUploadImage);
			buttonDownloadImage.onClick.AddListener(OnDownloadImage);
			buttonImageGen.onClick.AddListener(OnGenerateImage);

			buttonUploadImage.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.upload.image");
            buttonDownloadImage.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.download.image");
            buttonImageGen.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.generate.image");
            buttonHTML.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.data");
            buttonBrowser.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.view");
            buttonSummary.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("text.summary");

            _originalImageSize = contentImage.GetComponent<RectTransform>().sizeDelta;

			buttonImageGen.gameObject.SetActive(false);			

			SystemEventController.Instance.Event += OnSystemEvent;

			titleScreen.text = ""; 
            titleName.text = LanguageController.Instance.GetText("text.document.name");
            titleDescription.text = LanguageController.Instance.GetText("text.document.description");

            ForceOwnerToPlayer();
		}

        public override void Destroy()
		{
			base.Destroy();

			_currentData = null;
			_dataLocal = null;
			_dataGlobal = null;

			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
		}
		
		protected override void OnCancel()
        {
            base.OnCancel();

			if (_imagesCreated != null)
            {
				string imagesToDelete = "";
				foreach (string idImage in _imagesCreated)
                {
					if (imagesToDelete.Length > 0) imagesToDelete += ";";
					imagesToDelete += idImage;
				}
				_imagesCreated.Clear();
				WorkDayData.Instance.DeleteImage(imagesToDelete);
            }
        }

        protected override void OnConfirmation()
		{
			try
			{
				// IS CURRENT DOCUMENT PROGRESS CONTRIBUTION
				if (!_isGlobal && (_dataGlobal == null))
                {
					if (_dataLocal.Count == 0)
                    {
						OnAddData();
					}
                }

				if (_isGlobal)
                {
					// UPDATE GLOBAL
					UIEventController.Instance.DispatchUIEvent(EventScreenDocumentsDataViewUpdateGlobalData, _dataLocal);					
				}
				else
                {
					if (_dataGlobal == null)
                    {
						// UPDATE LOCAL
						if (_customOutputEvent != null)
						{
							if (_customOutputEvent.Length > 0)
							{
								UIEventController.Instance.DispatchUIEvent(_customOutputEvent, _origin, ScreenInformationResponses.Confirm, _dataLocal);
							}
						}
						UIEventController.Instance.DispatchUIEvent(EventScreenInformationResponse, _origin, ScreenInformationResponses.Confirm, _dataLocal);
					}
					else
                    {
						// UPDATE LOCAL
						if (_customOutputEvent != null)
						{
							if (_customOutputEvent.Length > 0)
							{
								UIEventController.Instance.DispatchUIEvent(_customOutputEvent, _origin, ScreenInformationResponses.Confirm, _dataLocal);
							}
						}
						UIEventController.Instance.DispatchUIEvent(EventScreenInformationResponse, _origin, ScreenInformationResponses.Confirm, _dataLocal);

						// UPDATE GLOBAL
						UIEventController.Instance.DispatchUIEvent(EventScreenDocumentsDataViewUpdateGlobalData, _dataGlobal);
					}
				}

				UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
			}
			catch (Exception err) { };
		}

		private void OnFileTypeChanged(int value)
		{
			bool wasImage = _isImage;
			_isImage = (FileDocType.Image == (FileDocType)value);
			if (wasImage && !_isImage)
			{
				int idImage;
				if (int.TryParse(_dataDoc.GetHTML(), out idImage))
				{
					if (idImage != -1)
					{
						WorkDayData.Instance.DeleteImage(idImage.ToString());
					}
				}
				_inputValue.text = "";
			}
			if (_currentData != null)
            {
				_currentData.IsImage = _isImage;
				_currentData.IsChanged = true;
			}
			UpdateTabVisibility();
		}

		private void UpdateTabVisibility()
        {
			if (_isImage)
			{
				buttonImageGen.gameObject.SetActive(true);
				inputCodeEditor.gameObject.SetActive(false);
#if USE_VUPLEX
				webBrowser.gameObject.SetActive(false);
#endif
				buttonHTML.gameObject.SetActive(false);
				buttonBrowser.gameObject.SetActive(false);

				buttonUploadImage.gameObject.SetActive(true);
				buttonDownloadImage.gameObject.SetActive(true);
				contentImage.gameObject.SetActive(true);

				if (_tabSelected == TabsData.SUMMARY)
				{
					inputCodeEditor.gameObject.SetActive(true);
					contentImage.gameObject.SetActive(false);
#if USE_VUPLEX
					webBrowser.gameObject.SetActive(false);
#endif
				}
				buttonSummary.interactable = true;
			}
			else
			{
				buttonImageGen.gameObject.SetActive(false);
				inputCodeEditor.gameObject.SetActive(true);
#if USE_VUPLEX
				webBrowser.gameObject.SetActive(true);
#endif
				buttonHTML.gameObject.SetActive(true);
				buttonBrowser.gameObject.SetActive(true);

				buttonUploadImage.gameObject.SetActive(false);
				buttonDownloadImage.gameObject.SetActive(false);
				contentImage.gameObject.SetActive(false);

				switch (_tabSelected)
				{
					case TabsData.HTML:
						inputCodeEditor.gameObject.SetActive(true);
#if USE_VUPLEX
						webBrowser.gameObject.SetActive(false);
#endif
						buttonHTML.interactable = false;
						buttonBrowser.interactable = true;
						buttonSummary.interactable = true;
						break;
					case TabsData.CSS:
						inputCodeEditor.gameObject.SetActive(true);
#if USE_VUPLEX
						webBrowser.gameObject.SetActive(false);
#endif
						buttonHTML.interactable = true;
						buttonBrowser.interactable = true;
						buttonSummary.interactable = true;
						break;
					case TabsData.BROWSER:
						inputCodeEditor.gameObject.SetActive(false);
#if USE_VUPLEX
						webBrowser.gameObject.SetActive(true);
#endif
						buttonHTML.interactable = true;
						buttonBrowser.interactable = false;
						buttonSummary.interactable = true;
						break;
					case TabsData.SUMMARY:
						inputCodeEditor.gameObject.SetActive(true);
#if USE_VUPLEX
						webBrowser.gameObject.SetActive(false);
#endif
						buttonHTML.interactable = true;
						buttonBrowser.interactable = true;
						buttonSummary.interactable = false;
						break;
				}
				inputCodeEditor.Refresh(true);
			}
		}

        private void OnDownloadImage()
        {
			if (_dataDoc.GetHTML().Length > 0)
			{
				int idImage;
				if (int.TryParse(_dataDoc.GetHTML(), out idImage))
				{
					if (idImage != -1)
                    {
						Application.OpenURL(DownloadImageDataHTTP.GetURLToDownload(idImage));
					}					
				}
			}
			_tabSelected = TabsData.HTML;
			UpdateTabVisibility();
        }

        private void OnUploadImage()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WebFileBrowser.Upload(OnWebBrowserUploadImage, "images/*");
#else
			ApplicationController.Instance.CreateFileBrowser(LanguageController.Instance.GetText("screen.image.creating.select.file"), EventScreenDocumentsDataViewUploadFile, "png", "jpg", "jpeg");
#endif
			_tabSelected = TabsData.HTML;
			UpdateTabVisibility();
		}

		private void OnWebBrowserUploadImage(string fileName, string mime, byte[] bytes)
		{
			SystemEventController.Instance.DispatchSystemEvent(EventScreenDocumentsDataViewUploadFile, true, bytes);
		}

		private void OnEditorHTML()
		{
			_tabSelected = TabsData.HTML;
			if (_currentData != null)
            {
				string dataRawHTML = _currentData.Data.GetHTML();
				_inputValue.text = dataRawHTML;				
			}
			else
            {				
				_inputValue.text = _dataDoc.GetHTML();
			}
			UpdateTabVisibility();
		}

		private void OnViewBrowser()
		{
			_tabSelected = TabsData.BROWSER;
			UpdateTabVisibility();
			SystemEventController.Instance.DelaySystemEvent(EventScreenDocumentsDataViewReloadBrowser, 0.1f);
		}

		private void OnGenerateImage()
		{
			GameObject screen = ScreenController.Instance.CreateScreen(ScreenImageGenerationView.ScreenName, false, false);
			UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, screen, _canvas.sortingOrder + 1);
		}

		private void OnViewSummary()
		{
			if (_isImage)
            {
				if (_tabSelected != TabsData.SUMMARY)
                {
					_tabSelected = TabsData.SUMMARY;
				}
				else
                {
					_tabSelected = TabsData.HTML;
				}
				if (_tabSelected == TabsData.SUMMARY)
                {
					if (_currentData != null)
					{
						string dataRawHTML = _currentData.Summary;
						_inputValue.text = dataRawHTML;
					}
					else
					{
						_inputValue.text = _newSummary;
					}
					inputCodeEditor.Refresh();
				}
			}
			else
            {
				_tabSelected = TabsData.SUMMARY;
				if (_currentData != null)
				{
					string dataRawHTML = _currentData.Summary;
					_inputValue.text = dataRawHTML;
				}
				else
				{
					_inputValue.text = _newSummary;
				}
				inputCodeEditor.Refresh();
			}
			UpdateTabVisibility();
		}

#if USE_VUPLEX
		private async void ReloadBrowser()
        {
			await webBrowser.WaitUntilInitialized();
			if (_currentData != null)
			{
				webBrowser.WebView.LoadHtml(_currentData.Data.GetHTML());
			}
			else
			{
				webBrowser.WebView.LoadHtml(_dataDoc.GetHTML());
			}
		}
#endif
		private void OnCleanData()
		{
			SetCurrentData(null, false, false);
			DropDownFileType.interactable = true;
			if (!ApplicationController.Instance.IsImageAuthorized())
			{
				DropDownFileType.interactable = false;
			}
			ForceOwnerToPlayer();
			UIEventController.Instance.DispatchUIEvent(ItemDataView.EventItemDataViewUnSelecteAll);
		}

		private void UpdateStateSave()
        {
			buttonAddData.interactable = false;
			if (_currentData == null)
			{
				if ((inputNameData.text.Length > 0) && (inputDescriptionData.text.Length > 0) && (_inputValue.text.Length > 0))
				{
					DocumentData tmpDocument = new DocumentData(-1, _projectIdSelected, inputNameData.text, inputDescriptionData.text, _owner, _dataDoc, _isGlobal, _isImage, _newSummary, false, -1, -1);

					if (_isGlobal)
					{
						if (!_dataLocal.Contains(tmpDocument))
						{
							buttonAddData.interactable = true;
						}
					}
					else
					{
						if (_dataGlobal != null)
                        {
							if (!_dataLocal.Contains(tmpDocument) && !_dataGlobal.Contains(tmpDocument))
							{
								buttonAddData.interactable = true;
							}
						}
						else
                        {
							if (!_dataLocal.Contains(tmpDocument))
							{
								buttonAddData.interactable = true;
							}
						}
					}
				}
			}
		}

		private void OnAddData()
		{
			if ((inputNameData.text.Length > 0) && (inputDescriptionData.text.Length > 0) && (_inputValue.text.Length > 0))
            {
				if (_isImage)
                {
					int idImage;
					if (int.TryParse(_inputValue.text, out idImage))
					{
						if (_imageBytes == null)
                        {
							AddNewImageUploaded(idImage);
						}
						else
                        {
							ImageDatabaseController.Instance.UploadImageData(idImage, inputNameData.text, _imageBytes, _canvas.sortingOrder + 1);
						}
					}
				}
				else
                {
					AddNewDocument();
				}
			}
		}

		private void AddNewDocument()
        {
			bool isChanged = true;
			if ((_newSummary != null) && (_newSummary.Length > 2))
            {
				isChanged = false;
			}
			DocumentData newDocument = new DocumentData(WorkDayData.Instance.CurrentProject.GetDocumentNextID(), _projectIdSelected, inputNameData.text, inputDescriptionData.text, _owner, _dataDoc, _isGlobal, _isImage, _newSummary, isChanged, -1, -1);
			if (!_dataLocal.Contains(newDocument))
			{
				_dataLocal.Add(newDocument);
				LoadData(SlotManagerCurrentData, _dataLocal);
				SetCurrentData(null, false, false);
				if (_forceAutomaticUploadGeneratedImage)
                {
					_forceAutomaticUploadGeneratedImage = false;
					UIEventController.Instance.DispatchUIEvent(ItemDataView.EventItemDataViewForceSelection, newDocument.Id);
				}
			}
		}

		private void SetCurrentData(DocumentData document, bool isSelectionLocal, bool isGlobalSelection)
        {
			_isSelectionLocal = isSelectionLocal;
			_isGlobalSelection = isGlobalSelection;
			buttonAddProject.interactable = false;
			buttonAddLocal.interactable = false;
			contentImage.GetComponent<RectTransform>().sizeDelta = _originalImageSize;
			contentImage.overrideSprite = null;
			buttonUploadImage.interactable = true;
			buttonImageGen.interactable = true;
			if (document != null)
            {				
				_currentData = document;				
				inputNameData.text = _currentData.Name;
				inputDescriptionData.text = _currentData.Description;
				_dataDoc = _currentData.Data;
				_isImage = _currentData.IsImage;

				DropDownFileType.onValueChanged.RemoveListener(OnFileTypeChanged);
				DropDownFileType.value = (int)(_isImage? FileDocType.Image: FileDocType.Text);
				DropDownFileType.onValueChanged.AddListener(OnFileTypeChanged);

				if (!ApplicationController.Instance.IsImageAuthorized())
                {
					DropDownFileType.interactable = false;
				}

				OnEditorHTML();
				_inputValue.verticalScrollbar.value = 0;				

				inputNameData.interactable = true;
				inputDescriptionData.interactable = true;
				_inputValue.interactable = true;

				_owner = _currentData.Owner;
				Color colorOwner = WorkDayData.Instance.CurrentProject.GetColorForMember(_owner);
				bool isColorAssigned = false;
				if (colorOwner != Color.white)
                {
					for (int i = 0; i < DropDownMembers.options.Count; i++)
					{
						if (DropDownMembers.options[i].text.Equals(_owner))
                        {
							isColorAssigned = true;
							DropDownMembers.value = i;
							break;
                        }
					}
				}
				if (!isColorAssigned)
                {
					DropDownMembers.value = 0;
				}

				DropDownMembers.interactable = true;
				DropDownFileType.interactable = true;
				if (_isSelectionLocal)
                {
					if (_currentData.IsGlobal)
                    {
						inputNameData.interactable = false;
						inputDescriptionData.interactable = false;
						_inputValue.interactable = false;
						DropDownMembers.interactable = false;
						DropDownFileType.interactable = false;
					}
				}
				if (!ApplicationController.Instance.IsImageAuthorized())
				{
					DropDownFileType.interactable = false;
				}

				if (_isImage)
                {
					int idImage = _currentData.GetImageID();
					if (idImage != -1)
                    {
						buttonUploadImage.interactable = false;
						buttonImageGen.interactable = false;
						SystemEventController.Instance.DispatchSystemEvent(ImageDatabaseController.EventImageDatabaseControllerDownloadImage, idImage, true);
					}
				}
			}
			else
            {
				_currentData = null;
				inputNameData.text = "";
				inputDescriptionData.text = "";
				_inputValue.text = "";
				_owner = "";
				DropDownMembers.value = 0;
				DropDownMembers.interactable = true;

				inputNameData.interactable = true;
				inputDescriptionData.interactable = true;
				_inputValue.interactable = true;
				_isImage = false;
				DropDownFileType.value = 0;
			}
			CodeEditor codeEditor = GameObject.FindAnyObjectByType<CodeEditor>();
			if (codeEditor != null)
			{
				codeEditor.Refresh(true);
			}
			UpdateStateSave();

			if (ApplicationController.Instance.IsPlayMode)
			{
				if (_isGlobalSelection 
					|| ((_currentData != null) && _currentData.IsGlobal && !_currentData.IsUserCreated))
				{
					buttonAddProject.interactable = false;
					buttonAddLocal.interactable = false;
					inputNameData.interactable = false;
					inputDescriptionData.interactable = false;
					DropDownMembers.interactable = false;
					DropDownFileType.interactable = false;
				}

				if (_currentData == null)
                {
					ForceOwnerToPlayer();
				}
				else
                {
					if (!_currentData.Owner.Equals(ApplicationController.Instance.HumanPlayer.NameHuman))
					{
						DropDownMembers.interactable = false;
					}
					else
					{
						ForceOwnerToPlayer();
					}
				}
			}
		}

		private void ForceOwnerToPlayer()
        {
			if (ApplicationController.Instance.IsPlayMode)
			{
				DropDownMembers.interactable = true;
				for (int i = 0; i < DropDownMembers.options.Count; i++)
				{
					if (DropDownMembers.options[i].text.Equals(ApplicationController.Instance.HumanPlayer.NameHuman))
					{
						DropDownMembers.value = i;
					}
				}
				DropDownMembers.interactable = false;
			}
		}

		private void LoadData(SlotManagerView slotManager, List<DocumentData> documents)
		{
			slotManager.ClearCurrentGameObject(true);
			slotManager.Initialize(0, new List<ItemMultiObjectEntry>(), PrefabData);

			List<DocumentData> documentCurrentProject = new List<DocumentData>();
			for (int i = 0; i < documents.Count; i++)
			{
				if (documents[i].ProjectId == _projectIdSelected)
                {
					documentCurrentProject.Add(documents[i]);
				}				
			}

			for (int i = 0; i < documentCurrentProject.Count; i++)
			{
				slotManager.AddItem(new ItemMultiObjectEntry(slotManager.gameObject, slotManager.Data.Count, documentCurrentProject[i]));
			}
		}

		private bool _considerChangeGlobal = true;

		private void OnNameDocumentChanged(string value)
		{
			if (_currentData != null)
			{
				_currentData.Name = value;
				UIEventController.Instance.DispatchUIEvent(ItemDataView.EventItemDataViewRefresh, _currentData);
				if (_considerChangeGlobal && _currentData.IsGlobal)
				{
					buttonRefresh.interactable = true;
				}
			}
			UpdateStateSave();
		}

		private void OnSelectedHuman(int value)
		{
			_owner = DropDownMembers.options[value].text;
			colorDocument.Refresh();
			if (_currentData != null)
			{
				_currentData.Owner = _owner;
				UIEventController.Instance.DispatchUIEvent(ItemDataView.EventItemDataViewRefresh, _currentData);
				if (_considerChangeGlobal && _currentData.IsGlobal)
				{
					buttonRefresh.interactable = true;
				}
			}
		}

		private void OnDescriptionDocumentChanged(string value)
		{
			if (_currentData != null)
			{
				string previousData = _currentData.Description;
				_currentData.Description = value;
				if (_currentData.Description != previousData)
				{
					_currentData.IsChanged = true;
				}
				if (_considerChangeGlobal && _currentData.IsGlobal)
				{
					buttonRefresh.interactable = true;
				}
			}
			UpdateStateSave();
		}

		private void OnDataDocumentChanged(string value)
		{
			if (_currentData != null)
			{
				bool enableChange = false;
				if (!ApplicationController.Instance.IsPlayMode)
                {
					enableChange = true;
				}
				if (ApplicationController.Instance.IsPlayMode && _currentData.IsUserCreated)
				{
					enableChange = true;
				}
				if (enableChange)
				{ 
					if (_tabSelected == TabsData.HTML)
					{
						string previousData = _currentData.Data.GetHTML();
						_currentData.Data.SetHTML(value);
						if (previousData != value)
						{
							_currentData.IsChanged = true;
						}

						if (_considerChangeGlobal && _currentData.IsGlobal)
						{
							buttonRefresh.interactable = true;
						}
					}
					if (_tabSelected == TabsData.SUMMARY)
					{
						_currentData.Summary = value;
					}
				}
			}
			else
            {
				if (_tabSelected == TabsData.HTML)
				{
					_dataDoc.SetHTML(value);
				}
				if (_tabSelected == TabsData.SUMMARY)
				{
					_newSummary = value;
				}				
			}
			
			UpdateStateSave();
		}

		private void OnAddToGlobal()
		{
			if (_isSelectionLocal)
            {
				if (_currentData != null)
				{
					if (!_dataGlobal.Contains(_currentData))
                    {
						_currentData.IsGlobal = true;
						_dataGlobal.Add(_currentData.Clone());
						LoadData(SlotManagerProjectData, _dataGlobal);

						SetCurrentData(null, false, false);
						UIEventController.Instance.DispatchUIEvent(ItemDataView.EventItemDataViewUnSelecteAll);
					}
				}
			}
		}

		private void OnAddToLocal()
		{
			if (_isGlobalSelection)
			{
				if (_currentData != null)
				{
					if (!_dataLocal.Contains(_currentData))
					{
						_dataLocal.Add(_currentData.Clone());
						LoadData(SlotManagerCurrentData, _dataLocal);

						SetCurrentData(null, false, false);
						UIEventController.Instance.DispatchUIEvent(ItemDataView.EventItemDataViewUnSelecteAll);
					}
				}
			}
		}

		private void OnRefreshGlobal()
		{
			// UPDATE GLOBAL
			UIEventController.Instance.DispatchUIEvent(EventScreenDocumentsDataViewUpdateGlobalData, _dataGlobal);
			LoadData(SlotManagerCurrentData, _dataLocal);
			buttonRefresh.interactable = false;
		}

		private void ResizeImageToUpload()
		{
			Vector2Int resolutionImage = ImageUtils.GetImageResolution(_imageBytes);
			if (resolutionImage.x > 720)
			{
				int finalWidth = 720;
				int finalHeight = (int)((720f / (float)resolutionImage.x) * (float)resolutionImage.y);
				_imageBytes = ImageUtils.ResizeImage(_imageBytes, finalWidth, finalHeight);
			}
			else
			{
				if (resolutionImage.y > 720)
				{
					int finalHeight = 720;
					int finalWidth = (int)((720f / (float)resolutionImage.y) * (float)resolutionImage.x);
					_imageBytes = ImageUtils.ResizeImage(_imageBytes, finalWidth, finalHeight);
				}
			}
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ScreenImageGenerationView.EventScreenImageGenerationViewImageCompleted))
            {
				UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
				if ((bool)parameters[0])
				{
					_imageBytes = (byte[])parameters[1];
					ResizeImageToUpload();
					ImageUtils.LoadBytesSpriteResize(_originalImageSize, contentImage, _imageBytes);
					_dataDoc.SetHTML("-1");
					_inputValue.text = "-1";
					bool uploadImage = false;
					if (_forceAutomaticUploadGeneratedImage)
                    {
						uploadImage = true;
					}
					if (_currentData != null)
					{
						uploadImage = true;
					}

					if (!uploadImage)
                    {
						UpdateStateSave();
					}					
					else
					{
						int idImage = -1;
						if (!int.TryParse(_dataDoc.GetHTML(), out idImage))
						{
							idImage = -1;
						}
						ImageDatabaseController.Instance.UploadImageData(idImage, inputNameData.text, _imageBytes, _canvas.sortingOrder + 1);
					}
				}
				else
				{
					ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, this.gameObject, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.image.creating.no.image.created"));
					UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, ScreenInformationView.ScreenInformation, _canvas.sortingOrder + 1);
				}
			}
			if (nameEvent.Equals(EventScreenDocumentsDataViewReloadBrowser))
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
					buttonUploadImage.interactable = false;
					buttonImageGen.interactable = false;
					UpdateStateSave();
				}
            }
			if (nameEvent.Equals(EventScreenDocumentsDataViewUploadFile))
			{
				if ((bool)parameters[0])
				{
#if UNITY_WEBGL && !UNITY_EDITOR
                    _imageBytes = (byte[])parameters[1];
#else
					string filePath = (string)parameters[1];
					FileInfo fileInfo = new FileInfo(filePath);
					long fileSizeInBytes = (int)fileInfo.Length;
					float sizeFile = (float)(fileSizeInBytes / 1000000f);
					_imageBytes = System.IO.File.ReadAllBytes(filePath);
#endif

					ResizeImageToUpload();
					ImageUtils.LoadBytesSpriteResize(_originalImageSize, contentImage, _imageBytes);
					_dataDoc.SetHTML("-1");
					_inputValue.text = "-1";

					bool uploadImage = false;
					if (_forceAutomaticUploadGeneratedImage)
					{
						uploadImage = true;
					}
					if (_currentData != null)
					{
						uploadImage = true;
					}

					if (!uploadImage)
					{
						UpdateStateSave();
					}
					else
					{
						int idImage = -1;
						if (!int.TryParse(_dataDoc.GetHTML(), out idImage))
						{
							idImage = -1;
						}
						ImageDatabaseController.Instance.UploadImageData(idImage, inputNameData.text, _imageBytes, _canvas.sortingOrder + 1);
					}
				}
				else
				{
					ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, this.gameObject, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.image.creating.no.image.selected"));
					UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, ScreenInformationView.ScreenInformation, _canvas.sortingOrder + 1);
				}
			}
			if (nameEvent.Equals(UploadImageDataHTTP.EventUploadImageDataHTTPCompleted))
			{
				UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationByNameDestroy, ScreenInformationView.ScreenLoadingImage);
				if ((bool)parameters[0])
				{
					AddNewImageUploaded((int)parameters[1]);
				}
			}
		}

		private void AddNewImageUploaded(int idUploadedImage)
        {
			string idImage = idUploadedImage.ToString();
			_inputValue.text = idImage;
			_dataDoc.SetHTML(_inputValue.text);
			if (_currentData == null)
            {
				if (!_imagesCreated.Contains(idImage)) _imagesCreated.Add(idImage);
				AddNewDocument();
			}
			else
            {
				_currentData.IsImage = true;
				_currentData.IsChanged = true;
				_currentData.Data.SetHTML(_inputValue.text);
				buttonUploadImage.interactable = false;
				buttonImageGen.interactable = false;
			}
		}

		protected override void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventScreenInformationRequestAllScreensDestroyed))
			{
				return;
			}

			base.OnUIEvent(nameEvent, parameters);
			
			if (nameEvent.Equals(EventScreenDocumentsDataViewFixAuthor))
            {
				string authorFixed = (string)parameters[0];
				int indexSelected = -1;
				for (int i = 0; i < DropDownMembers.options.Count; i++)
				{
					if (DropDownMembers.options[i].text.Equals(authorFixed))
					{
						DropDownMembers.value = i;						
						break;
					}
				}
				if (indexSelected != -1) OnSelectedHuman(indexSelected);
				DropDownMembers.interactable = false;
			}
			if (nameEvent.Equals(EventScreenDocumentsDataViewForceAutomaticUpload))
            {
				_forceAutomaticUploadGeneratedImage = true;
            }
			if (nameEvent.Equals(EventScreenDocumentsDataViewSetUpDataDocument))
            {				
				OnCleanData();

				inputNameData.text = (string)parameters[0];
				inputDescriptionData.text = (string)parameters[1];
				_inputValue.text = (string)parameters[2];
				_newSummary = (string)parameters[3];

				int idImage = ScreenMultiInputDataView.GetImageFromText(_inputValue.text);
				if (idImage != -1)
                {
					DropDownFileType.value = 1;
					_dataDoc.SetHTML(idImage.ToString());
					_inputValue.text = idImage.ToString();
					buttonUploadImage.interactable = false;
					buttonImageGen.interactable = false;
					DropDownFileType.interactable = false;
					if (idImage != -1)
					{
						SystemEventController.Instance.DelaySystemEvent(ImageDatabaseController.EventImageDatabaseControllerDownloadImage, 0.2f, idImage, true);
					}
				}
				else
                {
					inputCodeEditor.Refresh();
				}
			}
			if (nameEvent.Equals(EventScreenDocumentsDataViewInitialization))
            {
				_projectIdSelected = (int)parameters[0];
				if (_projectIdSelected != -1)
                {
					ProjectInfoData projectInfo = WorkDayData.Instance.CurrentProject.GetProject(_projectIdSelected);
					_content.GetComponent<Image>().color = projectInfo.GetColor();
                }
				_isGlobal = (bool)parameters[1];
				if (_isGlobal)
                {
                    titleScreen.text = LanguageController.Instance.GetText("text.documents.global");

                    _dataLocal = (List<DocumentData>)parameters[2];
					LoadData(SlotManagerCurrentData, _dataLocal);

					buttonAddProject.gameObject.SetActive(false);
					buttonAddLocal.gameObject.SetActive(false);
					SlotManagerProjectData.gameObject.SetActive(false);
					buttonRefresh.gameObject.SetActive(false);
				}
				else
                {
                    titleScreen.text = LanguageController.Instance.GetText("text.documents.local");

                    _dataLocal = (List<DocumentData>)parameters[2];
					LoadData(SlotManagerCurrentData, _dataLocal);

					if (parameters.Length > 3)
                    {
						_dataGlobal = (List<DocumentData>)parameters[3];
						LoadData(SlotManagerProjectData, _dataGlobal);

						buttonRefresh.gameObject.SetActive(true);
						buttonRefresh.interactable = false;
					}
					else
                    {
						_dataGlobal = null;
						buttonAddProject.gameObject.SetActive(false);
						buttonAddLocal.gameObject.SetActive(false);
						SlotManagerProjectData.gameObject.SetActive(false);
						buttonRefresh.gameObject.SetActive(false);
					}
				}
				inputCodeEditor.Refresh(true);
			}
			if (nameEvent.Equals(ItemDataView.EventItemDataViewSelected))
            {
				_considerChangeGlobal = false;
				if (_isGlobal)
				{					
					if ((GameObject)parameters[0] == SlotManagerCurrentData.gameObject)
					{
						if ((int)parameters[2] == -1)
						{
							SetCurrentData(null, false, false);
						}
						else
						{
							SetCurrentData((DocumentData)parameters[3], false, false);							
						}
					}
				}
				else
                {
					if ((GameObject)parameters[0] == SlotManagerCurrentData.gameObject)
					{
						if ((int)parameters[2] == -1)
						{
							SetCurrentData(null, false, false);
							buttonAddProject.interactable = false;
						}
						else
						{
							DocumentData selectedLocalDocument = (DocumentData)parameters[3];
							SetCurrentData(selectedLocalDocument, true, false);
							if (_dataGlobal != null)
                            {
								UIEventController.Instance.DispatchUIEvent(ItemDataView.EventItemDataViewUnSelectContainer, SlotManagerProjectData.gameObject);
								if (!_dataGlobal.Contains(selectedLocalDocument))
								{
									if ((selectedLocalDocument.Summary != null) && (selectedLocalDocument.Summary.Length > 5))
                                    {
										buttonAddProject.interactable = true;
									}									
								}
							}
						}
					}
					if ((GameObject)parameters[0] == SlotManagerProjectData.gameObject)
					{
						if ((int)parameters[2] == -1)
						{
							SetCurrentData(null, false, false);
						}
						else
						{
							DocumentData selectedGlobalDocument = (DocumentData)parameters[3];
							UIEventController.Instance.DispatchUIEvent(ItemDataView.EventItemDataViewUnSelectContainer, SlotManagerCurrentData.gameObject);							
							SetCurrentData(selectedGlobalDocument, false, true);
							if (!_dataLocal.Contains(selectedGlobalDocument))
							{
								buttonAddLocal.interactable = true;
							}
						}
					}
				}
				_considerChangeGlobal = true;
			}
			if (nameEvent.Equals(ItemDataView.EventItemDataViewDelete))
            {
				if (_isGlobal)
				{
					if ((GameObject)parameters[0] == SlotManagerCurrentData.gameObject)
                    {
						DocumentData dataToDelete = (DocumentData)parameters[2];
						if (dataToDelete != null)
						{
							if (_dataLocal.Remove(dataToDelete))
							{
								if (dataToDelete.IsImage)
                                {
									WorkDayData.Instance.DeleteImage(dataToDelete.GetImageID().ToString());
                                }
								LoadData(SlotManagerCurrentData, _dataLocal);
								OnCleanData();
							}
						}
					}
				}
				else
                {
					if ((GameObject)parameters[0] == SlotManagerCurrentData.gameObject)
					{
						DocumentData dataToDelete = (DocumentData)parameters[2];
						if (dataToDelete != null)
						{
							if (_dataLocal.Remove(dataToDelete))
							{
								if (dataToDelete.IsImage)
								{
									WorkDayData.Instance.DeleteImage(dataToDelete.GetImageID().ToString());
								}
								LoadData(SlotManagerCurrentData, _dataLocal);
								OnCleanData();
							}
						}
					}

					if ((GameObject)parameters[0] == SlotManagerProjectData.gameObject)
					{
						DocumentData dataToDelete = (DocumentData)parameters[2];
						if (dataToDelete != null)
						{
							if (_dataGlobal.Remove(dataToDelete))
							{
								if (dataToDelete.IsImage)
								{
									WorkDayData.Instance.DeleteImage(dataToDelete.GetImageID().ToString());
								}
								LoadData(SlotManagerProjectData, _dataGlobal);
								OnCleanData();
							}
							if (_dataLocal.Remove(dataToDelete))
                            {
								if (dataToDelete.IsImage)
								{
									WorkDayData.Instance.DeleteImage(dataToDelete.GetImageID().ToString());
								}
								LoadData(SlotManagerCurrentData, _dataLocal);
								OnCleanData();
							}
						}
					}
				}
			}
		}
	}
}