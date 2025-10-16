using CleanArchitecture.Domain.TodoListAggregate.Events;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Application.Features.TodoItems.EventHandlers;

public class TodoItemCreatedEventHandler(ILogger<TodoItemCreatedEventHandler> logger)
    : INotificationHandler<TodoItemCreatedEvent>
{
    public ValueTask Handle(TodoItemCreatedEvent message, CancellationToken cancellationToken)
    {
        logger.LogInformation("TaosGrowthBackend Domain Event: {DomainEvent}", message.GetType().Name);

        return ValueTask.CompletedTask;
    }
}
