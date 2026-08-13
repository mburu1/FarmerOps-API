using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Loans.Dtos;
using FarmerOps.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Loans.Commands.RecordRepayment;

public sealed record RecordRepaymentCommand(Guid LoanId, decimal Amount) : IRequest<LoanDto>;

public sealed class RecordRepaymentCommandValidator : AbstractValidator<RecordRepaymentCommand>
{
    public RecordRepaymentCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public sealed class RecordRepaymentCommandHandler(IApplicationDbContext db) : IRequestHandler<RecordRepaymentCommand, LoanDto>
{
    public async Task<LoanDto> Handle(RecordRepaymentCommand request, CancellationToken cancellationToken)
    {
        var loan = await db.Loans.Include(l => l.Farmer).FirstOrDefaultAsync(l => l.Id == request.LoanId, cancellationToken)
            ?? throw new NotFoundException(nameof(Loan), request.LoanId);

        loan.RecordRepayment(request.Amount);
        await db.SaveChangesAsync(cancellationToken);

        return LoanDto.FromEntity(loan);
    }
}
