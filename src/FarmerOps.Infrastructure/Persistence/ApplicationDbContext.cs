using System.Reflection;
using System.Text.Json;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Domain.Common;
using FarmerOps.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IPublisher publisher)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<Farmer> Farmers => Set<Farmer>();
    public DbSet<FieldAgent> FieldAgents => Set<FieldAgent>();
    public DbSet<AgentAssignment> AgentAssignments => Set<AgentAssignment>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<InputOrder> InputOrders => Set<InputOrder>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Transactional outbox: every domain event raised by a tracked aggregate is serialized into
    /// an <see cref="OutboxMessage"/> row in the SAME transaction as the business change, so a
    /// webhook subscriber notification can never be lost even if the process crashes right after
    /// commit. In-process notification handlers (e.g. SMS alerts) are then published on a
    /// best-effort basis after the commit succeeds.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregatesWithEvents = ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count != 0)
            .ToList();

        var domainEvents = aggregatesWithEvents.SelectMany(e => e.DomainEvents).ToList();

        foreach (var domainEvent in domainEvents)
        {
            var outboxMessage = new OutboxMessage(
                domainEvent.GetType().Name,
                JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                domainEvent.OccurredOnUtc);

            OutboxMessages.Add(outboxMessage);
        }

        aggregatesWithEvents.ForEach(e => e.ClearDomainEvents());

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in domainEvents)
            await publisher.Publish(domainEvent, cancellationToken);

        return result;
    }
}
