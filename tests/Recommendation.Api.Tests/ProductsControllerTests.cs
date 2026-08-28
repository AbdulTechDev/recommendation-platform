using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Recommendation.Api.Data;
using Recommendation.Api.Models;
using System.Net.Http.Json;

namespace Recommendation.Api.Tests;

public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProductsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            });
        });
    }

    [Fact]
    public async Task PostProduct_ThenGet_ReturnsCreatedAndContainsProduct()
    {
        var client = _factory.CreateClient();

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
