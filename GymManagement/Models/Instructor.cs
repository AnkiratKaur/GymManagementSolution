using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models
{
    public class Instructor : Auditable,IValidatableObject
    {
        public int ID { get; set; }

        #region Summary Properties
        [Display(Name = "Instructor")]
        [DisplayFormat(NullDisplayText = "None")]
        public string Summary
        {
            get
            {
                return FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? " " :
                        (" " + (char?)MiddleName[0] + ". ").ToUpper())
                    + LastName;
            }
        }

        public string FormalName
        {
            get
            {
                return LastName + ", " + FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? "" :
                        (" " + (char?)MiddleName[0] + ".").ToUpper());
            }
        }

        public string Seniority
        {
            get
            {
                DateTime today = DateTime.Today;
                int s = today.Year - HireDate.Year
                    - ((today.Month < HireDate.Month ||
                        (today.Month == HireDate.Month && today.Day < HireDate.Day) ? 1 : 0));
                return s + " Yrs.".ToString();
            }
        }

        [Display(Name = "Seniority (Hired)")]
        public string SenioritySummary => Seniority + " (" + HireDate.ToString("yyyy-MM-dd") + ")";

        [Display(Name = "Phone")]
        public string PhoneFormatted => "(" + Phone.Substring(0, 3) + ") "
            + Phone.Substring(3, 3) + "-" + Phone[6..];

        #endregion

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "You cannot leave the first name blank.")]
        [StringLength(50, ErrorMessage = "First name cannot be more than 50 characters long.")]
        public string FirstName { get; set; } = "";

        [Display(Name = "Middle Name")]
        [StringLength(50, ErrorMessage = "Middle name cannot be more than 50 characters long.")]
        public string? MiddleName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "You cannot leave the last name blank.")]
        [StringLength(100, ErrorMessage = "Last name cannot be more than 100 characters long.")]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "You must enter the date the Instructor was hired.")]
        [Display(Name = "Hired")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]

        public DateTime HireDate { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid 10-digit phone number (no spaces).")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(10)]
        public string Phone { get; set; } = "";

        [Required(ErrorMessage = "Email address is required.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please follow the correct email format test@email.com")]
        [StringLength(255)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = "";

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = false;

        [Display(Name = "Group Classes")]
        public ICollection<GroupClass> GroupClasses { get; set; } = new HashSet<GroupClass>();

        public ICollection<Workout> Workouts { get; set; } = new HashSet<Workout>();

        [Display(Name = "Documents")]
        public ICollection<InstructorDocument> InstructorDocuments { get; set; } = new HashSet<InstructorDocument>();
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // The second argument(memberNames) is a IEnumerable < string > that
            //identifies which property has a validation error.
            //If the error is with the entire object, use an empty string or
            //leave the argument out and the messge will display in the validation summary.
            if (HireDate < DateTime.Parse("2018-01-01"))
            {
                yield return new ValidationResult("Hire Date cannot be before the Gym opened for business.", ["HireDate"]);
            }
            else if (HireDate > DateTime.Today.AddMonths(1))
            {
                yield return new ValidationResult("Hire Date cannot be more than one month in the furure.", ["HireDate"]);
            }
        }
    }
}
