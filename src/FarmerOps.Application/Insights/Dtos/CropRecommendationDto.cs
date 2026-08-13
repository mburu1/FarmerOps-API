using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Domain.Enums;

namespace FarmerOps.Application.Insights.Dtos;

public sealed record CropRecommendationDto(
    CropType RecommendedCrop,
    double ConfidenceScore,
    string Rationale,
    IReadOnlyCollection<string> SuggestedInputs)
{
    public static CropRecommendationDto FromResult(CropRecommendationResult result) => new(
        result.RecommendedCrop, result.ConfidenceScore, result.Rationale, result.SuggestedInputs);
}
