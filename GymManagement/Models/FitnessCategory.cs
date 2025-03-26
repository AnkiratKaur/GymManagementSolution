using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models
{
    public class FitnessCategory : Auditable
    {
        public int ID { get; set; }

        [Display(Name = "Category")]
        public string Summary => (ExerciseCategories.Count()==0) ? Category
            : Category + " (" + ExerciseCategories.Count() + " Exercises)";

        [Required(ErrorMessage = "You cannot leave the category name blank.")]
        [StringLength(50, ErrorMessage = "Category name cannot be more than 50 characters long.")]
        public string Category { get; set; } = "";

        [Display(Name = "Group Classes")]
        public ICollection<GroupClass> GroupClasses { get; set; } = new HashSet<GroupClass>();

        [Display(Name = "Exercises")]
        public ICollection<ExerciseCategory> ExerciseCategories { get; set; } = new HashSet<ExerciseCategory>();

    }
}
