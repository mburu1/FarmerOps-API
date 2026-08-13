using FarmerOps.Domain.Common;
using FarmerOps.Domain.Exceptions;

namespace FarmerOps.Domain.Entities;

public class FieldAgent : AggregateRoot
{
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public Guid? UserId { get; private set; }
    public bool IsActive { get; private set; } = true;

    /// <summary>Running performance score in [0, 100], derived from completed vs. missed visits.</summary>
    public double PerformanceScore { get; private set; } = 100;

    public string FullName => $"{FirstName} {LastName}";

    private readonly List<AgentAssignment> _assignments = [];
    public IReadOnlyCollection<AgentAssignment> Assignments => _assignments.AsReadOnly();

    private FieldAgent()
    {
    }

    public FieldAgent(string firstName, string lastName, string phoneNumber, string email, Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("Field agent first name is required.");
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Field agent email is required.");

        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        Email = email;
        UserId = userId;
    }

    public void RecordVisitOutcome(bool completed)
    {
        // Exponential moving average keeps the score responsive to recent performance
        // while smoothing out single-visit noise.
        const double smoothing = 0.15;
        var outcomeScore = completed ? 100 : 0;
        PerformanceScore = (smoothing * outcomeScore) + ((1 - smoothing) * PerformanceScore);
        Touch();
    }

    public void Deactivate() => IsActive = false;
}
