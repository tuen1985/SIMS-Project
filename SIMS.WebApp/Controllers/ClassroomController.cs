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
        // GET: Sửa lại để dùng ViewModel
        public async Task<IActionResult> Create()
        {
            var faculties = await _userManager.GetUsersInRoleAsync("Faculty");
            var viewModel = new ClassroomCreateViewModel
            {
                Courses = new SelectList(_context.Courses, "Id", "CourseName"),
                Faculties = new SelectList(faculties, "Id", "FullName"),
                // === THÊM 2 DÒNG NÀY ĐỂ GÁN NGÀY MẶC ĐỊNH ===
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(3) // Mặc định là 3 tháng sau
                                                    // ===========================================
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassroomCreateViewModel viewModel)
        {
            // KIỂM TRA TÊN LỚP TRÙNG LẶP
            var classroomExists = await _context.Classrooms.AnyAsync(c => c.ClassName == viewModel.ClassName);
            if (classroomExists)
            {
                ModelState.AddModelError("ClassName", "A classroom with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                var classroom = new Classroom
                {
                    ClassName = viewModel.ClassName,
                    Semester = viewModel.Semester,
                    CourseId = viewModel.CourseId,
                    FacultyId = viewModel.FacultyId,
                    StartDate = viewModel.StartDate,
                    EndDate = viewModel.EndDate
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
        public async Task<IActionResult> AddStudentToClass(int classroomId, List<int> studentIds)
        {
            if (studentIds != null && studentIds.Any())
            {
                // Lấy danh sách ID sinh viên đã có trong lớp để tránh trùng lặp
                var existingStudentIds = await _context.Enrollments
                    .Where(e => e.ClassroomId == classroomId)
                    .Select(e => e.StudentId)
                    .ToListAsync();

                // Lọc ra những sinh viên mới thực sự cần thêm
                var newStudentIds = studentIds.Except(existingStudentIds);

                foreach (var studentId in newStudentIds)
                {
                    var enrollment = new Enrollment
                    {
                        ClassroomId = classroomId,
                        StudentId = studentId,
                        Status = EnrollmentStatus.Approved
                    };
                    _context.Enrollments.Add(enrollment);
                }
                await _context.SaveChangesAsync();
            }
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
            var classroom = await _context.Classrooms
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classroom != null)
            {
                // === LOGIC MỚI: KIỂM TRA NẾU CÓ SINH VIÊN TRONG LỚP ===
                if (classroom.Enrollments.Any())
                {
                    TempData["CannotDeleteMessage"] = $"Cannot delete class '{classroom.ClassName}' because there are {classroom.Enrollments.Count} students enrolled. Please remove all students first.";
                    return RedirectToAction(nameof(Index));
                }
                // =======================================================

                // Nếu không có sinh viên, tiến hành xóa lớp học
                _context.Classrooms.Remove(classroom);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        // Dán phương thức này vào bên trong file ClassroomController.cs

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMultipleStudents(int classroomId, List<int> enrollmentIds)
        {
            if (enrollmentIds != null && enrollmentIds.Any())
            {
                // Tìm tất cả các bản ghi enrollment có ID nằm trong danh sách được gửi lên
                var enrollmentsToDelete = await _context.Enrollments
                    .Where(e => enrollmentIds.Contains(e.Id))
                    .ToListAsync();

                if (enrollmentsToDelete.Any())
                {
                    _context.Enrollments.RemoveRange(enrollmentsToDelete);
                    await _context.SaveChangesAsync();
                }
            }

            // Chuyển hướng về lại trang chi tiết của lớp học
            return RedirectToAction("Details", new { id = classroomId });
        }
    }
}