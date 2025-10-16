using ValidationException = CleanArchitecture.Application.Common.Exceptions.ValidationException;

namespace CleanArchitecture.Application.Common.Behaviours;

public sealed class ValidationBehaviour<TRequest, TResponse>(IServiceScopeFactory serviceScopeFactory)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IMessage
{
    public async ValueTask<TResponse> Handle(TRequest message, MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var validators = scope.ServiceProvider.GetServices<IValidator<TRequest>>().ToArray();
        
        if (validators.Length != 0)
        {
            var validationResults = await Task.WhenAll(
                validators.Select(v =>
                    v.ValidateAsync(new ValidationContext<TRequest>(message), cancellationToken)));

            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToArray();

            if (failures.Length != 0)
            {
                throw new ValidationException(failures);
            }
        }

        return await next(message, cancellationToken);
    }
}
