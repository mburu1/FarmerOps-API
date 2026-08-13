using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Insights.Dtos;
using FarmerOps.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Insights.Queries.GetCropRecommendation;

public sealed record GetCropRecommendationQuery(Guid FarmerId) : IRequest<CropRecommendationDto>;

public sealed class GetCropRecommendationQueryHandler(IApplicationDbContext db, ICropRecommendationEngine recommendationEngine)
    : IRequestHandler<GetCropRecommendationQuery, CropRecommendationDto>
{
    public async Task<CropRecommendationDto> Handle(GetCropRecommendationQuery request, CancellationToken cancellationToken)
    {
        var farmer = await db.Farmers
            .Include(f => f.District)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == request.FarmerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Farmer), request.FarmerId);

        var input = new CropRecommendationInput(farmer.PrimaryCrop, farmer.FarmSizeAcres, farmer.District?.Name ?? "Unknown");
        var result = await recommendationEngine.RecommendAsync(input, cancellationToken);

        return CropRecommendationDto.FromResult(result);
    }
}
