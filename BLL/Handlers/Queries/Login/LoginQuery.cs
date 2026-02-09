using MyBasisWebApi.Logic.Models.Users;
using MediatR;

namespace MyBasisWebApi.Logic.Handlers.Queries.Login;

/// <summary>
/// Query to authenticate a user and retrieve JWT tokens.
/// </summary>
/// <param name="LoginData">The user login credentials.</param>
/// <remarks>
/// Design decision: Use query pattern (not command) because login is a read operation
/// that retrieves tokens without modifying user state (except refresh token update).
/// Returns null if authentication fails.
/// </remarks>
public sealed record LoginQuery(LoginDto LoginData) 
    : IRequest<AuthResponseDto?>;
