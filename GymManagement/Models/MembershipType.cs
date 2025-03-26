using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models
{
    public class MembershipType
    {
        public int ID { get; set; }

        [Display(Name = "Mem. Type")]
        public string Summary
        {
            get
            {
                return Type + " (Std. Fee: " + StandardFee.ToString("c") + ")";
            }
        }

        [Required(ErrorMessage = "You cannot leave the type of membership blank.")]
        [StringLength(50, ErrorMessage = "Membership type cannot be more than 50 characters long.")]
        public string Type { get; set; } = "";

        [Display(Name = "Standard Fee")]
        [Required(ErrorMessage = "You must enter the standard fee for the membership type.")]
        [DataType(DataType.Currency)]
        public double StandardFee { get; set; }

        public ICollection<Client> Clients { get; set; } = new HashSet<Client>();
    }
}
