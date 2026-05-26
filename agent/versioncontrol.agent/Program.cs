using VersionControl.Agent;
using VersionControl.Agent.Services;

var builder = Host.CreateApplicationBuilder(args);

// ── Logging ────────────────────────────────────────────────────────────
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ── Windows Service support ────────────────────────────────────────────
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "VersionControl.Agent";
});

// ── Agent services ─────────────────────────────────────────────────────
builder.Services.AddSingleton<ProcessScanner>();
builder.Services.AddSingleton<VersionDetector>();
builder.Services.AddSingleton<PolicyEvaluator>();

// ── HTTP client to API ─────────────────────────────────────────────────
var apiBaseUrl = builder.Configuration["Agent:ApiBaseUrl"]
    ?? "http://localhost:5186/";

builder.Services.AddHttpClient<ApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout     = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    UseProxy = false
});

// ── Worker ─────────────────────────────────────────────────────────────
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
