using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace yourvrexperience.WorkDay
{
	public class HumanInfoLabelView : MonoBehaviour
	{
		[SerializeField] private GameObject Content;
		[SerializeField] private TextMeshProUGUI Name;
		[SerializeField] private TextMeshProUGUI Task;
		[SerializeField] private Image Background;
		[SerializeField] private IconColorView IconColor;

		[SerializeField] private GameObject IconContent;
		[SerializeField] private Image IconWorking;

		[SerializeField] private GameObject ContentDialog;
		[SerializeField] private TextMeshProUGUI TextDialog;

		private bool _isWorking = false;

        private void Start()
        {
			HideDialog();
		}

        public void SetActivation(bool active)
        {
			Content.SetActive(active);
		}

		public void SetData(string name, string task, Color color)
        {
			Name.text = name;
			Task.text = task;
			Background.color = color;
			IconColor.Refresh();
		}

		public void HideDialog()
        {
			ContentDialog.SetActive(false);
			TextDialog.text = "";
		}

		public void ShowDialog(string text)
		{
			ContentDialog.SetActive(true);
			TextDialog.text = text;
		}

		public void SetWorking(bool isWorking)
		{
			_isWorking = isWorking;
			IconContent.gameObject.SetActive(_isWorking);
		}

		private void Update()
		{
			if (_isWorking)
			{
				IconWorking.transform.localEulerAngles += new Vector3(0, 0, 90 * Time.deltaTime);
			}
		}
	}
}
