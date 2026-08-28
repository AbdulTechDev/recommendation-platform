using Recommendation.Api.Models;
using Recommendation.Api.Services;
using System.Collections.Generic;

namespace Recommendation.Api.Tests.TestDoubles;

public class FakeRecommendationService : IRecommendationService
{
    public Task<RecommendationResponse?> GetRecommendationsAsync(RecommendationRequest request, CancellationToken cancellationToken = default)
    {
        var list = new List<RecommendationItem>
        {
            new RecommendationItem(new Product { Id = 1, Name = "Dummy", Category = "Test", Description = "", Price = 1.0M }, 0.9)
        };

        var resp = new RecommendationResponse(request.UserId, request.Query, list);

        return Task.FromResult<RecommendationResponse?>(resp);
    }
}
