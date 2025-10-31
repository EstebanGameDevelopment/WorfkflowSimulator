#if ENABLE_OFUSCATION
#if ENABLE_NEW_OFUSCATION
using GUPS.Obfuscator.Attribute;
#else
using OPS.Obfuscator.Attribute;
#endif
#endif
using Newtonsoft.Json;
using System;
using System.Text;
using UnityEngine;
using yourvrexperience.UserManagement;
using yourvrexperience.Utils;
using static yourvrexperience.Utils.LanguageController;

namespace yourvrexperience.WorkDay
{
#if ENABLE_OFUSCATION
	[DoNotRenameAttribute]
#endif
	public class AskBaseaCreateSessionChatGPTHTTP : BaseDataHTTP, IHTTPComms
	{
		public const string EventAskBaseaCreateSessionChatGPTHTTPCompleted = "EventAskBaseaCreateSessionChatGPTHTTPCompleted";

		[Serializable]
		public class SessionResponseJSON : IJsonValidatable
		{
			public string session_name;
			public int port_number;

			public bool IsValid()
			{
				bool isNameValid = !string.IsNullOrEmpty(session_name);
				bool isPortValid = port_number != -1;

				return isNameValid && isPortValid;
			}
		}

		public class RequestNewSession
		{
			[JsonProperty(PropertyName = "session_name")]
			public string SessionName { get; set; }

			[JsonProperty(PropertyName = "script_path")]
			public string ScriptPath { get; set; }

			[JsonProperty(PropertyName = "timeout_seconds")]
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
				return WorkDayData.Instance.GetServerScreenSession() + "/" + "create_session";
#else
				return WorkDayData.Instance.serverScreenSession + ":" + WorkDayData.PORT_SESSION_SERVER + "/" + "create_session";
#endif
			}
		}

		public string Build(params object[] _list)
		{
			_method = METHOD_POST;
			_formPost = null;

			TranslationTypes languageSelected = LanguageController.Instance.GetTypeLanguageByCode((string)_list[0]);
			_customEvent = (string)_list[1];
			string scriptName = "ServerAIEnglish.py";
			switch (languageSelected)
			{
				case TranslationTypes.English:
					scriptName = "ServerAIEnglish.py";
					break;
				case TranslationTypes.Spanish:
					scriptName = "ServerAISpanish.py";
					break;
				case TranslationTypes.German:
					scriptName = "ServerAIGerman.py";
					break;
				case TranslationTypes.French:
					scriptName = "ServerAIFrench.py";
					break;
				case TranslationTypes.Italian:
					scriptName = "ServerAIItalian.py";
					break;
				case TranslationTypes.Russian:
					scriptName = "ServerAIRussian.py";
					break;
				case TranslationTypes.Catalan:
					scriptName = "ServerAICatalan.py";
					break;
			}

			string randomSalt = SHAEncryption.GenerateSalt();
			long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			string combinedSalt = SHAEncryption.GenerateSaltWithTimestamp(randomSalt, timestamp);
			string hashedUserId = SHAEncryption.HashWithSalt(UsersController.Instance.CurrentUser.Id.ToString(), combinedSalt);

			_rawData = System.Text.Encoding.UTF8.GetBytes(
					JsonConvert.SerializeObject(new RequestNewSession
					{
						SessionName = UsersController.Instance.CurrentUser.Id.ToString(),
						ScriptPath = scriptName,
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
					SystemEventController.Instance.DispatchSystemEvent(EventAskBaseaCreateSessionChatGPTHTTPCompleted, false);
				}
				return;
			}

			if (!JSONDataFormatValidator.ValidateJsonItem<SessionResponseJSON>(_response))
			{
				if (_customEvent.Length > 0)
				{
					SystemEventController.Instance.DispatchSystemEvent(_customEvent, false);
				}
				else
				{
					SystemEventController.Instance.DispatchSystemEvent(EventAskBaseaCreateSessionChatGPTHTTPCompleted, false);
				}
			}
			else
			{
				SessionResponseJSON sessionResponse = JsonUtility.FromJson<SessionResponseJSON>(_response);
				if (_customEvent.Length > 0)
				{
					SystemEventController.Instance.DispatchSystemEvent(_customEvent, true, sessionResponse);
				}
				else
				{
					SystemEventController.Instance.DispatchSystemEvent(EventAskBaseaCreateSessionChatGPTHTTPCompleted, true, sessionResponse);
				}
			}
		}
	}

}