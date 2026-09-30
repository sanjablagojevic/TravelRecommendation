using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelRecommendation.Domain.Entities;

namespace TravelRecommendation.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasData(
            new Category { Id = 1, Name = "Beach", Type = "Interest", Description = "Beach destinations" },
            new Category { Id = 2, Name = "Nature", Type = "Interest", Description = "Nature destinations" },
            new Category { Id = 3, Name = "Culture", Type = "Interest", Description = "Cultural destinations" },
            new Category { Id = 4, Name = "History", Type = "Interest", Description = "Historical destinations" },
            new Category { Id = 5, Name = "Food", Type = "Interest", Description = "Food-focused destinations" },
            new Category { Id = 6, Name = "Nightlife", Type = "Interest", Description = "Nightlife destinations" },
            new Category { Id = 7, Name = "Adventure", Type = "Interest", Description = "Adventure destinations" },
            new Category { Id = 8, Name = "Relaxation", Type = "Interest", Description = "Relaxation destinations" },
            new Category { Id = 9, Name = "Mountains", Type = "Interest", Description = "Mountain destinations" },
            new Category { Id = 10, Name = "City Break", Type = "TripStyle", Description = "City break destinations" });
    }
}
