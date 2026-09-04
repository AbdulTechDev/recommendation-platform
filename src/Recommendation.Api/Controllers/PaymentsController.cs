using Microsoft.AspNetCore.Mvc;
using Recommendation.Api.Data;
using Recommendation.Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace Recommendation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PaymentsController(AppDbContext db) : ControllerBase
{
    [HttpPost("process")]
    [Authorize(Policy = "UserOrAdmin")]
    public async Task<ActionResult<Payment>> ProcessPayment(Payment req, CancellationToken ct)
    {
        // Simple stub: mark payment completed and persist
        req.Status = "Completed";
        db.Payments.Add(req);
        await db.SaveChangesAsync(ct);
        return Ok(req);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "UserOrAdmin")]
    public async Task<ActionResult<Payment>> GetPayment(int id, CancellationToken ct)
    {
        var p = await db.Payments.FindAsync(new object?[] { id }, ct);
        if (p is null) return NotFound();
        return Ok(p);
    }
}
