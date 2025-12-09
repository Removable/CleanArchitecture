using System.Data.Common;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using CleanArchitecture.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace CleanArchitecture.IntegrationTests.Infrastructure;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
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
            var optionConfigurators = services
                .Where(d => d.ServiceType == typeof(IConfigureOptions<DbContextOptions<AppDbContext>>)
                            || d.ServiceType == typeof(IPostConfigureOptions<DbContextOptions<AppDbContext>>))
                .ToList();
            foreach (var d in optionConfigurators)
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

            var sqliteOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(conn)
                .UseInternalServiceProvider(efServices)
                .Options;

            services.AddSingleton(sqliteOptions);
            services.AddScoped<AppDbContext>(_ => new AppDbContext(sqliteOptions));
        });
    }

    public Task InitializeAsync()
    {
        // Ensure the in-memory SQLite database has the schema applied before tests run.
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
#if DEBUG
        var providerNames = dbContext.Database.GetDbConnection().GetType().FullName;
        Console.WriteLine($"[CustomWebApplicationFactory] DbConnection provider: {providerNames}");
        var extensions = dbContext.GetService<Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptions>().Extensions;
        foreach (var ext in extensions)
        {
            Console.WriteLine($"[CustomWebApplicationFactory] DbContextOptions extension: {ext.GetType().FullName}");
        }
#endif
        return dbContext.Database.EnsureCreatedAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        await base.DisposeAsync();
    }

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

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
        UrlEncoder encoder) : base(options, logger, encoder)
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
