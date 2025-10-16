using CleanArchitecture.Domain.TodoListAggregate;
using CleanArchitecture.Domain.TodoListAggregate.Specifications;

namespace CleanArchitecture.Application.Features.TodoLists.Commands.UpdateTodoList;

[Authorize]
public sealed record UpdateTodoListCommand : IRequest
{
    public Guid Id { get; init; }

    public required string Title { get; init; }
}

public sealed class UpdateTodoListCommandHandler(IServiceScopeFactory serviceScopeFactory)
    : IRequestHandler<UpdateTodoListCommand>
{
    public async ValueTask<Unit> Handle(UpdateTodoListCommand request, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<TodoList>>();
        var user = scope.ServiceProvider.GetRequiredService<IUser>();
        
        var spec = new TodoListGetByIdSpec(request.Id, user.Id!);
        var entity = await repository
            .FirstOrDefaultAsync(spec, cancellationToken).ConfigureAwait(false);

        Guard.Against.NotFound(request.Id, entity);

        entity.Title = request.Title;

        await repository.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
