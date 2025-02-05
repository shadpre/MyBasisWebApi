namespace BLL.DTO.Users
{
    /// <summary>
    /// Data Transfer Object for Authentication Response.
    /// </summary>
    public class AuthResponseDto
    {
        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        public string RefreshToken { get; set; }
    }
}