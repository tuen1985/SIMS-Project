using System.ComponentModel.DataAnnotations; // Thêm dòng này

namespace SIMS.WebApp.ViewModels
{
    public class GradeUpdateViewModel
    {
        public int EnrollmentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Score must be between 0 and 100.")]
        public float? Score { get; set; }
    }
}