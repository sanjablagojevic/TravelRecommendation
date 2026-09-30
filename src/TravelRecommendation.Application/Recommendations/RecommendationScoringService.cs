using TravelRecommendation.Application.DTOs.Recommendations;

namespace TravelRecommendation.Application.Recommendations;

public sealed class RecommendationScoringService : IRecommendationScoringService
{
    private readonly RecommendationWeights _weights;

    private static readonly Dictionary<string, string[]> TripTypeMappings =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Beach"] = ["Beach"],
            ["City Break"] = ["City Break"],
            ["Nature"] = ["Nature"],
            ["Culture"] = ["Culture"],
            ["Adventure"] = ["Adventure"],
            ["Relaxation"] = ["Relaxation"],
            ["Mountains"] = ["Mountains"]
        };

    public RecommendationScoringService(RecommendationWeights weights)
    {
        weights.EnsureValid();
        _weights = weights;
    }

    public DestinationScoreResult ScoreDestination(
        PreferenceScoringInput preference,
        DestinationScoringInput destination)
    {
        var userInterests = preference.InterestNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var destinationInterests = destination.InterestNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var matched = userInterests
            .Where(ui => destinationInterests.Any(di =>
                string.Equals(ui, di, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var missing = userInterests
            .Where(ui => !matched.Any(m => string.Equals(m, ui, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        decimal? interestScore = null;
        if (userInterests.Count > 0)
        {
            interestScore = Clamp((decimal)matched.Count / userInterests.Count);
        }

        decimal? estimatedCost = null;
        decimal? budgetScore = null;
        if (preference.Budget.HasValue && preference.Duration.HasValue && preference.Duration.Value > 0)
        {
            estimatedCost = destination.AverageDailyCost * preference.Duration.Value;
            budgetScore = CalculateBudgetScore(preference.Budget.Value, estimatedCost.Value);
        }

        decimal? tripTypeScore = null;
        if (!string.IsNullOrWhiteSpace(preference.TripType))
        {
            tripTypeScore = CalculateTripTypeScore(
                preference.TripType,
                destinationInterests,
                destination.CategoryNames);
        }

        decimal? climateScore = null;
        if (!string.IsNullOrWhiteSpace(preference.PreferredClimate)
            && !string.IsNullOrWhiteSpace(destination.Climate))
        {
            climateScore = string.Equals(
                preference.PreferredClimate.Trim(),
                destination.Climate.Trim(),
                StringComparison.OrdinalIgnoreCase)
                ? 1.0m
                : 0.0m;
        }
        else if (!string.IsNullOrWhiteSpace(preference.PreferredClimate)
                 && string.IsNullOrWhiteSpace(destination.Climate))
        {
            climateScore = 0.0m;
        }

        var popularityScore = Clamp(destination.Popularity);

        var components = BuildComponents(
            interestScore,
            budgetScore,
            tripTypeScore,
            climateScore,
            popularityScore);

        var active = components.Where(c => c.IsActive).ToList();
        var activeWeightTotal = active.Sum(c => c.weight);

        decimal finalScore;
        if (activeWeightTotal <= 0)
        {
            finalScore = popularityScore;
        }
        else
        {
            finalScore = Clamp(active.Sum(c => c.weight * c.score!.Value) / activeWeightTotal);
        }

        finalScore = RoundScore(finalScore);

        var breakdown = new RecommendationScoreBreakdownDto
        {
            InterestScore = interestScore.HasValue ? RoundScore(interestScore.Value) : null,
            BudgetScore = budgetScore.HasValue ? RoundScore(budgetScore.Value) : null,
            TripTypeScore = tripTypeScore.HasValue ? RoundScore(tripTypeScore.Value) : null,
            ClimateScore = climateScore.HasValue ? RoundScore(climateScore.Value) : null,
            PopularityScore = RoundScore(popularityScore),
            ActiveWeightTotal = activeWeightTotal,
            Components = components.Select(c => new ScoreComponentDto
            {
                Name = c.name,
                Score = c.score.HasValue ? RoundScore(c.score.Value) : null,
                Weight = c.weight,
                IsActive = c.IsActive,
                WeightedValue = c.IsActive
                    ? RoundScore(c.weight * c.score!.Value)
                    : null
            }).ToList()
        };

        return new DestinationScoreResult
        {
            DestinationId = destination.Id,
            DestinationName = destination.Name,
            FinalScore = finalScore,
            EstimatedCost = estimatedCost.HasValue
                ? Math.Round(estimatedCost.Value, 2, MidpointRounding.AwayFromZero)
                : null,
            MatchedInterests = matched,
            MissingInterests = missing,
            Breakdown = breakdown,
            Explanation = BuildExplanation(
                destination.Name,
                matched,
                userInterests.Count,
                budgetScore,
                estimatedCost,
                preference.Budget,
                tripTypeScore,
                climateScore,
                active.Count == 1 && active[0].name == "Popularity")
        };
    }

    public IReadOnlyList<DestinationScoreResult> RankDestinations(
        PreferenceScoringInput preference,
        IEnumerable<DestinationScoringInput> destinations,
        int topCount)
    {
        var destinationList = destinations.ToList();
        var popularityById = destinationList.ToDictionary(d => d.Id, d => d.Popularity);

        return destinationList
            .Select(d => ScoreDestination(preference, d))
            .OrderByDescending(r => r.FinalScore)
            .ThenByDescending(r => popularityById[r.DestinationId])
            .ThenBy(r => r.DestinationId)
            .Take(topCount)
            .ToList();
    }

    public static decimal CalculateBudgetScore(decimal budget, decimal estimatedCost)
    {
        if (estimatedCost <= 0)
        {
            return 0m;
        }

        if (estimatedCost <= budget)
        {
            return 1.0m;
        }

        return Clamp(budget / estimatedCost);
    }

    private decimal CalculateTripTypeScore(
        string tripType,
        IReadOnlyList<string> destinationInterests,
        IReadOnlyList<string> categoryNames)
    {
        if (!TripTypeMappings.TryGetValue(tripType.Trim(), out var targets))
        {
            return 0.0m;
        }

        var matches = targets.Any(target =>
            destinationInterests.Any(i => string.Equals(i, target, StringComparison.OrdinalIgnoreCase))
            || categoryNames.Any(c => string.Equals(c, target, StringComparison.OrdinalIgnoreCase)));

        return matches ? 1.0m : 0.0m;
    }

    private List<(string name, decimal weight, decimal? score, bool IsActive)> BuildComponents(
        decimal? interestScore,
        decimal? budgetScore,
        decimal? tripTypeScore,
        decimal? climateScore,
        decimal popularityScore)
    {
        return
        [
            ("Interest", _weights.Interest, interestScore, interestScore.HasValue),
            ("Budget", _weights.Budget, budgetScore, budgetScore.HasValue),
            ("TripType", _weights.TripType, tripTypeScore, tripTypeScore.HasValue),
            ("Climate", _weights.Climate, climateScore, climateScore.HasValue),
            ("Popularity", _weights.Popularity, popularityScore, true)
        ];
    }

    private static string BuildExplanation(
        string destinationName,
        IReadOnlyList<string> matched,
        int totalUserInterests,
        decimal? budgetScore,
        decimal? estimatedCost,
        decimal? budget,
        decimal? tripTypeScore,
        decimal? climateScore,
        bool popularityOnly)
    {
        if (popularityOnly)
        {
            return $"{destinationName} is ranked mainly by popularity because few preference criteria were provided.";
        }

        var parts = new List<string>();

        if (totalUserInterests > 0)
        {
            parts.Add($"{destinationName} matches {matched.Count} of your {totalUserInterests} selected interests");
        }
        else
        {
            parts.Add($"{destinationName} was evaluated without selected interests");
        }

        if (budgetScore.HasValue && estimatedCost.HasValue && budget.HasValue)
        {
            if (budgetScore.Value >= 1.0m)
            {
                parts.Add("and fits within your estimated budget.");
            }
            else
            {
                var overPercent = Math.Round(((estimatedCost.Value - budget.Value) / budget.Value) * 100m, 0);
                parts.Add($"and the estimated trip cost is approximately {overPercent}% above your selected budget.");
            }
        }
        else if (parts.Count > 0)
        {
            parts[^1] = parts[^1].TrimEnd('.') + ".";
        }

        if (tripTypeScore == 1.0m)
        {
            parts.Add("It also matches your preferred trip type");
        }

        if (climateScore == 1.0m)
        {
            if (tripTypeScore == 1.0m)
            {
                parts[^1] += " and preferred climate";
            }
            else
            {
                parts.Add("It also matches your preferred climate");
            }
        }

        var text = string.Join(" ", parts).Trim();
        if (!text.EndsWith('.'))
        {
            text += ".";
        }

        return text;
    }

    private static decimal Clamp(decimal value)
    {
        if (value < 0m)
        {
            return 0m;
        }

        if (value > 1m)
        {
            return 1m;
        }

        return value;
    }

    private static decimal RoundScore(decimal value)
    {
        return Math.Round(Clamp(value), 4, MidpointRounding.AwayFromZero);
    }
}
