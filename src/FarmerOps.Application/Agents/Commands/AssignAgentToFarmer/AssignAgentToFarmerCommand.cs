using FarmerOps.Application.Agents.Dtos;
using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Agents.Commands.AssignAgentToFarmer;

public sealed record AssignAgentToFarmerCommand(Guid AgentId, Guid FarmerId) : IRequest<AgentAssignmentDto>;

public sealed class AssignAgentToFarmerCommandValidator : AbstractValidator<AssignAgentToFarmerCommand>
{
    public AssignAgentToFarmerCommandValidator()
    {
        RuleFor(x => x.AgentId).NotEmpty();
        RuleFor(x => x.FarmerId).NotEmpty();
    }
}

public sealed class AssignAgentToFarmerCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AssignAgentToFarmerCommand, AgentAssignmentDto>
{
    public async Task<AgentAssignmentDto> Handle(AssignAgentToFarmerCommand request, CancellationToken cancellationToken)
    {
        var agent = await db.FieldAgents.FirstOrDefaultAsync(a => a.Id == request.AgentId, cancellationToken)
            ?? throw new NotFoundException(nameof(FieldAgent), request.AgentId);
        var farmer = await db.Farmers.FirstOrDefaultAsync(f => f.Id == request.FarmerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Farmer), request.FarmerId);

        var alreadyAssigned = await db.AgentAssignments
            .AnyAsync(a => a.FarmerId == request.FarmerId && a.IsActive, cancellationToken);
        if (alreadyAssigned)
            throw new FluentValidation.ValidationException([
                new FluentValidation.Results.ValidationFailure(nameof(request.FarmerId), "Farmer already has an active agent assignment.")
            ]);

        var assignment = new AgentAssignment(request.AgentId, request.FarmerId);
        db.AgentAssignments.Add(assignment);
        await db.SaveChangesAsync(cancellationToken);

        return AgentAssignmentDto.FromEntity(assignment) with { AgentName = agent.FullName, FarmerName = farmer.FullName };
    }
}
