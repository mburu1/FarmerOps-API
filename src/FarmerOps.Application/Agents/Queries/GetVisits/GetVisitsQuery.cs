using FarmerOps.Application.Agents.Dtos;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Common.Models;
using FarmerOps.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Agents.Queries.GetVisits;

public sealed record GetVisitsQuery(
    Guid? AgentId = null,
    Guid? FarmerId = null,
    VisitStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 25) : IRequest<PagedResult<VisitDto>>;

public sealed class GetVisitsQueryHandler(IApplicationDbContext db) : IRequestHandler<GetVisitsQuery, PagedResult<VisitDto>>
{
    public async Task<PagedResult<VisitDto>> Handle(GetVisitsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Visits.Include(v => v.Agent).Include(v => v.Farmer).AsNoTracking().AsQueryable();

        if (request.AgentId is not null)
            query = query.Where(v => v.AgentId == request.AgentId);

        if (request.FarmerId is not null)
            query = query.Where(v => v.FarmerId == request.FarmerId);

        if (request.Status is not null)
            query = query.Where(v => v.Status == request.Status);

        var paged = await query
            .OrderByDescending(v => v.ScheduledAtUtc)
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        var items = paged.Items.Select(VisitDto.FromEntity).ToList();
        return new PagedResult<VisitDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}
