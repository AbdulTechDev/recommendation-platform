using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Recommendation.Api.Data;
using Recommendation.Api.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Recommendation.Api.Tests;

public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProductsControllerTests(WebApplicationFactory<Program> factory)
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
                        options.UseInMemoryDatabase("ProductsTestDb"));

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
    public async Task PostProduct_ThenGet_ReturnsCreatedAndContainsProduct()
    {
        var client = _factory.CreateClient();
        // authenticate as seeded admin
        var loginPayload = JsonSerializer.Serialize(new { username = "testadmin", password = "adminpass" });
        var loginResp = await client.PostAsync("/api/auth/token", new StringContent(loginPayload, Encoding.UTF8, "application/json"));
        loginResp.EnsureSuccessStatusCode();
        var loginBody = await loginResp.Content.ReadAsStringAsync();
        using var loginDoc = JsonDocument.Parse(loginBody);
        var token = loginDoc.RootElement.GetProperty("token").GetString();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var product = new Product { Name = "Test Item", Category = "TestCategory", Description = "desc", Price = 9.99m };
        var postResp = await client.PostAsJsonAsync("/api/products", product);
        postResp.EnsureSuccessStatusCode();

        var getResp = await client.GetAsync("/api/products");
        getResp.EnsureSuccessStatusCode();
        var products = await getResp.Content.ReadFromJsonAsync<List<Product>>();
        Assert.NotNull(products);
        Assert.Contains(products!, p => p.Name == "Test Item");
    }
}
