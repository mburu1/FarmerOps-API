using FarmerOps.Domain.Entities;
using FarmerOps.Domain.Enums;

namespace FarmerOps.Application.Alerts.Dtos;

public sealed record AlertDto(
    Guid Id,
    Guid? FarmerId,
    Guid? AgentId,
    AlertType Type,
    string Message,
    AlertStatus Status,
    DateTime? SentAtUtc,
    string? FailureReason,
    int AttemptCount,
    DateTime CreatedAtUtc)
{
    public static AlertDto FromEntity(Alert alert) => new(
        alert.Id,
        alert.FarmerId,
        alert.AgentId,
        alert.Type,
        alert.Message,
        alert.Status,
        alert.SentAtUtc,
        alert.FailureReason,
        alert.AttemptCount,
        alert.CreatedAtUtc);
}
