namespace CleanArchitecture.Application.Features.TodoLists.Queries.GetTodos;

public sealed record TodoItemDto
{
    public Guid Id { get; init; }

    public Guid ListId { get; init; }

    public string? Title { get; init; }

    public bool Done { get; init; }

    public int Priority { get; init; }

    public string? Note { get; init; }
}
