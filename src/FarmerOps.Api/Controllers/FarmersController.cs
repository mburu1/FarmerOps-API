using FarmerOps.Application.Common.Models;
using FarmerOps.Application.Farmers.Commands.CreateFarmer;
using FarmerOps.Application.Farmers.Commands.SetFarmerActiveStatus;
using FarmerOps.Application.Farmers.Commands.UpdateFarmer;
using FarmerOps.Application.Farmers.Dtos;
using FarmerOps.Application.Farmers.Queries.GetFarmerById;
using FarmerOps.Application.Farmers.Queries.GetFarmers;
using FarmerOps.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerOps.Api.Controllers;

[Route("api/v1/[controller]")]
[Authorize]
public class FarmersController(ISender sender) : BaseApiController(sender)
{
    /// <summary>Paginated, filterable, searchable list of farmer profiles.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<FarmerDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<FarmerDto>>> GetFarmers(
        [FromQuery] Guid? districtId,
        [FromQuery] CropType? primaryCrop,
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
        => Ok(await Sender.Send(new GetFarmersQuery(districtId, primaryCrop, search, isActive, pageNumber, pageSize), cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<FarmerDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<FarmerDto>> GetFarmerById(Guid id, CancellationToken cancellationToken)
        => Ok(await Sender.Send(new GetFarmerByIdQuery(id), cancellationToken));

    [HttpPost]
    [ProducesResponseType<FarmerDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<FarmerDto>> CreateFarmer(CreateFarmerCommand command, CancellationToken cancellationToken)
    {
        var farmer = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetFarmerById), new { id = farmer.Id }, farmer);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<FarmerDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<FarmerDto>> UpdateFarmer(Guid id, UpdateFarmerRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateFarmerCommand(
            id, request.FirstName, request.LastName, request.PhoneNumber,
            request.FarmSizeAcres, request.PrimaryCrop, request.GeoLatitude, request.GeoLongitude);

        return Ok(await Sender.Send(command, cancellationToken));
    }

    [HttpPost("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetActiveStatus(Guid id, SetFarmerActiveStatusRequest request, CancellationToken cancellationToken)
    {
        await Sender.Send(new SetFarmerActiveStatusCommand(id, request.IsActive), cancellationToken);
        return NoContent();
    }
}

public sealed record UpdateFarmerRequest(
    string FirstName,
    string LastName,
    string PhoneNumber,
    decimal FarmSizeAcres,
    CropType PrimaryCrop,
    double? GeoLatitude,
    double? GeoLongitude);

public sealed record SetFarmerActiveStatusRequest(bool IsActive);
