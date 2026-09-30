using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TravelRecommendation.Domain.Entities;
using TravelRecommendation.Infrastructure.Persistence;

namespace TravelRecommendation.Infrastructure.Persistence;

/// <summary>
/// Development/demo seeder (not EF HasData) to keep migrations small and readable.
/// Idempotent: skips destinations that already exist by Name + Country + City.
/// AverageDailyCost values are DEMO estimates in EUR, not live market prices.
/// </summary>
public static class DevelopmentDemoDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TravelDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DevelopmentDemoDataSeeder");

        var demoData = GetDemoDestinations();
        var createdDestinations = 0;
        var createdAttractions = 0;
        var createdActivities = 0;

        foreach (var item in demoData)
        {
            var exists = await dbContext.Destinations.AnyAsync(d =>
                d.Name == item.Destination.Name
                && d.Country == item.Destination.Country
                && d.City == item.Destination.City);

            if (exists)
            {
                continue;
            }

            var destination = item.Destination;
            destination.CreatedAt = DateTime.UtcNow;
            destination.IsActive = true;
            destination.DestinationCategories = item.CategoryIds
                .Distinct()
                .Select(categoryId => new DestinationCategory { CategoryId = categoryId })
                .ToList();

            foreach (var attraction in item.Attractions)
            {
                destination.Attractions.Add(attraction);
                createdAttractions++;
            }

            foreach (var activity in item.Activities)
            {
                destination.Activities.Add(activity);
                createdActivities++;
            }

            dbContext.Destinations.Add(destination);
            createdDestinations++;
        }

        if (createdDestinations > 0)
        {
            await dbContext.SaveChangesAsync();
            logger.LogInformation(
                "Demo seed complete: {Destinations} destinations, {Attractions} attractions, {Activities} activities.",
                createdDestinations,
                createdAttractions,
                createdActivities);
        }
        else
        {
            logger.LogInformation("Demo destinations already present. Skipping destination seed.");
        }

        var updatedImages = await BackfillMissingImageUrlsAsync(dbContext, demoData);
        if (updatedImages > 0)
        {
            logger.LogInformation("Demo ImageUrl backfill updated {Count} destinations.", updatedImages);
        }

        var linkedInterests = await SeedDestinationInterestsAsync(dbContext);
        logger.LogInformation("Demo DestinationInterest links ensured/created: {Count}.", linkedInterests);
    }

    private static async Task<int> BackfillMissingImageUrlsAsync(
        TravelDbContext dbContext,
        List<DemoDestinationSeed> demoData)
    {
        var byKey = demoData.ToDictionary(
            d => (d.Destination.Name, d.Destination.Country, d.Destination.City),
            d => d.Destination.ImageUrl);

        var destinations = await dbContext.Destinations.ToListAsync();
        var updated = 0;

        foreach (var destination in destinations)
        {
            if (!string.IsNullOrWhiteSpace(destination.ImageUrl))
            {
                continue;
            }

            if (!byKey.TryGetValue((destination.Name, destination.Country, destination.City), out var imageUrl)
                || string.IsNullOrWhiteSpace(imageUrl))
            {
                continue;
            }

            destination.ImageUrl = imageUrl;
            updated++;
        }

        if (updated > 0)
        {
            await dbContext.SaveChangesAsync();
        }

        return updated;
    }

    private static async Task<int> SeedDestinationInterestsAsync(TravelDbContext dbContext)
    {
        // Interest names match seeded Interests: Beach, Nature, Culture, History, Food,
        // Nightlife, Adventure, Relaxation, Mountains, Shopping
        var mappings = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Barcelona"] = ["Beach", "Culture", "History", "Food", "Nightlife", "Shopping"],
            ["Rome"] = ["Culture", "History", "Food"],
            ["Paris"] = ["Culture", "History", "Food", "Shopping"],
            ["Lisbon"] = ["Beach", "Culture", "History", "Food", "Nightlife"],
            ["Athens"] = ["Beach", "Culture", "History", "Food"],
            ["Santorini"] = ["Beach", "Nature", "Food", "Relaxation"],
            ["Istanbul"] = ["Culture", "History", "Food", "Shopping", "Nightlife"],
            ["Dubrovnik"] = ["Beach", "Culture", "History", "Relaxation"],
            ["Split"] = ["Beach", "Culture", "History", "Nightlife"],
            ["Kotor"] = ["Beach", "Nature", "Culture", "History", "Relaxation"],
            ["Budva"] = ["Beach", "Nightlife", "Relaxation"],
            ["Vienna"] = ["Culture", "History", "Food", "Shopping"],
            ["Prague"] = ["Culture", "History", "Food", "Nightlife"],
            ["Amsterdam"] = ["Culture", "History", "Nightlife"],
            ["Malaga"] = ["Beach", "Culture", "Food", "Relaxation"],
            ["Seville"] = ["Culture", "History", "Food"],
            ["Madeira"] = ["Nature", "Adventure", "Relaxation", "Mountains"],
            ["Tenerife"] = ["Beach", "Nature", "Adventure", "Relaxation"],
            ["Marrakesh"] = ["Culture", "History", "Food", "Shopping"],
            ["Cairo"] = ["Culture", "History", "Adventure"]
        };

        var interests = await dbContext.Interests.AsNoTracking().ToListAsync();
        var interestByName = interests.ToDictionary(i => i.Name, i => i.Id, StringComparer.OrdinalIgnoreCase);

        var destinationNames = mappings.Keys.ToList();
        var destinations = await dbContext.Destinations
            .Where(d => destinationNames.Contains(d.Name))
            .ToListAsync();

        var existingLinks = await dbContext.DestinationInterests
            .Select(di => new { di.DestinationId, di.InterestId })
            .ToListAsync();

        var existingSet = existingLinks
            .Select(x => (x.DestinationId, x.InterestId))
            .ToHashSet();

        var created = 0;

        foreach (var destination in destinations)
        {
            if (!mappings.TryGetValue(destination.Name, out var interestNames))
            {
                continue;
            }

            foreach (var interestName in interestNames)
            {
                if (!interestByName.TryGetValue(interestName, out var interestId))
                {
                    continue;
                }

                if (existingSet.Contains((destination.Id, interestId)))
                {
                    continue;
                }

                dbContext.DestinationInterests.Add(new DestinationInterest
                {
                    DestinationId = destination.Id,
                    InterestId = interestId
                });
                existingSet.Add((destination.Id, interestId));
                created++;
            }
        }

        if (created > 0)
        {
            await dbContext.SaveChangesAsync();
        }

        return created;
    }

    private static List<DemoDestinationSeed> GetDemoDestinations()
    {
        // CategoryIds: 1 Beach, 2 Nature, 3 Culture, 4 History, 5 Food,
        // 6 Nightlife, 7 Adventure, 8 Relaxation, 9 Mountains, 10 City Break
        return
        [
            Create(
                "Barcelona", "Spain", "Barcelona",
                "Vibrant Mediterranean city known for Gaudí architecture, beaches and food.",
                95m, 41.3874m, 2.1686m, 0.92m, "Mediterranean",
                [1, 3, 5, 10],
                [("Sagrada Familia", "Iconic unfinished basilica by Antoni Gaudí."),
                 ("Park Güell", "Colorful park with panoramic city views.")],
                [("City Walking Tour", 25m, "Guided orientation walk through Gothic Quarter.")],
                "https://cdn.getyourguide.com/image/format=avif%2Cfit=cover%2Cgravity=auto%2Cquality=60/dam/logan-armstrong-hVhfqhDYciU-unsplash-edited-MOBILE-HEADER.jpg"),

            Create(
                "Rome", "Italy", "Rome",
                "Historic capital with ancient ruins, museums and Italian cuisine.",
                110m, 41.9028m, 12.4964m, 0.95m, "Mediterranean",
                [3, 4, 5, 10],
                [("Colosseum", "Ancient amphitheatre and symbol of Imperial Rome."),
                 ("Vatican Museums", "Extensive museum complex including the Sistine Chapel.")],
                [("Colosseum Guided Tour", 45m, "Skip-the-line guided visit of the Colosseum.")],
                "https://res.klook.com/image/upload/fl_lossy.progressive,q_60/Mobile/City/afmqgg5h0jl9wnr1dfmf.jpg"),

            Create(
                "Paris", "France", "Paris",
                "Romantic capital famous for art, fashion and landmarks.",
                130m, 48.8566m, 2.3522m, 0.97m, "Temperate",
                [3, 4, 5, 10],
                [("Eiffel Tower", "Landmark iron tower on the Champ de Mars."),
                 ("Louvre Museum", "World-renowned art museum home to the Mona Lisa.")],
                [],
                "https://media-cdn.tripadvisor.com/media/photo-c/1280x250/17/15/6d/d6/paris.jpg"),

            Create(
                "Lisbon", "Portugal", "Lisbon",
                "Hilly coastal capital with tram rides, viewpoints and seafood.",
                80m, 38.7223m, -9.1393m, 0.88m, "Mediterranean",
                [1, 3, 5, 10],
                [("Belém Tower", "Manueline fortress at the Tagus river mouth."),
                 ("Jerónimos Monastery", "Historic monastery and UNESCO site.")],
                [],
                "https://dynamic-media-cdn.tripadvisor.com/media/photo-o/2c/50/c7/a6/caption.jpg?w=1200&h=-1&s=1&cx=4096&cy=2732&chk=v1_590889ee7dd671bade3e"),

            Create(
                "Athens", "Greece", "Athens",
                "Ancient capital combining classical sites and lively neighborhoods.",
                75m, 37.9838m, 23.7275m, 0.86m, "Mediterranean",
                [3, 4, 5, 10],
                [("Acropolis", "Ancient citadel with the Parthenon."),
                 ("Acropolis Museum", "Modern museum dedicated to Acropolis finds.")],
                [],
                "https://dynamic-media-cdn.tripadvisor.com/media/photo-o/29/94/f8/c3/caption.jpg?w=1200&h=1600&s=1&cx=1059&cy=707&chk=v1_dbc744db8746e4bc888e"),

            Create(
                "Santorini", "Greece", "Fira",
                "Cycladic island known for caldera views, sunsets and beaches.",
                140m, 36.3932m, 25.4615m, 0.91m, "Mediterranean",
                [1, 2, 8],
                [("Oia Sunset Viewpoint", "Famous cliffside sunset overlooking the caldera."),
                 ("Red Beach", "Distinctive volcanic red-sand beach near Akrotiri.")],
                [],
                "https://media.cntraveller.com/photos/611be9bb69410e829d87e0c2/1:1/w_1280,h_1280,c_limit/Blue-domed-church-along-caldera-edge-in-Oia-Santorini-greece-conde-nast-traveller-11aug17-iStock.jpg"),

            Create(
                "Istanbul", "Turkey", "Istanbul",
                "City spanning Europe and Asia with bazaars, mosques and cuisine.",
                70m, 41.0082m, 28.9784m, 0.93m, "Temperate",
                [3, 4, 5, 6, 10],
                [("Hagia Sophia", "Historic mosque and former cathedral."),
                 ("Grand Bazaar", "One of the oldest and largest covered markets.")],
                [("Bosphorus Cruise", 30m, "Scenic boat cruise between Europe and Asia.")],
                "https://media.cntravellerme.com/photos/6a69f82cf005b0de89801654/1:1/w_3024,h_3024,c_limit/2172850160"),

            Create(
                "Dubrovnik", "Croatia", "Dubrovnik",
                "Walled Adriatic city with historic Old Town and coastal views.",
                100m, 42.6507m, 18.0944m, 0.90m, "Mediterranean",
                [1, 3, 4, 8],
                [("City Walls", "Medieval fortifications encircling the Old Town."),
                 ("Lokrum Island", "Nature reserve island near the Old Town.")],
                [("Old Town Walking Tour", 20m, "Guided walk through Dubrovnik Old Town.")],
                "https://cdn.croatia.hr/mediagallery-dxp-production/_heroBig_stari-grad-dubrovnik-pogled-sa-srdja-luka-esenko-HTZ_sm_10141.jpg"),

            Create(
                "Split", "Croatia", "Split",
                "Dalmatian hub built around Diocletian's Palace and seafront.",
                85m, 43.5081m, 16.4402m, 0.84m, "Mediterranean",
                [1, 3, 4, 5],
                [("Diocletian's Palace", "Roman palace complex in the city center."),
                 ("Riva Promenade", "Waterfront promenade with cafes and views.")],
                []),

            Create(
                "Kotor", "Montenegro", "Kotor",
                "Bay-side town with medieval walls and mountain backdrop.",
                70m, 42.4247m, 18.7712m, 0.82m, "Mediterranean",
                [2, 3, 4, 9],
                [("Kotor Old Town", "UNESCO-listed medieval old town."),
                 ("Kotor Fortress", "Hilltop fortress with bay panoramas.")],
                []),

            Create(
                "Budva", "Montenegro", "Budva",
                "Beach resort town with nightlife and a compact old town.",
                75m, 42.2864m, 18.8400m, 0.80m, "Mediterranean",
                [1, 6, 8],
                [("Budva Old Town", "Fortified historic core by the sea."),
                 ("Mogren Beach", "Popular sandy beach near the old town.")],
                []),

            Create(
                "Vienna", "Austria", "Vienna",
                "Imperial capital known for palaces, classical music and cafes.",
                120m, 48.2082m, 16.3738m, 0.89m, "Continental",
                [3, 4, 5, 10],
                [("Schönbrunn Palace", "Former imperial summer residence."),
                 ("St. Stephen's Cathedral", "Gothic cathedral in the city center.")],
                [],
                "https://res.klook.com/image/upload/w_750,h_469,c_fill,q_85/w_80,x_15,y_15,g_south_west,l_Klook_water_br_trans_yhcmh3/activities/uztbdmn0iu506yaxrywj.jpg"),

            Create(
                "Prague", "Czech Republic", "Prague",
                "Historic city with castle views, bridges and beer culture.",
                75m, 50.0755m, 14.4378m, 0.90m, "Continental",
                [3, 4, 5, 10],
                [("Charles Bridge", "Historic stone bridge lined with statues."),
                 ("Prague Castle", "Large castle complex overlooking the city.")],
                [],
                "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSByKzGJMRUcr3QM_m3qIZ98rhZTwWLN2HpaJhjsxKcyzFmkurkEiv4twE&s=10"),

            Create(
                "Amsterdam", "Netherlands", "Amsterdam",
                "Canal city with museums, cycling culture and lively districts.",
                125m, 52.3676m, 4.9041m, 0.91m, "Temperate",
                [3, 5, 6, 10],
                [("Rijksmuseum", "National museum of Dutch arts and history."),
                 ("Anne Frank House", "Historic museum in the former hiding place.")],
                [],
                "https://media.cntraveller.com/photos/6925c058c8997bceeb7ade5c/16:9/w_2560%2Cc_limit/Canal-Bicycles-Amsterdam-GettyImages-2186230864.jpg"),

            Create(
                "Malaga", "Spain", "Malaga",
                "Andalusian coastal city with beaches, museums and tapas.",
                85m, 36.7213m, -4.4214m, 0.83m, "Mediterranean",
                [1, 3, 5, 8],
                [("Alcazaba of Málaga", "Moorish fortress overlooking the city."),
                 ("Picasso Museum", "Museum dedicated to Pablo Picasso.")],
                []),

            Create(
                "Seville", "Spain", "Seville",
                "Andalusian capital known for flamenco, plazas and historic sites.",
                90m, 37.3891m, -5.9845m, 0.85m, "Mediterranean",
                [3, 4, 5, 10],
                [("Seville Cathedral", "Gothic cathedral with the Giralda tower."),
                 ("Alcázar of Seville", "Royal palace complex with gardens.")],
                []),

            Create(
                "Madeira", "Portugal", "Funchal",
                "Atlantic island with dramatic cliffs, levadas and mild climate.",
                95m, 32.6669m, -16.9241m, 0.81m, "Subtropical",
                [2, 7, 8, 9],
                [("Cabo Girão", "High sea cliff viewpoint near Câmara de Lobos."),
                 ("Monte Palace Tropical Garden", "Lush gardens above Funchal.")],
                []),

            Create(
                "Tenerife", "Spain", "Santa Cruz de Tenerife",
                "Canary Island destination with beaches and volcanic landscapes.",
                90m, 28.4636m, -16.2518m, 0.84m, "Subtropical",
                [1, 2, 7, 9],
                [("Teide National Park", "Volcanic national park with Mount Teide."),
                 ("Los Gigantes Cliffs", "Dramatic coastal cliffs on the west coast.")],
                []),

            Create(
                "Marrakesh", "Morocco", "Marrakesh",
                "Imperial city with souks, riads and vibrant medina life.",
                65m, 31.6295m, -7.9811m, 0.87m, "Semi-arid",
                [3, 4, 5, 7],
                [("Jemaa el-Fnaa", "Famous square and marketplace in the medina."),
                 ("Bahia Palace", "19th-century palace with ornate courtyards.")],
                [],
                "https://cdn.kimkim.com/files/a/images/b7e8e21fe7379c039866a6b92deb8e66a85f6741/big-3f3187ff367101f551cc1d442386e381.jpg"),

            Create(
                "Cairo", "Egypt", "Cairo",
                "Historic capital near the Giza pyramids and Nile riverfront.",
                60m, 30.0444m, 31.2357m, 0.88m, "Desert",
                [3, 4, 7],
                [("Giza Pyramid Complex", "Ancient pyramids and the Great Sphinx."),
                 ("Egyptian Museum", "Museum of ancient Egyptian antiquities.")],
                [("Giza Pyramids Tour", 55m, "Guided tour of the Giza pyramid plateau.")],
                "https://www.saltinourhair.com/wp-content/uploads/2019/03/cairo-pyramids-giza.jpg")
        ];
    }

    private static DemoDestinationSeed Create(
        string name,
        string country,
        string city,
        string description,
        decimal averageDailyCost,
        decimal latitude,
        decimal longitude,
        decimal popularity,
        string climate,
        int[] categoryIds,
        (string Name, string Description)[] attractions,
        (string Name, decimal Price, string Description)[] activities,
        string? imageUrl = null)
    {
        return new DemoDestinationSeed
        {
            Destination = new Destination
            {
                Name = name,
                Country = country,
                City = city,
                Description = description,
                AverageDailyCost = averageDailyCost,
                Latitude = latitude,
                Longitude = longitude,
                Popularity = popularity,
                Climate = climate,
                ImageUrl = imageUrl
            },
            CategoryIds = categoryIds,
            Attractions = attractions
                .Select(a => new Attraction
                {
                    Name = a.Name,
                    Description = a.Description,
                    Location = city
                })
                .ToList(),
            Activities = activities
                .Select(a => new Activity
                {
                    Name = a.Name,
                    Provider = "Local Demo",
                    Price = a.Price,
                    Currency = "EUR",
                    Description = a.Description
                })
                .ToList()
        };
    }

    private sealed class DemoDestinationSeed
    {
        public Destination Destination { get; set; } = null!;
        public int[] CategoryIds { get; set; } = [];
        public List<Attraction> Attractions { get; set; } = [];
        public List<Activity> Activities { get; set; } = [];
    }
}
