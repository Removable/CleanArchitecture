namespace CleanArchitecture.Domain.TodoListAggregate.Specifications;

public sealed class TodoListGetByIdSpec : Ardalis.Specification.SingleResultSpecification<TodoList>
{
    public TodoListGetByIdSpec(Guid todoListId, string userId)
    {
        Query.Where(x => x.Id == todoListId && x.UserId == userId);
    }
}
