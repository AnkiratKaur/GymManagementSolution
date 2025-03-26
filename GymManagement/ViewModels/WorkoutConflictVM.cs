using GymManagement.Models;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace GymManagement.ViewModels
{
    public class WorkoutConflictVM
    {
        #region Summary Properties

        [Display(Name = "Date")]
        public string StartDateSummary
        {
            get
            {
                return StartTime?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
        }
        [Display(Name = "Start")]
        public string StartTimeSummary
        {
            get
            {
                return StartTime?.ToString("h:mm tt", CultureInfo.InvariantCulture);
            }
        }

        [Display(Name = "End")]
        public string EndTimeSummary
        {
            get
            {
                if (EndTime == null)
                {
                    return "Unknown";
                }
                else
                {
                    string endtime = EndTime.GetValueOrDefault().ToString("h:mm tt", CultureInfo.InvariantCulture);
                    TimeSpan difference = ((TimeSpan)(EndTime - StartTime));
                    int days = difference.Days;
                    if (days > 0)
                    {
                        return endtime + " (" + days + " day" + (days > 1 ? "s" : "") + " later)";
                    }
                    else
                    {
                        return endtime;
                    }
                }
            }
        }
        [Display(Name = "Duration")]
        public string DurationSummary
        {
            get
            {
                if (EndTime == null)
                {
                    return "";
                }
                else
                {
                    TimeSpan d = ((TimeSpan)(EndTime - StartTime));
                    string duration = "";
                    if (d.Minutes > 0) //Show the minutes if there are any
                    {
                        duration = d.Minutes.ToString() + " min";
                    }
                    if (d.Hours > 0) //Show the hours if there are any
                    {
                        duration = d.Hours.ToString() + " hr" + (d.Hours > 1 ? "s" : "")
                            + (d.Minutes > 0 ? ", " + duration : ""); //Put a ", " between hours and minutes if there are both
                    }
                    if (d.Days > 0) //Show the days if there are any
                    {
                        duration = d.Days.ToString() + " day" + (d.Days > 1 ? "s" : "")
                            + (d.Hours > 0 || d.Minutes > 0 ? ", " + duration : ""); //Put a ", " between days and hours/minutes if there are any
                    }
                    return duration;
                }
            }
        }
        #endregion

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool isConflict { get; set; } = false;
        public string Comment { get; set; } = "";
    }
}
