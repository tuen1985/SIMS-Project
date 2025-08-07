using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SIMS.Domain;
using SIMS.Infrastructure;
using SIMS.WebApp.ViewModels; // Thêm using này
using System.Linq;
using System.Threading.Tasks;

namespace SIMS.WebApp.Controllers
{
    [Authorize(Roles = "Department Staff")]
    public class ClassroomController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClassroomController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var classrooms = await _context.Classrooms
                .Include(c => c.Course)
                .Include(c => c.Faculty)
                .ToListAsync();
            return View(classrooms);
        }

        // GET: Sửa lại để dùng ViewModel
        public async Task<IActionResult> Create()
        {
            var faculties = await _userManager.GetUsersInRoleAsync("Faculty");
            var viewModel = new ClassroomCreateViewModel
            {
                Courses = new SelectList(_context.Courses, "Id", "CourseName"),
                Faculties = new SelectList(faculties, "Id", "FullName")
            };
            return View(viewModel);
        }

        // POST: Sửa lại để nhận ViewModel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassroomCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Chuyển dữ liệu từ ViewModel sang Domain Model để lưu
                var classroom = new Classroom
                {
                    ClassName = viewModel.ClassName,
                    Semester = viewModel.Semester,
                    CourseId = viewModel.CourseId,
                    FacultyId = viewModel.FacultyId
                };

                _context.Add(classroom);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Nếu không hợp lệ, tải lại danh sách cho dropdown
            viewModel.Courses = new SelectList(_context.Courses, "Id", "CourseName", viewModel.CourseId);
            var faculties = await _userManager.GetUsersInRoleAsync("Faculty");
            viewModel.Faculties = new SelectList(faculties, "Id", "FullName", viewModel.FacultyId);
            return View(viewModel);
        }

        // GET: Details/5 (Phiên bản nâng cấp)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classroom = await _context.Classrooms
                .Include(c => c.Course)
                .Include(c => c.Faculty)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (classroom == null)
            {
                return NotFound();
            }

            // === PHẦN MỚI THÊM VÀO ===
            // Lấy ID của các sinh viên đã có trong lớp
            var enrolledStudentIds = classroom.Enrollments.Select(e => e.StudentId).ToList();

            // Lấy danh sách các sinh viên chưa có trong lớp để làm dropdown
            var availableStudents = await _context.Students
                .Where(s => !enrolledStudentIds.Contains(s.Id))
                .ToListAsync();

            ViewBag.AvailableStudents = new SelectList(availableStudents, "Id", "FullName"); // Giả sử Student có thuộc tính FullName
                                                                                             // === KẾT THÚC PHẦN MỚI ===

            return View(classroom);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudentToClass(int classroomId, int studentId)
        {
            if (studentId > 0 && classroomId > 0)
            {
                // Kiểm tra xem sinh viên đã tồn tại trong lớp chưa
                var existingEnrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.ClassroomId == classroomId && e.StudentId == studentId);

                if (existingEnrollment == null)
                {
                    var enrollment = new Enrollment
                    {
                        ClassroomId = classroomId,
                        StudentId = studentId,
                        Status = EnrollmentStatus.Approved // Staff thêm vào thì duyệt luôn
                    };
                    _context.Enrollments.Add(enrollment);
                    await _context.SaveChangesAsync();
                }
            }
            // Chuyển hướng về lại trang chi tiết của chính lớp học đó
            return RedirectToAction("Details", new { id = classroomId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStudentFromClass(int enrollmentId, int classroomId)
        {
            // Tìm bản ghi đăng ký cần xóa
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);

            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
            }

            // Sau khi xóa, chuyển hướng người dùng về lại trang chi tiết của lớp học
            return RedirectToAction("Details", new { id = classroomId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // Tìm lớp học cần xóa, bao gồm cả các lượt đăng ký liên quan
            var classroom = await _context.Classrooms
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classroom != null)
            {
                // Xóa tất cả các lượt đăng ký trong lớp học này trước
                _context.Enrollments.RemoveRange(classroom.Enrollments);

                // Sau đó mới xóa lớp học
                _context.Classrooms.Remove(classroom);

                // Lưu thay đổi vào database
                await _context.SaveChangesAsync();
            }

            // Chuyển hướng về lại trang danh sách
            return RedirectToAction(nameof(Index));
        }
    }
}