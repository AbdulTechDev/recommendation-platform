namespace Recommendation.Api.Models;

public sealed record RecommendationRequest(
	int UserId,
	string Query,
	int TopN = 5);
