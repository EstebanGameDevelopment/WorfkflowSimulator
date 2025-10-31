#if ENABLE_OFUSCATION
#if ENABLE_NEW_OFUSCATION
using GUPS.Obfuscator.Attribute;
#else
using OPS.Obfuscator.Attribute;
# endif
# endif
using UnityEngine;
using yourvrexperience.UserManagement;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
#if ENABLE_OFUSCATION
	[DoNotRenameAttribute]
#endif
	public class UpdatePurchaseSlotHTTP : BaseDataHTTP, IHTTPComms
	{
		public const string EventUpdatePurchaseSlotHTTPCompleted = "EventUpdatePurchaseSlotHTTPCompleted";

		private string _urlRequest = "";

		public string UrlRequest
		{
			get
			{
				if (_urlRequest.Length == 0)
				{
					_urlRequest = WorkDayData.Instance.URLBase + "WorkDayPurchaseSlot.php";
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

			_formPost.AddField("slot", (int)_list[0]);
			_formPost.AddField("level", (int)_list[1]);
			_formPost.AddField("timeout", ((long)_list[2]).ToString());
			_formPost.AddField("data", (string)_list[3]);

			return null;
		}

		public override void Response(string _response)
		{
			if (!ResponseCode(_response))
			{
				SystemEventController.Instance.DispatchSystemEvent(EventUpdatePurchaseSlotHTTPCompleted, false);
				return;
			}

			bool success = false;
			if (!bool.TryParse(_jsonResponse, out success))
			{
				SystemEventController.Instance.DispatchSystemEvent(EventUpdatePurchaseSlotHTTPCompleted, false);
			}
			else
			{
				if (success)
				{
					SystemEventController.Instance.DispatchSystemEvent(EventUpdatePurchaseSlotHTTPCompleted, true);
				}
				else
				{
					SystemEventController.Instance.DispatchSystemEvent(EventUpdatePurchaseSlotHTTPCompleted, false);
				}
			}
		}
	}
}