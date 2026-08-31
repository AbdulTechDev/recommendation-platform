using Microsoft.AspNetCore.Mvc;
using Recommendation.Api.Models;
using Recommendation.Api.Services;
using Microsoft.AspNetCore.Authorization;

namespace Recommendation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RecommendationsController : ControllerBase
{
	private readonly IRecommendationService _recommendationService;

	public RecommendationsController(IRecommendationService recommendationService)
	{
		_recommendationService = recommendationService;
	}

	[HttpPost]
	[Authorize(Policy = "UserOrAdmin")]
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
