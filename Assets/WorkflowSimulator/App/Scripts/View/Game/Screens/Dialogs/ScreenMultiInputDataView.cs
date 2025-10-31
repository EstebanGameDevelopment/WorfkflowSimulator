#if UNITY_WEBGL && !UNITY_EDITOR
using Assets.SimpleFileBrowserForWebGL;
#endif
using InGameCodeEditor;
using System;
using System.IO;
using System.Text.RegularExpressions;
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
	public class ScreenMultiInputDataView : ScreenInformationView, IScreenView
	{
		public const string ScreenName = "ScreenMultiInputDataView";

		private const string IMAGE_START_TAG = "<image_id>";
		private const string IMAGE_END_TAG = "</image_id>";

		public const string EventScreenMultiInputDataViewUploadFile = "EventScreenDocumentsDataViewUploadFile";
		public const string EventScreenMultiInputDataViewReloadBrowser = "EventScreenMultiInputDataViewReloadBrowser";
		
		[SerializeField] private TextMeshProUGUI titleScreen;

		[SerializeField] private CodeEditor inputCodeEditor;

		[SerializeField] private Button buttonHTML;
		[SerializeField] private Button buttonBrowser;

#if USE_VUPLEX
		[SerializeField] private CanvasWebViewPrefab webBrowser;
#endif

		[SerializeField] private TMP_Dropdown DropDownFileType;
		[SerializeField] private Button buttonUploadImage;
		[SerializeField] private Button buttonDownloadImage;
		[SerializeField] private Button buttonImageGen;
		[SerializeField] private Image contentImage;

		private bool _isImage = false;
		private int _imageId = -1;

		private TabsData _tabSelected = TabsData.HTML;
		
		private byte[] _imageBytes;

		private Vector2 _originalImageSize;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			buttonHTML.onClick.AddListener(OnViewEditor);
			buttonBrowser.onClick.AddListener(OnViewBrowser);

			DropDownFileType.ClearOptions();
			DropDownFileType.options.Add(new TMP_Dropdown.OptionData(FileDocType.Text.ToString()));
			DropDownFileType.options.Add(new TMP_Dropdown.OptionData(FileDocType.Image.ToString()));
			DropDownFileType.value = 0;
			DropDownFileType.onValueChanged.AddListener(OnFileTypeChanged);

			buttonUploadImage.onClick.AddListener(OnUploadImage);
			buttonDownloadImage.onClick.AddListener(OnDownloadImage);
			buttonImageGen.onClick.AddListener(OnImageGeneration);
			buttonImageGen.gameObject.SetActive(false);

			_originalImageSize = contentImage.GetComponent<RectTransform>().sizeDelta;

			SystemEventController.Instance.Event += OnSystemEvent;
		}

        public override void Destroy()
		{
			base.Destroy();

			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
		}

		protected override void OnCancel()
		{
			base.OnCancel();

			if (_imageId != -1)
			{
				int imageId = _imageId;
				_imageId = -1;
				WorkDayData.Instance.DeleteImage(imageId.ToString());
			}
		}

		private void OnImageGeneration()
		{
			GameObject screen = ScreenController.Instance.CreateScreen(ScreenImageGenerationView.ScreenName, false, false);
			UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, screen, _canvas.sortingOrder + 1);
		}

		private void OnFileTypeChanged(int value)
		{
			bool wasImage = _isImage;
			_isImage = (FileDocType.Image == (FileDocType)value);
			if (!_isImage && wasImage)
            {
				if (_imageId != -1)
                {
					WorkDayData.Instance.DeleteImage(_imageId.ToString());
					_inputValue.text = "";
				}
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

				contentImage.GetComponent<RectTransform>().sizeDelta = _originalImageSize;
				contentImage.overrideSprite = null;

				if (_imageId != -1)
                {
					SystemEventController.Instance.DispatchSystemEvent(ImageDatabaseController.EventImageDatabaseControllerDownloadImage, _imageId, true);
				}
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
						break;
					case TabsData.CSS:
						inputCodeEditor.gameObject.SetActive(true);
#if USE_VUPLEX
						webBrowser.gameObject.SetActive(false);
#endif
						buttonHTML.interactable = true;
						buttonBrowser.interactable = true;
						break;
					case TabsData.BROWSER:
						inputCodeEditor.gameObject.SetActive(false);
#if USE_VUPLEX
						webBrowser.gameObject.SetActive(true);
#endif
						buttonHTML.interactable = true;
						buttonBrowser.interactable = false;
						break;
				}
				inputCodeEditor.Refresh(true);
			}
		}

        private void OnDownloadImage()
        {
			if (_imageId != -1)
			{
				Application.OpenURL(DownloadImageDataHTTP.GetURLToDownload(_imageId));
			}
        }

        private void OnUploadImage()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WebFileBrowser.Upload(OnWebBrowserUploadImage, "images/*");
