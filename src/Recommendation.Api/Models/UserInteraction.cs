using System;

namespace Recommendation.Api.Models;

public sealed class UserInteraction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public string InteractionType { get; set; } = "view"; // view, click, purchase, rating
    public int? Value { get; set; } // e.g., rating value
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
