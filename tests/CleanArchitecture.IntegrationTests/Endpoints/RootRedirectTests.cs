using System.Net;
using CleanArchitecture.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace CleanArchitecture.IntegrationTests.Endpoints;

public class RootRedirectTests
{
    [Fact]
    public async Task Get_root_should_redirect_to_api()
    {
        await using var factory = new MinimalWebApplicationFactory();
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().Be("/api");
    }
}
