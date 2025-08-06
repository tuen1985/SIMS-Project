using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SIMS.Domain
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        // Thuộc tính điều hướng để tạo mối quan hệ một-một với Student
        // Điều này cho biết một tài khoản người dùng có thể liên kết với một hồ sơ sinh viên
        public virtual Student? Student { get; set; }
    }
}
