using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recommendation.Api.Data;
using Recommendation.Api.Models;

namespace Recommendation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UserInteractionsController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserInteraction>>> GetInteractions(CancellationToken ct)
    {
        return Ok(await dbContext.UserInteractions.AsNoTracking().ToListAsync(ct));
    }

    [HttpPost]
    public async Task<ActionResult<UserInteraction>> CreateInteraction(UserInteraction interaction, CancellationToken ct)
    {
        dbContext.UserInteractions.Add(interaction);
        await dbContext.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetInteractions), new { id = interaction.Id }, interaction);
    }
}
