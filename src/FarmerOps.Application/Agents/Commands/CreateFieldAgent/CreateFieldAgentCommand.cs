using FarmerOps.Application.Agents.Dtos;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Domain.Entities;
using FluentValidation;
using MediatR;

namespace FarmerOps.Application.Agents.Commands.CreateFieldAgent;

public sealed record CreateFieldAgentCommand(string FirstName, string LastName, string PhoneNumber, string Email) : IRequest<FieldAgentDto>;

public sealed class CreateFieldAgentCommandValidator : AbstractValidator<CreateFieldAgentCommand>
{
    public CreateFieldAgentCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public sealed class CreateFieldAgentCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateFieldAgentCommand, FieldAgentDto>
{
    public async Task<FieldAgentDto> Handle(CreateFieldAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = new FieldAgent(request.FirstName, request.LastName, request.PhoneNumber, request.Email);
        db.FieldAgents.Add(agent);
        await db.SaveChangesAsync(cancellationToken);

        return FieldAgentDto.FromEntity(agent);
    }
}
