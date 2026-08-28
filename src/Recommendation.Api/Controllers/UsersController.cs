using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recommendation.Api.Data;
using Recommendation.Api.Models;

namespace Recommendation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<User>>> GetUsers(CancellationToken ct)
    {
        return Ok(await dbContext.Users.AsNoTracking().ToListAsync(ct));
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(User user, CancellationToken ct)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
    }
}
