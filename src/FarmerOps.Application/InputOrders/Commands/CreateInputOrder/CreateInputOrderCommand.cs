using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.InputOrders.Dtos;
using FarmerOps.Domain.Entities;
using FarmerOps.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.InputOrders.Commands.CreateInputOrder;

public sealed record CreateInputOrderCommand(
    Guid FarmerId,
    InputType InputType,
    decimal Quantity,
    decimal UnitCost,
    Guid? LoanId) : IRequest<InputOrderDto>;

public sealed class CreateInputOrderCommandValidator : AbstractValidator<CreateInputOrderCommand>
{
    public CreateInputOrderCommandValidator()
    {
        RuleFor(x => x.FarmerId).NotEmpty();
        RuleFor(x => x.InputType).IsInEnum();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitCost).GreaterThan(0);
    }
}

public sealed class CreateInputOrderCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateInputOrderCommand, InputOrderDto>
{
    public async Task<InputOrderDto> Handle(CreateInputOrderCommand request, CancellationToken cancellationToken)
    {
        var farmer = await db.Farmers.FirstOrDefaultAsync(f => f.Id == request.FarmerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Farmer), request.FarmerId);

        if (request.LoanId is not null)
        {
            var loanExists = await db.Loans.AnyAsync(l => l.Id == request.LoanId && l.FarmerId == request.FarmerId, cancellationToken);
            if (!loanExists)
                throw new NotFoundException(nameof(Loan), request.LoanId);
        }

        var order = new InputOrder(request.FarmerId, request.InputType, request.Quantity, request.UnitCost, request.LoanId);
        db.InputOrders.Add(order);
        await db.SaveChangesAsync(cancellationToken);

        return InputOrderDto.FromEntity(order) with { FarmerName = farmer.FullName };
    }
}
