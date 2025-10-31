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
	public class UploadImageDataHTTP : BaseDataHTTP, IHTTPComms
	{
		public const string EventUploadImageDataHTTPCompleted = "EventUploadImageDataHTTPCompleted";

		private string _urlRequest = "";

		public string UrlRequest
		{
			get
			{
				if (_urlRequest.Length == 0)
				{
					_urlRequest = WorkDayData.Instance.URLBase + "WorkDayUploadImageData.php";
				}
				return _urlRequest;
			}
		}

		public string Build(params object[] _list)
		{
			_method = METHOD_POST;

			_formPost = new WWWForm();

			_formPost.AddField("user", UsersController.Instance.CurrentUser.Id.ToString());
			_formPost.AddField("password", UsersController.Instance.CurrentUser.Password);
			_formPost.AddField("salt", UsersController.Instance.CurrentUser.Salt);

			_formPost.AddField("id", (int)_list[0]);
			_formPost.AddField("project", (int)_list[1]);
			_formPost.AddField("name", (string)_list[2]);
			
			byte[] imageData = (byte[])_list[3];
			_formPost.AddField("size", imageData.Length);
			_formPost.AddBinaryData("data", imageData);

			return null;
		}

		public override void Response(string _response)
		{
			if (!ResponseCode(_response))
			{
				SystemEventController.Instance.DispatchSystemEvent(EventUploadImageDataHTTPCompleted, false);
				return;
			}

			string[] data = _jsonResponse.Split(new string[] { CommController.TOKEN_SEPARATOR_EVENTS }, StringSplitOptions.None);
			bool success = false;
			if (!bool.TryParse(data[0], out success))
			{
				SystemEventController.Instance.DelaySystemEvent(EventUploadImageDataHTTPCompleted, 1, false);
			}
			else
			{
				if (success)
				{
                    SystemEventController.Instance.DelaySystemEvent(EventUploadImageDataHTTPCompleted, 1, true, int.Parse(data[1]));
				}
				else
				{
					SystemEventController.Instance.DelaySystemEvent(EventUploadImageDataHTTPCompleted, 1, false);
				}
			}
		}
	}
}