namespace SIMS.Domain
{
    public enum EnrollmentStatus { Pending, Approved, Rejected }

    public class Enrollment
    {
        public int Id { get; set; }

        // Khóa ngoại đến Student
        public int StudentId { get; set; }
        public virtual Student Student { get; set; }

        // Khóa ngoại đến Classroom
        public int ClassroomId { get; set; }
        public virtual Classroom Classroom { get; set; }

        // Trạng thái đăng ký
        public EnrollmentStatus Status { get; set; }

        // Một bản ghi đăng ký sẽ có một điểm số (hoặc không)
        public virtual Grade Grade { get; set; }
    }
}