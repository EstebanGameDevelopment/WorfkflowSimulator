using yourvrexperience.Utils;
using UnityEngine;
using System.Collections.Generic;
using yourvrexperience.ai;

namespace yourvrexperience.WorkDay
{
	public class ImageDatabaseController : MonoBehaviour
	{
		public class ImageInfo
		{
			public string Name;
			public byte[] Data;

			public ImageInfo(string name, byte[] data)
			{
				Name = name;
				Data = data;
			}
		}

		public const string TagFilenameImage = "image_";

		public const string EventImageDatabaseControllerDownloadImage = "EventImageDatabaseControllerDownloadImage";
		public const string EventImageDatabaseControllerAvailableImage = "EventImageDatabaseControllerAvailableImage";
		public const string EventImageDatabaseControllerGenerateImage = "EventImageDatabaseControllerGenerateImage";
		
		public const int TOTAL_ALLOWED_IMAGES = 50;

		private static ImageDatabaseController _instance;

		public static ImageDatabaseController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(ImageDatabaseController)) as ImageDatabaseController;
				}
				return _instance;
			}
		}

		private List<int> _imagesIDs = new List<int>();
		private Dictionary<int, ImageInfo> _imagesData = new Dictionary<int, ImageInfo>();
		private List<int> _imagesToProcess = new List<int>();

		public void Initialize()
		{
			SystemEventController.Instance.Event += OnSystemEvent;
		}

		void OnDestroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
		}

		public void ClearAll()
		{
			_imagesIDs.Clear();
			_imagesData.Clear();
			_imagesToProcess.Clear();
		}

		public byte[] GetImageDataByID(int id)
		{
			ImageInfo imageInfo = null;
			if (_imagesData.TryGetValue(id, out imageInfo))
			{
				return imageInfo.Data;
			}
			else
			{
				return null;
			}
		}

		public string GetImageNameByID(int id)
		{
			ImageInfo imageInfo = null;
			if (_imagesData.TryGetValue(id, out imageInfo))
			{
				return imageInfo.Name;
			}
			else
			{
				return null;
			}
		}

		public void AddImageData(int id, string name, byte[] image)
		{
			if (_imagesData.ContainsKey(id))
			{
				_imagesData.Remove(id);
			}
			_imagesData.Add(id, new ImageInfo(name, image));
			if (_imagesIDs.Contains(id))
			{
				_imagesIDs.Remove(id);
			}
			_imagesIDs.Add(id);
			if (_imagesIDs.Count > TOTAL_ALLOWED_IMAGES)
			{
				int idToRemove = _imagesIDs[0];
				_imagesData.Remove(idToRemove);
				_imagesIDs.RemoveAt(0);
			}
		}

		public void UploadImageData(int idImage, string nameImage, byte[] bytesImage, int sortingOrder)
		{
			if ((bytesImage != null) && (bytesImage.Length > 0))
			{
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoadingImage, this.gameObject, LanguageController.Instance.GetText("text.info"), LanguageController.Instance.GetText("screen.image.creating.uploading.image"));
				if (sortingOrder != -1)
                {
					UIEventController.Instance.DispatchUIEvent(BaseScreenView.EventBaseScreenViewSetCanvasOrder, ScreenInformationView.ScreenLoadingImage, sortingOrder);
				}
				WorkDayData.Instance.UploadImageData(idImage, WorkDayData.Instance.CurrentProject.ProjectInfoSelected, nameImage, bytesImage);
			}
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventImageDatabaseControllerGenerateImage))
			{
				int provider = (int)parameters[0];
				int cyclesQuality = (int)((((float)parameters[1]) * 50) + 25);
				if (provider == 0)
				{
					cyclesQuality = (int)(((float)parameters[1]) * 8);
				}
				int widthImage = (int)parameters[2];
				int heightImage = (int)parameters[3];
				string instructions = (string)parameters[4];
				string customEvent = (string)parameters[5];

				GameAIData.Instance.AskGenericImageAI(provider, instructions, "", cyclesQuality, widthImage, heightImage, customEvent);
			}
			if (nameEvent.Equals(DeleteImageDataHTTP.EventDeleteImageDataHTTPCompleted))
			{
				if ((bool)parameters[0])
				{
					string dataIDs = (string)parameters[1];
					string[] arrayIDs = dataIDs.Split(";");
					for (int i = 0; i < arrayIDs.Length; i++)
					{
						int idToDelete = -1;
						if (int.TryParse(arrayIDs[i], out idToDelete))
						{
							_imagesData.Remove(idToDelete);
						}
					}
				}
			}
			if (nameEvent.Equals(EventImageDatabaseControllerDownloadImage))
			{
				int idImage = (int)parameters[0];
				bool shouldReport = true;
				if (parameters.Length > 1)
				{
					shouldReport = (bool)parameters[1];
				}
				byte[] data = GetImageDataByID(idImage);
				if (data == null)
				{
					if (!_imagesToProcess.Contains(idImage))
					{
						_imagesToProcess.Add(idImage);

						WorkDayData.Instance.DownloadImageData(idImage, shouldReport);
					}
				}
				else
				{
					if (shouldReport)
					{
						SystemEventController.Instance.DispatchSystemEvent(EventImageDatabaseControllerAvailableImage, idImage, true);
					}
				}
			}
			if (nameEvent.Equals(DownloadImageDataHTTP.EventDownloadImageDataHTTPCompleted))
			{
				bool success = (bool)parameters[0];
				int idImage = (int)parameters[1];
				if (_imagesToProcess.Remove(idImage))
				{
#if UNITY_EDITOR
                    Debug.Log(" -- REMOVED IMAGE[" + idImage + "] OF TOTAL=" + _imagesToProcess.Count);
#endif
				}
				if (success)
				{
					bool shouldReport = (bool)parameters[2];
					string nameImage = (string)parameters[3];
					byte[] dataImage = (byte[])parameters[4];
					AddImageData(idImage, nameImage, dataImage);
					if (shouldReport)
					{
						SystemEventController.Instance.DispatchSystemEvent(EventImageDatabaseControllerAvailableImage, idImage, true);
					}
				}
				else
				{
					SystemEventController.Instance.DispatchSystemEvent(EventImageDatabaseControllerAvailableImage, idImage, false);
				}
			}
			if (nameEvent.Equals(ApplicationController.EventMainControllerReleaseGameResources))
			{
				ClearAll();
			}
			if (nameEvent.Equals(SystemEventController.EventSystemEventControllerReleaseAllResources))
			{
				_instance = null;
				ClearAll();
				GameObject.Destroy(this.gameObject);
			}
			if (nameEvent.Equals(SystemEventController.EventSystemEventControllerDontDestroyOnLoad))
			{
				if (Instance)
				{
					DontDestroyOnLoad(Instance.gameObject);
				}
			}
		}
	}
}