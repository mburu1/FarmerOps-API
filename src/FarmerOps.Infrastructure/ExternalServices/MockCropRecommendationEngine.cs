using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Domain.Enums;

namespace FarmerOps.Infrastructure.ExternalServices;

/// <summary>
/// Deterministic stand-in for an ML crop-recommendation model. It encodes simple crop-rotation
/// agronomy (legumes replenish nitrogen for cereals, and vice versa) so the response looks
/// plausible without a real model — the point is the integration seam (<see cref="ICropRecommendationEngine"/>),
/// not the science. Swapping this for a hosted model or external AI API is a one-class change.
/// </summary>
public class MockCropRecommendationEngine : ICropRecommendationEngine
{
    private static readonly Dictionary<CropType, CropType> RotationMap = new()
    {
        [CropType.Maize] = CropType.Beans,
        [CropType.Beans] = CropType.Maize,
        [CropType.Coffee] = CropType.Beans,
        [CropType.Tea] = CropType.Beans,
        [CropType.Potatoes] = CropType.Beans,
        [CropType.Sorghum] = CropType.Beans,
        [CropType.Other] = CropType.Maize
    };

    public Task<CropRecommendationResult> RecommendAsync(CropRecommendationInput input, CancellationToken cancellationToken = default)
    {
        var recommended = RotationMap.GetValueOrDefault(input.CurrentCrop, CropType.Maize);
        var confidence = Math.Round(0.65 + (double)Math.Min(input.FarmSizeAcres, 5) * 0.05, 2);

        var rationale =
            $"Rotating from {input.CurrentCrop} to {recommended} in {input.DistrictName} replenishes soil nitrogen " +
            "and interrupts pest/disease cycles specific to the current crop.";

        var suggestedInputs = recommended switch
        {
            CropType.Beans => new[] { "Certified bean seed", "Rhizobium inoculant", "DAP fertilizer" },
            CropType.Maize => new[] { "Hybrid maize seed", "CAN top-dressing fertilizer", "Pre-emergent herbicide" },
            _ => new[] { "Certified seed", "General-purpose fertilizer" }
        };

        var result = new CropRecommendationResult(recommended, confidence, rationale, suggestedInputs);
        return Task.FromResult(result);
    }
}
