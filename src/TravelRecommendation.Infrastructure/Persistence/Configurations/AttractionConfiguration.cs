using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelRecommendation.Domain.Entities;

namespace TravelRecommendation.Infrastructure.Persistence.Configurations;

public class AttractionConfiguration : IEntityTypeConfiguration<Attraction>
{
    public void Configure(EntityTypeBuilder<Attraction> builder)
    {
        builder.ToTable("Attractions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Location)
            .HasMaxLength(250);

        builder.Property(x => x.Latitude)
            .HasColumnType("decimal(9,6)");

        builder.Property(x => x.Longitude)
            .HasColumnType("decimal(9,6)");

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Destination)
            .WithMany(x => x.Attractions)
            .HasForeignKey(x => x.DestinationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
