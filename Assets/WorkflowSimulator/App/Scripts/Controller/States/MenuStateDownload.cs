using UnityEngine;
using yourvrexperience.Utils;
using yourvrexperience.ai;
using System.Collections.Generic;
using yourvrexperience.UserManagement;

namespace yourvrexperience.WorkDay
{
	public class MenuStateDownload : IBasicState
	{
		public const string EventGameStateDownloadLoadCompleted = "EventGameStateDownloadLoadCompleted";
		public const string EventGameStateDownloadNoConnection = "EventGameStateDownloadNoConnection";
		public const string EventGameStateDownloadReportNoConnection = "EventGameStateDownloadReportNoConnection";

		public const string CoockieAppVersion = "CoockieAppVersion";
		private int _counterNarration = 0;
		private int _currentVersion = -1;
		private int _newServerVersion = -1;
		private List<string> _languagesForAI = new List<string>();

		private bool _loadingFinished = false;

		public void Initialize()
		{
			SystemEventController.Instance.Event += OnSystemEvent;
			AssetBundleController.Instance.AssetBundleEvent += OnAssetBundleEvent;

			ScreenController.Instance.CreateScreen(ScreenDownloadAssetsView.ScreenName, true, false);

			_counterNarration = 0;
			SystemEventController.Instance.DelaySystemEvent(EventGameStateDownloadNoConnection, 10);
			LoadAssetBundle(false);
		}

		public void Destroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			if (AssetBundleController.Instance != null) AssetBundleController.Instance.AssetBundleEvent -= OnAssetBundleEvent;
		}

		private void LoadAssetBundle(bool clearCache)
		{
			if (_newServerVersion != -1)
			{
				GameAIData.Instance.SaveAllData();
			}

			if (clearCache) AssetBundleController.Instance.ClearLocalCache();

#if ENABLE_REMOTE_CORS_SERVER
#if UNITY_WEBGL
			AssetBundleController.Instance.LoadAssetBundle(WorkDayData.WEBSERVER_ASSETBUNDLE_WORKDAYEDITOR + "webgl/workflowsimulator");
#elif UNITY_STANDALONE_WIN
			AssetBundleController.Instance.LoadAssetBundle(WorkDayData.WEBSERVER_ASSETBUNDLE_WORKDAYEDITOR + "windows/workflowsimulator");
#elif UNITY_STANDALONE_LINUX
            AssetBundleController.Instance.LoadAssetBundle(WorkDayData.WEBSERVER_ASSETBUNDLE_WORKDAYEDITOR + "linux/workflowsimulator");
#elif UNITY_STANDALONE_OSX
            AssetBundleController.Instance.LoadAssetBundle(WorkDayData.WEBSERVER_ASSETBUNDLE_WORKDAYEDITOR + "osx/workflowsimulator");
#endif
#else
#if UNITY_WEBGL
			// AssetBundleController.Instance.LoadAssetBundle(WorkDayData.Instance.URLBase + "webgl/workflowsimulator");
			AssetBundleController.Instance.LoadAssetBundle(WorkDayData.WEBSERVER_ASSETBUNDLE_WORKDAYEDITOR + "webgl/workflowsimulator");
#elif UNITY_STANDALONE_WIN
			AssetBundleController.Instance.LoadAssetBundle(WorkDayData.Instance.URLBase + "windows/workflowsimulator");
#elif UNITY_STANDALONE_LINUX
            AssetBundleController.Instance.LoadAssetBundle(WorkDayData.Instance.URLBase + "linux/workflowsimulator");
#elif UNITY_STANDALONE_OSX
            AssetBundleController.Instance.LoadAssetBundle(WorkDayData.Instance.URLBase + "osx/workflowsimulator");
#endif
#endif
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventGameStateDownloadLoadCompleted))
			{
				ApplicationController.Instance.ChangeGameState(ApplicationController.StatesGame.MainMenu);
			}
			if (nameEvent.Equals(EventGameStateDownloadNoConnection))
			{
				SystemEventController.Instance.DispatchSystemEvent(EventGameStateDownloadReportNoConnection);
			}
			if (nameEvent.Equals(UsersController.EVENT_USER_LOGIN_FORMATTED))
			{
				if ((bool)parameters[0])
				{
					WorkDayData.Instance.DownloadUserSlots((int)UsersController.Instance.CurrentUser.Id);
				}
				else
				{
					SystemEventController.Instance.DelaySystemEvent(MenuStateDownload.EventGameStateDownloadLoadCompleted, 0.1f);
				}
			}
			if (nameEvent.Equals(CheckoutController.EventCheckoutControllerDownloadedSlotsConfirmation))
			{
				SystemEventController.Instance.DelaySystemEvent(MenuStateDownload.EventGameStateDownloadLoadCompleted, 0.1f);
			}
		}

		private void OnAssetBundleEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(AssetBundleController.EventAssetBundleAssetsLoaded))
			{
				if (!_loadingFinished)
				{
					_loadingFinished = true;
					AssetBundleController.Instance.ClearAssetBundleEvents();
					if ((UsersController.Instance.CurrentUser.Email.Length > 0) && (UsersController.Instance.CurrentUser.Password.Length > 0))
					{
						UserModel.LoginWithStoredLogin();
					}
					else
					{
						SystemEventController.Instance.DelaySystemEvent(MenuStateDownload.EventGameStateDownloadLoadCompleted, 0.5f);
					}
				}
			}
		}

		public void Run()
		{
		}
	}
}