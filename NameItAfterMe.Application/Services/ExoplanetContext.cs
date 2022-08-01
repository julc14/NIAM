using Microsoft.EntityFrameworkCore;

namespace NameItAfterMe.Application.Services;

public class ExoplanetContext : DbContext
{
    public ExoplanetContext(DbContextOptions<ExoplanetContext> options) : base(options)
    {
    }
}
