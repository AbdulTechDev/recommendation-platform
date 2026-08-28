using Recommendation.Api.Models;

namespace Recommendation.Api.Data;

public static class SeedData
{
    public static void EnsureSeedData(AppDbContext db)
    {
        if (db.Products.Any()) return;

        var products = new List<Product>
        {
            new Product { Name = "Wireless Headphones", Description = "Noise cancelling over-ear headphones", Price = 199.99m },
            new Product { Name = "Bluetooth Speaker", Description = "Portable speaker with deep bass", Price = 49.99m },
            new Product { Name = "USB-C Charger", Description = "Fast charger for phones and tablets", Price = 19.99m }
        };

        db.Products.AddRange(products);
        db.SaveChanges();
    }
}
