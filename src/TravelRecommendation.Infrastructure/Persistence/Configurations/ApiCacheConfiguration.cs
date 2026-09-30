using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelRecommendation.Domain.Entities;

namespace TravelRecommendation.Infrastructure.Persistence.Configurations;

public class ApiCacheConfiguration : IEntityTypeConfiguration<ApiCache>
{
    public void Configure(EntityTypeBuilder<ApiCache> builder)
    {
        builder.ToTable("ApiCaches");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Action)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.RequestKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ResponseJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.HasIndex(x => x.RequestKey);

        builder.HasIndex(x => new { x.Provider, x.Action, x.RequestKey })
            .IsUnique()
            .HasDatabaseName("IX_ApiCaches_Provider_Action_RequestKey");

        builder.HasOne(x => x.User)
            .WithMany(x => x.ApiCaches)
            .HasForeignKey(x => x.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
