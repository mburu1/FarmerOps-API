using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Loans.Dtos;
using FarmerOps.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Loans.Commands.DisburseLoan;

public sealed record DisburseLoanCommand(Guid LoanId, int RepaymentTermDays) : IRequest<LoanDto>;

public sealed class DisburseLoanCommandValidator : AbstractValidator<DisburseLoanCommand>
{
    public DisburseLoanCommandValidator()
    {
        RuleFor(x => x.RepaymentTermDays).GreaterThan(0).LessThanOrEqualTo(365);
    }
}

public sealed class DisburseLoanCommandHandler(IApplicationDbContext db) : IRequestHandler<DisburseLoanCommand, LoanDto>
{
    public async Task<LoanDto> Handle(DisburseLoanCommand request, CancellationToken cancellationToken)
    {
        var loan = await db.Loans.Include(l => l.Farmer).FirstOrDefaultAsync(l => l.Id == request.LoanId, cancellationToken)
            ?? throw new NotFoundException(nameof(Loan), request.LoanId);

        loan.Disburse(request.RepaymentTermDays);
        await db.SaveChangesAsync(cancellationToken);

        return LoanDto.FromEntity(loan);
    }
}
