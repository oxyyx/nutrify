using Microsoft.EntityFrameworkCore;
using Keycloak.AuthServices.Authentication;
using Nutrify.Api.Data;
using Nutrify.Api.Endpoints;
using Nutrify.Api.Middleware;
using Nutrify.Api.Services;
using Nutrify.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Aspire service defaults (telemetry, health checks, service discovery)
builder.AddServiceDefaults();

// Database
builder.AddNpgsqlDbContext<NutrifyDbContext>("nutrify");

// Caching (optional - Redis)
builder.AddRedisDistributedCache("redis");

// Authentication - Keycloak JWT Bearer
builder.Services.AddKeycloakWebApiAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// Exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// External food database used by barcode lookups. Open Food Facts asks for a
// descriptive User-Agent identifying the app (https://openfoodfacts.github.io/openfoodfacts-server/api/).
builder.Services.AddHttpClient<IOpenFoodFactsClient, OpenFoodFactsClient>(client =>
{
    client.BaseAddress = new Uri("https://world.openfoodfacts.org/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Nutrify/1.0 (https://nutrify.oxy.sh)");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Application services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IFoodItemService, FoodItemService>();
builder.Services.AddScoped<IIntakeService, IntakeService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// No CORS policy: the SPA is served from this same origin in production, and
// the Vite dev server proxies /api, so browser requests are always same-origin.

var app = builder.Build();

// Apply any pending EF Core migrations on startup so a freshly deployed
// container reaches a usable schema without a separate migration step.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NutrifyDbContext>();
    await db.Database.MigrateAsync();
}

// Middleware pipeline
app.UseExceptionHandler();

// Serve the bundled SPA static assets (wwwroot, populated at container build time)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapCategoryEndpoints();
app.MapFoodItemEndpoints();
app.MapIntakeEndpoints();
app.MapDashboardEndpoints();
app.MapConfigEndpoints();
app.MapDefaultEndpoints();

// SPA fallback: any non-/api, non-file request returns index.html so the
// client-side router can handle it.
app.MapFallbackToFile("index.html");

await app.RunAsync();
