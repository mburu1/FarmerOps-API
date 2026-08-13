using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Loans.Dtos;
using FarmerOps.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Loans.Queries.GetLoanById;

public sealed record GetLoanByIdQuery(Guid Id) : IRequest<LoanDto>;

public sealed class GetLoanByIdQueryHandler(IApplicationDbContext db) : IRequestHandler<GetLoanByIdQuery, LoanDto>
{
    public async Task<LoanDto> Handle(GetLoanByIdQuery request, CancellationToken cancellationToken)
    {
        var loan = await db.Loans
            .Include(l => l.Farmer)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Loan), request.Id);

        return LoanDto.FromEntity(loan);
    }
}
