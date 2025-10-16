using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Application.Common.Behaviours;

public sealed class LoggingBehaviour<TMessage, TResponse>(
    ILogger<TMessage> logger,
    IServiceScopeFactory serviceScopeFactory)
    : MessagePreProcessor<TMessage, TResponse>
    where TMessage : IMessage
{
    private readonly ILogger _logger = logger;

    protected override async ValueTask Handle(TMessage message, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var user = scope.ServiceProvider.GetRequiredService<IUser>();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();

        var requestName = typeof(TMessage).Name;
        var userId = user.Id ?? string.Empty;
        var userName = string.Empty;

        if (!string.IsNullOrEmpty(userId))
        {
            userName = await identityService.GetUserNameAsync(userId).ConfigureAwait(false);
        }

        _logger.LogInformation("CleanArchitecture Request: {Name} {@UserId} {@UserName} {@Request}",
            requestName, userId, userName, message);
    }
}
