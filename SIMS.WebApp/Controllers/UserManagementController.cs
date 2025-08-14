// File: SIMS.WebApp/Controllers/UserManagementController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SIMS.Domain;
using SIMS.Infrastructure;
using SIMS.WebApp.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")]
public class UserManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public UserManagementController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    // File: Controllers/UserManagementController.cs

    // Action Index được sửa lại logic chống trùng lặp
    public async Task<IActionResult> Index(string roleFilter)
    {
        ViewBag.CurrentFilter = roleFilter;
        var unifiedList = new List<UnifiedUserViewModel>();

        // Lấy email của TẤT CẢ người dùng đã có tài khoản (ĐÃ SỬA LỖI)
        var emailList = await _userManager.Users.Where(u => u.Email != null).Select(u => u.Email!).ToListAsync();
        var emailsWithAccounts = new HashSet<string>(emailList);

        // 1. Lấy thông tin từ các tài khoản đã có
        var usersWithAccounts = await _userManager.Users.ToListAsync();
        foreach (var user in usersWithAccounts)
        {
            var roles = await _userManager.GetRolesAsync(user);
            unifiedList.Add(new UnifiedUserViewModel
            {
                UserId = user.Id,
                FirstName = user.FirstName ?? "N/A",
                LastName = user.LastName ?? "N/A",
                Email = user.Email ?? "N/A",
                Role = roles.FirstOrDefault() ?? "N/A",
                AccountStatus = "Đã có tài khoản"
            });
        }

        // 2. Lấy các hồ sơ sinh viên chưa có tài khoản
        var studentsWithoutAccounts = await _context.Students
            .Where(s => !emailsWithAccounts.Contains(s.Email))
            .ToListAsync();

        foreach (var student in studentsWithoutAccounts)
        {
            unifiedList.Add(new UnifiedUserViewModel
            {
                UserId = null,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                Role = "Student",
                AccountStatus = "Chưa có tài khoản"
            });
        }

        // 3. Lọc danh sách nếu có yêu cầu
        if (!string.IsNullOrEmpty(roleFilter))
        {
            unifiedList = unifiedList.Where(u => u.Role == roleFilter).ToList();
        }

        return View(unifiedList.OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList());
    }

    // Các Action khác giữ nguyên...
    public async Task<IActionResult> PendingApprovals()
    {
        var pendingUsers = await _userManager.Users.Where(u => !u.EmailConfirmed).ToListAsync();
        var userRolesViewModel = new List<UserRolesViewModel>();
        foreach (var user in pendingUsers)
        {
            userRolesViewModel.Add(new UserRolesViewModel
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = await _userManager.GetRolesAsync(user)
            });
        }
        return View(userRolesViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveUser(string userId)
    {
        if (string.IsNullOrEmpty(userId)) { return NotFound(); }
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) { return NotFound(); }
        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);
        return RedirectToAction(nameof(PendingApprovals));
    }

    public async Task<IActionResult> ManageRoles(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();
        if (await _userManager.IsInRoleAsync(user, "Admin")) return RedirectToAction("Index");
        var userRoles = await _userManager.GetRolesAsync(user);
        var viewModel = new ManageUserRolesViewModel
        {
            UserId = user.Id,
            UserName = user.UserName ?? "N/A",
            SelectedRole = userRoles.FirstOrDefault() ?? string.Empty,
            Roles = await _roleManager.Roles.Where(r => r.Name != "Admin").Select(r => r.Name ?? "").ToListAsync()
        };
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManageRoles(ManageUserRolesViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null) return NotFound();
        if (await _userManager.IsInRoleAsync(user, "Admin")) return RedirectToAction("Index");
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!string.IsNullOrEmpty(model.SelectedRole) && model.SelectedRole != "Admin")
        {
            await _userManager.AddToRoleAsync(user, model.SelectedRole);
        }
        return RedirectToAction("Index");
    }

    // GET: /UserManagement/Create
    public async Task<IActionResult> Create()
    {
        var roles = await _roleManager.Roles.Where(r => r.Name != "Admin").ToListAsync();
        var viewModel = new CreateUserViewModel
        {
            RolesList = new SelectList(roles, "Name", "Name")
        };
        return View(viewModel);
    }

    // POST: /UserManagement/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                EmailConfirmed = true // Admin tạo thì duyệt luôn
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.SelectedRole);

                // Nếu tạo sinh viên, đồng thời tạo hồ sơ sinh viên
                if (model.SelectedRole == "Student")
                {
                    var studentProfile = new Student
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        StudentCode = $"BH{new Random().Next(10000, 99999)}", // Tạo mã SV ngẫu nhiên
                        DateOfBirth = System.DateTime.Now.AddYears(-18),
                        ApplicationUserId = user.Id
                    };
                    _context.Students.Add(studentProfile);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // Nếu thất bại, tải lại danh sách vai trò
        var roles = await _roleManager.Roles.Where(r => r.Name != "Admin").ToListAsync();
        model.RolesList = new SelectList(roles, "Name", "Name");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            // Ngăn không cho xóa tài khoản Admin
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction("Index");
            }

            // === KIỂM TRA NẾU LÀ GIẢNG VIÊN VÀ CÓ LỚP HỌC ===
            if (await _userManager.IsInRoleAsync(user, "Faculty"))
            {
                var classrooms = await _context.Classrooms
                    .Where(c => c.FacultyId == userId)
                    .ToListAsync();
                if (classrooms.Any())
                {
                    var classroomNames = string.Join(", ", classrooms.Select(c => c.ClassName));
                    TempData["CannotDeleteMessage"] = $"Cannot delete faculty member '{user.FullName}' because they are assigned to the following classes: {classroomNames}. Please re-assign these classes to another faculty member or delete them first.";
                    return RedirectToAction("Index");
                }
            }
            // === KẾT THÚC LOGIC KIỂM TRA CHO GIẢNG VIÊN ===

            // === BẮT ĐẦU LOGIC MỚI: KIỂM TRA CHO SINH VIÊN ===
            if (await _userManager.IsInRoleAsync(user, "Student"))
            {
                var studentProfile = await _context.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
                if (studentProfile != null)
                {
                    var enrollments = await _context.Enrollments
                        .Include(e => e.Classroom)
                        .Where(e => e.StudentId == studentProfile.Id)
                        .ToListAsync();

                    if (enrollments.Any())
                    {
                        var classroomNames = string.Join(", ", enrollments.Select(e => e.Classroom.ClassName));
                        TempData["CannotDeleteMessage"] = $"Cannot delete student '{studentProfile.FullName}' because they are enrolled in the following classes: {classroomNames}. Please un-enroll the student from these classes first.";
                        return RedirectToAction("Index");
                    }
                    // Nếu không có đăng ký, thì xóa hồ sơ sinh viên
                    _context.Students.Remove(studentProfile);
                    await _context.SaveChangesAsync();
                }
            }
            // === KẾT THÚC LOGIC MỚI ===

            // Cuối cùng, xóa tài khoản người dùng
            await _userManager.DeleteAsync(user);
        }

        return RedirectToAction("Index");
    }
}