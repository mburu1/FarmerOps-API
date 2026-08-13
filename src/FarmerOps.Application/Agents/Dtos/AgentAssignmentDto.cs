using FarmerOps.Domain.Entities;

namespace FarmerOps.Application.Agents.Dtos;

public sealed record AgentAssignmentDto(
    Guid Id,
    Guid AgentId,
    string? AgentName,
    Guid FarmerId,
    string? FarmerName,
    DateTime AssignedAtUtc,
    bool IsActive)
{
    public static AgentAssignmentDto FromEntity(AgentAssignment assignment) => new(
        assignment.Id,
        assignment.AgentId,
        assignment.Agent?.FullName,
        assignment.FarmerId,
        assignment.Farmer?.FullName,
        assignment.AssignedAtUtc,
        assignment.IsActive);
}
