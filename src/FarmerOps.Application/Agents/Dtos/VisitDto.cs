using FarmerOps.Domain.Entities;
using FarmerOps.Domain.Enums;

namespace FarmerOps.Application.Agents.Dtos;

public sealed record VisitDto(
    Guid Id,
    Guid AgentId,
    string? AgentName,
    Guid FarmerId,
    string? FarmerName,
    DateTime ScheduledAtUtc,
    DateTime? CompletedAtUtc,
    VisitStatus Status,
    string? Notes)
{
    public static VisitDto FromEntity(Visit visit) => new(
        visit.Id,
        visit.AgentId,
        visit.Agent?.FullName,
        visit.FarmerId,
        visit.Farmer?.FullName,
        visit.ScheduledAtUtc,
        visit.CompletedAtUtc,
        visit.Status,
        visit.Notes);
}
