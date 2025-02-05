using BLL.DTO.Users;
using Microsoft.AspNetCore.Identity;

namespace BLL.Interfaces
{
    /// <summary>
    /// Interface for Authentication Manager.
    /// </summary>
    public interface IAuthManager
    {
        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="userDto">The user data transfer object.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of identity errors.</returns>
        Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto);

        /// <summary>
        /// Logs in a user.
        /// </summary>
        /// <param name="loginDto">The login data transfer object.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the authentication response data transfer object.</returns>
        Task<AuthResponseDto> Login(LoginDto loginDto);

        /// <summary>
        /// Creates a new refresh token.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the refresh token as a string.</returns>
        Task<string> CreateRefreshToken();

        /// <summary>
        /// Verifies the refresh token.
        /// </summary>
        /// <param name="request">The authentication response data transfer object.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the authentication response data transfer object.</returns>
        Task<AuthResponseDto> VerifyRefreshToken(AuthResponseDto request);
    }
}