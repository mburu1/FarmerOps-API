using FarmerOps.Domain.Common;

namespace FarmerOps.Domain.Entities;

/// <summary>Assigns a field agent to a farmer cluster (a farmer's household/plot).</summary>
public class AgentAssignment : BaseEntity
{
    public Guid AgentId { get; private set; }
    public FieldAgent? Agent { get; private set; }
    public Guid FarmerId { get; private set; }
    public Farmer? Farmer { get; private set; }
    public DateTime AssignedAtUtc { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? UnassignedAtUtc { get; private set; }

    private AgentAssignment()
    {
    }

    public AgentAssignment(Guid agentId, Guid farmerId)
    {
        AgentId = agentId;
        FarmerId = farmerId;
        AssignedAtUtc = DateTime.UtcNow;
    }

    public void Unassign()
    {
        IsActive = false;
        UnassignedAtUtc = DateTime.UtcNow;
        Touch();
    }
}
