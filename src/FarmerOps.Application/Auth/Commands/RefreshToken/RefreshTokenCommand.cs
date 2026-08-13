using FarmerOps.Application.Auth.Dtos;
using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FarmerOps.Application.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResultDto>;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

/// <summary>
/// Implements refresh-token rotation: the presented token is revoked and replaced by a freshly
/// issued one on every use, so a leaked-but-unused token becomes worthless the moment its
/// legitimate owner refreshes.
/// </summary>
public sealed class RefreshTokenCommandHandler(
    IApplicationDbContext db,
    IJwtTokenService tokenService,
    IOptions<JwtSettings> jwtSettings) : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == request.RefreshToken), cancellationToken);

        var existingToken = user?.RefreshTokens.FirstOrDefault(rt => rt.Token == request.RefreshToken);

        if (user is null || !user.IsActive || existingToken is null || !existingToken.IsActive)
            throw new AuthenticationFailedException("Invalid or expired refresh token.");

        var settings = jwtSettings.Value;
        var newRefreshToken = tokenService.GenerateRefreshToken();
        existingToken.Revoke(newRefreshToken);
        user.IssueRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(settings.RefreshTokenDays));

        var accessToken = tokenService.GenerateAccessToken(user);
        await db.SaveChangesAsync(cancellationToken);

        var accessExpiresAtUtc = DateTime.UtcNow.AddMinutes(settings.AccessTokenMinutes);
        return new AuthResultDto(accessToken, newRefreshToken, accessExpiresAtUtc, Dtos.UserDto.FromEntity(user));
    }
}
