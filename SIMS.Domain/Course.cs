using System.ComponentModel.DataAnnotations;

namespace SIMS.Domain
{
    public class Course
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Course Code cannot be empty.")]
        [Display(Name = "Course Code")]
        [StringLength(10, ErrorMessage = "Course Code cannot exceed 10 characters.")]
        public string CourseCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course Name cannot be empty.")]
        [Display(Name = "Course Name")]
        [StringLength(100, ErrorMessage = "Course Name cannot exceed 100 characters.")]
        public string CourseName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Credits cannot be empty.")]
        [Display(Name = "Credits")]
        [Range(1, 10, ErrorMessage = "Credits must be between 1 and 10.")]
        public int Credits { get; set; }
    }
}