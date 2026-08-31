using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Recommendation.Api.Data;
using Recommendation.Api.Models;
using System.Net.Http.Json;

namespace Recommendation.Api.Tests;

public class OrdersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OrdersControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            // Ensure the app runs in the Test environment so Program.cs skips Npgsql
            builder.UseEnvironment("Test");
            builder.ConfigureAppConfiguration(cfg =>
            {
                cfg.AddInMemoryCollection(new[] { new KeyValuePair<string, string>("Jwt:Key", "test_integration_jwt_key_which_is_long_enough_for_hmac") });
            });

            builder.ConfigureServices(services =>
            {
                // Remove any existing registrations for AppDbContext or its options
                var toRemove = services.Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                                                 || d.ServiceType == typeof(AppDbContext)
                                                 || d.ImplementationType == typeof(AppDbContext))
                                       .ToList();
                foreach (var d in toRemove) services.Remove(d);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("OrdersTestDb"));

                // seed admin user
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();
                    if (!db.Users.Any(u => u.Username == "testadmin"))
                    {
                        db.Users.Add(new User { Username = "testadmin", Email = "admin@test", Role = "Admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("adminpass") });
                        db.SaveChanges();
                    }
                }
            });
        });
    }

    [Fact]
    public async Task PostOrder_ThenGet_ReturnsCreatedAndContainsOrder()
    {
        var client = _factory.CreateClient();

        var order = new Order { UserId = 1, Total = 42.50m };
        var postResp = await client.PostAsJsonAsync("/api/orders", order);
        postResp.EnsureSuccessStatusCode();

        var getResp = await client.GetAsync("/api/orders");
        getResp.EnsureSuccessStatusCode();
        var orders = await getResp.Content.ReadFromJsonAsync<List<Order>>();
        Assert.NotNull(orders);
        Assert.Contains(orders!, o => o.UserId == 1 && o.Total == 42.50m);
    }
}
