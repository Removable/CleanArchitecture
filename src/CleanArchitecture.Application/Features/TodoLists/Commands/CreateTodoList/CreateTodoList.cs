using CleanArchitecture.Domain.TodoListAggregate;

namespace CleanArchitecture.Application.Features.TodoLists.Commands.CreateTodoList;

[Authorize]
public sealed record CreateTodoListCommand : IRequest<Guid>
{
    public required string Title { get; init; }
}

public sealed class CreateTodoListCommandHandler(IRepository<TodoList> repository, IUser user)
    : IRequestHandler<CreateTodoListCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateTodoListCommand request, CancellationToken cancellationToken)
    {
        var entity = new TodoList { Title = request.Title, UserId = user.Id! };

        await repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return entity.Id;
    }
}
