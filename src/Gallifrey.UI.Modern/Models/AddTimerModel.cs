using System;
using System.ComponentModel;

namespace Gallifrey.UI.Modern.Models
{
    public class AddTimerModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string JiraReference { get; set; }
        public bool JiraReferenceEditable { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime DisplayDate { get; set; }
        public bool DateEditable { get; set; }
        public int StartHours { get; set; }
        public int StartMinutes { get; set; }
        public bool StartNow { get; set; }
        public bool StartNowEditable { get; set; }
        public bool AssignToMe { get; set; }
        public bool InProgress { get; set; }

        public AddTimerModel(IBackend gallifrey, string jiraRef, DateTime? startDate, bool? enableDateChange, TimeSpan? preloadTime, bool? startNow)
        {
            var dateToday = DateTime.Now;

            JiraReference = jiraRef;
            JiraReferenceEditable = string.IsNullOrWhiteSpace(jiraRef);

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

            if (!startDate.HasValue) startDate = dateToday;

            if (startDate.Value < MinDate || startDate.Value > MaxDate)
            {
                DisplayDate = dateToday;
                StartDate = dateToday;
            }
            else
            {
                DisplayDate = startDate.Value;
                StartDate = startDate.Value;
            }

            DateEditable = !enableDateChange.HasValue || enableDateChange.Value;

            if (preloadTime.HasValue)
            {
                StartHours = preloadTime.Value.Hours > 9 ? 9 : preloadTime.Value.Hours;
                StartMinutes = preloadTime.Value.Minutes;
            }

            StartNow = startNow.HasValue && startNow.Value;
        }

        public void SetStartNowEnabled(bool enabled)
        {
            if (enabled)
            {
                StartNowEditable = true;
            }
            else
            {
                StartNow = false;
                StartNowEditable = false;
            }

            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("StartNow"));
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("StartNowEditable"));
        }

        public void SetJiraReference(string jiraRef)
        {
            JiraReference = jiraRef;
            JiraReferenceEditable = false;

            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("JiraReference"));
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("JiraReferenceEditable"));
        }
    }
}