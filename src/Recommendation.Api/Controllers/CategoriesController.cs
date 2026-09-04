using Microsoft.AspNetCore.Mvc;
using Recommendation.Api.Data;
using Microsoft.EntityFrameworkCore;
using Recommendation.Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace Recommendation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CategoriesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Category>>> GetCategories(CancellationToken ct)
    {
        return Ok(await db.Categories.AsNoTracking().ToListAsync(ct));
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<Category>> CreateCategory(Category c, CancellationToken ct)
    {
        db.Categories.Add(c);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetCategories), new { id = c.Id }, c);
    }
}
