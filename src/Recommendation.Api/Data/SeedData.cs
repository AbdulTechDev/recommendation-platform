using Recommendation.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Recommendation.Api.Data;

public static class SeedData
{
    public static void EnsureSeedData(AppDbContext db)
    {
        List<Product> products = new();
        if (!db.Products.Any())
        {
            products = new List<Product>
            {
                new Product { Name = "Wireless Headphones", Category = "Electronics", Description = "Noise cancelling over-ear headphones", Price = 199.99m },
                new Product { Name = "Bluetooth Speaker", Category = "Electronics", Description = "Portable speaker with deep bass", Price = 49.99m },
                new Product { Name = "USB-C Charger", Category = "Accessories", Description = "Fast charger for phones and tablets", Price = 19.99m }
            };

            db.Products.AddRange(products);
            db.SaveChanges();
        }

        // Seed categories from products (use DB products if local list is empty)
        var productSource = products.Any() ? products : db.Products.AsNoTracking().ToList();
        var categories = productSource.Select(p => p.Category).Distinct()
            .Select(name => new Recommendation.Api.Models.Category { Name = name, Description = name + " products" }).ToList();
        if (!db.Categories.Any())
        {
            db.Categories.AddRange(categories);
            db.SaveChanges();
        }

        // Seed inventory entries for each product (simple default stock)
        if (!db.Inventory.Any())
        {
            var inventory = db.Products.AsNoTracking().Select(p => new Recommendation.Api.Models.InventoryItem { ProductId = p.Id, QuantityAvailable = 100 }).ToList();
            db.Inventory.AddRange(inventory);
            db.SaveChanges();
        }

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
