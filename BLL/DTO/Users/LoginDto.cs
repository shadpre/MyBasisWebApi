using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Users
{
    /// <summary>
    /// Data Transfer Object for Login.
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Gets or sets the email address.
        /// This field is required and must be a valid email address.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// This field is required and must be between 6 and 15 characters.
        /// </summary>
        [Required]
        [StringLength(15, ErrorMessage = "Your Password is limited to {2} to {1} characters", MinimumLength = 6)]
        public string Password { get; set; }
    }
}