# ScanitechDanmark Coding Standards & Architecture Guidelines

## Overview

This document defines the standard architecture, coding patterns, and policies for all .NET projects at ScanitechDanmark. These guidelines ensure consistency, maintainability, and quality across all repositories.

**Applies to:** Console Applications, Web APIs, WinForms, WPF, Background Services/Workers, Class Libraries

---
## Core Principles
- SOLID & DRY
- Extreme focus on SRP
- One class per file
- Many small methods (entry methods act as orchestration/playlists)
- Async-first design
- **Comprehensive XML documentation on all public types and members**
  - Describe WHAT the code does, WHY it exists, and WHEN to use it
  - Include `<summary>`, `<param>`, `<returns>`, `<exception>` tags
  - Document edge cases, assumptions, and design decisions
- **Comprehensive inline comments explaining WHY, not WHAT**
  - Explain business rules and domain logic
  - Clarify non-obvious decisions and trade-offs
  - Document workarounds and performance optimizations
- Use of modern C# features (records, pattern matching, etc.)
- Use very good and detailed namespaces/folder and subfolder(Also remember SRP in namespaces), class, method, and variable names
- Use Generics where it makes good sence
- Good use of records and structs where it makes good sence

## Copilot Guardrails (Hard Rules)
- No Minimal APIs
- No business logic in controllers
- No DbContext usage outside repositories
- No static helper classes
- No dynamic or reflection unless explicitly required
- **Comprehensive XML documentation required on all public types and members**
- **Inline comments required for all non-trivial logic explaining WHY**
- Use very good and detailed namespaces/folder and subfolder(Also remember SRP in namespaces), class, method, and variable names

## Technology Stack

### Core Technologies

- **.NET 10+** - Use the latest stable .NET version
- **C# 14.0+** - Leverage modern C# language features
- **Nullable Reference Types** - Always enabled for type safety

### Data Access

**Critical Decision Point:**

- **Entity Framework Core** - Use for self-developed databases (full control over schema)
- **Dapper** - Use for 3rd party databases (existing schema, performance-critical queries)

**When to use each:**

```csharp
/// <summary>
/// Application database context for self-developed schema.
/// </summary>
/// <remarks>
/// Design decision: Use EF Core when you own the database schema and can leverage migrations,
/// navigation properties, and change tracking. Provides strong typing and LINQ support.
/// </remarks>
// V EF Core - Self-developed database
public class MyDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }
    
    // We own the schema, use migrations, relationships, etc.
}

/// <summary>
/// Repository for accessing third-party database with existing schema.
/// </summary>
/// <remarks>
/// Design decision: Use Dapper for third-party databases where you cannot control the schema
/// or for performance-critical queries. Provides raw SQL control and minimal overhead.
/// </remarks>
// v Dapper - 3rd party database
public class ThirdPartyRepository : IThirdPartyRepository
{
    /// <summary>
    /// Retrieves a customer from the external database by ID.
    /// </summary>
    /// <param name="id">The customer identifier.</param>
    /// <returns>The customer if found.</returns>
    /// <exception cref="InvalidOperationException">Thrown when customer not found.</exception>
    /// <remarks>
    /// Performance: Uses parameterized query to prevent SQL injection.
    /// Edge case: Throws if customer doesn't exist; use QuerySingleOrDefaultAsync for null returns.
    /// </remarks>
    // Existing schema we don't control
    public async Task<Customer> GetCustomerAsync(int id)
    {
        const string sql = "SELECT * FROM ExternalCustomers WHERE Id = @Id";
        return await connection.QuerySingleAsync<Customer>(sql, new { Id = id });
    }
}
```

**Standard Libraries:**
- **Microsoft.EntityFrameworkCore** - EF Core ORM
- **Microsoft.EntityFrameworkCore.SqlServer** - SQL Server provider
- **Dapper** - Micro-ORM for 3rd party databases
- **Microsoft.Data.SqlClient** - SQL Server connectivity
- **NoSQL** - Choose libraries ad-hoc based on project requirements

### Configuration & DI

- **Microsoft.Extensions.Configuration** - Configuration management
- **Microsoft.Extensions.DependencyInjection** - Built-in DI container
- **Microsoft.Extensions.Options** - Strongly-typed configuration
- **Microsoft.Extensions.Hosting** - Generic host for background services

### Logging

- **Serilog** - Structured logging framework
- **Serilog.Sinks.File** - File-based logging (daily rolling)
- **Serilog.Sinks.Console** - Console output

### Logging Context

- V Use `BeginScope` for correlation IDs
- V Include RequestId / OperationId where applicable


### Web API Specific

- **ASP.NET Core Web API** - Always use Controllers (not Minimal APIs)
- **Swashbuckle.AspNetCore** - Swagger/OpenAPI documentation
- **Microsoft.AspNetCore.Authentication.JwtBearer** - JWT authentication
- **Microsoft.AspNetCore.Mvc.Versioning** - API versioning
- **MediatR** - CQRS pattern and request/response handling
- **RestSharp** - REST API client for external services

### Validation & Mapping

- **FluentValidation** - Fluent validation rules (preferred)
- **Data Annotations** - Simple validation scenarios
- **AutoMapper** - Object-to-object mapping

### Serialization

- **System.Text.Json** - Default JSON serializer
- **Newtonsoft.Json** - Legacy compatibility
- **System.Xml.Serialization** - XML serialization

### Security

- **System.Security.Cryptography.ProtectedData** - DPAPI (on-premise/internal apps)
- **BCrypt.NET** - Password hashing (public-facing apps)

### Caching

- **Microsoft.Extensions.Caching.Memory** - In-memory caching (preferred)

### File Operations

- **CsvHelper** - CSV file parsing
- **ClosedXML** - Excel file operations
- **System.IO.Compression** - ZIP operations

### Email

- **MailKit** - SMTP email sending
- **MimeKit** - Email message construction

### Testing (Optional per project)

- **xUnit** - Unit testing framework
- **Moq** - Mocking framework
- **FluentAssertions** - Assertion library
- **Microsoft.AspNetCore.Mvc.Testing** - API integration testing

### Cancellation Tokens

- V All async public methods must accept `CancellationToken`
- V Tokens must be passed through all layers
- X Do not use `CancellationToken.None` except at app boundaries

### API Error Handling

- V Use global exception handling middleware
- X No try/catch in controllers except for very specific cases

### Transactions

- V Transaction boundaries belong in the service layer
- X Repositories must never manage transactions
- V Use transactions for all interactions where you do more than one Create/Update/Delete

### Date & Time

- V Use `DateTime.UtcNow`
- V Prefer `DateTimeOffset` for persisted timestamps
- X Never use `DateTime.Now`
- Case specific exceptions from this rule might be okay in some circumstances

---

## Complete Rules & Guidelines Reference

### Architecture Rules

**Layer Separation:**
- V Upper layers depend on lower layers
- X Lower layers never depend on upper layers
- V Each layer only references layer directly below
- X No cross-layer references (Presentation → DataAccess)
- V Three-layer architecture mandatory: Presentation / Logic / DataAccess

**Project Structure:**
- V Follow standardized folder structure for MediatR (Commands/Queries/Notifications)
- V Separate configurations into dedicated classes
- V Group by feature, not by technical type

### Data Access Rules

**Technology Selection:**
- V Use EF Core for databases you own and control
- V Use Dapper for 3rd party/legacy databases
- V All I/O operations must be async
- V Return `IReadOnlyList<T>` for collections
- V Comprehensive XML documentation on all public interfaces and methods
- V Inline comments explaining WHY for business logic and data access patterns
- X No direct data access in business logic
- X No DbContext usage outside repositories

### Entity Framework Core Rules

**Entity Design:**
- V Design domain first, entities represent business concepts
- V Use GUID for aggregate roots, integers for high-volume simple entities  
- V Use `IEntityTypeConfiguration<T>` for configuration
- V Keep navigation properties minimal and unidirectional
- V Disable lazy loading, use explicit loading control
- V Create entities through factory methods/constructors
- X No anemic domain models
- X No EF Core attributes on domain entities

**Querying:**
- V Always use `AsNoTracking()` for read-only queries
- V Project to DTOs with `Select()`, never return tracked entities
- V Use explicit `Include()` only when needed
- V Query only required fields with projections
- V Use compiled queries for hot paths
- X No lazy loading everywhere
- X No returning entities from APIs
- X No deep Include chains

**Updates:**
- V Load-Modify-Save pattern for updates
- V Use optimistic concurrency (row versioning) for updates
- V Always load entity, modify through domain methods, then save
- X Never blindly attach/update detached entities

**Deletes:**
- V Prefer soft deletes for business data
- V Use hard deletes for technical/disposable data
- V Implement global query filters for soft deletes

**DbContext:**
- V Keep DbContext scoped with short lifetime
- V Use explicit repositories only when enforcing boundaries
- V Keep DbContext thin - no business logic
- X No business logic in DbContext
- X No singleton lifetime for DbContext

**Migrations:**
- V Treat migrations as production code, review carefully
- V Add indexes intentionally, measure performance
- V Generate SQL scripts for production deployment
- X No ignoring or auto-applying migrations
- X Never use Database.Migrate() at startup

**Layer Compliance:**
- V Keep EF Core in infrastructure layer only
- V Comprehensive XML documentation on all entity properties and methods
- V Inline comments explaining business rules, concurrency strategies, and WHY decisions
- X No generic repositories

### Dependency Injection Rules

**Lifetime Management:**
- V Use Scoped for DbContext and request-scoped services
- V Use Transient for lightweight stateless services
- V Use Singleton for stateless shared services only
- X Never use Singleton for DbContext
- X No service locator pattern

**Registration:**
- V Register interfaces, not concrete types
- V Validate services at startup
- V Use constructor injection
- X No property injection
- X No method injection

### Error Handling Rules

**By Layer:**
- V Repository: Log and re-throw
- V Service: Log and return Result/Option types
- V API: Use global exception handler
- V Convert domain exceptions to appropriate HTTP status codes
- X No try/catch in controllers except for very specific cases
- X Don't swallow exceptions

**Logging:**
- V Use structured logging with context
- V Log entity IDs for troubleshooting
- V Include RequestId/OperationId
- X Don't expose internal details in API responses

### Validation Rules

**Strategy:**
- V Use FluentValidation for complex rules (preferred)
- V Use Data Annotations for simple validation
- V Validate at service layer before business logic
- V Fail fast - return all errors at once
- X No validation in controllers
- X No validation in repositories

### Testing Rules

**Coverage:**
- V xUnit for unit testing framework
- V Moq for mocking dependencies
- V FluentAssertions for readable assertions
- V WebApplicationFactory for integration tests
- V Test business logic independently of infrastructure

**Patterns:**
- V Arrange-Act-Assert pattern
- V One assertion per test (when possible)
- V Use test fixtures for shared setup
- V Name tests descriptively: MethodName_Scenario_ExpectedResult

### Documentation Rules

**XML Documentation:**
- V `<summary>` - Describe WHAT the code does
- V `<param>` - Explain purpose and valid values  
- V `<returns>` - Describe return value and possible states
- V `<exception>` - Document exceptions that can be thrown
- V `<remarks>` - Explain WHY it exists, edge cases, assumptions, design decisions
- V `<example>` - Usage examples for complex APIs (optional)
- V Required on all public types and members

**Inline Comments:**
- V Explain WHY, not WHAT
- V Document business rules and domain logic
- V Clarify non-obvious decisions and trade-offs
- V Document workarounds and performance optimizations
- X Don't restate what the code obviously does
- X Don't leave commented-out code

### Naming Rules

**Conventions:**
- V Use very good and detailed namespaces (SRP applies)
- V PascalCase for: classes, methods, properties, constants
- V camelCase for: parameters, local variables, private fields 
- V Prefix interfaces with `I`
- V Prefix private fields with `_`
- V Use descriptive names that reveal intent
- V Domain language in domain layer
- X No abbreviations except widely known (Id, Url, Dto)
- X No Hungarian notation

### Configuration Rules

**Settings Management:**
- V Use strongly-typed configuration with IOptions
- V Store secrets in environment variables or key vault
- V Use appsettings.json for non-sensitive config
- V Use appsettings.{Environment}.json for environment overrides
- X Never commit secrets to source control
- X No hardcoded connection strings

### Security Rules

**Passwords:**
- V DPAPI for internal/on-premise applications
- V BCrypt for public-facing applications
- X Never store plaintext passwords
- X Never log passwords or secrets

**JWT:**
- V Validate issuer, audience, lifetime, signing key
- V Use strong secret keys (minimum 32 characters)
- V Store JWT secret in configuration, not code
- X Never expose signing key

**CORS:**
- V Explicitly list allowed origins
- X Never use AllowAnyOrigin in production

---

## Architecture Principles

### Three-Layer Architecture

**All projects must follow this pattern:**

```
<ProjectName>_Presentation/                 # Entry Point Layer
├── Program.cs                              # Application entry point
├── Controllers/                            # API controllers (Web API)
├── Forms/                                  # Forms (WinForms/WPF)
├── Commands/                               # CLI commands (Console)
└── appsettings.json                        # Configuration


<ProjectName>_Logic/                        # Business Logic Layer
├── Services/                               # Business services / orchestration
│
├── Handlers/                               # MediatR
│   ├── Commands/                           # State-changing requests
│   │   ├── CreateItem/
│   │   │   ├── CreateItemCommand.cs
│   │   │   ├── CreateItemHandler.cs
│   │   │   └── CreateItemResult.cs
│   │   ├── UpdateItem/
│   │   │   ├── UpdateItemCommand.cs
│   │   │   └── UpdateItemHandler.cs
│   │   └── DeleteItem/
│   │       ├── DeleteItemCommand.cs
│   │       └── DeleteItemHandler.cs
│   │
│   ├── Queries/                            # Read-only requests
│   │   ├── GetItemById/
│   │   │   ├── GetItemByIdQuery.cs
│   │   │   └── GetItemByIdHandler.cs
│   │   └── GetItems/
│   │       ├── GetItemsQuery.cs
│   │       └── GetItemsHandler.cs
│   │
│   ├── Notifications/                     # Domain / integration events
│   │   └── ItemCreated/
│   │       ├── ItemCreatedNotification.cs
│   │       └── ItemCreatedHandler.cs
│   │
│   └── Behaviors/                         # MediatR pipeline behaviors
│       ├── ValidationBehavior.cs
│       ├── LoggingBehavior.cs
│       └── TransactionBehavior.cs
│
├── Validation/                             # FluentValidation validators
│   ├── Commands/
│   │   └── CreateItemCommandValidator.cs
│   └── Queries/
│       └── GetItemByIdQueryValidator.cs
│
├── Models/                                 # Business models & DTOs
│   ├── Requests/
│   ├── Responses/
│   └── Dtos/
│
├── Mapping/                                # AutoMapper profiles
│   └── ItemMappingProfile.cs
│
├── Interfaces/                             # Service interfaces
│
└── Exceptions/                             # Domain / business exceptions
    ├── NotFoundException.cs
    ├── DuplicateEntityException.cs
    └── ValidationException.cs


<ProjectName>_DataAccess/                   # Data Access Layer
├── DbContext/                              # EF Core contexts
├── Repositories/                           # Dapper repositories
├── Entities/                               # Database entities
├── Migrations/                             # EF Core migrations (if applicable)
└── Interfaces/                             # Data access contracts

```

