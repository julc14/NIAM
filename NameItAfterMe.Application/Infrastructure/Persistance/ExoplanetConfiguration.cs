using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NameItAfterMe.Application.Domain;

namespace NameItAfterMe.Infrastructure.Persistance;

internal class ExoplanetConfiguration : IEntityTypeConfiguration<Exoplanet>
{
    public void Configure(EntityTypeBuilder<Exoplanet> builder)
    {
        builder.ToContainer("Exoplanet");
        builder.HasNoDiscriminator();
        builder.Property(x => x.Id).ToJsonProperty("id").ValueGeneratedOnAdd();
        builder.HasKey(x => x.Id);
        builder.HasPartitionKey(x => x.Id);
        builder.Property(x => x.Distance);
        // todo revise
        builder.Property(x => x.DistanceUnits);
        builder.Property(x => x.HostName);
        builder.Property(x => x.Name);
        builder.Property(x => x.ProvidedName);
    }
}
