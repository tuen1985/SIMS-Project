using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SIMS.Domain;

namespace SIMS.Application.Repositories
{
    public interface ICourseRepository
    {
        Task<Course?> GetByIdAsync(int id);
        Task<IEnumerable<Course>> GetAllAsync();
        Task AddAsync(Course course);
        Task UpdateAsync(Course course);
        Task DeleteAsync(int id);
    }
}