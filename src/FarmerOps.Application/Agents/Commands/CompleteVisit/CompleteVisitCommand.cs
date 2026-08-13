using FarmerOps.Application.Agents.Dtos;
using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Agents.Commands.CompleteVisit;

public sealed record CompleteVisitCommand(Guid VisitId, string? Notes) : IRequest<VisitDto>;

public sealed class CompleteVisitCommandHandler(IApplicationDbContext db) : IRequestHandler<CompleteVisitCommand, VisitDto>
{
    public async Task<VisitDto> Handle(CompleteVisitCommand request, CancellationToken cancellationToken)
    {
        var visit = await db.Visits.FirstOrDefaultAsync(v => v.Id == request.VisitId, cancellationToken)
            ?? throw new NotFoundException(nameof(Visit), request.VisitId);
        var agent = await db.FieldAgents.FirstOrDefaultAsync(a => a.Id == visit.AgentId, cancellationToken)
            ?? throw new NotFoundException(nameof(FieldAgent), visit.AgentId);

        visit.Complete(request.Notes);
        agent.RecordVisitOutcome(completed: true);

        await db.SaveChangesAsync(cancellationToken);

        return VisitDto.FromEntity(visit);
    }
}
