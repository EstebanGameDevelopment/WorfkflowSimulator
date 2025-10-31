using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class ScreenBlockerView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenBlockerView";
		
		public const string EventScreenBlockerViewDestroy = "EventScreenBlockerViewDestroy";

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			UIEventController.Instance.Event += OnUIEvent;
		}

        public override void Destroy()
		{
			base.Destroy();

			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventScreenBlockerViewDestroy))
            {
				UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
			}
		}
	}
}