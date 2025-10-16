using CleanArchitecture.Domain.Constants;
using CleanArchitecture.Domain.TodoListAggregate;
using CleanArchitecture.Domain.TodoListAggregate.Specifications;

namespace CleanArchitecture.Application.Features.TodoLists.Commands.PurgeTodoLists;

[Authorize(Roles = [Roles.Administrator])]
[Authorize(Policy = Policies.CanPurge)]
public sealed record PurgeTodoListsCommand : IRequest<Unit>;

public sealed class PurgeTodoListsCommandHandler(IServiceScopeFactory serviceScopeFactory)
    : IRequestHandler<PurgeTodoListsCommand, Unit>
{
    public async ValueTask<Unit> Handle(PurgeTodoListsCommand request, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<TodoList>>();
        var user = scope.ServiceProvider.GetRequiredService<IUser>();
        
        await repository.DeleteRangeAsync(new GetUserTodoListsSpec(Guard.Against.NullOrEmpty(user.Id)), cancellationToken).ConfigureAwait(false);
        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
