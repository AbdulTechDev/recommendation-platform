using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recommendation.Api.Data;
using Recommendation.Api.Models;

namespace Recommendation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(AppDbContext dbContext) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts(
		CancellationToken cancellationToken)
	{
		return Ok(await dbContext.Products.AsNoTracking().ToListAsync(cancellationToken));
	}

	[HttpPost]
	public async Task<ActionResult<Product>> CreateProduct(
		Product product,
		CancellationToken cancellationToken)
	{
		dbContext.Products.Add(product);
		await dbContext.SaveChangesAsync(cancellationToken);

		return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
	}
}
