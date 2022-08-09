using Microsoft.ApplicationInsights.Extensibility;
using MinimalEndpoints;
using MinimalEndpoints.OpenApi;
using NameItAfterMe.Application;
using NameItAfterMe.Application.Domain;
using NameItAfterMe.Server;
using NameItAfterMe.Server.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddAzureAppConfiguration(
    builder.Configuration.GetConnectionString("AppConfig"));

builder.Host.UseSerilog((context, services, config) =>
{
    config.MinimumLevel.Debug()
          .MinimumLevel.Override("Microsoft", LogEventLevel.Information);

    config.WriteTo.ApplicationInsights(
        services.GetRequiredService<TelemetryConfiguration>(),
        TelemetryConverter.Traces);

    config.WriteTo.Async(x => x.Console(theme: AnsiConsoleTheme.Code));
});

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddControllersWithViews();
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMinimalEndpointServices();
builder.Services.AddHostedService<ExoplanetSyncronizationService>();

builder.Services.Configure<BackgroundServiceOptions>(builder.Configuration);

builder.Services.AddSwaggerGen(x => x.AddMinimalEndpointSupport());

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();

app.UseEndpoints(builder =>
{
    builder.MapSwagger();
    builder.MapUseCasesFromAssembly(typeof(Exoplanet).Assembly);
});

app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }