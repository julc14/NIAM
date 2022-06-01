using Microsoft.EntityFrameworkCore;

namespace NameItAfterMe.Application.Abstractions;

public interface IExoplanetContext
{
    DbSet<T> Set<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
