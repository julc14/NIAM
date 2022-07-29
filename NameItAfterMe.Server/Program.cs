using Microsoft.ApplicationInsights.Extensibility;
using MinimalEndpoints;
using MinimalEndpoints.OpenApi;
using NameItAfterMe.Application;
using NameItAfterMe.Application.Abstractions;
using NameItAfterMe.Infrastructure;
using NameItAfterMe.Server.Services;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddAzureAppConfiguration(
    builder.Configuration.GetConnectionString("AppConfig"));

builder.Host.UseSerilog((context, services, config) =>
{
    config.WriteTo.ApplicationInsights(
        services.GetRequiredService<TelemetryConfiguration>(),
        TelemetryConverter.Traces);

    config.WriteTo.Async(x => x.Console(theme: AnsiConsoleTheme.Code));
});

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMinimalEndpointServices();
builder.Services.AddHostedService<ExoplanetSyncronizationService>();

builder.Services.AddSwaggerGen(x => x.AddMinimalEndpointSupport());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();

app.UseEndpoints(builder =>
{
    builder.MapSwagger();
    builder.MapUseCasesFromAssembly(typeof(IExoplanetApi).Assembly);
});

app.MapRazorPages();
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }