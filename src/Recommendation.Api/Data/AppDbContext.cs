using Microsoft.EntityFrameworkCore;
using Recommendation.Api.Models;

namespace Recommendation.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<Product> Products => Set<Product>();
}
