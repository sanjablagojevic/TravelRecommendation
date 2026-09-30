using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelRecommendation.Domain.Entities;

namespace TravelRecommendation.Infrastructure.Persistence.Configurations;

public class RecommendationConfiguration : IEntityTypeConfiguration<Recommendation>
{
    public void Configure(EntityTypeBuilder<Recommendation> builder)
    {
        builder.ToTable("Recommendations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OriginalQuery)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(x => x.Recommendations)
            .HasForeignKey(x => x.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UserPreference)
            .WithMany(x => x.Recommendations)
            .HasForeignKey(x => x.UserPreferenceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
