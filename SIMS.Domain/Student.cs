using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using SIMS.Domain.ValidationAttributes;

namespace SIMS.Domain
{
    public class Student
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã sinh viên không được để trống.")]
        [Display(Name = "Mã sinh viên")]
        [RegularExpression(@"^BH\d{5}$", ErrorMessage = "Mã sinh viên phải có định dạng BHxxxxx (ví dụ: BH12345).")]
        public string StudentCode { get; set; }

        [Required(ErrorMessage = "Tên không được để trống.")]
        [StringLength(50, ErrorMessage = "Tên không được vượt quá 50 ký tự.")]
        [Display(Name = "Tên")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Họ không được để trống.")]
        [StringLength(50, ErrorMessage = "Họ không được vượt quá 50 ký tự.")]
        [Display(Name = "Họ")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Ngày sinh không được để trống.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        [ValidDateOfBirth(minAge: 17, maxAge: 100)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}