**Layer Rules:**
- V Upper layers depend on lower layers
- X Lower layers never depend on upper layers
- V Each layer only references layer directly below
- X No cross-layer references (Presentation → DataAccess)

---

## Project Type Patterns

### Console Applications

```csharp
/// <summary>
/// Main entry point for the console application.
/// </summary>
public class Program
{
    /// <summary>
    /// Main application entry point.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>Exit code: 0 for success, 1 for failure.</returns>
    /// <remarks>
    /// Sets up dependency injection, configuration, and logging.
    /// Uses async Main for better resource management and modern patterns.
    /// </remarks>
    public static async Task<int> Main(string[] args)
    {
        // Configure logging first - we need it for error handling
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            // Build configuration from multiple sources
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory) // Use app directory for config files
                .AddJsonFile("appsettings.json", optional: false) // Required config
                .AddCommandLine(args) // Command-line args override appsettings
                .Build();

            // Set up dependency injection container
            var services = new ServiceCollection();
            ConfigureServices(services, configuration);
            var serviceProvider = services.BuildServiceProvider();

            // Resolve and run the application
            var app = serviceProvider.GetRequiredService<IApplicationRunner>();
            await app.RunAsync();

            return 0; // Success exit code
        }
        catch (Exception ex)
        {
            // Log fatal errors for troubleshooting
            Log.Fatal(ex, "Application terminated unexpectedly");
            return 1; // Failure exit code
        }
        finally
        {
            // Ensure all log entries are written before exit
            Log.CloseAndFlush();
        }
    }
}
```

### Web API Applications

```csharp
/// <summary>
/// Main program class for Web API application.
/// </summary>
public class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        /// <summary>
        /// Configures structured logging with Serilog.
        /// </summary>
        /// <remarks>
        /// Design decision: Use Serilog for structured logging with file and console sinks.
        /// Reads configuration from appsettings.json for environment-specific log levels.
        /// Daily rolling file prevents unbounded log growth.
        /// </remarks>
        // Logging
        builder.Host.UseSerilog((context, config) =>
            config.ReadFrom.Configuration(context.Configuration)
                  .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day));

        // Services
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();  // Required for minimal APIs and Swagger
        builder.Services.AddSwaggerGen();  // API documentation generation
        builder.Services.AddProblemDetails();  // RFC 7807 compliant error responses

        /// <summary>
        /// Configures JWT Bearer authentication.
        /// </summary>
        /// <remarks>
        /// Security: Validates issuer, audience, lifetime, and signing key.
        /// All parameters must match between token generation and validation.
        /// IssuerSigningKey uses symmetric encryption - keep secret key secure.
        /// </remarks>
        // JWT Authentication
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,  // Prevents tokens from untrusted sources
                    ValidateAudience = true,  // Ensures token is for this API
                    ValidateLifetime = true,  // Rejects expired tokens
                    ValidateIssuerSigningKey = true,  // Prevents token tampering
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });

        /// <summary>
        /// Configures API versioning for backward compatibility.
        /// </summary>
        /// <remarks>
        /// Design decision: Default to v1.0 for unspecified versions to support legacy clients.
        /// ReportApiVersions adds supported versions to response headers for client discovery.
        /// </remarks>
        // API Versioning
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;  // Adds api-supported-versions header
        });

        // MediatR - CQRS pattern for separating commands from queries
        builder.Services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

        /// <summary>
        /// Configures CORS policy for cross-origin requests.
        /// </summary>
        /// <remarks>
        /// Security: Only allow origins specified in configuration to prevent CSRF attacks.
        /// Do NOT use AllowAnyOrigin in production - explicitly list trusted origins.
        /// </remarks>
        // CORS
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>())
                      .AllowAnyMethod()  // Allows GET, POST, PUT, DELETE, etc.
                      .AllowAnyHeader());  // Allows custom headers like Authorization
        });

        // Application Services - scoped lifetime for DbContext compatibility
        builder.Services.AddScoped<IMyService, MyService>();

        /// <summary>
        /// Database configuration based on ownership.
        /// </summary>
        /// <remarks>
        /// Design decision: Use EF Core for self-developed databases with migrations.
        /// Use Dapper for third-party databases where schema is externally controlled.
        /// </remarks>
        // Database - Choose based on ownership
        // Self-developed database:
        builder.Services.AddDbContext<MyDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("MyDatabase")));

        // 3rd party database:
        builder.Services.AddScoped<IThirdPartyRepository, ThirdPartyRepository>();

        // Memory Cache - for frequently accessed, rarely changing data
        builder.Services.AddMemoryCache();

        var app = builder.Build();

        // Global exception handler - converts exceptions to ProblemDetails
        app.UseExceptionHandler();

        // Development-only tools - never expose in production
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();  // JSON API documentation
            app.UseSwaggerUI();  // Interactive API explorer
        }

        // Middleware pipeline - order matters!
        app.UseHttpsRedirection();  // Redirect HTTP to HTTPS
        app.UseSerilogRequestLogging();  // Log all HTTP requests
        app.UseCors();  // Must be before Authentication/Authorization
        app.UseAuthentication();  // Identifies user from token
        app.UseAuthorization();  // Enforces authorization policies
        app.MapControllers();  // Maps attribute-routed controllers
        app.Run();  // Blocks until application shutdown
    }
}
```

**Controller Pattern:**

```csharp
/// <summary>
/// API controller for managing items.
/// </summary>
/// <remarks>
/// Controller is thin and delegates all business logic to MediatR handlers.
/// Version 1.0 API following RESTful conventions.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ItemsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemsController"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR instance for command/query handling.</param>
    /// <param name="logger">The logger instance.</param>
    public ItemsController(IMediator mediator, ILogger<ItemsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all items.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for request cancellation.</param>
    /// <returns>A list of all items.</returns>
    /// <response code="200">Returns the list of items.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ItemDto>), 200)]
    public async Task<ActionResult<IReadOnlyList<ItemDto>>> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllItemsQuery();
        
        // Delegate to handler - controller has no business logic
        var result = await _mediator.Send(query, cancellationToken);
        
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ItemDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ItemDto>> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var query = new GetItemByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ItemDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ItemDto>> Create(
        [FromBody] CreateItemCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
```

**MediatR Pattern:**

```csharp
// Query
public sealed record GetAllItemsQuery : IRequest<IReadOnlyList<ItemDto>>;

public sealed record GetItemByIdQuery(int Id) : IRequest<ItemDto?>;

/// <summary>
/// Handler for retrieving all items.
/// </summary>
/// <remarks>
/// Implements the query side of CQRS pattern using MediatR.
/// Handler is responsible for orchestrating the business logic flow.
/// </remarks>
public sealed class GetAllItemsHandler : IRequestHandler<GetAllItemsQuery, IReadOnlyList<ItemDto>>
{
    private readonly IMyService _service;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllItemsHandler"/> class.
    /// </summary>
    /// <param name="service">The service for item operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when service is null.</exception>
    public GetAllItemsHandler(IMyService service)
    {
        // Fail fast if dependencies are missing - prevents runtime errors
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
    }
    
    /// <summary>
    /// Handles the query to retrieve all items.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A read-only list of item DTOs.</returns>
    public async Task<IReadOnlyList<ItemDto>> Handle(GetAllItemsQuery request, CancellationToken ct)
    {
        // Delegate to service layer which contains the business logic
        return await _service.GetAllAsync(ct);
    }
}
```

### WinForms Applications

```csharp
/// <summary>
/// Entry point for the WinForms application.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Main entry point for the application.
    /// </summary>
    /// <remarks>
    /// STA thread required for COM interop and Windows Forms.
    /// Sets up dependency injection before creating forms.
    /// </remarks>
    [STAThread]
    static void Main()
    {
        // Initialize logging first
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            // Load configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // Configure DI container
            var services = new ServiceCollection();
            ConfigureServices(services, configuration);
            
            // Using ensures proper disposal when app exits
            using var serviceProvider = services.BuildServiceProvider();

            // Enable modern Windows Forms rendering
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            // Resolve main form from DI container
            var mainForm = serviceProvider.GetRequiredService<MainForm>();
            Application.Run(mainForm);
        }
        catch (Exception ex)
        {
            // Log and display fatal errors
            Log.Fatal(ex, "Application terminated unexpectedly");
            MessageBox.Show($"Fatal error: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            // Flush logs before exit
            Log.CloseAndFlush();
        }
    }
}

/// <summary>
/// Main application form with dependency injection support.
/// </summary>
public partial class MainForm : Form
{
    private readonly IMyService _service;
    private readonly ILogger<MainForm> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainForm"/> class.
    /// </summary>
    /// <param name="service">The business service.</param>
    /// <param name="logger">The logger instance.</param>
    public MainForm(IMyService service, ILogger<MainForm> logger)
    {
        _service = service;
        _logger = logger;
        InitializeComponent();
    }

    /// <summary>
    /// Handles the Load button click event.
    /// </summary>
    /// <remarks>
    /// Async void is acceptable for event handlers.
    /// UI state management prevents duplicate operations.
    /// </remarks>
    private async void LoadButton_Click(object sender, EventArgs e)
    {
        try
        {
            // Disable UI to prevent concurrent operations
            LoadButton.Enabled = false;
            Cursor = Cursors.WaitCursor;
            
            // CancellationToken.None is acceptable at UI boundary
            var items = await _service.GetAllAsync(CancellationToken.None);
            
            // ToList() required for DataGridView binding
            dataGridView.DataSource = items.ToList();
        }
        catch (Exception ex)
        {
            // Log for troubleshooting
            _logger.LogError(ex, "Failed to load items");
            
            // Show user-friendly error message
            MessageBox.Show($"Error: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            // Always restore UI state
            LoadButton.Enabled = true;
            Cursor = Cursors.Default;
        }
    }
}
```

### WPF Applications

```csharp
/// <summary>
/// WPF application entry point with dependency injection.
/// </summary>
public partial class App : Application
{
    private IServiceProvider? ServiceProvider { get; set; }

    /// <summary>
    /// Called when the application starts.
    /// </summary>
    /// <param name="e">Startup event arguments.</param>
    /// <remarks>
    /// Sets up logging, configuration, and DI before showing main window.
    /// </remarks>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize logging
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // Configure dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services, configuration);
        ServiceProvider = services.BuildServiceProvider();

        // Resolve and show main window
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    /// <summary>
    /// Called when the application exits.
    /// </summary>
    /// <param name="e">Exit event arguments.</param>
    protected override void OnExit(ExitEventArgs e)
    {
        // Ensure all logs are flushed before exit
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}

/// <summary>
/// View model for the main window.
/// </summary>
/// <remarks>
/// Implements MVVM pattern with data binding and commands.
/// INotifyPropertyChanged enables automatic UI updates.
/// </remarks>
public class MainViewModel : INotifyPropertyChanged
{
    private readonly IMyService _service;
    private ObservableCollection<ItemDto> _items;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    /// <param name="service">The business service.</param>
    public MainViewModel(IMyService service)
    {
        _service = service;
        _items = new ObservableCollection<ItemDto>();
        
        // RelayCommand binds UI actions to async methods
        LoadCommand = new RelayCommand(async () => await LoadItemsAsync());
    }

    /// <summary>
    /// Gets or sets the collection of items.
    /// </summary>
    /// <remarks>
    /// ObservableCollection automatically notifies UI of changes.
    /// </remarks>
    public ObservableCollection<ItemDto> Items
    {
        get => _items;
        set { _items = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Gets the command to load items.
    /// </summary>
    public ICommand LoadCommand { get; }

    /// <summary>
    /// Loads items from the service.
    /// </summary>
    private async Task LoadItemsAsync()
    {
        // CancellationToken.None acceptable at UI boundary
        var items = await _service.GetAllAsync(CancellationToken.None);
        
        // Replace collection to trigger UI update
        Items = new ObservableCollection<ItemDto>(items);
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    
    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    /// <param name="name">The property name. Automatically populated by CallerMemberName.</param>
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

### Background Services/Workers

```csharp
/// <summary>
/// Configures and runs a background service application.
/// </summary>
/// <remarks>
/// Design decision: Use Host.CreateApplicationBuilder for non-HTTP workloads like
/// Windows Services, scheduled jobs, or message processors. Provides DI and configuration
/// without the overhead of web server components.
/// </remarks>
// Host builder setup for background service
var builder = Host.CreateApplicationBuilder(args);

/// <summary>
/// Configures structured logging with Serilog.
/// </summary>
/// <remarks>
/// Design decision: Daily rolling files prevent unbounded log growth.
/// Serilog provides structured logging for better searchability and analysis.
/// </remarks>
// Configure structured logging with Serilog
builder.Services.AddSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration)
          .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day));

// Register background service - implements IHostedService for lifecycle management
builder.Services.AddHostedService<Worker>();

/// <summary>
/// Registers services with Transient lifetime.
/// </summary>
/// <remarks>
/// Design decision: Transient creates new instance per request, suitable for
/// lightweight stateless services. Use Scoped for DbContext-dependent services.
/// </remarks>
// Register transient services (new instance per operation)
builder.Services.AddTransient<IMyService, MyService>();

var host = builder.Build();
host.Run(); // Blocks until service is stopped (Ctrl+C or service stop command)

/// <summary>
/// Background worker service for periodic processing.
/// </summary>
/// <remarks>
/// Runs continuously in the background, processing on a scheduled interval.
/// Gracefully handles cancellation when service is stopped.
/// </remarks>
public class Worker : BackgroundService
{
    private readonly IMyService _service;
    private readonly ILogger<Worker> _logger;
    private readonly int _intervalMinutes;

    /// <summary>
    /// Initializes a new instance of the <see cref="Worker"/> class.
    /// </summary>
    /// <param name="service">The business service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configuration">The configuration for retrieving settings.</param>
    public Worker(IMyService service, ILogger<Worker> logger, 
        IConfiguration configuration)
    {
        _service = service;
        _logger = logger;
        
        // Load interval from configuration
        _intervalMinutes = configuration.GetValue<int>("IntervalMinutes");
    }

