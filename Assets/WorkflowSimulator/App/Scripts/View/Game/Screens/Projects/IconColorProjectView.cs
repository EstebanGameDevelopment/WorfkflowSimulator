using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace yourvrexperience.WorkDay
{
	public class IconColorProjectView : MonoBehaviour
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
				ProjectInfoData project = WorkDayData.Instance.CurrentProject.GetProject(Text.text);
				if (project != null)
                {
					ApplyColor(project.GetColor());
					if (Label != null)
					{
						Label.text = project.Name.Substring(0,1);
					}
				}
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