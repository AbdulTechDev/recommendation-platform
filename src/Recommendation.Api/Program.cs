using Recommendation.Api.Services;
using Microsoft.EntityFrameworkCore;
using Recommendation.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}
builder.Services.AddHttpClient<IRecommendationService, RecommendationService>(client =>
{
    var recommendationApiUrl = builder.Configuration["RecommendationApi:BaseUrl"]
        ?? "http://localhost:8000/";
    client.BaseAddress = new Uri(recommendationApiUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Urls.Any(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps))
{
    app.UseHttpsRedirection();
}

app.MapControllers();

// Ensure database migrations are applied on startup (skip in Test env)
if (!app.Environment.IsEnvironment("Test"))
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
            // seed data if necessary
            Recommendation.Api.Data.SeedData.EnsureSeedData(db);
        }
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Could not apply EF migrations on startup.");
    }
}

app.Run();

// Expose Program for integration tests
public partial class Program { }
