# Documentation Index

> CleanArchitecture Template - Knowledge Base

## Quick Navigation

| Document | Description |
|----------|-------------|
| [PROJECT_INDEX.md](../PROJECT_INDEX.md) | High-level project overview and structure |
| [01-DOMAIN-LAYER.md](01-DOMAIN-LAYER.md) | Entities, aggregates, events, specifications |
| [02-APPLICATION-LAYER.md](02-APPLICATION-LAYER.md) | CQRS handlers, validators, behaviors |
| [03-INFRASTRUCTURE-LAYER.md](03-INFRASTRUCTURE-LAYER.md) | Database, identity, repositories |
| [04-API-LAYER.md](04-API-LAYER.md) | FastEndpoints, Swagger, authentication |

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                        Presentation                          │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                    Web (API)                          │   │
│  │  FastEndpoints · Swagger · Exception Handling        │   │
│  └─────────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────────┤
│                       Application                            │
│  ┌─────────────────────────────────────────────────────┐   │
│  │              CQRS · Mediator · Validation             │   │
│  │  Commands · Queries · Behaviors · DTOs               │   │
│  └─────────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────────┤
│                        Domain                                │
│  ┌─────────────────────────────────────────────────────┐   │
│  │         Entities · Value Objects · Events            │   │
│  │  Aggregates · Specifications · Interfaces            │   │
│  └─────────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────────┤
│                      Infrastructure                          │
│  ┌─────────────────────────────────────────────────────┐   │
│  │        EF Core · Identity · Repositories             │   │
│  │  PostgreSQL · Interceptors · Configurations          │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Key Concepts

### Clean Architecture Layers

| Layer | Project | Responsibility |
|-------|---------|----------------|
| **Domain** | `CleanArchitecture.Domain` | Business entities and rules |
| **Application** | `CleanArchitecture.Application` | Use cases and orchestration |
| **Infrastructure** | `CleanArchitecture.Infrastructure` | External concerns |
| **Presentation** | `CleanArchitecture.Web` | API endpoints |

### Dependency Flow

```
Web → Application → Domain ← Infrastructure
           ↓
    Infrastructure (implements Domain interfaces)
```

### CQRS Pattern

```
Request → Pipeline Behaviors → Handler → Response
              │
              ├── Logging
              ├── Exception Handling
              ├── Authorization
              ├── Validation
              └── Performance Monitoring
```

### Domain Events Flow

```
Entity.AddDomainEvent() → SaveChanges() → DispatchDomainEventsInterceptor → IMediator.Publish()
```

## Common Tasks

### Adding a New Feature

1. **Domain**: Create entity in `Domain/NewAggregate/`
2. **Application**: Add command/query in `Application/Features/New/`
3. **Infrastructure**: Add EF configuration in `Infrastructure/Data/Config/`
4. **Web**: Create endpoint in `Web/Endpoints/New/`

### Adding a New Command

```csharp
// 1. Create command record
[Authorize]
public sealed record CreateXxxCommand : IRequest<Guid>
{
    public required string Property { get; init; }
}

// 2. Create handler
public sealed class CreateXxxCommandHandler(IServiceScopeFactory sf)
    : IRequestHandler<CreateXxxCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateXxxCommand req, CancellationToken ct)
    {
        using var scope = sf.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<Xxx>>();
        // ... implementation
    }
}

// 3. Create validator
public sealed class CreateXxxCommandValidator : AbstractValidator<CreateXxxCommand>
{
    public CreateXxxCommandValidator()
    {
        RuleFor(x => x.Property).NotEmpty();
    }
}

// 4. Create endpoint
public sealed class CreateXxxEndpoint(IMediator mediator) : Endpoint<CreateXxxCommand, XxxResponse>
{
    public override void Configure()
    {
        Version(1);
        Post("/xxx");
        Group<XxxGroup>();
    }

    public override async Task HandleAsync(CreateXxxCommand req, CancellationToken ct)
    {
        var id = await mediator.Send(req, ct);
        await Send.CreatedAtAsync<CreateXxxEndpoint>(new { id }, new XxxResponse { Id = id }, cancellation: ct);
    }
}
```

### Adding a Domain Event

