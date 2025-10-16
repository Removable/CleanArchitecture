using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Application.Common.Behaviours;

public sealed class UnhandledExceptionBehaviour<TRequest, TResponse>(ILogger<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IMessage
{
    public async ValueTask<TResponse> Handle(TRequest message, MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(message, cancellationToken);
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;

            logger.LogError(ex, "CleanArchitecture Request: Unhandled Exception for Request {Name} {@Request}",
                requestName,
                message);

            throw;
        }
    }
}
