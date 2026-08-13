using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Regions.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Regions.Queries.GetRegions;

public sealed record GetRegionsQuery : IRequest<IReadOnlyCollection<RegionDto>>;

public sealed class GetRegionsQueryHandler(IApplicationDbContext db) : IRequestHandler<GetRegionsQuery, IReadOnlyCollection<RegionDto>>
{
    public async Task<IReadOnlyCollection<RegionDto>> Handle(GetRegionsQuery request, CancellationToken cancellationToken)
    {
        var regions = await db.Regions
            .Include(r => r.Districts)
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return regions.Select(RegionDto.FromEntity).ToList();
    }
}
