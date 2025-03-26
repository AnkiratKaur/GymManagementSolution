using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models
{
    public class UploadedFile
    {
        public int ID { get; set; }

        [StringLength(255, ErrorMessage = "The name of the file cannot be more than 255 characters.")]
        [Display(Name = "File Name")]
        public string? FileName { get; set; }

        [StringLength(255)]
        public string? MimeType { get; set; }

        [StringLength(255, ErrorMessage = "The description cannot be more than 255 characters.")]
        public string? Description { get; set; }

        public FileContent? FileContent { get; set; } = new FileContent();
    }
}
