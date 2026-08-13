using FarmerOps.Application.Auth.Dtos;
using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Common.Models;
using FarmerOps.Domain.Entities;
using FarmerOps.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ValidationException = FarmerOps.Application.Common.Exceptions.ValidationException;

namespace FarmerOps.Application.Auth.Commands.Register;

public sealed record RegisterCommand(string Email, string Password, UserRole Role, Guid? FieldAgentId) : IRequest<AuthResultDto>;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.");
        RuleFor(x => x.Role).IsInEnum();
    }
}

public sealed class RegisterCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenService tokenService,
    IOptions<JwtSettings> jwtSettings) : IRequestHandler<RegisterCommand, AuthResultDto>
{
    public async Task<AuthResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var alreadyExists = await db.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (alreadyExists)
            throw new ValidationException([new FluentValidation.Results.ValidationFailure(nameof(request.Email), "An account with this email already exists.")]);

        var passwordHash = passwordHasher.Hash(request.Password);
        var user = new User(request.Email, passwordHash, request.Role, request.FieldAgentId);

        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        var settings = jwtSettings.Value;
        var refreshExpiresAtUtc = DateTime.UtcNow.AddDays(settings.RefreshTokenDays);
        var refreshTokenEntity = user.IssueRefreshToken(refreshToken, refreshExpiresAtUtc);

        db.Users.Add(user);
        db.RefreshTokens.Add(refreshTokenEntity);
        await db.SaveChangesAsync(cancellationToken);

        var accessExpiresAtUtc = DateTime.UtcNow.AddMinutes(settings.AccessTokenMinutes);
        return new AuthResultDto(accessToken, refreshToken, accessExpiresAtUtc, UserDto.FromEntity(user));
    }
}
