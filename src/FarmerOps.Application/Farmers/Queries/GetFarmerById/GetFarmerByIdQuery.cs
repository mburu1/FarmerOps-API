using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Farmers.Dtos;
using FarmerOps.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Farmers.Queries.GetFarmerById;

public sealed record GetFarmerByIdQuery(Guid Id) : IRequest<FarmerDto>;

public sealed class GetFarmerByIdQueryHandler(IApplicationDbContext db) : IRequestHandler<GetFarmerByIdQuery, FarmerDto>
{
    public async Task<FarmerDto> Handle(GetFarmerByIdQuery request, CancellationToken cancellationToken)
    {
        var farmer = await db.Farmers
            .Include(f => f.District)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Farmer), request.Id);

        return FarmerDto.FromEntity(farmer);
    }
}
