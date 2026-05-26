using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VersionControl.Api.Hubs;
using VersionControl.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ── DbContext ──────────────────────────────────────────────────────────
var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "app.db");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite($"Data Source={dbPath}");
    options.EnableDetailedErrors();
    options.ConfigureWarnings(w =>
        w.Ignore(RelationalEventId.PendingModelChangesWarning));
});

// ── Controllers ────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// ── Swagger ────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "VersionControl API",
        Version = "v1",
        Description = "Software Version Control & Monitoring System"
    });
});

// ── SignalR ────────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── CORS ───────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// ── Auto-migrate ───────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    app.Logger.LogInformation("Database ready. Path: {path}", dbPath);
}

// ── Pipeline ───────────────────────────────────────────────────────────
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "VersionControl API v1");
    c.RoutePrefix = "swagger";
});

app.UseRouting();
app.MapControllers();
app.MapHub<MonitoringHub>("/hubs/monitoring");

app.MapGet("/", () => Results.Ok(new
{
    Status = "Running",
    Time = DateTime.UtcNow,
    Docs = "/swagger"
}));

app.Run();