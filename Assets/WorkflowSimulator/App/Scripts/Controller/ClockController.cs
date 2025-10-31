using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
using static yourvrexperience.WorkDay.ApplicationController;

namespace yourvrexperience.WorkDay
{
    public class ClockController : MonoBehaviour
    {
        public const string EventClockControllerChangedDay = "EventClockControllerChangedDay";
        public const string EventClockControllerHour = "EventClockControllerHour";
        public const string EventClockControllerEndingDay = "EventClockControllerEndingDay";
        public const string EventClockControllerPlayChanged = "EventClockControllerPlayChanged";
        public const string EventClockControllerTimeSpeedUp = "EventClockControllerTimeSpeedUp";

        public const string SubEventClockControllerNewDayStarted = "SubEventClockControllerNewDayStarted";

        public const int SpeedTimeEnterOffice = 30;
        public const int SpeedTimeLunch = 5;
        public const int SpeedTimeLeavingOffice = 30;

        public const int TimeEnterOffice = 10;
        public const int TimeLeaveOffice = 15;
        public const int TotalLunchTime = 75;
        public const int MinutesToStartLeaving = 20;

        [Header("Dails")]
        [SerializeField] private GameObject HourDail;
        [SerializeField] private GameObject MinuteDail;

        [Header("Needle")]
        [SerializeField] private GameObject Hour_Needle;
        [SerializeField] private GameObject Minute_Needle;

        [Header("Buttons")]
        [SerializeField] private Button Hour_Button;
        [SerializeField] private Button Minute_Button;

        [SerializeField] private TextMeshProUGUI DateShort;

        [SerializeField] private TextMeshProUGUI speedTimeDebug;

        private bool _isAM = true;
        private DateTime _currentStartTimeToday;
        private DateTime _currentEndTimeToday;
        private TimeSpan _currentSpanToday;
        private DateTime _currentDateTime;
        private float _timeAcum = 0;

        private Text _hourText;
        private Text _minuteText;

        private TimeSpan _incrementTime;

        private bool _timePlaying = false;
        private bool _hasBeenChangedDay = false;
        
        private float _timeBathroomTrigger = 0;
        private float _timeOutBathroomTrigger = 0;
        
        private float _timeBreakTrigger = 0;
        private float _timeOutBreakTrigger = 0;

        private float _timeInterruptionTrigger = 0;
        private float _timeOutInterruptionTrigger = 0;

        private bool _hasBeenDestroyed = false;

        public bool IsAM
        {
            get { return _isAM; }
            set
            {
                _isAM = value;
            }
        }
        public string HourText
        {
            get { return _hourText.text; }
        }
        public string MinuteText
        {
            get { return _minuteText.text; }
        }
        public bool TimePlaying
        {
            get { return _timePlaying; }
            set {
                bool timePlaying = _timePlaying;
                _timePlaying = value; 
                if (timePlaying != _timePlaying)
                {
                    if (_timePlaying)
                    {
                        _currentStartTimeToday = new DateTime(_currentDateTime.Year, _currentDateTime.Month, _currentDateTime.Day, WorkDayData.Instance.CurrentProject.StartingHour, 0, 0);
                        _currentEndTimeToday = new DateTime(_currentDateTime.Year, _currentDateTime.Month, _currentDateTime.Day, WorkDayData.Instance.CurrentProject.EndingHour, 0, 0);
                        _currentSpanToday = _currentEndTimeToday - _currentStartTimeToday;
                    }
                    SystemEventController.Instance.DispatchSystemEvent(EventClockControllerPlayChanged, _timePlaying);
                }
            }
        }
        public TimeSpan IncrementTime
        {
            get { return _incrementTime; }
            set
            {
                _incrementTime = value;
                RefreshTimeIncreaseDisplayed();
            }
        }

