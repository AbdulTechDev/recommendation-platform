using System;

namespace Recommendation.Api.Models;

// Simple payment stub for development/testing. Replace with real gateway integration.
public sealed class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
