using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class RunStateExit : IBasicState
	{
		public void Initialize()
		{
			SystemEventController.Instance.Event += OnSystemEvent;

			ApplicationController.Instance.DestroyHUD();
		}

		public void Destroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
		}

		public void Run()
		{
		}
	}
}