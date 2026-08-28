using Recommendation.Api.Models;

namespace Recommendation.Api.Services;

public interface IRecommendationService
{
    Task<RecommendationResponse?> GetRecommendationsAsync(RecommendationRequest request, CancellationToken cancellationToken = default);
}