        public void Initialize()
        {
            _hourText = Hour_Button.GetComponentInChildren<Text>();
            _minuteText = Minute_Button.GetComponentInChildren<Text>();

            Hour_Button.interactable = false;
            Minute_Button.interactable = false;

            _currentDateTime = WorkDayData.Instance.CurrentProject.GetCurrentTime();
            IsAM = (_currentDateTime.Hour <= 12);
            IncrementTime = new TimeSpan(0, 0, 1);
            RenderTime();
            RefreshTimeIncreaseDisplayed();

            TimeSetForBathroom();
            TimeSetForBreak();
            TimeSetForInterruption();

            WorkDayData.Instance.CurrentProject.EndDayTrigger = false;

            SystemEventController.Instance.Event += OnSystemEvent;
            UIEventController.Instance.Event += OnUIEvent;
        }

        void OnDestroy()
        {
            Destroy();
        }

        public void Destroy()
        {
            if (!_hasBeenDestroyed)
            {
                _hasBeenDestroyed = true;
                if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
                if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
            }
        }

        private void TimeSetForBathroom()
        {
            _timeBathroomTrigger = 0;
            _timeOutBathroomTrigger = UnityEngine.Random.Range(10, 30);
        }

        private void TimeSetForBreak()
        {
            _timeBreakTrigger = 0;
            _timeOutBreakTrigger = UnityEngine.Random.Range(40, 80);
        }

        private void TimeSetForInterruption()
        {
            _timeInterruptionTrigger = 0;
            _timeOutInterruptionTrigger = UnityEngine.Random.Range(20, 40);
        }

        public void ResetIncrementTime()
        {
            IncrementTime = new TimeSpan(0, 0, 1);
            RefreshTimeIncreaseDisplayed();
        }

        public void SpeedUpTime(int target)
        {
            IncrementTime = new TimeSpan(0, 0, target);
            RefreshTimeIncreaseDisplayed();            
        }

        public void IncreaseTimeSpan(int value)
        {
            double totalSeconds = _incrementTime.TotalSeconds + value;
            if (totalSeconds >= 1)
            {
                IncrementTime = IncrementTime.Add(new TimeSpan(0, 0, value));
                RefreshTimeIncreaseDisplayed();
            }
        }

        private void RefreshTimeIncreaseDisplayed()
        {
            speedTimeDebug.text = GetFormattedTime((int)IncrementTime.TotalSeconds) + "s";
        }

        public void SelectHour(float z)
        {
            Hour_Needle.transform.rotation = Quaternion.Euler(new Vector3(Hour_Needle.transform.rotation.x, Hour_Needle.transform.rotation.y, z));
        }

        public void SelectMinute(float z)
        {
            Minute_Needle.transform.rotation = Quaternion.Euler(new Vector3(Minute_Needle.transform.rotation.x, Minute_Needle.transform.rotation.y, z));
        }

        public string getSelectedTime()
        {
            return GetFormattedTime(_currentDateTime.Hour) + ":" + GetFormattedTime(_currentDateTime.Minute);
        }

        private string GetFormattedTime(int time)
        {
            if (time < 10)
            {
                return "0" + time;
            }
            else
            {
                return time.ToString();
            }
        }

        private void RenderTime()
        {
            // ANALOG TIME
            int extraRotationHour = (int)(((float)_currentDateTime.Minute * 28f) / 60f);
            SelectHour(48 - (((_currentDateTime.Hour % 12) * 30) + extraRotationHour));
            SelectMinute(48 - (((float)_currentDateTime.Minute / 5) * 30f));

            // DIGITAL TIME
            _hourText.text = GetFormattedTime(_currentDateTime.Hour);
            _minuteText.text = GetFormattedTime(_currentDateTime.Minute);

            DateShort.text = _currentDateTime.ToShortDateString();
        }

        public void ChangeToNextDay()
        {
            _hasBeenChangedDay = true;
            
            ApplicationController.Instance.LastProjectFeedback = "";
            ApplicationController.Instance.LastProjectColor = Color.white;

            SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, false);

