using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.InputOrders.Dtos;
using FarmerOps.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.InputOrders.Commands.FulfillInputOrder;

public sealed record FulfillInputOrderCommand(Guid Id) : IRequest<InputOrderDto>;

public sealed class FulfillInputOrderCommandHandler(IApplicationDbContext db) : IRequestHandler<FulfillInputOrderCommand, InputOrderDto>
{
    public async Task<InputOrderDto> Handle(FulfillInputOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await db.InputOrders.Include(o => o.Farmer).FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(InputOrder), request.Id);

        order.Fulfill();
        await db.SaveChangesAsync(cancellationToken);

        return InputOrderDto.FromEntity(order);
    }
}
