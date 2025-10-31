#if ENABLE_OFUSCATION
#if ENABLE_NEW_OFUSCATION
using GUPS.Obfuscator.Attribute;
using Mirror.BouncyCastle.Asn1.Cms;

#else
using OPS.Obfuscator.Attribute;
#endif
#endif
using UnityEngine;
using yourvrexperience.UserManagement;
using yourvrexperience.Utils;
using static yourvrexperience.Utils.AESEncryption;

namespace yourvrexperience.WorkDay
{
#if ENABLE_OFUSCATION
	[DoNotRenameAttribute]
#endif
	public class UpdateAnalysisDataHTTP : BaseDataHTTP, IHTTPComms
	{
		public const string EventUpdateAnalysisDataHTTPCompleted = "EventUpdateAnalysisDataHTTPCompleted";

		private string _urlRequest = "";

		public string UrlRequest
		{
			get
			{
				if (_urlRequest.Length == 0)
				{
					_urlRequest = WorkDayData.Instance.URLBase + "WorkDayUploadDataAnalysis.php";
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

			_formPost.AddField("candidate", (string)_list[0]);
			_formPost.AddField("analysis", (string)_list[1]);

            return null;
		}

		public override void Response(string _response)
		{
			if (!ResponseCode(_response))
			{
				SystemEventController.Instance.DelaySystemEvent(EventUpdateAnalysisDataHTTPCompleted, 1, false);
				return;
			}

			bool success = false;
			if (!bool.TryParse(_jsonResponse, out success))
			{
				SystemEventController.Instance.DelaySystemEvent(EventUpdateAnalysisDataHTTPCompleted, 1, true);
			}
			else
			{
				SystemEventController.Instance.DelaySystemEvent(EventUpdateAnalysisDataHTTPCompleted, 1, success);
			}
		}
	}
}