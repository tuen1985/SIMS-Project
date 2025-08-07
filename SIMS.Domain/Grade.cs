namespace SIMS.Domain
{
    public class Grade
    {
        public int Id { get; set; }
        public float? Score { get; set; } // Dùng float? để cho phép điểm trống
        public string Comments { get; set; } // Ghi chú của giảng viên

        // Khóa ngoại một-một đến Enrollment
        public int EnrollmentId { get; set; }
        public virtual Enrollment Enrollment { get; set; }
    }
}