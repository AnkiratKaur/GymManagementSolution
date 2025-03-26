using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace GymManagement.Models
{
    public class InstructorDocument : UploadedFile
    {
        [Display(Name = "Instructor")]
        public int InstructorID { get; set; }

        public Instructor? Instructor { get; set; }
    }
}
