using CleanArchitecture.Domain.TodoListAggregate;
using CleanArchitecture.Domain.TodoListAggregate.Specifications;

namespace CleanArchitecture.Application.Features.TodoItems.Commands.DeleteTodoItem;

[Authorize]
public sealed record DeleteTodoItemCommand(Guid TodoListId, Guid TodoItemId) : IRequest<Result>;

public sealed class DeleteTodoItemCommandHandler(IUser user, IRepository<TodoList> repository)
    : IRequestHandler<DeleteTodoItemCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteTodoItemCommand request, CancellationToken cancellationToken)
    {
        var todoList = await repository
            .SingleOrDefaultAsync(new TodoListGetByIdSpec(request.TodoListId, Guard.Against.NullOrEmpty(user.Id)),
                cancellationToken).ConfigureAwait(false);
        if (todoList == null)
        {
            return Result.NotFound();
        }

        todoList.RemoveTodoItem(request.TodoItemId);

        await repository.UpdateAsync(todoList, cancellationToken).ConfigureAwait(false);
        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
