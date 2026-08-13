using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Common.Models;
using FarmerOps.Application.Farmers.Dtos;
using FarmerOps.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Farmers.Queries.GetFarmers;

public sealed record GetFarmersQuery(
    Guid? DistrictId = null,
    CropType? PrimaryCrop = null,
    string? Search = null,
    bool? IsActive = null,
    int PageNumber = 1,
    int PageSize = 25) : IRequest<PagedResult<FarmerDto>>;

public sealed class GetFarmersQueryHandler(IApplicationDbContext db) : IRequestHandler<GetFarmersQuery, PagedResult<FarmerDto>>
{
    public async Task<PagedResult<FarmerDto>> Handle(GetFarmersQuery request, CancellationToken cancellationToken)
    {
        var query = db.Farmers.Include(f => f.District).AsNoTracking().AsQueryable();

        if (request.DistrictId is not null)
            query = query.Where(f => f.DistrictId == request.DistrictId);

        if (request.PrimaryCrop is not null)
            query = query.Where(f => f.PrimaryCrop == request.PrimaryCrop);

        if (request.IsActive is not null)
            query = query.Where(f => f.IsActive == request.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(f =>
                EF.Functions.Like(f.FirstName, $"%{search}%") ||
                EF.Functions.Like(f.LastName, $"%{search}%") ||
                EF.Functions.Like(f.PhoneNumber, $"%{search}%") ||
                EF.Functions.Like(f.NationalId, $"%{search}%"));
        }

        var pagedFarmers = await query
            .OrderByDescending(f => f.CreatedAtUtc)
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        var items = pagedFarmers.Items.Select(FarmerDto.FromEntity).ToList();
        return new PagedResult<FarmerDto>(items, pagedFarmers.TotalCount, pagedFarmers.PageNumber, pagedFarmers.PageSize);
    }
}
