using FarmerOps.Application.Common.Models;
using FarmerOps.Application.InputOrders.Commands.CancelInputOrder;
using FarmerOps.Application.InputOrders.Commands.CreateInputOrder;
using FarmerOps.Application.InputOrders.Commands.FulfillInputOrder;
using FarmerOps.Application.InputOrders.Dtos;
using FarmerOps.Application.InputOrders.Queries.GetInputOrders;
using FarmerOps.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerOps.Api.Controllers;

[Route("api/v1/input-orders")]
[Authorize]
public class InputOrdersController(ISender sender) : BaseApiController(sender)
{
    [HttpGet]
    [ProducesResponseType<PagedResult<InputOrderDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<InputOrderDto>>> GetInputOrders(
        [FromQuery] Guid? farmerId,
        [FromQuery] InputOrderStatus? status,
        [FromQuery] InputType? inputType,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
        => Ok(await Sender.Send(new GetInputOrdersQuery(farmerId, status, inputType, pageNumber, pageSize), cancellationToken));

    [HttpPost]
    [ProducesResponseType<InputOrderDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<InputOrderDto>> CreateInputOrder(CreateInputOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetInputOrders), new { }, order);
    }

    [HttpPost("{id:guid}/fulfill")]
    [ProducesResponseType<InputOrderDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<InputOrderDto>> Fulfill(Guid id, CancellationToken cancellationToken)
        => Ok(await Sender.Send(new FulfillInputOrderCommand(id), cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType<InputOrderDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<InputOrderDto>> Cancel(Guid id, CancellationToken cancellationToken)
        => Ok(await Sender.Send(new CancelInputOrderCommand(id), cancellationToken));
}
