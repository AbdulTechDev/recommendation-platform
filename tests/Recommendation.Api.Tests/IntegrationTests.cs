using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Recommendation.Api.Data;
using Recommendation.Api.Services;
using Recommendation.Api.Models;
using Xunit;

namespace Recommendation.Api.Tests;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public IntegrationTests(WebApplicationFactory<Program> factory)
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
                // Remove existing AppDbContext / DbContextOptions registrations
                var dbOptionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (dbOptionsDescriptor != null) services.Remove(dbOptionsDescriptor);

                var appDbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(AppDbContext));
                if (appDbContextDescriptor != null) services.Remove(appDbContextDescriptor);

                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("TestDb"));
                // Build a temp provider to seed an admin user for protected endpoint tests
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
                // Replace Recommendation service with a fake implementation
                var recDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IRecommendationService));
                if (recDescriptor != null) services.Remove(recDescriptor);

                services.AddSingleton<IRecommendationService, Recommendation.Api.Tests.TestDoubles.FakeRecommendationService>();
            });
        });

        _client = webFactory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsOk()
    {
        var res = await _client.GetAsync("/api/products");
        res.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CreateUser_ReturnsCreated()
    {
        var payload = JsonSerializer.Serialize(new { username = "testuser", email = "t@example.com", password = "testpass" });
        var res = await _client.PostAsync("/api/users", new StringContent(payload, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
    }

    [Fact]
    public async Task Register_Login_ProtectedEndpoints_Behavior()
    {
        // Register
        var register = JsonSerializer.Serialize(new { username = "ru", email = "ru@example.com", password = "rpwd" });
        var regRes = await _client.PostAsync("/api/users", new StringContent(register, Encoding.UTF8, "application/json"));
        regRes.EnsureSuccessStatusCode();

        // Login
        var login = JsonSerializer.Serialize(new { username = "ru", password = "rpwd" });
        var loginRes = await _client.PostAsync("/api/auth/token", new StringContent(login, Encoding.UTF8, "application/json"));
        loginRes.EnsureSuccessStatusCode();
        var loginBody = await loginRes.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(loginBody);
        var token = doc.RootElement.GetProperty("token").GetString();
        Assert.False(string.IsNullOrEmpty(token));

        // Call recommendations (UserOrAdmin) - should succeed
        var recReq = JsonSerializer.Serialize(new { query = "test", topN = 3 });
        var recHttp = new HttpRequestMessage(HttpMethod.Post, "/api/recommendations")
        {
            Content = new StringContent(recReq, Encoding.UTF8, "application/json")
        };
        recHttp.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var recRes = await _client.SendAsync(recHttp);
        Assert.Equal(HttpStatusCode.OK, recRes.StatusCode);

        // Call create product (AdminOnly) - should be Forbidden for regular user
        var prod = JsonSerializer.Serialize(new { name = "P", category = "C", description = "D", price = 1.0m });
        var prodReq = new HttpRequestMessage(HttpMethod.Post, "/api/products") { Content = new StringContent(prod, Encoding.UTF8, "application/json") };
        prodReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var prodRes = await _client.SendAsync(prodReq);
        Assert.Equal(HttpStatusCode.Forbidden, prodRes.StatusCode);
    }

    [Fact]
    public async Task CreateInteraction_ReturnsCreated()
    {
        // create user first
        var userPayload = JsonSerializer.Serialize(new { username = "u1", email = "u1@example.com" });
        var userRes = await _client.PostAsync("/api/users", new StringContent(userPayload, Encoding.UTF8, "application/json"));
        userRes.EnsureSuccessStatusCode();

        var interactionPayload = JsonSerializer.Serialize(new { userId = 1, productId = 1, interactionType = "view" });
        var res = await _client.PostAsync("/api/userinteractions", new StringContent(interactionPayload, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
    }
}
