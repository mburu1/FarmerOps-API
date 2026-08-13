using FarmerOps.Domain.Entities;
using FarmerOps.Domain.Enums;

namespace FarmerOps.Application.InputOrders.Dtos;

public sealed record InputOrderDto(
    Guid Id,
    Guid FarmerId,
    string? FarmerName,
    Guid? LoanId,
    InputType InputType,
    decimal Quantity,
    decimal UnitCost,
    decimal TotalCost,
    InputOrderStatus Status,
    DateTime OrderedAtUtc,
    DateTime? FulfilledAtUtc)
{
    public static InputOrderDto FromEntity(InputOrder order) => new(
        order.Id,
        order.FarmerId,
        order.Farmer?.FullName,
        order.LoanId,
        order.InputType,
        order.Quantity,
        order.UnitCost,
        order.TotalCost,
        order.Status,
        order.OrderedAtUtc,
        order.FulfilledAtUtc);
}
