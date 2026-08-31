using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recommendation.Api.Data;
using Recommendation.Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace Recommendation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IReadOnlyList<User>>> GetUsers(CancellationToken ct)
    {
        return Ok(await dbContext.Users.AsNoTracking().ToListAsync(ct));
    }

    public sealed record RegisterRequest(string Username, string Email, string Password);

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<User>> CreateUser(RegisterRequest req, CancellationToken ct)
    {
        var user = new User
        {
            Username = req.Username,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
    }
}
