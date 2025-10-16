namespace CleanArchitecture.Domain.TodoListAggregate.Events;

public sealed record TodoItemDeletedEvent(TodoItem Item) : BaseEvent;
