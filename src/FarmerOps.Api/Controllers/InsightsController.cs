using FarmerOps.Application.Insights.Dtos;
using FarmerOps.Application.Insights.Queries.GetCropRecommendation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerOps.Api.Controllers;

/// <summary>AI integration-point stub: <see cref="Application.Common.Interfaces.ICropRecommendationEngine"/> is the seam a real model plugs into.</summary>
[Route("api/v1/[controller]")]
[Authorize]
public class InsightsController(ISender sender) : BaseApiController(sender)
{
    [HttpGet("crop-recommendation")]
    [ProducesResponseType<CropRecommendationDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CropRecommendationDto>> CropRecommendation([FromQuery] Guid farmerId, CancellationToken cancellationToken)
        => Ok(await Sender.Send(new GetCropRecommendationQuery(farmerId), cancellationToken));
}
