using FarmerOps.Application.Alerts.Dtos;
using FarmerOps.Application.Alerts.Queries.GetAlerts;
using FarmerOps.Application.Common.Models;
using FarmerOps.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerOps.Api.Controllers;

[Route("api/v1/[controller]")]
[Authorize]
public class AlertsController(ISender sender) : BaseApiController(sender)
{
    /// <summary>Delivery log for every SMS alert dispatched through the mock gateway (overdue repayments, loan status changes, ...).</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<AlertDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AlertDto>>> GetAlerts(
        [FromQuery] Guid? farmerId,
        [FromQuery] AlertStatus? status,
        [FromQuery] AlertType? type,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
        => Ok(await Sender.Send(new GetAlertsQuery(farmerId, status, type, pageNumber, pageSize), cancellationToken));
}
