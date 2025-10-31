using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class ScreenDownloadAssetsView : BaseScreenView, IScreenView
    {
        public const string ScreenName = "ScreenDownloadAssetsView";

        public const string EventScreenDownloadAssetsViewProgress = "EventScreenDownloadAssetsViewProgress";

        [SerializeField] private TextMeshProUGUI titleScreen;
        [SerializeField] private TextMeshProUGUI descriptionScreen;
        [SerializeField] private Image BackgroundProgressBar;
        [SerializeField] private Image ProgressBar;

        public override string NameScreen
        {
            get { return ScreenName; }
        }

        public override void Initialize(params object[] parameters)
        {
            base.Initialize(parameters);

            titleScreen.text = LanguageController.Instance.GetText("screen.main.menu.title");
            descriptionScreen.text = "";

            UpdateProgressBar(0);

            AssetBundleController.Instance.AssetBundleEvent += OnAssetBundleEvent;
            SystemEventController.Instance.Event += OnSystemEvent;
        }

        public override void Destroy()
        {
            base.Destroy();
            if (AssetBundleController.Instance != null) AssetBundleController.Instance.AssetBundleEvent -= OnAssetBundleEvent;
            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
        }

        private void UpdateProgressBar(float progress)
        {
            ProgressBar.fillAmount = progress;
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(MenuStateDownload.EventGameStateDownloadReportNoConnection))
            {
                BackgroundProgressBar.gameObject.SetActive(false);
                ProgressBar.gameObject.SetActive(false);
                descriptionScreen.text = LanguageController.Instance.GetText("screen.download.no.internet.connection");
            }
            if (nameEvent.Equals(EventScreenDownloadAssetsViewProgress))
            {
                float progress = (float)parameters[0];
                UpdateProgressBar(progress);
            }
        }

        private void OnAssetBundleEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(AssetBundleController.EventAssetBundleAssetsProgress))
            {
                float progress = (float)parameters[0];
                UpdateProgressBar(0.1f + progress);
            }
        }
    }
}
