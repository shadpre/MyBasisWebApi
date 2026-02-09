using MyBasisWebApi.Logic.Models.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace MyBasisWebApi.Logic.Handlers.Commands.Register;

/// <summary>
/// Command to register a new user in the system.
/// </summary>
/// <param name="UserData">The user registration data.</param>
/// <remarks>
/// Design decision: Use MediatR command pattern to separate request handling from controllers.
/// Returns list of identity errors if registration fails, empty list if successful.
/// </remarks>
public sealed record RegisterCommand(ApiUserDto UserData) 
    : IRequest<IReadOnlyList<IdentityError>>;
