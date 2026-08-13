using FarmerOps.Application.Agents.Commands.CompleteVisit;
using FarmerOps.Application.Agents.Commands.MarkVisitMissed;
using FarmerOps.Application.Agents.Commands.ScheduleVisit;
using FarmerOps.Application.Agents.Dtos;
using FarmerOps.Application.Agents.Queries.GetVisits;
using FarmerOps.Application.Common.Models;
using FarmerOps.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerOps.Api.Controllers;

[Route("api/v1/[controller]")]
[Authorize]
public class VisitsController(ISender sender) : BaseApiController(sender)
{
    [HttpGet]
    [ProducesResponseType<PagedResult<VisitDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<VisitDto>>> GetVisits(
        [FromQuery] Guid? agentId,
        [FromQuery] Guid? farmerId,
        [FromQuery] VisitStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
        => Ok(await Sender.Send(new GetVisitsQuery(agentId, farmerId, status, pageNumber, pageSize), cancellationToken));

    [HttpPost]
    [ProducesResponseType<VisitDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<VisitDto>> Schedule(ScheduleVisitCommand command, CancellationToken cancellationToken)
    {
        var visit = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetVisits), new { }, visit);
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType<VisitDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<VisitDto>> Complete(Guid id, CompleteVisitRequest request, CancellationToken cancellationToken)
        => Ok(await Sender.Send(new CompleteVisitCommand(id, request.Notes), cancellationToken));

    [HttpPost("{id:guid}/missed")]
    [ProducesResponseType<VisitDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<VisitDto>> MarkMissed(Guid id, CancellationToken cancellationToken)
        => Ok(await Sender.Send(new MarkVisitMissedCommand(id), cancellationToken));
}

public sealed record CompleteVisitRequest(string? Notes);
