using FarmerOps.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Analytics.Queries.GetAgentCoverageGaps;

public sealed record GetAgentCoverageGapsQuery : IRequest<IReadOnlyCollection<AgentCoverageGapDto>>;

public sealed record AgentCoverageGapDto(
    Guid DistrictId,
    string DistrictName,
    int FarmerCount,
    int AssignedFarmerCount,
    int UnassignedFarmerCount,
    double CoveragePercent);

/// <summary>Surfaces districts where field-agent coverage is falling behind farmer registrations.</summary>
public sealed class GetAgentCoverageGapsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetAgentCoverageGapsQuery, IReadOnlyCollection<AgentCoverageGapDto>>
{
    public async Task<IReadOnlyCollection<AgentCoverageGapDto>> Handle(
        GetAgentCoverageGapsQuery request, CancellationToken cancellationToken)
    {
        var farmerRows = await db.Farmers
            .AsNoTracking()
            .Where(f => f.IsActive)
            .Select(f => new { f.Id, f.DistrictId, DistrictName = f.District!.Name })
            .ToListAsync(cancellationToken);

        var assignedFarmerIds = await db.AgentAssignments
            .AsNoTracking()
            .Where(a => a.IsActive)
            .Select(a => a.FarmerId)
            .ToListAsync(cancellationToken);

        var assignedSet = assignedFarmerIds.ToHashSet();

        return farmerRows
            .GroupBy(f => new { f.DistrictId, f.DistrictName })
            .Select(g =>
            {
                var total = g.Count();
                var assigned = g.Count(x => assignedSet.Contains(x.Id));
                var coverage = total == 0 ? 0 : Math.Round(assigned * 100.0 / total, 1);

                return new AgentCoverageGapDto(g.Key.DistrictId, g.Key.DistrictName, total, assigned, total - assigned, coverage);
            })
            .OrderBy(r => r.CoveragePercent)
            .ToList();
    }
}
