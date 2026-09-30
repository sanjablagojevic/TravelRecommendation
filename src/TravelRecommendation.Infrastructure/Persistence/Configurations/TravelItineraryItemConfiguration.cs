using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelRecommendation.Domain.Entities;

namespace TravelRecommendation.Infrastructure.Persistence.Configurations;

public class TravelItineraryItemConfiguration : IEntityTypeConfiguration<TravelItineraryItem>
{
    public void Configure(EntityTypeBuilder<TravelItineraryItem> builder)
    {
        builder.ToTable("TravelItineraryItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Order)
            .IsRequired();

        builder.Property(x => x.TimeOfDay)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.ItemType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Location)
            .HasMaxLength(300);

        builder.HasIndex(x => new { x.TravelItineraryDayId, x.Order })
            .IsUnique();

        builder.HasOne(x => x.Attraction)
            .WithMany()
            .HasForeignKey(x => x.AttractionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Activity)
            .WithMany()
            .HasForeignKey(x => x.ActivityId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
