using FarmerOps.Application.Alerts.Dtos;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Common.Models;
using FarmerOps.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Alerts.Queries.GetAlerts;

public sealed record GetAlertsQuery(
    Guid? FarmerId = null,
    AlertStatus? Status = null,
    AlertType? Type = null,
    int PageNumber = 1,
    int PageSize = 25) : IRequest<PagedResult<AlertDto>>;

public sealed class GetAlertsQueryHandler(IApplicationDbContext db) : IRequestHandler<GetAlertsQuery, PagedResult<AlertDto>>
{
    public async Task<PagedResult<AlertDto>> Handle(GetAlertsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Alerts.AsNoTracking().AsQueryable();

        if (request.FarmerId is not null)
            query = query.Where(a => a.FarmerId == request.FarmerId);

        if (request.Status is not null)
            query = query.Where(a => a.Status == request.Status);

        if (request.Type is not null)
            query = query.Where(a => a.Type == request.Type);

        var paged = await query
            .OrderByDescending(a => a.CreatedAtUtc)
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        var items = paged.Items.Select(AlertDto.FromEntity).ToList();
        return new PagedResult<AlertDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}
