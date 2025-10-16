using CleanArchitecture.Domain.TodoListAggregate;
using CleanArchitecture.Domain.TodoListAggregate.Specifications;

namespace CleanArchitecture.Application.Features.TodoItems.Commands.
    CreateTodoItem;

[Authorize]
public sealed record CreateTodoItemCommand : IRequest<Result<Guid>>
{
    public Guid ListId { get; init; }

    public string? Title { get; init; }
}

public sealed class CreateTodoItemCommandHandler(
    IServiceScopeFactory serviceScopeFactory)
    : IRequestHandler<CreateTodoItemCommand, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(CreateTodoItemCommand request,
        CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<TodoList>>();
        var user = scope.ServiceProvider.GetRequiredService<IUser>();

        var newTodoItem = new TodoItem
        {
            ListId = request.ListId,
            Title = request.Title,
            Done = false,
            UserId = Guard.Against.NullOrEmpty(user.Id)
        };

        var todoList = await repository.SingleOrDefaultAsync(
                new TodoListGetByIdSpec(request.ListId, Guard.Against.NullOrEmpty(user.Id)), cancellationToken)
            .ConfigureAwait(false);
        if (todoList == null)
            return Result<Guid>.NotFound();

        todoList.AddTodoItem(newTodoItem);

        await repository.UpdateAsync(todoList, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(newTodoItem.Id);
    }
}
