using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models
{
    public class GroupClass : Auditable
    {
        public int ID { get; set; }

        #region Summary Properties
        [Display(Name = "Class")]
        public string Summary
        {
            get
            {
                string summary; ;
                if (FitnessCategory != null && ClassTime != null)
                {
                    summary = FitnessCategory.Category + " - " + DOW.ToString() + " " + ClassTime.StartTime;
                }
                else
                {
                    summary = "Class - " + DOW.ToString() + " " + ClassTimeID.ToString() + ":00 Hrs";
                }
                return summary;
            }
        }

        [Display(Name = "Description")]
        public string ShortDescription => Description.Length > 20 ? Description.Substring(0, 20) + "..." : Description;
        #endregion

        [Display(Name = "Full Description")]
        [Required(ErrorMessage = "You cannot leave the class description blank.")]
        [MaxLength(200, ErrorMessage = "Description cannot be more than 200 characters long.")]
        [MinLength(10, ErrorMessage = "The description must be at least 10 characters long.")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = "";

        [Required(ErrorMessage = "You must select Day of Week for this scheduled class!")]
        [Display(Name = "Day of Week")]
        public DOW DOW { get; set; }

        [Required(ErrorMessage = "You must select the Fitness Category of the class.")]
        [Display(Name = "Fitness Category")]
        public int FitnessCategoryID { get; set; }

        [Display(Name = "Fitness Category")]
        public FitnessCategory? FitnessCategory { get; set; }

        [Required(ErrorMessage = "You must select the Instructor leading the class.")]
        [Display(Name = "Instructor")]
        public int InstructorID { get; set; }

        public Instructor? Instructor { get; set; }

        [Required(ErrorMessage = "You must select the time the scheduled class starts.")]
        [Display(Name = "Start Time")]
        public int ClassTimeID { get; set; }

        [Display(Name = "Start Time")]
        public ClassTime? ClassTime { get; set; }

        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[]? RowVersion { get; set; }//Added for concurrency
        public ICollection<Enrollment> Enrollments { get; set; } = new HashSet<Enrollment>();
    }
}
