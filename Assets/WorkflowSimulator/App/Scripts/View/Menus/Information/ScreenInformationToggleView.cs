using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class ScreenInformationToggleView : ScreenInformationView, IScreenView
	{
		public const string EventScreenInformationToggleViewSetToggle = "EventScreenInformationToggleViewSetToggle";

		public const string ScreenName = "ScreenConfirmationToggle";

		[SerializeField] private Toggle toggleOption;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);
		}


		public override void Destroy()
		{
			base.Destroy();
		}

		protected override void OnConfirmation()
		{
			try
			{
				if (_customOutputEvent != null)
				{
					if (_customOutputEvent.Length > 0)
					{
						UIEventController.Instance.DispatchUIEvent(_customOutputEvent, _origin, ScreenInformationResponses.Confirm, toggleOption.isOn);
					}
				}
				UIEventController.Instance.DispatchUIEvent(EventScreenInformationResponse, _origin, ScreenInformationResponses.Confirm, toggleOption.isOn);
				UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
			}
			catch (Exception err) { };
		}

		protected override void OnUIEvent(string nameEvent, object[] parameters)
		{
			base.OnUIEvent(nameEvent, parameters);

			if (nameEvent.Equals(EventScreenInformationToggleViewSetToggle))
			{
				toggleOption.GetComponentInChildren<TextMeshProUGUI>().text = (string)parameters[0];
				toggleOption.isOn = (bool)parameters[1];
			}
		}
	}
}