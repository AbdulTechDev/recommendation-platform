using System.Net.Http.Json;
using Recommendation.Api.Models;

namespace Recommendation.Api.Services;

public class RecommendationService
{
    private readonly HttpClient _httpClient;

    public RecommendationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<RecommendationResponse?> GetRecommendationsAsync(
        RecommendationRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/recommendations",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<RecommendationResponse>(
            cancellationToken);
    }
}