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
    public class StudentRepositoryTests
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
        public async Task AddAsync_ShouldAddStudentToDatabase()
        {
            // Arrange
            var context = await GetDbContext();
            var repository = new StudentRepository(context);
            var newStudent = new Student
            {
                StudentCode = "BH00001",
                FirstName = "Nguyen Van",
                LastName = "An",
                Email = "an.nv@student.com",
                DateOfBirth = new DateTime(2003, 1, 15)
            };

            // Act
            await repository.AddAsync(newStudent);

            // Assert
            var studentsCount = await context.Students.CountAsync();
            Assert.Equal(1, studentsCount);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllStudents()
        {
            // Arrange
            var context = await GetDbContext();
            var repository = new StudentRepository(context);
            await repository.AddAsync(new Student { StudentCode = "BH00001", FirstName = "An", LastName = "Nguyen", Email = "an@test.com", DateOfBirth = new DateTime(2003, 1, 1) });
            await repository.AddAsync(new Student { StudentCode = "BH00002", FirstName = "Binh", LastName = "Tran", Email = "binh@test.com", DateOfBirth = new DateTime(2003, 1, 1) });

            // Act
            var students = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(students);
            Assert.Equal(2, students.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectStudent()
        {
            // Arrange
            var context = await GetDbContext();
            var repository = new StudentRepository(context);
            var expectedStudent = new Student
            {
                StudentCode = "BH00001",
                FirstName = "Nguyen Van",
                LastName = "An",
                Email = "an.nv@student.com",
                DateOfBirth = new DateTime(2003, 1, 15)
            };
            await repository.AddAsync(expectedStudent);

            // Act
            var result = await repository.GetByIdAsync(expectedStudent.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedStudent.Id, result.Id);
            Assert.Equal(expectedStudent.StudentCode, result.StudentCode);
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyStudentInDatabase()
        {
            // Arrange
            var context = await GetDbContext();
            var repository = new StudentRepository(context);
            var studentToUpdate = new Student
            {
                StudentCode = "BH00001",
                FirstName = "An",
                LastName = "Nguyen",
                Email = "an@test.com",
                DateOfBirth = new DateTime(2003, 1, 1)
            };
            await repository.AddAsync(studentToUpdate);
            studentToUpdate.FirstName = "Binh";

            // Act
            await repository.UpdateAsync(studentToUpdate);

            // Assert
            var updatedStudent = await context.Students.FindAsync(studentToUpdate.Id);
            Assert.NotNull(updatedStudent);
            Assert.Equal("Binh", updatedStudent.FirstName);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveStudentFromDatabase()
        {
            // Arrange
            var context = await GetDbContext();
            var repository = new StudentRepository(context);
            var studentToDelete = new Student
            {
                StudentCode = "BH00001",
                FirstName = "An",
                LastName = "Nguyen",
                Email = "an@test.com",
                DateOfBirth = new DateTime(2003, 1, 1)
            };
            await repository.AddAsync(studentToDelete);

            // Act
            await repository.DeleteAsync(studentToDelete.Id);

            // Assert
            var deletedStudent = await context.Students.FindAsync(studentToDelete.Id);
            Assert.Null(deletedStudent);
        }
    }
}