    /// <summary>
    /// Executes the background work.
    /// </summary>
    /// <param name="stoppingToken">Token to signal when service should stop.</param>
    /// <returns>A task that represents the background work.</returns>
    /// <remarks>
    /// Runs in a loop until cancellation is requested.
    /// Individual failures are logged but don't stop the service.
    /// </remarks>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Continue until service is stopped
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Process business logic with cancellation support
                await _service.ProcessAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // Log but don't rethrow - keeps worker running
                _logger.LogError(ex, "Worker encountered an error");
            }

            // Wait for next interval (cancellable)
            await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
        }
    }
}
```

---

## Data Access Patterns

**Data Access Rules:**
- V Use EF Core for databases you own and control
- V Use Dapper for 3rd party/legacy databases
-V All I/O operations must be async
- V Return `IReadOnlyList<T>` for collections
- V Comprehensive XML documentation on all public interfaces and methods
- V Inline comments explaining WHY for business logic and data access patterns
- X No direct data access in business logic
- X No DbContext usage outside repositories

### Entity Framework Core (Self-Developed Database)

```csharp
/// <summary>
/// Application database context for order management domain.
/// </summary>
/// <remarks>
/// Design decision: Thin DbContext with configuration moved to IEntityTypeConfiguration classes.
/// This keeps DbContext focused on registering entities while separating persistence concerns.
/// </remarks>
// DbContext
public class MyDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MyDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }

    /// <summary>
    /// Configures the entity model using Fluent API.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model.</param>
    /// <remarks>
    /// Best practice: Use separate IEntityTypeConfiguration classes for complex configurations.
    /// This inline configuration shown for simple scenarios only.
    /// </remarks>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Business rule: Order numbers must be unique per customer
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Orders)
                  .HasForeignKey(e => e.CustomerId);
        });
    }
}

/// <summary>
/// Repository for managing order aggregate persistence.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Retrieves an order by its unique identifier.
    /// </summary>
    /// <param name="id">The order identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The order if found; otherwise, null.</returns>
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken);
    
    /// <summary>
    /// Retrieves all orders.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of all orders.</returns>
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Adds a new order to the repository.
    /// </summary>
    /// <param name="order">The order to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The added order with generated values populated.</returns>
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken);
    
    /// <summary>
    /// Updates an existing order.
    /// </summary>
    /// <param name="order">The order with updated values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(Order order, CancellationToken cancellationToken);
    
    /// <summary>
    /// Deletes an order by its identifier.
    /// </summary>
    /// <param name="id">The order identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}

/// <summary>
/// Entity Framework Core implementation of the order repository.
/// </summary>
/// <remarks>
/// Responsible for data access only - no business logic.
/// All queries include necessary navigation properties for aggregate completeness.
/// </remarks>
public class OrderRepository : IOrderRepository
{
    private readonly MyDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public OrderRepository(MyDbContext context) => _context = context;

    /// <summary>
    /// Retrieves an order by its unique identifier.
    /// </summary>
    /// <param name="id">The order identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The order with related customer data if found; otherwise, null.</returns>
    public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        // Include related entities to load the complete aggregate
        return await _context.Orders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves all orders.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of all orders with customer data.</returns>
    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken)
    {
        // Include ensures related data is loaded in single query (prevents N+1)
        return await _context.Orders
            .Include(o => o.Customer)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Adds a new order to the repository.
    /// </summary>
    /// <param name="order">The order to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The added order with generated ID.</returns>
    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken)
    {
        _context.Orders.Add(order);
        
        // SaveChanges must be called to persist and generate database values
        await _context.SaveChangesAsync(cancellationToken);
        
        return order;
    }

    /// <summary>
    /// Updates an existing order.
    /// </summary>
    /// <param name="order">The order with updated values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task UpdateAsync(Order order, CancellationToken cancellationToken)
    {
        // Mark entity as modified - EF Core will track changes
        _context.Entry(order).State = EntityState.Modified;
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes an order by its identifier.
    /// </summary>
    /// <param name="id">The order identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        // Find with array syntax required for composite keys (single key works too)
        var order = await _context.Orders.FindAsync([id], cancellationToken);
        
        // Null check prevents exception if order doesn't exist
        if (order != null)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
```

### Dapper (3rd Party Database)

```csharp
public interface IThirdPartyRepository
{
    Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken);
    Task<int> InsertAsync(Customer customer, CancellationToken cancellationToken);
}

/// <summary>
/// Dapper-based repository for accessing third-party customer data.
/// </summary>
/// <remarks>
/// Uses raw SQL queries because we don't control the database schema.
/// Connection is opened/closed per operation to avoid connection pool exhaustion.
/// </remarks>
public class ThirdPartyRepository : IThirdPartyRepository
{
    private readonly string _connectionString;
    private readonly ILogger<ThirdPartyRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThirdPartyRepository"/> class.
    /// </summary>
    /// <param name="configuration">The configuration to retrieve connection strings.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration or logger is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when connection string is missing.</exception>
    public ThirdPartyRepository(IConfiguration configuration, ILogger<ThirdPartyRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(logger);

        // Fail fast if connection string is missing - prevents runtime errors later
        _connectionString = configuration.GetConnectionString("ThirdPartyDb")
            ?? throw new InvalidOperationException("Missing connection string: ThirdPartyDb");
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a customer by identifier from the third-party database.
    /// </summary>
    /// <param name="id">The customer identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer if found; otherwise, null.</returns>
    public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        // Create new connection per operation (connection pooling handles efficiency)
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // Use parameterized query to prevent SQL injection
        const string sql = "SELECT * FROM Customers WHERE Id = @Id";
        
        // CommandDefinition enables cancellation token support in Dapper
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Customer>(command);
    }

    /// <summary>
    /// Retrieves all customers from the third-party database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of all customers.</returns>
    /// <remarks>
    /// Performance: Use SELECT * only for small tables or when all columns needed.
    /// Prefer explicit column list for large tables to reduce network traffic.
    /// </remarks>
    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken)
    {
        // await using ensures connection is disposed even if exception occurs
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = "SELECT * FROM Customers";
        // CommandDefinition supports cancellation token propagation
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
        var results = await connection.QueryAsync<Customer>(command);
        return results.ToList();
    }

    /// <summary>
    /// Inserts a new customer and returns the generated ID.
    /// </summary>
    /// <param name="customer">The customer to insert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated customer ID.</returns>
    /// <remarks>
    /// Design decision: SCOPE_IDENTITY() returns the last identity value inserted in the current scope.
    /// Safer than @@IDENTITY which can return IDs from triggers.
    /// </remarks>
    public async Task<int> InsertAsync(Customer customer, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // Dapper automatically maps object properties to SQL parameters - prevents SQL injection
        const string sql = @"
            INSERT INTO Customers (Name, Email) 
            VALUES (@Name, @Email);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        var command = new CommandDefinition(sql, customer, cancellationToken: cancellationToken);
        return await connection.ExecuteScalarAsync<int>(command);
    }
}
```

---

## Entity Framework Core Best Practices

**Entity Framework Core Rules:**

**Entity Design:**
- V Design domain first, entities represent business concepts
- V Use GUID for aggregate roots, integers for high-volume simple entities  
- V Use `IEntityTypeConfiguration<T>` for configuration
- V Keep navigation properties minimal and unidirectional
- V Disable lazy loading, use explicit loading control
- V Create entities through factory methods/constructors
- X No anemic domain models
- X No EF Core attributes on domain entities

**Querying:**
- V Always use `AsNoTracking()` for read-only queries
- V Project to DTOs with `Select()`, never return tracked entities
- V Use explicit `Include()` only when needed
- V Query only required fields with projections
- V Use compiled queries for hot paths
- X No lazy loading everywhere
- X No returning entities from APIs
- X No deep Include chains

**Updates:**
- V Load-Modify-Save pattern for updates
- V Use optimistic concurrency (row versioning) for updates
- V Always load entity, modify through domain methods, then save
- X Never blindly attach/update detached entities

**Deletes:**
- V Prefer soft deletes for business data
- V Use hard deletes for technical/disposable data
- V Implement global query filters for soft deletes

**DbContext:**
- V Keep DbContext scoped with short lifetime
- V Use explicit repositories only when enforcing boundaries
- V Keep DbContext thin - no business logic
- X No business logic in DbContext
- X No singleton lifetime for DbContext

**Migrations:**
- V Treat migrations as production code, review carefully
- V Add indexes intentionally, measure performance
- V Generate SQL scripts for production deployment
- X No ignoring or auto-applying migrations
- X Never use Database.Migrate() at startup

**Layer Compliance:**
- V Keep EF Core in infrastructure layer only
- V Comprehensive XML documentation on all entity properties and methods
- V Inline comments explaining business rules, concurrency strategies, and WHY decisions
- X No generic repositories

### Database & Entity Design

**Design domain first, not database.** Entities represent business concepts, not tables.

```csharp
/// <summary>
/// Represents an order aggregate root in the domain.
/// Encapsulates order state and enforces business rules through behavior.
/// </summary>
/// <remarks>
/// Design decision: GUID primary key chosen for distributed systems scalability.
/// All state modifications go through domain methods to maintain invariants.
/// </remarks>
public sealed class Order
{
    /// <summary>
    /// Gets the unique identifier for this order.
    /// </summary>
    public Guid Id { get; private set; }
    
    /// <summary>
    /// Gets the human-readable order number.
    /// </summary>
    public string OrderNumber { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the current status of the order.
    /// </summary>
    public OrderStatus Status { get; private set; }
    
    /// <summary>
    /// Gets the UTC timestamp when the order was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }
    
    /// <summary>
    /// Gets the UTC timestamp when the order was completed, if applicable.
    /// </summary>
    public DateTime? CompletedAt { get; private set; }
    
    // Private backing field ensures collection can only be modified through domain methods
    private readonly List<OrderLine> _lines = new();
    
    /// <summary>
    /// Gets the read-only collection of order lines.
    /// </summary>
    /// <remarks>
    /// Returns IReadOnlyList to prevent external modification and maintain encapsulation.
    /// </remarks>
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    
    /// <summary>
    /// Factory method to create a new order.
    /// </summary>
    /// <param name="orderNumber">The unique order number.</param>
    /// <returns>A new order in Draft status.</returns>
    /// <exception cref="ArgumentException">Thrown when orderNumber is null or whitespace.</exception>
    /// <remarks>
    /// Factory method pattern enforces that all orders are created in a valid state.
    /// Generates GUID immediately to support distributed scenarios.
    /// </remarks>
    public static Order Create(string orderNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderNumber);
        
        return new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            Status = OrderStatus.Draft, // All orders start as Draft
            CreatedAt = DateTime.UtcNow // Always UTC to avoid timezone issues
        };
    }
    
    /// <summary>
    /// Adds a line item to the order.
    /// </summary>
    /// <param name="productName">The name of the product.</param>
    /// <param name="quantity">The quantity ordered.</param>
    /// <param name="price">The unit price.</param>
    /// <exception cref="InvalidOperationException">Thrown when order is not in Draft status.</exception>
    /// <remarks>
    /// Business rule: Only Draft orders can be modified.
    /// This prevents modification of completed orders which may have been invoiced.
    /// </remarks>
    public void AddLine(string productName, int quantity, decimal price)
    {
        // Enforce business rule: completed orders are immutable
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Cannot modify completed order");
            
        _lines.Add(new OrderLine(productName, quantity, price));
    }
    
    /// <summary>
    /// Marks the order as completed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when order is already completed.</exception>
    /// <remarks>
    /// Sets the completion timestamp for audit and reporting purposes.
    /// Completed orders become immutable.
    /// </remarks>
    public void Complete()
    {
        // Idempotency check - prevent double completion
        if (Status == OrderStatus.Completed)
            throw new InvalidOperationException("Order already completed");
            
        Status = OrderStatus.Completed;
        CompletedAt = DateTime.UtcNow; // Record when the state transition occurred
    }
    
    // Parameterless constructor required by EF Core for entity materialization
    private Order() { }
}

/// <summary>
/// Anti-pattern: Anemic domain model with no behavior or encapsulation.
/// </summary>
/// <remarks>
/// WRONG: This class is a data bag with no business logic or invariant protection.
/// Public setters allow invalid states, foreign keys instead of value objects reduce clarity,
/// and collections lack encapsulation. All business logic ends up in services, creating
/// transaction script anti-pattern. Use rich domain models with factory methods, private
/// setters, and behavior methods that enforce invariants.
/// </remarks>
// X Anemic domain model (avoid)
public class BadOrder
{
    public int Id { get; set; }
    public string OrderNumber { get; set; }  // No validation, can be null or empty
    public int StatusId { get; set; }  // Foreign key instead of enum - loses meaning
    public List<OrderLine> Lines { get; set; } // Public setter breaks encapsulation
}
```

**Key Selection:**

```csharp
/// <summary>
/// Customer aggregate root with GUID primary key.
/// </summary>
/// <remarks>
/// Design decision: Use GUID for aggregate roots to enable:
/// - Client-side ID generation (no database roundtrip)
/// - Distributed system support (no central ID generator)
/// - Merge conflicts avoidance in distributed scenarios
/// Trade-off: Larger index size (16 bytes) vs int (4 bytes), non-sequential clustering
/// </remarks>
// V GUID for aggregate roots (scalability, distribution)
public sealed class Customer
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
}

