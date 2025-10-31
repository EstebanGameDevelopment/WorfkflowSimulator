using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class EditionSubStateAvatar : EditionSubStateAdd, IBasicState
	{
		public const string EventSubStateAvatarStarted = "EventSubStateAvatarStarted";

		public EditionSubStateAvatar(AssetDefinitionItem item)
		{
			_itemDefinition = item;
		}

		public override void Initialize()
		{
			base.Initialize();

			_factorScale = 1;

			UIEventController.Instance.DispatchUIEvent(ScreenPanelEditionView.EventScreenPanelEditionUpdateTitle, "Avatar");
			SystemEventController.Instance.DispatchSystemEvent(EventSubStateAvatarStarted);

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