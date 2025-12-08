# Domain Layer Documentation

> **Path**: `src/CleanArchitecture.Domain`
> **Purpose**: Core business logic, entities, value objects, domain events, and specifications

## Overview

The Domain layer is the innermost layer in Clean Architecture, containing enterprise business rules with zero dependencies on other layers. It defines:

- **Entities**: Business objects with identity
- **Value Objects**: Immutable objects defined by attributes
- **Domain Events**: Notifications of domain changes
- **Specifications**: Encapsulated query logic
- **Interfaces**: Contracts for repositories

## Entity Hierarchy

```
BaseEntity
└── BaseAuditableEntity
    ├── TodoList (Aggregate Root)
    └── TodoItem
```

### BaseEntity

**Path**: `Common/BaseEntity.cs`

Base class for all domain entities providing identity and domain event support.

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; } = GuidHelper.NewGuidId();
    public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(BaseEvent domainEvent);
    public void RemoveDomainEvent(BaseEvent domainEvent);
    public void ClearDomainEvents();
}
```

**Key Features**:
- Auto-generated GUID using `GuidHelper.NewGuidId()` (v7 UUID for time-ordering)
- Domain events collection for eventual consistency patterns
- Events are dispatched by `DispatchDomainEventsInterceptor`

### BaseAuditableEntity

**Path**: `Common/BaseAuditableEntity.cs`

Extends `BaseEntity` with audit trail properties.

```csharp
public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTimeOffset Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}
```

**Populated By**: `AuditableEntityInterceptor` (Infrastructure layer)

## Aggregates

### TodoList Aggregate

The `TodoList` is the aggregate root that owns `TodoItem` entities.

#### TodoList (Aggregate Root)

**Path**: `TodoListAggregate/TodoList.cs`

```csharp
[Table("TodoLists")]
public sealed class TodoList : BaseAuditableEntity, IAggregateRoot, IOwnerId
{
    public required string Title { get; set; }
    public Colour Colour { get; set; } = Colour.White;
    public required string UserId { get; set; }
    public IReadOnlyCollection<TodoItem> Items { get; }

    public void AddTodoItem(TodoItem item);
    public void RemoveTodoItem(TodoItem item);
    public void RemoveTodoItem(Guid id);
    public void ClearTodoItems();
}
```

**Business Rules**:
- Title is required, max length defined in `LengthConstants.MediumTitleMaxLength`
- Each list has an owner (`UserId`)
- Items are managed through aggregate methods (not direct collection access)
- Domain events raised on item operations

#### TodoItem

**Path**: `TodoListAggregate/TodoItem.cs`

```csharp
[Table("TodoItems")]
public sealed class TodoItem : BaseAuditableEntity, IOwnerId
{
    public Guid ListId { get; set; }
    public string? Title { get; set; }
    public string? Note { get; set; }
    public required string UserId { get; set; }
    public PriorityLevel Priority { get; set; }
    public DateTime? Reminder { get; set; }
    public bool Done { get; set; }  // Raises TodoItemCompletedEvent when set to true
    public TodoList List { get; set; }
}
```

**Domain Behavior**:
- Setting `Done = true` automatically raises `TodoItemCompletedEvent`
- Belongs to a `TodoList` via `ListId`

## Value Objects

### Colour

**Path**: `TodoListAggregate/Colour.cs`

Immutable value object representing a hex color code.

```csharp
public readonly record struct Colour(string Code)
{
    public static Colour White => new("#FFFFFF");
    public static Colour Red => new("#FF5733");
    public static Colour Orange => new("#FFC300");
    public static Colour Yellow => new("#FFFF66");
    public static Colour Green => new("#CCFF99");
    public static Colour Blue => new("#6666FF");
    public static Colour Purple => new("#9966CC");
    public static Colour Grey => new("#999999");

    public static Colour From(string code);  // Throws UnsupportedColourException if invalid
}
```

**Supported Colors**: White, Red, Orange, Yellow, Green, Blue, Purple, Grey

## Enums

### PriorityLevel

**Path**: `TodoListAggregate/Enums/PriorityLevel.cs`

```csharp
public enum PriorityLevel
{
    None = 0,
    Low = 1,
    Medium = 2,
    High = 3
}
```

## Domain Events

Events are records inheriting from `BaseEvent` (which implements `INotification`).

| Event | Path | Trigger |
|-------|------|---------|
| `TodoItemCreatedEvent` | `Events/TodoItemCreatedEvent.cs` | `TodoList.AddTodoItem()` |
| `TodoItemCompletedEvent` | `Events/TodoItemCompletedEvent.cs` | `TodoItem.Done = true` |
| `TodoItemDeletedEvent` | `Events/TodoItemDeletedEvent.cs` | `TodoList.RemoveTodoItem()` |

**Event Structure**:
```csharp
public sealed record TodoItemCreatedEvent(TodoItem Item) : BaseEvent;
```

**Event Flow**:
1. Entity method raises event via `AddDomainEvent()`
2. `DispatchDomainEventsInterceptor` collects events before `SaveChanges`
3. Events published via `IMediator.Publish()`
4. Handlers in Application layer process events

## Specifications

Using [Ardalis.Specification](https://github.com/ardalis/Specification) for query encapsulation.

| Specification | Path | Purpose |
|--------------|------|---------|
| `GetUserTodoListsSpec` | `Specifications/GetUserTodoListsSpec.cs` | Get lists for a user, ordered by title |
| `TodoListGetByIdSpec` | `Specifications/TodoListGetByIdSpec.cs` | Get single list by ID |
| `GetTodoItemsByListIdSpec` | `Specifications/GetTodoItemsByListIdSpec.cs` | Get items for a list |
| `TodoListTitleUniquenessCheckSpec` | `Specifications/TodoListTitleUniquenessCheckSpec.cs` | Check title uniqueness |

**Example**:
```csharp
public sealed class GetUserTodoListsSpec : Specification<TodoList>
{
    public GetUserTodoListsSpec(string userId)
    {
        Query.Where(x => x.UserId == userId)
             .OrderBy(x => x.Title);
    }
}
```

## Interfaces

### Repository Interfaces

**Path**: `Common/Interfaces/`

```csharp
// Read operations
public interface IReadRepository<T> : IReadRepositoryBase<T> where T : BaseEntity;

