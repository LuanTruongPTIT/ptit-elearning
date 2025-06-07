using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elearning.Modules.Program.Domain.Entities;

namespace Elearning.Modules.Program.Domain.Repositories
{
    public interface ICourseRepository
    {
        Task<Course> GetAsync(Guid id);
        Task<Course> GetByCodeAsync(string code);
        Task<IEnumerable<Course>> GetAllAsync();
        Task<IEnumerable<Course>> GetByDepartmentAsync(string department);
        Task<IEnumerable<Course>> GetByTeacherAsync(Guid teacherId);
        Task<IEnumerable<Course>> GetByStatusAsync(CourseStatus status);
        Task AddAsync(Course course);
        Task UpdateAsync(Course course);
        Task DeleteAsync(Course course);
        Task<bool> ExistsAsync(string code);
    }
}
