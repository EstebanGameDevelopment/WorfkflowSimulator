using TMPro;
using UnityEngine;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class ScreenWaitProgressView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenWaitProgressView";

		public const string EventScreenWaitProgressViewSetText = "EventScreenWaitProgressViewSetText";
		public const string EventScreenWaitProgressViewSetDescription = "EventScreenWaitProgressViewSetDescription";
		public const string EventScreenWaitProgressViewDestroy = "EventScreenWaitProgressViewDestroy";

		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private TextMeshProUGUI titleDescription;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			titleScreen.text = (string)parameters[0];
			titleDescription.text = (string)parameters[1];

			UIEventController.Instance.Event += OnUIEvent;
		}

		public override void Destroy()
		{
			base.Destroy();

			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventScreenWaitProgressViewDestroy))
            {
				UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
			}
			if (nameEvent.Equals(EventScreenWaitProgressViewSetText))
            {
				titleScreen.text = (string)parameters[0];
				titleDescription.text = (string)parameters[1];
			}
			if (nameEvent.Equals(EventScreenWaitProgressViewSetDescription))
			{
                titleDescription.text = (string)parameters[0];
            }
		}
	}
}