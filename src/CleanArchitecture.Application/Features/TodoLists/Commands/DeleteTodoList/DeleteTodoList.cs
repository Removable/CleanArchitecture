using CleanArchitecture.Domain.TodoListAggregate;
using CleanArchitecture.Domain.TodoListAggregate.Specifications;

namespace CleanArchitecture.Application.Features.TodoLists.Commands.DeleteTodoList;

[Authorize]
public sealed record DeleteTodoListCommand(Guid Id) : IRequest<Unit>;

public sealed class DeleteTodoListCommandHandler(IUser user, IRepository<TodoList> repository)
    : IRequestHandler<DeleteTodoListCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteTodoListCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository
            .FirstOrDefaultAsync(new TodoListGetByIdSpec(request.Id, user.Id!), cancellationToken)
            .ConfigureAwait(false);

        Guard.Against.NotFound(request.Id, entity);

        await repository.DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
