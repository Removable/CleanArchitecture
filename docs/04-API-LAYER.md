# API Layer (Web) Documentation

> **Path**: `src/CleanArchitecture.Web`
> **Purpose**: HTTP API endpoints, Swagger, authentication, and request handling

## Overview

The Web layer provides the HTTP API using **FastEndpoints** - a minimal API framework that combines the simplicity of minimal APIs with structured endpoint classes.

- **FastEndpoints**: Structured endpoint classes
- **Swagger/OpenAPI**: Interactive API documentation
- **Authentication**: Bearer token with ASP.NET Identity
- **Exception Handling**: Global ProblemDetails responses

## Dependency Injection

**Path**: `DependencyInjection.cs`

```csharp
public static void AddWebServices(this IHostApplicationBuilder builder)
{
    // Development only
    if (builder.Environment.IsDevelopment())
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    // Current user service
    builder.Services.AddScoped<IUser, CurrentUser>();

    // HTTP context
    builder.Services.AddHttpContextAccessor();

    // Exception handling
    builder.Services.AddExceptionHandler<CustomExceptionHandler>();
    builder.Services.AddProblemDetails();

    // Suppress default 400 response
    builder.Services.Configure<ApiBehaviorOptions>(options =>
        options.SuppressModelStateInvalidFilter = true);

    // OpenAPI
    builder.Services.AddEndpointsApiExplorer();

    // FastEndpoints
    builder.Services.AddFastEndpointsService();
}
```

## Entry Point

**Path**: `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

// Service registration (order matters)
builder.AddServiceDefaults();      // Aspire defaults
builder.AddApplicationServices();   // Mediator, validators
builder.AddInfrastructureServices();// EF Core, Identity
builder.AddWebServices();           // FastEndpoints, Swagger

var app = builder.Build();

// Database initialization (skipped in Testing)
if (!app.Environment.IsEnvironment("Testing"))
    await app.InitialiseDatabaseAsync();

// Middleware pipeline
if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseExceptionHandler();

// Swagger UI at /api
app.UseSwaggerUi(settings => settings.Path = "/api");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Routes
app.Map("/", () => Results.Redirect("/api"));
app.MapDefaultEndpoints();  // Health, metrics
app.MapEndpoints();         // FastEndpoints

app.Run();
```

## Endpoints

FastEndpoints organizes APIs as endpoint classes with Configure/Handle methods.

### Endpoint Groups

**Path**: `EndpointGroups/`

Groups provide shared configuration for related endpoints.

```csharp
// TodoListGroup.cs
public sealed class TodoListGroup : Group
{
    public TodoListGroup()
    {
        Configure("TodoLists", ep =>
        {
            ep.Description(x => x
                .Produces((int)HttpStatusCode.Unauthorized)
                .WithTags("TodoLists"));
        });
    }
}

// TodoItemGroup.cs
public sealed class TodoItemGroup : Group
{
    public TodoItemGroup()
    {
        Configure("TodoItems", ep => { /* ... */ });
    }
}
```

### TodoLists Endpoints

**Path**: `Endpoints/TodoLists/`

| Endpoint | Method | Route | Description |
|----------|--------|-------|-------------|
| `GetTodoLists` | GET | `/api/v1/TodoLists` | Get all user's lists |
| `CreateTodoList` | POST | `/api/v1/TodoLists` | Create new list |
| `UpdateTodoList` | PUT | `/api/v1/TodoLists/{id}` | Update list |
| `DeleteTodoList` | DELETE | `/api/v1/TodoLists/{id}` | Delete list |

**GetTodoLists Example**:
```csharp
public sealed class GetTodoLists(IMediator mediator)
    : EndpointWithoutRequest<Results<Ok<TodosVm>, UnauthorizedHttpResult>>
{
    public override void Configure()
    {
        Version(1);
        Get("/");
        Group<TodoListGroup>();
    }

    public override async Task<Results<Ok<TodosVm>, UnauthorizedHttpResult>> ExecuteAsync(CancellationToken ct)
    {
        var vm = await mediator.Send(new GetTodosQuery(), ct);
        return TypedResults.Ok(vm);
    }
}
```

**CreateTodoList Example**:
```csharp
sealed class CreateTodoListResponse
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
}

sealed class CreateTodoListEndpoint(IMediator mediator)
    : Endpoint<CreateTodoListCommand, CreateTodoListResponse>
{
    public override void Configure()
    {
        Version(1);
        Post("");
        Group<TodoListGroup>();
    }

    public override async Task HandleAsync(CreateTodoListCommand r, CancellationToken c)
    {
        var id = await mediator.Send(r, c);
        var res = new CreateTodoListResponse { Id = id, Title = r.Title };
        await Send.CreatedAtAsync<CreateTodoListEndpoint>(new { id }, res, cancellation: c);
    }
}
```

### TodoItems Endpoints

**Path**: `Endpoints/TodoItems/`

| Endpoint | Method | Route | Description |
|----------|--------|-------|-------------|
| `GetTodoItemsWithPagination` | GET | `/api/v1/TodoItems` | Paginated items |

## API Versioning

FastEndpoints supports versioning via the `Version()` method:

```csharp
public override void Configure()
{
    Version(1);  // Results in /api/v1/...
    Get("/");
}
```

**URL Pattern**: `/api/v{version}/{group}/{route}`

Example: `/api/v1/TodoLists`

## Exception Handling

### CustomExceptionHandler

**Path**: `Extensions/CustomExceptionHandler.cs`

