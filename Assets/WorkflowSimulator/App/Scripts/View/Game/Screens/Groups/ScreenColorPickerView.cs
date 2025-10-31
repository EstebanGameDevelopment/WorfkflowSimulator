using UnityEngine;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class ScreenColorPickerView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenColorPickerView";
	
		public const string EventScreenColorPickerViewColorSelected = "EventScreenColorPickerViewColorSelected";

		[SerializeField] private ColorPicker colorPickerValue;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			colorPickerValue.ColorPickedEvent += OnColorPickedEvent;
			colorPickerValue.ColorCancelEvent += OnColorCancelEvent;
		}

		private void OnColorCancelEvent()
		{			
			UIEventController.Instance.DispatchUIEvent(EventScreenColorPickerViewColorSelected, Color.white);
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnColorPickedEvent(Color value)
		{
			UIEventController.Instance.DispatchUIEvent(EventScreenColorPickerViewColorSelected, value);
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}
	}
}