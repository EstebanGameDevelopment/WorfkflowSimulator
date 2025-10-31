using yourvrexperience.Utils;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace yourvrexperience.WorkDay
{
	public class HourProgressLabelView : MonoBehaviour
	{
		[SerializeField] private GameObject Content;
		[SerializeField] private TextMeshProUGUI Name;

		public void SetHours(float hours)
        {
			Name.text = Utilities.CeilDecimal(hours, 1) + "h";
		}

		public void SetText(string text)
		{
			Name.text = text;
		}
	}
}
