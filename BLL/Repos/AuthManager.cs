using AutoMapper;
using BLL.DTO.Users;
using BLL.Interfaces;
using DAL;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BLL.Repos
{
    /// <summary>
    /// Manages authentication operations.
    /// </summary>
    public class AuthManager : IAuthManager
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApiUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthManager> _logger;
        private ApiUser _user;

        //private const string _loginProvider = "HotelListingApi";
        private const string _refreshToken = "RefreshToken";

        /// <summary>
        /// Initializes a new instance of the AuthManager class.
        /// </summary>
        /// <param name="mapper">The mapper instance.</param>
        /// <param name="userManager">The user manager instance.</param>
        /// <param name="configuration">The configuration instance.</param>
        /// <param name="logger">The logger instance.</param>
        public AuthManager(IMapper mapper, UserManager<ApiUser> userManager, IConfiguration configuration, ILogger<AuthManager> logger)
        {
            _mapper = mapper;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new refresh token.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the refresh token as a string.</returns>
        public async Task<string> CreateRefreshToken()
        {
            await _userManager.RemoveAuthenticationTokenAsync(_user, _configuration["JwtSettings:Issuer"], _refreshToken);
            var newRefreshToken = await _userManager.GenerateUserTokenAsync(_user, _configuration["JwtSettings:Issuer"], _refreshToken);
            var result = await _userManager.SetAuthenticationTokenAsync(_user, _configuration["JwtSettings:Issuer"], _refreshToken, newRefreshToken);
            return newRefreshToken;
        }

        /// <summary>
        /// Logs in a user.
        /// </summary>
        /// <param name="loginDto">The login data transfer object.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the authentication response data transfer object.</returns>
        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
            _logger.LogInformation($"Looking for user with email {loginDto.Email}");
            _user = await _userManager.FindByEmailAsync(loginDto.Email);
            bool isValidUser = await _userManager.CheckPasswordAsync(_user, loginDto.Password);

            if (_user == null || isValidUser == false)
            {
                _logger.LogWarning($"User with email {loginDto.Email} was not found");
                return null;
            }

            var token = await GenerateToken();
            _logger.LogInformation($"Token generated for user with email {loginDto.Email} | Token: {token}");

            return new AuthResponseDto
            {
                Token = token,
                UserId = _user.Id,
                RefreshToken = await CreateRefreshToken()
            };
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="userDto">The user data transfer object.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of identity errors.</returns>
        public async Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto)
        {
            _user = _mapper.Map<ApiUser>(userDto);
            _user.UserName = userDto.Email;

            var result = await _userManager.CreateAsync(_user, userDto.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(_user, "User");
            }

            return result.Errors;
        }

        /// <summary>
        /// Verifies the refresh token.
        /// </summary>
        /// <param name="request">The authentication response data transfer object.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the authentication response data transfer object.</returns>
        public async Task<AuthResponseDto> VerifyRefreshToken(AuthResponseDto request)
        {
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var tokenContent = jwtSecurityTokenHandler.ReadJwtToken(request.Token);
            var username = tokenContent.Claims.ToList().FirstOrDefault(q => q.Type == JwtRegisteredClaimNames.Email)?.Value;
            _user = await _userManager.FindByNameAsync(username);

            if (_user == null || _user.Id != request.UserId)
            {
                return null;
            }

            var isValidRefreshToken = await _userManager.VerifyUserTokenAsync(_user, _configuration["JwtSettings:Issuer"], _refreshToken, request.RefreshToken);

            if (isValidRefreshToken)
            {
                var token = await GenerateToken();
                return new AuthResponseDto
                {
                    Token = token,
                    UserId = _user.Id,
                    RefreshToken = await CreateRefreshToken()
                };
            }

            await _userManager.UpdateSecurityStampAsync(_user);
            return null;
        }

        /// <summary>
        /// Generates a new JWT token.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the JWT token as a string.</returns>
        private async Task<string> GenerateToken()
        {
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));

            var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(_user);
            var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();
            var userClaims = await _userManager.GetClaimsAsync(_user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, _user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, _user.Email),
                new Claim("uid", _user.Id),
            }
            .Union(userClaims).Union(roleClaims);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JwtSettings:DurationInMinutes"])),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}