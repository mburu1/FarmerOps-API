using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Loans.Dtos;
using FarmerOps.Domain.Entities;
using FarmerOps.Domain.Rules;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Loans.Commands.ApplyForLoan;

public sealed record ApplyForLoanCommand(Guid FarmerId, decimal PrincipalAmount) : IRequest<LoanDto>;

public sealed class ApplyForLoanCommandValidator : AbstractValidator<ApplyForLoanCommand>
{
    public ApplyForLoanCommandValidator()
    {
        RuleFor(x => x.FarmerId).NotEmpty();
        RuleFor(x => x.PrincipalAmount).GreaterThan(0);
    }
}

public sealed class ApplyForLoanCommandHandler(IApplicationDbContext db, LoanEligibilityEngine eligibilityEngine)
    : IRequestHandler<ApplyForLoanCommand, LoanDto>
{
    public async Task<LoanDto> Handle(ApplyForLoanCommand request, CancellationToken cancellationToken)
    {
        var farmer = await db.Farmers.FirstOrDefaultAsync(f => f.Id == request.FarmerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Farmer), request.FarmerId);

        var loanHistory = await db.Loans.AsNoTracking()
            .Where(l => l.FarmerId == request.FarmerId)
            .ToListAsync(cancellationToken);

        var report = eligibilityEngine.Evaluate(new LoanEligibilityContext(farmer, request.PrincipalAmount, loanHistory));
        if (!report.IsEligible)
        {
            var failures = report.RuleOutcomes
                .Where(o => !o.Result.Passed)
                .Select(o => new FluentValidation.Results.ValidationFailure(o.RuleCode, o.Result.Reason));
            throw new FluentValidation.ValidationException(failures);
        }

        var loan = new Loan(request.FarmerId, request.PrincipalAmount);
        db.Loans.Add(loan);
        await db.SaveChangesAsync(cancellationToken);

        return LoanDto.FromEntity(loan) with { FarmerName = farmer.FullName };
    }
}
