using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Analytics.Queries.GetInputUptakeByCropType;

public sealed record GetInputUptakeByCropTypeQuery : IRequest<IReadOnlyCollection<CropInputUptakeDto>>;

public sealed record CropInputUptakeDto(
    CropType CropType,
    int FarmerCount,
    int TotalOrders,
    decimal TotalQuantity,
    decimal TotalCost);

public sealed class GetInputUptakeByCropTypeQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetInputUptakeByCropTypeQuery, IReadOnlyCollection<CropInputUptakeDto>>
{
    public async Task<IReadOnlyCollection<CropInputUptakeDto>> Handle(
        GetInputUptakeByCropTypeQuery request, CancellationToken cancellationToken)
    {
        var rows = await db.InputOrders
            .AsNoTracking()
            .Where(o => o.Status == InputOrderStatus.Fulfilled)
            .Select(o => new
            {
                o.FarmerId,
                CropType = o.Farmer!.PrimaryCrop,
                o.Quantity,
                o.UnitCost
            })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(r => r.CropType)
            .Select(g => new CropInputUptakeDto(
                g.Key,
                g.Select(x => x.FarmerId).Distinct().Count(),
                g.Count(),
                g.Sum(x => x.Quantity),
                g.Sum(x => x.Quantity * x.UnitCost)))
            .OrderByDescending(r => r.TotalCost)
            .ToList();
    }
}
