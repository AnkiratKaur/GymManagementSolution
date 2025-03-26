using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Models
{
    public class ClassTime
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        [Required(ErrorMessage = "Class start time is required.")]
        [Display(Name = "Start Time")]
        [StringLength(8)]
        public string StartTime { get; set; } = "";

        public ICollection<GroupClass> GroupClasses { get; set; } = new HashSet<GroupClass>();
    }
}
