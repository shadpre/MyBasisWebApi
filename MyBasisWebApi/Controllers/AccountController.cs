using MyBasisWebApi.Logic.Models.Users;
using MyBasisWebApi.Logic.Handlers.Commands.RefreshToken;
using MyBasisWebApi.Logic.Handlers.Commands.Register;
using MyBasisWebApi.Logic.Handlers.Queries.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MyBasisWebApi.Presentation.Controllers;

/// <summary>
/// API controller for account-related operations including registration, login, and token refresh.
/// </summary>
/// <remarks>
/// Design decision: Thin controller pattern - delegates all business logic to MediatR handlers.
/// Controller is responsible only for HTTP concerns (routing, status codes, request/response mapping).
/// All validation is handled by FluentValidation through MediatR pipeline.
/// </remarks>
[Route("api/[controller]")]
[ApiController]
public sealed class AccountController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AccountController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountController"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR instance for command/query handling.</param>
    /// <param name="logger">The logger instance for structured logging.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public AccountController(IMediator mediator, ILogger<AccountController> logger)
    {
        // Fail fast - validate dependencies at construction time
        ArgumentNullException.ThrowIfNull(mediator);
        ArgumentNullException.ThrowIfNull(logger);

        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="apiUserDto">The user registration data.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>200 OK if registration succeeds; 400 Bad Request with errors if registration fails.</returns>
    /// <response code="200">User registered successfully.</response>
    /// <response code="400">Registration failed due to validation or business rule violations.</response>
    /// <remarks>
    /// Validation is automatically performed by FluentValidation before handler execution.
    /// Passwords are hashed using ASP.NET Core Identity's default hasher.
    /// All new users are automatically assigned the "User" role.
    /// </remarks>
    [HttpPost]
    [Route("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Register(
        [FromBody] ApiUserDto apiUserDto,
        CancellationToken cancellationToken)
    {
        // Log registration attempt for audit trail (structured logging with email property)
        _logger.LogInformation(
            "Registration attempt for user with email {Email}",
            apiUserDto.Email);

        // Delegate to MediatR handler - controller has no business logic
        var command = new RegisterCommand(apiUserDto);
        var errors = await _mediator.Send(command, cancellationToken);

        // Check if registration succeeded (empty error list)
        if (errors.Count > 0)
        {
            // Add Identity errors to ModelState for standardized error response
            foreach (var error in errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return BadRequest(ModelState);
        }

        return Ok();
    }

    /// <summary>
    /// Authenticates a user and returns JWT tokens.
    /// </summary>
    /// <param name="loginDto">The user login credentials.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>200 OK with authentication tokens if successful; 401 Unauthorized if authentication fails.</returns>
    /// <response code="200">Returns JWT access token and refresh token.</response>
    /// <response code="401">Invalid credentials or user not found.</response>
    /// <remarks>
    /// Validation is automatically performed by FluentValidation before handler execution.
    /// Returns both access token (short-lived) and refresh token (long-lived).
    /// Access token should be included in Authorization header as "Bearer {token}".
    /// Security: Passwords are never logged or returned in responses.
    /// </remarks>
    [HttpPost]
    [Route("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login(
        [FromBody] LoginDto loginDto,
        CancellationToken cancellationToken)
    {
        // Log login attempt for security monitoring (structured logging with email property)
        _logger.LogInformation(
            "Login attempt for user with email {Email}",
            loginDto.Email);

        // Delegate to MediatR handler - controller has no business logic
        var query = new LoginQuery(loginDto);
        var authResponse = await _mediator.Send(query, cancellationToken);

        // Null response indicates authentication failure
        if (authResponse is null)
        {
            _logger.LogWarning(
                "Failed login attempt for email {Email}",
                loginDto.Email);

            return Unauthorized();
        }

        return Ok(authResponse);
    }

    /// <summary>
    /// Refreshes an expired JWT access token using a refresh token.
    /// </summary>
    /// <param name="request">The current authentication response containing tokens.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>200 OK with new tokens if successful; 401 Unauthorized if refresh token is invalid.</returns>
    /// <response code="200">Returns new JWT access token and refresh token.</response>
    /// <response code="401">Invalid or expired refresh token.</response>
    /// <remarks>
    /// Use this endpoint when access token expires to obtain a new token without re-authentication.
    /// Refresh tokens are single-use - each successful refresh issues a new refresh token.
    /// If refresh token is invalid, user must re-authenticate via login endpoint.
    /// Security: Invalid refresh tokens trigger security stamp update to invalidate all user sessions.
    /// </remarks>
    [HttpPost]
    [Route("refreshtoken")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken(
        [FromBody] AuthResponseDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Refresh token attempt for user ID {UserId}",
            request.UserId);

        // Delegate to MediatR handler - controller has no business logic
        var command = new RefreshTokenCommand(request);
        var authResponse = await _mediator.Send(command, cancellationToken);

        // Null response indicates invalid refresh token
        if (authResponse is null)
        {
            _logger.LogWarning(
                "Failed refresh token attempt for user ID {UserId}",
                request.UserId);

            return Unauthorized();
        }

        return Ok(authResponse);
    }
}