#else
			ApplicationController.Instance.CreateFileBrowser(LanguageController.Instance.GetText("screen.image.creating.select.file"), EventScreenMultiInputDataViewUploadFile, "png", "jpg", "jpeg");
#endif
		}

		private void OnWebBrowserUploadImage(string fileName, string mime, byte[] bytes)
		{
			SystemEventController.Instance.DispatchSystemEvent(EventScreenMultiInputDataViewUploadFile, true, bytes);
		}

		private void OnViewEditor()
		{
			_tabSelected = TabsData.HTML;
			UpdateTabVisibility();
		}

		private void OnViewBrowser()
		{
			_tabSelected = TabsData.BROWSER;
			UpdateTabVisibility();
			SystemEventController.Instance.DelaySystemEvent(EventScreenMultiInputDataViewReloadBrowser, 0.1f);
		}

#if USE_VUPLEX
		private async void ReloadBrowser()
        {
			await webBrowser.WaitUntilInitialized();
			webBrowser.WebView.LoadHtml(_inputValue.text);
		}
#endif
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

		public static int GetImageFromText(string text)
        {
			try
            {
				Match match = Regex.Match(text, @"<image_id>(\d+)</image_id>");

				if (match.Success)
				{
					return int.Parse(match.Groups[1].Value);
				}
				else
				{
					return -1;
				}
			}
			catch (Exception err)
            {
				return -1;
            }
		}

		private void UploadImageData()
        {
			if ((_imageBytes != null) && (_imageBytes.Length > 0))
            {
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, this.gameObject, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("screen.image.creating.uploading.image"));
				UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, ScreenInformationView.ScreenLoadingImage, _canvas.sortingOrder + 1);
				string nameImage = "Image_" + UnityEngine.Random.Range(1000, 10000);
				WorkDayData.Instance.UploadImageData(_imageId, WorkDayData.Instance.CurrentProject.ProjectInfoSelected, nameImage, _imageBytes);
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
					UploadImageData();
				}
				else
				{
					ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, this.gameObject, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.image.creating.no.image.created"));
					UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, ScreenInformationView.ScreenInformation, _canvas.sortingOrder + 1);
				}
			}
			if (nameEvent.Equals(EventScreenMultiInputDataViewReloadBrowser))
            {
#if USE_VUPLEX
				ReloadBrowser();
#endif
			}
			if (nameEvent.Equals(ImageDatabaseController.EventImageDatabaseControllerAvailableImage))
            {
				if ((bool)parameters[1])
                {
					_imageId = (int)parameters[0];					
					ImageUtils.LoadBytesSpriteResize(_originalImageSize, contentImage, ImageDatabaseController.Instance.GetImageDataByID(_imageId));
				}
            }
			if (nameEvent.Equals(EventScreenMultiInputDataViewUploadFile))
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
					UploadImageData();
				}
				else
				{
					ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, this.gameObject, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.image.creating.no.image.selected"));
				}
			}
			if (nameEvent.Equals(UploadImageDataHTTP.EventUploadImageDataHTTPCompleted))
			{
				UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationByNameDestroy, ScreenInformationView.ScreenLoadingImage);
				if ((bool)parameters[0])
				{
					_imageId = (int)parameters[1];
					_inputValue.text = IMAGE_START_TAG + _imageId + IMAGE_END_TAG;
					buttonUploadImage.interactable = false;
					buttonImageGen.interactable = false;					
				}
			}
		}

		protected override void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventScreenInformationRequestAllScreensDestroyed))
			{
				return;
			}

			base.OnUIEvent(nameEvent, parameters);

			if (nameEvent.Equals(EventScreenInformationSetInputText))
			{
				if (_inputValue != null)
				{
					_imageId = GetImageFromText(_inputValue.text);
					if (_imageId != -1)
                    {
						DropDownFileType.value = 1;
					}
					else
                    {
						DropDownFileType.value = 0;
						inputCodeEditor.Refresh(true);
					}
					UpdateTabVisibility();
				}
			}
		}
	}
}