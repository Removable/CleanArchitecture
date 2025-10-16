namespace CleanArchitecture.Application.Features.TodoItems.Queries.GetTodoItemsWithPagination;

public sealed record TodoItemBriefDto
{
    public Guid Id { get; init; }

    public Guid ListId { get; init; }

    public string Title { get; init; } = "";

    public bool Done { get; init; }
}
