using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Loans.Dtos;
using FarmerOps.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Loans.Commands.RejectLoan;

public sealed record RejectLoanCommand(Guid LoanId, string Reason) : IRequest<LoanDto>;

public sealed class RejectLoanCommandValidator : AbstractValidator<RejectLoanCommand>
{
    public RejectLoanCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class RejectLoanCommandHandler(IApplicationDbContext db) : IRequestHandler<RejectLoanCommand, LoanDto>
{
    public async Task<LoanDto> Handle(RejectLoanCommand request, CancellationToken cancellationToken)
    {
        var loan = await db.Loans.Include(l => l.Farmer).FirstOrDefaultAsync(l => l.Id == request.LoanId, cancellationToken)
            ?? throw new NotFoundException(nameof(Loan), request.LoanId);

        loan.Reject(request.Reason);
        await db.SaveChangesAsync(cancellationToken);

        return LoanDto.FromEntity(loan);
    }
}
