namespace Elearning.Modules.Program.Application.Data;

public interface IProgramUnitOfWork
{
  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}