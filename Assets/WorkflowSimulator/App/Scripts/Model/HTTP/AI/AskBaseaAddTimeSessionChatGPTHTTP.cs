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
	public class AskBaseaAddTimeSessionChatGPTHTTP : BaseDataHTTP, IHTTPComms
	{
		public const string EventAskBaseaAddTimeSessionChatGPTHTTPCompleted = "EventAskBaseaAddTimeSessionChatGPTHTTPCompleted";

		[Serializable]
		public class AddTimeSessionResponseJSON : IJsonValidatable
		{
			public string session_name;
			public int start_time;

			public bool IsValid()
			{
				bool isNameValid = !string.IsNullOrEmpty(session_name);
				bool isTimeValid = start_time != -1;

				return isNameValid && isTimeValid;
			}
		}

		public class AddTimeSession
		{
			[JsonProperty(PropertyName = "session_name")]
			public string SessionName { get; set; }

			[JsonProperty(PropertyName = "additional_seconds")]
			public int TimeoutSeconds { get; set; }

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
				return WorkDayData.Instance.GetServerScreenSession() + "/" + "refresh_time_session";
#else
				return WorkDayData.Instance.serverScreenSession + ":" + WorkDayData.PORT_SESSION_SERVER + "/" + "refresh_time_session";
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
					JsonConvert.SerializeObject(new AddTimeSession
					{
						SessionName = UsersController.Instance.CurrentUser.Id.ToString(),
						TimeoutSeconds = WorkDayData.TOTAL_TIME_SCREEN_SESSION,
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
					SystemEventController.Instance.DispatchSystemEvent(EventAskBaseaAddTimeSessionChatGPTHTTPCompleted, false);
				}
				return;
			}

			if (!JSONDataFormatValidator.ValidateJsonItem<AddTimeSessionResponseJSON>(_response))
			{
				if (_customEvent.Length > 0)
				{
					SystemEventController.Instance.DispatchSystemEvent(_customEvent, false);
				}
				else
				{
					SystemEventController.Instance.DispatchSystemEvent(EventAskBaseaAddTimeSessionChatGPTHTTPCompleted, false);
				}
			}
			else
			{
				AddTimeSessionResponseJSON addTimeResponse = JsonUtility.FromJson<AddTimeSessionResponseJSON>(_response);
				if (_customEvent.Length > 0)
				{
					SystemEventController.Instance.DispatchSystemEvent(_customEvent, true, addTimeResponse);
				}
				else
				{
					SystemEventController.Instance.DispatchSystemEvent(EventAskBaseaAddTimeSessionChatGPTHTTPCompleted, true, addTimeResponse);
				}
			}
		}
	}
}