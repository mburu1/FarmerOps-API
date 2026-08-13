using FarmerOps.Domain.Entities;

namespace FarmerOps.Application.Agents.Dtos;

public sealed record FieldAgentDto(
    Guid Id,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string Email,
    double PerformanceScore,
    bool IsActive)
{
    public static FieldAgentDto FromEntity(FieldAgent agent) => new(
        agent.Id,
        agent.FirstName,
        agent.LastName,
        agent.PhoneNumber,
        agent.Email,
        agent.PerformanceScore,
        agent.IsActive);
}
