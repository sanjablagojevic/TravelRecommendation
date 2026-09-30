using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelRecommendation.Domain.Entities;

namespace TravelRecommendation.Infrastructure.Persistence.Configurations;

public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("Favorites");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AddedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.DestinationId })
            .IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(x => x.Favorites)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Destination)
            .WithMany(x => x.Favorites)
            .HasForeignKey(x => x.DestinationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
