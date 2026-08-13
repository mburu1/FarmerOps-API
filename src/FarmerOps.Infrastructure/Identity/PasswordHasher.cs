using FarmerOps.Application.Common.Interfaces;

namespace FarmerOps.Infrastructure.Identity;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

    public bool Verify(string password, string passwordHash) => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
