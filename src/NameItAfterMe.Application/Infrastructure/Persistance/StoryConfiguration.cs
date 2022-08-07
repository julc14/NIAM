using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NameItAfterMe.Application.Domain;

namespace NameItAfterMe.Application.Infrastructure.Persistance;

internal class StoryConfiguration : IEntityTypeConfiguration<Story>
{
    public void Configure(EntityTypeBuilder<Story> builder)
    {
        builder.HasPartitionKey(x => x.Id);
        builder.HasKey(x => x.Name);
        builder.Property(x => x.Id).ToJsonProperty("id").ValueGeneratedOnAdd();
    }
}