/// <summary>
/// Log entry entity with integer identity key.
/// </summary>
/// <remarks>
/// Design decision: Use integer for high-volume entities where:
/// - Sequential ordering is beneficial (chronological logs)
/// - Smaller index size matters (millions of rows)
/// - No distributed ID generation needed
/// Performance: Integer keys provide better clustering and smaller indexes
/// </remarks>
// V Integer for high-volume, simple entities
public sealed class LogEntry
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
}
```

**Value Objects:**

```csharp
/// <summary>
/// Value object representing a physical address.
/// </summary>
/// <param name="Street">The street address.</param>
/// <param name="City">The city name.</param>
/// <param name="PostalCode">The postal/ZIP code.</param>
/// <param name="Country">The country name.</param>
/// <remarks>
/// Value objects are immutable and compared by value, not identity.
/// Use for domain concepts that have no identity and are defined by their attributes.
/// </remarks>
public sealed record Address(
    string Street,
    string City,
    string PostalCode,
    string Country)
{
    /// <summary>
    /// Factory method to create a validated address.
    /// </summary>
    /// <param name="street">The street address.</param>
    /// <param name="city">The city name.</param>
    /// <param name="postalCode">The postal/ZIP code.</param>
    /// <param name="country">The country name.</param>
    /// <returns>A validated address instance.</returns>
    /// <exception cref="ArgumentException">Thrown when any parameter is null or whitespace.</exception>
    /// <remarks>
    /// Factory method enforces validation rules at creation time.
    /// Prevents invalid addresses from existing in the system.
    /// </remarks>
    public static Address Create(string street, string city, string postalCode, string country)
    {
        // Validate all required fields - fail fast if data is invalid
        ArgumentException.ThrowIfNullOrWhiteSpace(street);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(postalCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(country);
        
        return new Address(street, city, postalCode, country);
    }
}

public sealed class Customer
{
    public Guid Id { get; private set; }
    public Address ShippingAddress { get; private set; } = null!;
    
    // X Avoid: Multiple foreign keys instead of value object
    // public int ShippingAddressId { get; set; }
    // public ShippingAddress ShippingAddress { get; set; }
}
```

---

### Entity Configuration

**Use Fluent API with `IEntityTypeConfiguration<T>` for non-trivial models.**

```csharp
// V Clean entity without attributes
/// <summary>
/// Order aggregate root with encapsulated collection.
/// </summary>
/// <remarks>
/// Design decision: Private backing field with public IReadOnlyList prevents external modification.
/// This encapsulation ensures order lines can only be added through controlled business methods
/// that enforce invariants. Direct list access would allow bypassing validation.
/// </remarks>
public sealed class Order
{
    public Guid Id { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public byte[] RowVersion { get; private set; } = null!;
    
    // Private backing field prevents external modification
    private readonly List<OrderLine> _lines = new();
    // Public readonly interface exposes collection without mutation capability
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
}

/// <summary>
/// Entity Framework Core configuration for the Order entity.
/// </summary>
/// <remarks>
/// Separates persistence configuration from domain model to keep entities clean.
/// All EF-specific mapping logic is centralized here.
/// </remarks>
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    /// <summary>
    /// Configures the Order entity mapping.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // Table & Primary Key
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);
        
        // Property Configurations
        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50); // Explicit length prevents unbounded nvarchar(max)
            
        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>(); // Store enum as string for database readability
        
        // Concurrency Control
        // RowVersion enables optimistic concurrency - prevents lost updates
        builder.Property(o => o.RowVersion)
            .IsRowVersion();
        
        // Relationships
        // HasMany with private backing field maintains encapsulation
        builder.HasMany(typeof(OrderLine))
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade); // Cascade delete removes orphaned lines
        
        // Indexes for Query Performance
        // Unique index ensures no duplicate order numbers
        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();
        
        // Non-unique index for filtering by status
        builder.HasIndex(o => o.Status);
    }
}

/// <summary>
/// Application database context with auto-discovered entity configurations.
/// </summary>
/// <remarks>
/// Best practice: Use ApplyConfigurationsFromAssembly to automatically discover and apply
/// all IEntityTypeConfiguration implementations. This eliminates manual registration and
/// prevents forgetting to register new configurations.
/// </remarks>
// DbContext registration
public class MyDbContext : DbContext
{
    /// <summary>
    /// Configures the model by applying all entity type configurations from the assembly.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // V Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyDbContext).Assembly);
    }
}
```

**Value Object Configuration:**

```csharp
/// <summary>
/// Entity configuration for Customer aggregate with owned value object.
/// </summary>
/// <remarks>
/// Design decision: Use OwnsOne for value objects that:
/// - Have no identity of their own
/// - Are always accessed through the parent entity
/// - Should be stored in the same table (performance)
/// EF Core maps owned entity properties to parent table with prefixed column names.
/// </remarks>
public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    /// <summary>
    /// Configures Customer entity and its owned ShippingAddress value object.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);
        
        /// <summary>
        /// Configures Address as owned entity mapped to Customer table.
        /// </summary>
        /// <remarks>
        /// Performance: Storing value object in parent table avoids joins.
        /// Column names will be prefixed (e.g., ShippingAddress_Street).
        /// All properties required because null Address would leave dangling columns.
        /// </remarks>
        // V Owned entity for value object
        builder.OwnsOne(c => c.ShippingAddress, address =>
        {
            address.Property(a => a.Street).HasMaxLength(200).IsRequired();
            address.Property(a => a.City).HasMaxLength(100).IsRequired();
            address.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
            address.Property(a => a.Country).HasMaxLength(100).IsRequired();
        });
    }
}
```

---

### Navigation Properties & Relationships

**Keep navigation properties minimal and unidirectional.**

```csharp
// V Unidirectional relationships
public sealed class Order
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    
    // Navigation only when needed
    private readonly List<OrderLine> _lines = new();
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    
    // No navigation to Customer - use CustomerId directly
}

public sealed class OrderLine
{
    public Guid Id { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal Price { get; private set; }
    
    // No back-reference to Order
    private OrderLine() { }
}

// X Avoid bidirectional unless truly needed
public class BadOrder
{
    public List<OrderLine> Lines { get; set; }
    public Customer Customer { get; set; } // Unnecessary navigation
}

public class BadOrderLine
{
    public Order Order { get; set; } // Creates circular reference
}
```

**Disable Lazy Loading by Default:**

```csharp
/// <summary>
/// Application database context with lazy loading disabled for explicit control.
/// </summary>
/// <remarks>
/// Design decision: Disable lazy loading to prevent N+1 query issues and avoid
/// accidental queries in presentation layer. Use explicit Include() or Load() instead.
/// Performance: Prevents unintended database roundtrips by requiring explicit loading strategy.
/// </remarks>
public class MyDbContext : DbContext
{
    /// <summary>
    /// Configures the database context options including lazy loading behavior.
    /// </summary>
    /// <param name="optionsBuilder">The builder used to configure the context.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // V Explicit loading control
        optionsBuilder.UseLazyLoadingProxies(false);
    }
}
```

---

### Create Operations

**Let entities create themselves through constructors or factory methods.**

```csharp
/// <summary>
/// Order aggregate with factory method for controlled creation.
/// </summary>
/// <remarks>
/// Design decision: Factory method centralizes validation and initialization logic.
/// Prevents invalid entities from being created by enforcing all invariants at construction.
/// Private constructor prevents direct instantiation outside factory.
/// </remarks>
// V Factory method enforces invariants
public sealed class Order
{
    /// <summary>
    /// Creates a new order with validated initial state.
    /// </summary>
    /// <param name="orderNumber">The business order number.</param>
    /// <param name="customerId">The customer identifier.</param>
    /// <returns>A new order in Draft status.</returns>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
    /// <remarks>
    /// Factory method ensures all required fields are set and valid before order exists.
    /// All new orders start in Draft status - this business rule is enforced here.
    /// </remarks>
    public static Order Create(string orderNumber, Guid customerId)
    {
        // Validate required business key
        ArgumentException.ThrowIfNullOrWhiteSpace(orderNumber);
        // Ensure customer exists (non-empty GUID)
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID is required", nameof(customerId));
        
        return new Order
        {
            Id = Guid.NewGuid(),  // Generate ID on client side
            OrderNumber = orderNumber,
            CustomerId = customerId,
            Status = OrderStatus.Draft,  // Business rule: all orders start as Draft
            CreatedAt = DateTime.UtcNow
        };
    }
    
    private Order() { } // EF Core constructor - prevents external instantiation
}

/// <summary>
/// Service demonstrating proper entity creation through factory method.
/// </summary>
// V Repository/Service usage
public sealed class OrderService : IOrderService
{
    private readonly MyDbContext _context;
    
    /// <summary>
    /// Creates a new order with validation.
    /// </summary>
    /// <param name="orderNumber">The order number.</param>
    /// <param name="customerId">The customer ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the created order.</returns>
    /// <remarks>
    /// Design decision: Use factory method to ensure entity validity before persistence.
    /// Add to context and SaveChanges in single unit of work for consistency.
    /// </remarks>
    public async Task<Guid> CreateOrderAsync(
        string orderNumber, 
        Guid customerId, 
        CancellationToken cancellationToken)
    {
        // Factory method enforces all validation and business rules
        var order = Order.Create(orderNumber, customerId);
        
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
        
        return order.Id;
    }
}

/// <summary>
/// Anti-pattern: Manually setting entity state without validation.
/// </summary>
/// <remarks>
/// WRONG: Directly setting properties bypasses validation, allows incomplete entities,
/// and duplicates initialization logic. Use factory methods to enforce invariants.
/// </remarks>
// X Avoid setting state directly
public async Task BadCreateAsync(Order order)
{
    order.Id = Guid.NewGuid();  // Duplicates logic - should be in factory
    order.CreatedAt = DateTime.UtcNow;  // Missing Status, validation, etc.
    _context.Orders.Add(order);
    await _context.SaveChangesAsync();
}
```

---

### Read Operations

**Never return tracked entities from read operations. Use DTO projections.**

```csharp
/// <summary>
/// Query service for retrieving order data optimized for reading.
/// </summary>
/// <remarks>
/// Separate query services from command services for CQRS pattern.
/// All queries use AsNoTracking for performance since we don't modify data.
/// </remarks>
public sealed class OrderQueryService : IOrderQueryService
{
    private readonly MyDbContext _context;
    
    /// <summary>
    /// Retrieves all orders as lightweight DTOs.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of order DTOs.</returns>
    /// <remarks>
    /// Projects to DTO to avoid loading full entities.
    /// AsNoTracking improves performance by skipping change tracking.
    /// </remarks>
    public async Task<IReadOnlyList<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken)
    {
        return await _context.Orders
            .AsNoTracking() // V Always for read-only - improves performance and memory
            .Select(o => new OrderDto( // V Project to DTO - load only needed fields
                o.Id,
                o.OrderNumber,
                o.Status.ToString(),
                o.Lines.Count,
                o.Lines.Sum(l => l.Quantity * l.Price)))
            .ToListAsync(cancellationToken);
    }
    
    /// <summary>
    /// Retrieves a single order by identifier.
    /// </summary>
    /// <param name="id">The order identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The order DTO if found; otherwise, null.</returns>
    public async Task<OrderDto?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.Id == id)
            .Select(o => new OrderDto(
                o.Id,
                o.OrderNumber,
                o.Status.ToString(),
                o.Lines.Count,
                o.Lines.Sum(l => l.Quantity * l.Price)))
            .FirstOrDefaultAsync(cancellationToken);
    }
}

/// <summary>
/// Anti-pattern: Returning tracked entities for read-only operations.
/// </summary>
/// <param name="id">Order identifier.</param>
/// <returns>Tracked entity with full object graph.</returns>
/// <remarks>
/// WRONG: Tracking entities for read-only operations wastes memory and CPU.
/// Include loads entire object graph even if not needed. Properties can be
/// accidentally modified. Use AsNoTracking() and project to DTOs for queries.
/// Performance: Change tracker overhead increases with entity count.
/// </remarks>
// X Avoid returning tracked entities
public async Task<Order> BadGetAsync(Guid id)
{
    return await _context.Orders
        .Include(o => o.Lines) // Loads entire graph into memory
        .FirstAsync(o => o.Id == id);  // Tracked - change tracker overhead
}
```

**Query Only Required Fields:**

```csharp
/// <summary>
/// Data transfer object for order summary information.
/// </summary>
/// <param name="OrderNumber">The order identifier displayed to users.</param>
/// <param name="Status">The current status of the order.</param>
/// <remarks>
/// Design decision: Minimal DTO containing only fields needed for summary views.
/// Reduces memory footprint and database I/O by projecting only required columns.
/// Prevents over-fetching and reduces serialization overhead.
/// </remarks>
// V Project minimal fields
public sealed record OrderSummaryDto(string OrderNumber, string Status);

/// <summary>
/// Retrieves order summaries with minimal data projection.
/// </summary>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>List of order summaries.</returns>
/// <remarks>
/// Performance: AsNoTracking avoids change tracking overhead for read-only queries.
/// Projection with Select reduces data transfer by fetching only required columns.
/// </remarks>
public async Task<IReadOnlyList<OrderSummaryDto>> GetOrderSummariesAsync(
    CancellationToken cancellationToken)
{
    return await _context.Orders
        .AsNoTracking()  // No tracking for read-only data
        .Select(o => new OrderSummaryDto(o.OrderNumber, o.Status.ToString()))
        .ToListAsync(cancellationToken);
}

/// <summary>
/// Retrieves an order with its line items using explicit loading.
/// </summary>
/// <param name="id">The order identifier.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>The order with lines if found; otherwise, null.</returns>
/// <remarks>
/// Design decision: Use Include only when navigation property is truly needed.
/// Explicit Include statements document exactly what related data is loaded,
/// preventing accidental N+1 queries and providing query intent clarity.
/// </remarks>
// V Use Include only when truly needed
public async Task<Order?> GetOrderWithLinesAsync(Guid id, CancellationToken cancellationToken)
{
    return await _context.Orders
        .Include(o => o.Lines) // Explicit control - single query with JOIN
        .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
}
```

---

### Update Operations

**Always load, modify through domain methods, then save.**

```csharp
/// <summary>
/// Service for managing order business operations.
/// </summary>
/// <remarks>
/// Implements Load-Modify-Save pattern to ensure domain logic is applied.
/// Transaction boundaries are managed at this layer.
/// </remarks>
public sealed class OrderService : IOrderService
{
    private readonly MyDbContext _context;
    private readonly ILogger<OrderService> _logger;
    
    /// <summary>
    /// Adds a line item to an existing order.
    /// </summary>
    /// <param name="orderId">The order identifier.</param>
    /// <param name="productName">The product name.</param>
    /// <param name="quantity">The quantity to order.</param>
    /// <param name="price">The unit price.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="NotFoundException">Thrown when order is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when business rules are violated.</exception>
    public async Task AddLineToOrderAsync(
        Guid orderId, 
        string productName, 
        int quantity, 
        decimal price,
        CancellationToken cancellationToken)
    {
        // Step 1: Load the complete aggregate with related data
        // Include is necessary because we need to modify the collection
        var order = await _context.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        
        // Step 2: Guard clause - fail fast if order doesn't exist
        if (order is null)
            throw new NotFoundException($"Order {orderId} not found");
        
        // Step 3: Modify through domain method (not direct property access)
        // Domain method enforces business rules (e.g., only draft orders can be modified)
        order.AddLine(productName, quantity, price);
        
        // Step 4: Persist changes - EF Core tracks modifications automatically
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    /// <summary>
    /// Completes an order, making it immutable.
    /// </summary>
    /// <param name="orderId">The order identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="NotFoundException">Thrown when order is not found.</exception>
    /// <exception cref="ConcurrencyException">Thrown when order was modified by another user.</exception>
    public async Task CompleteOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
            
        if (order is null)
            throw new NotFoundException($"Order {orderId} not found");
        
        try
        {
            // Domain method enforces state transition rules
            order.Complete();
            
            // SaveChanges checks RowVersion for concurrency conflicts
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Log concurrency conflicts for troubleshooting
            _logger.LogWarning(ex, "Concurrency conflict updating order {OrderId}", orderId);
            
            // Wrap in domain exception to hide infrastructure details from upper layers
            throw new ConcurrencyException("Order was modified by another user", ex);
        }
    }
}

