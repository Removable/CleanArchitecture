using CleanArchitecture.Domain.Common.Interfaces;
using CleanArchitecture.Domain.TodoListAggregate.Enums;
using CleanArchitecture.Domain.TodoListAggregate.Events;

namespace CleanArchitecture.Domain.TodoListAggregate;

public sealed class TodoItem : BaseAuditableEntity, IOwnerId
{
    private bool _done;

    public Guid ListId { get; set; }

    public string? Title { get; set; }

    public string? Note { get; set; }

    public required string UserId { get; set; }

    public PriorityLevel Priority { get; set; }

    public DateTime? Reminder { get; set; }

    public bool Done
    {
        get => _done;
        set
        {
            if (value && !_done)
            {
                AddDomainEvent(new TodoItemCompletedEvent(this));
            }

            _done = value;
        }
    }

    public TodoList List { get; set; } = null!;
}
