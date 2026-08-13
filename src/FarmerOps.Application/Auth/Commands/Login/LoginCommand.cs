using FarmerOps.Application.Auth.Dtos;
using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FarmerOps.Application.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResultDto>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenService tokenService,
    IOptions<JwtSettings> jwtSettings) : IRequestHandler<LoginCommand, AuthResultDto>
{
    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new AuthenticationFailedException();

        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        var settings = jwtSettings.Value;
        var refreshExpiresAtUtc = DateTime.UtcNow.AddDays(settings.RefreshTokenDays);
        user.IssueRefreshToken(refreshToken, refreshExpiresAtUtc);

        await db.SaveChangesAsync(cancellationToken);

        var accessExpiresAtUtc = DateTime.UtcNow.AddMinutes(settings.AccessTokenMinutes);
        return new AuthResultDto(accessToken, refreshToken, accessExpiresAtUtc, Dtos.UserDto.FromEntity(user));
    }
}
