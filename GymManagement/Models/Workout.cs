using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models
{
    public class Workout : IValidatableObject
    {
        public int ID { get; set; }

        #region Summary Properties

        [Display(Name = "Date")]
        public string StartDateSummary
        {
            get
            {
                return StartTime.ToString("yyyy-MM-dd");
            }
        }
        [Display(Name = "Start")]
        public string StartTimeSummary
        {
            get
            {
                return StartTime.ToString("h:mm tt");
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
                    string endtime = EndTime.GetValueOrDefault().ToString("h:mm tt");
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


        [Required(ErrorMessage = "You must enter a start date and time for the workout.")]
        [Display(Name = "Start")]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [Display(Name = "End")]
        [DataType(DataType.DateTime)]
        public DateTime? EndTime { get; set; }

        [StringLength(2000, ErrorMessage = "Only 2000 characters for notes.")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; } = "";

        [Required(ErrorMessage = "You must select the Client.")]
        [Display(Name = "Client")]
        public int ClientID { get; set; }
        public Client? Client { get; set; }

        [Display(Name = "Instructor")]
        public int? InstructorID { get; set; }
        public Instructor? Instructor { get; set; }

        [Display(Name = "Exercises")]
        public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = new HashSet<WorkoutExercise>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndTime < StartTime)
            {
                yield return new ValidationResult("Workout cannot end before it starts.", new[] { "EndTime" });
            }
        }
    }
}
