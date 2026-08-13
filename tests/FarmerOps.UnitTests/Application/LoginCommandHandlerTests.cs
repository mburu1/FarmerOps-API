using FarmerOps.Application.Auth.Commands.Login;
using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Common.Models;
using FarmerOps.Domain.Entities;
using FarmerOps.Domain.Enums;
using FarmerOps.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace FarmerOps.UnitTests.Application;

/// <summary>
/// Regression coverage for a real bug: issuing a refresh token for an already-tracked (loaded via
/// query, not Add()-ed) User previously left the new RefreshToken with the wrong EF Core entity
/// state, because its parent collection navigation was never loaded/included. SaveChanges then
/// tried to UPDATE a row that was never inserted and threw DbUpdateConcurrencyException. The fix
/// explicitly tracks the returned token via db.RefreshTokens.Add(...).
/// </summary>
public class LoginCommandHandlerTests
{
    private static readonly JwtSettings Settings = new()
    {
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        SecretKey = "unit-test-signing-key-at-least-32-characters-long",
        AccessTokenMinutes = 15,
        RefreshTokenDays = 7
    };

    private static IPasswordHasher PasswordHasher => new PasswordHasher();
    private static IJwtTokenService TokenService => new JwtTokenService(Options.Create(Settings));

    [Fact]
    public async Task Handle_ValidCredentials_PersistsRefreshTokenWithoutConcurrencyException()
    {
        await using var db = TestDbContextFactory.Create();
        var passwordHash = PasswordHasher.Hash("P@ssw0rd123!");
        var user = new User("admin@farmerops.test", passwordHash, UserRole.Admin);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new LoginCommandHandler(db, PasswordHasher, TokenService, Options.Create(Settings));

        var act = () => handler.Handle(new LoginCommand("admin@farmerops.test", "P@ssw0rd123!"), CancellationToken.None);

        await act.Should().NotThrowAsync();
        db.RefreshTokens.Should().ContainSingle(rt => rt.UserId == user.Id);
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsAuthenticationFailedException()
    {
        await using var db = TestDbContextFactory.Create();
        var user = new User("admin@farmerops.test", PasswordHasher.Hash("P@ssw0rd123!"), UserRole.Admin);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new LoginCommandHandler(db, PasswordHasher, TokenService, Options.Create(Settings));

        var act = () => handler.Handle(new LoginCommand("admin@farmerops.test", "wrong-password"), CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationFailedException>();
    }
}