Global exception handler implementing `IExceptionHandler`.

```csharp
public class CustomExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        switch (exception)
        {
            case ValidationException:
                await HandleValidationException(httpContext, exception);
                return true;
            case NotFoundException:
                await HandleNotFoundException(httpContext, exception);
                return true;
            case UnauthorizedAccessException:
                await HandleUnauthorizedAccessException(httpContext);
                return true;
            case ForbiddenAccessException:
                await HandleForbiddenAccessException(httpContext);
                return true;
            default:
                return false;  // Let default handler process
        }
    }
}
```

### Response Mapping

| Exception | HTTP Status | Response Type |
|-----------|-------------|---------------|
| `ValidationException` | 400 Bad Request | `ValidationProblemDetails` |
| `NotFoundException` | 404 Not Found | `ProblemDetails` |
| `UnauthorizedAccessException` | 401 Unauthorized | `ProblemDetails` |
| `ForbiddenAccessException` | 403 Forbidden | `ProblemDetails` |

**ValidationProblemDetails Example**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "status": 400,
  "errors": {
    "Title": ["'Title' must not be empty."]
  }
}
```

## Authentication

### CurrentUser Service

**Path**: `Services/CurrentUser.cs`

Implements `IUser` by extracting claims from `HttpContext`.

```csharp
public class CurrentUser(IHttpContextAccessor httpContextAccessor) : IUser
{
    public string? Id => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string[]? Roles => httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)
        .Select(c => c.Value).ToArray();
}
```

### Authentication Flow

1. Client sends Bearer token in `Authorization` header
2. `UseAuthentication()` middleware validates token
3. `UseAuthorization()` middleware checks policies
4. `CurrentUser` extracts claims from validated token
5. `AuthorizationBehaviour` in pipeline checks `[Authorize]` attributes

### Default Authentication

The project uses ASP.NET Identity with Bearer tokens:

```csharp
builder.Services.AddAuthentication()
    .AddBearerToken(IdentityConstants.BearerScheme);
```

Identity endpoints are auto-mapped by `.AddApiEndpoints()`.

## Swagger / OpenAPI

**Path**: `/api`

Swagger UI is available at `/api` for interactive API exploration.

```csharp
app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
});
```

**Features**:
- Interactive endpoint testing
- Request/response schema display
- Authentication support
- Model validation display

## Extensions

### FastEndpointsExtension

**Path**: `Extensions/FastEndpointsExtension.cs`

Configures FastEndpoints services and options.

### WebApplicationExtensions

**Path**: `Extensions/WebApplicationExtensions.cs`

Extension methods for `WebApplication` configuration.

```csharp
public static void MapEndpoints(this WebApplication app)
{
    app.UseFastEndpoints(c =>
    {
        c.Endpoints.RoutePrefix = "api";
        // ... additional configuration
    });
}
```

## Configuration Files

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### launchSettings.json

**Path**: `Properties/launchSettings.json`

```json
{
  "profiles": {
    "https": {
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

## Dependencies

```
Web Layer
├── CleanArchitecture.Infrastructure (project reference)
├── CleanArchitecture.ServiceDefaults (project reference)
├── Ardalis.Result
├── Ardalis.Result.AspNetCore
├── FastEndpoints
├── FastEndpoints.Swagger
├── FluentValidation.AspNetCore
├── Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore
├── Microsoft.AspNetCore.Identity.EntityFrameworkCore
├── Microsoft.EntityFrameworkCore.Design
└── Serilog.AspNetCore
```

## Testing Environment

The Web project supports a "Testing" environment that skips database initialization:

```csharp
if (!app.Environment.IsEnvironment("Testing"))
{
    await app.InitialiseDatabaseAsync();
}
```

Used by `CustomWebApplicationFactory` in integration tests.

## Endpoint Pattern

All endpoints follow this pattern:

```csharp
// Without request body
public sealed class GetXxx(IMediator mediator)
    : EndpointWithoutRequest<Results<Ok<TResponse>, UnauthorizedHttpResult>>
{
    public override void Configure()
    {
        Version(1);
        Get("/route");
        Group<XxxGroup>();
    }

    public override async Task<...> ExecuteAsync(CancellationToken ct)
    {
        var result = await mediator.Send(new Query(), ct);
        return TypedResults.Ok(result);
    }
}

// With request body
public sealed class CreateXxx(IMediator mediator)
    : Endpoint<CreateXxxCommand, CreateXxxResponse>
{
    public override void Configure()
    {
        Version(1);
        Post("/route");
        Group<XxxGroup>();
    }

    public override async Task HandleAsync(CreateXxxCommand req, CancellationToken ct)
    {
        var id = await mediator.Send(req, ct);
        await Send.CreatedAtAsync<CreateXxx>(new { id }, response, cancellation: ct);
    }
}
```

## Route Summary

| Method | Route | Handler | Auth |
|--------|-------|---------|------|
| GET | `/` | Redirect to `/api` | No |
| GET | `/api` | Swagger UI | No |
| GET | `/api/v1/TodoLists` | GetTodos | Yes |
| POST | `/api/v1/TodoLists` | CreateTodoList | Yes |
| PUT | `/api/v1/TodoLists/{id}` | UpdateTodoList | Yes |
| DELETE | `/api/v1/TodoLists/{id}` | DeleteTodoList | Yes |
| GET | `/api/v1/TodoItems` | GetTodoItemsWithPagination | Yes |
