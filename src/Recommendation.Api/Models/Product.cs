namespace Recommendation.Api.Models;

public sealed class Product
{
	public int Id { get; set; }
	public required string Name { get; set; }
	public required string Category { get; set; }
	public required string Description { get; set; }
	public decimal Price { get; set; }
}
