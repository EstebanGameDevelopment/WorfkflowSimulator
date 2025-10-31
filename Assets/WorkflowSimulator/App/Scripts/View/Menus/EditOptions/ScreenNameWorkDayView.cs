using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.ai;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class ScreenNameWorkDayView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenNameWorkDayView";

		public const string EventScreenNameWorkDayViewCreate = "EventScreenNameWorkDayViewCreate";
		public const string EventScreenNameWorkDayViewBack = "EventScreenNameWorkDayViewBack";
		public const string EventScreenNameWorkDayViewTeamGenerated = "EventScreenNameWorkDayViewTeamGenerated";

		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private TextMeshProUGUI titleName;
		[SerializeField] private CustomInput inputName;
		[SerializeField] private TextMeshProUGUI titleDescription;
		[SerializeField] private CustomInput inputDescription;
		[SerializeField] private CustomToggle toogleAIGenerated;
		[SerializeField] private CustomToggle toogleEnableBreaks;
		[SerializeField] private CustomToggle toogleEnableInterruptions;
		[SerializeField] private TextMeshProUGUI feedback;
		[SerializeField] private Button buttonCreate;
		[SerializeField] private Button buttonUseSampleData;
		[SerializeField] private Button buttonBack;

		[SerializeField] private TextMeshProUGUI titleTimeStart;
		[SerializeField] private TextMeshProUGUI titleTimeLunch;
		[SerializeField] private TextMeshProUGUI titleTimeEnd;
		[SerializeField] private TextMeshProUGUI titleOptionalRealism;

		[SerializeField] private TimePicker pickTimeStart;
		[SerializeField] private TimePicker pickTimeLunch;
		[SerializeField] private TimePicker pickTimeEnd;

		[SerializeField] private TextMeshProUGUI titleWeekend;
		[SerializeField] private CustomDropdown DropDownWeekend;

		[SerializeField] private CustomButton btnStartDay;
		[SerializeField] private CustomButton btnLunchTime;
		[SerializeField] private CustomButton btnEndDay;

		private bool _isAITriggered = false;
		private int _startingHour = 9;
		private int _lunchHour = 13;
		private int _endHour = 18;
		private DayOfWeek _weekend = DayOfWeek.Saturday;

		private bool _enableAITeamGeneration = false;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			buttonBack.onClick.AddListener(OnButtonBack);
			buttonCreate.onClick.AddListener(OnButtonCreateStory);

			titleScreen.text = LanguageController.Instance.GetText("screen.create.new.company.title");
			titleName.text = LanguageController.Instance.GetText("screen.create.new.company.name");
			titleDescription.text = LanguageController.Instance.GetText("screen.create.new.company.description");

			inputDescription.OnFocusOverEvent += OnFeedbackInputDescription;
			inputDescription.OnFocusOutEvent += OnFeedbackReset;

            buttonUseSampleData.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.create.new.company.use.sample.data");
			buttonUseSampleData.onClick.AddListener(OnUseSampleData);

            toogleAIGenerated.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.create.new.company.toggle.ai.generation");
			toogleAIGenerated.onValueChanged.AddListener(OnGenerationAI);

			toogleEnableBreaks.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.create.new.company.toggle.enable.breaks");
			toogleEnableBreaks.onValueChanged.AddListener(OnGenerationAI);

			toogleEnableInterruptions.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.create.new.company.toggle.enable.interruptions");
			toogleEnableInterruptions.onValueChanged.AddListener(OnGenerationAI);

			toogleAIGenerated.PointerEnterButton += OnToogleAIEnter;
			toogleAIGenerated.PointerExitButton += OnToogleExit;

			toogleEnableBreaks.PointerEnterButton += OnToogleEnableBreaksEnter;
			toogleEnableBreaks.PointerExitButton += OnToogleExit;

			toogleEnableInterruptions.PointerEnterButton += OnToogleEnableInterruptionsEnter;
			toogleEnableInterruptions.PointerExitButton += OnToogleExit;

			DropDownWeekend.PointerEnterButton += OnWeekendEnter;
			DropDownWeekend.PointerExitButton += OnWeekendExit;

			btnStartDay.PointerEnterButton += OnEnterStartDay;
			btnStartDay.PointerExitButton += OnExitFeedback;

			btnLunchTime.PointerEnterButton += OnEnterLunchTime;
			btnLunchTime.PointerExitButton += OnExitFeedback;

			btnEndDay.PointerEnterButton += OnEnterEndDay;
			btnEndDay.PointerExitButton += OnExitFeedback;

			feedback.text = "";

            pickTimeStart.OnTimeSelected.AddListener(OnStartTimeSelect);
            pickTimeLunch.OnTimeSelected.AddListener(OnLunchTimeSelect);
            pickTimeEnd.OnTimeSelected.AddListener(OnEndTimeSelect);

			SetTimesMajorWorkEvents();

            titleTimeStart.text = LanguageController.Instance.GetText("screen.project.start.hour");
            titleTimeLunch.text = LanguageController.Instance.GetText("screen.project.lunch.hour");
            titleTimeEnd.text = LanguageController.Instance.GetText("screen.project.end.hour");
            titleWeekend.text = LanguageController.Instance.GetText("screen.project.day.weekend");
            titleOptionalRealism.text = LanguageController.Instance.GetText("screen.project.optional.realism");

            DropDownWeekend.ClearOptions();
			int selectionWeekend = 0;
			DropDownWeekend.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("day.week."+ DayOfWeek.Monday.ToString().ToLower())));

            if (DayOfWeek.Monday == _weekend) selectionWeekend = 0;

			DropDownWeekend.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("day.week." + DayOfWeek.Tuesday.ToString().ToLower())));
            if (DayOfWeek.Tuesday == _weekend) selectionWeekend = 1;

			DropDownWeekend.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("day.week." + DayOfWeek.Wednesday.ToString().ToLower())));
            if (DayOfWeek.Wednesday == _weekend) selectionWeekend = 2;

			DropDownWeekend.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("day.week." + DayOfWeek.Thursday.ToString().ToLower())));
            if (DayOfWeek.Thursday == _weekend) selectionWeekend = 3;

			DropDownWeekend.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("day.week." + DayOfWeek.Friday.ToString().ToLower())));
            if (DayOfWeek.Friday == _weekend) selectionWeekend = 4;

			DropDownWeekend.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("day.week." + DayOfWeek.Saturday.ToString().ToLower())));
            if (DayOfWeek.Saturday == _weekend) selectionWeekend = 5;

			DropDownWeekend.options.Add(new TMP_Dropdown.OptionData(LanguageController.Instance.GetText("day.week." + DayOfWeek.Sunday.ToString().ToLower())));
            if (DayOfWeek.Sunday == _weekend) selectionWeekend = 6;

			DropDownWeekend.value = selectionWeekend;
			DropDownWeekend.onValueChanged.AddListener(OnWeekendDayChanged);

			SystemEventController.Instance.Event += OnSystemEvent;

            inputName.text = "";
            inputDescription.text = "";
        }

        public override void Destroy()
		{
			base.Destroy();

			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
		}

		private void SetTimesMajorWorkEvents()
		{
            pickTimeStart.ChangeCurrentSelectedHour(_startingHour);
            pickTimeStart.ChangeCurrentSelectedMinute(0);
            pickTimeStart.IsAM = (_startingHour <= 12);
            pickTimeStart.UpdateTimeDisplayed();
            pickTimeStart.SelectHour(48 - ((pickTimeStart.CurrentSelectedHour % 12) * 30));
            pickTimeStart.SelectMinute(48 - (((float)pickTimeStart.CurrentSelectedMinute / 5) * 30f));

            pickTimeLunch.ChangeCurrentSelectedHour(_lunchHour);
            pickTimeLunch.ChangeCurrentSelectedMinute(0);
            pickTimeLunch.IsAM = (_lunchHour <= 12);
            pickTimeLunch.UpdateTimeDisplayed();
            pickTimeLunch.SelectHour(48 - ((pickTimeLunch.CurrentSelectedHour % 12) * 30));
            pickTimeLunch.SelectMinute(48 - (((float)pickTimeLunch.CurrentSelectedMinute / 5) * 30f));

            pickTimeEnd.ChangeCurrentSelectedHour(_endHour);
            pickTimeEnd.ChangeCurrentSelectedMinute(0);
            pickTimeEnd.IsAM = (_endHour <= 12);
            pickTimeEnd.UpdateTimeDisplayed();
            pickTimeEnd.SelectHour(48 - ((pickTimeEnd.CurrentSelectedHour % 12) * 30));
            pickTimeEnd.SelectMinute(48 - (((float)pickTimeEnd.CurrentSelectedMinute / 5) * 30f));
        }

        private void OnUseSampleData()
        {
			inputName.text = LanguageController.Instance.GetText("screen.create.new.company.title.sample");
            inputDescription.text = LanguageController.Instance.GetText("screen.create.new.company.description.sample");

            pickTimeStart.IsAM = true;
            pickTimeLunch.IsAM = true;
            pickTimeEnd.IsAM = true;

            _startingHour = 9;
			_lunchHour = 13;
			_endHour = 18;
			_weekend = DayOfWeek.Saturday;
            DropDownWeekend.value = 5;
			toogleAIGenerated.isOn = true;

            toogleEnableBreaks.isOn = true;
            toogleEnableInterruptions.isOn = false;

            SetTimesMajorWorkEvents();
        }

        private void OnButtonBack()
		{
            UIEventController.Instance.DispatchUIEvent(EventScreenNameWorkDayViewBack);
            UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void OnWeekendDayChanged(int value)
		{
			switch (value)
			{
				case 0:
					_weekend = DayOfWeek.Monday;
					break;
				case 1:
					_weekend = DayOfWeek.Tuesday;
					break;
				case 2:
					_weekend = DayOfWeek.Wednesday;
					break;
				case 3:
					_weekend = DayOfWeek.Thursday;
					break;
				case 4:
					_weekend = DayOfWeek.Friday;
					break;
				case 5:
					_weekend = DayOfWeek.Saturday;
					break;
				case 6:
					_weekend = DayOfWeek.Sunday;
					break;
			}
		}

		private void OnWeekendExit(CustomDropdown value)
		{
			feedback.text = "";
		}

		private void OnWeekendEnter(CustomDropdown value)
		{
			feedback.text = LanguageController.Instance.GetText("screen.create.new.company.description.weekend.starts");
		}

		private void OnEnterEndDay(CustomButton value)
		{
			feedback.text = LanguageController.Instance.GetText("screen.create.new.company.description.day.end");
		}

		private void OnEnterLunchTime(CustomButton value)
		{
			feedback.text = LanguageController.Instance.GetText("screen.create.new.company.description.lunch.time");
		}

		private void OnEnterStartDay(CustomButton value)
		{
			feedback.text = LanguageController.Instance.GetText("screen.create.new.company.description.start.day");
		}

		private void OnExitFeedback(CustomButton value)
		{
			feedback.text = "";
		}

		private void OnToogleEnableBreaksEnter(CustomToggle value)
        {
			feedback.text = LanguageController.Instance.GetText("screen.create.new.company.description.enable.breaks");
		}

        private void OnToogleEnableInterruptionsEnter(CustomToggle value)
        {
			feedback.text = LanguageController.Instance.GetText("screen.create.new.company.description.enable.interruptions");
		}

        private void OnToogleExit(CustomToggle value)
        {
			OnFeedbackReset();
		}

        private void OnToogleAIEnter(CustomToggle value)
        {
			feedback.text = LanguageController.Instance.GetText("screen.create.new.company.description.toogle.ai.generation");			
		}

		private void OnFeedbackReset()
        {
			feedback.text = "";
		}

        private void OnFeedbackInputDescription()
        {
			feedback.text = LanguageController.Instance.GetText("screen.create.new.company.description.ai.generation");
		}

		private void OnStartTimeSelect()
		{
			if (pickTimeEnd.CurrentSelectedHour - pickTimeStart.CurrentSelectedHour < 2)
			{
				pickTimeStart.IsAM = true;
				pickTimeStart.ChangeCurrentSelectedHour(_startingHour);
				pickTimeStart.ChangeCurrentSelectedMinute(0);
				pickTimeStart.IsAM = (_startingHour <= 12);
				pickTimeStart.SelectHour(48 - ((pickTimeStart.CurrentSelectedHour % 12) * 30));
				pickTimeStart.SelectMinute(48 - (((float)pickTimeStart.CurrentSelectedMinute / 5) * 30f));
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.projects.there.should.be.a.difference.of.2.hours"));
			}
			else
			{
				_startingHour = pickTimeStart.CurrentSelectedHour;
			}
			pickTimeStart.UpdateTimeDisplayed();
		}

		private void OnLunchTimeSelect()
		{
			if ((pickTimeEnd.CurrentSelectedHour - pickTimeLunch.CurrentSelectedHour < 2)
				|| (pickTimeLunch.CurrentSelectedHour - pickTimeStart.CurrentSelectedHour < 2))
			{
				pickTimeLunch.IsAM = true;
				pickTimeLunch.ChangeCurrentSelectedHour(_lunchHour);
				pickTimeLunch.ChangeCurrentSelectedMinute(0);
				pickTimeLunch.IsAM = (_lunchHour <= 12);
				pickTimeLunch.SelectHour(48 - ((pickTimeLunch.CurrentSelectedHour % 12) * 30));
				pickTimeLunch.SelectMinute(48 - (((float)pickTimeLunch.CurrentSelectedMinute / 5) * 30f));
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.projects.lunch.time.should.be.in.the.right.hours"));
			}
			else
			{
				_lunchHour = pickTimeLunch.CurrentSelectedHour;
			}
			pickTimeLunch.UpdateTimeDisplayed();
		}


		private void OnEndTimeSelect()
		{
			if (pickTimeEnd.CurrentSelectedHour - pickTimeStart.CurrentSelectedHour < 2)
			{
				pickTimeEnd.IsAM = true;
				pickTimeEnd.ChangeCurrentSelectedHour(_endHour);
				pickTimeEnd.ChangeCurrentSelectedMinute(0);
				pickTimeEnd.IsAM = (_endHour <= 12);
				pickTimeEnd.SelectHour(48 - ((pickTimeEnd.CurrentSelectedHour % 12) * 30));
				pickTimeEnd.SelectMinute(48 - (((float)pickTimeEnd.CurrentSelectedMinute / 5) * 30f));
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.warning"), LanguageController.Instance.GetText("screen.projects.there.should.be.a.difference.of.2.hours"));
			}
			else
			{
				_endHour = pickTimeEnd.CurrentSelectedHour;
			}
			pickTimeEnd.UpdateTimeDisplayed();
		}

		private void OnButtonCreateStory()
		{
			if ((inputName.text.Length > 4) && (inputDescription.text.Length > 20))
			{
				if (!toogleAIGenerated.isOn)
                {
					UIEventController.Instance.DispatchUIEvent(EventScreenNameWorkDayViewCreate, true, inputName.text, inputDescription.text, toogleEnableBreaks.isOn, toogleEnableInterruptions.isOn, _startingHour, _lunchHour, _endHour, _weekend);
				}
				else
                {
					_enableAITeamGeneration = true;
					string titleCreate = LanguageController.Instance.GetText("text.info");
					string descriptionCreate = LanguageController.Instance.GetText("screen.gamestateeditoptions.now.ai.initialization");
					ScreenController.Instance.CreateScreen(ScreenWaitProgressView.ScreenName, false, true, titleCreate, descriptionCreate);
					ApplicationController.Instance.SetUpAISession();
				}
			}
			else
			{
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformation, null, LanguageController.Instance.GetText("text.error"), LanguageController.Instance.GetText("screen.name.story.name.too.short"));
			}
		}

		private void OnGenerationAI(bool value)
		{

		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(InitProviderLLMHTTP.EventInitProviderLLMHTTPCompleted))
			{
				if (_enableAITeamGeneration)
                {
					if (!_isAITriggered)
					{
						_isAITriggered = true;
						UIEventController.Instance.DispatchUIEvent(ScreenWaitProgressView.EventScreenWaitProgressViewDestroy);
						AICommandsController.Instance.AddNewAICommand(new AICommandGenerateTeam(), true, EventScreenNameWorkDayViewTeamGenerated, inputDescription.text, toogleEnableInterruptions.isOn);
					}
				}
			}
			if (nameEvent.Equals(EventScreenNameWorkDayViewTeamGenerated))
			{
				if ((bool)parameters[0])
                {
					UIEventController.Instance.DispatchUIEvent(EventScreenNameWorkDayViewCreate, false, inputName.text, inputDescription.text, toogleEnableBreaks.isOn, toogleEnableInterruptions.isOn, _startingHour, _lunchHour, _endHour, _weekend, (TeamCompanyListJSON)parameters[1]);
				}
			}
		}
	}
}