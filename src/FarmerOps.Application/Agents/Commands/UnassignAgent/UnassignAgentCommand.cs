using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Agents.Commands.UnassignAgent;

public sealed record UnassignAgentCommand(Guid AssignmentId) : IRequest;

public sealed class UnassignAgentCommandHandler(IApplicationDbContext db) : IRequestHandler<UnassignAgentCommand>
{
    public async Task Handle(UnassignAgentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await db.AgentAssignments.FirstOrDefaultAsync(a => a.Id == request.AssignmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(AgentAssignment), request.AssignmentId);

        assignment.Unassign();
        await db.SaveChangesAsync(cancellationToken);
    }
}
