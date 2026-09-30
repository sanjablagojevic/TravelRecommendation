using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelRecommendation.Domain.Entities;

namespace TravelRecommendation.Infrastructure.Persistence.Configurations;

public class InterestConfiguration : IEntityTypeConfiguration<Interest>
{
    public void Configure(EntityTypeBuilder<Interest> builder)
    {
        builder.ToTable("Interests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasData(
            new Interest { Id = 1, Name = "Beach", Description = "Beaches and coastal destinations" },
            new Interest { Id = 2, Name = "Nature", Description = "Nature and outdoor experiences" },
            new Interest { Id = 3, Name = "Culture", Description = "Culture and local traditions" },
            new Interest { Id = 4, Name = "History", Description = "Historical sites and heritage" },
            new Interest { Id = 5, Name = "Food", Description = "Food and gastronomy" },
            new Interest { Id = 6, Name = "Nightlife", Description = "Nightlife and entertainment" },
            new Interest { Id = 7, Name = "Adventure", Description = "Adventure and extreme activities" },
            new Interest { Id = 8, Name = "Relaxation", Description = "Relaxation and wellness" },
            new Interest { Id = 9, Name = "Mountains", Description = "Mountain destinations" },
            new Interest { Id = 10, Name = "Shopping", Description = "Shopping and markets" });
    }
}
