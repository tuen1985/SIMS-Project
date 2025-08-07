// File: SIMS.Infrastructure/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Thêm using này
using Microsoft.EntityFrameworkCore;
using SIMS.Domain;
using System.Diagnostics;

namespace SIMS.Infrastructure
{
    // Kế thừa từ IdentityDbContext<ApplicationUser>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Các DbSet này không cần nữa vì đã có trong IdentityDbContext
        // public DbSet<Student> Students { get; set; }
        // public DbSet<Course> Courses { get; set; }

        // Tuy nhiên, bạn vẫn cần giữ lại các DbSet cho các domain model khác của bạn
        public DbSet<Student> Students { get; set; } = default!;
        public DbSet<Course> Courses { get; set; } = default!;
        public DbSet<Classroom> Classrooms { get; set; } = default!;
        public DbSet<Enrollment> Enrollments { get; set; } = default!;
        public DbSet<Grade> Grades { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Rất quan trọng! Phải gọi phương thức của lớp cha
            // Các cấu hình Fluent API khác của bạn sẽ ở đây
        }
    }
}