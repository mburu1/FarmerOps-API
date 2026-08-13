using FarmerOps.Application.Auth.Dtos;
using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Auth.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<UserDto>;

/// <summary>Resolves the caller's user record from the JWT principal — proves [Authorize] + token validation works end to end.</summary>
public sealed class GetCurrentUserQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
            throw new AuthenticationFailedException("No authenticated user context.");

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), currentUser.UserId);

        return UserDto.FromEntity(user);
    }
}
