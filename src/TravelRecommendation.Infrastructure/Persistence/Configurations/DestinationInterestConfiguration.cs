using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelRecommendation.Domain.Entities;

namespace TravelRecommendation.Infrastructure.Persistence.Configurations;

public class DestinationInterestConfiguration : IEntityTypeConfiguration<DestinationInterest>
{
    public void Configure(EntityTypeBuilder<DestinationInterest> builder)
    {
        builder.ToTable("DestinationInterests");

        builder.HasKey(x => new { x.DestinationId, x.InterestId });

        builder.HasOne(x => x.Destination)
            .WithMany(x => x.DestinationInterests)
            .HasForeignKey(x => x.DestinationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Interest)
            .WithMany(x => x.DestinationInterests)
            .HasForeignKey(x => x.InterestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
