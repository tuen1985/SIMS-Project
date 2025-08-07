// File: SIMS.WebApp/Controllers/UserManagementController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
}