namespace CleanArchitecture.Application.Features.TodoLists.Queries.GetTodos;

public sealed class TodoListDto
{
    public Guid Id { get; init; }

    public string? Title { get; init; }

    public string? Colour { get; init; }

    public IReadOnlyCollection<TodoItemDto> Items { get; init; } = [];
}
