using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SIMS.Domain;

namespace SIMS.Application.Repositories
{
    public interface IStudentRepository
    {
        Task<Student?> GetByIdAsync(int id);
        Task<IEnumerable<Student>> GetAllAsync();
        Task AddAsync(Student student);
        Task UpdateAsync(Student student);
        Task DeleteAsync(int id);
    }
}