// X Never blindly attach/update detached entities
public async Task BadUpdateAsync(Order detachedOrder)
{
    _context.Orders.Update(detachedOrder); // Dangerous!
    await _context.SaveChangesAsync();
}
```

**Optimistic Concurrency:**

```csharp
/// <summary>
/// Order entity with optimistic concurrency control.
/// </summary>
/// <remarks>
/// Design decision: Use RowVersion for optimistic concurrency to detect conflicting updates.
/// When two users modify the same record, the second save operation will throw
/// DbUpdateConcurrencyException. This prevents lost updates without locking rows.
/// </remarks>
// V Entity with row version
public sealed class Order
{
    public Guid Id { get; private set; }
    /// <summary>
    /// Gets the row version for optimistic concurrency control.
    /// </summary>
    /// <remarks>
    /// Automatically updated by database on every modification.
    /// Used to detect concurrent modifications by comparing expected vs actual value.
    /// </remarks>
    public byte[] RowVersion { get; private set; } = null!;
    // ... other properties
}

/// <summary>
/// Entity configuration for order with concurrency token.
/// </summary>
// Configuration
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    /// <summary>
    /// Configures the order entity with optimistic concurrency.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // SQL Server automatically updates this column on every UPDATE
        builder.Property(o => o.RowVersion)
            .IsRowVersion(); // SQL Server: rowversion/timestamp
    }
}

/// <summary>
/// Example of handling concurrency conflicts during save operations.
/// </summary>
/// <remarks>
/// Best practice: Catch DbUpdateConcurrencyException and translate to domain exception.
/// Inform user that record was modified and they need to reload and retry.
/// Never silently overwrite changes (last-write-wins is usually wrong).
/// </remarks>
// V Handle concurrency exceptions
try
{
    await _context.SaveChangesAsync(cancellationToken);
}
catch (DbUpdateConcurrencyException ex)
{
    // Log the conflict for diagnostics
    _logger.LogWarning(ex, "Concurrency conflict");
    // Throw domain exception with user-friendly message
    throw new ConcurrencyException("The record was modified by another user", ex);
}
```

---

### Delete Operations

**Prefer soft deletes for business-critical data.**

```csharp
/// <summary>
/// Order entity with soft delete capability.
/// </summary>
/// <remarks>
/// Soft deletes preserve data for audit and recovery purposes.
/// Global query filter automatically excludes deleted records from all queries.
/// </remarks>
public sealed class Order
{
    /// <summary>
    /// Gets the unique identifier.
    /// </summary>
    public Guid Id { get; private set; }
    
    /// <summary>
    /// Gets a value indicating whether this order has been soft-deleted.
    /// </summary>
    public bool IsDeleted { get; private set; }
    
    /// <summary>
    /// Gets the UTC timestamp when the order was deleted, if applicable.
    /// </summary>
    public DateTime? DeletedAt { get; private set; }
    
    /// <summary>
    /// Marks this order as deleted without removing it from the database.
    /// </summary>
    /// <remarks>
    /// Preserves data for audit trail and potential recovery.
    /// Sets deletion timestamp for compliance and reporting.
    /// </remarks>
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow; // Record when deletion occurred
    }
}

// Configuration with global filter
/// <summary>
/// Entity configuration with global query filter for soft deletes.
/// </summary>
/// <remarks>
/// Design decision: Global query filter automatically excludes soft-deleted records from all queries.
/// No need to add WHERE IsDeleted = false to every query. Index on IsDeleted improves filter performance.
/// </remarks>
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    /// <summary>
    /// Configures soft delete with global query filter.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // Automatically applied to all queries - prevents accidental retrieval of deleted records
        builder.HasQueryFilter(o => !o.IsDeleted);
        
        // Index improves performance of the query filter
        builder.HasIndex(o => o.IsDeleted);
    }
}

/// <summary>
/// Soft deletes an order by marking it as deleted.
/// </summary>
/// <param name="orderId">The order identifier.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <exception cref="NotFoundException">Thrown when order not found.</exception>
/// <remarks>
/// Business rule: Use soft delete for business data to maintain audit trail and referential integrity.
/// Record remains in database but is hidden from normal queries. Can be restored if needed.
/// </remarks>
// V Soft delete usage
public async Task DeleteOrderAsync(Guid orderId, CancellationToken cancellationToken)
{
    var order = await _context.Orders
        .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        
    if (order is null)
        throw new NotFoundException($"Order {orderId} not found");
    
    // Entity method encapsulates soft delete logic and sets DeletedAt timestamp
    order.MarkAsDeleted();
    await _context.SaveChangesAsync(cancellationToken);
}

/// <summary>
/// Retrieves order including soft-deleted records.
/// </summary>
/// <param name="id">Order identifier.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>Order if found, including soft-deleted records.</returns>
/// <remarks>
/// Use case: Admin tools, audit reports, or restore functionality need to see deleted records.
/// IgnoreQueryFilters() bypasses global query filter for this specific query.
/// </remarks>
// V Query including deleted (when needed)
public async Task<Order?> GetOrderIncludingDeletedAsync(Guid id, CancellationToken cancellationToken)
{
    return await _context.Orders
        .IgnoreQueryFilters()  // Bypasses HasQueryFilter to include deleted records
        .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
}

/// <summary>
/// Log entry entity for technical/disposable data.
/// </summary>
/// <remarks>
/// Design decision: No soft delete for technical data that has no business value after expiration.
/// Logs are disposable - hard delete frees disk space without affecting business operations.
/// </remarks>
// V Hard delete for technical/disposable data
public sealed class LogEntry
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Permanently removes old log entries to free disk space.
/// </summary>
/// <param name="before">Delete logs older than this date.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <remarks>
/// Performance: ExecuteSqlInterpolated is faster than loading entities into context.
/// Use for batch operations on technical data. Never use for business-critical data.
/// </remarks>
public async Task PurgeOldLogsAsync(DateTime before, CancellationToken cancellationToken)
{
    // Direct SQL execution - bypasses change tracker for better performance
    await _context.Database.ExecuteSqlInterpolatedAsync(
        $"DELETE FROM LogEntries WHERE Timestamp < {before}",
        cancellationToken);
}
```

---

### DbContext Usage

**Treat DbContext as a unit-of-work with short lifetime.**

```csharp
/// <summary>
/// Configures DbContext with proper lifetime management.
/// </summary>
/// <remarks>
/// Scoped lifetime ensures one DbContext per request/operation.
/// DbContext is NOT thread-safe and should never be singleton.
/// </remarks>
public void ConfigureServices(IServiceCollection services)
{
    // V Scoped is correct - new instance per request/operation
    services.AddDbContext<MyDbContext>(options =>
        options.UseSqlServer(connectionString),
        ServiceLifetime.Scoped); // Default, correct
}

/// <summary>
/// Entity Framework Core database context.
/// </summary>
/// <remarks>
/// Keep DbContext thin - no business logic, only data access.
/// DbContext represents a unit-of-work and change tracker.
/// </remarks>
public class MyDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MyDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }
    
    /// <summary>
    /// Gets or sets the Orders entity set.
    /// </summary>
    public DbSet<Order> Orders { get; set; }
    
    /// <summary>
    /// Gets or sets the Customers entity set.
    /// </summary>
    public DbSet<Customer> Customers { get; set; }
    
    /// <summary>
    /// Configures the model using Fluent API.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all IEntityTypeConfiguration<T> from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyDbContext).Assembly);
    }
    
    // X No business logic methods here
    // X No orchestration or workflow logic
}

// X Never use singleton lifetime
services.AddDbContext<MyDbContext>(options => 
    options.UseSqlServer(connectionString),
    ServiceLifetime.Singleton); // WRONG! DbContext is not thread-safe
```

---

### Repositories

**Use explicit repositories only when enforcing aggregate boundaries or hiding EF Core.**

```csharp
/// <summary>
/// Repository interface defining aggregate boundary for Order.
/// </summary>
/// <remarks>
/// Design decision: Repository methods express domain operations, not generic CRUD.
/// GetByOrderNumberAsync reflects business need to find orders by their business key.
/// Only exposes operations that make sense for the Order aggregate.
/// </remarks>
// V Explicit repository for aggregate boundary
public interface IOrderRepository
{
    /// <summary>
    /// Retrieves order with its line items by ID.
    /// </summary>
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    
    /// <summary>
    /// Retrieves order by business key (order number).
    /// </summary>
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken);
    
    /// <summary>
    /// Adds new order to the context.
    /// </summary>
    void Add(Order order);
    
    /// <summary>
    /// Persists changes to the database.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Repository implementation for Order aggregate.
/// </summary>
/// <remarks>
/// Design decision: Repository encapsulates data access and aggregate loading strategy.
/// Controls which related entities are loaded (Include) to ensure aggregate consistency.
/// </remarks>
public sealed class OrderRepository : IOrderRepository
{
    private readonly MyDbContext _context;
    
    /// <summary>
    /// Initializes repository with database context.
    /// </summary>
    /// <param name="context">The database context.</param>
    public OrderRepository(MyDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }
    
    /// <summary>
    /// Retrieves order with line items by ID.
    /// </summary>
    /// <param name="id">Order identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Order if found; otherwise, null.</returns>
    /// <remarks>
    /// Design decision: Always load Lines with Order to maintain aggregate boundary.
    /// Order and OrderLines form a transaction boundary - load together for consistency.
    /// </remarks>
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .Include(o => o.Lines)  // Load full aggregate
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }
    
    /// <summary>
    /// Retrieves order by business key (order number).
    /// </summary>
    /// <param name="orderNumber">The order number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Order if found; otherwise, null.</returns>
    /// <remarks>
    /// Business rule: Order number is unique business identifier.
    /// No Include of Lines - use when only need to check existence or read order header.
    /// </remarks>
    public async Task<Order?> GetByOrderNumberAsync(
        string orderNumber, 
        CancellationToken cancellationToken)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }
    
    /// <summary>
    /// Adds new order to the context.
    /// </summary>
    /// <param name="order">The order to add.</param>
    /// <remarks>
    /// Design decision: Synchronous Add because it only marks entity for insertion.
    /// Actual database INSERT happens in SaveChangesAsync for unit of work pattern.
    /// </remarks>
    public void Add(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        _context.Orders.Add(order);
    }
    
    /// <summary>
    /// Persists all changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// Design decision: Repository exposes SaveChanges for explicit control over transaction boundary.
    /// Alternative: UnitOfWork pattern could centralize SaveChanges, but adds complexity.
    /// </remarks>
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Anti-pattern: Generic repository that adds no value and reduces clarity.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
/// <remarks>
/// WRONG: Generic repositories abstract away EF Core's already-abstracted DbSet without adding value.
/// They force you to use object ids, limit query capabilities, and hide EF Core's powerful features.
/// Use aggregate-specific repositories (IOrderRepository, ICustomerRepository) that expose
/// domain-meaningful operations instead. For simple CRUD, use DbContext directly.
/// </remarks>
// X Avoid generic repositories (no value)
public interface IGenericRepository<T> where T : class
{
    Task<T> GetByIdAsync(object id);  // Loses type safety with object id
    Task<IEnumerable<T>> GetAllAsync();  // Hides filtering and projection capabilities
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}
```

**When Repositories Are Optional:**

```csharp
/// <summary>
/// Service using DbContext directly without repository.
/// </summary>
/// <remarks>
/// Design decision: Skip repository for simple read-only queries that don't need
/// aggregate loading logic. Repository adds no value for single-table DTO projections.
/// Use repositories for complex aggregates with business logic.
/// </remarks>
// V Direct DbContext usage in simple cases
public sealed class CustomerService : ICustomerService
{
    private readonly MyDbContext _context;
    
    /// <summary>
    /// Retrieves customer DTO by ID.
    /// </summary>
    /// <param name="id">Customer identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Customer DTO if found; otherwise, null.</returns>
    /// <remarks>
    /// Performance: AsNoTracking disables change tracking for read-only query.
    /// Select projects to DTO in database - only transfers required columns.
    /// No repository needed - query is too simple to benefit from abstraction.
    /// </remarks>
    public async Task<CustomerDto?> GetCustomerAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Customers
            .AsNoTracking()  // No tracking for read-only
            .Where(c => c.Id == id)
            .Select(c => new CustomerDto(c.Id, c.Name, c.Email))  // Database projection
            .FirstOrDefaultAsync(cancellationToken);
    }
}
```

---

### Migrations & Schema Evolution

**Treat migrations as production code.**

```bash
# V Create migration with clear name
dotnet ef migrations add AddOrderLineQuantityConstraint

# V Review the generated migration
# Check:
# - Column types and constraints
# - Index definitions
# - Data migrations
# - Destructive operations

# V Apply in development
dotnet ef database update

# V Generate SQL script for production
dotnet ef migrations script --idempotent --output migration.sql
```

**Explicit Index Configuration:**

```csharp
/// <summary>
/// Entity configuration with explicit index definitions.
/// </summary>
/// <remarks>
/// Design decision: Always define indexes explicitly with meaningful names for query optimization.
/// Indexes should match common query patterns. Use composite indexes for multi-column filters.
/// Filtered indexes reduce index size by excluding soft-deleted records.
/// </remarks>
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    /// <summary>
    /// Configures order entity with indexes for optimal query performance.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // Unique index for business key - prevents duplicate order numbers
        // V Explicit indexes for queries
        builder.HasIndex(o => o.OrderNumber)
            .IsUnique()
            .HasDatabaseName("IX_Orders_OrderNumber");
        
        // Non-unique index for filtering by status (WHERE Status = ...)
        builder.HasIndex(o => o.Status)
            .HasDatabaseName("IX_Orders_Status");
        
        // Composite index for customer order history queries (WHERE CustomerId = ... ORDER BY CreatedAt)
        builder.HasIndex(o => new { o.CustomerId, o.CreatedAt })
            .HasDatabaseName("IX_Orders_CustomerId_CreatedAt");
        
        // Filtered index excludes soft-deleted records - smaller, faster index
        // V Filtered index (SQL Server)
        builder.HasIndex(o => o.Status)
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_Orders_Status_Active");
    }
}
```

**Data Migrations:**

```csharp
/// <summary>
/// Database migration adding Status column with data transformation.
/// </summary>
/// <remarks>
/// Best practice: Combine schema changes with data migrations in a single migration.
/// Use defaultValue to provide valid values for existing rows during schema change.
/// Follow with SQL statement to set correct values based on business logic.
/// Always test migrations on copy of production data before deployment.
/// </remarks>
// V Split schema and data migrations
public partial class AddOrderStatusColumn : Migration
{
    /// <summary>
    /// Applies the migration by adding Status column and populating existing data.
    /// </summary>
    /// <param name="migrationBuilder">The migration builder.</param>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Schema change - add new required column with safe default value
        migrationBuilder.AddColumn<string>(
            name: "Status",
            table: "Orders",
            nullable: false,
            defaultValue: "Draft");
        
