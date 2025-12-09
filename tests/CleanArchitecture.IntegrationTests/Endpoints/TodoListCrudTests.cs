using System.Net;
using System.Net.Http.Json;
using CleanArchitecture.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace CleanArchitecture.IntegrationTests.Endpoints;

public class TodoListCrudTests
{
    [Fact]
    public async Task UpdateTodoList_should_update_title()
    {
        await using var factory = new CustomWebApplicationFactory();
        await factory.InitializeAsync();
        var client = factory.CreateJsonClient();

        // Create a new list
        var createResponse = await client.PostAsJsonAsync("v1/TodoLists", new { title = "Original Title" });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<TodoListDto>();
        created.Should().NotBeNull();

        // Update the list title
        var updateResponse = await client.PatchAsJsonAsync($"v1/TodoLists/{created!.Id}", new { id = created.Id, title = "Updated Title" });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await updateResponse.Content.ReadFromJsonAsync<TodoListDto>();
        updated.Should().NotBeNull();
        updated!.Title.Should().Be("Updated Title");

        // Verify the update persisted
        var getResponse = await client.GetAsync("v1/TodoLists");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var vm = await getResponse.Content.ReadFromJsonAsync<TodosVmDto>();
        vm!.Lists.Should().Contain(x => x.Title == "Updated Title");
        vm.Lists.Should().NotContain(x => x.Title == "Original Title");
    }

    [Fact]
    public async Task DeleteTodoList_should_remove_list()
    {
        await using var factory = new CustomWebApplicationFactory();
        await factory.InitializeAsync();
        var client = factory.CreateJsonClient();

        // Create a new list
        var createResponse = await client.PostAsJsonAsync("v1/TodoLists", new { title = "List To Delete" });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<TodoListDto>();
        created.Should().NotBeNull();

        // Delete the list
        var deleteResponse = await client.DeleteAsync($"v1/TodoLists/{created!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the list was deleted
        var getResponse = await client.GetAsync("v1/TodoLists");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var vm = await getResponse.Content.ReadFromJsonAsync<TodosVmDto>();
        vm!.Lists.Should().NotContain(x => x.Id == created.Id);
    }

    [Fact]
    public async Task CreateTodoList_with_empty_title_should_fail_validation()
    {
        await using var factory = new CustomWebApplicationFactory();
        await factory.InitializeAsync();
        var client = factory.CreateJsonClient();

        // Try to create with empty title
        var createResponse = await client.PostAsJsonAsync("v1/TodoLists", new { title = "" });

        // Should return a validation error (400 Bad Request)
        createResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTodoList_with_duplicate_title_should_fail_validation()
    {
        await using var factory = new CustomWebApplicationFactory();
        await factory.InitializeAsync();
        var client = factory.CreateJsonClient();

        // Create first list
        var createResponse1 = await client.PostAsJsonAsync("v1/TodoLists", new { title = "Unique List" });
        createResponse1.StatusCode.Should().Be(HttpStatusCode.Created);

        // Try to create second list with same title
        var createResponse2 = await client.PostAsJsonAsync("v1/TodoLists", new { title = "Unique List" });

        // Should return a validation error (400 Bad Request)
        createResponse2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTodoList_with_empty_title_should_fail_validation()
    {
        await using var factory = new CustomWebApplicationFactory();
        await factory.InitializeAsync();
        var client = factory.CreateJsonClient();

        // Create a list
        var createResponse = await client.PostAsJsonAsync("v1/TodoLists", new { title = "My List" });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<TodoListDto>();
        created.Should().NotBeNull();

        // Try to update with empty title
        var updateResponse = await client.PatchAsJsonAsync($"v1/TodoLists/{created!.Id}", new { id = created.Id, title = "" });

        // Should return a validation error (400 Bad Request)
        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTodoLists_should_return_multiple_lists()
    {
        await using var factory = new CustomWebApplicationFactory();
        await factory.InitializeAsync();
        var client = factory.CreateJsonClient();

        // Create multiple lists
        await client.PostAsJsonAsync("v1/TodoLists", new { title = "List 1" });
        await client.PostAsJsonAsync("v1/TodoLists", new { title = "List 2" });
        await client.PostAsJsonAsync("v1/TodoLists", new { title = "List 3" });

        // Query lists
        var getResponse = await client.GetAsync("v1/TodoLists");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var vm = await getResponse.Content.ReadFromJsonAsync<TodosVmDto>();
        vm.Should().NotBeNull();
        vm!.Lists.Should().HaveCountGreaterThanOrEqualTo(3);
        vm.Lists.Should().Contain(x => x.Title == "List 1");
        vm.Lists.Should().Contain(x => x.Title == "List 2");
        vm.Lists.Should().Contain(x => x.Title == "List 3");
    }

    private sealed class TodoListDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    private sealed class TodosVmDto
    {
        public List<TodoListDto> Lists { get; set; } = new();
    }
}
