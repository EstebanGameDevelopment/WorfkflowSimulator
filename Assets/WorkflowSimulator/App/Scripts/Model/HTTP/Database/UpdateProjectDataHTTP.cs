#if ENABLE_OFUSCATION
#if ENABLE_NEW_OFUSCATION
using GUPS.Obfuscator.Attribute;
#else
using OPS.Obfuscator.Attribute;
#endif
#endif
using System;
using System.Text;
using UnityEngine;
using yourvrexperience.UserManagement;
using yourvrexperience.Utils;
using static yourvrexperience.Utils.AESEncryption;

namespace yourvrexperience.WorkDay
{
#if ENABLE_OFUSCATION
	[DoNotRenameAttribute]
#endif
	public class UpdateProjectDataHTTP : BaseDataHTTP, IHTTPComms
	{
		public const string EventUpdateProjectDataHTTPCompleted = "EventUpdateProjectDataHTTPCompleted";

		private string _urlRequest = "";

		public string UrlRequest
		{
			get
			{
				if (_urlRequest.Length == 0)
				{
					_urlRequest = WorkDayData.Instance.URLBase + "WorkDayUploadDataProject.php";
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

			byte[] jsonData = Encoding.UTF8.GetBytes((string)_list[1]);
			byte[] compressedData = CompressionUtils.CompressWithBrotli(jsonData);
			_formPost.AddBinaryData("data", compressedData);

			return null;
		}

		public override void Response(string _response)
		{
			if (!ResponseCode(_response))
			{
				SystemEventController.Instance.DelaySystemEvent(EventUpdateProjectDataHTTPCompleted, 1, false);
				return;
			}

			bool success = false;
			if (!bool.TryParse(_jsonResponse, out success))
			{
				SystemEventController.Instance.DelaySystemEvent(EventUpdateProjectDataHTTPCompleted, 1, true);
			}
			else
			{
				SystemEventController.Instance.DelaySystemEvent(EventUpdateProjectDataHTTPCompleted, 1, success);
			}
		}
	}
}