namespace TravelRecommendation.Application.Recommendations;

public interface IRecommendationScoringService
{
    DestinationScoreResult ScoreDestination(
        PreferenceScoringInput preference,
        DestinationScoringInput destination);

    IReadOnlyList<DestinationScoreResult> RankDestinations(
        PreferenceScoringInput preference,
        IEnumerable<DestinationScoringInput> destinations,
        int topCount);
}
