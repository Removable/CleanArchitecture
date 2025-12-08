# Infrastructure Layer Documentation

> **Path**: `src/CleanArchitecture.Infrastructure`
> **Purpose**: External concerns - database, identity, repositories, and interceptors

## Overview

The Infrastructure layer implements interfaces defined in Domain/Application layers and handles all external concerns:

- **Database**: Entity Framework Core with PostgreSQL
- **Identity**: ASP.NET Core Identity with JWT bearer tokens
- **Repositories**: Generic repository implementation using Ardalis.Specification
- **Interceptors**: EF Core interceptors for auditing and domain events

## Dependency Injection

**Path**: `DependencyInjection.cs`

```csharp
public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
{
    // Connection string
    var connectionString = builder.Configuration.GetConnectionString("CleanArchitectureDb");
    Guard.Against.Null(connectionString);

    // EF Core interceptors (singleton)
    builder.Services.AddSingleton<ISaveChangesInterceptor, AuditableEntityInterceptor>();
    builder.Services.AddSingleton<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

    // DbContext factory with pooling
    builder.Services.AddPooledDbContextFactory<AppDbContext>((sp, options) =>
    {
        options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        options.UseNpgsql(connectionString);
    });

    // Scoped DbContext from factory
    builder.Services.AddScoped<AppDbContext>(provider =>
        provider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

    // Repositories
    builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
    builder.Services.AddScoped(typeof(IReadRepository<>), typeof(EfReadRepository<>));

    // Database initializer
    builder.Services.AddScoped<ApplicationDbContextInitialiser>();

    // Identity
    builder.Services.AddIdentityCore<ApplicationUser>(options => {
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddApiEndpoints();

    // Authentication
    builder.Services.AddAuthentication()
        .AddBearerToken(IdentityConstants.BearerScheme);

    // Authorization policies
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator));
}
```

## Database

### AppDbContext

**Path**: `Data/AppDbContext.cs`

```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Auto-register entities inheriting from BaseEntity
        var entities = Assembly.GetAssembly(typeof(BaseEntity))?.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                && typeof(BaseEntity).IsAssignableFrom(t));

        foreach (var entity in entities)
        {
            entityMethod?.MakeGenericMethod(entity).Invoke(modelBuilder, []);
        }
    }
}
```

**Key Features**:
- Extends `IdentityDbContext<ApplicationUser>` for Identity support
- Auto-discovers entity configurations from assembly
- Auto-registers all `BaseEntity` subclasses
- Uses pooled context factory for performance

### Entity Configurations

**Path**: `Data/Config/`

| Configuration | Entity | Table |
|--------------|--------|-------|
| `TodoListConfiguration.cs` | `TodoList` | `TodoLists` |
| `TodoItemConfiguration.cs` | `TodoItem` | `TodoItems` |

### Database Initialization

**Path**: `Data/ApplicationDbContextInitialiser.cs`

```csharp
public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        if (app.Environment.IsDevelopment())
        {
            await initialiser.InitialiseAsync();  // Delete + Create
        }
        else
        {
            await initialiser.MigrateAsync();     // Apply migrations
        }
        await initialiser.SeedAsync();
    }
}
```

**Initialization Strategies**:
- **Development**: `EnsureDeleted()` + `EnsureCreated()` (fresh DB each run)
- **Production**: `MigrateAsync()` (apply pending migrations)

**Seed Data**:
```csharp
public async Task TrySeedAsync()
{
    // Create Administrator role
    var administratorRole = new IdentityRole(Roles.Administrator);
    await roleManager.CreateAsync(administratorRole);

    // Create admin user
    var administrator = new ApplicationUser
    {
        UserName = "administrator@localhost.dev",
        Email = "administrator@localhost.dev"
    };
    await userManager.CreateAsync(administrator, "Administrator1!");
    await userManager.AddToRolesAsync(administrator, [administratorRole.Name]);

    // Create sample TodoList
    var todoList = new TodoList { Title = "Todo List", UserId = administrator.Id };
    todoList.AddTodoItem(new TodoItem { Title = "Make a todo list", UserId = administrator.Id });
    // ...
}
```

**Default Credentials**:
- Email: `administrator@localhost.dev`
- Password: `Administrator1!`

## Interceptors

EF Core interceptors handle cross-cutting concerns during `SaveChanges`.

### AuditableEntityInterceptor

**Path**: `Data/Interceptors/AuditableEntityInterceptor.cs`

Automatically populates audit fields on `BaseAuditableEntity`.

```csharp
public sealed class AuditableEntityInterceptor(TimeProvider dateTime, IServiceScopeFactory serviceScopeFactory)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public void UpdateEntities(DbContext? context)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var user = scope.ServiceProvider.GetRequiredService<IUser>();

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                var utcNow = dateTime.GetUtcNow();
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = user.Id;
                    entry.Entity.Created = utcNow;
                }
                entry.Entity.LastModifiedBy = user.Id;
                entry.Entity.LastModified = utcNow;
            }
        }
    }
}
```

**Audit Fields Set**:
| State | Fields |
|-------|--------|
| Added | `Created`, `CreatedBy`, `LastModified`, `LastModifiedBy` |
| Modified | `LastModified`, `LastModifiedBy` |

### DispatchDomainEventsInterceptor

**Path**: `Data/Interceptors/DispatchDomainEventsInterceptor.cs`

