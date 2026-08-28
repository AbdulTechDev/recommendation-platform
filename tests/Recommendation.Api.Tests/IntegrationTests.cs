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
            builder.ConfigureServices(services =>
            {
                // Remove existing AppDbContext / DbContextOptions registrations
                var dbOptionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (dbOptionsDescriptor != null) services.Remove(dbOptionsDescriptor);

                var appDbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(AppDbContext));
                if (appDbContextDescriptor != null) services.Remove(appDbContextDescriptor);

                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("TestDb"));
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
        var payload = JsonSerializer.Serialize(new { username = "testuser", email = "t@example.com" });
        var res = await _client.PostAsync("/api/users", new StringContent(payload, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
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
