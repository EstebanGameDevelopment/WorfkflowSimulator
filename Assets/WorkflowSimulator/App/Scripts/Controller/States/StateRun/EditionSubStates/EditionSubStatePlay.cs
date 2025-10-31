using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class EditionSubStatePlay : EditionSubStateBase, IBasicState
	{
		public override void Initialize()
		{
			base.Initialize();

			UIEventController.Instance.DispatchUIEvent(ScreenPanelEditionView.EventScreenPanelEditionUpdateTitle, "Play");
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

			ApplicationController.Instance.PlayerView.Run();
		}
	}
}