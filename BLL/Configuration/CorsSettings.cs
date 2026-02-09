namespace MyBasisWebApi.Logic.Configuration;

/// <summary>
/// Strongly-typed configuration settings for CORS (Cross-Origin Resource Sharing).
/// </summary>
/// <remarks>
/// Security: Never use AllowAnyOrigin in production.
/// Explicitly list trusted origins to prevent CSRF attacks.
/// Empty list will block all cross-origin requests.
/// </remarks>
public sealed class CorsSettings
{
    /// <summary>
    /// Gets the list of allowed origins for CORS requests.
    /// </summary>
    /// <remarks>
    /// Security critical: Only include fully trusted origins.
    /// Format: "https://example.com" (include protocol, no trailing slash)
    /// Wildcards are not supported for security reasons.
    /// </remarks>
    public required string[] AllowedOrigins { get; init; }

    /// <summary>
    /// Gets a value indicating whether credentials (cookies, auth headers) are allowed.
    /// </summary>
    /// <remarks>
    /// Default: true for authenticated APIs
    /// Cannot be true when using AllowAnyOrigin (security restriction).
    /// </remarks>
    public bool AllowCredentials { get; init; } = true;

    /// <summary>
    /// Gets the list of allowed HTTP methods.
    /// </summary>
    /// <remarks>
    /// Default: Common REST methods
    /// Restrict to only methods your API actually uses.
    /// </remarks>
    public string[] AllowedMethods { get; init; } = new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS" };

    /// <summary>
    /// Gets the list of allowed request headers.
    /// </summary>
    /// <remarks>
    /// Default: Common headers including Authorization
    /// Add custom headers as needed (e.g., "X-Custom-Header").
    /// </remarks>
    public string[] AllowedHeaders { get; init; } = new[] { "Content-Type", "Authorization", "Accept", "X-Requested-With" };

    /// <summary>
    /// Gets the maximum age (in seconds) for preflight cache.
    /// </summary>
    /// <remarks>
    /// Default: 1 hour (3600 seconds)
    /// Browsers cache preflight responses to reduce OPTIONS requests.
    /// Longer values improve performance but delay policy changes.
    /// </remarks>
    public int MaxAgeSeconds { get; init; } = 3600;
}
