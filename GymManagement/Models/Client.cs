using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models
{
    public class Client : Auditable, IValidatableObject
    {
        public int ID { get; set; }

        #region Summary Properties

        [Display(Name = "Client")]
        public string Summary
        {
            get
            {
                string FullName = FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? " " :
                        (" " + (char?)MiddleName[0] + ". ").ToUpper())
                    + LastName;
                return FullName + Membership;
            }
        }

        [Display(Name = "Client")]
        public string FormalName
        {
            get
            {
                return LastName + ", " + FirstName
                    + (String.IsNullOrEmpty(MiddleName) ? "" :
                        (" " + (char?)MiddleName[0] + ".").ToUpper());
            }
        }

        [Display(Name = "Full Name")]
        public string FullFormalName
        {
            get
            {
                return LastName + ", " + FirstName
                    + (String.IsNullOrEmpty(MiddleName) ? "" :
                        (" " + MiddleName));
            }
        }
        public string Age
        {
            get
            {
                DateTime today = DateTime.Today;
                int a = today.Year - DOB.Year
                    - ((today.Month < DOB.Month ||
                        (today.Month == DOB.Month && today.Day < DOB.Day) ? 1 : 0));
                return a.ToString();
            }
        }

        [Display(Name = "Mem. Status")]
        public string MembershipStatus
        {
            get
            {
                DateTime today = DateTime.Today;
                if (MembershipEndDate <= today)
                {
                    return "Expired";
                }
                int totalMonths = ((MembershipEndDate.Year - today.Year) * 12) + MembershipEndDate.Month - today.Month;
                DateTime tempDate = today.AddMonths(totalMonths);

                if (tempDate > MembershipEndDate)
                {
                    totalMonths--;
                    tempDate = today.AddMonths(totalMonths);
                }

                int days = (MembershipEndDate - tempDate).Days;

                return "Exp. in " + $"{totalMonths} Months, {days} Days";
            }
        }

        [Display(Name = "Mem.Type")]
        public string Membership => (String.IsNullOrEmpty(MembershipType?.Type)) ? ""
                    : " - " + MembershipType?.Type + " Mem.";

        [Display(Name = "Age (DOB)")]
        public string AgeSummary => Age + " (" + DOB.ToString("yyyy-MM-dd") + ")";

        [Display(Name = "Phone")]
        public string PhoneFormatted => "(" + Phone.Substring(0, 3) + ") "
            + Phone.Substring(3, 3) + "-" + Phone[6..];

        #endregion

        [Display(Name = "Mem. No.")]
        [Required(ErrorMessage = "You cannot leave the membership number blank.")]
        [Range(10000, 99999, ErrorMessage = "The membership number must be between 10,000 and 99,999.")]
        public int MembershipNumber { get; set; }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "You cannot leave the first name blank.")]
        [MaxLength(50, ErrorMessage = "First name cannot be more than 50 characters long.")]
        public string FirstName { get; set; } = "";

        [Display(Name = "Middle Name")]
        [MaxLength(50, ErrorMessage = "Middle name cannot be more than 50 characters long.")]
        public string? MiddleName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "You cannot leave the last name blank.")]
        [MaxLength(100, ErrorMessage = "Last name cannot be more than 100 characters long.")]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid 10-digit phone number (no spaces).")]
        [DataType(DataType.PhoneNumber)]
        [MaxLength(10)]
        public string Phone { get; set; } = "";

        [Required(ErrorMessage = "Email address is required.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please follow the correct email format test@email.com")]
        [StringLength(255)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = "";

        [Display(Name = "Date of Birth")]
        [Required(ErrorMessage = "Date of Birth is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DOB { get; set; }

        [Display(Name = "Postal Code")]
        [Required]
        [RegularExpression(@"^[A-Za-z]\d[A-Za-z][ -]?\d[A-Za-z]\d$", ErrorMessage = "Invalid postal code.")]
        public string PostalCode { get; set; } = "";

        [Display(Name = "Health Condition")]
        [Required(ErrorMessage = "You must enter comments about the client's health condition.")]
        [MaxLength(255, ErrorMessage = "Limit of 255 characters for health condition.")]
        [DataType(DataType.MultilineText)]
        public string HealthCondition { get; set; } = "";

        [MaxLength(2000, ErrorMessage = "Limit of 2000 characters for notes.")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; } = "";

        [Display(Name = "Mem. Start")]
        [Required(ErrorMessage = "Membership start date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime MembershipStartDate { get; set; } = DateTime.Today;

        [Display(Name = "Mem. End")]
        [Required(ErrorMessage = "Membership end date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime MembershipEndDate { get; set; } = DateTime.Today.AddYears(1);

        [Display(Name = "Mem. Fee")]
        [Required(ErrorMessage = "You must enter the membership fee.")]
        [DataType(DataType.Currency)]
        public double MembershipFee { get; set; } = 100d;

        [Display(Name = "Mem. Fee Paid")]
        public bool FeePaid { get; set; } = false;

        public ClientPhoto? ClientPhoto { get; set; }
        public ClientThumbnail? ClientThumbnail { get; set; }

        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[]? RowVersion { get; set; }//Added for concurrency

        [Display(Name = "Mem. Type")]
        [Required(ErrorMessage = "You must select the membership type.")]
        public int MembershipTypeID { get; set; }

        [Display(Name = "Membership Type")]
        public MembershipType? MembershipType { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new HashSet<Enrollment>();

        public ICollection<Workout> Workouts { get; set; } = new HashSet<Workout>();


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DOB > DateTime.Today.AddYears(-16))
            {
                yield return new ValidationResult("Client must be at least 16 years old.", ["DOB"]);
            }
            else if (DOB < DateTime.Today.AddYears(-100))
            {
                yield return new ValidationResult("Client cannot be over 100 years old.", ["DOB"]);
            }

            if (MembershipEndDate < MembershipStartDate)
            {
                yield return new ValidationResult("Membership end date cannot be earlier than the start date.", ["MembershipEndDate"]);
            }
            else if (MembershipEndDate > DateTime.Today.AddYears(5))
            {
                yield return new ValidationResult("Membership end date cannot be more than 5 years in the future.", ["MembershipEndDate"]);
            }

            //For fun, if we have access to the MembershipType object and the 
            //fee has not been paid yet, warn the user if the fee does match the
            //standard fee for the membership type.
            if (!FeePaid && MembershipType?.StandardFee != MembershipFee)
            {
                yield return new ValidationResult("Note that the fee does not match the " +
                    "standard fee of: " + MembershipType?.StandardFee.ToString("c"), ["MembershipFee"]);
            }
        }
    }
}
