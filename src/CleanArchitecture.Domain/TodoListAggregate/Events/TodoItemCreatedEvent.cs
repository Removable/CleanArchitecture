namespace CleanArchitecture.Domain.TodoListAggregate.Events;

public sealed record TodoItemCreatedEvent(TodoItem Item) : BaseEvent;
