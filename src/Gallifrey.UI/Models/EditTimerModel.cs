using System;
using System.ComponentModel;

namespace Gallifrey.UI.Models
{
    public class EditTimerModel : INotifyPropertyChanged
    {
        private readonly bool hasExportedTime;

        private bool tempTimer;
        private string jiraReference;
        private string tempTimerDescription;
        private DateTime? runDate;
        private int hours;
        private int minutes;

        public event PropertyChangedEventHandler PropertyChanged;
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public DateTime DisplayDate { get; set; }
        public bool TimeEditable { get; set; }
        public string OriginalJiraReference { get; set; }
        public string OriginalTempTimerDescription { get; set; }
        public DateTime? OriginalRunDate { get; set; }
        public int OriginalHours { get; set; }
        public int OriginalMinutes { get; set; }
        public bool IsDefaultOnButton { get; set; }

        public bool DateEditable => !hasExportedTime && !HasModifiedJiraReference;
        public bool JiraReferenceEditable => !hasExportedTime && !HasModifiedRunDate;
        public bool HasModifiedJiraReference => (OriginalJiraReference != JiraReference) || (OriginalTempTimerDescription != TempTimerDescription);
        public bool HasModifiedRunDate => OriginalRunDate.Value.Date != RunDate.Value.Date;
        public bool HasModifiedTime => OriginalHours != Hours || OriginalMinutes != Minutes;

        public EditTimerModel(IBackend gallifrey, Guid timerId)
        {
            var dateToday = DateTime.Now;
            var timer = gallifrey.JiraTimerCollection.GetTimer(timerId);

            TempTimer = timer.TempTimer;
            JiraReference = timer.JiraReference;
            
            if (gallifrey.Settings.AppSettings.KeepTimersForDays > 0)
            {
                MinDate = dateToday.AddDays(gallifrey.Settings.AppSettings.KeepTimersForDays * -1);
                MaxDate = dateToday.AddDays(gallifrey.Settings.AppSettings.KeepTimersForDays);
            }
            else
            {
                MinDate = dateToday.AddDays(-300);
                MaxDate = dateToday.AddDays(300);
            }

            RunDate = timer.DateStarted;
            DisplayDate = timer.DateStarted;

            Hours = timer.ExactCurrentTime.Hours > 9 ? 9 : timer.ExactCurrentTime.Hours;
            Minutes = timer.ExactCurrentTime.Minutes;

            hasExportedTime = timer.HasExportedTime();
            TimeEditable = !timer.IsRunning;

            if (TempTimer)
            {
                TempTimerDescription = timer.JiraName;               
            }

            OriginalTempTimerDescription = TempTimerDescription;
            OriginalJiraReference = JiraReference;
            OriginalRunDate = RunDate;
            OriginalHours = Hours ?? 0;
            OriginalMinutes = Minutes ?? 0;

            IsDefaultOnButton = true;
        }

        public string JiraReference
        {
            get { return jiraReference; }
            set
            {
                jiraReference = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DateEditable"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JiraReferenceEditable"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasModifiedJiraReference"));
            }
        }

        public string TempTimerDescription
        {
            get { return tempTimerDescription; }
            set
            {
                tempTimerDescription = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DateEditable"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JiraReferenceEditable"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasModifiedJiraReference"));
            }
        }

        public DateTime? RunDate
        {
            get { return runDate; }
            set
            {
                runDate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DateEditable"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JiraReferenceEditable"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasModifiedRunDate"));
            }
        }

        public int? Hours
        {
            get { return hours; }
            set
            {
                hours = value ?? 0;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasModifiedTime"));
            }
        }

        public int? Minutes
        {
            get { return minutes; }
            set
            {
                var newValue = value ?? 0;
                if (newValue < 0)
                {
                    if (Hours == 0)
                    {
                        minutes = 0;
                    }
                    else
                    {
                        minutes = 60 + newValue;
                        Hours--;
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Hours"));
                }
                else if(value >= 60)
                {
                    if (Hours == 9)
                    {
                        minutes = 59;
                    }
                    else
                    {
                        Hours++;
                        minutes = newValue - 60;
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Hours"));
                }
                else
                {
                    minutes = newValue;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasModifiedTime"));
            }
        }

        public bool TempTimer
        {
            get { return tempTimer; }
            set
            {
                tempTimer = value;
                TempTimerDescription = OriginalTempTimerDescription;
                JiraReference = OriginalJiraReference;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TempTimer"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TempTimerDescription"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("JiraReference"));
            }
        }

        public void SetNotDefaultButton()
        {
            IsDefaultOnButton = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDefaultOnButton"));
        }
        
        public void AdjustTime(TimeSpan timeAdjustmentAmount, bool addTime)
        {
            var currentTime = new TimeSpan(Hours ?? 0, Minutes ?? 0, 0);

            currentTime = addTime ? currentTime.Add(timeAdjustmentAmount) : currentTime.Subtract(timeAdjustmentAmount);

            Hours = currentTime.Hours > 9 ? 9 : currentTime.Hours;
            Minutes = currentTime.Minutes;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Hours"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Minutes"));
        }
    }
}