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

Swagger UI is available in Development mode at the default launch URL.

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
- Application handler tests mock repositories and domain services via Moq. Pattern: `Mock<IRepository>` fields initialized in-line as class members, handler constructed in the test class constructor.
- Test method naming: `MethodUnderTest_Scenario_ExpectedBehavior` (e.g. `Create_WithEmptyOrNullParts_ThrowsInvalidAddressException`).
- Assertions use FluentAssertions: `.Should().Be(...)`, `.Should().BeTrue()`, `.Should().ThrowAsync<Exception>()`.
- Tests reference `SportAcademy.Application` and `SportAcademy.Domain` — never `Infrastructure` or `Web`.

## Database & Migrations

EF Core 9 with SQL Server. Connection string: `SportAcademy.Web/appsettings.json` → `ConnectionStrings:DefaultConnection`. Startup project is `SportAcademy.Web`, migrations project is `SportAcademy.Infrastructure`.

```powershell
# Add a new migration
dotnet ef migrations add <MigrationName> --project SportAcademy.Infrastructure --startup-project SportAcademy.Web

# Update database
dotnet ef database update --project SportAcademy.Infrastructure --startup-project SportAcademy.Web
```

In **Production** mode, the app auto-migrates on startup (`db.Database.Migrate()` in `Program.cs`) and exposes a `/health` endpoint. In **Development** mode, it does not auto-migrate — run `dotnet ef database update` manually.

**Seeding:** On every startup (both modes), two seeders run in order:
1. `DatabaseInitializer` → `AspUsersSeeder.SeedUsersAsync()` — creates 100 Identity users via `UserManager<AppUser>`. Skips if ≥100 users exist.
2. `DatabaseSeeder.SeedDatabase()` — seeds domain data using Bogus (branches, sports, subscription types, sport-branch links, sport prices, employees, coaches, trainees, trainee groups, group schedules, payments, subscription details, enrollments). Skips if any branches exist. Employees and trainees are linked to the pre-seeded `AppUser` records.

## Configuration Keys

Required keys in `appsettings.json`:
- `ConnectionStrings:DefaultConnection` — SQL Server connection string
- `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpireMinutes` — JWT authentication config
- `OpenAiSettings:ApiKey` — API key for the OpenAI chatbot integration (used by `OpenAiChatClient`)

## Architecture

This is a .NET 8 Clean Architecture solution (targets `net8.0`, EF Core 9) with five projects:

**Dependency flow:** `Web → Application → Domain` and `Web → Infrastructure → Application/Domain`. Tests reference `Application` and `Domain` only.

### SportAcademy.Domain
Pure domain layer with zero infrastructure dependencies.

**Entity hierarchy:**
- `Person` (abstract base) → implements `IAuditableEntity` + `ISoftDeletable`. Contains: FirstName, LastName, SSN, Email (value object), BirthDate, Gender, Nationality, Address (value object), PhoneNumber, SecondPhoneNumber, and all audit/soft-delete fields.
- `Trainee : Person` — PK is `int Id` (business-generated code, not auto-increment — see `TraineeService.CreateTraineeCode()`). Has: JoinDate, IsSubscribed, ParentNumber/GuardianName (required for minors <15). Nav props → AppUser?, Sports, Enrollments, SubscriptionDetails.
- `Employee : Person` — Has: Salary, HireDate, Position (enum), IsWork (active flag, default true), BranchId, AppUserId (required). Nav props → AppUser, Branch, Coach.
- `Coach` — NOT a Person subclass. Implements `ISoftDeletable`. Composite key or FK-based: EmployeeId + SportId, with SkillLevel and Rate. Nav props → Employee, Sport, TraineeGroups.
- `AppUser : IdentityUser` — Implements `IAuditableEntity` + `ISoftDeletable`. Has IsBanned flag. Nav props → Employee, Trainee, Profile, Notifications.

