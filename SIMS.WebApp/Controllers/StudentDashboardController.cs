using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.Domain;
using SIMS.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace SIMS.WebApp.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentDashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /StudentDashboard/LearningPath
        public async Task<IActionResult> LearningPath()
        {
            var user = await _userManager.GetUserAsync(User);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);

            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == student.Id)
                .Include(e => e.Classroom.Course)
                .Include(e => e.Classroom.Faculty)
                .Include(e => e.Grade)
                .ToListAsync();

            return View(enrollments);
        }

        // GET: /StudentDashboard/AcademicResults
        public async Task<IActionResult> AcademicResults()
        {
            var user = await _userManager.GetUserAsync(User);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);

            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == student.Id && e.Status == EnrollmentStatus.Approved)
                .Include(e => e.Classroom.Course)
                .Include(e => e.Grade)
                .ToListAsync();

            return View(enrollments);
        }
    }
}