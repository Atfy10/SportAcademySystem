# AGENTS.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Build & Run Commands

```powershell
# Build the entire solution
dotnet build SportAcademy.sln

# Run the Web API (startup project)
dotnet run --project SportAcademy.Web/SportAcademy.Web.csproj

# Build a specific project
dotnet build SportAcademy.Application/SportAcademy.Application.csproj
```

## Testing

Tests use **xUnit** with **FluentAssertions** and **Moq**. All tests live in `SportAcademy.Tests/`, mirroring the source structure (`Domain/` and `Application/` subfolders).

```powershell
# Run all tests
dotnet test SportAcademy.Tests/SportAcademy.Tests.csproj

# Run a single test class
dotnet test SportAcademy.Tests/SportAcademy.Tests.csproj --filter "FullyQualifiedName~CreateTraineeCommandHandlerTests"

# Run a single test method
dotnet test SportAcademy.Tests/SportAcademy.Tests.csproj --filter "FullyQualifiedName~Handle_ValidAdultTrainee_ReturnsSuccessResult"
```

**Test conventions:**
- Domain tests (value objects, services) instantiate real classes directly — no mocking.
- Application handler tests mock repositories and services via Moq, using the pattern: `Mock<IRepository>` fields initialized in-line, handler constructed in the test class constructor.
- Test method naming: `MethodUnderTest_Scenario_ExpectedBehavior` (e.g. `Create_WithEmptyOrNullParts_ThrowsInvalidAddressException`).
- Assertions use FluentAssertions: `.Should().Be(...)`, `.Should().BeTrue()`, `.Should().ThrowAsync<Exception>()`.

## Database & Migrations

The project uses EF Core with SQL Server. Connection string is in `SportAcademy.Web/appsettings.json` under `ConnectionStrings:DefaultConnection`. The startup project is `SportAcademy.Web` and the migrations project is `SportAcademy.Infrastructure`.

```powershell
# Add a new migration
dotnet ef migrations add <MigrationName> --project SportAcademy.Infrastructure --startup-project SportAcademy.Web

# Update database
dotnet ef database update --project SportAcademy.Infrastructure --startup-project SportAcademy.Web
```

The app auto-migrates on startup (`db.Database.Migrate()` in `Program.cs`). Seeders also run automatically: `DatabaseInitializer` seeds Identity users, then `DatabaseSeeder` seeds domain data (branches, sports, trainees, etc.) using Bogus.

## Architecture

This is a .NET 8 Clean Architecture solution with four projects:

**Dependency flow:** `Web → Application → Domain` and `Web → Infrastructure → Application/Domain`

### SportAcademy.Domain
Pure domain layer with no infrastructure dependencies.
- **Entities** inherit from `Person` (base for Trainee/Employee) which implements `IAuditableEntity` and `ISoftDeletable`
- **Value Objects**: `Address`, `Email`, `Coordinates` — created via static factory methods (e.g. `Address.Create(...)`)
- **Domain Services**: `PersonService`, `TraineeService`, `SoftDeleteService`, `SubscriptionDetailsService`
- **Custom Exceptions** organized by entity: `BaseExceptions/` (generic like `IdNotFoundException`), `SharedExceptions/` (cross-entity like `ConflictException`, `SSNNotUniqueException`), and entity-specific folders. All extend `Exception` directly with templated messages.

### SportAcademy.Application (CQRS + MediatR)
- **Commands/Queries** follow a strict folder convention: `Commands/{Entity}Commands/{Operation}/{OperationCommand.cs + OperationCommandHandler.cs}`. Queries follow the same pattern under `Queries/`.
- All handlers return `Result<T>` (wraps data + success status + operation type + message + status code). The non-generic `Result` is for void operations.
- **MediatR Pipeline Behaviors** (registered in order):
  1. `ExceptionHandlingBehavior` — catches all exceptions, returns `Result.Failure(...)` with the error
  2. `ValidationBehavior` — runs FluentValidation validators, throws `ValidationException` on failure
  3. `PaginationNormalizationBehavior` — normalizes page/pageSize on any request implementing `IPaginatedRequest`
