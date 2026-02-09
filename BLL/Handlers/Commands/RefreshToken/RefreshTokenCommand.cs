using MyBasisWebApi.Logic.Models.Users;
using MediatR;

namespace MyBasisWebApi.Logic.Handlers.Commands.RefreshToken;

/// <summary>
/// Command to refresh an expired JWT access token using a refresh token.
/// </summary>
/// <param name="TokenData">The current authentication response containing token and refresh token.</param>
/// <remarks>
/// Design decision: Use command pattern because refresh operation modifies state (creates new tokens).
/// Returns null if refresh token is invalid or expired.
/// </remarks>
public sealed record RefreshTokenCommand(AuthResponseDto TokenData) 
    : IRequest<AuthResponseDto?>;
