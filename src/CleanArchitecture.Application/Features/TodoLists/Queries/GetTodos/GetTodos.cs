using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Domain.TodoListAggregate;
using CleanArchitecture.Domain.TodoListAggregate.Enums;
using CleanArchitecture.Domain.TodoListAggregate.Specifications;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.TodoLists.Queries.GetTodos;

[Authorize]
public sealed record GetTodosQuery : IRequest<TodosVm>;

public sealed class GetTodosQueryHandler(IServiceScopeFactory serviceScopeFactory)
    : IRequestHandler<GetTodosQuery, TodosVm>
{
    public async ValueTask<TodosVm> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IReadRepository<TodoList>>();
        var user = scope.ServiceProvider.GetRequiredService<IUser>();
        
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
