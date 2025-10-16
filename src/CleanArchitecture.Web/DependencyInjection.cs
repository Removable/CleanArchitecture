using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Web.Extensions;
using CleanArchitecture.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Web;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddScoped<IUser, CurrentUser>();

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddExceptionHandler<CustomExceptionHandler>();

        // This is used to suppress the default 400 Bad Request response.
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddFastEndpointsService();
    }
}
