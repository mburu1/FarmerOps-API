using FarmerOps.Domain.Common;
using FarmerOps.Domain.Enums;
using FarmerOps.Domain.Exceptions;

namespace FarmerOps.Domain.Entities;

public class User : AggregateRoot
{
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public UserRole Role { get; private set; }
    public Guid? FieldAgentId { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<RefreshToken> _refreshTokens = [];
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User()
    {
    }

    public User(string email, string passwordHash, UserRole role, Guid? fieldAgentId = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = role;
        FieldAgentId = fieldAgentId;
    }

    public void ChangePasswordHash(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("Password hash is required.");

        PasswordHash = newPasswordHash;
        Touch();
    }

    public RefreshToken IssueRefreshToken(string token, DateTime expiresAtUtc)
    {
        var refreshToken = new RefreshToken(Id, token, expiresAtUtc);
        _refreshTokens.Add(refreshToken);
        return refreshToken;
    }

    public void Deactivate() => IsActive = false;
}
