namespace Recommendation.Api.Models;

public sealed class InventoryItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int QuantityAvailable { get; set; }
}
