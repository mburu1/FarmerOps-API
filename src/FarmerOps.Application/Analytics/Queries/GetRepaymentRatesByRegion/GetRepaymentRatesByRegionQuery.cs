using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Analytics.Queries.GetRepaymentRatesByRegion;

public sealed record GetRepaymentRatesByRegionQuery : IRequest<IReadOnlyCollection<RegionRepaymentRateDto>>;

public sealed record RegionRepaymentRateDto(
    Guid RegionId,
    string RegionName,
    int TotalLoans,
    int RepaidLoans,
    int OverdueLoans,
    int DefaultedLoans,
    double RepaymentRatePercent);

/// <summary>Aggregate endpoint demonstrating measurable operational impact: repayment health per county.</summary>
public sealed class GetRepaymentRatesByRegionQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetRepaymentRatesByRegionQuery, IReadOnlyCollection<RegionRepaymentRateDto>>
{
    public async Task<IReadOnlyCollection<RegionRepaymentRateDto>> Handle(
        GetRepaymentRatesByRegionQuery request, CancellationToken cancellationToken)
    {
        var rows = await db.Loans
            .AsNoTracking()
            .Select(l => new
            {
                l.Status,
                RegionId = l.Farmer!.District!.RegionId,
                RegionName = l.Farmer.District.Region!.Name
            })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(r => new { r.RegionId, r.RegionName })
            .Select(g =>
            {
                var total = g.Count();
                var repaid = g.Count(x => x.Status == LoanStatus.Repaid);
                var overdue = g.Count(x => x.Status == LoanStatus.Overdue);
                var defaulted = g.Count(x => x.Status == LoanStatus.Defaulted);
                var rate = total == 0 ? 0 : Math.Round(repaid * 100.0 / total, 1);

                return new RegionRepaymentRateDto(g.Key.RegionId, g.Key.RegionName, total, repaid, overdue, defaulted, rate);
            })
            .OrderByDescending(r => r.TotalLoans)
            .ToList();
    }
}
