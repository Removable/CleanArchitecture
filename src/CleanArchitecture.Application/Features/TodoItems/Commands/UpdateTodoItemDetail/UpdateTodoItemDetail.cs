using CleanArchitecture.Domain.TodoListAggregate;
using CleanArchitecture.Domain.TodoListAggregate.Enums;
using CleanArchitecture.Domain.TodoListAggregate.Specifications;

namespace CleanArchitecture.Application.Features.TodoItems.Commands.UpdateTodoItemDetail;

[Authorize]
public sealed record UpdateTodoItemDetailCommand : IRequest<Result>
{
    public Guid TodoItemId { get; init; }

    public Guid TodoListId { get; init; }

    public PriorityLevel Priority { get; init; }

    public string? Note { get; init; }
}

public sealed class UpdateTodoItemDetailCommandHandler(IServiceScopeFactory serviceScopeFactory)
    : IRequestHandler<UpdateTodoItemDetailCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateTodoItemDetailCommand request, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<TodoList>>();
        var user = scope.ServiceProvider.GetRequiredService<IUser>();

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

        todoItem.ListId = request.TodoListId;
        todoItem.Priority = request.Priority;
        todoItem.Note = request.Note;

        await repository.UpdateAsync(todoList, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
