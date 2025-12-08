# Application Layer Documentation

> **Path**: `src/CleanArchitecture.Application`
> **Purpose**: CQRS handlers, validation, behaviors, DTOs, and use case orchestration

## Overview

The Application layer implements the **CQRS (Command Query Responsibility Segregation)** pattern using source-generated Mediator. It contains:

- **Commands**: Write operations that modify state
- **Queries**: Read operations that return data
- **Validators**: FluentValidation rules per request
- **Behaviors**: Cross-cutting pipeline concerns
- **DTOs**: Data transfer objects for responses

## Architecture

```
Request → Pipeline Behaviors → Handler → Response
              │
              ├── LoggingBehaviour
              ├── UnhandledExceptionBehaviour
              ├── AuthorizationBehaviour
              ├── ValidationBehaviour
              └── PerformanceBehaviour
```

## Dependency Injection

**Path**: `DependencyInjection.cs`

```csharp
public static void AddApplicationServices(this IHostApplicationBuilder builder)
{
    builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

    builder.Services.AddMediator(options =>
    {
        options.Assemblies = [typeof(LookupDto)];
        options.PipelineBehaviors = [
            typeof(LoggingBehaviour<,>),
            typeof(UnhandledExceptionBehaviour<,>),
            typeof(AuthorizationBehaviour<,>),
            typeof(ValidationBehaviour<,>),
            typeof(PerformanceBehaviour<,>)
        ];
    });
}
```

## Pipeline Behaviors

Behaviors execute in order for every request. All behaviors use `IServiceScopeFactory` for scoped service resolution.

### 1. LoggingBehaviour

**Path**: `Common/Behaviours/LoggingBehaviour.cs`

Logs request details before handler execution.

```csharp
public sealed class LoggingBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
```

### 2. UnhandledExceptionBehaviour

**Path**: `Common/Behaviours/UnhandledExceptionBehaviour.cs`

Catches and logs unhandled exceptions with request context.

```csharp
public sealed class UnhandledExceptionBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
```

### 3. AuthorizationBehaviour

**Path**: `Common/Behaviours/AuthorizationBehaviour.cs`

Enforces `[Authorize]` attribute requirements.

```csharp
public sealed class AuthorizationBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
```

**Flow**:
1. Check for `[Authorize]` attributes on request type
2. Verify user is authenticated (`IUser.Id` not null)
3. Check role requirements via `IUser.Roles`
4. Check policy requirements via `IIdentityService.AuthorizeAsync()`
5. Throw `UnauthorizedAccessException` or `ForbiddenAccessException` on failure

### 4. ValidationBehaviour

**Path**: `Common/Behaviours/ValidationBehaviour.cs`

Runs FluentValidation validators before handler.

```csharp
public sealed class ValidationBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
```

**Flow**:
1. Resolve all `IValidator<TRequest>` from DI
2. Run all validators in parallel
3. Collect failures and throw `ValidationException` if any

### 5. PerformanceBehaviour

**Path**: `Common/Behaviours/PerformanceBehaviour.cs`

Logs slow requests (>500ms) for performance monitoring.

## Features

Features are organized by aggregate/domain concept using vertical slice architecture.

### TodoLists Feature

#### Commands

| Command | Path | Description |
|---------|------|-------------|
| `CreateTodoListCommand` | `Commands/CreateTodoList/CreateTodoList.cs` | Create new list |
| `UpdateTodoListCommand` | `Commands/UpdateTodoList/UpdateTodoList.cs` | Update list title |
| `DeleteTodoListCommand` | `Commands/DeleteTodoList/DeleteTodoList.cs` | Delete list by ID |
| `PurgeTodoListsCommand` | `Commands/PurgeTodoLists/PurgeTodoLists.cs` | Delete all lists (Admin only) |

**CreateTodoList Example**:
```csharp
[Authorize]
public sealed record CreateTodoListCommand : IRequest<Guid>
{
    public required string Title { get; init; }
}

public sealed class CreateTodoListCommandHandler(IServiceScopeFactory serviceScopeFactory)
    : IRequestHandler<CreateTodoListCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateTodoListCommand request, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<TodoList>>();
        var user = scope.ServiceProvider.GetRequiredService<IUser>();

        var entity = new TodoList { Title = request.Title, UserId = user.Id! };

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
```

#### Queries

| Query | Path | Description |
|-------|------|-------------|
| `GetTodosQuery` | `Queries/GetTodos/GetTodos.cs` | Get all lists with items for current user |

**GetTodos Example**:
```csharp
[Authorize]
public sealed record GetTodosQuery : IRequest<TodosVm>;

public sealed class GetTodosQueryHandler(IServiceScopeFactory serviceScopeFactory)
    : IRequestHandler<GetTodosQuery, TodosVm>
{
    public async ValueTask<TodosVm> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IReadRepository<TodoList>>();
        var user = scope.ServiceProvider.GetRequiredService<IUser>();

        var spec = new GetUserTodoListsSpec(Guard.Against.NullOrEmpty(user.Id));

        return new TodosVm
        {
            PriorityLevels = Enum.GetValues<PriorityLevel>()
                .Select(p => new LookupDto { Id = (int)p, Title = p.ToString() })
                .ToArray(),
            Lists = await repository.ArrayAsync<TodoListDto>(spec, cancellationToken)
        };
    }
}
```

