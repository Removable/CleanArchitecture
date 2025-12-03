using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CleanArchitecture.IntegrationTests.Infrastructure;

public sealed class MinimalWebApplicationFactory : WebApplicationFactory<Program>
{
    public MinimalWebApplicationFactory()
    {
        // Ensure Infrastructure can read a connection string at startup
        Environment.SetEnvironmentVariable("ConnectionStrings__CleanArchitectureDb",
            "Host=ignored;Database=ignored;Username=ignored;Password=ignored");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var dict = new Dictionary<string, string?>
            {
                ["ConnectionStrings:CleanArchitectureDb"] =
                    "Host=ignored;Database=ignored;Username=ignored;Password=ignored"
            };
            config.AddInMemoryCollection(dict);
        });
    }
}
