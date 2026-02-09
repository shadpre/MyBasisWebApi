using MyBasisWebApi.Logic.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace MyBasisWebApi.Logic.Interfaces
{
    /// <summary>
    /// Service contract for authentication and authorization operations.
    /// </summary>
    /// <remarks>
    /// Defines operations for user registration, login, and token management.
    /// Implemented by AuthManager which integrates ASP.NET Core Identity with JWT tokens.
    /// 
    /// Authentication flow:
    /// 1. Register: Create new user account
    /// 2. Login: Validate credentials and issue tokens
    /// 3. Use access token until expiration
    /// 4. VerifyRefreshToken: Get new tokens without re-authentication
    /// </remarks>
    public interface IAuthManager
    {
        /// <summary>
        /// Registers a new user account with the specified information.
        /// </summary>
        /// <param name="userDto">The user registration data including name, email, and password.</param>
        /// <returns>
        /// A collection of Identity errors. Empty collection indicates successful registration.
        /// Non-empty collection contains validation errors (e.g., weak password, duplicate email).
        /// </returns>
        /// <remarks>
        /// Business rules:
        /// - Email must be unique (enforced by Identity)
        /// - Password must meet complexity requirements (configured in Identity)
        /// - UserName is automatically set to Email
        /// - New users are assigned "User" role by default
        /// 
        /// Password is hashed using ASP.NET Core Identity's default algorithm (PBKDF2).
        /// </remarks>
        Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto);

        /// <summary>
        /// Authenticates a user and returns JWT tokens.
        /// </summary>
        /// <param name="loginDto">The login credentials (email and password).</param>
        /// <returns>
        /// AuthResponseDto containing access token and refresh token if authentication succeeds.
        /// Null if authentication fails (invalid credentials or user not found).
        /// </returns>
        /// <remarks>
        /// Security: Uses secure password comparison to prevent timing attacks.
        /// Failed login attempts are logged for security monitoring.
        /// 
        /// Access token:
        /// - Short-lived (configurable, default 10 minutes)
        /// - Contains user identity and role claims
        /// - Used for API authorization
        /// 
        /// Refresh token:
        /// - Long-lived (stored in Identity)
        /// - Used to obtain new access tokens without re-authentication
        /// - Can be revoked by updating user's SecurityStamp
        /// </remarks>
        Task<AuthResponseDto> Login(LoginDto loginDto);

        /// <summary>
        /// Creates a new refresh token for the current user.
        /// </summary>
        /// <returns>The generated refresh token string.</returns>
        /// <remarks>
        /// Internal method used during login and token refresh operations.
        /// Removes old refresh token before generating new one to prevent accumulation.
        /// Token is stored in ASP.NET Core Identity's token store.
        /// 
        /// Security: Tokens are cryptographically generated and validated by Identity.
        /// </remarks>
        Task<string> CreateRefreshToken();

        /// <summary>
        /// Verifies a refresh token and issues new access and refresh tokens.
        /// </summary>
        /// <param name="request">The authentication response containing expired access token and current refresh token.</param>
        /// <returns>
        /// New AuthResponseDto with fresh tokens if verification succeeds.
        /// Null if verification fails (invalid refresh token, user mismatch, or token revoked).
        /// </returns>
        /// <remarks>
        /// Token refresh flow:
        /// 1. Extract user identity from expired access token (signature not validated)
        /// 2. Verify user exists and matches token claims
        /// 3. Verify refresh token is valid and matches stored token
        /// 4. Generate new access token and refresh token
        /// 
        /// Security: Failed refresh attempts trigger SecurityStamp update to invalidate all tokens.
        /// This prevents token reuse after potential compromise.
        /// 
        /// Use case: Maintain user session without requiring re-login when access token expires.
        /// </remarks>
        Task<AuthResponseDto> VerifyRefreshToken(AuthResponseDto request);
    }
}