# Quick Reference Guide

Fast reference for common tasks in MyBasisWebApi following ScanitechDanmark standards.

## ?? Quick Commands

### Build & Run
```bash
# Build
dotnet build

# Run
dotnet run --project MyBasisWebApi/MyBasisWebApi_Presentation.csproj

# Watch mode (auto-reload)
dotnet watch run --project MyBasisWebApi/MyBasisWebApi_Presentation.csproj
```

### Database Migrations
```bash
# Add migration
dotnet ef migrations add MigrationName --project DAL/MyBasisWebApi_DataAccess.csproj --startup-project MyBasisWebApi/MyBasisWebApi_Presentation.csproj

# Update database
dotnet ef database update --project DAL/MyBasisWebApi_DataAccess.csproj --startup-project MyBasisWebApi/MyBasisWebApi_Presentation.csproj

# Remove last migration
dotnet ef migrations remove --project DAL/MyBasisWebApi_DataAccess.csproj --startup-project MyBasisWebApi/MyBasisWebApi_Presentation.csproj

# Generate SQL script
dotnet ef migrations script --idempotent --output migration.sql --project DAL/MyBasisWebApi_DataAccess.csproj --startup-project MyBasisWebApi/MyBasisWebApi_Presentation.csproj
```

---

## ?? Code Templates

### Adding a New Command (CQRS)

**1. Create Command** (`BLL/Handlers/Commands/CreateItem/CreateItemCommand.cs`):
```csharp
using MediatR;

namespace MyBasisWebApi.Logic.Handlers.Commands.CreateItem;

/// <summary>
/// Command to create a new item.
/// </summary>
/// <param name="Name">The item name.</param>
/// <param name="Description">The item description.</param>
public sealed record CreateItemCommand(
    string Name, 
    string Description) : IRequest<int>;
```

**2. Create Handler** (`BLL/Handlers/Commands/CreateItem/CreateItemCommandHandler.cs`):
```csharp
using MediatR;
using Microsoft.Extensions.Logging;

namespace MyBasisWebApi.Logic.Handlers.Commands.CreateItem;

/// <summary>
/// Handler for creating a new item.
/// </summary>
public sealed class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, int>
{
    private readonly ILogger<CreateItemCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateItemCommandHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public CreateItemCommandHandler(ILogger<CreateItemCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Handles the create item command.
    /// </summary>
    /// <param name="request">The command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created item ID.</returns>
    public async Task<int> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        // Business logic here
        _logger.LogInformation("Creating item with name {Name}", request.Name);
        
        // TODO: Implement
        await Task.CompletedTask;
        return 1;
    }
}
```

**3. Create Validator** (`BLL/Validation/CreateItemCommandValidator.cs`):
```csharp
using FluentValidation;
using MyBasisWebApi.Logic.Handlers.Commands.CreateItem;

namespace MyBasisWebApi.Logic.Validation;

/// <summary>
/// Validator for CreateItemCommand.
/// </summary>
public sealed class CreateItemCommandValidator : AbstractValidator<CreateItemCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateItemCommandValidator"/> class.
    /// </summary>
    public CreateItemCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
    }
}
```

**4. Add Controller Action**:
```csharp
/// <summary>
/// Creates a new item.
/// </summary>
/// <param name="command">The create item command.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>The created item ID.</returns>
[HttpPost]
[ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<ActionResult<int>> Create(
    [FromBody] CreateItemCommand command,
    CancellationToken cancellationToken)
{
    var id = await _mediator.Send(command, cancellationToken);
    return CreatedAtAction(nameof(GetById), new { id }, id);
}
```

---

### Adding a New Query (CQRS)

**1. Create Query** (`BLL/Handlers/Queries/GetItem/GetItemQuery.cs`):
```csharp
using MediatR;

namespace MyBasisWebApi.Logic.Handlers.Queries.GetItem;

/// <summary>
/// Query to retrieve an item by ID.
/// </summary>
/// <param name="Id">The item identifier.</param>
public sealed record GetItemQuery(int Id) : IRequest<ItemDto?>;

/// <summary>
/// DTO for item response.
/// </summary>
/// <param name="Id">The item ID.</param>
/// <param name="Name">The item name.</param>
public sealed record ItemDto(int Id, string Name);
```

**2. Create Handler** (`BLL/Handlers/Queries/GetItem/GetItemQueryHandler.cs`):
```csharp
using MediatR;
using Microsoft.Extensions.Logging;

namespace MyBasisWebApi.Logic.Handlers.Queries.GetItem;

/// <summary>
/// Handler for retrieving an item by ID.
/// </summary>
public sealed class GetItemQueryHandler : IRequestHandler<GetItemQuery, ItemDto?>
{
    private readonly ILogger<GetItemQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetItemQueryHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public GetItemQueryHandler(ILogger<GetItemQueryHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Handles the get item query.
    /// </summary>
    /// <param name="request">The query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The item DTO if found; otherwise, null.</returns>
    public async Task<ItemDto?> Handle(GetItemQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving item with ID {ItemId}", request.Id);
        
        // TODO: Implement database query
        await Task.CompletedTask;
        return null;
    }
}
```

