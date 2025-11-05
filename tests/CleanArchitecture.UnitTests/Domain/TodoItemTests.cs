using CleanArchitecture.Domain.TodoListAggregate;

namespace CleanArchitecture.UnitTests.Domain;

public class TodoItemTests
{
    [Fact]
    public void ShouldReturnCorrectBooleanValue()
    {
        var todoItem = new TodoItem() { UserId = string.Empty, };
        todoItem.Done.ShouldBe(false);
        
        todoItem.Done = true;
        todoItem.Done.ShouldBe(true);
    }
}
