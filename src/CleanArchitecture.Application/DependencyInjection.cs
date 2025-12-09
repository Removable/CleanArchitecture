using System.Reflection;
using CleanArchitecture.Application.Common.Behaviours;
using CleanArchitecture.Application.Common.Models;
using Microsoft.Extensions.Hosting;

namespace CleanArchitecture.Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(),
            lifetime: ServiceLifetime.Scoped);

        builder.Services.AddMediator(options =>
            {
                options.ServiceLifetime = ServiceLifetime.Scoped;
                options.Assemblies = [typeof(LookupDto).Assembly];
                options.PipelineBehaviors =
                [
                    typeof(LoggingBehaviour<,>),
                    typeof(UnhandledExceptionBehaviour<,>),
                    typeof(AuthorizationBehaviour<,>),
                    typeof(ValidationBehaviour<,>),
                    typeof(PerformanceBehaviour<,>)
                ];
            }
        );
    }
}
