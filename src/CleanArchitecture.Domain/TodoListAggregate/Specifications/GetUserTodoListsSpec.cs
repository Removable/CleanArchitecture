namespace CleanArchitecture.Domain.TodoListAggregate.Specifications;

public sealed class GetUserTodoListsSpec : Specification<TodoList>
{
    public GetUserTodoListsSpec(string userId)
    {
        Query.Where(x => x.UserId == userId)
            .OrderBy(x => x.Title);
    }
}
