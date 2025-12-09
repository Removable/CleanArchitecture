using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Domain.TodoListAggregate;
using CleanArchitecture.Domain.TodoListAggregate.Enums;
using CleanArchitecture.Domain.TodoListAggregate.Specifications;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.TodoLists.Queries.GetTodos;

[Authorize]
public sealed record GetTodosQuery : IRequest<TodosVm>;

public sealed class GetTodosQueryHandler(IReadRepository<TodoList> repository, IUser user)
    : IRequestHandler<GetTodosQuery, TodosVm>
{
    public async ValueTask<TodosVm> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetUserTodoListsSpec(Guard.Against.NullOrEmpty(user.Id));

        return new TodosVm
        {
            PriorityLevels = Enum.GetValues<PriorityLevel>()
                .Select(p => new LookupDto { Id = (int)p, Title = p.ToString() })
                .ToArray(),
            Lists = await repository.ArrayAsync<TodoListDto>(spec, cancellationToken).ConfigureAwait(false)
        };
    }
}
