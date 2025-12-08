using CleanArchitecture.Domain.Common.Interfaces;
using CleanArchitecture.Domain.TodoListAggregate.Events;

namespace CleanArchitecture.Domain.TodoListAggregate;

public sealed class TodoList : BaseAuditableEntity, IAggregateRoot, IOwnerId
{
    public required string Title { get; set; }

    public Colour Colour { get; set; } = Colour.White;

    public required string UserId { get; set; }

    private readonly List<TodoItem> _todoItems = [];

    public IReadOnlyCollection<TodoItem> Items
    {
        get => _todoItems;
    }

    public void AddTodoItem(TodoItem item)
    {
        AddDomainEvent(new TodoItemCreatedEvent(item));
        _todoItems.Add(item);
    }

    public void RemoveTodoItem(TodoItem item)
    {
        AddDomainEvent(new TodoItemDeletedEvent(item));
        _todoItems.Remove(item);
    }

    public void RemoveTodoItem(Guid id)
    {
        var item = Guard.Against.Null(_todoItems.FirstOrDefault(x => x.Id == id), nameof(Items));

        RemoveTodoItem(item);
    }

    public void ClearTodoItems()
    {
        _todoItems.ForEach(item => AddDomainEvent(new TodoItemDeletedEvent(item)));
        _todoItems.Clear();
    }
}
