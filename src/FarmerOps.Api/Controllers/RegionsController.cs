using FarmerOps.Application.Regions.Commands.CreateDistrict;
using FarmerOps.Application.Regions.Commands.CreateRegion;
using FarmerOps.Application.Regions.Dtos;
using FarmerOps.Application.Regions.Queries.GetRegions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerOps.Api.Controllers;

[Route("api/v1/[controller]")]
[Authorize]
public class RegionsController(ISender sender) : BaseApiController(sender)
{
    /// <summary>Lists all regions (counties) with their districts (sub-counties).</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<RegionDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<RegionDto>>> GetRegions(CancellationToken cancellationToken)
        => Ok(await Sender.Send(new GetRegionsQuery(), cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<RegionDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<RegionDto>> CreateRegion(CreateRegionCommand command, CancellationToken cancellationToken)
    {
        var region = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetRegions), new { }, region);
    }

    [HttpPost("districts")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<DistrictDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<DistrictDto>> CreateDistrict(CreateDistrictCommand command, CancellationToken cancellationToken)
    {
        var district = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetRegions), new { }, district);
    }
}
