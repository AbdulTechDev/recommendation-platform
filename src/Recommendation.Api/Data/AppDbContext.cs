using Microsoft.EntityFrameworkCore;
using Recommendation.Api.Models;

namespace Recommendation.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<Product> Products => Set<Product>();
	public DbSet<User> Users => Set<User>();
	public DbSet<Order> Orders => Set<Order>();
	public DbSet<UserInteraction> UserInteractions => Set<UserInteraction>();
}
