using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class ScreenWorkLogsView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenCheckLogsView";

		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private TextMeshProUGUI titleStarted;
		[SerializeField] private TextMeshProUGUI titleEnded;
		[SerializeField] private TextMeshProUGUI titleHours;
		[SerializeField] private TMP_InputField inputLogWork;
		[SerializeField] private TextMeshProUGUI dateStarted;
		[SerializeField] private TextMeshProUGUI dateEnded;
		[SerializeField] private TextMeshProUGUI valueHours;
		[SerializeField] private SlotManagerView SlotManagerWorkLogs;
		[SerializeField] private GameObject PrefabWorkLog;
		[SerializeField] private Button CloseButton;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);
			
			int taskUID = (int)parameters[0];

			List<TimeWorkingDataDisplay> unfilteredLogWorks = WorkDayData.Instance.CurrentProject.GetAllLogsWorkForTask(-1, taskUID);
			List<TimeWorkingDataDisplay> logWorks = new List<TimeWorkingDataDisplay>();
			for (int i = 0; i < unfilteredLogWorks.Count; i++)
            {
				string logComment = unfilteredLogWorks[i].Data;
				string ownerComment = unfilteredLogWorks[i].Owner;
				bool include = true;
				if ((logComment!=null) && (logComment.Length > 1))
                {
					foreach (TimeWorkingDataDisplay log in logWorks)
					{
						if (log.Data.Equals(logComment) && log.Owner.Equals(ownerComment))
						{
							include = false;
						}
					}
				}
				else
                {
					include = false;
				}
				if (include)
                {
					logWorks.Add(unfilteredLogWorks[i]);
				}
			}

			int totalWorkingHours = (WorkDayData.Instance.CurrentProject.EndingHour - WorkDayData.Instance.CurrentProject.StartingHour);
			int totalOffHours = 24 - totalWorkingHours;

			for (int i = 0; i < logWorks.Count; i++)
			{
				TimeWorkingDataDisplay currentLog = logWorks[i];
				DateTime earliestStartDate = currentLog.StartTime;
				DateTime latestEndDate = currentLog.EndTime;
				foreach (TimeWorkingDataDisplay unfilteredLog in unfilteredLogWorks)
				{
					if (unfilteredLog != currentLog)
                    {
						if (unfilteredLog.Data.Equals(currentLog.Data))
						{
							if (unfilteredLog.StartTime < earliestStartDate) earliestStartDate = unfilteredLog.StartTime;
							if (unfilteredLog.EndTime > latestEndDate) latestEndDate = unfilteredLog.EndTime;
						}
					}
				}
				
				currentLog.StartTime = earliestStartDate;
				currentLog.EndTime = latestEndDate;
				float totalTimeHours = (float)((currentLog.EndTime - currentLog.StartTime).TotalMinutes / 60f);

				int totalDays = latestEndDate.DayOfYear - earliestStartDate.DayOfYear;
				currentLog.TotalDisplayTime = totalTimeHours - (totalDays * totalOffHours);
			}
			
			List<TimeWorkingDataDisplay> sortedLogWork = logWorks.OrderBy(task => task.StartTime).ToList();

			SlotManagerWorkLogs.ClearCurrentGameObject(true);
			SlotManagerWorkLogs.Initialize(0, new List<ItemMultiObjectEntry>(), PrefabWorkLog);

			for (int i = 0; i < sortedLogWork.Count; i++)
			{
				SlotManagerWorkLogs.AddItem(new ItemMultiObjectEntry(SlotManagerWorkLogs.gameObject, SlotManagerWorkLogs.Data.Count, sortedLogWork[(sortedLogWork.Count - 1) - i]));
			}

			UIEventController.Instance.Event += OnUIEvent;

			CloseButton.onClick.AddListener(OnCloseButton);

			titleStarted.gameObject.SetActive(false);
			titleEnded.gameObject.SetActive(false);
			inputLogWork.gameObject.SetActive(false);
			dateStarted.gameObject.SetActive(false);
			dateEnded.gameObject.SetActive(false);
			titleHours.gameObject.SetActive(false);
			valueHours.gameObject.SetActive(false);

			titleScreen.text = LanguageController.Instance.GetText("screen.log.title");
            titleStarted.text = LanguageController.Instance.GetText("screen.log.started");
            titleEnded.text = LanguageController.Instance.GetText("screen.log.ended");
            titleHours.text = LanguageController.Instance.GetText("screen.log.hours");
        }

		public override void Destroy()
		{
			base.Destroy();
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
		}

		private void OnCloseButton()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ItemLogWorkView.EventItemLogWorkViewSelected))
            {
				if ((GameObject)parameters[0] == SlotManagerWorkLogs.gameObject)
                {
					if ((int)parameters[2] == -1)
                    {
						titleStarted.gameObject.SetActive(false);
						titleEnded.gameObject.SetActive(false);
						inputLogWork.gameObject.SetActive(false);
						dateStarted.gameObject.SetActive(false);
						dateEnded.gameObject.SetActive(false);
						titleHours.gameObject.SetActive(false);
						valueHours.gameObject.SetActive(false);
					}
					else
                    {
						titleStarted.gameObject.SetActive(true);
						titleEnded.gameObject.SetActive(true);
						inputLogWork.gameObject.SetActive(true);
						dateStarted.gameObject.SetActive(true);
						dateEnded.gameObject.SetActive(true);
						titleHours.gameObject.SetActive(true);
						valueHours.gameObject.SetActive(true);

						TimeWorkingDataDisplay logWork = (TimeWorkingDataDisplay)parameters[3];
						if (logWork.InProgress)
                        {
							dateStarted.text = logWork.StartTime.ToShortDateString() + " " + logWork.StartTime.ToShortTimeString();
							inputLogWork.gameObject.SetActive(false);
							titleEnded.gameObject.SetActive(false);
							valueHours.text = Utilities.CeilDecimal(logWork.TotalDisplayTime, 1);
							dateEnded.text = LanguageController.Instance.GetText("screen.work.logs.currently.working");
						}
						else
                        {
							inputLogWork.text = logWork.Data;
							dateStarted.text = logWork.StartTime.ToShortDateString() + " " + logWork.StartTime.ToShortTimeString();
							dateEnded.text = logWork.EndTime.ToShortDateString() + " " + logWork.EndTime.ToShortTimeString();
							valueHours.text = Utilities.CeilDecimal(logWork.TotalDisplayTime, 1);
						}
					}
				}
			}
		}
	}
}