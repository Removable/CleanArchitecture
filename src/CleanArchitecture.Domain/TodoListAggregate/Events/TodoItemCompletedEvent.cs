namespace CleanArchitecture.Domain.TodoListAggregate.Events;

public sealed record TodoItemCompletedEvent(TodoItem Item) : BaseEvent;
