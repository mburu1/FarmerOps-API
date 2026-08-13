using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Domain.Entities;
using FarmerOps.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Agents.Queries.GetAgentPerformance;

public sealed record GetAgentPerformanceQuery(Guid AgentId) : IRequest<AgentPerformanceDto>;

public sealed record AgentPerformanceDto(
    Guid AgentId,
    string AgentName,
    double PerformanceScore,
    int TotalVisits,
    int CompletedVisits,
    int MissedVisits,
    int ScheduledVisits,
    int ActiveFarmerAssignments);

public sealed class GetAgentPerformanceQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetAgentPerformanceQuery, AgentPerformanceDto>
{
    public async Task<AgentPerformanceDto> Handle(GetAgentPerformanceQuery request, CancellationToken cancellationToken)
    {
        var agent = await db.FieldAgents.AsNoTracking().FirstOrDefaultAsync(a => a.Id == request.AgentId, cancellationToken)
            ?? throw new NotFoundException(nameof(FieldAgent), request.AgentId);

        var visits = await db.Visits.AsNoTracking().Where(v => v.AgentId == request.AgentId).ToListAsync(cancellationToken);
        var activeAssignments = await db.AgentAssignments.AsNoTracking()
            .CountAsync(a => a.AgentId == request.AgentId && a.IsActive, cancellationToken);

        return new AgentPerformanceDto(
            agent.Id,
            agent.FullName,
            agent.PerformanceScore,
            visits.Count,
            visits.Count(v => v.Status == VisitStatus.Completed),
            visits.Count(v => v.Status == VisitStatus.Missed),
            visits.Count(v => v.Status == VisitStatus.Scheduled),
            activeAssignments);
    }
}
