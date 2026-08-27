using Recommendation.Api.Services;
using Microsoft.EntityFrameworkCore;
using Recommendation.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient<RecommendationService>(client =>
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

app.Run();
