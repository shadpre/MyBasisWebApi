using MyBasisWebApi.Logic.Models.Users;
using FluentValidation;

namespace MyBasisWebApi.Logic.Validation.Users;

/// <summary>
/// Validator for <see cref="LoginDto"/> using FluentValidation.
/// </summary>
/// <remarks>
/// Centralizes all validation rules for login requests.
/// Provides better error messages and more flexible validation than Data Annotations.
/// Automatically discovered and registered by FluentValidation.
/// </remarks>
public sealed class LoginDtoValidator : AbstractValidator<LoginDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginDtoValidator"/> class.
    /// </summary>
    public LoginDtoValidator()
    {
        // Email validation - required and must be valid email format
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email address is required")
            .EmailAddress()
            .WithMessage("A valid email address is required");

        // Password validation - required with length constraints
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters")
            .MaximumLength(100)
            .WithMessage("Password cannot exceed 100 characters");
    }
}
