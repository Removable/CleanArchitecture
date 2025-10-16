using System.Text.Json;
using CleanArchitecture.Infrastructure.Identity;
using FastEndpoints.Swagger;

namespace CleanArchitecture.Web.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        app.MapGroup("/api").WithTags("Api").MapIdentityApi<ApplicationUser>();

        app.UseFastEndpoints(c =>
            {
                c.Errors.UseProblemDetails();
                c.Endpoints.RoutePrefix = "api";
                c.Versioning.Prefix = "v";
                c.Versioning.PrependToRoute = true;
                c.Binding.UsePropertyNamingPolicy = true;
                c.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            })
            .UseSwaggerGen();

        return app;
    }
}
