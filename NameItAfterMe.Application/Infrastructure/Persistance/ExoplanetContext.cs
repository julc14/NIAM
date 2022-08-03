using Microsoft.EntityFrameworkCore;

namespace NameItAfterMe.Infrastructure.Persistance;

public class ExoplanetContext : DbContext
{
    public ExoplanetContext(DbContextOptions<ExoplanetContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ExoplanetContext).Assembly);
    }
}
