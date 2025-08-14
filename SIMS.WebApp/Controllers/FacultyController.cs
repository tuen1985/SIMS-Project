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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageGrades(int classroomId, List<GradeUpdateViewModel> studentGrades)
        {
            if (!ModelState.IsValid)
            {
                // Khi ModelState không hợp lệ, cần phải tải lại dữ liệu cần thiết cho view
                // Bao gồm thông tin về lớp học và sinh viên
                var classroom = await _context.Classrooms
                    .Include(c => c.Enrollments)
                        .ThenInclude(e => e.Student)
                    .Include(c => c.Enrollments)
                        .ThenInclude(e => e.Grade)
                    .FirstOrDefaultAsync(c => c.Id == classroomId);

                if (classroom == null || classroom.FacultyId != _userManager.GetUserId(User))
                {
                    return NotFound();
                }

                // Cập nhật lại ViewModel từ dữ liệu mới nhất trong DB, nhưng giữ lại các giá trị nhập từ form
                var viewModel = classroom.Enrollments.Select(e => new GradeUpdateViewModel
                {
                    EnrollmentId = e.Id,
                    StudentName = e.Student.FullName,
                    StudentCode = e.Student.StudentCode,
                    Score = e.Grade?.Score
                }).ToList();

                // Gán các giá trị điểm đã nhập từ form không hợp lệ trở lại cho ViewModel
                // để người dùng không phải nhập lại
                foreach (var submittedGrade in studentGrades)
                {
                    var vm = viewModel.FirstOrDefault(x => x.EnrollmentId == submittedGrade.EnrollmentId);
                    if (vm != null)
                    {
                        vm.Score = submittedGrade.Score;
                    }
                }

                ViewBag.ClassroomId = classroomId;
                ViewBag.ClassName = classroom.ClassName;

                // Trả về view với dữ liệu đầy đủ và lỗi ModelState
                return View(viewModel);
            }

            // Logic lưu điểm khi ModelState hợp lệ
            foreach (var studentGrade in studentGrades)
            {
                var enrollment = await _context.Enrollments
                    .Include(e => e.Grade)
                    .FirstOrDefaultAsync(e => e.Id == studentGrade.EnrollmentId);

                if (enrollment != null)
                {
                    if (enrollment.Grade != null)
                    {
                        enrollment.Grade.Score = studentGrade.Score;
                    }
                    else if (studentGrade.Score.HasValue)
                    {
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
    }
}