        // Data migration - update existing records with correct status based on CompletedAt
        migrationBuilder.Sql(
            "UPDATE Orders SET Status = 'Completed' WHERE CompletedAt IS NOT NULL");
    }
}
```

---

### Performance Principles

**Project instead of loading, disable tracking, measure performance.**

```csharp
// V Query only what you need
/// <summary>
/// Retrieves paginated order summaries.
/// </summary>
/// <param name="pageNumber">Page number (1-based).</param>
/// <param name="pageSize">Number of items per page.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>Page of order summaries.</returns>
/// <remarks>
/// Performance: AsNoTracking + projection minimizes memory and network usage.
/// Skip/Take generates efficient OFFSET/FETCH query in SQL Server.
/// Always order before paging to ensure consistent results.
/// </remarks>
public async Task<IReadOnlyList<OrderSummaryDto>> GetOrderSummariesAsync(
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken)
{
    return await _context.Orders
        .AsNoTracking() // V Disable tracking for read-only query
        .OrderByDescending(o => o.CreatedAt)  // Required for deterministic paging
        .Skip((pageNumber - 1) * pageSize)  // OFFSET in SQL
        .Take(pageSize)  // FETCH NEXT in SQL
        .Select(o => new OrderSummaryDto( // V Project to DTO in database
            o.OrderNumber,
            o.Status.ToString(),
            o.CreatedAt))
        .ToListAsync(cancellationToken);
}

/// <summary>
/// Anti-pattern: Deep Include chains with circular references.
/// </summary>
/// <param name="id">Order identifier.</param>
/// <returns>Order with massive object graph.</returns>
/// <remarks>
/// WRONG: Loads entire object graph including circular references (Customer.Orders).
/// Memory explosion: Each order loads customer, which loads all orders, which load customers...
/// Performance disaster: Cartesian product from multiple includes causes duplicate data transfer.
/// Solution: Load only what you need, use separate queries, or use DTO projections.
/// </remarks>
// X Avoid deep Include chains
public async Task<Order> BadGetAsync(Guid id)
{
    return await _context.Orders
        .Include(o => o.Lines)
        .Include(o => o.Customer)
            .ThenInclude(c => c.Address)
        .Include(o => o.Customer)
            .ThenInclude(c => c.Orders) // Circular! Loads all customer's orders
        .FirstAsync(o => o.Id == id);
}

/// <summary>
/// Retrieves order details using multiple focused queries.
/// </summary>
/// <param name="id">Order identifier.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>Order details DTO.</returns>
/// <remarks>
/// Design decision: Split into two queries to avoid cartesian product from Include.
/// Each query is optimized and transfers minimal data. Often faster than single complex query.
/// Use for complex object graphs where Include generates inefficient SQL.
/// </remarks>
// V Split into multiple queries if needed
public async Task<OrderDetailsDto> GetOrderDetailsAsync(Guid id, CancellationToken cancellationToken)
{
    // First query: Load order header with minimal fields
    var order = await _context.Orders
        .AsNoTracking()
        .Select(o => new { o.Id, o.OrderNumber, o.CustomerId })
        .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    
    if (order is null)
        return null;
    
    // Second query: Load order lines separately
    var lines = await _context.Set<OrderLine>()
        .AsNoTracking()
        .Where(l => l.OrderId == order.Id)
        .Select(l => new OrderLineDto(l.ProductName, l.Quantity, l.Price))
        .ToListAsync(cancellationToken);
    
    return new OrderDetailsDto(order.OrderNumber, lines);
}
```

**Enable Query Logging in Development:**

```csharp
/// <summary>
/// Database context configured with enhanced logging for development troubleshooting.
/// </summary>
/// <remarks>
/// Design decision: Enable sensitive data logging and detailed query logging only in Development.
/// Security: Sensitive data logging can expose parameter values; never enable in Production.
/// Performance: Console logging has performance overhead; use structured logging in Production.
/// </remarks>
public class MyDbContext : DbContext
{
    /// <summary>
    /// Configures enhanced logging for development environment.
    /// </summary>
    /// <param name="optionsBuilder">The builder used to configure the context.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            optionsBuilder
                .EnableSensitiveDataLogging()  // Shows parameter values in logs
                .LogTo(Console.WriteLine, LogLevel.Information);
        }
    }
}
```

**Compiled Queries for Hot Paths:**

```csharp
/// <summary>
/// Static compiled query for hot path optimization.
/// </summary>
/// <remarks>
/// Performance: EF.CompileAsyncQuery pre-compiles LINQ expression tree to avoid repeated parsing.
/// Use for queries executed frequently (hot paths). First execution compiles and caches query plan.
/// Subsequent executions reuse compiled plan - saves 1-2ms per query.
/// Trade-off: Static field increases memory, only worth it for high-frequency queries.
/// </remarks>
// V Compiled query for frequently-used queries
private static readonly Func<MyDbContext, Guid, CancellationToken, Task<Order?>> GetOrderByIdCompiled =
    EF.CompileAsyncQuery((MyDbContext context, Guid id, CancellationToken ct) =>
        context.Orders
            .Include(o => o.Lines)
            .FirstOrDefault(o => o.Id == id));

/// <summary>
/// Retrieves order using pre-compiled query.
/// </summary>
/// <param name="id">Order identifier.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>Order if found; otherwise, null.</returns>
/// <remarks>
/// Performance: Compiled query eliminates LINQ-to-SQL translation overhead.
/// Use for queries called thousands of times per second. Measure before optimizing.
/// </remarks>
public async Task<Order?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken)
{
    return await GetOrderByIdCompiled(_context, id, cancellationToken);
}
```

---

### Architecture Placement

**Keep EF Core in the infrastructure layer. Domain must not depend on EF Core.**

```
MyProject_Logic/                           # V Domain layer
├── Entities/
│   └── Order.cs                           # V Pure domain entity
├── Interfaces/
│   └── IOrderRepository.cs                # V Abstractions only

MyProject_DataAccess/                      # V Infrastructure layer
├── DbContext/
│   └── MyDbContext.cs                     # V EF Core context
├── Configurations/
│   └── OrderConfiguration.cs              # V EF Core config
└── Repositories/
    └── OrderRepository.cs                 # V EF Core implementation
```

```csharp
// V Domain entity (Logic layer)
namespace MyProject.Logic.Entities;

/// <summary>
/// Clean domain entity with no infrastructure dependencies.
/// </summary>
/// <remarks>
/// Design decision: Keep domain model free of ORM attributes and dependencies.
/// This allows domain to evolve independently of persistence technology.
/// Configuration is externalized to DataAccess layer using IEntityTypeConfiguration.
/// Private constructor allows EF Core to materialize entities while preventing external instantiation.
/// </remarks>
public sealed class Order
{
    public Guid Id { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    
    // No EF Core attributes or dependencies - pure domain model
    
    private Order() { } // For EF Core materialization only
}

/// <summary>
/// Repository interface in Logic layer defines contract.
/// </summary>
/// <remarks>
/// Layer separation: Interface in Logic layer, implementation in DataAccess layer.
/// Logic layer depends on abstraction, not concrete EF Core implementation.
/// Enables unit testing with mock repositories.
/// </remarks>
// V Repository interface (Logic layer)
namespace MyProject.Logic.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    void Add(Order order);
}

/// <summary>
/// EF Core configuration separated from domain model.
/// </summary>
/// <remarks>
/// Design decision: All ORM mapping in DataAccess layer, not in domain.
/// This keeps domain model clean and allows different persistence strategies.
/// IEntityTypeConfiguration keeps configuration organized by entity.
/// </remarks>
// V EF Core configuration (DataAccess layer)
namespace MyProject.DataAccess.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    /// <summary>
    /// Configures Order entity mapping.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // All EF Core mapping here - table name, keys, indexes, relationships
    }
}
```

---

### Common Anti-Patterns to Avoid

**X Anemic Domain Models:**
```csharp
/// <summary>
/// Anti-pattern: Entity with all public setters and no business logic.
/// </summary>
/// <remarks>
/// WRONG: Allows invalid states by exposing all setters publicly. Forces business logic
/// into service layer, creating transaction script pattern. Use private setters, factory
/// methods, and behavioral methods to enforce invariants within the entity.
/// </remarks>
// X All public setters, no behavior
public class Order
{
    public Guid Id { get; set; }  // Can be set to empty Guid
    public string Status { get; set; }  // No validation, any string allowed
    public List<OrderLine> Lines { get; set; }  // Can be set to null, no encapsulation
}
```

**X Generic Repositories:**
```csharp
/// <summary>
/// Anti-pattern: Generic repository that provides no value over DbSet.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
/// <remarks>
/// WRONG: Abstracts an already-abstracted DbSet without adding domain-specific operations.
/// Hides EF Core's rich querying capabilities and forces generic operations that don't
/// express business intent. Use aggregate-specific repositories with meaningful method names.
/// </remarks>
// X Adds no value, reduces clarity
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(object id);  // Loses type safety
    Task<IEnumerable<T>> GetAllAsync();  // No filtering, sorting, or projection
}
```

**X Lazy Loading Everywhere:**
```csharp
/// <summary>
/// Anti-pattern: Lazy loading causes N+1 query problem.
/// </summary>
/// <remarks>
/// WRONG: Queries all orders in one query, then makes one additional query PER ORDER
/// to load Customer. If there are 100 orders, this executes 101 database queries.
/// Performance catastrophe. Use explicit Include() to load related data in single query:
/// _context.Orders.Include(o => o.Customer).ToListAsync();
/// </remarks>
// X Causes N+1 queries
var orders = await _context.Orders.ToListAsync();
foreach (var order in orders)
{
    // This triggers a separate database query for EACH order
    Console.WriteLine(order.Customer.Name); // Lazy load!
}
```

**X Returning Entities from APIs:**
```csharp
/// <summary>
/// Anti-pattern: Controller returning domain entity directly to API clients.
/// </summary>
/// <param name="id">Order identifier.</param>
/// <returns>Domain entity exposed to external consumers.</returns>
/// <remarks>
/// WRONG: Exposes internal domain structure to API consumers, creating tight coupling.
/// Breaks encapsulation by serializing private state. Makes API versioning difficult.
/// Entity changes force API contract changes. Use DTOs to decouple API from domain:
/// return _mapper.Map&lt;OrderDto&gt;(order);
/// </remarks>
// X Exposes domain to API layer
[HttpGet("{id}")]
public async Task<Order> GetOrder(Guid id)
{
    return await _context.Orders.FindAsync(id);
}
```

**X Bloated DbContext:**
```csharp
/// <summary>
/// Anti-pattern: Business logic should never be in DbContext.
/// </summary>
/// <remarks>
/// WRONG: DbContext is for data access only, not business logic orchestration.
/// This violates separation of concerns and makes unit testing extremely difficult.
/// Move business logic to service layer where it belongs.
/// </remarks>
// X Business logic in DbContext
public class MyDbContext : DbContext
{
    /// <summary>
    /// ANTI-PATTERN: Business logic method in data access layer.
    /// </summary>
    /// <param name="orderId">Order identifier.</param>
    /// <returns>Operation result.</returns>
    public async Task<OperationResult> ProcessOrderAsync(Guid orderId)
    {
        // Wrong layer!
    }
}
```

**X Ignoring Migrations:**
```csharp
/// <summary>
/// Anti-pattern: Auto-applying migrations at startup is dangerous in production.
/// </summary>
/// <remarks>
/// WRONG: Database.Migrate() at startup can cause deployment failures, race conditions
/// with multiple instances, and data loss if migrations have errors. Always apply
/// migrations through controlled deployment pipeline, never automatically at runtime.
/// </remarks>
// X Auto-apply migrations in production
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseSqlServer(connectionString);
    Database.Migrate(); // DANGEROUS! Can cause race conditions in multi-instance deployments
}
```

**X Uncontrolled Navigation Graphs:**
```csharp
/// <summary>
/// Anti-pattern: Entities with circular navigation properties.
/// </summary>
/// <remarks>
/// WRONG: Circular references cause:
/// - JSON serialization errors (infinite loops)
/// - Memory issues (loads entire object graph)
/// - Confusing ownership (who owns the relationship?)
/// - N+1 query problems with lazy loading
/// Solution: Use unidirectional relationships with foreign keys only.
/// </remarks>
// X Circular references, large object graphs
public class Order
{
    public Customer Customer { get; set; }  // Navigation to customer
    public List<OrderLine> Lines { get; set; }
}

public class Customer
{
    public List<Order> Orders { get; set; }  // Back-reference creates circular graph
}

public class OrderLine
{
    public Order Order { get; set; }  // Back-reference to parent
    public Product Product { get; set; }  // Another navigation
}
```

---

## Dependency Injection Pattern

**Dependency Injection Rules:**

**Lifetime Management:**
- V Use Scoped for DbContext and request-scoped services
- V Use Transient for lightweight stateless services
- V Use Singleton for stateless shared services only
- X Never use Singleton for DbContext
- X No service locator pattern

**Registration:**
- V Register interfaces, not concrete types
- V Validate services at startup
- V Use constructor injection
- X No property injection
- X No method injection

```csharp
/// <summary>
/// Service implementing business logic for order operations.
/// </summary>
/// <remarks>
/// Service layer is responsible for:
/// - Orchestrating business workflows
/// - Transaction management
/// - Business rule enforcement
/// - Logging business operations
/// </remarks>
public sealed class MyService : IMyService
{
    private readonly IDependency _dependency;
    private readonly ILogger<MyService> _logger;
    private readonly MySettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyService"/> class.
    /// </summary>
    /// <param name="dependency">The business dependency.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="settings">Strongly-typed configuration settings.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public MyService(
        IDependency dependency,
        ILogger<MyService> logger,
        IOptions<MySettings> settings)
    {
        // Validate all dependencies at construction time - fail fast principle
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(settings);

        _dependency = dependency;
        _logger = logger;
        _settings = settings.Value; // Extract value from IOptions wrapper
    }
}
```

**Requirements:**
- V Constructor injection only
- V All parameters must be interfaces (except framework types like ILogger, IOptions, CancellationToken)
- V Validate with `ArgumentNullException.ThrowIfNull` or `??`
- V Store in `readonly` fields
- V Comprehensive XML documentation on public constructors and services
- X No service locator pattern

---

## Security Standards

**Security Rules:**

**Passwords:**
- V DPAPI for internal/on-premise applications
- V BCrypt for public-facing applications
- X Never store plaintext passwords
- X Never log passwords or secrets

**JWT:**
- V Validate issuer, audience, lifetime, signing key
- V Use strong secret keys (minimum 32 characters)
- V Store JWT secret in configuration, not code
- X Never expose signing key

**CORS:**
- V Explicitly list allowed origins
- X Never use AllowAnyOrigin in production

**General:**
- V Use parameterized queries to prevent SQL injection
- V Validate and sanitize all user input
- V Use HTTPS for all external communication
- V Store secrets in environment variables or key vault

### Password Protection

**On-Premise/Internal (Less Security Accepted):**

```csharp
/// <summary>
/// Service for protecting sensitive values using Windows DPAPI.
/// </summary>
/// <remarks>
/// Uses Data Protection API (DPAPI) for encryption.
/// Suitable for on-premise applications with lower security requirements.
/// Encrypted data is machine-specific and cannot be decrypted on other machines.
/// </remarks>
public interface IPasswordProtector
{
    /// <summary>
    /// Protects a value if it's not already encrypted.
    /// </summary>
    /// <param name="value">The value to protect.</param>
    /// <returns>The encrypted value with "enc:" prefix, or original if already encrypted.</returns>
    string ProtectIfNeeded(string value);
    
