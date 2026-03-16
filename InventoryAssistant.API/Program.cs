using Microsoft.EntityFrameworkCore;
using InventoryAssistant.API.Data;

var builder = WebApplication.CreateBuilder(args);

// DbContext — SQLite locally, swap connection string for PostgreSQL in prod
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=inventory-assistant.db";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (connectionString.StartsWith("Data Source"))
        options.UseSqlite(connectionString);
    else
        options.UseNpgsql(connectionString);
});

// Controllers
builder.Services.AddControllers();

// CORS — allow the frontend (any origin in dev; restrict in prod)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Auto-run migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
