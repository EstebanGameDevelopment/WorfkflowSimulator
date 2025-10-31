using TMPro;
using UnityEngine;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class ScreenSplashView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenSplashView";

		public const string EventScreenSplashViewSetDescription = "EventScreenSplashViewSetDescription";

		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private TextMeshProUGUI descriptionScreen;

		public override string NameScreen
		{
			get { return ScreenName; }
		}

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			titleScreen.text = LanguageController.Instance.GetText("screen.main.menu.title");
			descriptionScreen.text = "";

			UIEventController.Instance.Event += OnUIEvent;
		}

		public override void Destroy()
		{
			base.Destroy();
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventScreenSplashViewSetDescription))
			{
				descriptionScreen.text = (string)parameters[0];
			}
		}
	}
}