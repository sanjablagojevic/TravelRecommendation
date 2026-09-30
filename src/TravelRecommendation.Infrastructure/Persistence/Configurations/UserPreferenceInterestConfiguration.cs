using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelRecommendation.Domain.Entities;

namespace TravelRecommendation.Infrastructure.Persistence.Configurations;

public class UserPreferenceInterestConfiguration : IEntityTypeConfiguration<UserPreferenceInterest>
{
    public void Configure(EntityTypeBuilder<UserPreferenceInterest> builder)
    {
        builder.ToTable("UserPreferenceInterests");

        builder.HasKey(x => new { x.UserPreferenceId, x.InterestId });

        builder.HasOne(x => x.UserPreference)
            .WithMany(x => x.UserPreferenceInterests)
            .HasForeignKey(x => x.UserPreferenceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Interest)
            .WithMany(x => x.UserPreferenceInterests)
            .HasForeignKey(x => x.InterestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
