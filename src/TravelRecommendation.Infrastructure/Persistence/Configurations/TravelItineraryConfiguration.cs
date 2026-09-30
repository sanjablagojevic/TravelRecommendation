using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelRecommendation.Domain.Entities;

namespace TravelRecommendation.Infrastructure.Persistence.Configurations;

public class TravelItineraryConfiguration : IEntityTypeConfiguration<TravelItinerary>
{
    public void Configure(EntityTypeBuilder<TravelItinerary> builder)
    {
        builder.ToTable("TravelItineraries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.DurationDays)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.AiModel)
            .HasMaxLength(100);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.DestinationId);
        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne(x => x.User)
            .WithMany(x => x.TravelItineraries)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Destination)
            .WithMany(x => x.TravelItineraries)
            .HasForeignKey(x => x.DestinationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UserPreference)
            .WithMany(x => x.TravelItineraries)
            .HasForeignKey(x => x.UserPreferenceId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Recommendation)
            .WithMany(x => x.TravelItineraries)
            .HasForeignKey(x => x.RecommendationId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Days)
            .WithOne(x => x.TravelItinerary)
            .HasForeignKey(x => x.TravelItineraryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
