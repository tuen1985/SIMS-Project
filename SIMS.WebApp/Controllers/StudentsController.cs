using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.Application.Repositories;
using SIMS.Domain;
using Microsoft.AspNetCore.Authorization; // Thêm using cho phân quyền truy cập

namespace SIMS.WebApp.Controllers
{
    // Chỉ cho phép người dùng có vai trò "Department Staff" truy cập vào controller này
    [Authorize(Roles = "Department Staff")]
    public class StudentsController : Controller
    {
        // Tiêm dependency: Repository dùng để thao tác với sinh viên
        private readonly IStudentRepository _studentRepository;

        // Constructor: Gán repository được inject
        public StudentsController(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        // GET: Students
        // Trả về view hiển thị danh sách sinh viên
        public async Task<IActionResult> Index()
        {
            return View(await _studentRepository.GetAllAsync());
        }

        // GET: Students/Details/5
        // Trả về thông tin chi tiết một sinh viên theo id
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Trả lỗi nếu không có id
            }

            var student = await _studentRepository.GetByIdAsync(id.Value);
            if (student == null)
            {
                return NotFound(); // Trả lỗi nếu không tìm thấy sinh viên
            }

            return View(student); // Hiển thị thông tin sinh viên
        }

        // GET: Students/Create
        // Hiển thị form tạo mới sinh viên
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // Xử lý tạo mới sinh viên khi submit form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            if (ModelState.IsValid)
            {
                await _studentRepository.AddAsync(student); // Thêm sinh viên vào DB
                return RedirectToAction(nameof(Index)); // Quay lại danh sách
            }
            return View(student); // Dữ liệu không hợp lệ → hiện lại form
        }

        // GET: Students/Edit/5
        // Hiển thị form chỉnh sửa thông tin sinh viên
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
        // Xử lý cập nhật thông tin sinh viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.Id)
            {
                return NotFound(); // ID không trùng → lỗi
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _studentRepository.UpdateAsync(student); // Cập nhật
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Kiểm tra lại xem sinh viên có tồn tại không
                    if (await _studentRepository.GetByIdAsync(id) == null)
                    {
                        return NotFound(); // Không tồn tại → lỗi
                    }
                    else
                    {
                        throw; // Ném lại lỗi nếu khác
                    }
                }
                return RedirectToAction(nameof(Index)); // Cập nhật xong → quay về
            }
            return View(student); // Nếu form lỗi → hiển thị lại
        }

        // GET: Students/Delete/5
        // Hiển thị trang xác nhận xóa sinh viên
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
        // Xử lý xóa sinh viên khi xác nhận
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _studentRepository.DeleteAsync(id); // Xóa sinh viên khỏi DB
            return RedirectToAction(nameof(Index)); // Quay lại danh sách
        }
    }
}
