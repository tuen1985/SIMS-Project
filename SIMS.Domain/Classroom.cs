using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SIMS.Domain
{
    public class Classroom
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên lớp học.")]
        [Display(Name = "Tên Lớp học")]
        public string ClassName { get; set; } = string.Empty; // Sửa ở đây

        [Required(ErrorMessage = "Vui lòng nhập học kỳ.")]
        [Display(Name = "Học kỳ")]
        public string Semester { get; set; } = string.Empty; // Sửa ở đây

        [Required(ErrorMessage = "Vui lòng chọn một khóa học.")]
        [Display(Name = "Khóa học")]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; } = default!; // Sửa ở đây

        [Required(ErrorMessage = "Vui lòng chọn một giảng viên.")]
        [Display(Name = "Giảng viên")]
        public string FacultyId { get; set; } = string.Empty; // Sửa ở đây
        public virtual ApplicationUser Faculty { get; set; } = default!; // Sửa ở đây

        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}