    /// <summary>
    /// Unprotects an encrypted value.
    /// </summary>
    /// <param name="value">The value to unprotect.</param>
    /// <returns>The decrypted value, or original if not encrypted.</returns>
    string Unprotect(string value);
}

/// <summary>
/// Implementation of password protection using Windows DPAPI.
/// </summary>
public sealed class PasswordProtector : IPasswordProtector
{
    // Prefix identifies encrypted values
    private const string EncryptedPrefix = "enc:";

    /// <summary>
    /// Protects a value if it's not already encrypted.
    /// </summary>
    /// <param name="value">The value to protect.</param>
    /// <returns>The encrypted value with prefix.</returns>
    public string ProtectIfNeeded(string value)
    {
        // Skip if already encrypted or empty
        if (string.IsNullOrWhiteSpace(value) || value.StartsWith(EncryptedPrefix))
            return value;

        // Encrypt using DPAPI for local machine
        var bytes = Encoding.UTF8.GetBytes(value);
        var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.LocalMachine);
        
        // Add prefix to identify encrypted values
        return EncryptedPrefix + Convert.ToBase64String(encrypted);
    }

    /// <summary>
    /// Unprotects an encrypted value.
    /// </summary>
    /// <param name="value">The value to unprotect.</param>
    /// <returns>The decrypted value.</returns>
    public string Unprotect(string value)
    {
        // Return as-is if not encrypted
        if (!value.StartsWith(EncryptedPrefix))
            return value;

        // Remove prefix and decrypt
        var encryptedValue = value.Substring(EncryptedPrefix.Length);
        var bytes = Convert.FromBase64String(encryptedValue);
        var decrypted = ProtectedData.Unprotect(bytes, null, DataProtectionScope.LocalMachine);
        return Encoding.UTF8.GetString(decrypted);
    }
}
```

**Public-Facing:**

```csharp
/// <summary>
/// Service for securely hashing and verifying passwords.
/// </summary>
/// <remarks>
/// Uses BCrypt for password hashing - industry standard for secure password storage.
/// Suitable for public-facing applications with high security requirements.
/// Hashes are one-way - passwords cannot be retrieved, only verified.
/// </remarks>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password using BCrypt.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <returns>The BCrypt hash.</returns>
    string HashPassword(string password);
    
    /// <summary>
    /// Verifies a password against a BCrypt hash.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <param name="hash">The BCrypt hash to verify against.</param>
    /// <returns>True if password matches; otherwise, false.</returns>
    bool VerifyPassword(string password, string hash);
}

/// <summary>
/// BCrypt-based password hasher implementation.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Hashes a password using BCrypt with work factor 12.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <returns>The BCrypt hash.</returns>
    /// <remarks>
    /// Work factor 12 balances security and performance.
    /// Higher values increase security but slow down hashing.
    /// </remarks>
    public string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

    /// <summary>
    /// Verifies a password against a BCrypt hash.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <param name="hash">The BCrypt hash.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public bool VerifyPassword(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}
```

### JWT Authentication (Web APIs)

```csharp
/// <summary>
/// Service for generating JWT tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user to generate a token for.</param>
    /// <returns>A JWT token string.</returns>
    string GenerateToken(UserDto user);
}

/// <summary>
/// JWT token service implementation.
/// </summary>
/// <remarks>
/// Generates JWT tokens for authenticated users.
/// Tokens include user identity and role claims.
/// Tokens are signed with HS256 algorithm for integrity.
/// </remarks>
public sealed class TokenService : ITokenService
{
    private readonly JwtSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenService"/> class.
    /// </summary>
    /// <param name="settings">JWT configuration settings.</param>
    /// <exception cref="ArgumentNullException">Thrown when settings is null.</exception>
    public TokenService(IOptions<JwtSettings> settings)
        => _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

    /// <summary>
    /// Generates a signed JWT token.
    /// </summary>
    /// <param name="user">The user to generate a token for.</param>
    /// <returns>A JWT token string.</returns>
    /// <remarks>
    /// Token includes standard claims (NameIdentifier, Name, Role).
    /// Token is time-limited based on configured expiry minutes.
    /// </remarks>
    public string GenerateToken(UserDto user)
    {
        // Build claims for the token - identity and authorization info
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role) // Used for [Authorize(Roles = "...")]
        };

        // Create signing credentials using secret key
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Build the token
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer, // Who issued the token
            audience: _settings.Audience, // Who can use the token
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes), // Time-limited
            signingCredentials: credentials); // Signature for integrity

        // Serialize to string for HTTP header transmission
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

---

## Error Handling Standards

**Error Handling Rules:**

**By Layer:**
- V Repository: Log and re-throw
- V Service: Log and return Result/Option types
- V API: Use global exception handler
- V Convert domain exceptions to appropriate HTTP status codes
- X No try/catch in controllers except for very specific cases
- X Don't swallow exceptions

**Logging:**
- V Use structured logging with context
- V Log entity IDs for troubleshooting
- V Include RequestId/OperationId
- X Don't expose internal details in API responses

**Strategy:**
- V Three-tier classification (Success/Error/Warning)
- V Fail fast with validation errors
- V Graceful degradation for partial failures

### Three-Tier Classification

```csharp
/// <summary>
/// Represents the result of a batch operation with success/error/warning tracking.
/// </summary>
/// <param name="SuccessCount">The number of successfully processed items.</param>
/// <param name="Errors">List of error messages for failed items.</param>
/// <param name="Warnings">List of warning messages for items needing attention.</param>
/// <remarks>
/// Three-tier classification allows graceful degradation.
/// Operation can partially succeed with warnings while reporting errors.
/// </remarks>
public sealed record OperationResult(
    int SuccessCount,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings)
{
    /// <summary>
    /// Gets a value indicating whether any errors occurred.
    /// </summary>
    public bool HasErrors => Errors.Count > 0;
    
    /// <summary>
    /// Gets a value indicating whether the operation was fully successful.
    /// </summary>
    public bool IsSuccess => Errors.Count == 0;
}
```

### Graceful Degradation

```csharp
/// <summary>
/// Processes a batch of items with graceful degradation.
/// </summary>
/// <param name="items">The items to process.</param>
/// <returns>An operation result with success count, errors, and warnings.</returns>
/// <remarks>
/// Implements graceful degradation - failures don't stop the entire batch.
/// Each item is processed independently with error isolation.
/// Useful for batch imports where partial success is acceptable.
/// </remarks>
public async Task<OperationResult> ProcessBatchAsync(IEnumerable<Item> items)
{
    var errors = new List<string>();
    var warnings = new List<string>();
    var successCount = 0;

    foreach (var item in items)
    {
        try
        {
            // Validation check - invalid items are warned but don't fail the batch
            if (!ValidateItem(item))
            {
                warnings.Add($"Item {item.Id}: validation failed, skipped");
                continue; // Skip this item but continue processing
            }

            await ProcessItemAsync(item);
            successCount++;
        }
        catch (Exception ex)
        {
            // Log exception for troubleshooting
            _logger.LogError(ex, "Failed to process item {ItemId}", item.Id);
            
            // Record error but continue with next item
            errors.Add($"Item {item.Id}: {ex.Message}");
        }
    }

    // Return complete result with all outcomes
    return new OperationResult(successCount, errors, warnings);
}
```

### Exception Hierarchy

**Repository Layer:**
```csharp
/// <summary>
/// Repository layer error handling.
/// </summary>
/// <remarks>
/// Design decision: Log and re-throw - repository shouldn't swallow exceptions.
/// Structured logging captures entity ID for troubleshooting.
/// Re-throw preserves stack trace for upper layers to handle.
/// </remarks>
try
{
    return await connection.ExecuteAsync(sql, data);
}
catch (Exception ex)
{
    // Log with context for diagnostics
    _logger.LogError(ex, "Failed to save {EntityId}", data.Id);
    throw; // Re-throw after logging - don't swallow exceptions
}
```

**Service Layer:**
```csharp
/// <summary>
/// Service layer error handling with graceful degradation.
/// </summary>
/// <remarks>
/// Design decision: Catch expected exceptions and return Result type.
/// Service layer converts exceptions to business results for API layer.
/// Unexpected exceptions propagate to global handler.
/// </remarks>
try
{
    // Business logic with validation
    return new OperationResult(successCount, errors, warnings);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Operation failed");
    // Return failure result instead of throwing - graceful degradation
    return OperationResult.Failure(ex.Message);
}
```

**API Layer:**
```csharp
/// <summary>
/// Controller error handling for specific exceptions.
/// </summary>
/// <remarks>
/// Design decision: Catch expected domain exceptions and convert to appropriate HTTP status.
/// ValidationException → 400 Bad Request (client error)
/// Unexpected exceptions → 500 Internal Server Error (logged for investigation)
/// Use global exception handler for unhandled exceptions to avoid repetition.
/// </remarks>
try
{
    var result = await _service.CreateAsync(request);
    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
}
catch (ValidationException ex)
{
    // Expected exception - client provided invalid data
    return BadRequest(ex.Message);
}
catch (Exception ex)
{
    // Unexpected exception - log and return generic message
    _logger.LogError(ex, "Failed to create entity");
    return StatusCode(500, "An error occurred");  // Don't expose internal details
}
```

---

## Logging Standards

**Logging Rules:**

**Structured Logging:**
- V Use Serilog for structured logging
- V Log to file and console in development
- V Use daily rolling files to prevent unbounded growth
- V Include context: RequestId, OperationId, UserId
- V Use BeginScope for correlation IDs

**Log Levels:**
- V Error: Exceptional situations that require attention
- V Warning: Unexpected but handled situations
- V Information: Important business events
- V Debug: Detailed diagnostics for troubleshooting
- X Don't log sensitive data (passwords, tokens, PII)

**Best Practices:**
- V Log entity IDs for troubleshooting
- V Use structured logging properties, not string formatting
- V Log at appropriate levels
- V Include exception objects when logging errors
- X Don't over-log (respect minimum levels)

### Serilog Configuration

```csharp
/// <summary>
/// Configures Serilog for structured logging.
/// </summary>
/// <remarks>
/// Logs to both file and console for development visibility.
/// Daily rolling files prevent unbounded disk usage.
/// Minimum levels reduce noise in logs.
/// </remarks>
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // Default minimum level
    
    // Override for Microsoft/System namespaces to reduce noise
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    
    // File sink with rolling interval
    .WriteTo.File(
        "logs/log-.txt", // File pattern includes date
        rollingInterval: RollingInterval.Day, // New file daily
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    
    // Console sink for real-time monitoring
    .WriteTo.Console()
    .CreateLogger();
```

### Structured Logging

V **Correct:**
```csharp
_logger.LogInformation("Processing {FileName} with {RecordCount} records", fileName, count);
_logger.LogError(ex, "Failed to process {FileName}", fileName);
```

X **Incorrect:**
```csharp
_logger.LogInformation($"Processing {fileName}"); // String interpolation
```

### Log Levels

| Level | Usage | Examples |
|-------|-------|----------|
| `Fatal` | Unrecoverable errors | App crash |
| `Error` | Operation failures | DB errors, file failures |
| `Warning` | Unexpected situations | Missing optional data |
| `Information` | Normal flow | Processing started/completed |
| `Debug` | Diagnostics | Variable values |

---

## Validation Standards

**Validation Rules:**

**Strategy:**
- V Use FluentValidation for complex rules (preferred)
- V Use Data Annotations for simple validation
- V Validate at service layer before business logic
- V Fail fast - return all errors at once
- X No validation in controllers
- X No validation in repositories

**Implementation:**
- V Create dedicated validator classes
- V Use meaningful error messages
- V Validate business rules, not just data types
- V Test validation rules independently

### FluentValidation

```csharp
/// <summary>
/// FluentValidation validator for create item requests.
/// </summary>
/// <remarks>
/// Centralizes validation rules for CreateItemRequest.
/// Validators are automatically discovered and registered by FluentValidation.
/// </remarks>
public sealed class CreateItemRequestValidator : AbstractValidator<CreateItemRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateItemRequestValidator"/> class.
    /// </summary>
    public CreateItemRequestValidator()
    {
        // Name is required and has maximum length
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        // Price must be positive (business rule)
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        // Email is optional, but must be valid format when provided
        // When() creates a conditional rule
        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
    }
}

/// <summary>
/// Service method demonstrating FluentValidation usage.
/// </summary>
/// <param name="request">The request to validate and process.</param>
/// <returns>Result indicating success or validation failure.</returns>
/// <remarks>
/// Design decision: Validate input at service layer before business logic execution.
/// Fail fast - return early if validation fails to avoid processing invalid data.
/// Aggregate all validation errors to provide complete feedback to client.
/// </remarks>
// Usage
public async Task<Result> CreateAsync(CreateItemRequest request)
{
    // Create validator instance
    var validator = new CreateItemRequestValidator();
    var validationResult = await validator.ValidateAsync(request);

    // Fail fast - return all errors at once
    if (!validationResult.IsValid)
    {
        // Aggregate all validation errors into single message
        var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
        return Result.Failure(errors);
    }

    // Proceed with business logic only if validation passed
}
```

### Data Annotations (Simple Cases)

