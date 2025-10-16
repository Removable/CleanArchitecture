namespace CleanArchitecture.Domain.TodoListAggregate.Specifications;

public sealed class TodoListTitleUniquenessCheckSpec : Specification<TodoList>
{
    public TodoListTitleUniquenessCheckSpec(Guid? id, string userId, string title)
    {
        Query.Where(x => x.UserId == userId && x.Title == title);
        if (id.HasValue)
        {
            Query.Where(x => x.Id != id);
        }
    }
}
