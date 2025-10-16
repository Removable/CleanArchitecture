using CleanArchitecture.Domain.TodoListAggregate;

namespace CleanArchitecture.Application.Features.TodoLists.Commands.CreateTodoList;

[Authorize]
public sealed record CreateTodoListCommand : IRequest<Guid>
{
    public required string Title { get; init; }
}

public sealed class CreateTodoListCommandHandler(IServiceScopeFactory serviceScopeFactory)
    : IRequestHandler<CreateTodoListCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateTodoListCommand request, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<TodoList>>();
        var user = scope.ServiceProvider.GetRequiredService<IUser>();
        
        var entity = new TodoList { Title = request.Title, UserId = user.Id! };

        await repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return entity.Id;
    }
}
