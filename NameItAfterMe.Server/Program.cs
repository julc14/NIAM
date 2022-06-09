using NameItAfterMe.Application;
using NameItAfterMe.Infrastructure;
using NameItAfterMe.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddAzureAppConfiguration(
    builder.Configuration.GetConnectionString("AppConfig"));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
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
    builder.MapUseCasesFromAssembly(typeof(GenerateEndpointAttribute).Assembly,
    options =>
    {
        options.ParseRequestPropertiesFromBody();
        options.ParseRequestPropertiesFromRouteData();
        options.ParseRequestPropertiesFromQueryParameters();
    });
});

app.MapRazorPages();
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }