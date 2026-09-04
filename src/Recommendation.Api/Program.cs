using Recommendation.Api.Services;
using Microsoft.EntityFrameworkCore;
using Recommendation.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
// CORS for local frontend development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
// Authentication & Authorization (JWT-based, simple dev defaults)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"] ?? "dev_secret_key_please_change";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
});
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

// Simple preflight responder for local development to ensure OPTIONS requests
// return CORS headers before reaching MVC endpoints. This makes local frontend
// development simpler while we use a specific CORS policy.
app.Use(async (context, next) =>
{
    if (string.Equals(context.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type,Authorization");
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
        return;
    }
    await next();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ensure routing is enabled, then enable CORS so preflight requests are handled
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

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