**Core domain model (non-Person entities):**
- `Branch` — Id, Name, City, Country, PhoneNumber, Email, CoX/CoY (coordinates as strings), IsActive. Central entity linking employees, sports, groups, payments.
- `Sport` — Id, Name, Description, Category (enum: Team/Individual), IsRequireHealthTest. Links to coaches, subscription types, branches, trainees via join tables.
- `TraineeGroup` — Represents a training class. Has SkillLevel, MaximumCapacity (default 15), DurationInMinutes (default 55), Gender, BranchId, CoachId. Nav props → Enrollments, GroupSchedules.
- `GroupSchedule` — Recurring weekly slot: TraineeGroupId, Day (DayOfWeek), StartTime (TimeOnly). Links to SessionOccurrences.
- `SessionOccurrence` — A specific instance of a scheduled session. Has GroupScheduleId, StartDateTime, Status (SessionStatus enum). Implements `IAuditableEntity`.
- `Enrollment` — Links Trainee ↔ TraineeGroup ↔ SubscriptionDetails. Tracks: EnrollmentDate, ExpiryDate, SessionAllowed, SessionRemaining, IsActive. Implements `IAuditableEntity` + `ISoftDeletable`.
- `Attendance` — Tracks per-session attendance. Has EnrollmentId, SessionOccurrenceId, AttendanceStatus (enum), CheckInTime, CoachNote. Implements `IAuditableEntity`.
- `SubscriptionDetails` — Tracks a trainee's subscription period. Has StartDate/EndDate (DateOnly), IsActive, PaymentNumber (FK to Payment), TraineeId, SubscriptionTypeId, SportId, BranchId. Implements `IAuditableEntity` + `ISoftDeletable`. Linked to `SportPrice` for pricing.
- `Payment` — PK is `PaymentNumber` (string, format `PAY-YYYY-NNNNN`). Has Method (Cash/Online), PaidDate, BranchId. One-to-one with SubscriptionDetails.
- `SubscriptionType` — Name (SubType enum: Basic/Silver/Gold/Platinum/Monthly), DaysPerMonth, IsActive, IsOffer.
- `SportPrice` — Composite key: SportId + BranchId + SubsTypeId. Stores Price per sport/branch/subscription-type combination.
- `SportBranch` — Many-to-many join: SportId + BranchId.
- `SportTrainee` — Many-to-many join with data: SportId + TraineeId + SkillLevel.
- `SportSubscriptionType` — Many-to-many join: SportId + SubscriptionTypeId.

**Notification system:**
- `Notification` — Id, Message, CreatedAt, GroupName. Has Recipients collection.
- `NotificationRecipient` — Join table linking Notification to AppUser.

**ChatBot entities:**
- `ChatConversation` — Id (Guid), Title, CreatedAt. Has Messages collection.
- `OpenAiMessage` (file: `ChatMessage.cs`) — Id (Guid), ChatConversationId, Role (ChatRole enum), Content, CreatedAt.

**Value Objects** — immutable, created via static factory methods that validate:
- `Address.Create(street, city)` — trims whitespace, throws `InvalidAddressException` on empty/null.
- `Email.Create(value)` — validates format, throws on invalid.
- `Coordinates` — stores CoX/CoY.

**Domain Services:**
- `PersonService` — `CalculateAge(DateOnly)`, `GenerateUserName(first, last)`, `GeneratePassword()`, `IsSSNValid(ssn, birthDate)` (delegates to `PersonValidationHelper`).
- `TraineeService` — `CreateTraineeCode(trainee, branchId)` (generates business ID from branchId + DOB + ASCII of first letter + counter), `IsAdult(birthDate)` (≥15 years), `CalculateAge()`, `IsSSNValid()`.
- `SubscriptionDetailsService` — static `HasActiveSubscriptionConflict()` checks for overlapping active subscriptions.
- `SoftDeleteService` — helper for soft-delete operations.

**Custom Exceptions** organized by entity folder under `Exceptions/`:
- `BaseExceptions/` — generic: `IdNotFoundException(entityName, id)`
- `SharedExceptions/` — cross-entity: `ConflictException`, `SSNNotUniqueException`, `PhoneNumberNotUniqueException`, `InvalidAddressException`, `InvalidPriceException`, `InvalidCategoryException`, `InvalidSkillLevelException`, `InvalidSortChoiceException`, `SSNSyntaxErrorException`
- Entity-specific folders: `TraineeExceptions/`, `EmployeeExceptions/`, `BranchExceptions/`, `SportExceptions/`, `EnrollmentExceptions/`, `SubscriptonExceptions/`, `PaymentExceptions/`, `UserExceptions/`, `AttendanceExceptions/`, `ChatBotExceptions/`, `SessionOccurrenceExceptions/`, `GeneralExceptions/`
- All exceptions extend `Exception` directly with templated messages.

**Enums** in `Domain/Enums/`: Gender, Nationality, Position, SkillLevel, SportCategory, SportName, SubType, AttendanceStatus, SessionStatus, PaymentMethod, ChatRole, EntityTypes, OperationType, Month.

### SportAcademy.Application (CQRS + MediatR)

**Commands/Queries** follow a strict folder convention:
- Commands: `Commands/{Entity}Commands/{Operation}/{OperationCommand.cs + OperationCommandHandler.cs}`
- Queries: `Queries/{Entity}Queries/{Operation}/{OperationQuery.cs + OperationQueryHandler.cs}`

