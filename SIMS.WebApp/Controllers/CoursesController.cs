using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SIMS.Domain;
using SIMS.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using SIMS.Application.Repositories; // Thêm using này

namespace SIMS.WebApp.Controllers
{
    [Authorize(Roles = "Department Staff")]
    public class CoursesController : Controller
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ApplicationDbContext _context; // Thêm ApplicationDbContext

        public CoursesController(ICourseRepository courseRepository, ApplicationDbContext context) // Cập nhật constructor
        {
            _courseRepository = courseRepository;
            _context = context; // Khởi tạo context
        }

        // GET: Courses
        public async Task<IActionResult> Index()
        {
            return View(await _courseRepository.GetAllAsync());
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _courseRepository.GetByIdAsync(id.Value);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CourseCode,CourseName,Credits")] Course course)
        {
            if (ModelState.IsValid)
            {
                await _courseRepository.AddAsync(course);
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _courseRepository.GetByIdAsync(id.Value);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CourseCode,CourseName,Credits")] Course course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _courseRepository.UpdateAsync(course);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Kiểm tra xem course có còn tồn tại không
                    if (await _courseRepository.GetByIdAsync(id) == null)
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
            return View(course);
        }

        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _courseRepository.GetByIdAsync(id.Value);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // === LOGIC MỚI: Kiểm tra xem khóa học có được sử dụng trong bất kỳ lớp học nào không ===
            var classrooms = await _context.Classrooms
                .Where(c => c.CourseId == id)
                .ToListAsync();

            if (classrooms.Any())
            {
                // Nếu có, hiển thị thông báo lỗi và ngăn xóa
                var course = await _courseRepository.GetByIdAsync(id);
                var classroomNames = string.Join(", ", classrooms.Select(c => c.ClassName));
                TempData["CannotDeleteMessage"] = $"Cannot delete course '{course.CourseName}' because it is used in the following classes: {classroomNames}. Please delete these classes first.";
                return RedirectToAction(nameof(Index));
            }
            // =========================================================================================

            // Nếu không có lớp học nào sử dụng khóa học này, tiến hành xóa
            await _courseRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
