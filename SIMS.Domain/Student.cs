using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using SIMS.Domain.ValidationAttributes;
using System.ComponentModel.DataAnnotations.Schema; // Dòng using quan trọng cho [ForeignKey]

namespace SIMS.Domain
{
    public class Student
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Student Code cannot be empty.")]
        [Display(Name = "Student Code")]
        [RegularExpression(@"^BH\d{5}$", ErrorMessage = "Student Code must be in the format BHxxxxx (e.g., BH12345).")]
        public string StudentCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "First Name cannot be empty.")]
        [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name cannot be empty.")]
        [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of Birth cannot be empty.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        [ValidDateOfBirth(minAge: 17, maxAge: 100)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Email cannot be empty.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        // Trong file Domain/Student.cs
        public string FullName => $"{LastName} {FirstName}";

        // ==========================================================
        //      THUỘC TÍNH MỚI ĐỂ LIÊN KẾT VỚI TÀI KHOẢN USER
        // ==========================================================

        // Khóa ngoại đến bảng AspNetUsers
        public string? ApplicationUserId { get; set; }

        // Thuộc tính điều hướng (Navigation Property) để EF Core hiểu mối quan hệ
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser? ApplicationUser { get; set; }
    }
}
