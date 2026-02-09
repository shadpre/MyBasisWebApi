using Microsoft.AspNetCore.Identity;

namespace MyBasisWebApi.DataAccess
{
    /// <summary>
    /// Represents an API user in the authentication and authorization system.
    /// </summary>
    /// <remarks>
    /// Design decision: Inherits from ASP.NET Core Identity's IdentityUser for authentication.
    /// IdentityUser provides built-in properties: Email, UserName, PasswordHash, etc.
    /// 
    /// Extended with FirstName and LastName for user profile information.
    /// 
    /// Business rules:
    /// - UserName is always set to Email (enforced in RegisterCommandHandler)
    /// - All new users are assigned "User" role by default
    /// 
    /// Architecture decision: Kept in DataAccess layer despite general rule of domain entities in Logic layer.
    /// Rationale: ApiUser is an infrastructure entity tightly coupled with EF Core Identity framework.
    /// Moving to Logic layer would create circular dependency and violate Identity architecture patterns.
    /// Identity entities (IdentityUser, IdentityRole) are infrastructure concerns, not pure domain entities.
    /// </remarks>
    public sealed class ApiUser : IdentityUser
    {
        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        /// <remarks>
        /// Required for user registration and profile display.
        /// Used in user-facing features and personalization.
        /// </remarks>
        public required string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        /// <remarks>
        /// Required for user registration and profile display.
        /// Combined with FirstName for full name display: "{FirstName} {LastName}".
        /// </remarks>
        public required string LastName { get; set; }
        
        /// <summary>
        /// Gets the user's full name.
        /// </summary>
        /// <remarks>
        /// Computed property combining first and last names.
        /// Useful for display purposes without string concatenation in UI.
        /// </remarks>
        public string FullName => $"{FirstName} {LastName}";
    }
}
