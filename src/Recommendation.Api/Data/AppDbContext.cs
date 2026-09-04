using Microsoft.EntityFrameworkCore;
using Recommendation.Api.Models;

namespace Recommendation.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<Product> Products => Set<Product>();
	public DbSet<User> Users => Set<User>();
	public DbSet<Order> Orders => Set<Order>();
	public DbSet<UserInteraction> UserInteractions => Set<UserInteraction>();

	// E-commerce related sets
	public DbSet<Category> Categories => Set<Category>();
	public DbSet<Cart> Carts => Set<Cart>();
	public DbSet<CartItem> CartItems => Set<CartItem>();
	public DbSet<Review> Reviews => Set<Review>();
	public DbSet<InventoryItem> Inventory => Set<InventoryItem>();
	public DbSet<Payment> Payments => Set<Payment>();
}
