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
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            // Lấy các dịch vụ cần thiết
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // --- TẠO CÁC VAI TRÒ (ROLES) ---
            await SeedRolesAsync(roleManager);

            // --- TẠO TÀI KHOẢN ADMIN MẶC ĐỊNH ---
            await SeedAdminUserAsync(userManager);

            // --- TẠO TÀI KHOẢN GIẢNG VIÊN MẪU ---
            await SeedFacultyUsersAsync(userManager);

            // --- TẠO TÀI KHOẢN NV PHÒNG ĐÀO TẠO MẪU ---
            await SeedDepartmentStaffUserAsync(userManager);

            // --- TẠO DỮ LIỆU KHÓA HỌC MẪU ---
            await SeedCoursesAsync(context);

            // --- TẠO DỮ LIỆU SINH VIÊN MẪU (Bao gồm cả tài khoản đăng nhập) ---
            await SeedStudentsAsync(context, userManager);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "Faculty", "Student", "Department Staff" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            if (await userManager.FindByEmailAsync("admin@sims.com") == null)
            {
                var newAdminUser = new ApplicationUser
                {
                    UserName = "admin@sims.com",
                    Email = "admin@sims.com",
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(newAdminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdminUser, "Admin");
                }
            }
        }

        private static async Task SeedFacultyUsersAsync(UserManager<ApplicationUser> userManager)
        {
            if (await userManager.FindByEmailAsync("teacher1@sims.com") == null)
            {
                var facultyUser1 = new ApplicationUser
                {
                    UserName = "teacher1@sims.com",
                    Email = "teacher1@sims.com",
                    FirstName = "Minh",
                    LastName = "Le",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(facultyUser1, "Faculty@123");
                await userManager.AddToRoleAsync(facultyUser1, "Faculty");
            }

            if (await userManager.FindByEmailAsync("teacher2@sims.com") == null)
            {
                var facultyUser2 = new ApplicationUser
                {
                    UserName = "teacher2@sims.com",
                    Email = "teacher2@sims.com",
                    FirstName = "An",
                    LastName = "Nguyen",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(facultyUser2, "Faculty@123");
                await userManager.AddToRoleAsync(facultyUser2, "Faculty");
            }
        }

        // ==========================================================
        //      PHẦN MỚI ĐƯỢC BỔ SUNG
        // ==========================================================
        private static async Task SeedDepartmentStaffUserAsync(UserManager<ApplicationUser> userManager)
        {
            if (await userManager.FindByEmailAsync("staff1@sims.com") == null)
            {
                var staffUser = new ApplicationUser
                {
                    UserName = "staff1@sims.com",
                    Email = "staff1@sims.com",
                    FirstName = "Phong",
                    LastName = "Dao Tao",
                    EmailConfirmed = true // Tài khoản được duyệt sẵn
                };
                await userManager.CreateAsync(staffUser, "Staff@123");
                await userManager.AddToRoleAsync(staffUser, "Department Staff");
            }
        }


        private static async Task SeedCoursesAsync(ApplicationDbContext context)
        {
            if (context.Courses.Any()) return;
            var courses = new List<Course>
            {
                new Course { CourseCode = "CS101", CourseName = "Introduction to Computer Science", Credits = 4 },
                new Course { CourseCode = "MA202", CourseName = "Calculus II", Credits = 4 },
                new Course { CourseCode = "EN105", CourseName = "English Composition", Credits = 3 },
                new Course { CourseCode = "DB301", CourseName = "Database Systems", Credits = 4 },
                new Course { CourseCode = "NT101", CourseName = "Networking Basics", Credits = 3 },
                new Course { CourseCode = "PRJ401", CourseName = "Software Project Management", Credits = 4 },
                new Course { CourseCode = "AI100", CourseName = "Artificial Intelligence", Credits = 4 },
                new Course { CourseCode = "WE201", CourseName = "Web Development", Credits = 3 },
                new Course { CourseCode = "OS404", CourseName = "Operating Systems", Credits = 4 },
                new Course { CourseCode = "SS301", CourseName = "Soft Skills", Credits = 2 }
            };
            await context.Courses.AddRangeAsync(courses);
            await context.SaveChangesAsync();
        }

        private static async Task SeedStudentsAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (context.Students.Any()) return;

            var studentData = new List<(string FirstName, string LastName, string StudentCode, string Email, DateTime DateOfBirth)>
            {
                ("An", "Nguyen Van", "BH00001", "an.nv@student.com", new DateTime(2003, 1, 15)),
                ("Binh", "Tran Minh", "BH00002", "binh.tm@student.com", new DateTime(2003, 3, 22)),
                ("Chi", "Le Thi", "BH00003", "chi.lt@student.com", new DateTime(2002, 5, 10)),
                ("Dung", "Pham Tien", "BH00004", "dung.pt@student.com", new DateTime(2003, 7, 30)),
                ("Giang", "Hoang Thu", "BH00005", "giang.ht@student.com", new DateTime(2002, 9, 5)),
                ("Hieu", "Ngo Trung", "BH00006", "hieu.nt@student.com", new DateTime(2003, 2, 18)),
                ("Khanh", "Vu Ngoc", "BH00007", "khanh.vn@student.com", new DateTime(2002, 11, 12)),
                ("Linh", "Dang Thuy", "BH00008", "linh.dt@student.com", new DateTime(2003, 4, 25)),
                ("Minh", "Nguyen Quang", "BH00009", "minh.nq@student.com", new DateTime(2002, 8, 8)),
                ("Nam", "Doan Thanh", "BH00010", "nam.dt@student.com", new DateTime(2003, 10, 1))
            };

            var studentsToCreate = new List<Student>();

            foreach (var data in studentData)
            {
                var studentUser = new ApplicationUser
                {
                    UserName = data.Email,
                    Email = data.Email,
                    FirstName = data.FirstName,
                    LastName = data.LastName,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(studentUser, "Student@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(studentUser, "Student");
                    var studentProfile = new Student
                    {
                        FirstName = data.FirstName,
                        LastName = data.LastName,
                        StudentCode = data.StudentCode,
                        Email = data.Email,
                        DateOfBirth = data.DateOfBirth,
                        ApplicationUserId = studentUser.Id
                    };
                    studentsToCreate.Add(studentProfile);
                }
            }

            if (studentsToCreate.Any())
            {
                await context.Students.AddRangeAsync(studentsToCreate);
                await context.SaveChangesAsync();
            }
        }
    }
}