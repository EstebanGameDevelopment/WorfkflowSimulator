#if ENABLE_OFUSCATION
#if ENABLE_NEW_OFUSCATION
using GUPS.Obfuscator.Attribute;
#else
using OPS.Obfuscator.Attribute;
#endif
#endif
using Newtonsoft.Json;
using yourvrexperience.ai;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
#if ENABLE_OFUSCATION
	[DoNotRenameAttribute]
#endif
	public class AskWorkDayAIDocSummaryImageHTTP : BaseDataHTTP, IHTTPComms
	{
		public const string EventAskWorkDayAIDocSummaryImageHTTPCompleted = "EventAskWorkDayAIDocSummaryImageHTTPCompleted";

		private bool _requestCost = true;
		private string _customEvent;
		private int _inputTokens = 0;

		public class ChatGPTImageRequest
		{
			[JsonProperty(PropertyName = "userid")]
			public int UserID { get; set; }

			[JsonProperty(PropertyName = "username")]
			public string Username { get; set; }

			[JsonProperty(PropertyName = "password")]
			public string Password { get; set; }
			[JsonProperty(PropertyName = "conversationid")]
			public string ConversationId { get; set; }

			[JsonProperty(PropertyName = "question")]
			public string Question { get; set; }
			[JsonProperty(PropertyName = "instructions")]
			public string Instructions { get; set; }
			[JsonProperty(PropertyName = "image")]
			public string Image { get; set; }
			[JsonProperty(PropertyName = "chain")]
			public bool Chain { get; set; }
			[JsonProperty(PropertyName = "debug")]
			public bool Debug { get; set; }
		}


		public string UrlRequest
		{
			get { return GameAIData.Instance.ServerChatGPT + "question/summary_image?debug=true"; }
		}

		public string Build(params object[] _list)
		{
			string question = (string)_list[0];
			string imageBase64 = (string)_list[1];
			_requestCost = (bool)_list[2];
			_customEvent = (string)_list[3];

			_inputTokens = question.Split(' ').Length;

			_method = METHOD_POST;
			_formPost = null;
			_rawData = System.Text.Encoding.UTF8.GetBytes(
					JsonConvert.SerializeObject(new ChatGPTImageRequest
					{
						UserID = GameAIData.Instance.ChatGPTID,
						Username = GameAIData.Instance.ChatGPTUsername,
						Password = GameAIData.Instance.ChatGPTPassword,
						ConversationId = "",
						Instructions = "",
						Question = question,
						Image = imageBase64,
						Chain = false,
						Debug = false
					}));

			SystemEventController.Instance.DispatchSystemEvent(GameAIData.EventGameAIDataAIStartRequest, "summary_image", question);

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
					SystemEventController.Instance.DispatchSystemEvent(EventAskWorkDayAIDocSummaryImageHTTPCompleted, false);
				}
				return;
			}

			// Get Response list
			SystemEventController.Instance.DispatchSystemEvent(GameAIData.EventGameAIDataAIEndRequest, "summary_image", _response);
			if (_requestCost) SystemEventController.Instance.DelaySystemEvent(GameAIData.EventGameAIDataCostAIRequest, 0.2f, "summary_image", _inputTokens, _response);
			SoundsController.Instance.PlaySoundFX(GameSounds.FxAIProcessCompleted, false, 1);
			if (_customEvent.Length > 0)
			{
				SystemEventController.Instance.DispatchSystemEvent(_customEvent, true, _response);
			}
			else
			{
				SystemEventController.Instance.DispatchSystemEvent(EventAskWorkDayAIDocSummaryImageHTTPCompleted, true, _response);
			}
		}
	}

}