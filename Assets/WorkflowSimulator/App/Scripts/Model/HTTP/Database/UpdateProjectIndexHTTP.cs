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
	public class UpdateProjectIndexHTTP : BaseDataHTTP, IHTTPComms
	{
		public const string EventUpdateProjectIndexHTTPCompleted = "EventUpdateProjectIndexHTTPCompleted";

		private string _urlRequest = "";

		public string UrlRequest
		{
			get
			{
				if (_urlRequest.Length == 0)
				{
					_urlRequest = WorkDayData.Instance.URLBase + "WorkDayUploadIndexProject.php";
				}
				return _urlRequest;
			}
		}

		public string Build(params object[] _list)
		{
			_method = METHOD_POST;

			_formPost = new WWWForm();

			_formPost.AddField("usr", UsersController.Instance.CurrentUser.Id.ToString());
			_formPost.AddField("pwd", UsersController.Instance.CurrentUser.Password);
			_formPost.AddField("salt", UsersController.Instance.CurrentUser.Salt);

			_formPost.AddField("id", (int)_list[0]);
			_formPost.AddField("user", (int)_list[1]);
			_formPost.AddField("dataid", (int)_list[2]);
			_formPost.AddField("title", (string)_list[3]);
			_formPost.AddField("description", (string)_list[4]);
			_formPost.AddField("category1", (int)_list[5]);
			_formPost.AddField("category2", (int)_list[6]);
			_formPost.AddField("category3", (int)_list[7]);
			_formPost.AddField("time", 10000);

			return null;
		}

		public override void Response(string _response)
		{
			if (!ResponseCode(_response))
			{
				SystemEventController.Instance.DispatchSystemEvent(EventUpdateProjectIndexHTTPCompleted, false);
				return;
			}

			string[] data = _jsonResponse.Split(new string[] { CommController.TOKEN_SEPARATOR_EVENTS }, StringSplitOptions.None);
			bool success = false;
			if (!bool.TryParse(data[0], out success))
			{
				SystemEventController.Instance.DispatchSystemEvent(EventUpdateProjectIndexHTTPCompleted, false);
			}
			else
			{
				if (success)
				{
					SystemEventController.Instance.DispatchSystemEvent(EventUpdateProjectIndexHTTPCompleted, true, int.Parse(data[1]), int.Parse(data[2]));
				}
				else
				{
					SystemEventController.Instance.DispatchSystemEvent(EventUpdateProjectIndexHTTPCompleted, false);
				}
			}
		}
	}
}