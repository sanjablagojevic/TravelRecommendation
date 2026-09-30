using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelRecommendation.Domain.Entities;

namespace TravelRecommendation.Infrastructure.Persistence.Configurations;

public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.ToTable("UserPreferences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Budget)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Duration);

        builder.Property(x => x.TripType)
            .HasMaxLength(100);

        builder.Property(x => x.TravelPeriod)
            .HasMaxLength(100);

        builder.Property(x => x.PreferredClimate)
            .HasMaxLength(100);

        builder.Property(x => x.MinTemperature)
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.MaxTemperature)
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(x => x.UserPreferences)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
