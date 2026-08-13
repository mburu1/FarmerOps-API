using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Loans.Dtos;
using FarmerOps.Domain.Entities;
using FarmerOps.Domain.Rules;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Loans.Queries.CheckLoanEligibility;

public sealed record CheckLoanEligibilityQuery(Guid FarmerId, decimal RequestedAmount) : IRequest<LoanEligibilityReportDto>;

public sealed class CheckLoanEligibilityQueryHandler(IApplicationDbContext db, LoanEligibilityEngine eligibilityEngine)
    : IRequestHandler<CheckLoanEligibilityQuery, LoanEligibilityReportDto>
{
    public async Task<LoanEligibilityReportDto> Handle(CheckLoanEligibilityQuery request, CancellationToken cancellationToken)
    {
        var farmer = await db.Farmers.AsNoTracking().FirstOrDefaultAsync(f => f.Id == request.FarmerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Farmer), request.FarmerId);

        var loanHistory = await db.Loans.AsNoTracking()
            .Where(l => l.FarmerId == request.FarmerId)
            .ToListAsync(cancellationToken);

        var report = eligibilityEngine.Evaluate(new LoanEligibilityContext(farmer, request.RequestedAmount, loanHistory));
        return LoanEligibilityReportDto.FromDomain(report);
    }
}
