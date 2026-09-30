using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelRecommendation.Domain.Entities;

namespace TravelRecommendation.Infrastructure.Persistence.Configurations;

public class DestinationCategoryConfiguration : IEntityTypeConfiguration<DestinationCategory>
{
    public void Configure(EntityTypeBuilder<DestinationCategory> builder)
    {
        builder.ToTable("DestinationCategories");

        builder.HasKey(x => new { x.DestinationId, x.CategoryId });

        builder.HasOne(x => x.Destination)
            .WithMany(x => x.DestinationCategories)
            .HasForeignKey(x => x.DestinationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.DestinationCategories)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