```csharp
// 1. Create event
public sealed record XxxCreatedEvent(Xxx Entity) : BaseEvent;

// 2. Raise in entity
public void DoSomething()
{
    AddDomainEvent(new XxxCreatedEvent(this));
}

// 3. Create handler
public sealed class XxxCreatedEventHandler : INotificationHandler<XxxCreatedEvent>
{
    public ValueTask Handle(XxxCreatedEvent notification, CancellationToken ct)
    {
        // Handle event
        return ValueTask.CompletedTask;
    }
}
```

## Configuration Reference

### Connection Strings

| Key | Description |
|-----|-------------|
| `ConnectionStrings:CleanArchitectureDb` | PostgreSQL connection |

### Default Users

| Email | Password | Roles |
|-------|----------|-------|
| `administrator@localhost.dev` | `Administrator1!` | Administrator |

### Environment Variables

| Variable | Description |
|----------|-------------|
| `ASPNETCORE_ENVIRONMENT` | Development, Production, Testing |

## Testing

### Unit Tests

```bash
dotnet test tests/CleanArchitecture.UnitTests
```

Coverage: Domain entities and aggregates

### Integration Tests

```bash
dotnet test tests/CleanArchitecture.IntegrationTests
```

Coverage: API endpoints and database operations

## Running the Application

### With Aspire (Recommended)

```bash
dotnet run --project src/CleanArchitecture.AppHost
```

Starts: Web API + PostgreSQL container

### Standalone

```bash
# Set connection string first
dotnet run --project src/CleanArchitecture.Web
```

### URLs

| URL | Description |
|-----|-------------|
| `https://localhost:5001` | HTTPS endpoint |
| `https://localhost:5001/api` | Swagger UI |

## Technology Stack

| Category | Technology |
|----------|------------|
| Framework | .NET 9.0 |
| API | FastEndpoints |
| CQRS | Mediator (source-generated) |
| Validation | FluentValidation |
| Mapping | Mapster |
| ORM | Entity Framework Core |
| Database | PostgreSQL |
| Identity | ASP.NET Core Identity |
| Specifications | Ardalis.Specification |
| Orchestration | .NET Aspire |
| Testing | xUnit, Shouldly, FluentAssertions |

## File Structure

```
CleanArchitecture/
├── src/
│   ├── CleanArchitecture.AppHost/          # Aspire orchestration
│   ├── CleanArchitecture.Web/              # API layer
│   │   ├── Endpoints/                      # FastEndpoints
│   │   ├── EndpointGroups/                 # Endpoint groupings
│   │   ├── Extensions/                     # Exception handling, etc.
│   │   └── Services/                       # CurrentUser
│   ├── CleanArchitecture.Application/      # CQRS layer
│   │   ├── Common/                         # Behaviors, interfaces
│   │   └── Features/                       # Commands, queries
│   ├── CleanArchitecture.Infrastructure/   # Data access layer
│   │   ├── Data/                           # DbContext, repositories
│   │   └── Identity/                       # User, service
│   ├── CleanArchitecture.Domain/           # Core domain
│   │   ├── Common/                         # Base classes
│   │   └── TodoListAggregate/              # Entities, events
│   ├── CleanArchitecture.ServiceDefaults/  # Aspire defaults
│   └── CleanArchitecture.Shared/           # Utilities
├── tests/
│   ├── CleanArchitecture.UnitTests/        # Domain tests
│   └── CleanArchitecture.IntegrationTests/ # API tests
├── docs/                                   # Documentation
└── .template.config/                       # dotnet new template
```

## Cross-Reference Index

### By Feature

| Feature | Domain | Application | Infrastructure | Web |
|---------|--------|-------------|----------------|-----|
| TodoList | `TodoList.cs` | `CreateTodoList.cs` | `TodoListConfiguration.cs` | `CreateTodoListEndpoint.cs` |
| TodoItem | `TodoItem.cs` | `CreateTodoItem.cs` | `TodoItemConfiguration.cs` | `GetTodoItemsWithPagination.cs` |

### By Pattern

| Pattern | Implementation |
|---------|----------------|
| Aggregate Root | `TodoList : IAggregateRoot` |
| Value Object | `Colour` |
| Domain Event | `TodoItemCreatedEvent` |
| Specification | `GetUserTodoListsSpec` |
| Repository | `EfRepository<T>` |
| CQRS Handler | `CreateTodoListCommandHandler` |
| Pipeline Behavior | `ValidationBehaviour<,>` |
| Exception Handler | `CustomExceptionHandler` |
