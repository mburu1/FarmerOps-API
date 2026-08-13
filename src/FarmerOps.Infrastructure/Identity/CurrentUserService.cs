using System.Security.Claims;
using FarmerOps.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FarmerOps.Infrastructure.Identity;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email) ?? Principal?.FindFirstValue("email");

    public string? Role => Principal?.FindFirstValue(ClaimTypes.Role);
}
