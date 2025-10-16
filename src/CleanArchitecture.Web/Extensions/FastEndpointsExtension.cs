using FastEndpoints.Swagger;

namespace CleanArchitecture.Web.Extensions;

public static class FastEndpointsExtension
{
    public static void AddFastEndpointsService(this IServiceCollection services)
    {
        services.AddFastEndpoints()
            .SwaggerDocument(o =>
            {
                o.DocumentSettings = s =>
                {
                    s.DocumentName = "v1";
                    s.Title = "CleanArchitecture API";
                    s.Version = "v1";
                };
                o.ReleaseVersion = 1;
                o.TagCase = TagCase.None;
            });
    }
}
