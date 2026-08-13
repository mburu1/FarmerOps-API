using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Common.Models;
using FarmerOps.Application.Loans.Dtos;
using FarmerOps.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Loans.Queries.GetLoans;

public sealed record GetLoansQuery(
    Guid? FarmerId = null,
    LoanStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 25) : IRequest<PagedResult<LoanDto>>;

public sealed class GetLoansQueryHandler(IApplicationDbContext db) : IRequestHandler<GetLoansQuery, PagedResult<LoanDto>>
{
    public async Task<PagedResult<LoanDto>> Handle(GetLoansQuery request, CancellationToken cancellationToken)
    {
        var query = db.Loans.Include(l => l.Farmer).AsNoTracking().AsQueryable();

        if (request.FarmerId is not null)
            query = query.Where(l => l.FarmerId == request.FarmerId);

        if (request.Status is not null)
            query = query.Where(l => l.Status == request.Status);

        var paged = await query
            .OrderByDescending(l => l.AppliedAtUtc)
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        var items = paged.Items.Select(LoanDto.FromEntity).ToList();
        return new PagedResult<LoanDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}
