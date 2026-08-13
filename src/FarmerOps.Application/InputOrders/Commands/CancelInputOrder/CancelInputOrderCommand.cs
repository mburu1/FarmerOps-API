using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.InputOrders.Dtos;
using FarmerOps.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.InputOrders.Commands.CancelInputOrder;

public sealed record CancelInputOrderCommand(Guid Id) : IRequest<InputOrderDto>;

public sealed class CancelInputOrderCommandHandler(IApplicationDbContext db) : IRequestHandler<CancelInputOrderCommand, InputOrderDto>
{
    public async Task<InputOrderDto> Handle(CancelInputOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await db.InputOrders.Include(o => o.Farmer).FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(InputOrder), request.Id);

        order.Cancel();
        await db.SaveChangesAsync(cancellationToken);

        return InputOrderDto.FromEntity(order);
    }
}
