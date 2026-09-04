using Microsoft.AspNetCore.Mvc;
using Recommendation.Api.Data;
using Microsoft.EntityFrameworkCore;
using Recommendation.Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace Recommendation.Api.Controllers;

[ApiController]
[Route("api/products/{productId}/[controller]")]
public sealed class ReviewsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Review>>> GetForProduct(int productId, CancellationToken ct)
    {
        var reviews = await db.Reviews.AsNoTracking().Where(r => r.ProductId == productId).ToListAsync(ct);
        return Ok(reviews);
    }

    [HttpPost]
    [Authorize(Policy = "UserOrAdmin")]
    public async Task<ActionResult<Review>> CreateForProduct(int productId, Review review, CancellationToken ct)
    {
        review.ProductId = productId;
        db.Reviews.Add(review);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetForProduct), new { productId }, review);
    }
}
