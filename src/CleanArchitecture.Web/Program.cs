using CleanArchitecture.Application;
using CleanArchitecture.Infrastructure;
using CleanArchitecture.Web;
using CleanArchitecture.Web.Extensions;
using CleanArchitecture.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsEnvironment("Testing"))
{
    await app.InitialiseDatabaseAsync().ConfigureAwait(false);
}

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseExceptionHandler();

app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    // settings.DocumentPath = "/api/specification.json";
});

app.UseAuthentication();
app.UseAuthorization();

app.Map("/", () => Results.Redirect("/api"));

app.MapDefaultEndpoints();
app.MapEndpoints();

app.Run();

public partial class Program {}
