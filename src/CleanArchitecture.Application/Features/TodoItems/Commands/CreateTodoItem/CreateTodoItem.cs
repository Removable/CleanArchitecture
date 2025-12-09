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
    IUser user,
    IRepository<TodoList> repository)
    : IRequestHandler<CreateTodoItemCommand, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(CreateTodoItemCommand request,
        CancellationToken cancellationToken)
    {
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

        await repository.UpdateAsync(todoList, cancellationToken).ConfigureAwait(false);
        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<Guid>.Success(newTodoItem.Id);
    }
}
