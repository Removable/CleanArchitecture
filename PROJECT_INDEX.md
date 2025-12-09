# Project Index: CleanArchitecture Template

Generated: 2025-12-09

## Overview

ASP.NET Core 10 Clean Architecture project template with .NET Aspire orchestration. Implements a TodoList/TodoItem domain using CQRS pattern with Mediator, FastEndpoints for API, and PostgreSQL for persistence.

## Project Structure

```
CleanArchitecture/
├── src/
│   ├── CleanArchitecture.AppHost/       # .NET Aspire orchestration host
│   ├── CleanArchitecture.Web/           # API layer (FastEndpoints, DI)
│   ├── CleanArchitecture.Application/   # CQRS handlers, validators, behaviors
│   ├── CleanArchitecture.Infrastructure/# EF Core, Identity, repositories
│   ├── CleanArchitecture.Domain/        # Entities, aggregates, events, specs
│   ├── CleanArchitecture.ServiceDefaults/# Aspire service defaults
│   └── CleanArchitecture.Shared/        # Cross-cutting utilities
├── tests/
│   ├── CleanArchitecture.UnitTests/     # Domain unit tests (xUnit + Shouldly)
│   └── CleanArchitecture.IntegrationTests/ # API integration tests
└── .template.config/                    # dotnet new template config
```

## Entry Points

| Entry Point | Path | Description |
|------------|------|-------------|
| Web API | `src/CleanArchitecture.Web/Program.cs` | Main web application startup |
| Aspire Host | `src/CleanArchitecture.AppHost/Program.cs` | Orchestrates web + postgres |
| Template | `.template.config/template.json` | `dotnet new remo-clean-arch` |

## Core Layers

### Domain (`CleanArchitecture.Domain`)
- **Base Classes**: `BaseEntity`, `BaseAuditableEntity`, `BaseEvent`
- **Interfaces**: `IAggregateRoot`, `ISoftDeletion`, `IOwnerId`, `IRepository<T>`, `IReadRepository<T>`
- **TodoList Aggregate**:
  - `TodoList.cs` - Aggregate root with items collection
  - `TodoItem.cs` - Entity with priority, done status, reminder
  - `Colour.cs` - Value object for list color
  - `PriorityLevel.cs` - Enum (None, Low, Medium, High)
- **Domain Events**: `TodoItemCreatedEvent`, `TodoItemCompletedEvent`, `TodoItemDeletedEvent`
- **Specifications**: `GetUserTodoListsSpec`, `TodoListGetByIdSpec`, `GetTodoItemsByListIdSpec`

### Application (`CleanArchitecture.Application`)
- **CQRS with Mediator**: Commands and queries per feature
- **Pipeline Behaviors**: Logging, Authorization, Validation, Performance, UnhandledException
- **Features**:
  - `TodoLists/Commands/`: CreateTodoList, UpdateTodoList, DeleteTodoList, PurgeTodoLists
  - `TodoLists/Queries/`: GetTodos
  - `TodoItems/Commands/`: CreateTodoItem, UpdateTodoItem, UpdateTodoItemDetail, DeleteTodoItem
  - `TodoItems/Queries/`: GetTodoItemsWithPagination
- **Validators**: FluentValidation per command/query
- **Mappings**: Mapster for DTO projections

### Infrastructure (`CleanArchitecture.Infrastructure`)
- **Database**: `AppDbContext` (EF Core + Identity)
- **Repositories**: `EfRepository<T>`, `EfReadRepository<T>` (Ardalis.Specification)
- **Identity**: `ApplicationUser`, `IdentityService`
- **Interceptors**: `AuditableEntityInterceptor`, `DispatchDomainEventsInterceptor`
- **Configuration**: Entity configs in `Data/Config/`

### Web (`CleanArchitecture.Web`)
- **Framework**: FastEndpoints for minimal API style
- **Endpoints**: Versioned API at `/api/v1/`
  - `TodoLists/`: Create, Get, Update, Delete
  - `TodoItems/`: GetWithPagination
- **Groups**: `TodoListGroup`, `TodoItemGroup`
- **Exception Handler**: `CustomExceptionHandler` (ProblemDetails)
- **Swagger**: Available at `/api`

## API Endpoints

| Method | Route | Handler |
|--------|-------|---------|
| GET | `/api/v1/TodoLists` | GetTodos query |
| POST | `/api/v1/TodoLists` | CreateTodoList command |
| PUT | `/api/v1/TodoLists/{id}` | UpdateTodoList command |
| DELETE | `/api/v1/TodoLists/{id}` | DeleteTodoList command |
| GET | `/api/v1/TodoItems` | GetTodoItemsWithPagination query |

## Key Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| FastEndpoints | 7.1.1 | Minimal API endpoints |
| Mediator | 3.0.1 | Source-generated CQRS |
| FluentValidation | 12.1.1 | Request validation |
| Mapster | 7.4.0 | Object mapping |
| Ardalis.Specification | 9.3.1 | Repository pattern |
| Ardalis.GuardClauses | 5.0.0 | Guard clauses |
| Ardalis.Result | 10.1.0 | Result pattern |
| EF Core + Npgsql | 10.0.0 | PostgreSQL persistence |
| ASP.NET Identity | 10.0.0 | Authentication/Authorization |
| Aspire.Npgsql.EFCore | 13.0.2 | Cloud-native orchestration |

## Test Structure

### Unit Tests (`CleanArchitecture.UnitTests`)
- Framework: xUnit + Shouldly
- Coverage: Domain entities and aggregates
- Files: `ColourTests.cs`, `TodoItemTests.cs`, `TodoListTests.cs`

### Integration Tests (`CleanArchitecture.IntegrationTests`)
- Framework: xUnit + FluentAssertions
- `CustomWebApplicationFactory` - Test server setup
- `TodoListApiTests` - API endpoint tests (pending DB setup)

## Quick Start

```bash
# Install template
dotnet new install .

# Create new project
dotnet new remo-clean-arch -n MyApp

# Run with Aspire (includes PostgreSQL)
cd MyApp
dotnet run --project src/MyApp.AppHost

# Run tests
dotnet test

# Run API only (requires DB connection string)
dotnet run --project src/MyApp.Web
```

## Configuration

| File | Purpose |
|------|---------|
| `appsettings.json` | Production config |
| `appsettings.Development.json` | Dev config with logging |
| `launchSettings.json` | Debug profiles |

**Connection String Key**: `ConnectionStrings:CleanArchitectureDb`

## Architecture Patterns

- **Clean Architecture**: Domain-centric with dependency inversion
- **CQRS**: Separate command/query handlers via Mediator
- **Repository Pattern**: Ardalis.Specification with EF Core
- **Domain Events**: Dispatched via EF interceptor
- **Aggregate Root**: TodoList owns TodoItems
- **Value Objects**: Colour
- **Specification Pattern**: Query encapsulation

## Constants

- `LengthConstants`: String length limits
- `Roles`: Administrator
- `Policies`: CanPurge

## File Count Summary

| Category | Count |
|----------|-------|
| Source Projects | 7 |
| Test Projects | 2 |
| Domain Entities | 2 |
| Commands | 7 |
| Queries | 2 |
| Endpoints | 5 |

## CI/CD

| Workflow | Purpose |
|----------|---------|
| `.github/workflows/dotnet.yml` | Build and test on push/PR to master |
| `.github/workflows/codeql.yml` | Security analysis |
