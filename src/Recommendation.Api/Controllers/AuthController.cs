using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Recommendation.Api.Data;
using Recommendation.Api.Models;

namespace Recommendation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(AppDbContext dbContext) : ControllerBase
{
    [HttpPost("token")]
    public ActionResult GetToken([FromBody] LoginRequest req)
    {
        // find user by username
        var user = dbContext.Users.SingleOrDefault(u => u.Username == req.Username);
        if (user is null || string.IsNullOrEmpty(user.PasswordHash))
        {
            return Unauthorized();
        }

        // verify password
        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
        {
            return Unauthorized();
        }

        var key = HttpContext.RequestServices
            .GetRequiredService<IConfiguration>()["Jwt:Key"] ?? "dev_secret_key_please_change";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role ?? "User")
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);

        return Ok(new { token = jwt });
    }
}

public sealed record LoginRequest(string Username, string Password);
