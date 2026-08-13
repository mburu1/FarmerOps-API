using FarmerOps.Domain.Entities;

namespace FarmerOps.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
