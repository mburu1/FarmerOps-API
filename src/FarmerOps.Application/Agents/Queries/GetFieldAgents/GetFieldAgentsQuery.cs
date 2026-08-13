using FarmerOps.Application.Agents.Dtos;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Agents.Queries.GetFieldAgents;

public sealed record GetFieldAgentsQuery(bool? IsActive = null, int PageNumber = 1, int PageSize = 25) : IRequest<PagedResult<FieldAgentDto>>;

public sealed class GetFieldAgentsQueryHandler(IApplicationDbContext db) : IRequestHandler<GetFieldAgentsQuery, PagedResult<FieldAgentDto>>
{
    public async Task<PagedResult<FieldAgentDto>> Handle(GetFieldAgentsQuery request, CancellationToken cancellationToken)
    {
        var query = db.FieldAgents.AsNoTracking().AsQueryable();

        if (request.IsActive is not null)
            query = query.Where(a => a.IsActive == request.IsActive);

        var paged = await query
            .OrderBy(a => a.FirstName)
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        var items = paged.Items.Select(FieldAgentDto.FromEntity).ToList();
        return new PagedResult<FieldAgentDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}
