using AutoMapper;
using MyBasisWebApi.Logic.Models.Users;
using MyBasisWebApi.DataAccess;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace MyBasisWebApi.Logic.Handlers.Commands.Register;

/// <summary>
/// Handler for processing user registration commands.
/// </summary>
/// <remarks>
/// Design decision: Implements the command side of CQRS pattern using MediatR.
/// Responsible for orchestrating user registration business logic.
/// Uses ASP.NET Core Identity for user management and role assignment.
/// </remarks>
public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, IReadOnlyList<IdentityError>>
{
    private readonly IMapper _mapper;
    private readonly UserManager<ApiUser> _userManager;
    private readonly ILogger<RegisterCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterCommandHandler"/> class.
    /// </summary>
    /// <param name="mapper">The AutoMapper instance for DTO-to-entity mapping.</param>
    /// <param name="userManager">The ASP.NET Core Identity UserManager for user operations.</param>
    /// <param name="logger">The logger instance for structured logging.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public RegisterCommandHandler(
        IMapper mapper,
        UserManager<ApiUser> userManager,
        ILogger<RegisterCommandHandler> logger)
    {
        // Fail fast - validate all dependencies at construction time
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(logger);

        _mapper = mapper;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Handles the user registration command.
    /// </summary>
    /// <param name="request">The registration command containing user data.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A read-only list of identity errors. Empty list indicates success.</returns>
    /// <remarks>
    /// Business logic:
    /// 1. Maps DTO to entity
    /// 2. Sets username to email (business rule)
    /// 3. Creates user with password using Identity
    /// 4. Assigns default "User" role if creation succeeds
    /// 5. Returns any validation or creation errors
    /// 
    /// Performance: Identity operations are async and I/O bound (database operations).
    /// </remarks>
    public async Task<IReadOnlyList<IdentityError>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        // Log registration attempt with user email for audit trail
        _logger.LogInformation(
            "Registration attempt for user with email {Email}",
            request.UserData.Email);

        // Map DTO to entity - separates API contract from domain model
        var user = _mapper.Map<ApiUser>(request.UserData);

        // Business rule: Username is always the email address for this system
        user.UserName = request.UserData.Email;

        // Create user with password using ASP.NET Core Identity
        // Identity handles password hashing, validation, and user storage
        var result = await _userManager.CreateAsync(user, request.UserData.Password);

        if (result.Succeeded)
        {
            // Business rule: All new users are assigned the "User" role by default
            await _userManager.AddToRoleAsync(user, "User");

            _logger.LogInformation(
                "User {Email} registered successfully with ID {UserId}",
                user.Email,
                user.Id);

            // Return empty list to indicate success
            return Array.Empty<IdentityError>();
        }

        // Log registration failure for troubleshooting
        _logger.LogWarning(
            "Registration failed for user {Email}. Errors: {Errors}",
            request.UserData.Email,
            string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}")));

        // Return errors as read-only list to prevent modification
        return result.Errors.ToList().AsReadOnly();
    }
}
