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
	public class ScreenImageView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenImageView";

		public const string EventScreenImageViewClosed = "EventScreenImageViewClosed";

		[SerializeField] private Button closeWindow;
		[SerializeField] private Image contentImage;

		private Vector2 _originalImageSize;
		private int _idImage;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			_idImage = (int)parameters[0];

			closeWindow.onClick.AddListener(OnCloseWindow);

			_originalImageSize = contentImage.GetComponent<RectTransform>().sizeDelta;

			SystemEventController.Instance.DispatchSystemEvent(ImageDatabaseController.EventImageDatabaseControllerDownloadImage, _idImage, true);
			SystemEventController.Instance.Event += OnSystemEvent;
		}

		public override void Destroy()
		{
			base.Destroy();

			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;

			SystemEventController.Instance.DispatchSystemEvent(EventScreenImageViewClosed, _idImage);
		}

        private void OnCloseWindow()
        {
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ImageDatabaseController.EventImageDatabaseControllerAvailableImage))
			{
				if ((bool)parameters[1])
				{
					int idImage = (int)parameters[0];
					ImageUtils.LoadBytesSpriteResize(_originalImageSize, contentImage, ImageDatabaseController.Instance.GetImageDataByID(idImage));
				}
			}
		}

	}
}