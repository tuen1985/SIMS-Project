using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SIMS.Domain
{
    public class Classroom
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter the class name.")]
        [Display(Name = "Class Name")]
        public string ClassName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the semester.")]
        [Display(Name = "Semester")]
        public string Semester { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a course.")]
        [Display(Name = "Course")]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; } = default!;

        [Required(ErrorMessage = "Please select a faculty member.")]
        [Display(Name = "Faculty")]
        public string FacultyId { get; set; } = string.Empty;
        public virtual ApplicationUser Faculty { get; set; } = default!;

        [Required(ErrorMessage = "Please enter a start date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Please enter an end date.")]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}