- **Pagination**: Queries implement `IPaginatedRequest` with a `PageRequest Page` property. `PageRequest.Create(page, size)` normalizes inputs (defaults: page=1, size=10, max=100). Handlers return `Result<PagedData<TDto>>`.
- **Validators** use FluentValidation. Custom extension `ApplyIdRuleFor()` in `ValidatorExtensions.cs` for ID validation rules. One validator class per command, placed in `Validators/{Entity}Validators/`.
- **Mappings** use AutoMapper. One profile class per entity in `Mappings/{Entity}Profile/`. Profiles map between Entity ↔ Command and Entity ↔ DTO.
- **Repository interfaces** defined here (`IBaseRepository<TEntity, TKey>` and entity-specific repos). `IBaseRepository` provides: `GetByIdAsync`, `GetAllAsync` (with pagination overload), `AddAsync`, `UpdateAsync`, `DeleteAsync`, `IsExistAsync`.

### SportAcademy.Infrastructure
- **BaseRepository** implements `IBaseRepository<TEntity, TKey>`, uses `ApplicationDbContext` and AutoMapper's `ProjectTo<TDto>()` for paginated queries.
- **PaginationExtensions** provides `ToPagedDataAsync()` extension on `IQueryable<T>`.
- **ApplicationDbContext** extends `IdentityDbContext<AppUser>`. In `OnModelCreating`, it dynamically configures `IAuditableEntity` (CreatedAt/By, UpdatedAt/By), `ISoftDeletable` (IsDeleted with global query filter), and `Person` base properties via reflection over the model.
- **EF Interceptors**: `AuditingInterceptor` auto-sets audit fields on Add/Modify. `SoftDeleteInterceptor` converts Delete to Update (sets IsDeleted=true). Both use `IUserContextService` for the current user.
- Entity configurations use Fluent API in `Persistence/Configurations/`, one per entity.

### SportAcademy.Web
- Controllers use `[Authorize]`, `[ApiController]`, `[Route("api/[controller]")]`. They are thin: inject `IMediator`, send commands/queries, return `Ok(result)`.
- Pagination parameters come via `[FromQuery] int? page, [FromQuery] int? pageSize`, passed to `PageRequest.Create(page, pageSize)`.
- JWT auth via cookie (`jwt` cookie) or bearer token. `UserContextService` extracts user info from claims.
- SignalR hub at `/hubs/notification`.
- Enums are serialized as camelCase strings via `JsonStringEnumConverter`.
- CORS configured for `localhost:8080` and `localhost:8081`.

## Conventions for Adding New Features

### New Entity
1. Add entity class in `Domain/Entities/`, inheriting `Person` if applicable, implementing `IAuditableEntity`/`ISoftDeletable` as needed
2. Add EF configuration in `Infrastructure/Persistence/Configurations/`
3. Add `DbSet` in `ApplicationDbContext`
4. Add repository interface in `Application/Interfaces/` extending `IBaseRepository<TEntity, TKey>`
5. Add repository implementation in `Infrastructure/Persistence/Repositories/`
6. Register repository in `Program.cs` as `AddScoped`

### New Command
1. Create folder: `Application/Commands/{Entity}Commands/{Operation}/`
2. Add `{Operation}Command.cs` as a `record` implementing `IRequest<Result<T>>`
3. Add `{Operation}CommandHandler.cs` implementing `IRequestHandler<TCommand, Result<T>>`
4. Handler should set `_operationType` from `OperationType` enum and return via `Result<T>.Success(data, _operationType)`
5. Add validator in `Validators/{Entity}Validators/`
6. Add AutoMapper mapping if needed in `Mappings/{Entity}Profile/`

### New Paginated Query
1. Create `record` implementing both `IRequest<Result<PagedData<TDto>>>` and `IPaginatedRequest`
2. Accept `PageRequest Page` as constructor parameter with `{ get; set; }` (mutable for the normalization behavior)
3. Handler calls repository's paginated `GetAllAsync<TDto>(request.Page, ct)`

## Branching
- All development happens in feature branches — no direct push to `main`
- PRs must be reviewed. Merge strategy: Squash & Rebase
