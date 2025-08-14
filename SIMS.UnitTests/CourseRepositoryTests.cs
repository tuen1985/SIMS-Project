using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SIMS.Domain;
using SIMS.Infrastructure;
using SIMS.Infrastructure.Repositories;
using Xunit;

namespace SIMS.UnitTests
{
    public class CourseRepositoryTests
    {
        // Helper method to create an in-memory mock DbContext
        private async Task<ApplicationDbContext> GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            await context.Database.EnsureCreatedAsync();
            return context;
        }

        [Fact]
        public async Task AddAsync_ShouldAddCourseToDatabase()
        {
            // Arrange
            var context = await GetDbContext();
            var repository = new CourseRepository(context);
            var newCourse = new Course { CourseCode = "CS101", CourseName = "Introduction to C#", Credits = 3 };

            // Act
            await repository.AddAsync(newCourse);

            // Assert
            var coursesCount = await context.Courses.CountAsync();
            Assert.Equal(1, coursesCount);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectCourse()
        {
            // Arrange
            var context = await GetDbContext();
            var repository = new CourseRepository(context);
            var course1 = new Course { CourseCode = "CS101", CourseName = "Intro to C#", Credits = 3 };
            var course2 = new Course { CourseCode = "PRJ301", CourseName = "Web Development", Credits = 4 };
            await context.Courses.AddRangeAsync(course1, course2);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(course1.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(course1.Id, result.Id);
            Assert.Equal(course1.CourseName, result.CourseName);
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyCourseInDatabase()
        {
            // Arrange
            var context = await GetDbContext();
            var repository = new CourseRepository(context);
            var courseToUpdate = new Course { CourseCode = "CS101", CourseName = "Old Name", Credits = 3 };
            await repository.AddAsync(courseToUpdate);

            // Assign a new value
            courseToUpdate.CourseName = "New Name";

            // Act
            await repository.UpdateAsync(courseToUpdate);

            // Assert
            var updatedCourse = await context.Courses.FindAsync(courseToUpdate.Id);
            Assert.NotNull(updatedCourse);
            Assert.Equal("New Name", updatedCourse.CourseName);
        }
    }
}
