#if ENABLE_OFUSCATION
#if ENABLE_NEW_OFUSCATION
using GUPS.Obfuscator.Attribute;
#else
using OPS.Obfuscator.Attribute;
#endif
#endif
using System.Text;
using yourvrexperience.UserManagement;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
#if ENABLE_OFUSCATION
    [DoNotRenameAttribute]
#endif
    public class DownloadProjectDataHTTP : BaseDataHTTP, IHTTPComms
    {
        public const string EventDownloadProjectDataHTTPCompleted = "EventDownloadProjectDataHTTPCompleted";

        private string _urlRequest = "";

        public string UrlRequest
        {
            get
            {
                if (_urlRequest.Length == 0)
                {
                    _urlRequest = WorkDayData.Instance.URLBase + "WorkDayDownloadProject.php";
                }
                return _urlRequest;
            }
        }

        public string Build(params object[] _list)
        {
            string callParams = "?user=" + UsersController.Instance.CurrentUser.Id + "&password=" + UsersController.Instance.CurrentUser.Password + "&salt=" + UsersController.Instance.CurrentUser.Salt + "&id=" + (int)_list[0];
            return callParams;
        }

        public override void Response(byte[] _response)
        {
            if ((_response == null) || (_response.Length == 0))
            {
                SystemEventController.Instance.DispatchSystemEvent(EventDownloadProjectDataHTTPCompleted, true, "");
            }
            else
            {
                byte[] dataBinary = CompressionUtils.DecompressWithBrotli(_response);
                string dataJson = Encoding.UTF8.GetString(dataBinary);
                SystemEventController.Instance.DispatchSystemEvent(EventDownloadProjectDataHTTPCompleted, true, dataJson);
            }
        }
    }
}