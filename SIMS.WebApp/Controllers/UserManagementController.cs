// File: SIMS.WebApp/Controllers/UserManagementController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SIMS.Domain;
using SIMS.Infrastructure;
using SIMS.WebApp.ViewModels;
using System;
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

    public async Task<IActionResult> Index(string roleFilter)
    {
        ViewBag.CurrentFilter = roleFilter;
        var unifiedList = new List<UnifiedUserViewModel>();

        var emailList = await _userManager.Users.Where(u => u.Email != null).Select(u => u.Email!).ToListAsync();
        var emailsWithAccounts = new HashSet<string>(emailList);

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
                HasAccount = true,
                IsBlocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow
            });
        }

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
                HasAccount = false,
                IsBlocked = false
            });
        }

        if (!string.IsNullOrEmpty(roleFilter))
        {
            if (roleFilter == "Blocked Accounts")
            {
                unifiedList = unifiedList.Where(u => u.IsBlocked).ToList();
            }
            else
            {
                unifiedList = unifiedList.Where(u => u.Role == roleFilter).ToList();
            }
        }

        return View(unifiedList.OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BlockUser(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return NotFound();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || await _userManager.IsInRoleAsync(user, "Admin"))
        {
            return RedirectToAction(nameof(Index));
        }

        if (await _userManager.IsInRoleAsync(user, "Faculty"))
        {
            var classrooms = await _context.Classrooms.Where(c => c.FacultyId == userId).ToListAsync();
            if (classrooms.Any())
            {
                var classroomNames = string.Join(", ", classrooms.Select(c => c.ClassName));
                TempData["CannotBlockMessage"] = $"Cannot block faculty member '{user.FullName}' because they are assigned to classes: {classroomNames}. Please re-assign these classes first.";
                return RedirectToAction(nameof(Index));
            }
        }

        if (await _userManager.IsInRoleAsync(user, "Student"))
        {
            var studentProfile = await _context.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
            if (studentProfile != null)
            {
                var enrollments = await _context.Enrollments.Include(e => e.Classroom).Where(e => e.StudentId == studentProfile.Id).ToListAsync();
                if (enrollments.Any())
                {
                    var classroomNames = string.Join(", ", enrollments.Select(e => e.Classroom.ClassName));
                    TempData["CannotBlockMessage"] = $"Cannot block student '{studentProfile.FullName}' because they are enrolled in classes: {classroomNames}. Please un-enroll the student first.";
                    return RedirectToAction(nameof(Index));
                }
            }
        }

        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnblockUser(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return NotFound();

        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
        }

        return RedirectToAction(nameof(Index));
    }

    #region Other Actions
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

    // ĐÃ XÓA 2 ACTION ManageRoles (GET và POST) TẠI ĐÂY

    public async Task<IActionResult> Create()
    {
        var roles = await _roleManager.Roles.Where(r => r.Name != "Admin").ToListAsync();
        var viewModel = new CreateUserViewModel
        {
            RolesList = new SelectList(roles, "Name", "Name")
        };
        return View(viewModel);
    }

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
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.SelectedRole);
                if (model.SelectedRole == "Student")
                {
                    var studentProfile = new Student
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        StudentCode = $"BH{new Random().Next(10000, 99999)}",
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

        var roles = await _roleManager.Roles.Where(r => r.Name != "Admin").ToListAsync();
        model.RolesList = new SelectList(roles, "Name", "Name");
        return View(model);
    }
    #endregion
}