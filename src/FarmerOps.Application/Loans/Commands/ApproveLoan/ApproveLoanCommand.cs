using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Loans.Dtos;
using FarmerOps.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Loans.Commands.ApproveLoan;

public sealed record ApproveLoanCommand(Guid LoanId) : IRequest<LoanDto>;

public sealed class ApproveLoanCommandHandler(IApplicationDbContext db) : IRequestHandler<ApproveLoanCommand, LoanDto>
{
    public async Task<LoanDto> Handle(ApproveLoanCommand request, CancellationToken cancellationToken)
    {
        var loan = await db.Loans.Include(l => l.Farmer).FirstOrDefaultAsync(l => l.Id == request.LoanId, cancellationToken)
            ?? throw new NotFoundException(nameof(Loan), request.LoanId);

        loan.Approve();
        await db.SaveChangesAsync(cancellationToken);

        return LoanDto.FromEntity(loan);
    }
}
