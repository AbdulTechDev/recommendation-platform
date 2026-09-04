using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recommendation.Api.Data;
using Recommendation.Api.Models;

namespace Recommendation.Api.Controllers;

using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(AppDbContext dbContext) : ControllerBase
{
	[HttpGet]
	[AllowAnonymous]
	public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts(CancellationToken cancellationToken)
	{
		return Ok(await dbContext.Products.AsNoTracking().ToListAsync(cancellationToken));
	}

	[HttpGet("{id}")]
	[AllowAnonymous]
	public async Task<ActionResult<Product>> GetProduct(int id, CancellationToken cancellationToken)
	{
		var p = await dbContext.Products.FindAsync(new object?[] { id }, cancellationToken);
		if (p is null) return NotFound();
		return Ok(p);
	}

	[HttpPost]
	[Authorize(Policy = "AdminOnly")]
	public async Task<ActionResult<Product>> CreateProduct(Product product, CancellationToken cancellationToken)
	{
		dbContext.Products.Add(product);
		await dbContext.SaveChangesAsync(cancellationToken);

		return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
	}

	[HttpPut("{id}")]
	[Authorize(Policy = "AdminOnly")]
	public async Task<ActionResult> UpdateProduct(int id, Product updated, CancellationToken cancellationToken)
	{
		var p = await dbContext.Products.FindAsync(new object?[] { id }, cancellationToken);
		if (p is null) return NotFound();
		p.Name = updated.Name;
		p.Category = updated.Category;
		p.Description = updated.Description;
		p.Price = updated.Price;
		await dbContext.SaveChangesAsync(cancellationToken);
		return NoContent();
	}

	[HttpDelete("{id}")]
	[Authorize(Policy = "AdminOnly")]
	public async Task<ActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
	{
		var p = await dbContext.Products.FindAsync(new object?[] { id }, cancellationToken);
		if (p is null) return NotFound();
		dbContext.Products.Remove(p);
		await dbContext.SaveChangesAsync(cancellationToken);
		return NoContent();
	}
}
