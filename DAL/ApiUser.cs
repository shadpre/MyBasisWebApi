using Microsoft.AspNetCore.Identity;

namespace DAL
{
    /// <summary>
    /// Represents an API user.
    /// Inherits from IdentityUser.
    /// </summary>
    public class ApiUser : IdentityUser
    {
        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        public string LastName { get; set; }
    }
}