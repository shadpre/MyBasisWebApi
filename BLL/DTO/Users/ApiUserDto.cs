using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Users
{
    /// <summary>
    /// Data Transfer Object for API User.
    /// Inherits from LoginDto.
    /// </summary>
    public class ApiUserDto : LoginDto
    {
        /// <summary>
        /// Gets or sets the first name of the user.
        /// This field is required.
        /// </summary>
        [Required]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the user.
        /// This field is required.
        /// </summary>
        [Required]
        public string LastName { get; set; }
    }
}