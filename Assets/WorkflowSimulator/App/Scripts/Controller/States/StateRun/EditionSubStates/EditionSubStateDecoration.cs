using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class EditionSubStateDecoration : EditionSubStateAdd, IBasicState
	{
		public const string EventSubStateDecorationStarted = "EventSubStateDecorationStarted";


		public EditionSubStateDecoration(AssetDefinitionItem item)
        {
			_itemDefinition = item;
		}

		public override void Initialize()
		{
			base.Initialize();
			
			UIEventController.Instance.DispatchUIEvent(ScreenPanelEditionView.EventScreenPanelEditionUpdateTitle, "Decoration");
			SystemEventController.Instance.DispatchSystemEvent(EventSubStateDecorationStarted);

			UIEventController.Instance.DispatchUIEvent(ScreenPanelEditionView.EventScreenPanelEditionViewActivateCancellation, true);
		}

		public override void Destroy()
		{
			base.Destroy();
		}

		protected override void OnSystemEvent(string nameEvent, object[] parameters)
		{
			base.OnSystemEvent(nameEvent, parameters);
		}
		
		public override void Run()
		{
			base.Run();

			RunPlacement();
		}
	}
}