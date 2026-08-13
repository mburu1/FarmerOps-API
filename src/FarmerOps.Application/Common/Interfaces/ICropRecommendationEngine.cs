using FarmerOps.Domain.Enums;

namespace FarmerOps.Application.Common.Interfaces;

/// <summary>
/// Integration seam for an AI/ML crop recommendation service. The Infrastructure implementation
/// is a deterministic stub today; swapping in a real model or external API later requires no
/// change to Application or Api code.
/// </summary>
public interface ICropRecommendationEngine
{
    Task<CropRecommendationResult> RecommendAsync(CropRecommendationInput input, CancellationToken cancellationToken = default);
}

public sealed record CropRecommendationInput(CropType CurrentCrop, decimal FarmSizeAcres, string DistrictName);

public sealed record CropRecommendationResult(
    CropType RecommendedCrop,
    double ConfidenceScore,
    string Rationale,
    IReadOnlyCollection<string> SuggestedInputs);
