using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models
{
    public class Exercise
    {
        public int ID { get; set; }

        [Display(Name = "Exercise")]
        public string Summary
        {
            get
            {
                int howMany = ExerciseCategories.Count();
                if (howMany > 1)
                {
                    return Name + " (Multiple Categories)";
                }
                else if (howMany == 1)
                {
                    var firstCategory = ExerciseCategories.FirstOrDefault();
                    return Name + " (" + firstCategory?.FitnessCategory?.Category + ")";
                }
                else
                {
                    return Name;
                }
            }
        }

        [Display(Name = "Exercise")]
        [Required(ErrorMessage = "You cannot leave the exercise name blank.")]
        [StringLength(50, ErrorMessage = "Exercise name cannot be more than 50 characters long.")]
        public string Name { get; set; } = "";

        [Display(Name = "Fitness Categories")]
        public ICollection<ExerciseCategory> ExerciseCategories { get; set; } = new HashSet<ExerciseCategory>();

        [Display(Name = "Workouts")]
        public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = new HashSet<WorkoutExercise>();
    }
}