            WorkDayData.Instance.CurrentProject.StartDayTrigger = false;
            WorkDayData.Instance.CurrentProject.LunchDayTrigger = false;
            WorkDayData.Instance.CurrentProject.EndDayTrigger = false;

            DateTime currentDay = new DateTime(_currentDateTime.Year, _currentDateTime.Month, _currentDateTime.Day, WorkDayData.Instance.CurrentProject.StartingHour, 0, 0);
            _currentDateTime = currentDay.Add(new TimeSpan(24, 0, 0));
            if (WorkDayData.Instance.CurrentProject.EndingDayOfWeek != DayOfWeek.Monday)
            {
                if (_currentDateTime.DayOfWeek == WorkDayData.Instance.CurrentProject.EndingDayOfWeek)
                {
                    DateTime nextMonday = ApplicationController.Instance.GetNextMonday(WorkDayData.Instance.CurrentProject.GetCurrentTime());
                    _currentDateTime = new DateTime(nextMonday.Year, nextMonday.Month, nextMonday.Day, WorkDayData.Instance.CurrentProject.StartingHour, 0, 0);
                }
            }
            _currentStartTimeToday = _currentDateTime;
            _currentEndTimeToday = new DateTime(_currentDateTime.Year, _currentDateTime.Month, _currentDateTime.Day, WorkDayData.Instance.CurrentProject.EndingHour, 0, 0);
            WorkDayData.Instance.CurrentProject.SetCurrentTime(_currentDateTime);
            SystemEventController.Instance.DelaySystemEvent(EventClockControllerChangedDay, 0.1f);
        }
 
        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(ScreenCalendarView.EventScreenCalendarViewSetNewDate))
            {
                ApplicationController.Instance.SunLight.transform.rotation = Quaternion.Euler(40, -30, 0);
            }
            if (nameEvent.Equals(SubEventClockControllerNewDayStarted))
            {
                SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDUpdatePlayTime, true);
            }
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventClockControllerChangedDay))
            {
                SystemEventController.Instance.DelaySystemEvent(RunStateRun.SubEventInformationPopupSavedDisplayed, 1);
            }
            if (nameEvent.Equals(RunStateRun.SubEventInformationPopupSavedDisplayed))
            {
                if (_hasBeenChangedDay)
                {
                    _hasBeenChangedDay = false;
                    UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationRequestAllScreensDestroyed);
                    string title = LanguageController.Instance.GetText("text.info");
                    string description = LanguageController.Instance.GetText("text.starting.new.day") + " " + WorkDayData.Instance.CurrentProject.GetCurrentTime().ToShortDateString();
                    ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenInformationImage, null, title, description, SubEventClockControllerNewDayStarted, "", "", ApplicationController.Instance.GetContentImage(ImagesIndex.NewDay));
                }
            }
        }

        private bool ForbidMeeting()
        {
            DateTime timeToCheck = _currentDateTime.Add(new TimeSpan(0, 20, 0));
            if (timeToCheck.Hour >= WorkDayData.Instance.CurrentProject.LunchHour)
            {
                DateTime timeToLunchCompleted = new DateTime(_currentDateTime.Year, _currentDateTime.Month, _currentDateTime.Day, WorkDayData.Instance.CurrentProject.LunchHour, 0, 0);
                timeToLunchCompleted = timeToLunchCompleted.Add(new TimeSpan(0, TotalLunchTime, 0));
                if (_currentDateTime < timeToLunchCompleted)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            timeToCheck = _currentDateTime.Add(new TimeSpan(0, 40, 0));
            if (timeToCheck.Hour >= WorkDayData.Instance.CurrentProject.EndingHour)
            {
                return true;
            }
            return false;
        }

        private void TriggerTimeEvents()
        {
            if (!WorkDayData.Instance.CurrentProject.StartDayTrigger)
            {
                WorkDayData.Instance.CurrentProject.StartDayTrigger = true;
                CommandsController.Instance.CommandHumansToEnterOffice(SpeedTimeEnterOffice, TimeEnterOffice);
            }
            if (!WorkDayData.Instance.CurrentProject.LunchDayTrigger)
            {
                if (_currentDateTime.Hour >= WorkDayData.Instance.CurrentProject.LunchHour)
                {
                    WorkDayData.Instance.CurrentProject.LunchDayTrigger = true;
                    CommandsController.Instance.CommandHumansToGoLunch(SpeedTimeLunch, TotalLunchTime);
                }
            }
            if (!WorkDayData.Instance.CurrentProject.EndDayTrigger)
            {
                DateTime timeToEnd = _currentDateTime.Add(new TimeSpan(0, MinutesToStartLeaving, 0));
                if (timeToEnd.Hour >= WorkDayData.Instance.CurrentProject.EndingHour)
                {
                    WorkDayData.Instance.CurrentProject.EndDayTrigger = true;
                    SystemEventController.Instance.DispatchSystemEvent(EventClockControllerEndingDay);
                    CommandsController.Instance.CommandHumansToLeaveOffice(SpeedTimeLeavingOffice, TimeLeaveOffice);
                }
            }
            if (CommandsController.Instance.CurrentCommandState == CommandsController.CommandStates.Idle)
            {
                // BATHROOM
                _timeBathroomTrigger += (float)_incrementTime.TotalSeconds;
                if (_timeBathroomTrigger > (_timeOutBathroomTrigger * 60))
                {
                    TimeSetForBathroom();
                    if (!ForbidMeeting())
                    {
                        CommandsController.Instance.CommandRandomHumanGoesBathroom();
                    }
                }
                
                // BREAK
                if (WorkDayData.Instance.CurrentProject.EnableBreaks)
                {
                    _timeBreakTrigger += (float)_incrementTime.TotalSeconds;
                    if (_timeBreakTrigger > (_timeOutBreakTrigger * 60))
                    {
                        TimeSetForBreak();
                        if (!ForbidMeeting())
                        {
                            CommandsController.Instance.CommandRandomHumanGoesBreak();
                        }
                    }
                }

                // INTERRUPTIONS                
                if (WorkDayData.Instance.CurrentProject.EnableInterruptions)
                {
                    _timeInterruptionTrigger += (float)_incrementTime.TotalSeconds;
                    if (_timeInterruptionTrigger > (_timeOutInterruptionTrigger * 60))
                    {
                        TimeSetForInterruption();
                        if (!ForbidMeeting())
                        {
                            CommandsController.Instance.CommandRandomAssholeInterrupts();
                        }
                    }
                }
            }
        }

        public void Run()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y))
            {
                IncreaseTimeSpan(10);
            }

            if (!_timePlaying) return;

            _timeAcum += Time.deltaTime;
            if (_timeAcum > 1)
            {
                _timeAcum -= 1;
                int previousHour = _currentDateTime.Hour;
                _currentDateTime = _currentDateTime.Add(IncrementTime);
                int nextHour = _currentDateTime.Hour;
                if (previousHour < nextHour)
                {                    
                    SystemEventController.Instance.DispatchSystemEvent(EventClockControllerHour);
                }
                WorkDayData.Instance.CurrentProject.SetCurrentTime(_currentDateTime);
                TimeSpan currSpan = (_currentDateTime - _currentStartTimeToday);
                float progressDay = (float)(currSpan.TotalSeconds / _currentSpanToday.TotalSeconds);
                float sunAngle = progressDay * 100f;
                ApplicationController.Instance.SunLight.transform.rotation = Quaternion.Euler(sunAngle + 50, -30, 0);                
                _currentEndTimeToday = new DateTime(_currentDateTime.Year, _currentDateTime.Month, _currentDateTime.Day, WorkDayData.Instance.CurrentProject.EndingHour, 0, 0);
                IsAM = (_currentDateTime.Hour <= 12);
                RenderTime();
                TriggerTimeEvents();
            }
        }
    }
}