```csharp
/// <summary>
/// Request model for creating an item.
/// </summary>
/// <remarks>
/// Uses data annotations for simple validation rules.
/// For complex validation, use FluentValidation instead.
/// </remarks>
public sealed class CreateItemRequest
{
    /// <summary>
    /// Gets or sets the item name.
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100)] // Prevents excessively long names
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the item price.
    /// </summary>
    /// <remarks>
    /// Price must be positive to represent valid monetary value.
    /// </remarks>
    [Range(0.01, double.MaxValue)] // Enforce positive values
    public decimal Price { get; set; }
}
```

---

## Code Quality Standards

### Nullable Reference Types

```xml
<PropertyGroup>
    <Nullable>enable</Nullable>
</PropertyGroup>
```

```csharp
/// <summary>
/// Service demonstrating nullable reference type usage.
/// </summary>
public sealed class MyService
{
    // Non-nullable - compiler guarantees this is never null
    private readonly ILogger<MyService> _logger;
    
    // Nullable - explicitly allows null values
    private string? _optionalValue;

    /// <summary>
    /// Searches for an item by ID.
    /// </summary>
    /// <param name="id">The item identifier.</param>
    /// <returns>The item name if found; otherwise, null.</returns>
    /// <remarks>
    /// Return type is nullable to indicate \"not found\" case.
    /// Null-conditional operator (?.) safely handles null repository results.
    /// </remarks>
    public string? FindItem(int id)
    {
        // ?. returns null if GetById returns null
        return _repository.GetById(id)?.Name;
    }
}
```

### Sealed Classes by Default

```csharp
public sealed class MyService : IMyService { }  // V Default
public abstract class MyBaseClass { }           // V Designed for inheritance
```

### XML Documentation

**All public types and members must have comprehensive XML documentation.**

```csharp
/// <summary>
/// Processes a batch of items.
/// </summary>
/// <param name="items">The items to process.</param>
/// <returns>Operation result with success count and errors.</returns>
/// <exception cref="ArgumentNullException">Thrown when items is null.</exception>
/// <remarks>
/// Design decision: Uses graceful degradation to continue processing after individual failures.
/// Edge case: Empty collection returns success with zero count.
/// Performance: Processes items sequentially; use parallel version for large batches.
/// </remarks>
public async Task<OperationResult> ProcessBatchAsync(IEnumerable<Item> items)
{
    ArgumentNullException.ThrowIfNull(items);
    
    // Implementation...
}
```

**Required XML tags:**
- `<summary>` - Describes WHAT the member does
- `<param>` - For each parameter, explain purpose and valid values
- `<returns>` - Describe return value and possible states
- `<exception>` - Document exceptions that can be thrown
- `<remarks>` - Explain WHY it exists, edge cases, assumptions, design decisions
- `<example>` - (Optional) Usage examples for complex APIs

### Inline Comments

**Explain WHY, not WHAT. The code already shows what it does.**

V **Good inline comments:**
```csharp
public void ProcessOrder(Order order)
{
    // Business rule: Orders over $1000 require manager approval
    if (order.Total > 1000m && !order.IsApproved)
        throw new BusinessException("Manager approval required");
    
    // Workaround: Legacy system requires 24-hour delay before cancellation
    // TODO: Remove when migrated to new order system (Q2 2026)
    if (order.CreatedAt.AddHours(24) > DateTime.UtcNow)
        return;
    
    // Performance: Batch size limited to prevent OOM on large orders
    const int maxBatchSize = 100;
    foreach (var batch in order.Lines.Chunk(maxBatchSize))
    {
        ProcessBatch(batch);
    }
}
```

X **Bad inline comments (stating the obvious):**
```csharp
public void ProcessOrder(Order order)
{
    // Check if total is greater than 1000
    if (order.Total > 1000m)
    {
        // Throw exception
        throw new BusinessException("Too large");
    }
    
    // Loop through lines
    foreach (var line in order.Lines)
    {
        // Process the line
        ProcessLine(line);
    }
}
```

**When to use inline comments:**
- Explaining business rules and domain logic
- Clarifying non-obvious design decisions
- Documenting workarounds and their reasons
- Explaining performance optimizations
- Adding TODO/FIXME with context and timeline
- Clarifying complex algorithms or calculations

### Constants for Magic Values

```csharp
public sealed class DataProcessor
{
    private const string DefaultSourceName = "System";
    private const int MaxRetryAttempts = 3;
    private const double DefaultRate = 1.0;
}
```

### Record Types for DTOs

```csharp
/// <summary>
/// Data transfer object representing an item for API responses.
/// </summary>
/// <param name="Id">The item identifier.</param>
/// <param name="Name">The item name.</param>
/// <param name="Price">The item price.</param>
/// <remarks>
/// Design decision: Use sealed records for DTOs to get immutability, value equality,
/// and concise syntax. Records provide compiler-generated Equals, GetHashCode, and ToString.
/// Sealed prevents inheritance which simplifies serialization and ensures type stability.
/// </remarks>
public sealed record ItemDto(int Id, string Name, decimal Price);

/// <summary>
/// Result of a batch operation with success count and categorized messages.
/// </summary>
/// <param name="SuccessCount">Number of successfully processed items.</param>
/// <param name="Errors">Collection of error messages for failed items.</param>
/// <param name="Warnings">Collection of warning messages for items needing attention.</param>
/// <remarks>
/// Design decision: Separate errors from warnings for graceful degradation.
/// Allows operations to partially succeed while reporting issues.
/// IReadOnlyList prevents modification after creation, enforcing immutability.
/// </remarks>
public sealed record OperationResult(int SuccessCount, IReadOnlyList<string> Errors, IReadOnlyList<string> Warnings);
```

---

## Configuration Standards

**Configuration Rules:**

**Settings Management:**
- V Use strongly-typed configuration with IOptions
- V Store secrets in environment variables or key vault
- V Use appsettings.json for non-sensitive config
- V Use appsettings.{Environment}.json for environment overrides
- X Never commit secrets to source control
- X No hardcoded connection strings

**Structure:**
- V Group related settings into sections
- V Use meaningful section names
- V Document required vs optional settings
- V Provide default values where appropriate

### appsettings.json

```json
{
  "ConnectionStrings": {
    "MyDatabase": "Server=localhost;Database=MyDb;...",
    "ThirdPartyDb": "Server=external;Database=Legacy;..."
  },
  "DatabaseSettings": {
    "Server": "localhost",
    "Database": "MyDb",
    "User": "admin",
    "Password": "enc:...",
    "PasswordEncrypted": true
  },
  "Jwt": {
    "Key": "your-secret-key-minimum-32-characters",
    "Issuer": "MyApp",
    "Audience": "MyApp",
    "ExpiryMinutes": 60
  },
  "AllowedOrigins": ["https://localhost:5001"],
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning"
      }
    }
  }
}
```

### Options Pattern

```csharp
/// <summary>
/// Configuration class for database settings.
/// </summary>
/// <remarks>
/// Strongly-typed configuration prevents typos and provides IntelliSense.
/// Required keyword ensures all properties must be set.
/// Init-only properties make configuration immutable after construction.
/// </remarks>
public sealed class DatabaseSettings
{
    /// <summary>
    /// Gets the database server address.
    /// </summary>
    public required string Server { get; init; }
    
    /// <summary>
    /// Gets the database name.
    /// </summary>
    public required string Database { get; init; }
    
    /// <summary>
    /// Gets the database user.
    /// </summary>
    public required string User { get; init; }
    
    /// <summary>
    /// Gets the database password.
    /// </summary>
    public required string Password { get; init; }
}

// Registration in DI container
// Binds configuration section to strongly-typed class
services.Configure<DatabaseSettings>(configuration.GetSection("DatabaseSettings"));

// Usage in service
/// <summary>
/// Service that uses database settings.
/// </summary>
public sealed class MyService
{
    private readonly DatabaseSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyService"/> class.
    /// </summary>
    /// <param name="settings">The database settings.</param>
    public MyService(IOptions<DatabaseSettings> settings)
    {
        // Extract value from IOptions wrapper
        // Settings are validated and bound from configuration
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }
}
```

---

## Testing Standards (Optional)

**Testing Rules:**

**Coverage:**
- V xUnit for unit testing framework
- V Moq for mocking dependencies
- V FluentAssertions for readable assertions
- V WebApplicationFactory for integration tests
- V Test business logic independently of infrastructure

**Patterns:**
- V Arrange-Act-Assert pattern
- V One assertion per test (when possible)
- V Use test fixtures for shared setup
- V Name tests descriptively: MethodName_Scenario_ExpectedResult
- V Test happy path and edge cases
- V Mock external dependencies

### Unit Tests

```csharp
/// <summary>
/// Unit tests for MyService.
/// </summary>
/// <remarks>
/// Uses xUnit, Moq, and FluentAssertions for clean, readable tests.
/// Follows Arrange-Act-Assert pattern.
/// </remarks>
public class MyServiceTests
{
    /// <summary>
    /// Tests that ProcessDataAsync returns success for valid input.
    /// </summary>
    [Fact]
    public async Task ProcessData_ValidInput_ReturnsSuccess()
    {
        // Arrange - Set up test dependencies and data
        var mockRepo = new Mock<IRepository>();
        mockRepo.Setup(r => r.GetDataAsync(It.IsAny<int>()))
            .ReturnsAsync(new Data { Id = 1, Name = "Test" });

        var service = new MyService(mockRepo.Object);

        // Act - Execute the method under test
        var result = await service.ProcessDataAsync(1);

        // Assert - Verify expected outcomes
        result.IsSuccess.Should().BeTrue(); // FluentAssertions for readability
        result.Errors.Should().BeEmpty();
        
        // Verify repository was called exactly once with correct parameter
        mockRepo.Verify(r => r.GetDataAsync(1), Times.Once);
    }
}
```

### Integration Tests

```csharp
/// <summary>
/// Integration tests for API endpoints using WebApplicationFactory.
/// </summary>
/// <remarks>
/// Design decision: WebApplicationFactory creates in-memory test server for full HTTP testing.
/// IClassFixture shares factory instance across all tests in class for performance.
/// Tests full request pipeline including routing, middleware, and serialization.
/// Use in-memory database or test containers for data layer.
/// </remarks>
public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// Initializes integration test with shared factory.
    /// </summary>
    /// <param name="factory">The web application factory.</param>
    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Tests that GET /api/v1/items returns success status code.
    /// </summary>
    /// <remarks>
    /// Tests full pipeline: routing, authentication, controller, serialization.
    /// EnsureSuccessStatusCode throws if status is not 2xx.
    /// </remarks>
    [Fact]
    public async Task GetItems_ReturnsSuccessStatusCode()
    {
        // Arrange - create HTTP client for test server
        var client = _factory.CreateClient();
        
        // Act - make HTTP request to in-memory server
        var response = await client.GetAsync("/api/v1/items");
        
        // Assert - verify 2xx status code
        response.EnsureSuccessStatusCode();
    }
}
```

---

## Project Checklist

### New Project Setup

- [ ] Three-layer architecture
- [ ] Nullable reference types enabled
- [ ] .NET 10+ target
- [ ] Serilog configured
- [ ] EF Core (self-developed DB) or Dapper (3rd party DB)
- [ ] Password protection (if needed)
- [ ] Configuration validation
- [ ] Structured logging
- [ ] XML documentation on public APIs
- [ ] README.md with setup instructions
- [ ] .gitignore
- [ ] This copilot-instructions.md file

### Project Structure

```
MyProject/
├── .github/
│   └── copilot-instructions.md
├── .gitignore
├── README.md
├── MyProject.sln
├── MyProject_Presentation/
│   ├── Program.cs
│   ├── appsettings.json
│   └── Controllers/Forms/Commands/
├── MyProject_Logic/
│   ├── Services/
│   ├── Handlers/
│   ├── Validation/
│   └── Models/
├── MyProject_DataAccess/
│   ├── DbContext/             # EF Core
│   ├── Repositories/          # Dapper
│   ├── Entities/
│   └── Interfaces/
└── MyProject.Tests/           # Optional
```

---

## Common Patterns

### Startup Health Check

```csharp
/// <summary>
/// Service for checking application health at startup.
/// </summary>
public interface IHealthCheckService
{
    /// <summary>
    /// Checks the health of critical dependencies.
    /// </summary>
    /// <returns>Health status indicating which services are operational.</returns>
    Task<HealthStatus> CheckAsync();
}

/// <summary>
/// Result of a health check operation.
/// </summary>
/// <param name="DatabaseOk">Indicates if database is accessible.</param>
/// <param name="ExternalServiceOk">Indicates if external services are accessible.</param>
public sealed record HealthStatus(bool DatabaseOk, bool ExternalServiceOk);

/// <summary>
/// Health check service implementation.
/// </summary>
/// <remarks>
/// Verifies critical dependencies before application starts serving requests.
/// Helps detect configuration or infrastructure issues early.
/// </remarks>
public sealed class HealthCheckService : IHealthCheckService
{
    /// <summary>
    /// Performs health checks on all critical services.
    /// </summary>
    /// <returns>Health status for all checked services.</returns>
    public async Task<HealthStatus> CheckAsync()
    {
        // Check database connectivity
        var dbOk = await TestDatabaseAsync();
        
        // Check external service availability
        var serviceOk = await TestExternalServiceAsync();
        
        return new HealthStatus(dbOk, serviceOk);
    }
}
```

### Retry Pattern

```csharp
/// <summary>
/// Retries an operation with exponential backoff.
/// </summary>
/// <typeparam name="T">The type of result returned by the operation.</typeparam>
/// <param name="operation">The operation to retry.</param>
/// <param name="maxAttempts">Maximum number of attempts before failing.</param>
/// <param name="delay">Initial delay between retries. Defaults to 1 second.</param>
/// <returns>The result of the successful operation.</returns>
/// <exception cref="Exception">Throws the last exception if all retries are exhausted.</exception>
/// <remarks>
/// Use for transient failures (network timeouts, temporary service unavailability).
/// Not suitable for permanent failures (authentication errors, not found).
/// Consider using Polly library for production scenarios with more sophisticated policies.
/// </remarks>
public static async Task<T> RetryAsync<T>(
    Func<Task<T>> operation,
    int maxAttempts = 3,
    TimeSpan? delay = null)
{
    delay ??= TimeSpan.FromSeconds(1);

    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            // Log retry attempt for diagnostics
            Log.Warning(ex, "Attempt {Attempt} of {Max} failed", attempt, maxAttempts);
            
            // Delay before next attempt to allow transient issues to resolve
            await Task.Delay(delay.Value);
        }
    }

    // Final attempt - let exception propagate if it fails
    return await operation();
}
```

---

**Document Version:** 2.0 (Generalized Template)  
**Last Updated:** 2026-02-06  
**Maintained By:** ScanitechDanmark Development Team (KTE)
