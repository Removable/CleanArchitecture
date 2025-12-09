using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Application.Common.Behaviours;

public sealed class PerformanceBehaviour<TRequest, TResponse>(
    ILogger<TRequest> logger,
    IUser user,
    IIdentityService identityService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    private readonly Stopwatch _timer = new();

    public async ValueTask<TResponse> Handle(TRequest message, MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next(message, cancellationToken);

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            var userId = user.Id ?? string.Empty;
            var userName = string.Empty;

            if (!string.IsNullOrEmpty(userId))
            {
                userName = await identityService.GetUserNameAsync(userId).ConfigureAwait(false);
            }

            logger.LogWarning(
                "CleanArchitecture Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@UserName} {@Request}",
                requestName, elapsedMilliseconds, userId, userName, message);
        }

        return response;
    }
}
