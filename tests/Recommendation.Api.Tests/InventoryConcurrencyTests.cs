using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Recommendation.Api.Data;
using Recommendation.Api.Models;
using Xunit;

namespace Recommendation.Api.Tests;

public class InventoryConcurrencyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public InventoryConcurrencyTests(WebApplicationFactory<Program> factory)
    {
        var webFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");
            builder.ConfigureAppConfiguration(cfg =>
            {
                cfg.AddInMemoryCollection(new[] { new KeyValuePair<string, string>("Jwt:Key", "test_integration_jwt_key_which_is_long_enough_for_hmac") });
            });
            builder.ConfigureServices(services =>
            {
                var dbOptionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (dbOptionsDescriptor != null) services.Remove(dbOptionsDescriptor);
                var appDbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(AppDbContext));
                if (appDbContextDescriptor != null) services.Remove(appDbContextDescriptor);

                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("InvTestDb").ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();
                    // seed a product and inventory
                    if (!db.Products.Any())
                    {
                        db.Products.Add(new Product { Name = "InvProduct", Category = "Test", Description = "D", Price = 1.0m });
                        db.SaveChanges();
                    }
                    var prod = db.Products.First();
                    if (!db.Inventory.Any(i => i.ProductId == prod.Id))
                    {
                        db.Inventory.Add(new InventoryItem { ProductId = prod.Id, QuantityAvailable = 1 });
                        db.SaveChanges();
                    }
                }
            });
        });

        _client = webFactory.CreateClient();
    }

    [Fact]
    public async Task SequentialCheckouts_DecrementInventory_SecondFails()
    {
        // create user
        var payload = JsonSerializer.Serialize(new { username = "icuser", email = "ic@example.com", password = "pwd" });
        var res = await _client.PostAsync("/api/users", new StringContent(payload, Encoding.UTF8, "application/json"));
        res.EnsureSuccessStatusCode();

        // find product id
        var prods = JsonDocument.Parse(await (await _client.GetAsync("/api/products")).Content.ReadAsStringAsync());
        var id = prods.RootElement[0].GetProperty("id").GetInt32();

        // login to get token for user 1
        var login = JsonSerializer.Serialize(new { username = "icuser", password = "pwd" });
        var loginRes = await _client.PostAsync("/api/auth/token", new StringContent(login, Encoding.UTF8, "application/json"));
        loginRes.EnsureSuccessStatusCode();
        var loginBody = await loginRes.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(loginBody);
        var token = doc.RootElement.GetProperty("token").GetString();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // add item to cart for user 1
        var add = JsonSerializer.Serialize(new { productId = id, quantity = 1, unitPrice = 1.0m });
        var addRes = await _client.PostAsync($"/api/cart/user/1/items", new StringContent(add, Encoding.UTF8, "application/json"));
        addRes.EnsureSuccessStatusCode();

        // checkout should succeed
        var co = await _client.PostAsync($"/api/cart/user/1/checkout", null);
        co.EnsureSuccessStatusCode();

        // create another cart and add same product
        var addRes2 = await _client.PostAsync($"/api/cart/user/1/items", new StringContent(add, Encoding.UTF8, "application/json"));
        addRes2.EnsureSuccessStatusCode();

        // second checkout should fail due to insufficient stock
        var co2 = await _client.PostAsync($"/api/cart/user/1/checkout", null);
        Assert.Equal(HttpStatusCode.BadRequest, co2.StatusCode);
    }
}