// Read + Write operations
public interface IRepository<T> : IRepositoryBase<T> where T : BaseEntity;
```

Both extend Ardalis.Specification base interfaces.

### Marker Interfaces

| Interface | Purpose |
|-----------|---------|
| `IAggregateRoot` | Marks entities as aggregate roots |
| `ISoftDeletion` | Marks entities supporting soft delete |
| `IOwnerId` | Entities with `UserId` ownership |

## Constants

**Path**: `Constants/`

| Constant | Path | Values |
|----------|------|--------|
| `LengthConstants` | `LengthConstants.cs` | `MediumTitleMaxLength`, `UserIdMaxLength` |
| `Roles` | `Roles.cs` | `Administrator` |
| `Policies` | `Policies.cs` | `CanPurge` |

## Exceptions

### UnsupportedColourException

**Path**: `Exceptions/UnsupportedColourException.cs`

Thrown when creating a `Colour` from an unsupported hex code.

## Helper Classes

### PaginatedList<T>

**Path**: `Common/PaginatedList.cs`

Generic paginated list for query results.

### SingleResultSpecification

**Path**: `Common/SingleResultSpecification.cs`

Base specification for queries expecting single results.

## Dependency Graph

```
Domain Layer
├── Ardalis.GuardClauses (validation)
├── Ardalis.Result (result pattern)
├── Ardalis.Specification (query specs)
├── Mediator.Abstractions (INotification)
├── Microsoft.EntityFrameworkCore (attributes only)
├── Mapster (projection)
└── CleanArchitecture.Shared (GuidHelper)
```

## Testing

Domain entities are tested in `tests/CleanArchitecture.UnitTests/Domain/`:

- `TodoListTests.cs` - Aggregate behavior tests
- `TodoItemTests.cs` - Entity tests
- `ColourTests.cs` - Value object tests

**Test Examples**:
```csharp
[Fact]
public void ShouldAddTodoItemAndRaiseCreatedEvent()
{
    var list = new TodoList { UserId = "", Title = "Test" };
    var item = new TodoItem { UserId = "", Title = "Item 1" };

    list.AddTodoItem(item);

    list.Items.Count.ShouldBe(1);
    list.DomainEvents.OfType<TodoItemCreatedEvent>().Any(e => e.Item == item).ShouldBeTrue();
}
```
