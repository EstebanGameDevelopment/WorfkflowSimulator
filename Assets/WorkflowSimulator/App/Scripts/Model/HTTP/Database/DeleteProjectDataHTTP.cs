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
	public class DeleteProjectDataHTTP : BaseDataHTTP, IHTTPComms
	{
		public const string EventDeleteProjectDataHTTPCompleted = "EventDeleteProjectDataHTTPCompleted";

		private string _urlRequest = "";
		private int _projectID = -1;

		public string UrlRequest
		{
			get
			{
				if (_urlRequest.Length == 0)
				{
					_urlRequest = WorkDayData.Instance.URLBase + "WorkDayDeleteProjectData.php";
				}
				return _urlRequest;
			}
		}

		public string Build(params object[] _list)
		{
			_method = METHOD_POST;

			_projectID = (int)_list[0];

			_formPost = new WWWForm();

			_formPost.AddField("id", UsersController.Instance.CurrentUser.Id.ToString());
			_formPost.AddField("password", UsersController.Instance.CurrentUser.Password);
			_formPost.AddField("salt", UsersController.Instance.CurrentUser.Salt);

			_formPost.AddField("project", _projectID);

			return null;
		}

		public override void Response(string _response)
		{
			if (!ResponseCode(_response))
			{
				SystemEventController.Instance.DispatchSystemEvent(EventDeleteProjectDataHTTPCompleted, false);
				return;
			}

			bool success = false;
			if (!bool.TryParse(_jsonResponse, out success))
			{
				SystemEventController.Instance.DispatchSystemEvent(EventDeleteProjectDataHTTPCompleted, false);
			}
			else
			{				
				SystemEventController.Instance.DelaySystemEvent(EventDeleteProjectDataHTTPCompleted, 1, true, _projectID);
			}
		}
	}
}