#### Validators

| Validator | Path | Rules |
|-----------|------|-------|
| `CreateTodoListCommandValidator` | `Commands/CreateTodoList/CreateTodoListCommandValidator.cs` | Title required, max length |
| `UpdateTodoListCommandValidator` | `Commands/UpdateTodoList/UpdateTodoListCommandValidator.cs` | Id required, Title required |

### TodoItems Feature

#### Commands

| Command | Path | Description |
|---------|------|-------------|
| `CreateTodoItemCommand` | `Commands/CreateTodoItem/CreateTodoItem.cs` | Add item to list |
| `UpdateTodoItemCommand` | `Commands/UpdateTodoItem/UpdateTodoItem.cs` | Update item |
| `UpdateTodoItemDetailCommand` | `Commands/UpdateTodoItemDetail/UpdateTodoItemDetail.cs` | Update item details |
| `DeleteTodoItemCommand` | `Commands/DeleteTodoItem/DeleteTodoItem.cs` | Delete item |

#### Queries

| Query | Path | Description |
|-------|------|-------------|
| `GetTodoItemsWithPaginationQuery` | `Queries/GetTodoItemsWithPagination/GetTodoItemsWithPagination.cs` | Paginated items list |

#### Event Handlers

| Handler | Path | Handles |
|---------|------|---------|
| `TodoItemCreatedEventHandler` | `EventHandlers/TodoItemCreatedEventHandler.cs` | `TodoItemCreatedEvent` |
| `TodoItemCompletedEventHandler` | `EventHandlers/TodoItemCompletedEventHandler.cs` | `TodoItemCompletedEvent` |

## DTOs

### View Models

| DTO | Path | Purpose |
|-----|------|---------|
| `TodosVm` | `Queries/GetTodos/TodosVm.cs` | Main view model with lists and priority levels |
| `TodoListDto` | `Queries/GetTodos/TodoListDto.cs` | List with items |
| `TodoItemDto` | `Queries/GetTodos/TodoItemDto.cs` | Item details |
| `TodoItemBriefDto` | `Queries/GetTodoItemsWithPagination/TodoItemBriefDto.cs` | Simplified item |
| `LookupDto` | `Common/Models/LookupDto.cs` | Generic lookup (id, title) |

### Mappings

**Path**: `Common/Mappings/MappingExtensions.cs`

Uses **Mapster** for object mapping with extension methods.

```csharp
public static class MappingExtensions
{
    public static TDestination[] ArrayAsync<TDestination>(
        this IReadRepository<T> repository,
        Specification<T> spec,
        CancellationToken ct);
}
```

## Security

### AuthorizeAttribute

**Path**: `Common/Security/AuthorizeAttribute.cs`

Custom attribute for declarative authorization on commands/queries.

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AuthorizeAttribute : Attribute
{
    public string[] Roles { get; set; } = [];
    public string Policy { get; set; } = string.Empty;
}
```

**Usage**:
```csharp
[Authorize]  // Requires authentication
public sealed record GetTodosQuery : IRequest<TodosVm>;

[Authorize(Roles = [Roles.Administrator])]  // Requires Admin role
public sealed record PurgeTodoListsCommand : IRequest;

[Authorize(Policy = Policies.CanPurge)]  // Requires CanPurge policy
public sealed record PurgeTodoListsCommand : IRequest;
```

## Interfaces

### IUser

**Path**: `Common/Interfaces/IUser.cs`

Represents the current authenticated user.

```csharp
public interface IUser
{
    string? Id { get; }
    string[]? Roles { get; }
}
```

### IIdentityService

**Path**: `Common/Interfaces/IIdentityService.cs`

Identity operations contract.

```csharp
public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<bool> AuthorizeAsync(string userId, string policyName);
    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);
    Task<Result> DeleteUserAsync(string userId);
}
```

## Exceptions

| Exception | Path | HTTP Status |
|-----------|------|-------------|
| `ValidationException` | `Common/Exceptions/ValidationException.cs` | 400 Bad Request |
| `ForbiddenAccessException` | `Common/Exceptions/ForbiddenAccessException.cs` | 403 Forbidden |

**ValidationException**:
```csharp
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> failures)
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}
```

## Dependencies

```
Application Layer
├── CleanArchitecture.Domain (project reference)
├── Ardalis.Result
├── Ardalis.Specification
├── FastEndpoints (IMediator)
├── FluentValidation.DependencyInjectionExtensions
├── Mapster
├── Mediator.Abstractions
└── Mediator.SourceGenerator (compile-time)
```

## Handler Pattern

All handlers follow this pattern:

```csharp
public sealed class XxxHandler(IServiceScopeFactory serviceScopeFactory)
    : IRequestHandler<XxxCommand, TResponse>
{
    public async ValueTask<TResponse> Handle(XxxCommand request, CancellationToken ct)
    {
        // 1. Create scope for scoped services
        using var scope = serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<T>>();
        var user = scope.ServiceProvider.GetRequiredService<IUser>();

        // 2. Execute business logic
        // ...

        // 3. Return result
        return result;
    }
}
```

**Why IServiceScopeFactory?**
- Mediator handlers are singletons (source-generated)
- Scoped services (DbContext, IUser) require explicit scope creation
- Ensures proper lifetime management and disposal
