using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SIMS.WebApp.ViewModels
{
    public class ClassroomCreateViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Please enter the class name.")]
        [Display(Name = "Class Name")]
        public string ClassName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the semester.")]
        [Display(Name = "Semester")]
        public string Semester { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a course.")]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Please select a faculty member.")]
        [Display(Name = "Faculty")]
        public string FacultyId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a start date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Please enter an end date.")]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        public IEnumerable<SelectListItem> Courses { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Faculties { get; set; } = Enumerable.Empty<SelectListItem>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Check if EndDate is earlier than StartDate
            if (EndDate < StartDate)
            {
                // Return a validation error for the EndDate property
                yield return new ValidationResult(
                    "The end date cannot be earlier than the start date.",
                    new[] { nameof(EndDate) });
            }
        }
    }
}