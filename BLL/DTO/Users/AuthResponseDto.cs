namespace MyBasisWebApi.Logic.Models.Users;

/// <summary>
/// Data transfer object for authentication response containing JWT tokens.
/// </summary>
/// <param name="UserId">The unique identifier of the authenticated user.</param>
/// <param name="Token">The JWT access token for API authentication.</param>
/// <param name="RefreshToken">The refresh token for obtaining new access tokens.</param>
/// <remarks>
/// Design decision: Use sealed record for immutability and value equality.
/// Token should be included in Authorization header as "Bearer {Token}".
/// RefreshToken should be stored securely and used only when Token expires.
/// Security: Never log or expose tokens in client-side code beyond secure storage.
/// </remarks>
public sealed record AuthResponseDto(
    string UserId,
    string Token,
    string RefreshToken);