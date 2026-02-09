using AutoMapper;
using MyBasisWebApi.Logic.Models.Users;
using MyBasisWebApi.Logic.Interfaces;
using MyBasisWebApi.DataAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging; 
using Microsoft.IdentityModel.Tokens; 
using System.IdentityModel.Tokens.Jwt; 
using System.Security.Claims; 
using System.Text; 

namespace MyBasisWebApi.Logic.Services.Authentication
{
    /// <summary>
    /// Manages authentication operations.
    /// </summary>
    public class AuthManager : IAuthManager
    {
        private readonly IMapper _mapper; // Mapper instance for object mapping
        private readonly UserManager<ApiUser> _userManager; // User manager instance for managing users
        private readonly IConfiguration _configuration; // Configuration instance for accessing configuration settings
        private readonly ILogger<AuthManager> _logger; // Logger instance for logging
        private ApiUser _user; // Private field to store the current user

        //private const string _loginProvider = "HotelListingApi"; // Commented out constant for login provider
        private const string _refreshToken = "RefreshToken"; // Constant for refresh token

        /// <summary>
        /// Initializes a new instance of the AuthManager class.
        /// </summary>
        /// <param name="mapper">The mapper instance.</param>
        /// <param name="userManager">The user manager instance.</param>
        /// <param name="configuration">The configuration instance.</param>
        /// <param name="logger">The logger instance.</param>
        public AuthManager(IMapper mapper, UserManager<ApiUser> userManager, IConfiguration configuration, ILogger<AuthManager> logger)
        {
            _mapper = mapper; // Assign the mapper instance
            _userManager = userManager; // Assign the user manager instance
            _configuration = configuration; // Assign the configuration instance
            _logger = logger; // Assign the logger instance
        }

        /// <summary>
        /// Creates a new refresh token.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the refresh token as a string.</returns>
        public async Task<string> CreateRefreshToken()
        {
            await _userManager.RemoveAuthenticationTokenAsync(_user, _configuration["JwtSettings:Issuer"], _refreshToken); // Remove existing refresh token
            var newRefreshToken = await _userManager.GenerateUserTokenAsync(_user, _configuration["JwtSettings:Issuer"], _refreshToken); // Generate new refresh token
            var result = await _userManager.SetAuthenticationTokenAsync(_user, _configuration["JwtSettings:Issuer"], _refreshToken, newRefreshToken); // Set new refresh token
            return newRefreshToken; // Return new refresh token
        }

        /// <summary>
        /// Logs in a user.
        /// </summary>
        /// <param name="loginDto">The login data transfer object.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the authentication response data transfer object.</returns>
        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
            _logger.LogInformation($"Looking for user with email {loginDto.Email}"); // Log information about user lookup
            _user = await _userManager.FindByEmailAsync(loginDto.Email); // Find user by email
            bool isValidUser = await _userManager.CheckPasswordAsync(_user, loginDto.Password); // Check if password is valid

            if (_user == null || isValidUser == false) // If user is not found or password is invalid
            {
                _logger.LogWarning($"User with email {loginDto.Email} was not found"); // Log warning about user not found
                return null; // Return null indicating login failure
            }

            var token = await GenerateToken(); // Generate JWT token
            _logger.LogInformation($"Token generated for user with email {loginDto.Email} | Token: {token}"); // Log information about token generation

            // Use record constructor syntax (required for records with positional parameters)
            return new AuthResponseDto(
                UserId: _user.Id,
                Token: token,
                RefreshToken: await CreateRefreshToken()
            );
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="userDto">The user data transfer object.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of identity errors.</returns>
        public async Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto)
        {
            _user = _mapper.Map<ApiUser>(userDto); // Map user DTO to ApiUser entity
            _user.UserName = userDto.Email; // Set username to email

            var result = await _userManager.CreateAsync(_user, userDto.Password); // Create new user with password

            if (result.Succeeded) // If user creation succeeded
            {
                await _userManager.AddToRoleAsync(_user, "User"); // Add user to "User" role
            }

            return result.Errors; // Return any identity errors that occurred during user creation
        }

        /// <summary>
        /// Verifies the refresh token.
        /// </summary>
        /// <param name="request">The authentication response data transfer object.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the authentication response data transfer object.</returns>
        public async Task<AuthResponseDto> VerifyRefreshToken(AuthResponseDto request)
        {
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler(); // Create JWT security token handler
            var tokenContent = jwtSecurityTokenHandler.ReadJwtToken(request.Token); // Read JWT token content
            var username = tokenContent.Claims.ToList().FirstOrDefault(q => q.Type == JwtRegisteredClaimNames.Email)?.Value; // Extract username from token claims
            _user = await _userManager.FindByNameAsync(username); // Find user by username

            if (_user == null || _user.Id != request.UserId) // If user is not found or ID does not match request ID
            {
                return null; // Return null indicating verification failure
            }

            var isValidRefreshToken = await _userManager.VerifyUserTokenAsync(_user, _configuration["JwtSettings:Issuer"], _refreshToken, request.RefreshToken); // Verify refresh token

            if (isValidRefreshToken) // If refresh token is valid
            {
                var token = await GenerateToken(); // Generate new JWT token
                
                // Use record constructor syntax (required for records with positional parameters)
                return new AuthResponseDto(
                    UserId: _user.Id,
                    Token: token,
                    RefreshToken: await CreateRefreshToken()
                );
            }

            await _userManager.UpdateSecurityStampAsync(_user); // Update security stamp for user
            return null; // Return null indicating verification failure
        }

        /// <summary>
        /// Generates a new JWT token.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the JWT token as a string.</returns>
        private async Task<string> GenerateToken()
        {
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"])); // Create symmetric security key from configuration

            var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256); // Create signing credentials with security key and HMAC-SHA256 algorithm

            var roles = await _userManager.GetRolesAsync(_user); // Get roles for user
            var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList(); // Create claims for roles
            var userClaims = await _userManager.GetClaimsAsync(_user); // Get claims for user

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, _user.Email), // Add subject claim with user email
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Add JWT ID claim with new GUID
                new Claim(JwtRegisteredClaimNames.Email, _user.Email), // Add email claim with user email
                new Claim("uid", _user.Id), // Add user ID claim with user ID
            }
            .Union(userClaims).Union(roleClaims); // Combine user claims and role claims

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"], // Set token issuer from configuration
                audience: _configuration["JwtSettings:Audience"], // Set token audience from configuration
                claims: claims, // Set token claims
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JwtSettings:DurationInMinutes"])), // Set token expiration time from configuration
                signingCredentials: credentials // Set token signing credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token); // Write and return JWT token
        }
    }
}