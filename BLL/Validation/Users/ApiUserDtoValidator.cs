using MyBasisWebApi.Logic.Models.Users;
using FluentValidation;

namespace MyBasisWebApi.Logic.Validation.Users;

/// <summary>
/// Validator for <see cref="ApiUserDto"/> using FluentValidation.
/// </summary>
/// <remarks>
/// Centralizes all validation rules for user registration requests.
/// Ensures complete and valid user data before processing registration.
/// Automatically discovered and registered by FluentValidation.
/// </remarks>
public sealed class ApiUserDtoValidator : AbstractValidator<ApiUserDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiUserDtoValidator"/> class.
    /// </summary>
    public ApiUserDtoValidator()
    {
        // First name validation - required with reasonable length
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(50)
            .WithMessage("First name cannot exceed 50 characters")
            .Matches("^[a-zA-Z\\s'-]+$")
            .WithMessage("First name can only contain letters, spaces, hyphens, and apostrophes");

        // Last name validation - required with reasonable length
        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MaximumLength(50)
            .WithMessage("Last name cannot exceed 50 characters")
            .Matches("^[a-zA-Z\\s'-]+$")
            .WithMessage("Last name can only contain letters, spaces, hyphens, and apostrophes");

        // Email validation - required and must be valid email format
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email address is required")
            .EmailAddress()
            .WithMessage("A valid email address is required")
            .MaximumLength(100)
            .WithMessage("Email cannot exceed 100 characters");

        // Password validation - strong password requirements
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters")
            .MaximumLength(100)
            .WithMessage("Password cannot exceed 100 characters")
            .Matches("[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]")
            .WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]")
            .WithMessage("Password must contain at least one number");
    }
}

