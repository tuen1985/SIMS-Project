using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq; // Thêm using này

namespace SIMS.WebApp.ViewModels
{
    public class ClassroomCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên lớp học.")]
        [Display(Name = "Tên Lớp học")]
        public string ClassName { get; set; } = string.Empty; // Sửa ở đây

        [Required(ErrorMessage = "Vui lòng nhập học kỳ.")]
        [Display(Name = "Học kỳ")]
        public string Semester { get; set; } = string.Empty; // Sửa ở đây

        [Required(ErrorMessage = "Vui lòng chọn một khóa học.")]
        [Display(Name = "Khóa học")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn một giảng viên.")]
        [Display(Name = "Giảng viên")]
        public string FacultyId { get; set; } = string.Empty; // Sửa ở đây

        public IEnumerable<SelectListItem> Courses { get; set; } = Enumerable.Empty<SelectListItem>(); // Sửa ở đây
        public IEnumerable<SelectListItem> Faculties { get; set; } = Enumerable.Empty<SelectListItem>(); // Sửa ở đây
    }
}