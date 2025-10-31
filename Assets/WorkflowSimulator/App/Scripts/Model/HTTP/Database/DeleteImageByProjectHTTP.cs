#if ENABLE_OFUSCATION
#if ENABLE_NEW_OFUSCATION
using GUPS.Obfuscator.Attribute;
#else
using OPS.Obfuscator.Attribute;
#endif
#endif
using UnityEngine;
using yourvrexperience.UserManagement;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
#if ENABLE_OFUSCATION
	[DoNotRenameAttribute]
#endif
	public class DeleteImageByProjectHTTP : BaseDataHTTP, IHTTPComms
	{		
		public const string EventDeleteImageByProjectHTTPCompleted = "EventDeleteImageByProjectHTTPCompleted";

		private string _urlRequest = "";

		public string UrlRequest
		{
			get
			{
				if (_urlRequest.Length == 0)
				{
					_urlRequest = WorkDayData.Instance.URLBase + "WorkDayDeleteImageByProject.php";
				}
				return _urlRequest;
			}
		}

		public string Build(params object[] _list)
		{
			_method = METHOD_POST;

			int projectID = (int)_list[0];

			_formPost = new WWWForm();

			_formPost.AddField("id", UsersController.Instance.CurrentUser.Id.ToString());
			_formPost.AddField("password", UsersController.Instance.CurrentUser.Password);
			_formPost.AddField("salt", UsersController.Instance.CurrentUser.Salt);

			_formPost.AddField("project", projectID);

			return null;
		}

		public override void Response(string _response)
		{
			if (!ResponseCode(_response))
			{
				SystemEventController.Instance.DispatchSystemEvent(EventDeleteImageByProjectHTTPCompleted, false);
				return;
			}

			bool success = false;
			if (!bool.TryParse(_jsonResponse, out success))
			{				
				SystemEventController.Instance.DelaySystemEvent(EventDeleteImageByProjectHTTPCompleted, 1, false);
			}
			else
			{
				SystemEventController.Instance.DelaySystemEvent(EventDeleteImageByProjectHTTPCompleted, 1, true);
			}
		}
	}
}