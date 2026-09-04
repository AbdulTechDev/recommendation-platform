using Microsoft.AspNetCore.Mvc;
using Recommendation.Api.Data;
using Microsoft.EntityFrameworkCore;
using Recommendation.Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace Recommendation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public sealed class InventoryController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InventoryItem>>> GetAll(CancellationToken ct)
    {
        return Ok(await db.Inventory.AsNoTracking().ToListAsync(ct));
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult> UpdateQuantity(int id, InventoryItem update, CancellationToken ct)
    {
        var it = await db.Inventory.FindAsync(new object?[] { id }, ct);
        if (it is null) return NotFound();
        it.QuantityAvailable = update.QuantityAvailable;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
