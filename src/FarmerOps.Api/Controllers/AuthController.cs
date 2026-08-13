using FarmerOps.Application.Auth.Commands.Login;
using FarmerOps.Application.Auth.Commands.RefreshToken;
using FarmerOps.Application.Auth.Commands.Register;
using FarmerOps.Application.Auth.Dtos;
using FarmerOps.Application.Auth.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerOps.Api.Controllers;

[Route("auth")]
public class AuthController(ISender sender) : BaseApiController(sender)
{
    /// <summary>Creates a user account and returns a token pair — see it work at <c>/scalar/v1</c>.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType<AuthResultDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResultDto>> Register(RegisterCommand command, CancellationToken cancellationToken)
        => Ok(await Sender.Send(command, cancellationToken));

    /// <summary>Exchanges credentials for a JWT access token and refresh token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<AuthResultDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResultDto>> Login(LoginCommand command, CancellationToken cancellationToken)
        => Ok(await Sender.Send(command, cancellationToken));

    /// <summary>Rotates a refresh token for a new access/refresh token pair.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType<AuthResultDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResultDto>> Refresh(RefreshTokenCommand command, CancellationToken cancellationToken)
        => Ok(await Sender.Send(command, cancellationToken));

    /// <summary>Returns the authenticated caller — proves the bearer token and [Authorize] wiring works.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> Me(CancellationToken cancellationToken)
        => Ok(await Sender.Send(new GetCurrentUserQuery(), cancellationToken));
}
