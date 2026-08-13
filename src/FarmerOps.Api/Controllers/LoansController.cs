using FarmerOps.Application.Common.Models;
using FarmerOps.Application.Loans.Commands.ApplyForLoan;
using FarmerOps.Application.Loans.Commands.ApproveLoan;
using FarmerOps.Application.Loans.Commands.DisburseLoan;
using FarmerOps.Application.Loans.Commands.RecordRepayment;
using FarmerOps.Application.Loans.Commands.RejectLoan;
using FarmerOps.Application.Loans.Dtos;
using FarmerOps.Application.Loans.Queries.CheckLoanEligibility;
using FarmerOps.Application.Loans.Queries.GetLoanById;
using FarmerOps.Application.Loans.Queries.GetLoans;
using FarmerOps.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmerOps.Api.Controllers;

[Route("api/v1/[controller]")]
[Authorize]
public class LoansController(ISender sender) : BaseApiController(sender)
{
    [HttpGet]
    [ProducesResponseType<PagedResult<LoanDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<LoanDto>>> GetLoans(
        [FromQuery] Guid? farmerId,
        [FromQuery] LoanStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
        => Ok(await Sender.Send(new GetLoansQuery(farmerId, status, pageNumber, pageSize), cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<LoanDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<LoanDto>> GetLoanById(Guid id, CancellationToken cancellationToken)
        => Ok(await Sender.Send(new GetLoanByIdQuery(id), cancellationToken));

    /// <summary>Runs the loan eligibility rules engine without creating an application.</summary>
    [HttpGet("eligibility")]
    [ProducesResponseType<LoanEligibilityReportDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<LoanEligibilityReportDto>> CheckEligibility(
        [FromQuery] Guid farmerId, [FromQuery] decimal requestedAmount, CancellationToken cancellationToken)
        => Ok(await Sender.Send(new CheckLoanEligibilityQuery(farmerId, requestedAmount), cancellationToken));

    /// <summary>Applies for a loan. Eligibility is evaluated server-side; the loan starts life as Pending.</summary>
    [HttpPost("apply")]
    [ProducesResponseType<LoanDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<LoanDto>> Apply(ApplyForLoanCommand command, CancellationToken cancellationToken)
    {
        var loan = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetLoanById), new { id = loan.Id }, loan);
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin,OperationsManager")]
    [ProducesResponseType<LoanDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<LoanDto>> Approve(Guid id, CancellationToken cancellationToken)
        => Ok(await Sender.Send(new ApproveLoanCommand(id), cancellationToken));

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Admin,OperationsManager")]
    [ProducesResponseType<LoanDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<LoanDto>> Reject(Guid id, RejectLoanRequest request, CancellationToken cancellationToken)
        => Ok(await Sender.Send(new RejectLoanCommand(id, request.Reason), cancellationToken));

    [HttpPost("{id:guid}/disburse")]
    [Authorize(Roles = "Admin,OperationsManager")]
    [ProducesResponseType<LoanDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<LoanDto>> Disburse(Guid id, DisburseLoanRequest request, CancellationToken cancellationToken)
        => Ok(await Sender.Send(new DisburseLoanCommand(id, request.RepaymentTermDays), cancellationToken));

    [HttpPost("{id:guid}/repayments")]
    [ProducesResponseType<LoanDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<LoanDto>> RecordRepayment(Guid id, RecordRepaymentRequest request, CancellationToken cancellationToken)
        => Ok(await Sender.Send(new RecordRepaymentCommand(id, request.Amount), cancellationToken));
}

public sealed record RejectLoanRequest(string Reason);
public sealed record DisburseLoanRequest(int RepaymentTermDays);
public sealed record RecordRepaymentRequest(decimal Amount);
