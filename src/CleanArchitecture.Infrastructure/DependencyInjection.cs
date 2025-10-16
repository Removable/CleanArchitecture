using Ardalis.GuardClauses;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Common.Interfaces;
using CleanArchitecture.Domain.Constants;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Data.Interceptors;
using CleanArchitecture.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CleanArchitecture.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("CleanArchitectureDb");
        Guard.Against.Null(connectionString, message: "Connection string 'CleanArchitectureDb' not found.");

        builder.Services.AddSingleton<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddSingleton<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        builder.Services.AddPooledDbContextFactory<AppDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(connectionString);
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });
        builder.Services.AddScoped<AppDbContext>(provider => 
            provider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());
        builder.EnrichNpgsqlDbContext<AppDbContext>();

        builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
            .AddScoped(typeof(IReadRepository<>), typeof(EfReadRepository<>));
        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        builder.Services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddApiEndpoints();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, IdentityService>();

        builder.Services.AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator));
    }
}
