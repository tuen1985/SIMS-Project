namespace SIMS.Domain
{
    public class Grade
    {
        public int Id { get; set; }
        public float? Score { get; set; }
        public string Comments { get; set; } = string.Empty; // Sửa ở đây
        public int EnrollmentId { get; set; }
        public virtual Enrollment Enrollment { get; set; } = default!; // Sửa ở đây
    }
}