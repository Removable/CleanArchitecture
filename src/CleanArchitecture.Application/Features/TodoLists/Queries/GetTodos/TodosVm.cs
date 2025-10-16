using CleanArchitecture.Application.Common.Models;

namespace CleanArchitecture.Application.Features.TodoLists.Queries.GetTodos;

public sealed class TodosVm
{
    public IReadOnlyCollection<LookupDto> PriorityLevels { get; init; } = [];

    public IReadOnlyCollection<TodoListDto> Lists { get; init; } = [];
}
