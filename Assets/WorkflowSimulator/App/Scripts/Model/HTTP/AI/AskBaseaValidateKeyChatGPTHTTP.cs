#if ENABLE_OFUSCATION
#if ENABLE_NEW_OFUSCATION
using GUPS.Obfuscator.Attribute;
#else
using OPS.Obfuscator.Attribute;
#endif
#endif
using Newtonsoft.Json;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
#if ENABLE_OFUSCATION
	[DoNotRenameAttribute]
#endif
	public class AskBaseaValidateKeyChatGPTHTTP : BaseDataHTTP, IHTTPComms
	{
		public const string EventAskBaseaValidateKeyChatGPTHTTPCompleted = "EventAskBaseaValidateKeyChatGPTHTTPCompleted";

		public class ValidateAPIKey
		{
			[JsonProperty(PropertyName = "provider")]
			public int provider { get; set; }

			[JsonProperty(PropertyName = "api_key")]
			public string api_key { get; set; }
		}

		private string _customEvent;

		public string UrlRequest
		{
			get
			{
#if ENABLE_REMOTE_CORS_SERVER
				return WorkDayData.Instance.GetServerScreenSession() + "/" + "validate-api-key";
#else
				return WorkDayData.Instance.serverScreenSession + ":" + WorkDayData.PORT_SESSION_SERVER + "/" + "validate-api-key";
#endif
			}
		}

		public string Build(params object[] _list)
		{
			_method = METHOD_POST;
			_formPost = null;

			int provider = (int)_list[0];
			string apiKey = (string)_list[1];
			_customEvent = (string)_list[2];

			_rawData = System.Text.Encoding.UTF8.GetBytes(
					JsonConvert.SerializeObject(new ValidateAPIKey
					{
						provider = provider,
						api_key = apiKey
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
					SystemEventController.Instance.DelaySystemEvent(_customEvent, 3, false);
				}
				else
				{
					SystemEventController.Instance.DelaySystemEvent(EventAskBaseaValidateKeyChatGPTHTTPCompleted, 3, false);
				}
				return;
			}

			if (_customEvent.Length > 0)
			{
				SystemEventController.Instance.DelaySystemEvent(_customEvent, 3, true);
			}
			else
			{
				SystemEventController.Instance.DelaySystemEvent(EventAskBaseaValidateKeyChatGPTHTTPCompleted, 3, true);
			}
		}
	}
}