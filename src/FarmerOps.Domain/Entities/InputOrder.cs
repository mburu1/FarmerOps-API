using FarmerOps.Domain.Common;
using FarmerOps.Domain.Enums;
using FarmerOps.Domain.Events;
using FarmerOps.Domain.Exceptions;

namespace FarmerOps.Domain.Entities;

public class InputOrder : AggregateRoot
{
    public Guid FarmerId { get; private set; }
    public Farmer? Farmer { get; private set; }
    public Guid? LoanId { get; private set; }
    public Loan? Loan { get; private set; }
    public InputType InputType { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal TotalCost => Quantity * UnitCost;
    public InputOrderStatus Status { get; private set; } = InputOrderStatus.Pending;
    public DateTime OrderedAtUtc { get; private set; }
    public DateTime? FulfilledAtUtc { get; private set; }

    private InputOrder()
    {
    }

    public InputOrder(Guid farmerId, InputType inputType, decimal quantity, decimal unitCost, Guid? loanId = null)
    {
        if (quantity <= 0)
            throw new DomainException("Input order quantity must be greater than zero.");
        if (unitCost <= 0)
            throw new DomainException("Input order unit cost must be greater than zero.");

        FarmerId = farmerId;
        InputType = inputType;
        Quantity = quantity;
        UnitCost = unitCost;
        LoanId = loanId;
        OrderedAtUtc = DateTime.UtcNow;
    }

    public void Fulfill()
    {
        if (Status != InputOrderStatus.Pending)
            throw new DomainException($"Cannot fulfill an input order in status '{Status}'.");

        Status = InputOrderStatus.Fulfilled;
        FulfilledAtUtc = DateTime.UtcNow;
        Touch();

        Raise(new InputOrderFulfilledEvent(Id, FarmerId, TotalCost));
    }

    public void Cancel()
    {
        if (Status != InputOrderStatus.Pending)
            throw new DomainException($"Cannot cancel an input order in status '{Status}'.");

        Status = InputOrderStatus.Cancelled;
        Touch();
    }
}
