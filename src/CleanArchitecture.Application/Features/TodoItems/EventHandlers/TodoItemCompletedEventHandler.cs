using CleanArchitecture.Domain.TodoListAggregate.Events;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Application.Features.TodoItems.EventHandlers;

public sealed class TodoItemCompletedEventHandler(ILogger<TodoItemCompletedEventHandler> logger)
    : INotificationHandler<TodoItemCompletedEvent>
{
    public ValueTask Handle(TodoItemCompletedEvent message, CancellationToken cancellationToken)
    {
        logger.LogInformation("TaosGrowthBackend Domain Event: {DomainEvent}", message.GetType().Name);

        return ValueTask.CompletedTask;
    }
}
