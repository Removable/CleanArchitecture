using Mapster;

namespace CleanArchitecture.Domain.TodoListAggregate.Specifications;

public sealed class GetTodoItemsByListIdSpec : Specification<TodoItem>
{
    public GetTodoItemsByListIdSpec(Guid todoListId, string userId, int pageNumber, int pageSize)
    {
        Guard.Against.NullOrEmpty(userId);
        Guard.Against.NullOrEmpty(todoListId);
        Guard.Against.NegativeOrZero(pageNumber);
        Guard.Against.NegativeOrZero(pageSize);
        Query.Where(x => x.ListId == todoListId && x.UserId == userId)
            .AsNoTracking()
            .OrderBy(x => x.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