Dispatches domain events before saving changes.

```csharp
public class DispatchDomainEventsInterceptor(IMediator mediator) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct)
    {
        await DispatchDomainEvents(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, ct);
    }

    public async Task DispatchDomainEvents(DbContext? context)
    {
        // 1. Get entities with domain events
        var entities = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToArray();

        // 2. Collect all events
        var domainEvents = entities.SelectMany(e => e.DomainEvents).ToArray();

        // 3. Clear events from entities
        Array.ForEach(entities, e => e.ClearDomainEvents());

        // 4. Publish each event
        foreach (var domainEvent in domainEvents)
        {
            await mediator.Publish(domainEvent);
        }
    }
}
```

**Event Flow**:
1. `SaveChanges()` triggered
2. Interceptor collects domain events from tracked entities
3. Events cleared from entities (prevent re-dispatch)
4. Events published via Mediator
5. Handlers execute (in same transaction)
6. Actual save occurs

## Repositories

### Repository Pattern

Using [Ardalis.Specification.EntityFrameworkCore](https://github.com/ardalis/Specification) for generic repositories.

**Path**: `Data/`

| Repository | Interface | Purpose |
|------------|-----------|---------|
| `EfRepository<T>` | `IRepository<T>` | Read + Write operations |
| `EfReadRepository<T>` | `IReadRepository<T>` | Read-only operations |
| `EfBaseRepository<T>` | - | Shared base implementation |

```csharp
// Write repository
public class EfRepository<T>(AppDbContext dbContext)
    : RepositoryBase<T>(dbContext), IRepository<T>
    where T : BaseEntity;

// Read-only repository
public class EfReadRepository<T>(AppDbContext dbContext)
    : RepositoryBase<T>(dbContext), IReadRepository<T>
    where T : BaseEntity;
```

**Available Operations** (from Ardalis.Specification):
```csharp
// Read operations (IReadRepository<T>)
Task<T?> GetByIdAsync(TId id, CancellationToken ct);
Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct);
Task<List<T>> ListAsync(CancellationToken ct);
Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken ct);
Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct);
Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken ct);

// Write operations (IRepository<T>)
Task<T> AddAsync(T entity, CancellationToken ct);
Task UpdateAsync(T entity, CancellationToken ct);
Task DeleteAsync(T entity, CancellationToken ct);
Task<int> SaveChangesAsync(CancellationToken ct);
```

## Identity

### ApplicationUser

**Path**: `Identity/ApplicationUser.cs`

```csharp
public class ApplicationUser : IdentityUser
{
    // Extends IdentityUser with any custom properties
}
```

### IdentityService

**Path**: `Identity/IdentityService.cs`

Implements `IIdentityService` from Application layer.

```csharp
public class IdentityService(
    UserManager<ApplicationUser> userManager,
    IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
    IAuthorizationService authorizationService)
    : IIdentityService
{
    public async Task<string?> GetUserNameAsync(string userId);
    public async Task<bool> IsInRoleAsync(string userId, string role);
    public async Task<bool> AuthorizeAsync(string userId, string policyName);
    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);
    public async Task<Result> DeleteUserAsync(string userId);
}
```

### IdentityResultExtensions

**Path**: `Identity/IdentityResultExtensions.cs`

Converts `IdentityResult` to `Ardalis.Result`.

```csharp
public static class IdentityResultExtensions
{
    public static Result ToApplicationResult(this IdentityResult result);
}
```

## Configuration

### Connection String

**Key**: `ConnectionStrings:CleanArchitectureDb`

**PostgreSQL Format**:
```json
{
  "ConnectionStrings": {
    "CleanArchitectureDb": "Host=localhost;Database=CleanArchitectureDb;Username=postgres;Password=postgres"
  }
}
```

### Password Policy

```csharp
options.Password.RequireDigit = true;
options.Password.RequireNonAlphanumeric = true;
options.Password.RequiredLength = 8;
```

### Lockout Policy

```csharp
options.Lockout.MaxFailedAccessAttempts = 5;
options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
```

## Dependencies

```
Infrastructure Layer
├── CleanArchitecture.Application (project reference)
├── CleanArchitecture.Domain (project reference)
├── CleanArchitecture.Shared (project reference)
├── Ardalis.Specification
├── Ardalis.Specification.EntityFrameworkCore
├── Aspire.Npgsql.EntityFrameworkCore.PostgreSQL
├── Microsoft.AspNetCore.Authentication.JwtBearer
├── Microsoft.AspNetCore.Identity.EntityFrameworkCore
├── Microsoft.EntityFrameworkCore
├── Microsoft.EntityFrameworkCore.Relational
├── Npgsql.EntityFrameworkCore.PostgreSQL
└── OpenTelemetry.Extensions.Hosting
```

## Aspire Integration

The project uses .NET Aspire for cloud-native orchestration with built-in:

- **Connection resilience**: Automatic retries and health checks
- **Telemetry**: OpenTelemetry integration
- **Service discovery**: Automatic connection string injection

**From AppHost**:
```csharp
var postgres = builder.AddPostgres("postgres")
    .WithEnvironment("POSTGRES_DB", "CleanArchitectureDb");
var database = postgres.AddDatabase("CleanArchitectureDb");

builder.AddProject<CleanArchitecture_Web>("web")
    .WithReference(database)
    .WaitFor(database);
```

The connection string is automatically injected when running via Aspire.
