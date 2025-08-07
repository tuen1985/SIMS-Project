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

namespace SIMS.WebApp.Controllers
{
    [Authorize(Roles = "Faculty")]
    public class FacultyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FacultyController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Faculty hoặc /Faculty/Index
        // Trang này sẽ là trang "Quản lý điểm"
        public async Task<IActionResult> Index()
        {
            var facultyId = _userManager.GetUserId(User);
            var classrooms = await _context.Classrooms
                .Where(c => c.FacultyId == facultyId)
                .Include(c => c.Course)
                .ToListAsync();

            return View(classrooms);
        }

        // GET: /Faculty/ManageGrades/5
        public async Task<IActionResult> ManageGrades(int id)
        {
            var classroom = await _context.Classrooms
                .Include(c => c.Course)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Grade)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classroom == null || classroom.FacultyId != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            var viewModel = classroom.Enrollments.Select(e => new GradeUpdateViewModel
            {
                EnrollmentId = e.Id,
                StudentName = e.Student.FullName,
                StudentCode = e.Student.StudentCode,
                Score = e.Grade?.Score
            }).ToList();

            ViewBag.ClassroomId = id;
            ViewBag.ClassName = classroom.ClassName;

            return View(viewModel);
        }

        // POST: /Faculty/ManageGrades
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageGrades(int classroomId, List<GradeUpdateViewModel> studentGrades)
        {
            if (ModelState.IsValid)
            {
                foreach (var studentGrade in studentGrades)
                {
                    var enrollment = await _context.Enrollments
                        .Include(e => e.Grade)
                        .FirstOrDefaultAsync(e => e.Id == studentGrade.EnrollmentId);

                    if (enrollment != null)
                    {
                        if (enrollment.Grade != null)
                        {
                            // Cập nhật điểm đã có
                            enrollment.Grade.Score = studentGrade.Score;
                        }
                        else if (studentGrade.Score.HasValue)
                        {
                            // Tạo điểm mới nếu chưa có và điểm được nhập
                            var newGrade = new Grade
                            {
                                EnrollmentId = studentGrade.EnrollmentId,
                                Score = studentGrade.Score
                            };
                            _context.Grades.Add(newGrade);
                        }
                    }
                }
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(studentGrades);
        }
    }
}