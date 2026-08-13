using FarmerOps.Application.Agents.Dtos;
using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Agents.Commands.ScheduleVisit;

public sealed record ScheduleVisitCommand(Guid AgentId, Guid FarmerId, DateTime ScheduledAtUtc) : IRequest<VisitDto>;

public sealed class ScheduleVisitCommandValidator : AbstractValidator<ScheduleVisitCommand>
{
    public ScheduleVisitCommandValidator()
    {
        RuleFor(x => x.AgentId).NotEmpty();
        RuleFor(x => x.FarmerId).NotEmpty();
        RuleFor(x => x.ScheduledAtUtc).GreaterThan(DateTime.UtcNow).WithMessage("Visit must be scheduled in the future.");
    }
}

public sealed class ScheduleVisitCommandHandler(IApplicationDbContext db) : IRequestHandler<ScheduleVisitCommand, VisitDto>
{
    public async Task<VisitDto> Handle(ScheduleVisitCommand request, CancellationToken cancellationToken)
    {
        var agentExists = await db.FieldAgents.AnyAsync(a => a.Id == request.AgentId, cancellationToken);
        if (!agentExists)
            throw new NotFoundException(nameof(FieldAgent), request.AgentId);

        var farmerExists = await db.Farmers.AnyAsync(f => f.Id == request.FarmerId, cancellationToken);
        if (!farmerExists)
            throw new NotFoundException(nameof(Farmer), request.FarmerId);

        var visit = new Visit(request.AgentId, request.FarmerId, request.ScheduledAtUtc);
        db.Visits.Add(visit);
        await db.SaveChangesAsync(cancellationToken);

        return VisitDto.FromEntity(visit);
    }
}
