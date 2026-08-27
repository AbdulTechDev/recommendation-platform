public class RecommendationService
{
    private readonly HttpClient _httpClient;

    public RecommendationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetRecommendationsAsync(
        object request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "http://localhost:8000/api/recommendations",
            request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}