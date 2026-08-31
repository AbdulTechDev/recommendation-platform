using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recommendation.Api.Data;
using Recommendation.Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace Recommendation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IReadOnlyList<Order>>> GetOrders(CancellationToken ct)
    {
        return Ok(await dbContext.Orders.AsNoTracking().ToListAsync(ct));
    }

    [HttpPost]
    [Authorize(Policy = "UserOrAdmin")]
    public async Task<ActionResult<Order>> CreateOrder(Order order, CancellationToken ct)
    {
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetOrders), new { id = order.Id }, order);
    }
}
