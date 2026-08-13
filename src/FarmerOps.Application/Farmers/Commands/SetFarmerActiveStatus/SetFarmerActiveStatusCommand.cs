using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Farmers.Commands.SetFarmerActiveStatus;

public sealed record SetFarmerActiveStatusCommand(Guid Id, bool IsActive) : IRequest;

public sealed class SetFarmerActiveStatusCommandHandler(IApplicationDbContext db) : IRequestHandler<SetFarmerActiveStatusCommand>
{
    public async Task Handle(SetFarmerActiveStatusCommand request, CancellationToken cancellationToken)
    {
        var farmer = await db.Farmers.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Farmer), request.Id);

        if (request.IsActive)
            farmer.Reactivate();
        else
            farmer.Deactivate();

        await db.SaveChangesAsync(cancellationToken);
    }
}
