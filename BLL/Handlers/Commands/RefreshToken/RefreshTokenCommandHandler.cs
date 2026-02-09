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

namespace MyBasisWebApi.Logic.Handlers.Commands.RefreshToken;

/// <summary>
/// Handler for processing refresh token commands.
/// </summary>
/// <remarks>
/// Design decision: Implements command side of CQRS pattern using MediatR.
/// Responsible for validating refresh tokens and issuing new access tokens.
/// Returns null on validation failure to indicate invalid refresh token.
/// </remarks>
public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto?>
{
    private readonly UserManager<ApiUser> _userManager;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;
    private readonly JwtSettings _jwtSettings;
    private const string RefreshTokenName = "RefreshToken";

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenCommandHandler"/> class.
    /// </summary>
    /// <param name="userManager">The ASP.NET Core Identity UserManager.</param>
    /// <param name="logger">The logger instance for structured logging.</param>
    /// <param name="jwtSettings">The strongly-typed JWT configuration settings.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public RefreshTokenCommandHandler(
        UserManager<ApiUser> userManager,
        ILogger<RefreshTokenCommandHandler> logger,
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
    /// Handles the refresh token command.
    /// </summary>
    /// <param name="request">The refresh token command containing current tokens.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>New authentication response with fresh tokens if successful; null if refresh token is invalid.</returns>
    /// <remarks>
    /// Token refresh flow:
    /// 1. Extract username from expired access token (validation disabled for expiration)
    /// 2. Find user by username
    /// 3. Verify user ID matches token claim (prevents token hijacking)
    /// 4. Validate refresh token using Identity's token provider
    /// 5. Generate new access token and refresh token
    /// 6. Return new token pair
    /// 
    /// Security: If refresh token is invalid, update security stamp to invalidate all user tokens.
    /// Performance: I/O bound operations (database and token generation).
    /// </remarks>
    public async Task<AuthResponseDto?> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Refresh token attempt for user ID {UserId}", request.TokenData.UserId);

        // Parse JWT token to extract claims (don't validate expiration - that's why we're refreshing)
        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        var tokenContent = jwtSecurityTokenHandler.ReadJwtToken(request.TokenData.Token);

        // Extract email from token claims - this is our user identifier
        var email = tokenContent.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)
            ?.Value;

        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("Refresh token failed: No email claim found in token");
            return null;
        }

        // Find user by email from token
        var user = await _userManager.FindByEmailAsync(email);

        // Security check: Verify user exists and ID matches token claim
        if (user is null || user.Id != request.TokenData.UserId)
        {
            _logger.LogWarning(
                "Refresh token failed for email {Email}. User found: {UserFound}, ID match: {IdMatch}",
                email,
                user != null,
                user?.Id == request.TokenData.UserId);

            return null;
        }

        // Validate refresh token using Identity's token provider
        var isValidRefreshToken = await _userManager.VerifyUserTokenAsync(
            user,
            _jwtSettings.Issuer,
            RefreshTokenName,
            request.TokenData.RefreshToken);

        if (!isValidRefreshToken)
        {
            _logger.LogWarning(
                "Invalid refresh token for user {Email}. Updating security stamp.",
                user.Email);

            // Security: Update security stamp to invalidate all tokens for this user
            await _userManager.UpdateSecurityStampAsync(user);

            return null;
        }

        // Generate new access token with fresh expiration
        var newToken = await GenerateTokenAsync(user, cancellationToken);

        // Generate new refresh token
        var newRefreshToken = await CreateRefreshTokenAsync(user);

        _logger.LogInformation(
            "Refresh token successful for user {Email} with ID {UserId}",
            user.Email,
            user.Id);

        // Return new token pair
        return new AuthResponseDto(
            UserId: user.Id,
            Token: newToken,
            RefreshToken: newRefreshToken);
    }

    /// <summary>
    /// Creates a new refresh token for the user.
    /// </summary>
    /// <param name="user">The user to create a refresh token for.</param>
    /// <returns>The generated refresh token string.</returns>
    /// <remarks>
    /// Business rule: Remove old refresh token before creating new one to prevent token accumulation.
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
