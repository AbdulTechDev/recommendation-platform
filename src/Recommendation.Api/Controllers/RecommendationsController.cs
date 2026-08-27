using Microsoft.AspNetCore.Mvc;
using Recommendation.Api.Models;
using Recommendation.Api.Services;

namespace Recommendation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RecommendationsController : ControllerBase
{
	private readonly RecommendationService _recommendationService;

	public RecommendationsController(RecommendationService recommendationService)
	{
		_recommendationService = recommendationService;
	}

	[HttpPost]
	public async Task<ActionResult<RecommendationResponse>> GetRecommendations(
		RecommendationRequest request,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(request.Query))
		{
			return BadRequest("Query is required.");
		}

		if (request.TopN is < 1 or > 100)
		{
			return BadRequest("TopN must be between 1 and 100.");
		}

		var recommendations = await _recommendationService.GetRecommendationsAsync(
			request,
			cancellationToken);

		return Ok(recommendations);
	}
}
