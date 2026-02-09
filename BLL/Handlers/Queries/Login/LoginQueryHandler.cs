using MyBasisWebApi.Logic.Configuration;
using MyBasisWebApi.Logic.Models.Users;
using MyBasisWebApi.DataAccess;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyBasisWebApi.Logic.Handlers.Queries.Login;

/// <summary>
/// Handler for processing user login queries.
/// </summary>
/// <remarks>
/// Design decision: Implements query side of CQRS pattern using MediatR.
/// Responsible for user authentication and JWT token generation.
/// Returns null on authentication failure to indicate invalid credentials.
/// </remarks>
public sealed class LoginQueryHandler : IRequestHandler<LoginQuery, AuthResponseDto?>
{
    private readonly UserManager<ApiUser> _userManager;
    private readonly ILogger<LoginQueryHandler> _logger;
    private readonly JwtSettings _jwtSettings;
    private const string RefreshTokenName = "RefreshToken";

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginQueryHandler"/> class.
    /// </summary>
    /// <param name="userManager">The ASP.NET Core Identity UserManager.</param>
    /// <param name="logger">The logger instance for structured logging.</param>
    /// <param name="jwtSettings">The strongly-typed JWT configuration settings.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public LoginQueryHandler(
        UserManager<ApiUser> userManager,
        ILogger<LoginQueryHandler> logger,
        IOptions<JwtSettings> jwtSettings)
    {
        // Fail fast - validate all dependencies at construction time
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(jwtSettings);

        _userManager = userManager;
        _logger = logger;
        _jwtSettings = jwtSettings.Value;
    }

    /// <summary>
    /// Handles the user login query.
    /// </summary>
    /// <param name="request">The login query containing user credentials.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>Authentication response with tokens if successful; null if authentication fails.</returns>
    /// <remarks>
    /// Authentication flow:
    /// 1. Find user by email
    /// 2. Verify password using Identity's secure password hasher
    /// 3. Generate JWT access token with user claims
    /// 4. Generate refresh token for token renewal
    /// 5. Return both tokens in response
    /// 
    /// Security: Passwords are never logged or exposed.
    /// Performance: I/O bound operations (database and token generation).
    /// </remarks>
    public async Task<AuthResponseDto?> Handle(
        LoginQuery request,
        CancellationToken cancellationToken)
    {
        // Log authentication attempt for audit trail (do not log password)
        _logger.LogInformation(
            "Login attempt for user with email {Email}",
            request.LoginData.Email);

        // Find user by email - business rule: email is unique identifier
        var user = await _userManager.FindByEmailAsync(request.LoginData.Email);

        // Verify password using Identity's secure hash comparison
        var isValidPassword = user != null && await _userManager.CheckPasswordAsync(user, request.LoginData.Password);

        if (user is null || !isValidPassword)
        {
            // Log failed authentication for security monitoring
            _logger.LogWarning(
                "Failed login attempt for email {Email}. User found: {UserFound}, Valid password: {ValidPassword}",
                request.LoginData.Email,
                user != null,
                isValidPassword);

            // Return null to indicate authentication failure - don't reveal which part failed
            return null;
        }

        // Generate JWT access token with user claims
        var token = await GenerateTokenAsync(user, cancellationToken);

        // Generate refresh token for obtaining new access tokens
        var refreshToken = await CreateRefreshTokenAsync(user);

        _logger.LogInformation(
            "User {Email} logged in successfully with ID {UserId}",
            user.Email,
            user.Id);

        // Return authentication response with both tokens
        return new AuthResponseDto(
            UserId: user.Id,
            Token: token,
            RefreshToken: refreshToken);
    }

    /// <summary>
    /// Creates a new refresh token for the user.
    /// </summary>
    /// <param name="user">The user to create a refresh token for.</param>
    /// <returns>The generated refresh token string.</returns>
    /// <remarks>
    /// Business rule: Remove old refresh token before creating new one to prevent token reuse.
    /// Refresh tokens are stored by Identity's token provider.
    /// </remarks>
    private async Task<string> CreateRefreshTokenAsync(ApiUser user)
    {
        // Remove existing refresh token to prevent accumulation
        await _userManager.RemoveAuthenticationTokenAsync(
            user,
            _jwtSettings.Issuer,
            RefreshTokenName);

        // Generate new refresh token using Identity's token provider
        var newRefreshToken = await _userManager.GenerateUserTokenAsync(
            user,
            _jwtSettings.Issuer,
            RefreshTokenName);

        // Store new refresh token for future validation
        await _userManager.SetAuthenticationTokenAsync(
            user,
            _jwtSettings.Issuer,
            RefreshTokenName,
            newRefreshToken);

        return newRefreshToken;
    }

    /// <summary>
    /// Generates a JWT access token for the authenticated user.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The JWT token string.</returns>
    /// <remarks>
    /// Token includes:
    /// - Standard claims (sub, jti, email)
    /// - User ID claim for authorization
    /// - Role claims for role-based access control
    /// - Custom user claims if any
    /// 
    /// Security: Token is signed with HMAC-SHA256 using configured secret key.
    /// </remarks>
    private async Task<string> GenerateTokenAsync(ApiUser user, CancellationToken cancellationToken)
    {
        // Create signing key from configuration
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Retrieve user roles for role-based authorization
        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

        // Retrieve custom user claims if any
        var userClaims = await _userManager.GetClaimsAsync(user);

        // Build standard JWT claims
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email!), // Subject - user identifier
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID - unique token identifier
            new Claim(JwtRegisteredClaimNames.Email, user.Email!), // Email claim
            new Claim("uid", user.Id), // Custom user ID claim for convenience
        }
        .Union(userClaims) // Add custom user claims
        .Union(roleClaims); // Add role claims

        // Create JWT token with all claims and expiration
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            // Use UTC to avoid timezone issues
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: credentials);

        // Serialize token to string for HTTP transmission
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
