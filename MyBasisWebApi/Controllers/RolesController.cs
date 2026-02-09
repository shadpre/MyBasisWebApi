using MyBasisWebApi.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MyBasisWebApi.Presentation.Controllers
{
    /// <summary>
    /// API controller for managing user role assignments.
    /// </summary>
    /// <remarks>
    /// Design decision: Thin controller pattern - delegates to ASP.NET Core Identity UserManager and RoleManager.
    /// Business logic for role management is simple enough to use Identity directly rather than creating service layer.
    /// 
    /// Security: Should be protected with [Authorize(Roles = "Admin")] in production.
    /// Role management operations should only be accessible to administrators.
    /// 
    /// Use cases:
    /// - Admin assigns roles to users (e.g., promote user to Admin)
    /// - Admin revokes roles from users
    /// - Role changes take effect immediately (no need to refresh token)
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    // TODO: Add [Authorize(Roles = "Admin")] for production to restrict access to administrators only
    public sealed class RolesController : ControllerBase
    {
        private readonly UserManager<ApiUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RolesController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RolesController"/> class.
        /// </summary>
        /// <param name="userManager">The ASP.NET Core Identity UserManager for user operations.</param>
        /// <param name="roleManager">The ASP.NET Core Identity RoleManager for role operations.</param>
        /// <param name="logger">The logger instance for structured logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public RolesController(
            UserManager<ApiUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            ILogger<RolesController> logger)
        {
            // Fail fast - validate all dependencies at construction time
            ArgumentNullException.ThrowIfNull(userManager);
            ArgumentNullException.ThrowIfNull(roleManager);
            ArgumentNullException.ThrowIfNull(logger);

            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        /// <summary>
        /// Adds a role to a user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="roleName">The name of the role to assign (e.g., "Admin", "User").</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>200 OK if role added successfully; 404 if user not found; 400 if role doesn't exist.</returns>
        /// <response code="200">Role successfully assigned to user.</response>
        /// <response code="400">Role does not exist or operation failed.</response>
        /// <response code="404">User not found.</response>
        /// <remarks>
        /// Business rules:
        /// - User must exist (verified by user ID)
        /// - Role must exist in the system (predefined in RoleConfiguration)
        /// - User can have multiple roles
        /// - Adding existing role is idempotent (no error if user already has role)
        /// 
        /// Security: This endpoint should require Admin role authorization in production.
        /// Role changes are persisted immediately and don't require user re-login.
        /// </remarks>
        [HttpPost("add-role")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddRoleToUser(
            [FromQuery] string userId, 
            [FromQuery] string roleName,
            CancellationToken cancellationToken)
        {
            // Log role assignment attempt for audit trail
            _logger.LogInformation(
                "Attempting to add role {RoleName} to user {UserId}",
                roleName,
                userId);

            // Validate user exists
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning("Cannot add role: User {UserId} not found", userId);
                return NotFound(new { error = "User not found" });
            }

            // Validate role exists (predefined roles from RoleConfiguration)
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                _logger.LogWarning("Cannot add role: Role {RoleName} does not exist", roleName);
                return BadRequest(new { error = $"Role '{roleName}' does not exist" });
            }

            // Add role to user using Identity
            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                // Log failure with Identity errors for troubleshooting
                _logger.LogError(
                    "Failed to add role {RoleName} to user {UserId}. Errors: {Errors}",
                    roleName,
                    userId,
                    string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}")));

                return BadRequest(new { errors = result.Errors });
            }

            _logger.LogInformation(
                "Successfully added role {RoleName} to user {UserId}",
                roleName,
                userId);

            return Ok(new { message = $"Role '{roleName}' added to user successfully" });
        }

        /// <summary>
        /// Removes a role from a user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="roleName">The name of the role to remove.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>200 OK if role removed successfully; 404 if user not found; 400 if user doesn't have role.</returns>
        /// <response code="200">Role successfully removed from user.</response>
        /// <response code="400">User does not have this role or operation failed.</response>
        /// <response code="404">User not found.</response>
        /// <remarks>
        /// Business rules:
        /// - User must exist (verified by user ID)
        /// - User must currently have the role to remove it
        /// - Removing non-existent role returns error (not idempotent)
        /// 
        /// Security: This endpoint should require Admin role authorization in production.
        /// Cannot remove last Admin role if it would leave system without administrators.
        /// Role changes are persisted immediately and don't require user re-login.
        /// </remarks>
        [HttpPost("remove-role")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveRoleFromUser(
            [FromQuery] string userId, 
            [FromQuery] string roleName,
            CancellationToken cancellationToken)
        {
            // Log role removal attempt for audit trail
            _logger.LogInformation(
                "Attempting to remove role {RoleName} from user {UserId}",
                roleName,
                userId);

            // Validate user exists
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning("Cannot remove role: User {UserId} not found", userId);
                return NotFound(new { error = "User not found" });
            }

            // Check if user has the role before attempting removal
            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                _logger.LogWarning(
                    "Cannot remove role: User {UserId} is not in role {RoleName}",
                    userId,
                    roleName);

                return BadRequest(new { error = $"User is not in role '{roleName}'" });
            }

            // Remove role from user using Identity
            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                // Log failure with Identity errors for troubleshooting
                _logger.LogError(
                    "Failed to remove role {RoleName} from user {UserId}. Errors: {Errors}",
                    roleName,
                    userId,
                    string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}")));

                return BadRequest(new { errors = result.Errors });
            }

            _logger.LogInformation(
                "Successfully removed role {RoleName} from user {UserId}",
                roleName,
                userId);

            return Ok(new { message = $"Role '{roleName}' removed from user successfully" });
        }
    }
}