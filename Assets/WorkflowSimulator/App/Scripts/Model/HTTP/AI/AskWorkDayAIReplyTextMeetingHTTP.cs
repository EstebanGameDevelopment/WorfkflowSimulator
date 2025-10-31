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
	public class AskWorkDayAIReplyTextMeetingHTTP : BaseDataHTTP, IHTTPComms
	{
		public const string EventAskWorkDayAIReplyTextMeetingHTTPCompleted = "EventAskWorkDayAIReplyTextMeetingHTTPCompleted";

		private bool _requestCost = true;
		private string _customEvent;
		private int _inputTokens = 0;

		public string UrlRequest
		{
			get { return GameAIData.Instance.ServerChatGPT + "question/reply_meeting?debug=true"; }
		}

		public string Build(params object[] _list)
		{
			string question = (string)_list[0];
			_requestCost = (bool)_list[1];
			_customEvent = (string)_list[2];

			_inputTokens = question.Split(' ').Length;

			_method = METHOD_POST;
			_formPost = null;
			_rawData = System.Text.Encoding.UTF8.GetBytes(
					JsonConvert.SerializeObject(new ChatGPTRequest
					{
						UserID = GameAIData.Instance.ChatGPTID,
						Username = GameAIData.Instance.ChatGPTUsername,
						Password = GameAIData.Instance.ChatGPTPassword,
						ConversationId = "",
						Instructions = "",
						Question = question,
						Chain = false,
						Debug = false
					}));

			SystemEventController.Instance.DispatchSystemEvent(GameAIData.EventGameAIDataAIStartRequest, "reply_meeting", question);

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
					SystemEventController.Instance.DispatchSystemEvent(EventAskWorkDayAIReplyTextMeetingHTTPCompleted, false);
				}
				return;
			}

			// Get Response list
			SystemEventController.Instance.DispatchSystemEvent(GameAIData.EventGameAIDataAIEndRequest, "reply_meeting", _response);
			if (_requestCost) SystemEventController.Instance.DelaySystemEvent(GameAIData.EventGameAIDataCostAIRequest, 0.2f, "reply_meeting", _inputTokens, _response);
			SoundsController.Instance.PlaySoundFX(GameSounds.FxAIProcessCompleted, false, 1);
			if (_customEvent.Length > 0)
			{
				SystemEventController.Instance.DispatchSystemEvent(_customEvent, true, _response);
			}
			else
			{
				SystemEventController.Instance.DispatchSystemEvent(EventAskWorkDayAIReplyTextMeetingHTTPCompleted, true, _response);
			}
		}
	}

}