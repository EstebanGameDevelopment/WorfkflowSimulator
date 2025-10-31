#if ENABLE_OFUSCATION
#if ENABLE_NEW_OFUSCATION
using GUPS.Obfuscator.Attribute;
#else
using OPS.Obfuscator.Attribute;
#endif
#endif
using System;
using System.Collections.Generic;
using yourvrexperience.UserManagement;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
#if ENABLE_OFUSCATION
	[DoNotRenameAttribute]
#endif
	public class ConsultUserProjectsHTTP : BaseDataHTTP, IHTTPComms
	{
		public const string EventConsultUserProjectsHTTPCompleted = "EventConsultUserProjectsHTTPCompleted";

		private string _urlRequest = "";

		public string UrlRequest
		{
			get
			{
				if (_urlRequest.Length == 0)
				{
					_urlRequest = WorkDayData.Instance.URLBase + "WorkDayConsultUserProjects.php";
				}
				return _urlRequest;
			}
		}

		public string Build(params object[] _list)
		{
			string callParams = "?user=" + (int)_list[0] + "&password=" + UsersController.Instance.CurrentUser.Password + "&salt=" + UsersController.Instance.CurrentUser.Salt;
			return callParams;
		}

		public override void Response(string _response)
		{
			if (!ResponseCode(_response))
			{
				SystemEventController.Instance.DispatchSystemEvent(EventConsultUserProjectsHTTPCompleted, false);
				return;
			}

			string[] data = _jsonResponse.Split(new string[] { CommController.TOKEN_SEPARATOR_BLOCKS }, StringSplitOptions.None);
			bool success = false;
			if (!bool.TryParse(data[0], out success))
			{
				SystemEventController.Instance.DispatchSystemEvent(EventConsultUserProjectsHTTPCompleted, false);
			}
			else
			{
				if (success)
				{
					string[] information = data[1].Split(new string[] { CommController.TOKEN_SEPARATOR_LINES }, StringSplitOptions.None);
					List<ProjectEntryIndex> finalData = new List<ProjectEntryIndex>();
					foreach (string info in information)
					{
						string[] tokens = info.Split(new string[] { CommController.TOKEN_SEPARATOR_EVENTS }, StringSplitOptions.None);
						if (tokens.Length > 2)
						{
							finalData.Add(new ProjectEntryIndex(tokens));
						}
					}
					SystemEventController.Instance.DispatchSystemEvent(EventConsultUserProjectsHTTPCompleted, true, finalData);
				}
				else
				{
					SystemEventController.Instance.DispatchSystemEvent(EventConsultUserProjectsHTTPCompleted, false);
				}
			}
		}
	}
}