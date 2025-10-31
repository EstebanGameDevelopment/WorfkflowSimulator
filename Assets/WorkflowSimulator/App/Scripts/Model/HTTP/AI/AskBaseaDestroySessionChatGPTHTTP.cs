#if ENABLE_OFUSCATION
#if ENABLE_NEW_OFUSCATION
using GUPS.Obfuscator.Attribute;
#else
using OPS.Obfuscator.Attribute;
#endif
#endif
using Newtonsoft.Json;
using System;
using UnityEngine;
using yourvrexperience.UserManagement;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
#if ENABLE_OFUSCATION
	[DoNotRenameAttribute]
#endif
	public class AskBaseaDestroySessionChatGPTHTTP : BaseDataHTTP, IHTTPComms
	{
		public const string EventAskBaseaDestroySessionChatGPTHTTPCompleted = "EventAskBaseaDestroySessionChatGPTHTTPCompleted";

		[Serializable]
		public class DestroySessionResponseJSON : IJsonValidatable
		{
			public string session_name;

			public bool IsValid()
			{
				return !string.IsNullOrEmpty(session_name);
			}
		}

		public class DestroySession
		{
			[JsonProperty(PropertyName = "session_name")]
			public string SessionName { get; set; }

			[JsonProperty(PropertyName = "salt")]
			public string Salt { get; set; }
			[JsonProperty(PropertyName = "user_hash")]
			public string UserHash { get; set; }
			[JsonProperty(PropertyName = "timestamp")]
			public string Timestamp { get; set; }
		}

		private string _customEvent;

		public string UrlRequest
		{
			get
			{
#if ENABLE_REMOTE_CORS_SERVER
				return WorkDayData.Instance.GetServerScreenSession() + "/" + "destroy_session";
#else
				return WorkDayData.Instance.serverScreenSession + ":" + WorkDayData.PORT_SESSION_SERVER + "/" + "destroy_session";
#endif
			}
		}

		public string Build(params object[] _list)
		{
			_method = METHOD_POST;
			_formPost = null;

			_customEvent = (string)_list[0];

			string randomSalt = SHAEncryption.GenerateSalt();
			long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			string combinedSalt = SHAEncryption.GenerateSaltWithTimestamp(randomSalt, timestamp);
			string hashedUserId = SHAEncryption.HashWithSalt(UsersController.Instance.CurrentUser.Id.ToString(), combinedSalt);

			_rawData = System.Text.Encoding.UTF8.GetBytes(
					JsonConvert.SerializeObject(new DestroySession
					{
						SessionName = UsersController.Instance.CurrentUser.Id.ToString(),
						Salt = randomSalt,
						UserHash = hashedUserId,
						Timestamp = timestamp.ToString()
					}));

			return null;
		}

		public override void Response(string _response)
		{
			if (_cancelResponse) return;

			if (!ResponseCode(_response))
			{
				if (_customEvent.Length > 0)
				{
					SystemEventController.Instance.DispatchSystemEvent(_customEvent, false);
				}
				else
				{
					SystemEventController.Instance.DispatchSystemEvent(EventAskBaseaDestroySessionChatGPTHTTPCompleted, false);
				}
				return;
			}

			if (!JSONDataFormatValidator.ValidateJsonItem<DestroySessionResponseJSON>(_response))
			{
				if (_customEvent.Length > 0)
				{
					SystemEventController.Instance.DispatchSystemEvent(_customEvent, false);
				}
				else
				{
					SystemEventController.Instance.DispatchSystemEvent(EventAskBaseaDestroySessionChatGPTHTTPCompleted, false);
				}
			}
			else
			{
				DestroySessionResponseJSON destroyResponse = JsonUtility.FromJson<DestroySessionResponseJSON>(_response);
				if (_customEvent.Length > 0)
				{
					SystemEventController.Instance.DispatchSystemEvent(_customEvent, true, destroyResponse);
				}
				else
				{
					SystemEventController.Instance.DispatchSystemEvent(EventAskBaseaDestroySessionChatGPTHTTPCompleted, true, destroyResponse);
				}
			}
		}
	}
}