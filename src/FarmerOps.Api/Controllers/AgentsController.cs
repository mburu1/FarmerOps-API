using FarmerOps.Application.Agents.Commands.AssignAgentToFarmer;
using FarmerOps.Application.Agents.Commands.CreateFieldAgent;
using FarmerOps.Application.Agents.Commands.UnassignAgent;
using FarmerOps.Application.Agents.Dtos;
using FarmerOps.Application.Agents.Queries.GetAgentPerformance;
using FarmerOps.Application.Agents.Queries.GetFieldAgents;
using FarmerOps.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerOps.Api.Controllers;

[Route("api/v1/[controller]")]
[Authorize]
public class AgentsController(ISender sender) : BaseApiController(sender)
{
    [HttpGet]
    [ProducesResponseType<PagedResult<FieldAgentDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<FieldAgentDto>>> GetAgents(
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
        => Ok(await Sender.Send(new GetFieldAgentsQuery(isActive, pageNumber, pageSize), cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Admin,OperationsManager")]
    [ProducesResponseType<FieldAgentDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<FieldAgentDto>> CreateAgent(CreateFieldAgentCommand command, CancellationToken cancellationToken)
    {
        var agent = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAgents), new { }, agent);
    }

    /// <summary>Running performance score plus visit and assignment counters for one agent.</summary>
    [HttpGet("{id:guid}/performance")]
    [ProducesResponseType<AgentPerformanceDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AgentPerformanceDto>> GetPerformance(Guid id, CancellationToken cancellationToken)
        => Ok(await Sender.Send(new GetAgentPerformanceQuery(id), cancellationToken));

    [HttpPost("assignments")]
    [Authorize(Roles = "Admin,OperationsManager")]
    [ProducesResponseType<AgentAssignmentDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<AgentAssignmentDto>> AssignToFarmer(AssignAgentToFarmerCommand command, CancellationToken cancellationToken)
    {
        var assignment = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAgents), new { }, assignment);
    }

    [HttpDelete("assignments/{assignmentId:guid}")]
    [Authorize(Roles = "Admin,OperationsManager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Unassign(Guid assignmentId, CancellationToken cancellationToken)
    {
        await Sender.Send(new UnassignAgentCommand(assignmentId), cancellationToken);
        return NoContent();
    }
}
