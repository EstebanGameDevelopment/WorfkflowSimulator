using InGameCodeEditor;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.WorkDayData;

namespace yourvrexperience.WorkDay
{
	public class ScreenFinalRequestAIView : ScreenInformationView, IScreenView
	{
		public const string ScreenName = "ScreenFinalRequestAIView";
		public const string EventScreenFinalRequestAIViewDisableInput = "EventScreenFinalRequestAIViewDisableInput";
		public const string EventScreenFinalRequestAIViewOpened = "EventScreenFinalRequestAIViewOpened";
		public const string EventScreenFinalRequestAIViewClosed = "EventScreenFinalRequestAIViewClosed";

		[SerializeField] private TMP_Dropdown dropDownLLM;
		[SerializeField] private CodeEditor codeEditor;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			WorkDayData.Instance.InitializeDropdown(dropDownLLM);

			dropDownLLM.onValueChanged.AddListener(OnLLMSelectedDropdown);

			UIEventController.Instance.DispatchUIEvent(EventScreenFinalRequestAIViewOpened);
		}

		private void OnLLMSelectedDropdown(int value)
		{
			WorkDayData.Instance.SetLLMProvider(WorkDayData.Instance.GetLLMProviderIndex(dropDownLLM.options[value].text));
		}

		public override void Destroy()
		{
			base.Destroy();

            UIEventController.Instance.DispatchUIEvent(EventScreenFinalRequestAIViewClosed);
        }

		protected override void OnUIEvent(string nameEvent, object[] parameters)
		{
			base.OnUIEvent(nameEvent, parameters);

			if (nameEvent.Equals(EventScreenFinalRequestAIViewDisableInput))
			{
				_inputValue.interactable = false;
				Transform contentButtonOk = yourvrexperience.Utils.Utilities.FindNameInChildren(_content, "ButtonOk");
				if (contentButtonOk != null)
				{
					contentButtonOk.GetComponent<Button>().interactable = false;
				}
			}
			if (nameEvent.Equals(EventScreenInformationSetInputText))
			{
				codeEditor.Refresh();
			}
		}
	}
}