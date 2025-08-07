namespace SIMS.WebApp.ViewModels
{
    public class GradeUpdateViewModel
    {
        public int EnrollmentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public float? Score { get; set; }
    }
}