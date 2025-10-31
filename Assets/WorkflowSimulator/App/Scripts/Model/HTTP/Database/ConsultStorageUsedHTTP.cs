#if ENABLE_OFUSCATION
#if ENABLE_NEW_OFUSCATION
using GUPS.Obfuscator.Attribute;
#else
using OPS.Obfuscator.Attribute;
#endif
#endif
using System;
using UnityEngine;
using yourvrexperience.UserManagement;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
#if ENABLE_OFUSCATION
	[DoNotRenameAttribute]
#endif
	public class ConsultStorageUsedHTTP : BaseDataHTTP, IHTTPComms
	{
		public const string EventConsultStorageUsedHTTPCompleted = "EventConsultStorageUsedHTTPCompleted";

		private string _urlRequest = "";

		private int _idProject = -1;
		private int _level = -1;

		public string UrlRequest
		{
			get
			{
				if (_urlRequest.Length == 0)
				{
					_urlRequest = WorkDayData.Instance.URLBase + "WorkDayConsultStorageUsed.php";
				}
				return _urlRequest;
			}
		}

		public string Build(params object[] _list)
		{
			_idProject = (int)_list[0];
			_level = (int)_list[1];
			string callParams = "?id=" + UsersController.Instance.CurrentUser.Id + "&password=" + UsersController.Instance.CurrentUser.Password + "&salt=" + UsersController.Instance.CurrentUser.Salt + "&project=" + _idProject;
			return callParams;
		}

		public override void Response(string _response)
		{
			if (!ResponseCode(_response))
			{
				SystemEventController.Instance.DispatchSystemEvent(EventConsultStorageUsedHTTPCompleted, false);
				return;
			}

			string[] data = _jsonResponse.Split(new string[] { CommController.TOKEN_SEPARATOR_EVENTS }, StringSplitOptions.None);
			bool success = false;
			if (!bool.TryParse(data[0], out success))
			{
				SystemEventController.Instance.DispatchSystemEvent(EventConsultStorageUsedHTTPCompleted, false);
			}
			else
			{
				StorageUsed Storage = new StorageUsed();
				Storage.Data = int.Parse(data[1]);
				Storage.Images = int.Parse(data[2]);
#if UNITY_EDITOR
                Debug.Log("Storage.Data = " + Storage.Data);
				Debug.Log("Storage.Images = " + Storage.Images);
#endif
				Storage.Calculate(_level);
				SystemEventController.Instance.DispatchSystemEvent(EventConsultStorageUsedHTTPCompleted, true, Storage);
			}
		}
	}
}