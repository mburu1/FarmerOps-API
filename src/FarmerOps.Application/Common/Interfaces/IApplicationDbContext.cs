using FarmerOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Common.Interfaces;

/// <summary>
/// Application-facing view of the EF Core context. Handlers depend on this abstraction instead
/// of the concrete DbContext so the Application layer stays free of an Infrastructure reference.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Region> Regions { get; }
    DbSet<District> Districts { get; }
    DbSet<Farmer> Farmers { get; }
    DbSet<FieldAgent> FieldAgents { get; }
    DbSet<AgentAssignment> AgentAssignments { get; }
    DbSet<Visit> Visits { get; }
    DbSet<Loan> Loans { get; }
    DbSet<InputOrder> InputOrders { get; }
    DbSet<Alert> Alerts { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
