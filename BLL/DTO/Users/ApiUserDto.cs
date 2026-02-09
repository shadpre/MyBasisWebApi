namespace MyBasisWebApi.Logic.Models.Users;

/// <summary>
/// Data transfer object for API user registration.
/// </summary>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
/// <param name="Email">The user's email address (also used for login).</param>
/// <param name="Password">The user's chosen password.</param>
/// <remarks>
/// Design decision: Use sealed record for immutability and value equality.
/// Validation is handled by <see cref="ApiUserDtoValidator"/> using FluentValidation.
/// All properties are required for user registration to ensure complete user profiles.
/// </remarks>
public sealed record ApiUserDto(
    string FirstName,
    string LastName,
    string Email,
    string Password);