using Recommendation.Api.Models;

namespace Recommendation.Api.Data;

public static class SeedData
{
    public static void EnsureSeedData(AppDbContext db)
    {
        if (db.Products.Any()) return;

        var products = new List<Product>
        {
            new Product { Name = "Wireless Headphones", Category = "Electronics", Description = "Noise cancelling over-ear headphones", Price = 199.99m },
            new Product { Name = "Bluetooth Speaker", Category = "Electronics", Description = "Portable speaker with deep bass", Price = 49.99m },
            new Product { Name = "USB-C Charger", Category = "Accessories", Description = "Fast charger for phones and tablets", Price = 19.99m }
        };

        db.Products.AddRange(products);
        db.SaveChanges();

        // Ensure an admin user exists for development
        if (!db.Users.Any())
        {
            // default dev admin password: 'adminpass' (change in production)
            var admin = new Recommendation.Api.Models.User
            {
                Username = "admin",
                Email = "admin@example.com",
                Role = "Admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("adminpass")
            };

            db.Users.Add(admin);
            db.SaveChanges();
        }
    }
}
