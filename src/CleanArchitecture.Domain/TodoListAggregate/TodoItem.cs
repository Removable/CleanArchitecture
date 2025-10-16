using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CleanArchitecture.Domain.Common.Interfaces;
using CleanArchitecture.Domain.Constants;
using CleanArchitecture.Domain.TodoListAggregate.Enums;
using CleanArchitecture.Domain.TodoListAggregate.Events;

namespace CleanArchitecture.Domain.TodoListAggregate;

[Table("TodoItems")]
public sealed class TodoItem : BaseAuditableEntity, IOwnerId
{
    private bool _done;
    [Column] public Guid ListId { get; set; }

    [Column] public string? Title { get; set; }

    [Column] public string? Note { get; set; }

    [Column]
    [MaxLength(LengthConstants.UserIdMaxLength)]
    public required string UserId { get; set; }

    [Column] public PriorityLevel Priority { get; set; }

    [Column] public DateTime? Reminder { get; set; }

    [Column]
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
