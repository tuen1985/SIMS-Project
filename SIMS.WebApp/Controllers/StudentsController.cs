using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.Application.Repositories;
using SIMS.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SIMS.Infrastructure; // This using statement is necessary to access ApplicationDbContext

namespace SIMS.WebApp.Controllers
{
    [Authorize(Roles = "Admin, Department Staff")]
    public class StudentsController : Controller
    {
        private readonly IStudentRepository _studentRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context; // Add ApplicationDbContext here

        // Update the constructor to include ApplicationDbContext
        public StudentsController(IStudentRepository studentRepository, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _studentRepository = studentRepository;
            _userManager = userManager;
            _context = context; // Initialize the context
        }

        // GET: Students
        public async Task<IActionResult> Index()
        {
            return View(await _studentRepository.GetAllAsync());
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _studentRepository.GetByIdAsync(id.Value);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            if (ModelState.IsValid)
            {
                await _studentRepository.AddAsync(student);
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _studentRepository.GetByIdAsync(id.Value);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _studentRepository.UpdateAsync(student);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _studentRepository.GetByIdAsync(id) == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Delete/5
        [Authorize(Roles = "Admin")] // Restrict this action to Admin role for the check
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _studentRepository.GetByIdAsync(id.Value);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Restrict this action to Admin role
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Step 1: Find the student profile to delete
            var student = await _studentRepository.GetByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            // === NEW LOGIC: CHECK FOR CLASSROOM ENROLLMENTS ===
            var enrollments = await _context.Enrollments
                .Include(e => e.Classroom)
                .Where(e => e.StudentId == id)
                .ToListAsync();

            if (enrollments.Any())
            {
                var classroomNames = string.Join(", ", enrollments.Select(e => e.Classroom.ClassName));
                TempData["CannotDeleteMessage"] = $"Cannot delete student '{student.FullName}' because they are enrolled in the following classes: {classroomNames}. Please un-enroll the student from these classes first.";
                return RedirectToAction(nameof(Index));
            }
            // ===================================================

            // Step 2: Check if this student has an associated account
            if (!string.IsNullOrEmpty(student.ApplicationUserId))
            {
                // Step 3: Find the user account by the stored ID
                var user = await _userManager.FindByIdAsync(student.ApplicationUserId);
                if (user != null)
                {
                    // Step 4: Delete the user account (AspNetUsers)
                    var result = await _userManager.DeleteAsync(user);
                    if (!result.Succeeded)
                    {
                        // Handle the error if the account deletion fails
                        ModelState.AddModelError("", "Could not delete the associated user account.");
                        return View("Delete", student);
                    }
                }
            }

            // Step 5: Delete the student profile (Students)
            await _studentRepository.DeleteAsync(id);

            return RedirectToAction(nameof(Index));
        }
    }
}