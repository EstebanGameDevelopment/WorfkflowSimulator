#if ENABLE_OFUSCATION
#if ENABLE_NEW_OFUSCATION
using GUPS.Obfuscator.Attribute;
#else
using OPS.Obfuscator.Attribute;
#endif
#endif
using System;
using yourvrexperience.UserManagement;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
#if ENABLE_OFUSCATION
	[DoNotRenameAttribute]
#endif
	public class DownloadImageDataHTTP : BaseDataHTTP, IHTTPComms
	{
		public const string EventDownloadImageDataHTTPCompleted = "EventDownloadImageDataHTTPCompleted";

		private string _urlRequest = "";

		private int _imageID = -1;
		private bool _shouldReport;

		public string UrlRequest
		{
			get
			{
				if (_urlRequest.Length == 0)
				{
					_urlRequest = WorkDayData.Instance.URLBase + "WorkDayDownloadImageData.php";
				}
				return _urlRequest;
			}
		}

		public static string GetURLToDownload(int id)
		{
			return WorkDayData.Instance.URLBase + "WorkDayDownloadImageData.php?id=" + id + "&direct=" + 1 + "&user=" + UsersController.Instance.CurrentUser.Id;
		}

		public string Build(params object[] _list)
		{
			_imageID = (int)_list[0];
			_shouldReport = (bool)_list[1];
			string callParams = "?id=" + _imageID + "&direct=" + 0 + "&user=" + UsersController.Instance.CurrentUser.Id;
			return callParams;
		}

		public override void Response(byte[] _response)
		{
			if (!ResponseCode(_response))
			{
				SystemEventController.Instance.DispatchSystemEvent(EventDownloadImageDataHTTPCompleted, false, _imageID);
				return;
			}

			string[] data = _jsonResponse.Split(new string[] { CommController.TOKEN_SEPARATOR_EVENTS }, StringSplitOptions.None);
			if (bool.Parse(data[0]))
			{
				string nameImage = (string)data[1];
				int sizeData = int.Parse(data[2]);
				int startingPos = _response.Length - sizeData;
				byte[] dataImage = new byte[sizeData];
				Array.Copy(_response, startingPos, dataImage, 0, sizeData);
				SystemEventController.Instance.DispatchSystemEvent(EventDownloadImageDataHTTPCompleted, true, _imageID, _shouldReport, nameImage, dataImage);
			}
			else
			{
				SystemEventController.Instance.DispatchSystemEvent(EventDownloadImageDataHTTPCompleted, false, _imageID);
			}
		}
	}
}