using FarmerOps.Application.Analytics.Queries.GetAgentCoverageGaps;
using FarmerOps.Application.Analytics.Queries.GetInputUptakeByCropType;
using FarmerOps.Application.Analytics.Queries.GetRepaymentRatesByRegion;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerOps.Api.Controllers;

/// <summary>Aggregate, business-facing endpoints: the "measurable operational impact" surface of the API.</summary>
[Route("api/v1/[controller]")]
[Authorize]
public class AnalyticsController(ISender sender) : BaseApiController(sender)
{
    [HttpGet("repayment-rates")]
    [ProducesResponseType<IReadOnlyCollection<RegionRepaymentRateDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<RegionRepaymentRateDto>>> RepaymentRatesByRegion(CancellationToken cancellationToken)
        => Ok(await Sender.Send(new GetRepaymentRatesByRegionQuery(), cancellationToken));

    [HttpGet("input-uptake")]
    [ProducesResponseType<IReadOnlyCollection<CropInputUptakeDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<CropInputUptakeDto>>> InputUptakeByCropType(CancellationToken cancellationToken)
        => Ok(await Sender.Send(new GetInputUptakeByCropTypeQuery(), cancellationToken));

    [HttpGet("agent-coverage-gaps")]
    [ProducesResponseType<IReadOnlyCollection<AgentCoverageGapDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<AgentCoverageGapDto>>> AgentCoverageGaps(CancellationToken cancellationToken)
        => Ok(await Sender.Send(new GetAgentCoverageGapsQuery(), cancellationToken));
}
