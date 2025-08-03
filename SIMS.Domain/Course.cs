using System.ComponentModel.DataAnnotations;

namespace SIMS.Domain
{
    public class Course
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã khóa học không được để trống.")]
        [Display(Name = "Mã khóa học")]
        [StringLength(10, ErrorMessage = "Mã khóa học không được vượt quá 10 ký tự.")]
        public string CourseCode { get; set; }

        [Required(ErrorMessage = "Tên khóa học không được để trống.")]
        [Display(Name = "Tên khóa học")]
        [StringLength(100, ErrorMessage = "Tên khóa học không được vượt quá 100 ký tự.")]
        public string CourseName { get; set; }

        [Required(ErrorMessage = "Số tín chỉ không được để trống.")]
        [Display(Name = "Số tín chỉ")]
        [Range(1, 10, ErrorMessage = "Số tín chỉ phải từ 1 đến 10.")]
        public int Credits { get; set; }
    }
}