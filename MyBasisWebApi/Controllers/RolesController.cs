using DAL;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MyBasisWebApi.Controllers
{
    /// <summary>
    /// Controller for managing user roles.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly UserManager<ApiUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// Initializes a new instance of the RolesController class.
        /// </summary>
        /// <param name="userManager">The user manager instance.</param>
        /// <param name="roleManager">The role manager instance.</param>
        public RolesController(UserManager<ApiUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Adds a role to a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="roleName">The name of the role to add.</param>
        /// <returns>An IActionResult representing the result of the operation.</returns>
        [HttpPost("add-role")]
        public async Task<IActionResult> AddRoleToUser(string userId, string roleName)
        {
            // Find the user by their ID.
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // Return a 404 Not Found response if the user is not found.
                return NotFound("User not found");
            }

            // Check if the role exists.
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                return BadRequest("Role does not exist");
            }

            // Add the role to the user.
            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                // Return a 400 Bad Request response if the operation fails.
                return BadRequest(result.Errors);
            }

            // Return a 200 OK response if the role is successfully added.
            return Ok("Role added to user");
        }

        /// <summary>
        /// Removes a role from a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="roleName">The name of the role to remove.</param>
        /// <returns>An IActionResult representing the result of the operation.</returns>
        [HttpPost("remove-role")]
        public async Task<IActionResult> RemoveRoleFromUser(string userId, string roleName)
        {
            // Find the user by their ID.
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // Return a 404 Not Found response if the user is not found.
                return NotFound("User not found");
            }

            // Check if the user is in the specified role.
            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                // Return a 400 Bad Request response if the user is not in the role.
                return BadRequest("User is not in this role");
            }

            // Remove the role from the user.
            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                // Return a 400 Bad Request response if the operation fails.
                return BadRequest(result.Errors);
            }

            // Return a 200 OK response if the role is successfully removed.
            return Ok("Role removed from user");
        }
    }
}