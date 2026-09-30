using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelRecommendation.Domain.Entities;

namespace TravelRecommendation.Infrastructure.Persistence.Configurations;

public class DestinationConfiguration : IEntityTypeConfiguration<Destination>
{
    public void Configure(EntityTypeBuilder<Destination> builder)
    {
        builder.ToTable("Destinations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.AverageDailyCost)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Latitude)
            .HasColumnType("decimal(9,6)")
            .IsRequired();

        builder.Property(x => x.Longitude)
            .HasColumnType("decimal(9,6)")
            .IsRequired();

        builder.Property(x => x.Popularity)
            .HasColumnType("decimal(5,4)")
            .IsRequired();

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(1000);

        builder.Property(x => x.Climate)
            .HasMaxLength(100);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.Name, x.Country, x.City });
    }
}
