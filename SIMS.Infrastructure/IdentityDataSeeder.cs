// File: SIMS.Infrastructure/IdentityDataSeeder.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SIMS.Domain;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SIMS.Infrastructure
{
    public static class IdentityDataSeeder
    {
        public static async Task SeedRolesAndAdminUserAsync(IServiceProvider serviceProvider)
        {
            // Lấy các dịch vụ cần thiết
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // --- TẠO CÁC VAI TRÒ (ROLES) ---
            string[] roleNames = { "Admin", "Faculty", "Student", "Department Staff" };
            foreach (var roleName in roleNames)
            {
                // Kiểm tra xem vai trò đã tồn tại chưa
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // Nếu chưa, tạo vai trò mới
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // --- TẠO TÀI KHOẢN ADMIN MẶC ĐỊNH ---
            // Tìm kiếm xem có user nào có email này chưa
            var adminUser = await userManager.FindByEmailAsync("admin@sims.com");
            if (adminUser == null)
            {
                // Nếu chưa có, tạo một user mới
                var newAdminUser = new ApplicationUser
                {
                    UserName = "admin@sims.com",
                    Email = "admin@sims.com",
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true // Tự động xác thực email cho tiện
                };

                // Tạo user với mật khẩu đã định
                var result = await userManager.CreateAsync(newAdminUser, "Admin@123");

                if (result.Succeeded)
                {
                    // Nếu tạo user thành công, gán vai trò "Admin" cho user đó
                    await userManager.AddToRoleAsync(newAdminUser, "Admin");
                }
            }
        }
    }
}