using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NameItAfterMe.Application.Domain;

namespace NameItAfterMe.Infrastructure.Persistance;

internal class ExoplanetConfiguration : IEntityTypeConfiguration<Exoplanet>
{
    public void Configure(EntityTypeBuilder<Exoplanet> builder)
    {
        builder.ToContainer("ExoplanetContext");
        builder.HasKey(x => x.Name);
        builder.HasNoDiscriminator();
        builder.HasPartitionKey(x => x.Name);

        builder.Property(x => x.Name);
        builder.Property(x => x.ProvidedName);
        builder.Property(x => x.HostName);

        builder.OwnsOne(x => x.Distance);
    }
}