Current entity command groups: Attendance, Auth (Login/Register), Branch, Chat (AddMessage, CreateConversation, SendMessageToBot), Coach, Employee, Enrollment, SessionOccurrence, Sport, SportPrice, SportTrainee, SubscriptionDetails, Trainee, TraineeGroup, User.

Current entity query groups: Attendance (GetAll, GetById, GetAttendanceRate, GetGlobalAttendanceRate), Branch (GetAll, GetById, GetBranchesCount), Chat (GetConversationById), Employee (GetAll, GetById, GetActiveEmployees, GetActiveCoaches, GetActiveEmployeesCount, GetActiveCoachesCount, GetEmployeesCount, GetCoachEmployeesWithoutCoachRecord, SearchEmployees), Enrollment (GetAll, GetById, GetAllEnrollmentsForSport, GetAllEnrollmentsForAllSports, GetEnrollmentsCountForSport, GetEnrollmentsCountForSports), SessionOccurrence, Sport (GetAll, GetById, GetAvailableSportsForBranch), SportPrice, SportTrainee, SubscriptionDetails, Trainee (GetAll, GetById, GetAllTraineesOfSpecificDay, GetTraineesCountOfSpecificDay), TraineeGroup (GetAll, GetById, GetAllOfSpecificDay), User (GetAll, GetById, GetUnlinkedUsers).

**Result pattern:**
- `ResultBase` — abstract: IsSuccess, OperationType (string), Message, StatusCode (int).
- `Result<T> : ResultBase` — has `Data` property. Factory methods: `Result<T>.Success(data, operationName, message?)`, `Result<T>.Failure(operation, message, statusCode)`.
- `Result : ResultBase` — for void operations. Only has `Failure()` factory.
- Success message template defaults to `"{Operation} operation done successfully"` with automatic `{Operation}` replacement.

**MediatR Pipeline Behaviors** (registered in this order in `Program.cs`):
1. `ExceptionHandlingBehavior<TRequest, TResponse>` — `where TResponse : ResultBase`. Catches all exceptions, uses reflection to call `Result<T>.Failure()` on the generic response type. Logs via `ILogger`.
2. `ValidationBehavior<TRequest, TResponse>` — collects all `IValidator<TRequest>` from DI, runs validation, throws `ValidationException` if any fail.
3. `PaginationNormalizationBehavior<TRequest, TResponse>` — `where TRequest : IPaginatedRequest`. Re-normalizes `request.Page` by calling `PageRequest.Create()` again.

**Pagination system:**
- `IPaginatedRequest` — interface with mutable `PageRequest Page { get; set; }`.
- `PageRequest` — immutable after creation. `Create(int? page, int? size)` normalizes: page defaults to 1 (min 1), size defaults to 10 (min 1, max 100). Exposes `Skip` computed property.
- `PagedData<T>` — response DTO: `Items (IReadOnlyCollection<T>)`, `TotalCount`, `Page`, `PageSize`.
- Queries implement `IPaginatedRequest` and accept `PageRequest Page` as a constructor parameter with `{ get; set; }` override.

**Search system:**
- `ISearchRequest` — interface with `string Term { get; }` property.
- Search queries implement both `IPaginatedRequest` and `ISearchRequest` (e.g. `SearchEmployeeQuery`).
- Search validation is done in the handler itself (min 2 chars, non-empty term).
- `SearchValidationBehavior` exists as a placeholder but is currently commented out.

**Validators:** FluentValidation. Custom extensions in `ValidatorExtensions.cs`:
- `ApplyIdRuleFor<T>(entityName)` — CascadeMode.Stop → NotEmpty → GreaterThan(0).
- `ApplyOptionalIdRuleFor<T>(entityName)` — same but only when value is non-null.
- One validator per command, in `Validators/{Entity}Validators/`.

**Mappings:** AutoMapper. One profile class per entity in `Mappings/{Entity}Profile/`. Profiles map Entity ↔ Command and Entity ↔ DTO. Registered via `AddAutoMapper(typeof(TraineeProfile).Assembly)` to auto-discover all profiles.

