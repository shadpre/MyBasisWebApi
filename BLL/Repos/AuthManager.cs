using AutoMapper;
using MyBasisWebApi.Logic.Configuration;
using MyBasisWebApi.Logic.Models.Users;
using MyBasisWebApi.Logic.Interfaces;
using MyBasisWebApi.DataAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging; 
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens; 
using System.IdentityModel.Tokens.Jwt; 
using System.Security.Claims; 
using System.Text;

namespace MyBasisWebApi.Logic.Services.Authentication
{
    /// <summary>
    /// Service for managing authentication operations including login, registration, and token refresh.
    /// </summary>
    /// <remarks>
    /// Implements authentication business logic using ASP.NET Core Identity and JWT tokens.
    /// Responsible for:
    /// - User registration and role assignment
    /// - Credential validation and login
    /// - JWT token generation
    /// - Refresh token management
    /// 
    /// Security: Passwords are never logged or stored in plain text.
    /// Identity handles password hashing using industry-standard algorithms.
    /// </remarks>
    public sealed class AuthManager : IAuthManager
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApiUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthManager> _logger;
        
        // Stores current user context during authentication operations
        private ApiUser _user;

        private const string RefreshToken = "RefreshToken";

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthManager"/> class.
        /// </summary>
        /// <param name="mapper">The AutoMapper instance for DTO to entity mapping.</param>
        /// <param name="userManager">The ASP.NET Core Identity UserManager for user operations.</param>
        /// <param name="jwtSettings">The strongly-typed JWT configuration settings.</param>
        /// <param name="logger">The logger instance for structured logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public AuthManager(
            IMapper mapper, 
            UserManager<ApiUser> userManager, 
            IOptions<JwtSettings> jwtSettings, 
            ILogger<AuthManager> logger)
        {
            // Fail fast - validate all dependencies at construction time
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(userManager);
            ArgumentNullException.ThrowIfNull(jwtSettings);
            ArgumentNullException.ThrowIfNull(logger);

            _mapper = mapper;
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new refresh token for the current user.
        /// </summary>
        /// <returns>The generated refresh token string.</returns>
        /// <remarks>
        /// Refresh tokens are long-lived tokens used to obtain new access tokens without re-authentication.
        /// Old refresh token is removed before generating new one to prevent token reuse attacks.
        /// Stored in ASP.NET Core Identity's token store.
        /// </remarks>
        public async Task<string> CreateRefreshToken()
        {
            // Remove old refresh token to prevent accumulation and potential reuse
            await _userManager.RemoveAuthenticationTokenAsync(_user, _jwtSettings.Issuer, RefreshToken);
            
            // Generate new cryptographically secure refresh token
            var newRefreshToken = await _userManager.GenerateUserTokenAsync(_user, _jwtSettings.Issuer, RefreshToken);
            
            // Store refresh token for verification during refresh operations
            await _userManager.SetAuthenticationTokenAsync(_user, _jwtSettings.Issuer, RefreshToken, newRefreshToken);
            
            return newRefreshToken;
        }

        /// <summary>
        /// Authenticates a user and returns JWT tokens.
        /// </summary>
        /// <param name="loginDto">The login credentials containing email and password.</param>
        /// <returns>AuthResponseDto with tokens if successful; null if authentication fails.</returns>
        /// <remarks>
        /// Authentication flow:
        /// 1. Find user by email
        /// 2. Verify password using Identity's secure comparison
        /// 3. Generate access token (JWT) with claims
        /// 4. Generate refresh token for token renewal
        /// 
        /// Security: Password is never logged or exposed in responses.
        /// Failed attempts are logged for security monitoring.
        /// </remarks>
        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
            // Log login attempt for security audit trail (structured logging)
            _logger.LogInformation("Login attempt for user with email {Email}", loginDto.Email);
            
            // Find user by email (UserName is set to Email in this system)
            _user = await _userManager.FindByEmailAsync(loginDto.Email);
            
            // Verify password using Identity's secure comparison (timing-attack safe)
            bool isValidUser = _user != null && await _userManager.CheckPasswordAsync(_user, loginDto.Password);

            if (!isValidUser)
            {
                // Log failed attempt for security monitoring (potential brute force attack)
                _logger.LogWarning("Login failed for user with email {Email}", loginDto.Email);
                return null;
            }

            // Generate short-lived access token with user identity and roles
            var token = await GenerateToken();
            
            _logger.LogInformation("User {Email} logged in successfully", loginDto.Email);

            return new AuthResponseDto(
                UserId: _user.Id,
                Token: token,
                RefreshToken: await CreateRefreshToken()
            );
        }

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="userDto">The user registration data.</param>
        /// <returns>Collection of Identity errors. Empty if registration succeeded.</returns>
        /// <remarks>
        /// Registration flow:
        /// 1. Map DTO to entity
        /// 2. Set username to email (business rule)
        /// 3. Create user with hashed password
        /// 4. Assign default "User" role
        /// 
        /// Business rule: All new users are assigned the "User" role automatically.
        /// Password is hashed using Identity's default hasher (PBKDF2 with salt).
        /// </remarks>
        public async Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto)
        {
            // Map DTO to entity using AutoMapper profile
            _user = _mapper.Map<ApiUser>(userDto);
            
            // Business rule: Username is always the email address in this system
            _user.UserName = userDto.Email;

            // Create user with password - Identity handles hashing and validation
            var result = await _userManager.CreateAsync(_user, userDto.Password);

            if (result.Succeeded)
            {
                // Business rule: All new users get "User" role for basic access
                await _userManager.AddToRoleAsync(_user, "User");
                
                _logger.LogInformation("User {Email} registered successfully with ID {UserId}", _user.Email, _user.Id);
            }
            else
            {
                // Log registration failure for troubleshooting
                _logger.LogWarning("Registration failed for {Email}. Errors: {Errors}", 
                    userDto.Email, 
                    string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}")));
            }

            return result.Errors;
        }

        /// <summary>
        /// Verifies a refresh token and issues new tokens.
        /// </summary>
        /// <param name="request">The authentication response containing current tokens.</param>
        /// <returns>New tokens if refresh succeeds; null if validation fails.</returns>
        /// <remarks>
        /// Refresh flow:
        /// 1. Extract user identity from expired access token
        /// 2. Verify user exists and matches token claims
        /// 3. Verify refresh token is valid and matches stored token
        /// 4. Generate new access token and refresh token
        /// 
        /// Security: Invalidates security stamp on failure to prevent token reuse.
        /// Expired access tokens can still be decoded to extract user identity.
        /// </remarks>
        public async Task<AuthResponseDto> VerifyRefreshToken(AuthResponseDto request)
        {
            // Decode JWT to extract user identity (doesn't validate expiration for refresh)
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var tokenContent = jwtSecurityTokenHandler.ReadJwtToken(request.Token);
            
            // Extract email from token claims
            var username = tokenContent.Claims.FirstOrDefault(q => q.Type == JwtRegisteredClaimNames.Email)?.Value;
            
            if (string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("Refresh token verification failed: No email claim in token");
                return null;
            }

            // Find user by username (email)
            _user = await _userManager.FindByNameAsync(username);

            // Validate user exists and matches token claims
            if (_user == null || _user.Id != request.UserId)
            {
                _logger.LogWarning("Refresh token verification failed: User mismatch or not found");
                return null;
            }

            // Verify refresh token matches stored token and hasn't been revoked
            var isValidRefreshToken = await _userManager.VerifyUserTokenAsync(
                _user, 
                _jwtSettings.Issuer, 
                RefreshToken, 
                request.RefreshToken);

            if (isValidRefreshToken)
            {
                // Generate new tokens for continued access
                var token = await GenerateToken();
                
                _logger.LogInformation("Tokens refreshed for user {Email}", _user.Email);
                
                return new AuthResponseDto(
                    UserId: _user.Id,
                    Token: token,
                    RefreshToken: await CreateRefreshToken()
                );
            }

            // Security: Update security stamp to invalidate all tokens on failed refresh
            // Prevents potential token theft or replay attacks
            await _userManager.UpdateSecurityStampAsync(_user);
            
            _logger.LogWarning("Refresh token verification failed for user {Email}", _user.Email);
            return null;
        }

        /// <summary>
        /// Generates a JWT access token for the current user.
        /// </summary>
        /// <returns>The signed JWT token as a string.</returns>
        /// <remarks>
        /// Token structure:
        /// - Subject: User email
        /// - JWT ID: Unique token identifier
        /// - Email: User email claim
        /// - User ID: Custom claim for user identification
        /// - Roles: User's assigned roles for authorization
        /// - User claims: Additional custom claims
        /// 
        /// Token is signed with HMAC-SHA256 using secret key from configuration.
        /// Expiration is configured in JwtSettings.ExpiryMinutes.
        /// </remarks>
        private async Task<string> GenerateToken()
        {
            // Create signing key from secret (minimum 32 characters for HS256)
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            
            // Configure signing algorithm (HMAC-SHA256)
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Get user's roles for authorization claims
            var roles = await _userManager.GetRolesAsync(_user);
            var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();
            
            // Get additional custom claims if any
            var userClaims = await _userManager.GetClaimsAsync(_user);

            // Build claims collection with standard and custom claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, _user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, _user.Email),
                new Claim("uid", _user.Id), // Custom claim for user ID lookup
            }
            .Union(userClaims)
            .Union(roleClaims);

            // Create JWT with all claims and expiration
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes), // Use UTC for consistency
                signingCredentials: credentials
            );

            // Serialize token to string for HTTP header transmission
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}