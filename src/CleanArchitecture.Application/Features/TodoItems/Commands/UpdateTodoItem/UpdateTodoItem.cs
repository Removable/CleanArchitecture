using CleanArchitecture.Domain.TodoListAggregate;
using CleanArchitecture.Domain.TodoListAggregate.Specifications;

namespace CleanArchitecture.Application.Features.TodoItems.Commands.UpdateTodoItem;

[Authorize]
public sealed record UpdateTodoItemCommand : IRequest<Result>
{
    public Guid TodoListId { get; init; }

    public Guid TodoItemId { get; init; }

    public string? Title { get; init; }

    public bool Done { get; init; }
}

public sealed class UpdateTodoItemCommandHandler(IRepository<TodoList> repository, IUser user)
    : IRequestHandler<UpdateTodoItemCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateTodoItemCommand request, CancellationToken cancellationToken)
    {
        var todoList = await repository.SingleOrDefaultAsync(
                new TodoListGetByIdSpec(request.TodoListId, Guard.Against.NullOrEmpty(user.Id)), cancellationToken)
            .ConfigureAwait(false);
        if (todoList == null)
        {
            return Result.NotFound();
        }

        var todoItem = todoList.Items.FirstOrDefault(x => x.Id == request.TodoItemId);
        if (todoItem == null)
        {
            return Result.NotFound();
        }

        todoItem.Title = request.Title;
        todoItem.Done = request.Done;

        await repository.UpdateAsync(todoList, cancellationToken).ConfigureAwait(false);
        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
