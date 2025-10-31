using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class ScreenLeaveMeetingView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenLeaveMeetingView";

		[SerializeField] private TextMeshProUGUI titleLeaveMeeting;		
		[SerializeField] private TextMeshProUGUI descriptionLeaveMeeting;		
		[SerializeField] private Button buttonCancel;

		[SerializeField] private TextMeshProUGUI textLeaveMeeting;
		[SerializeField] private TextMeshProUGUI textEndMeeting;
		[SerializeField] private Button buttonLeaveMeeting;
		[SerializeField] private Button buttonEndMeeting;

		private MeetingData _meeting;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			_meeting = (MeetingData)parameters[0];

			if (_meeting.ProjectId != -1)
			{
				ProjectInfoData projectInfo = WorkDayData.Instance.CurrentProject.GetProject(_meeting.ProjectId);
				_content.GetComponent<Image>().color = projectInfo.GetColor();
			}

			if (parameters.Length > 1)
            {
				buttonCancel.gameObject.SetActive(false);
            }

			buttonLeaveMeeting.onClick.AddListener(OnLeaveMeeting);
			buttonEndMeeting.onClick.AddListener(OnEndMeeting);

			buttonEndMeeting.interactable = _meeting.CanClose;
			buttonLeaveMeeting.interactable = _meeting.CanLeave;

			titleLeaveMeeting.text = LanguageController.Instance.GetText("screen.leave.meeting.title");
            descriptionLeaveMeeting.text = LanguageController.Instance.GetText("screen.leave.meeting.description");
            textLeaveMeeting.text = LanguageController.Instance.GetText("screen.leave.meeting.leave.action");
            textEndMeeting.text = LanguageController.Instance.GetText("screen.leave.meeting.end.action");

            if (_meeting.IsInterruptionMeeting())
            {
				if (_meeting.InitiatedByPlayer)
                {
					buttonEndMeeting.interactable = true;
				}
			}

			SystemEventController.Instance.DispatchSystemEvent(HumanView.EventHumanViewForceSelection, ApplicationController.Instance.HumanPlayer.NameHuman);

			buttonCancel.onClick.AddListener(OnCancel);
		}

        public override void Destroy()
		{
			base.Destroy();
		}

		private void OnCancel()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnEndMeeting()
		{
			SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerStopMeeting, _meeting);
			OnCancel();
		}

		private void OnLeaveMeeting()
		{
			SystemEventController.Instance.DispatchSystemEvent(MeetingController.EventMeetingControllerLeaveMeeting, _meeting, ApplicationController.Instance.SelectedHuman.NameHuman);
			OnCancel();
		}
	}
}