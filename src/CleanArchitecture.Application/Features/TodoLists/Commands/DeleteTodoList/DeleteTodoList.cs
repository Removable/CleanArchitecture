using CleanArchitecture.Domain.TodoListAggregate;
using CleanArchitecture.Domain.TodoListAggregate.Specifications;

namespace CleanArchitecture.Application.Features.TodoLists.Commands.DeleteTodoList;

[Authorize]
public sealed record DeleteTodoListCommand(Guid Id) : IRequest<Unit>;

public sealed class DeleteTodoListCommandHandler(IServiceScopeFactory serviceScopeFactory)
    : IRequestHandler<DeleteTodoListCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteTodoListCommand request, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<TodoList>>();
        var user = scope.ServiceProvider.GetRequiredService<IUser>();
        
        var entity = await repository
            .FirstOrDefaultAsync(new TodoListGetByIdSpec(request.Id, user.Id!), cancellationToken)
            .ConfigureAwait(false);

        Guard.Against.NotFound(request.Id, entity);

        await repository.DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
