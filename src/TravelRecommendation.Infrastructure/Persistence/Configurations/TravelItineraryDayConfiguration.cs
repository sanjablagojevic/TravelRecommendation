using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelRecommendation.Domain.Entities;

namespace TravelRecommendation.Infrastructure.Persistence.Configurations;

public class TravelItineraryDayConfiguration : IEntityTypeConfiguration<TravelItineraryDay>
{
    public void Configure(EntityTypeBuilder<TravelItineraryDay> builder)
    {
        builder.ToTable("TravelItineraryDays");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DayNumber)
            .IsRequired();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Summary)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => new { x.TravelItineraryId, x.DayNumber })
            .IsUnique();

        builder.HasMany(x => x.Items)
            .WithOne(x => x.TravelItineraryDay)
            .HasForeignKey(x => x.TravelItineraryDayId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
