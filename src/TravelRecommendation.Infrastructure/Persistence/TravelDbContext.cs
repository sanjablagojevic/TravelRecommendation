using Microsoft.EntityFrameworkCore;
using TravelRecommendation.Domain.Entities;

namespace TravelRecommendation.Infrastructure.Persistence;

public class TravelDbContext : DbContext
{
    public TravelDbContext(DbContextOptions<TravelDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<Interest> Interests => Set<Interest>();
    public DbSet<UserPreferenceInterest> UserPreferenceInterests => Set<UserPreferenceInterest>();
    public DbSet<Destination> Destinations => Set<Destination>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<DestinationCategory> DestinationCategories => Set<DestinationCategory>();
    public DbSet<DestinationInterest> DestinationInterests => Set<DestinationInterest>();
    public DbSet<Attraction> Attractions => Set<Attraction>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Recommendation> Recommendations => Set<Recommendation>();
    public DbSet<RecommendationItem> RecommendationItems => Set<RecommendationItem>();
    public DbSet<TravelHistory> TravelHistories => Set<TravelHistory>();
    public DbSet<ApiCache> ApiCaches => Set<ApiCache>();
    public DbSet<TravelItinerary> TravelItineraries => Set<TravelItinerary>();
    public DbSet<TravelItineraryDay> TravelItineraryDays => Set<TravelItineraryDay>();
    public DbSet<TravelItineraryItem> TravelItineraryItems => Set<TravelItineraryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TravelDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
