using System.Net;
using System.Net.Http.Json;
using CleanArchitecture.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace CleanArchitecture.IntegrationTests.Endpoints;

public class TodoListApiTests
{
    [Fact(Skip = "Pending database test setup (SQLite/Testcontainers)")]
    public async Task Create_then_Get_TodoLists_should_succeed()
    {
        await using var factory = new CleanArchitecture.IntegrationTests.Infrastructure.CustomWebApplicationFactory();
        var client = factory.CreateJsonClient();

        // Create a new list
        var createResponse = await client.PostAsJsonAsync("api/v1/TodoLists", new { title = "My List" });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<CreateTodoListResponseDto>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.Title.Should().Be("My List");

        // Query lists
        var getResponse = await client.GetAsync("api/v1/TodoLists");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var vm = await getResponse.Content.ReadFromJsonAsync<TodosVmDto>();
        vm.Should().NotBeNull();
        vm!.Lists.Should().NotBeNull();
        vm.Lists.Should().Contain(x => x.Title == "My List");
    }

    private sealed class CreateTodoListResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    private sealed class TodosVmDto
    {
        public List<TodoListLookUpDto> Lists { get; set; } = new();
    }

    private sealed class TodoListLookUpDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
