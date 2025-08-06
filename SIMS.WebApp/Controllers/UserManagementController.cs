// File: SIMS.WebApp/Controllers/UserManagementController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.Domain;
using SIMS.WebApp.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMS.WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Action Index không thay đổi
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();
            foreach (var user in users)
            {
                var thisViewModel = new UserRolesViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? "N/A",
                    UserName = user.UserName ?? "N/A",
                    FirstName = user.FirstName ?? "N/A",
                    LastName = user.LastName ?? "N/A",
                    Roles = await _userManager.GetRolesAsync(user)
                };
                userRolesViewModel.Add(thisViewModel);
            }
            return View(userRolesViewModel);
        }

        // ==========================================================
        //      ACTION MỚI: HIỂN THỊ DANH SÁCH CHỜ DUYỆT
        // ==========================================================
        public async Task<IActionResult> PendingApprovals()
        {
            // Lấy tất cả user có EmailConfirmed = false
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

        // ==========================================================
        //      ACTION MỚI: XỬ LÝ DUYỆT TÀI KHOẢN
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Đặt EmailConfirmed = true để kích hoạt tài khoản
            user.EmailConfirmed = true;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                // Xử lý lỗi nếu cần
            }

            return RedirectToAction(nameof(PendingApprovals));
        }


        // Các action ManageRoles không thay đổi
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
}
