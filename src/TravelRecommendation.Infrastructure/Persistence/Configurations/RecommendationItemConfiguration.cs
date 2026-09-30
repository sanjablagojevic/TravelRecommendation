using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelRecommendation.Domain.Entities;

namespace TravelRecommendation.Infrastructure.Persistence.Configurations;

public class RecommendationItemConfiguration : IEntityTypeConfiguration<RecommendationItem>
{
    public void Configure(EntityTypeBuilder<RecommendationItem> builder)
    {
        builder.ToTable("RecommendationItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Score)
            .HasColumnType("decimal(5,4)")
            .IsRequired();

        builder.Property(x => x.Rank)
            .IsRequired();

        builder.Property(x => x.Explanation)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.AiExplanation)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.EstimatedCost)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(x => new { x.RecommendationId, x.DestinationId })
            .IsUnique();

        builder.HasIndex(x => new { x.RecommendationId, x.Rank })
            .IsUnique();

        builder.HasOne(x => x.Recommendation)
            .WithMany(x => x.RecommendationItems)
            .HasForeignKey(x => x.RecommendationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Destination)
            .WithMany(x => x.RecommendationItems)
            .HasForeignKey(x => x.DestinationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
