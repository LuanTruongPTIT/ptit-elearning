using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elearning.Modules.Program.Domain.Entities;

namespace Elearning.Modules.Program.Domain.Repositories
{
    public interface ITeacherRepository
    {
        Task<Teacher> GetAsync(Guid id);
        Task<IEnumerable<Teacher>> GetAllAsync();
        Task<IEnumerable<Teacher>> GetByDepartmentAsync(string department);
        Task AddAsync(Teacher teacher);
        Task UpdateAsync(Teacher teacher);
        Task DeleteAsync(Teacher teacher);
        Task<bool> ExistsAsync(string email);
    }
}
