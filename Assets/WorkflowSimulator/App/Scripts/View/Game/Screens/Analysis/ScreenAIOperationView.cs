using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class ScreenAIOperationView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenAIOperationView";

		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private Button askGeneral;
		[SerializeField] private Button askAnalysis;
		[SerializeField] private Button buttonCancel;

        [SerializeField] private TMP_Dropdown dropDownLLM;

        public override string NameScreen
		{
			get { return ScreenName; }
		}


		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			buttonCancel.onClick.AddListener(OnCancel);

			askGeneral.onClick.AddListener(OnAskGeneral);
			askGeneral.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.ai.operation.general");

			askAnalysis.onClick.AddListener(OnAskAnalysis);
			askAnalysis.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.ai.operation.analysis");

			if (ApplicationController.Instance.HumanPlayer == null)
            {
				askAnalysis.interactable = false;
			}

			title.text = LanguageController.Instance.GetText("screen.ai.operation.title");

            WorkDayData.Instance.InitializeDropdown(dropDownLLM);

            dropDownLLM.onValueChanged.AddListener(OnLLMSelectedDropdown);
        }

		public override void Destroy()
		{
			base.Destroy();
		}

		private void OnCancel()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnAskGeneral()
		{
			ScreenController.Instance.CreateScreen(ScreenChatAIView.ScreenName, true, false);
		}

        private void OnLLMSelectedDropdown(int value)
        {
            WorkDayData.Instance.SetLLMProvider(WorkDayData.Instance.GetLLMProviderIndex(dropDownLLM.options[value].text));
        }

        private void OnAskAnalysis()
		{
			ScreenController.Instance.CreateScreen(ScreenAnalysisHumanView.ScreenName, true, false);
		}
	}
}