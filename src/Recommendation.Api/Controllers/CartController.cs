using Microsoft.AspNetCore.Mvc;
using Recommendation.Api.Data;
using Microsoft.EntityFrameworkCore;
using Recommendation.Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace Recommendation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "UserOrAdmin")]
public sealed class CartController(AppDbContext db) : ControllerBase
{
    // For simplicity, cart is identified by userId in requests.

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<Cart>> GetCartForUser(int userId, CancellationToken ct)
    {
        var cart = await db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId, ct);
        if (cart is null)
        {
            cart = new Cart { UserId = userId };
            db.Carts.Add(cart);
            await db.SaveChangesAsync(ct);
        }
        return Ok(cart);
    }

    [HttpPost("user/{userId}/items")]
    public async Task<ActionResult<CartItem>> AddItem(int userId, CartItem item, CancellationToken ct)
    {
        var cart = await db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId, ct);
        if (cart is null)
        {
            cart = new Cart { UserId = userId };
            db.Carts.Add(cart);
            await db.SaveChangesAsync(ct);
        }
        item.CartId = cart.Id;
        db.CartItems.Add(item);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetCartForUser), new { userId }, item);
    }

    [HttpDelete("items/{id}")]
    public async Task<ActionResult> RemoveItem(int id, CancellationToken ct)
    {
        var it = await db.CartItems.FindAsync(new object?[] { id }, ct);
        if (it is null) return NotFound();
        db.CartItems.Remove(it);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("user/{userId}/checkout")]
    public async Task<ActionResult> Checkout(int userId, CancellationToken ct)
    {
        var cart = await db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId, ct);
        if (cart is null || !cart.Items.Any()) return BadRequest("Cart is empty");

        // run checkout in a transaction to validate and decrement inventory atomically
        const int maxAttempts = 5;
        var attempt = 0;
        while (true)
        {
            attempt++;
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            try
            {
                // validate inventory for each item
                foreach (var it in cart.Items)
                {
                    // Lock the inventory row for update to prevent race conditions on concurrent checkouts
                    InventoryItem? inv;
                    if (db.Database.IsRelational())
                    {
                        inv = await db.Inventory.FromSqlInterpolated($"SELECT * FROM \"InventoryItems\" WHERE \"ProductId\" = {it.ProductId} FOR UPDATE").FirstOrDefaultAsync(ct);
                    }
                    else
                    {
                        // In-memory provider doesn't support FOR UPDATE; fall back to simple read (tests)
                        inv = await db.Inventory.FirstOrDefaultAsync(x => x.ProductId == it.ProductId, ct);
                    }
                    if (inv is null)
                    {
                        await tx.RollbackAsync(ct);
                        return BadRequest($"No inventory record for product {it.ProductId}");
                    }
                    if (inv.QuantityAvailable < it.Quantity)
                    {
                        await tx.RollbackAsync(ct);
                        return BadRequest($"Insufficient stock for product {it.ProductId}");
                    }
                    inv.QuantityAvailable -= it.Quantity;
                    db.Inventory.Update(inv);
                }

                var order = new Order { UserId = userId, Total = cart.Items.Sum(i => i.UnitPrice * i.Quantity) };
                db.Orders.Add(order);
                await db.SaveChangesAsync(ct); // get order.Id

                // create payment stub
                var payment = new Payment { OrderId = order.Id, Amount = order.Total, Status = "Completed" };
                db.Payments.Add(payment);

                // clear cart
                db.CartItems.RemoveRange(cart.Items);

                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return Ok(new { orderId = order.Id, paymentId = payment.Id });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                // If this is a transient serialization or deadlock error from Postgres, retry
                var isTransient = false;
                try
                {
                    if (ex is Npgsql.PostgresException pex)
                    {
                        // 40001 = serialization_failure, 40P01 = deadlock_detected
                        if (pex.SqlState == "40001" || pex.SqlState == "40P01") isTransient = true;
                    }
                    else if (ex.InnerException is Npgsql.PostgresException pex2)
                    {
                        if (pex2.SqlState == "40001" || pex2.SqlState == "40P01") isTransient = true;
                    }
                }
                catch { }

                if (isTransient && attempt < maxAttempts)
                {
                    // backoff and retry
                    await Task.Delay(50 * attempt, ct);
                    continue;
                }

                return StatusCode(500, "Checkout failed: " + ex.Message);
            }
        }
    }
}
