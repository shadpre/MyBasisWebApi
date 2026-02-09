namespace MyBasisWebApi.Logic.Configuration;

/// <summary>
/// Strongly-typed configuration settings for JWT authentication.
/// </summary>
/// <remarks>
/// Design decision: Use strongly-typed configuration with IOptions pattern to:
/// - Prevent typos in configuration keys
/// - Enable compile-time checking
/// - Support validation at startup
/// Required keyword ensures all properties must be set in configuration.
/// </remarks>
public sealed class JwtSettings
{
    /// <summary>
    /// Gets the JWT signing key used for token generation and validation.
    /// </summary>
    /// <remarks>
    /// Security: Must be at least 32 characters for HS256 algorithm.
    /// Should be stored in user secrets or environment variables, not in appsettings.json.
    /// </remarks>
    public required string Key { get; init; }

    /// <summary>
    /// Gets the issuer claim value for generated tokens.
    /// </summary>
    /// <remarks>
    /// Identifies who issued the token (this application).
    /// Must match between token generation and validation.
    /// </remarks>
    public required string Issuer { get; init; }

    /// <summary>
    /// Gets the audience claim value for generated tokens.
    /// </summary>
    /// <remarks>
    /// Identifies who the token is intended for.
    /// Must match between token generation and validation.
    /// </remarks>
    public required string Audience { get; init; }

    /// <summary>
    /// Gets the token expiration time in minutes.
    /// </summary>
    /// <remarks>
    /// Default: 60 minutes
    /// Shorter expiration improves security but may impact user experience.
    /// Balance between security and convenience.
    /// </remarks>
    public int ExpiryMinutes { get; init; } = 60;

    /// <summary>
    /// Gets the refresh token expiration time in days.
    /// </summary>
    /// <remarks>
    /// Default: 7 days
    /// Refresh tokens allow obtaining new access tokens without re-authentication.
    /// Should be longer than access token but not indefinite.
    /// </remarks>
    public int RefreshTokenExpiryDays { get; init; } = 7;
}
