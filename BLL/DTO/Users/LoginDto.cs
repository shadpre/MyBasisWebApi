namespace MyBasisWebApi.Logic.Models.Users;

/// <summary>
/// Data transfer object for user login requests.
/// </summary>
/// <param name="Email">The user's email address used for authentication.</param>
/// <param name="Password">The user's password.</param>
/// <remarks>
/// Design decision: Use sealed record for immutability and value equality.
/// Validation is handled by <see cref="LoginDtoValidator"/> using FluentValidation.
/// </remarks>
public sealed record LoginDto(
    string Email,
    string Password);