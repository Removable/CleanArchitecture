using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using CleanArchitecture.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.IntegrationTests.Infrastructure;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<global::Program>, IAsyncLifetime
{
    private DbConnection? _connection;

    public CustomWebApplicationFactory()
    {
        // Ensure a connection string exists so Infrastructure.AddInfrastructureServices doesn't throw on Guard
        Environment.SetEnvironmentVariable("ConnectionStrings__CleanArchitectureDb", "Host=ignored;Database=ignored;Username=ignored;Password=ignored");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Provide a dummy connection string early via configuration as well
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var dict = new Dictionary<string, string?>
            {
                ["ConnectionStrings:CleanArchitectureDb"] = "Host=ignored;Database=ignored;Username=ignored;Password=ignored"
            };
            config.AddInMemoryCollection(dict);
        });

        builder.ConfigureServices(services =>
        {
            // 1) Always-authenticated test scheme
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            // 2) Remove existing AppDbContext registrations to avoid provider conflicts (Npgsql vs Sqlite)
            var toRemove = services
                .Where(d => d.ServiceType == typeof(IDbContextFactory<AppDbContext>)
                            || d.ServiceType == typeof(AppDbContext)
                            || d.ServiceType == typeof(DbContextOptions<AppDbContext>))
                .ToList();
            foreach (var d in toRemove)
            {
                services.Remove(d);
            }

            // 3) Use a separate EF service provider scoped to Sqlite provider only
            var efServices = new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .BuildServiceProvider();

            // 4) Register SQLite in-memory for tests with a shared open connection
            var conn = new SqliteConnection("Filename=:memory:");
            conn.Open();
            _connection = conn;

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(conn);
                options.UseInternalServiceProvider(efServices);
            });

            services.AddPooledDbContextFactory<AppDbContext>(options =>
            {
                options.UseSqlite(conn);
                options.UseInternalServiceProvider(efServices);
            });
        });
    }

    public Task InitializeAsync()
    {
        // No-op: for the first smoke tests we don't touch the database.
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _connection?.Dispose();
        return Task.CompletedTask;
    }

    public HttpClient CreateJsonClient()
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri(Server.BaseAddress, "api/"), // app uses PathBase("/api") and FastEndpoints prefix "api"
            AllowAutoRedirect = false,
        });
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
        return client;
    }
}

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock)
    { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "integration-test-user"),
            new Claim(ClaimTypes.Name, "integration-test-user"),
            new Claim(ClaimTypes.Role, "User"),
            new Claim(ClaimTypes.Role, "Administrator"),
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
