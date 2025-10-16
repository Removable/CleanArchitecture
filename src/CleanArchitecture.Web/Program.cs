using CleanArchitecture.Application;
using CleanArchitecture.Infrastructure;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Web;
using CleanArchitecture.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    // settings.DocumentPath = "/api/specification.json";
});
app.UsePathBase("/api");


app.UseExceptionHandler(options => { });

app.UseAuthentication();
app.UseAuthorization();

app.Map("/", () => Results.Redirect("/api"));

app.MapDefaultEndpoints();
app.MapEndpoints();

app.Run();