**3. Add Controller Action**:
```csharp
/// <summary>
/// Retrieves an item by ID.
/// </summary>
/// <param name="id">The item identifier.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>The item if found; otherwise, 404.</returns>
[HttpGet("{id:int}")]
[ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<ItemDto>> GetById(int id, CancellationToken cancellationToken)
{
    var query = new GetItemQuery(id);
    var result = await _mediator.Send(query, cancellationToken);
    return result is null ? NotFound() : Ok(result);
}
```

---

## ??? Architecture Patterns

### Controller Pattern (Thin Controllers)
```csharp
/// <summary>
/// API controller for item management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class ItemsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ItemsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemsController"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR instance.</param>
    /// <param name="logger">The logger instance.</param>
    public ItemsController(IMediator mediator, ILogger<ItemsController> logger)
    {
        ArgumentNullException.ThrowIfNull(mediator);
        ArgumentNullException.ThrowIfNull(logger);
        
        _mediator = mediator;
        _logger = logger;
    }

    // Actions delegate to MediatR - no business logic in controller
}
```

### Service Pattern
```csharp
/// <summary>
/// Service for item business logic.
/// </summary>
public sealed class ItemService : IItemService
{
    private readonly MyDbContext _context;
    private readonly ILogger<ItemService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public ItemService(MyDbContext context, ILogger<ItemService> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);
        
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all items.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of items.</returns>
    public async Task<IReadOnlyList<ItemDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Items
            .AsNoTracking()
            .Select(i => new ItemDto(i.Id, i.Name))
            .ToListAsync(cancellationToken);
    }
}
```

---

## ?? Documentation Standards

### XML Documentation Template
```csharp
/// <summary>
/// Brief description of what this does (one sentence).
/// </summary>
/// <param name="paramName">Describe the parameter purpose and valid values.</param>
/// <returns>Describe what is returned and possible states (null, empty, etc.).</returns>
/// <exception cref="ExceptionType">When this exception is thrown.</exception>
/// <remarks>
/// Design decision: Explain WHY this exists and how it should be used.
/// 
/// Business rule: Document any business logic or constraints.
/// Performance: Note any performance considerations.
/// Edge case: Describe edge cases and how they're handled.
/// </remarks>
/// <example>
/// <code>
/// var result = await MyMethod("example", cancellationToken);
/// </code>
/// </example>
public async Task<Result> MyMethod(string paramName, CancellationToken cancellationToken)
```

### Inline Comment Guidelines
```csharp
public async Task ProcessOrderAsync(Order order)
{
    // Business rule: Orders over $1000 require manager approval
    // This prevents fraud and ensures high-value orders are reviewed
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
        await ProcessBatchAsync(batch);
    }
}
```

---

## ? Code Review Checklist

Before committing, ensure:

- [ ] **XML documentation** on all public types and members
- [ ] **Inline comments** explaining WHY for business logic
- [ ] Classes are **sealed** (unless designed for inheritance)
- [ ] All async methods accept **CancellationToken**
- [ ] Constructor parameters validated with **ArgumentNullException.ThrowIfNull**
- [ ] **Structured logging** (no string interpolation)
- [ ] **DateTime.UtcNow** instead of DateTime.Now
- [ ] No business logic in controllers
- [ ] Validation in service layer (FluentValidation)
- [ ] No direct DbContext in controllers
- [ ] All tests passing
- [ ] Build successful

---

## ?? Common Fixes

### String Interpolation in Logging
? **Wrong:**
```csharp
_logger.LogInformation($"Processing order {orderId}");
```

? **Correct:**
```csharp
_logger.LogInformation("Processing order {OrderId}", orderId);
```

### DateTime Usage
? **Wrong:**
```csharp
var now = DateTime.Now;
```

? **Correct:**
```csharp
var now = DateTime.UtcNow;
```

### Class Sealing
? **Wrong:**
```csharp
public class MyService { }
```

? **Correct:**
```csharp
public sealed class MyService { }
```

### Null Validation
? **Wrong:**
```csharp
if (param == null) throw new ArgumentNullException(nameof(param));
```

? **Correct:**
```csharp
ArgumentNullException.ThrowIfNull(param);
```

---

## ?? URLs

- **Swagger UI:** https://localhost:7000/swagger
- **API Base:** https://localhost:7000/api
- **Logs:** `./logs/log-YYYYMMDD.txt`

---

## ?? Need Help?

1. Check `README.md` for detailed documentation
2. Review `.github/copilot-instructions.md` for coding standards
3. Check `REFACTORING_SUMMARY.md` for recent changes
4. Contact the development team

---

**Quick Reference Version:** 1.0  
**Last Updated:** 2026-02-06
