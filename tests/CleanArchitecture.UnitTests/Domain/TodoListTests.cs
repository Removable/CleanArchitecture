using System.Linq;
using CleanArchitecture.Domain.TodoListAggregate;
using CleanArchitecture.Domain.TodoListAggregate.Events;

namespace CleanArchitecture.UnitTests.Domain;

public class TodoListTests
{
    private const string Title = "Test";

    [Fact]
    public void ShouldCreateTodoList()
    {
        var todoList = new TodoList() { UserId = string.Empty, Title = Title, };
        todoList.Title.ShouldBe(Title);
    }

    [Fact]
    public void ShouldAddTodoItemAndRaiseCreatedEvent()
    {
        var list = new TodoList() { UserId = string.Empty, Title = Title, };
        var item = new TodoItem() { UserId = string.Empty, Title = "Item 1" };

        list.AddTodoItem(item);

        list.Items.Count.ShouldBe(1);
        list.Items.ShouldContain(item);

        list.DomainEvents.Count.ShouldBe(1);
        list.DomainEvents.OfType<TodoItemCreatedEvent>().Any(e => e.Item == item).ShouldBeTrue();
    }

    [Fact]
    public void ShouldRemoveTodoItemByInstanceAndRaiseDeletedEvent()
    {
        var list = new TodoList() { UserId = string.Empty, Title = Title, };
        var item = new TodoItem() { UserId = string.Empty, Title = "Item 1" };
        list.AddTodoItem(item);
        list.ClearDomainEvents();

        list.RemoveTodoItem(item);

        list.Items.Count.ShouldBe(0);
        list.DomainEvents.Count.ShouldBe(1);
        list.DomainEvents.OfType<TodoItemDeletedEvent>().Any(e => e.Item == item).ShouldBeTrue();
    }

    [Fact]
    public void ShouldRemoveTodoItemByIdAndRaiseDeletedEvent()
    {
        var list = new TodoList() { UserId = string.Empty, Title = Title, };
        var item = new TodoItem() { UserId = string.Empty, Title = "Item 1" };
        var item2 = new TodoItem() { UserId = string.Empty, Title = "Item 2" };
        list.AddTodoItem(item);
        list.AddTodoItem(item2);
        list.ClearDomainEvents();

        list.RemoveTodoItem(item.Id);

        list.Items.Count.ShouldBe(1);
        list.Items.ShouldContain(item2);
        list.Items.ShouldNotContain(item);
        list.DomainEvents.Count.ShouldBe(1);
        list.DomainEvents.OfType<TodoItemDeletedEvent>().Any(e => e.Item == item).ShouldBeTrue();
    }

    [Fact]
    public void ShouldClearTodoItemsAndRaiseDeletedEventsForEach()
    {
        var list = new TodoList() { UserId = string.Empty, Title = Title, };
        var item1 = new TodoItem() { UserId = string.Empty, Title = "Item 1" };
        var item2 = new TodoItem() { UserId = string.Empty, Title = "Item 2" };

        list.AddTodoItem(item1);
        list.AddTodoItem(item2);
        // Remove the created events to focus on delete events
        list.ClearDomainEvents();

        list.ClearTodoItems();

        list.Items.Count.ShouldBe(0);
        list.DomainEvents.Count.ShouldBe(2);
        var deletedEvents = list.DomainEvents.OfType<TodoItemDeletedEvent>().ToList();
        deletedEvents.Count.ShouldBe(2);
        deletedEvents.Any(e => e.Item == item1).ShouldBeTrue();
        deletedEvents.Any(e => e.Item == item2).ShouldBeTrue();
    }
}
