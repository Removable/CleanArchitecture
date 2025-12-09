using ValidationException = CleanArchitecture.Application.Common.Exceptions.ValidationException;

namespace CleanArchitecture.Application.Common.Behaviours;

public sealed class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>>? validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IMessage
{
    public async ValueTask<TResponse> Handle(TRequest message, MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        var validatorArray = validators?.ToArray();
        if (validatorArray is not { Length: > 0 })
        {
            return await next(message, cancellationToken);
        }

        var validationResults = await Task.WhenAll(
            validatorArray.Select(v =>
                v.ValidateAsync(new ValidationContext<TRequest>(message), cancellationToken)));

        var failures = validationResults
            .Where(r => r.Errors.Count != 0)
            .SelectMany(r => r.Errors)
            .ToArray();

        if (failures.Length != 0)
        {
            throw new ValidationException(failures);
        }

        return await next(message, cancellationToken);
    }
}
