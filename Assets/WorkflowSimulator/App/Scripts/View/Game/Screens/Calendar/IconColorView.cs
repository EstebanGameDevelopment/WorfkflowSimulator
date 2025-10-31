using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace yourvrexperience.WorkDay
{
	public class IconColorView : MonoBehaviour
	{
		public TextMeshProUGUI Text;
		public TextMeshProUGUI Label;
		public Image Icon;

		private bool _locked = false;

		public bool Locked
        {
			get { return _locked; }
			set { _locked = value; }
        }

        private void Start()
        {
			Refresh();
		}

        public void ApplyColor(Color color)
		{
			Icon.color = color;
		}

		public void Refresh()
        {
			if (_locked) return;

			if ((Text != null) && (Text.text.Length > 0))
            {
				ApplyColor(WorkDayData.Instance.CurrentProject.GetColorForMember(Text.text));
				if (Label != null)
                {
					Label.text = WorkDayData.Instance.CurrentProject.GetGroupLetter(Text.text);
				}				
			}
			else
            {
				Label.text = "";
			}
		}

		public void ApplyInfo(string text, Color color)
		{
			ApplyColor(color);
			if ((Label != null) && (text.Length > 0))
			{
				Label.text = text.Substring(0,1);
			}
		}



	}
}