**Repository interfaces** (in `Application/Interfaces/`):
- `IBaseRepository<TEntity, TKey>` — `GetByIdAsync`, `GetByIdsAsync(params TKey[])`, `GetAllAsync()`, `GetAllAsync<TDto>(PageRequest)`, `AddAsync`, `UpdateAsync`, `DeleteAsync(TKey)`, `DeleteAsync(TEntity)`, `IsExistAsync`.
- `IPersonRepository` — shared by Trainee/Employee repos: `IsPhoneNumberExistAsync(phone, excludedId)`, `IsSSNExistAsync(ssn)`.
- Entity-specific repos extend `IBaseRepository` and `IPersonRepository` where applicable, adding custom methods (e.g. `ITraineeRepository.GetFullTrainee()`, `IEmployeeRepository.SearchAsync()`, `IEmployeeRepository.GetEmployeesCountAsync()`).

**Application Services:**
- `ChatBotService` — orchestrates chatbot conversations: loads message history from `IChatMessageRepository`, sends to `IOpenAiChatClient`, returns AI response string.
- `SubDetailsManagementService` — validates payment existence/uniqueness and checks for active subscription conflicts before creating SubscriptionDetails.
- `OperationExecutor` — (legacy/alternative) wraps operations in try-catch with exception-to-status-code mapping. Used for non-MediatR flows.

### SportAcademy.Infrastructure

**BaseRepository<TEntity, TKey>** — generic implementation. Injected with `ApplicationDbContext` and optional `IMapper`. Uses `_context.Set<TEntity>()` for all operations. `GetAllAsync<TDto>(PageRequest)` uses AutoMapper's `ProjectTo<TDto>()` + `ToPagedDataAsync()`. Delete by ID throws `IdNotFoundException` if entity not found. All mutations call `SaveChangesAsync()`.

**Entity-specific repositories** extend `BaseRepository` and add custom query methods. Pattern: inject `ApplicationDbContext` + `IMapper` and call `base(context, mapper)`.

**Search implementation** (`EmployeeRepository.SearchAsync()`): Uses **Dapper** + SQL Server **Full-Text Search** (`CONTAINSTABLE`) for employee search. `BuildFullTextTerm()` tokenizes the search term and joins with `AND`, wrapping each token as `"token*"` for prefix matching. Returns paginated results ranked by full-text relevance.

**PaginationExtensions** (`Infrastructure/Persistence/Extensions/QueryExtensions/`):
- `ToPagedDataAsync<T>(IQueryable, PageRequest)` — executes `CountAsync` + Skip/Take on `IQueryable`, returns `PagedData<T>`.
- `ToPagedData<T>(IEnumerable, PageRequest)` — in-memory version for already-materialized collections (e.g. Dapper results).
- `ToGroupedPagedDataAsync(...)` — paginate first, then group in-memory. Used for grouped query responses.

**ApplicationDbContext** extends `IdentityDbContext<AppUser>`. In `OnModelCreating`:
- Applies all `IEntityTypeConfiguration` from the assembly via `ApplyConfigurationsFromAssembly`.
- Dynamically iterates all entity types to configure:
  - `IAuditableEntity` types → CreatedAt (required), CreatedBy, UpdatedAt, UpdatedBy.
  - `ISoftDeletable` types → IsDeleted (default false, required) + global query filter `WHERE IsDeleted = false`, DeletedAt, DeletedBy.
  - `Person` subtypes → column constraints: FirstName/LastName (max 50), SSN (max 14), Gender/Nationality stored as string conversions, PhoneNumber (max 12).
- Entity configurations in `Persistence/Configurations/`, one file per entity.

**EF Interceptors** (both registered as `AddScoped` in DI, attached to DbContext via `AddInterceptors`):
- `AuditingInterceptor` — On `SavingChanges`/`SavingChangesAsync`: finds all `IAuditableEntity` entries in Added/Modified state, sets CreatedAt/CreatedBy (for Added) or UpdatedAt/UpdatedBy (for Modified) using `IUserContextService.UserId` (falls back to `"Admin"`).
- `SoftDeleteInterceptor` — On `SavingChanges`/`SavingChangesAsync`: finds all Deleted entries that implement `ISoftDeletable`, changes state to Modified, sets `IsDeleted=true`, `DeletedAt=UtcNow`, `DeletedBy` from user context.

**External integrations:**
- `OpenAiChatClient` — calls OpenAI Chat Completions API (`gpt-4o-mini` model) via `HttpClient`. Reads `OpenAiSettings:ApiKey` from config. Registered via `AddHttpClient<IOpenAiChatClient, OpenAiChatClient>()`.
- `JwtTokenService` — generates JWT tokens for auth.
- `NotificationService` — sends SignalR notifications (broadcast, per-user, per-group) via `IHubContext<NotificationHub, INotificationClient>`. Persists notifications to DB via `INotificationRepository`.
- `NotificationHub` — SignalR hub mapped to `/hubs/notification`.

### SportAcademy.Web

