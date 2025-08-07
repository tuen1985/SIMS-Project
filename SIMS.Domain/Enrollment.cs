namespace SIMS.Domain
{
    public enum EnrollmentStatus { Pending, Approved, Rejected }

    public class Enrollment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public virtual Student Student { get; set; } = default!; // Sửa ở đây
        public int ClassroomId { get; set; }
        public virtual Classroom Classroom { get; set; } = default!; // Sửa ở đây
        public EnrollmentStatus Status { get; set; }
        public virtual Grade? Grade { get; set; } // Sửa ở đây, thêm dấu ?
    }
}