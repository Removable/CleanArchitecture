using System.Reflection;
using CleanArchitecture.Application.Common.Exceptions;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Application.Common.Behaviours;

public sealed class AuthorizationBehaviour<TRequest, TResponse>(
    IServiceScopeFactory serviceScopeFactory, 
    ILogger<AuthorizationBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IMessage
{
    public async ValueTask<TResponse> Handle(TRequest message, MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var user = scope.ServiceProvider.GetRequiredService<IUser>();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
        
        var authorizeAttributes = message.GetType().GetCustomAttributes<AuthorizeAttribute>().ToArray();

        if (authorizeAttributes.Length == 0)
        {
            return await next(message, cancellationToken);
        }

        logger.LogDebug("Authorization required for {RequestType}", typeof(TRequest).Name);

        // Must be an authenticated user
        if (string.IsNullOrWhiteSpace(user.Id))
        {
            logger.LogWarning("Unauthorized access attempt for {RequestType}", typeof(TRequest).Name);
            throw new UnauthorizedAccessException();
        }

        // Role-based authorization
        var authorizeAttributesWithRoles =
            authorizeAttributes.Where(a => a.Roles.Length > 0).ToArray();

        if (authorizeAttributesWithRoles.Length != 0)
        {
            var requiredRoles = authorizeAttributesWithRoles.SelectMany(a => a.Roles).Distinct();
            var userRoles = user.Roles ?? [];
            
            var authorized = requiredRoles.Any(role => userRoles.Contains(role));

            // Must be a member of at least one role in roles
            if (!authorized)
            {
                throw new ForbiddenAccessException();
            }
        }

        // Policy-based authorization
        var authorizeAttributesWithPolicies =
            authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Policy)).ToArray();
        if (authorizeAttributesWithPolicies.Length == 0)
        {
            return await next(message, cancellationToken);
        }

        {
            foreach (var policy in authorizeAttributesWithPolicies.Select(a => a.Policy))
            {
                var authorized = await identityService.AuthorizeAsync(user.Id, policy);

                if (!authorized)
                {
                    throw new ForbiddenAccessException();
                }
            }
        }

        // User is authorized / authorization not required
        return await next(message, cancellationToken);
    }
}
