using Microsoft.EntityFrameworkCore;
using NameItAfterMe.Application.Abstractions;
using NameItAfterMe.Application.Domain;

namespace NameItAfterMe.Infrastructure;

public class ExoplanetContext : DbContext, IExoplanetContext
{
    public ExoplanetContext(DbContextOptions<ExoplanetContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exoplanet>();
    }
}
