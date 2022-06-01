using NameItAfterMe.Application;
using NameItAfterMe.Infrastructure;
using NameItAfterMe.Persistance;
using NameItAfterMe.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddAzureAppConfiguration(
    builder.Configuration.GetConnectionString("AppConfig"));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistance(builder.Configuration);

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

// add use cases from application project
app.UseEndpoints(builder =>
    builder.MapUseCasesFromAssembly(typeof(WebHostedUseCaseAttribute).Assembly));

app.MapRazorPages();
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }