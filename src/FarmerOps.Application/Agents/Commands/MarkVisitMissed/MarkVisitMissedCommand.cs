using FarmerOps.Application.Agents.Dtos;
using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Agents.Commands.MarkVisitMissed;

public sealed record MarkVisitMissedCommand(Guid VisitId) : IRequest<VisitDto>;

public sealed class MarkVisitMissedCommandHandler(IApplicationDbContext db) : IRequestHandler<MarkVisitMissedCommand, VisitDto>
{
    public async Task<VisitDto> Handle(MarkVisitMissedCommand request, CancellationToken cancellationToken)
    {
        var visit = await db.Visits.FirstOrDefaultAsync(v => v.Id == request.VisitId, cancellationToken)
            ?? throw new NotFoundException(nameof(Visit), request.VisitId);
        var agent = await db.FieldAgents.FirstOrDefaultAsync(a => a.Id == visit.AgentId, cancellationToken)
            ?? throw new NotFoundException(nameof(FieldAgent), visit.AgentId);

        visit.MarkMissed();
        agent.RecordVisitOutcome(completed: false);

        await db.SaveChangesAsync(cancellationToken);

        return VisitDto.FromEntity(visit);
    }
}
