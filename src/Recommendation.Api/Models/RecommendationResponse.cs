using System.Text.Json.Serialization;

namespace Recommendation.Api.Models;

public sealed record RecommendationResponse(
	[property: JsonPropertyName("user_id")] int UserId,
	string Query,
	IReadOnlyList<RecommendationItem> Recommendations);

public sealed record RecommendationItem(
	Product Product,
	double Score);
