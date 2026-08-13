using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Common.Models;
using FarmerOps.Application.InputOrders.Dtos;
using FarmerOps.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.InputOrders.Queries.GetInputOrders;

public sealed record GetInputOrdersQuery(
    Guid? FarmerId = null,
    InputOrderStatus? Status = null,
    InputType? InputType = null,
    int PageNumber = 1,
    int PageSize = 25) : IRequest<PagedResult<InputOrderDto>>;

public sealed class GetInputOrdersQueryHandler(IApplicationDbContext db) : IRequestHandler<GetInputOrdersQuery, PagedResult<InputOrderDto>>
{
    public async Task<PagedResult<InputOrderDto>> Handle(GetInputOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = db.InputOrders.Include(o => o.Farmer).AsNoTracking().AsQueryable();

        if (request.FarmerId is not null)
            query = query.Where(o => o.FarmerId == request.FarmerId);

        if (request.Status is not null)
            query = query.Where(o => o.Status == request.Status);

        if (request.InputType is not null)
            query = query.Where(o => o.InputType == request.InputType);

        var paged = await query
            .OrderByDescending(o => o.OrderedAtUtc)
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        var items = paged.Items.Select(InputOrderDto.FromEntity).ToList();
        return new PagedResult<InputOrderDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}