**Controllers** — thin, `[Authorize]`, `[ApiController]`, `[Route("api/[controller]")]`. Inject `IMediator`, send command/query, return `Ok(result)`. Exception: `AuthController` has no `[Authorize]`.

Controllers: Auth, Attendance, Branch, ChatBot, Coach, Employee, Enrollment, SessionOccurrence, Sport, SportPrice, SportTrainee, SubscriptionDetails, Trainee, TraineeGroup, User.

**Pagination** — controllers accept `[FromQuery] int? page, [FromQuery] int? pageSize`, wrap in `PageRequest.Create(page, pageSize)` before sending to query.

**Search endpoints** — e.g. `GET /api/Employee/search?searchTerm=...&page=...&pageSize=...`. Controller wraps params into a search query record.

**Dashboard/count endpoints** — return `Result<int>` for aggregate counts (e.g. `GET /api/Employee/count`, `GET /api/Employee/active/count`, `GET /api/Employee/coaches/active/count`, `GET /api/Branch/count`).

**Auth flow:**
- `POST /api/Auth/sign-up` → `RegisterCommand`
- `POST /api/Auth/login` → `LoginCommand`
- JWT delivered via cookie (`jwt` cookie, read by `OnMessageReceived` event) or Bearer token in Authorization header.
- `UserContextService` extracts `UserId` from `ClaimTypes.NameIdentifier` or `"sub"` claim, `Role` from `ClaimTypes.Role` or `"role"` claim.

**JSON serialization:** enums as camelCase strings via `JsonStringEnumConverter(JsonNamingPolicy.CamelCase)`.

**CORS:** `AllowFrontend` policy permits origins `http(s)://localhost:8080` and `http(s)://localhost:8081` with credentials.

**DI registration** in `Program.cs` — all repositories and services registered as `AddScoped`. MediatR auto-discovers handlers from `CreateTraineeCommand` assembly. FluentValidation auto-discovers validators from `CreateTraineeValidator` assembly. AutoMapper auto-discovers profiles from `TraineeProfile` assembly.

## Conventions for Adding New Features

### New Entity
1. Add entity class in `Domain/Entities/`, inheriting `Person` if applicable, implementing `IAuditableEntity`/`ISoftDeletable` as needed
2. Add EF configuration in `Infrastructure/Persistence/Configurations/`
3. Add `DbSet` in `ApplicationDbContext`
4. Add repository interface in `Application/Interfaces/` extending `IBaseRepository<TEntity, TKey>` (and `IPersonRepository` if it's a Person subtype)
5. Add repository implementation in `Infrastructure/Persistence/Repositories/` extending `BaseRepository<TEntity, TKey>`
6. Register repository in `Program.cs` as `AddScoped<IXRepository, XRepository>()`
7. Add migration: `dotnet ef migrations add <Name> --project SportAcademy.Infrastructure --startup-project SportAcademy.Web`

### New Command
1. Create folder: `Application/Commands/{Entity}Commands/{Operation}/`
2. Add `{Operation}Command.cs` as a `record` implementing `IRequest<Result<T>>`
3. Add `{Operation}CommandHandler.cs` implementing `IRequestHandler<TCommand, Result<T>>`
4. Handler returns `Result<T>.Success(data, nameof(OperationCommand))` on success, or throws domain exceptions (caught by `ExceptionHandlingBehavior`)
5. Add validator in `Validators/{Entity}Validators/` using FluentValidation. Use `ApplyIdRuleFor()` for ID fields.
6. Add AutoMapper mapping if needed in `Mappings/{Entity}Profile/`
7. Add controller action (or new controller) in `Web/Controllers/`

### New Query
1. Create folder: `Application/Queries/{Entity}Queries/{Operation}/`
2. Add `{Operation}Query.cs` as a `record` implementing `IRequest<Result<T>>`
3. Add `{Operation}QueryHandler.cs`
4. For **paginated** queries: implement `IPaginatedRequest`, accept `PageRequest Page` with `{ get; set; }`, return `Result<PagedData<TDto>>`
5. For **search** queries: also implement `ISearchRequest`, validate min 2 chars in handler
6. For **count/aggregate** queries: return `Result<int>` (no pagination needed)

### New Exception
1. Add to the appropriate subfolder under `Domain/Exceptions/{Entity}Exceptions/`
2. Extend `Exception` directly with a message template in the constructor
3. The `ExceptionHandlingBehavior` will automatically catch it and wrap it in `Result.Failure()`

## Branching
- All development happens in feature branches — no direct push to `main`
- PRs must be reviewed. Merge strategy: Squash & Rebase
